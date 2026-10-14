using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public enum CraftResult { Ok, NotKnown, MissingIngredients, NoRoom, WrongStation }
    public enum LoadResult { Loaded, Busy, NoRecipe, MissingIngredients }

    // The rules of crafting, cooking and machines. Pure logic over a backpack and the saved state.
    public static class CraftingRules
    {
        // ---- what a recipe needs --------------------------------------------------------------------------------------

        public static int Available(RecipeIngredient ingredient, Inventory backpack)
        {
            if (ingredient.AnyOf != null && ingredient.AnyOf.Length > 0) return ingredient.AnyOf.Sum(backpack.Count);
            return backpack.Count(ingredient.ItemId);
        }

        public static bool HasIngredients(RecipeDefinition recipe, Inventory backpack) =>
            recipe.Ingredients.All(i => Available(i, backpack) >= i.Count);

        // Removes the ingredients (from the accepted items in list order). Call only after HasIngredients.
        public static void TakeIngredients(RecipeDefinition recipe, Inventory backpack)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                var left = ingredient.Count;
                var ids = ingredient.AnyOf != null && ingredient.AnyOf.Length > 0 ? ingredient.AnyOf : new[] { ingredient.ItemId };
                foreach (var id in ids)
                {
                    if (left <= 0) break;
                    left -= backpack.Remove(id, left);
                }
            }
        }

        // ---- with what is near the player (backpack first, then `near` in order) -----------------------------------------------

        static List<IIngredientSource> Sources(Inventory backpack, IReadOnlyList<IIngredientSource> near)
        {
            var all = new List<IIngredientSource> { new InventorySource(backpack) };
            if (near != null) all.AddRange(near);
            return all;
        }

        public static int Available(RecipeIngredient ingredient, Inventory backpack, IReadOnlyList<IIngredientSource> near)
        {
            var total = 0;
            foreach (var source in Sources(backpack, near)) total += Held(ingredient, source);
            return total;
        }

        static int Held(RecipeIngredient ingredient, IIngredientSource source) =>
            ingredient.AnyOf != null && ingredient.AnyOf.Length > 0 ? ingredient.AnyOf.Sum(source.Count) : source.Count(ingredient.ItemId);

        public static bool HasIngredients(RecipeDefinition recipe, Inventory backpack, IReadOnlyList<IIngredientSource> near) =>
            recipe.Ingredients.All(i => Available(i, backpack, near) >= i.Count);

        // Who gives what: the backpack's share first, then each place in turn. Nothing is removed. (pure)
        public static List<(int source, string itemId, int count)> Plan(RecipeDefinition recipe, IReadOnlyList<IIngredientSource> sources)
        {
            var plan = new List<(int, string, int)>();
            var used = new Dictionary<(int, string), int>();
            foreach (var ingredient in recipe.Ingredients)
            {
                var left = ingredient.Count;
                var ids = ingredient.AnyOf != null && ingredient.AnyOf.Length > 0 ? ingredient.AnyOf : new[] { ingredient.ItemId };
                for (var s = 0; s < sources.Count && left > 0; s++)
                    foreach (var id in ids)
                    {
                        if (left <= 0) break;
                        used.TryGetValue((s, id), out var already);
                        var take = Math.Min(left, sources[s].Count(id) - already);
                        if (take <= 0) continue;
                        used[(s, id)] = already + take;
                        plan.Add((s, id, take));
                        left -= take;
                    }
            }
            return plan;
        }

        public static CraftResult TryCraft(RecipeDefinition recipe, string station, GameState state, Inventory backpack, Func<string, int> skillLevel,
            IReadOnlyList<IIngredientSource> near)
        {
            if (near == null || near.Count == 0) return TryCraft(recipe, station, state, backpack, skillLevel);
            if (recipe.Station != station) return CraftResult.WrongStation;
            if (!Knows(recipe, state, skillLevel)) return CraftResult.NotKnown;
            if (!HasIngredients(recipe, backpack, near)) return CraftResult.MissingIngredients;

            var sources = Sources(backpack, near);
            var plan = Plan(recipe, sources);
            var probe = Inventory.FromData(backpack.ToData(), id => 999);
            foreach (var (source, id, count) in plan)
                if (source == 0) probe.Remove(id, count);
            if (!probe.CanAdd(recipe.OutputItemId, recipe.OutputCount)) return CraftResult.NoRoom;

            foreach (var (source, id, count) in plan) sources[source].Remove(id, count);
            backpack.Add(recipe.OutputItemId, recipe.OutputCount);
            return CraftResult.Ok;
        }

        // ---- knowing recipes ------------------------------------------------------------------------------------------

        // A recipe is known from the start (no skill requirement), once learned (a letter, a villager), or once the player's
        // skill reaches its level.
        public static bool Knows(RecipeDefinition recipe, GameState state, Func<string, int> skillLevel)
        {
            if (!string.IsNullOrEmpty(recipe.Condition) && state != null) { /* offered conditions are checked by callers with a world */ }
            if (recipe.SkillLevel <= 0 || string.IsNullOrEmpty(recipe.SkillId)) return true;
            return state.Recipes.Contains(recipe.Id) || skillLevel(recipe.SkillId) >= recipe.SkillLevel;
        }

        // Recipes a skill has just unlocked by going from `fromLevel` to `toLevel`.
        public static List<RecipeDefinition> NewlyUnlocked(RecipeCatalog recipes, string skill, int fromLevel, int toLevel) =>
            recipes.All.Where(r => r.SkillId == skill && r.SkillLevel > fromLevel && r.SkillLevel <= toLevel).ToList();

        // ---- crafting by hand or at a kitchen -------------------------------------------------------------------------

        public static CraftResult TryCraft(RecipeDefinition recipe, string station, GameState state, Inventory backpack, Func<string, int> skillLevel)
        {
            if (recipe.Station != station) return CraftResult.WrongStation;
            if (!Knows(recipe, state, skillLevel)) return CraftResult.NotKnown;
            if (!HasIngredients(recipe, backpack)) return CraftResult.MissingIngredients;

            // Check the room with the ingredients taken out, on a copy, so a full pack never loses materials.
            var probe = Inventory.FromData(backpack.ToData(), id => 999);
            TakeIngredients(recipe, probe);
            if (!probe.CanAdd(recipe.OutputItemId, recipe.OutputCount)) return CraftResult.NoRoom;

            TakeIngredients(recipe, backpack);
            backpack.Add(recipe.OutputItemId, recipe.OutputCount);
            return CraftResult.Ok;
        }

        // ---- machines -------------------------------------------------------------------------------------------------

        public static bool IsBusy(PlacedObject machine) => !string.IsNullOrEmpty(machine.RecipeId);
        public static bool IsReady(PlacedObject machine, int nowMinute) => IsBusy(machine) && nowMinute >= machine.ReadyAt;
        public static int MinutesLeft(PlacedObject machine, int nowMinute) => Math.Max(0, machine.ReadyAt - nowMinute);

        // The recipe a machine would start with the held item, or null.
        public static RecipeDefinition RecipeFor(PlaceableDefinition machine, string heldItemId, RecipeCatalog recipes, Inventory backpack) =>
            recipes.ForStation(machine.Station).FirstOrDefault(r => r.Ingredients.Any(i => i.Accepts(heldItemId)) && HasIngredients(r, backpack));

        // Loads a machine with the ingredients for the recipe that matches the held item.
        public static LoadResult TryLoad(PlacedObject machine, PlaceableDefinition def, string heldItemId, RecipeCatalog recipes,
            Inventory backpack, int nowMinute, out RecipeDefinition started)
        {
            started = null;
            if (IsBusy(machine)) return LoadResult.Busy;
            var candidates = recipes.ForStation(def.Station).Where(r => r.Ingredients.Any(i => i.Accepts(heldItemId))).ToList();
            if (candidates.Count == 0) return LoadResult.NoRecipe;
            var recipe = candidates.FirstOrDefault(r => HasIngredients(r, backpack));
            if (recipe == null) return LoadResult.MissingIngredients;

            TakeIngredients(recipe, backpack);
            machine.RecipeId = recipe.Id;
            machine.OutputItemId = recipe.OutputItemId;
            machine.OutputCount = recipe.OutputCount;
            machine.ReadyAt = nowMinute + Math.Max(recipe.Minutes, 10);
            started = recipe;
            return LoadResult.Loaded;
        }

        // "5 Copper Ore and 1 Coal": what a recipe takes, in words.
        public static string NeedsText(RecipeDefinition recipe, GameDatabase db)
        {
            var parts = recipe.Ingredients.Select(i =>
            {
                var name = i.AnyOf != null && i.AnyOf.Length > 0 ? L.Get(i.ItemId)
                    : db != null && db.TryGetItem(i.ItemId, out var item) ? L.Get(item.NameKey) : i.ItemId;
                return $"{i.Count} {name}";
            }).ToList();
            return parts.Count <= 1 ? string.Join("", parts) : string.Join(", ", parts.Take(parts.Count - 1)) + " " + L.Get("machine.and") + " " + parts[parts.Count - 1];
        }

        // What a machine can make and what each thing needs, for a player who does not know ("Copper Bar: 5 Copper Ore and 1 Coal. Iron Bar: ...").
        public static string WhatItTakes(string station, RecipeCatalog recipes, GameDatabase db, int most = 3)
        {
            var list = recipes.ForStation(station).ToList();
            var shown = list.Take(most).Select(r =>
                $"{(db != null && db.TryGetItem(r.OutputItemId, out var made) ? L.Get(made.NameKey) : r.OutputItemId)}: {NeedsText(r, db)}").ToList();
            return string.Join(". ", shown) + (list.Count > most ? L.Get("machine.and_more") : "");
        }

        // Takes the finished output. Returns false when it is not ready or the backpack has no room (nothing changes).
        public static bool TryCollect(PlacedObject machine, Inventory backpack, int nowMinute)
        {
            if (!IsReady(machine, nowMinute)) return false;
            if (!backpack.CanAdd(machine.OutputItemId, machine.OutputCount)) return false;
            backpack.Add(machine.OutputItemId, machine.OutputCount);
            machine.RecipeId = null;
            machine.OutputItemId = null;
            machine.OutputCount = 0;
            machine.ReadyAt = 0;
            return true;
        }
    }
}
