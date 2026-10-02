using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // A tool handed to a counter for upgrading; it comes back on ReadyDay (a day number, GameDateTime.TotalDays).
    [Serializable]
    public sealed class PendingUpgrade
    {
        public string ToolItemId;
        public int Tier;
        public int ReadyDay;
    }

    public enum UpgradeCheck { Ok, NotOffered, NoGold, NoMaterials }

    // Looks upgrade definitions up (the game database's, or the built-in table when it has none).
    public sealed class UpgradeCatalog
    {
        static UpgradeCatalog _builtIn;
        readonly List<UpgradeDefinition> _all = new List<UpgradeDefinition>();

        public static UpgradeCatalog BuiltIn => _builtIn ?? (_builtIn = new UpgradeCatalog(UpgradeDefaults.CreateAll()));

        public UpgradeCatalog(IEnumerable<UpgradeDefinition> definitions)
        {
            _all.AddRange(definitions.Where(d => d != null));
        }

        public static UpgradeCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllUpgrades.ToList() : new List<UpgradeDefinition>();
            return all.Count == 0 ? BuiltIn : new UpgradeCatalog(all);
        }

        public IReadOnlyList<UpgradeDefinition> All => _all;
        public UpgradeDefinition Get(string id) => _all.FirstOrDefault(u => u.Id == id);
    }

    // The rules of paying for upgrades. Pure logic over the game state and backpack.
    public static class Upgrades
    {
        // What a counter offers right now: the next tier of each tool the player holds (and has not handed in), and
        // the next backpack and energy steps.
        public static List<UpgradeDefinition> Offered(UpgradeCatalog catalog, string shopId, GameState state, Inventory backpack)
        {
            var offers = new List<UpgradeDefinition>();
            foreach (var def in catalog.All.Where(d => d.ShopId == shopId).OrderBy(d => d.Id, StringComparer.Ordinal))
            {
                switch (def.Kind)
                {
                    case UpgradeKind.Tool:
                        if (!backpack.Has(def.ToolItemId)) break;
                        if (state.PendingUpgrades.Any(p => p.ToolItemId == def.ToolItemId)) break;
                        state.ToolTiers.TryGetValue(def.ToolItemId, out var tier);
                        if (def.Tier == tier + 1) offers.Add(def);
                        break;
                    default:
                        if (state.UpgradesDone.Contains(def.Id)) break;
                        var earlierDone = catalog.All.Where(d => d.Kind == def.Kind && d.Tier < def.Tier).All(d => state.UpgradesDone.Contains(d.Id));
                        if (earlierDone) offers.Add(def);
                        break;
                }
            }
            return offers;
        }

        public static UpgradeCheck Check(UpgradeCatalog catalog, UpgradeDefinition def, GameState state, Inventory backpack)
        {
            if (!Offered(catalog, def.ShopId, state, backpack).Contains(def)) return UpgradeCheck.NotOffered;
            if (state.Gold < def.GoldCost) return UpgradeCheck.NoGold;
            if (!string.IsNullOrEmpty(def.MaterialItemId) && !backpack.Has(def.MaterialItemId, def.MaterialCount)) return UpgradeCheck.NoMaterials;
            return UpgradeCheck.Ok;
        }

        // Pays for the upgrade and applies it (a tool is taken in to be returned later). Returns what stopped it, if anything.
        public static UpgradeCheck Buy(UpgradeCatalog catalog, UpgradeDefinition def, GameState state, Inventory backpack, GameDateTime today)
        {
            var check = Check(catalog, def, state, backpack);
            if (check != UpgradeCheck.Ok) return check;

            state.Gold -= def.GoldCost;
            if (!string.IsNullOrEmpty(def.MaterialItemId)) backpack.Remove(def.MaterialItemId, def.MaterialCount);

            switch (def.Kind)
            {
                case UpgradeKind.Tool:
                    backpack.Remove(def.ToolItemId, 1);
                    state.PendingUpgrades.Add(new PendingUpgrade { ToolItemId = def.ToolItemId, Tier = def.Tier, ReadyDay = today.TotalDays + def.Days });
                    break;
                case UpgradeKind.Backpack:
                    backpack.Resize(Math.Max(backpack.Capacity, def.Value));
                    state.UpgradesDone.Add(def.Id);
                    break;
                case UpgradeKind.Energy:
                    state.MaxEnergy += def.Value;
                    state.Energy += def.Value;
                    state.UpgradesDone.Add(def.Id);
                    break;
            }
            return UpgradeCheck.Ok;
        }

        public static List<PendingUpgrade> Ready(GameState state, GameDateTime today) =>
            state.PendingUpgrades.Where(p => p.ReadyDay <= today.TotalDays).ToList();

        // Hands back every finished tool the backpack has room for, at its new tier. Returns the ones collected.
        public static List<PendingUpgrade> Collect(GameState state, Inventory backpack, GameDateTime today)
        {
            var collected = new List<PendingUpgrade>();
            foreach (var p in Ready(state, today))
            {
                if (!backpack.CanAdd(p.ToolItemId, 1)) continue;
                backpack.Add(p.ToolItemId, 1);
                state.ToolTiers[p.ToolItemId] = Math.Max(ToolModel.ClampTier(p.Tier), state.ToolTiers.TryGetValue(p.ToolItemId, out var t) ? t : 0);
                state.PendingUpgrades.Remove(p);
                collected.Add(p);
            }
            return collected;
        }

        public static int DaysLeft(PendingUpgrade p, GameDateTime today) => Math.Max(0, p.ReadyDay - today.TotalDays);
    }
}
