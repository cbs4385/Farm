using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public readonly struct DropRow
    {
        public readonly string ItemId; public readonly float Chance; public readonly int Min, Max;
        public DropRow(string itemId, float chance, int min = 1, int max = 1) { ItemId = itemId; Chance = chance; Min = min; Max = max; }
    }

    // One kind of monster. All balance numbers are here, in data: health, touch damage, speed (cells per second), how far
    // away it notices the player, XP, the floors it appears on and how common it is there, and what it drops.
    public readonly struct EnemyRow
    {
        public readonly string Id, Name;
        public readonly int Hp, Damage, Xp, MinFloor, MaxFloor;
        public readonly float Speed, Aggro, Weight;
        public readonly bool Flying, Boss;
        public readonly DropRow[] Drops;
        public readonly Color Color;

        public EnemyRow(string id, string name, int hp, int damage, float speed, float aggro, int xp, int minFloor, int maxFloor, float weight,
            Color color, DropRow[] drops, bool flying = false, bool boss = false)
        {
            Id = id; Name = name; Hp = hp; Damage = damage; Speed = speed; Aggro = aggro; Xp = xp; MinFloor = minFloor; MaxFloor = maxFloor;
            Weight = weight; Color = color; Drops = drops; Flying = flying; Boss = boss;
        }
    }

    public static class EnemyDefaults
    {
        public const string Boss = "cavern_warden";
        static Color C(float r, float g, float b) => new Color(r, g, b);

        public static readonly EnemyRow[] Rows =
        {
            new EnemyRow("slime", "Slime", 20, 5, 1.6f, 5f, 8, 1, 15, 10, C(0.4f, 0.8f, 0.4f), new[] { new DropRow("resource.slime", 0.7f, 1, 2) }),
            new EnemyRow("cave_bat", "Cave Bat", 12, 4, 3.2f, 7f, 10, 3, 40, 6, C(0.45f, 0.35f, 0.55f), new[] { new DropRow("resource.bat_wing", 0.5f) }, flying: true),
            new EnemyRow("rock_crab", "Rock Crab", 40, 8, 1.2f, 4f, 16, 8, 30, 5, C(0.6f, 0.5f, 0.45f), new[] { new DropRow(ItemIds.Stone, 0.8f, 2, 4), new DropRow(ItemIds.Coal, 0.25f) }),
            new EnemyRow("bone_walker", "Bone Walker", 60, 12, 1.8f, 6f, 24, 15, 40, 5, C(0.9f, 0.9f, 0.8f), new[] { new DropRow("resource.bone", 0.6f), new DropRow(ItemIds.Coal, 0.3f) }),
            new EnemyRow("ember_imp", "Ember Imp", 80, 16, 2.4f, 7f, 36, 25, 40, 4, C(0.95f, 0.45f, 0.2f), new[] { new DropRow("resource.ember", 0.5f), new DropRow(ItemIds.GoldOre, 0.3f) }),
            new EnemyRow(Boss, "Cavern Warden", 600, 25, 1.6f, 12f, 500, 40, 40, 0, C(0.5f, 0.2f, 0.6f),
                new[] { new DropRow(ItemIds.GoldBar, 1f, 3, 3), new DropRow("resource.warden_core", 1f) }, boss: true),
        };

        public static EnemyRow Row(string id) => Rows.First(r => r.Id == id);

        // Drop items (sold to the shipping bin; some feed crafting).
        public static ExtraItemRow[] CreateItems() => new[]
        {
            new ExtraItemRow("resource.slime", ItemCategory.Resource, 6, C(0.4f, 0.8f, 0.4f)),
            new ExtraItemRow("resource.bat_wing", ItemCategory.Resource, 14, C(0.45f, 0.35f, 0.55f)),
            new ExtraItemRow("resource.bone", ItemCategory.Resource, 25, C(0.9f, 0.9f, 0.8f)),
            new ExtraItemRow("resource.ember", ItemCategory.Resource, 60, C(0.95f, 0.45f, 0.2f)),
            new ExtraItemRow("resource.warden_core", ItemCategory.Resource, 1000, C(0.6f, 0.3f, 0.8f)),
        };
    }
}
