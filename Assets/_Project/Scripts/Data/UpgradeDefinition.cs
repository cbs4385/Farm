using System.Collections.Generic;
using UnityEngine;

namespace Farm.Data
{
    public enum UpgradeKind { Tool = 0, Backpack = 1, Energy = 2 }

    // Something the player can pay for at a counter: a better tool (handed over and returned after a few days), a
    // bigger backpack, a larger energy reserve. Upgrades of one kind are bought in order of Tier (a copper tool before
    // an iron one). Ids are saved and must never change once shipped.
    [CreateAssetMenu(menuName = "Farm/Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] UpgradeKind _kind;
        [SerializeField] string _shopId;          // business whose counter offers it (see BusinessHoursRegistry)
        [SerializeField] string _toolItemId;      // Tool: the tool being upgraded
        [SerializeField] int _tier = 1;           // Tool: the tier it becomes (1 copper .. 3 gold); others: the step
        [SerializeField] int _goldCost;
        [SerializeField] string _materialItemId;  // optional: consumed along with the gold
        [SerializeField] int _materialCount;
        [SerializeField] int _days;               // Tool: days until it is ready to collect
        [SerializeField] int _value;              // Backpack: new slot count; Energy: maximum energy gained

        public string Id => _id;
        public UpgradeKind Kind => _kind;
        public string ShopId => _shopId;
        public string ToolItemId => _toolItemId;
        public int Tier => _tier;
        public int GoldCost => _goldCost;
        public string MaterialItemId => _materialItemId;
        public int MaterialCount => _materialCount;
        public int Days => _days;
        public int Value => _value;
        public string NameKey => "upgrade." + _id + ".name";   // for the kinds that are not tools

        public static UpgradeDefinition Create(string id, UpgradeKind kind, string shopId, string toolItemId, int tier,
            int goldCost, string materialItemId, int materialCount, int days, int value)
        {
            var u = CreateInstance<UpgradeDefinition>();
            u._id = id;
            u.name = id;
            u._kind = kind;
            u._shopId = shopId;
            u._toolItemId = toolItemId;
            u._tier = tier;
            u._goldCost = goldCost;
            u._materialItemId = materialItemId;
            u._materialCount = materialCount;
            u._days = days;
            u._value = value;
            return u;
        }
    }

    // The base game's upgrade table. The content tool writes these out as assets so prices can be tuned.
    public static class UpgradeDefaults
    {
        const string CopperBar = ItemIds.CopperBar;
        const string IronBar = ItemIds.IronBar;
        const string GoldBar = ItemIds.GoldBar;

        static readonly string[] Tools = { ItemIds.Hoe, ItemIds.WateringCan, ItemIds.Axe, ItemIds.Pickaxe, ItemIds.Scythe };

        public static UpgradeDefinition[] CreateAll()
        {
            var all = new List<UpgradeDefinition>();
            foreach (var tool in Tools)
            {
                all.Add(UpgradeDefinition.Create($"{tool}.copper", UpgradeKind.Tool, "blacksmith", tool, 1, 1500, CopperBar, 5, 2, 0));
                all.Add(UpgradeDefinition.Create($"{tool}.iron", UpgradeKind.Tool, "blacksmith", tool, 2, 4000, IronBar, 5, 2, 0));
                all.Add(UpgradeDefinition.Create($"{tool}.gold", UpgradeKind.Tool, "blacksmith", tool, 3, 9000, GoldBar, 5, 2, 0));
            }
            all.Add(UpgradeDefinition.Create("backpack.24", UpgradeKind.Backpack, "general", null, 1, 2000, null, 0, 0, 24));
            all.Add(UpgradeDefinition.Create("backpack.36", UpgradeKind.Backpack, "general", null, 2, 10000, null, 0, 0, 36));
            all.Add(UpgradeDefinition.Create("energy.1", UpgradeKind.Energy, "clinic", null, 1, 1500, null, 0, 0, 20));
            all.Add(UpgradeDefinition.Create("energy.2", UpgradeKind.Energy, "clinic", null, 2, 4000, null, 0, 0, 20));
            all.Add(UpgradeDefinition.Create("energy.3", UpgradeKind.Energy, "clinic", null, 3, 9000, null, 0, 0, 20));
            return all.ToArray();
        }
    }
}
