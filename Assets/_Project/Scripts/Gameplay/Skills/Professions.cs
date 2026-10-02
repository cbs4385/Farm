using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    public enum ProfessionEffect
    {
        SellCrops, SellAnimalProducts, SellArtisan, SellForage, SellFish, SellOre,      // price multipliers (value = +fraction)
        LuckFarming, LuckForaging, LuckFishing,                                         // added to the luck of those rolls
        ToolEnergy,                                                                     // value = -fraction of energy per swing (axe, pickaxe)
        Damage,                                                                         // +fraction of sword damage
        MaxHealth,                                                                      // + health, applied once when chosen
        BiteSpeed,                                                                      // + fraction faster bites
    }

    public readonly struct ProfessionRow
    {
        public readonly string Id, Skill;
        public readonly int Level;
        public readonly ProfessionEffect Effect;
        public readonly float Value;
        public ProfessionRow(string id, string skill, int level, ProfessionEffect effect, float value) { Id = id; Skill = skill; Level = level; Effect = effect; Value = value; }
    }

    // Professions (T-057): at skill levels 5 and 10 the player chooses one of two. Each does one small, clear thing.
    public static class Professions
    {
        public static readonly ProfessionRow[] Rows =
        {
            new ProfessionRow("tiller", SkillIds.Farming, 5, ProfessionEffect.SellCrops, 0.10f),
            new ProfessionRow("rancher", SkillIds.Farming, 5, ProfessionEffect.SellAnimalProducts, 0.15f),
            new ProfessionRow("artisan", SkillIds.Farming, 10, ProfessionEffect.SellArtisan, 0.25f),
            new ProfessionRow("agriculturist", SkillIds.Farming, 10, ProfessionEffect.LuckFarming, 0.25f),
            new ProfessionRow("gatherer", SkillIds.Foraging, 5, ProfessionEffect.SellForage, 0.20f),
            new ProfessionRow("tracker", SkillIds.Foraging, 5, ProfessionEffect.LuckForaging, 0.30f),
            new ProfessionRow("lumberjack", SkillIds.Foraging, 10, ProfessionEffect.ToolEnergy, -0.25f),
            new ProfessionRow("botanist", SkillIds.Foraging, 10, ProfessionEffect.LuckForaging, 0.40f),
            new ProfessionRow("miner", SkillIds.Mining, 5, ProfessionEffect.SellOre, 0.20f),
            new ProfessionRow("geologist", SkillIds.Mining, 5, ProfessionEffect.ToolEnergy, -0.20f),
            new ProfessionRow("smelter", SkillIds.Mining, 10, ProfessionEffect.SellOre, 0.30f),
            new ProfessionRow("prospector", SkillIds.Mining, 10, ProfessionEffect.ToolEnergy, -0.20f),
            new ProfessionRow("fisher", SkillIds.Fishing, 5, ProfessionEffect.SellFish, 0.25f),
            new ProfessionRow("trapper", SkillIds.Fishing, 5, ProfessionEffect.BiteSpeed, 0.30f),
            new ProfessionRow("angler", SkillIds.Fishing, 10, ProfessionEffect.SellFish, 0.25f),
            new ProfessionRow("mariner", SkillIds.Fishing, 10, ProfessionEffect.LuckFishing, 0.30f),
            new ProfessionRow("fighter", SkillIds.Combat, 5, ProfessionEffect.Damage, 0.15f),
            new ProfessionRow("scout", SkillIds.Combat, 5, ProfessionEffect.MaxHealth, 30f),
            new ProfessionRow("champion", SkillIds.Combat, 10, ProfessionEffect.Damage, 0.25f),
            new ProfessionRow("defender", SkillIds.Combat, 10, ProfessionEffect.MaxHealth, 50f),
        };

        public static ProfessionRow? Find(string id) => Rows.Any(r => r.Id == id) ? Rows.First(r => r.Id == id) : (ProfessionRow?)null;

        public static bool Has(GameState state, string id) => state.Professions.Contains(id);

        // The choices open to the player in a skill: the unchosen tiers they have reached, two options each.
        public static List<ProfessionRow> Offers(GameState state, string skill, int level) =>
            Rows.Where(r => r.Skill == skill && r.Level <= level
                            && !Rows.Any(o => o.Skill == skill && o.Level == r.Level && state.Professions.Contains(o.Id))).ToList();

        // Chooses one (when it is on offer). Returns false otherwise.
        public static bool Choose(GameSession s, string id)
        {
            var row = Find(id);
            if (row == null) return false;
            if (!Offers(s.State, row.Value.Skill, s.GetSkillLevel(row.Value.Skill)).Any(o => o.Id == id)) return false;
            s.State.Professions.Add(id);
            if (row.Value.Effect == ProfessionEffect.MaxHealth)
            {
                s.State.MaxHealth += (int)row.Value.Value;
                s.State.Health += (int)row.Value.Value;
            }
            s.NotifyChanged();
            return true;
        }

        static float Sum(GameState state, ProfessionEffect effect) =>
            Rows.Where(r => r.Effect == effect && state.Professions.Contains(r.Id)).Sum(r => r.Value);

        public static float SellMultiplier(GameState state, ItemDefinition item)
        {
            ProfessionEffect? effect = null;
            switch (item.Category)
            {
                case ItemCategory.Crop: effect = ProfessionEffect.SellCrops; break;
                case ItemCategory.Forage: effect = ProfessionEffect.SellForage; break;
                case ItemCategory.Fish: effect = ProfessionEffect.SellFish; break;
                case ItemCategory.Artisan: effect = item.Id.StartsWith("product.", StringComparison.Ordinal) ? ProfessionEffect.SellAnimalProducts : ProfessionEffect.SellArtisan; break;
                case ItemCategory.Resource: if (item.Id.EndsWith("ore", StringComparison.Ordinal) || item.Id.EndsWith("bar", StringComparison.Ordinal)) effect = ProfessionEffect.SellOre; break;
            }
            return effect.HasValue ? 1f + Sum(state, effect.Value) : 1f;
        }

        public static float LuckBonus(GameState state, ProfessionEffect domain) => Sum(state, domain);
        public static float EnergyMultiplier(GameState state) => Math.Max(0.4f, 1f + Sum(state, ProfessionEffect.ToolEnergy));
        public static float DamageMultiplier(GameState state) => 1f + Sum(state, ProfessionEffect.Damage);
        public static float BiteSpeed(GameState state) => 1f + Sum(state, ProfessionEffect.BiteSpeed);
    }
}
