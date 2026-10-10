using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The numbers side of the session: energy, gold, items, skills, recipes, upgrades and the shipping bin.
    public sealed partial class GameSession
    {
        public UpgradeCatalog UpgradeTable => _upgradeCatalog ?? (_upgradeCatalog = UpgradeCatalog.From(_db));

        // Buys an upgrade at a counter (gold, materials, effect). Explains a refusal with a toast.
        public bool BuyUpgrade(UpgradeDefinition def)
        {
            var result = Upgrades.Buy(UpgradeTable, def, State, Backpack, Clock.Now);
            switch (result)
            {
                case UpgradeCheck.NoGold: Toast(L.Get("toast.not_enough_gold")); break;
                case UpgradeCheck.NoMaterials: Toast(L.Get("toast.upgrade_materials")); break;
                case UpgradeCheck.NotOffered: break;
                default:
                    if (def.Kind == UpgradeKind.Unlock && !string.IsNullOrEmpty(def.FlagId)) SetFlag(def.FlagId);
                    _bus.Publish(new StatsChanged());
                    Toast(L.Get(def.Kind == UpgradeKind.Tool ? "toast.upgrade_started" : "toast.upgrade_done"));
                    break;
            }
            return result == UpgradeCheck.Ok;
        }

        // Takes back every finished tool the backpack has room for.
        public int CollectUpgrades()
        {
            var collected = Upgrades.Collect(State, Backpack, Clock.Now);
            foreach (var p in collected) Toast(L.Get("toast.upgrade_collected", ToolTitle(p.ToolItemId, p.Tier)));
            if (collected.Count == 0 && Upgrades.Ready(State, Clock.Now).Count > 0) Toast(L.Get("toast.inventory_full"));
            if (collected.Count > 0) _bus.Publish(new StatsChanged());
            return collected.Count;
        }

        // "Copper Axe" for a tool item at a tier ("Axe" for basic).
        public string ToolTitle(string toolItemId, int tier)
        {
            var name = _db.TryGetItem(toolItemId, out var item) ? L.Get(item.NameKey) : toolItemId;
            return tier <= 0 ? name : L.Get("upgrade.tool", L.Get(ToolModel.TierKey(tier)), name);
        }

        // ---- player stats --------------------------------------------------------------------------------------

        public bool TrySpendEnergy(int amount)
        {
            if (amount > 0 && Settings != null && Settings.RelaxedEnergy) amount = Mathf.Max(1, amount / 2);
            if (State.Energy < amount) return false;
            State.Energy -= amount;
            _bus.Publish(new StatsChanged());
            return true;
        }

        public void SetEnergy(int value)
        {
            State.Energy = Mathf.Clamp(value, 0, State.MaxEnergy);
            _bus.Publish(new StatsChanged());
        }

        // Tells the HUD to redraw (used by tools that change the clock or other state directly).
        public void NotifyChanged() => _bus.Publish(new StatsChanged());

        public void RestoreEnergy(int amount)
        {
            State.Energy = Mathf.Min(State.MaxEnergy, State.Energy + amount);
            _bus.Publish(new StatsChanged());
        }

        public void AddGold(int amount)
        {
            State.Gold += amount;
            _bus.Publish(new StatsChanged());
        }


        // Teaches a recipe (a letter, a villager). Returns false when it is already known or does not exist.
        public bool LearnRecipe(string recipeId)
        {
            var recipe = Recipes.Get(recipeId);
            if (recipe == null || State.Recipes.Contains(recipeId)) return false;
            State.Recipes.Add(recipeId);
            if (_db.TryGetItem(recipe.OutputItemId, out var made)) Toast(L.Get("toast.recipe_learned", L.Get(made.NameKey)));
            return true;
        }

        // Crafts or cooks a recipe at a station; counts it for quests.
        public CraftResult Craft(RecipeDefinition recipe, string station)
        {
            var result = CraftingRules.TryCraft(recipe, station, State, Backpack, GetSkillLevel);
            if (result == CraftResult.Ok) AddVar(QuestLog.Stats.Crafted, 1);
            return result;
        }

        public bool KnowsRecipe(RecipeDefinition recipe) => CraftingRules.Knows(recipe, State, GetSkillLevel);

        // Adds or removes gold (a story effect); never goes below zero.
        public void ChangeGold(int delta)
        {
            State.Gold = System.Math.Max(0, State.Gold + delta);
            _bus.Publish(new StatsChanged());
        }

        // Puts items in the backpack (a reward, a gift). What does not fit waits in the mailbox (Parcels), with a toast saying so.
        public void GiveItem(string itemId, int count = 1, int quality = 0)
        {
            if (!InGame || count <= 0) return;
            if (!_db.TryGetItem(itemId, out _)) { Log.Error($"give: unknown item '{itemId}'"); return; }
            var left = Backpack.Add(itemId, count, quality);
            if (left <= 0) return;
            Parcels.Send(this, itemId, left);
            Toast(L.Get("toast.parcel_waiting"));
        }

        public bool TrySpendGold(int amount)
        {
            if (State.Gold < amount) return false;
            State.Gold -= amount;
            _bus.Publish(new StatsChanged());
            return true;
        }

        public int GetSkillXp(string skill) => InGame && State.SkillXp.TryGetValue(skill, out var xp) ? xp : 0;
        public int GetSkillLevel(string skill) => SkillModel.LevelForXp(GetSkillXp(skill));

        // Adds XP; announces each level gained (event and a toast).
        public void AddSkillXp(string skill, int xp)
        {
            if (!InGame || xp <= 0 || string.IsNullOrEmpty(skill)) return;
            var before = SkillModel.LevelForXp(GetSkillXp(skill));
            State.SkillXp[skill] = GetSkillXp(skill) + xp;
            var after = SkillModel.LevelForXp(State.SkillXp[skill]);
            for (var level = before + 1; level <= after; level++)
            {
                _bus.Publish(new SkillLevelUp(skill, level));
                Toast(L.Get("toast.skill_level", L.Get("skill." + skill), level));
                if (level == 5 || level == 10) Toast(L.Get("toast.profession_ready", L.Get("skill." + skill)));
            }
            // Levels unlock recipes: say what can be made now.
            foreach (var recipe in CraftingRules.NewlyUnlocked(Recipes, skill, before, after))
                if (_db.TryGetItem(recipe.OutputItemId, out var made)) Toast(L.Get("toast.recipe_unlocked", L.Get(made.NameKey)));
        }

        // The upgrade tier of a tool item the player owns (0 = basic).
        public int ToolTier(string itemId) => InGame && itemId != null && State.ToolTiers.TryGetValue(itemId, out var t) ? t : 0;

        // Publishes an event on the game's bus (for systems that only hold the session).
        public void Publish<T>(T evt) => _bus.Publish(evt);

        public void Toast(string message) => _bus.Publish(new ToastRequested(message));

        // ---- shipping ------------------------------------------------------------------------------------------

        // Moves the whole stack in a backpack slot into the shipping bin. Returns false if it cannot be sold.
        public bool ShipSlot(int slot) => ShipSlot(slot, int.MaxValue);

        // The same, for some of the stack (the shipping window ships a lot of several stacks at once).
        public bool ShipSlot(int slot, int count)
        {
            var stack = Backpack.Get(slot);
            if (stack == null || count <= 0 || !_db.TryGetItem(stack.ItemId, out var item) || item.IsTool || item.SellPrice <= 0) return false;

            var removed = Backpack.RemoveFromSlot(slot, System.Math.Min(count, stack.Count));
            AddVar(QuestLog.Stats.Shipped, removed.Count);
            foreach (var existing in State.ShippingBin)
            {
                if (existing.ItemId == removed.ItemId && existing.Quality == removed.Quality && existing.Mark == removed.Mark)
                {
                    existing.Count += removed.Count;
                    _bus.Publish(new StatsChanged());
                    return true;
                }
            }
            State.ShippingBin.Add(removed);
            _bus.Publish(new StatsChanged());
            return true;
        }
    }
}
