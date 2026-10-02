using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Data
{
    // One row of the crop table: everything the content tools need to write a crop, its seed and its harvest item, and its
    // placeholder art (one sprite per growth stage, tinted with Color).
    public readonly struct CropRow
    {
        public readonly string Id;
        public readonly int[] Days;               // watered days spent in each growth stage; mature after the last
        public readonly SeasonMask Seasons;       // trees: the seasons they bear fruit
        public readonly int Regrow;
        public readonly int SeedPrice, SellPrice;
        public readonly CropKind Kind;
        public readonly Color Color;
        public readonly bool IsTree;

        public CropRow(string id, int[] days, SeasonMask seasons, int regrow, int seedPrice, int sellPrice, CropKind kind, Color color, bool isTree = false)
        {
            Id = id; Days = days; Seasons = seasons; Regrow = regrow; SeedPrice = seedPrice; SellPrice = sellPrice;
            Kind = kind; Color = color; IsTree = isTree;
        }

        public int Stages => Days.Length;
    }

    // The 24 crops and 4 fruit trees of the base game (GDD 3.3), spring to fall; winter belongs to foraging, mining and the
    // greenhouse. The single source of truth for crop data, art and shop stock.
    public static class CropDefaults
    {
        const SeasonMask Sp = SeasonMask.Spring, Su = SeasonMask.Summer, Fa = SeasonMask.Fall;
        static Color C(float r, float g, float b) => new Color(r, g, b);

        public static readonly CropRow[] Rows =
        {
            // Spring
            new CropRow("parsnip",     new[] { 1, 1, 1, 1 },    Sp, 0, 20, 35,   CropKind.Vegetable, C(0.93f, 0.85f, 0.65f)),
            new CropRow("potato",      new[] { 1, 1, 1, 2, 1 }, Sp, 0, 50, 80,   CropKind.Vegetable, C(0.72f, 0.55f, 0.32f)),
            new CropRow("cauliflower", new[] { 2, 4, 4, 2 },    Sp, 0, 80, 175,  CropKind.Vegetable, C(0.94f, 0.94f, 0.88f)),
            new CropRow("greenbean",   new[] { 1, 1, 1, 3, 4 }, Sp, 3, 60, 40,   CropKind.Vegetable, C(0.35f, 0.75f, 0.30f)),
            new CropRow("strawberry",  new[] { 1, 1, 2, 2, 2 }, Sp, 4, 100, 120, CropKind.Fruit,     C(0.90f, 0.22f, 0.28f)),
            new CropRow("kale",        new[] { 1, 2, 2, 1 },    Sp, 0, 70, 110,  CropKind.Vegetable, C(0.20f, 0.50f, 0.30f)),
            // Summer
            new CropRow("tomato",      new[] { 2, 2, 2, 2, 3 }, Su, 4, 50, 60,   CropKind.Fruit,     C(0.90f, 0.20f, 0.15f)),
            new CropRow("melon",       new[] { 3, 3, 3, 3, 3 }, Su, 0, 80, 250,  CropKind.Fruit,     C(0.60f, 0.85f, 0.40f)),
            new CropRow("blueberry",   new[] { 1, 3, 3, 4, 2 }, Su, 4, 80, 50,   CropKind.Fruit,     C(0.25f, 0.30f, 0.70f)),
            new CropRow("pepper",      new[] { 1, 1, 1, 1, 2 }, Su, 3, 40, 40,   CropKind.Vegetable, C(0.85f, 0.30f, 0.10f)),
            new CropRow("radish",      new[] { 2, 1, 2, 1 },    Su, 0, 40, 90,   CropKind.Vegetable, C(0.90f, 0.30f, 0.40f)),
            new CropRow("hops",        new[] { 1, 1, 2, 3, 4 }, Su, 1, 60, 35,   CropKind.Vegetable, C(0.55f, 0.80f, 0.35f)),
            new CropRow("cucumber",    new[] { 2, 2, 2, 2, 2 }, Su, 3, 60, 50,   CropKind.Vegetable, C(0.30f, 0.65f, 0.35f)),
            // Fall
            new CropRow("pumpkin",     new[] { 1, 2, 3, 4, 3 }, Fa, 0, 100, 320, CropKind.Vegetable, C(0.95f, 0.55f, 0.10f)),
            new CropRow("eggplant",    new[] { 1, 2, 2, 2, 2 }, Fa, 5, 20, 60,   CropKind.Vegetable, C(0.45f, 0.20f, 0.50f)),
            new CropRow("cranberry",   new[] { 1, 2, 1, 1, 2 }, Fa, 5, 240, 130, CropKind.Fruit,     C(0.80f, 0.10f, 0.20f)),
            new CropRow("yam",         new[] { 1, 3, 3, 3 },    Fa, 0, 60, 160,  CropKind.Vegetable, C(0.80f, 0.50f, 0.25f)),
            new CropRow("beet",        new[] { 1, 1, 2, 2 },    Fa, 0, 20, 100,  CropKind.Vegetable, C(0.60f, 0.10f, 0.30f)),
            new CropRow("artichoke",   new[] { 2, 2, 1, 2, 1 }, Fa, 0, 30, 160,  CropKind.Vegetable, C(0.50f, 0.65f, 0.40f)),
            // Two seasons
            new CropRow("corn",        new[] { 2, 3, 3, 3, 3 }, Su | Fa, 4, 150, 80, CropKind.Vegetable, C(0.95f, 0.85f, 0.30f)),
            new CropRow("wheat",       new[] { 1, 1, 1, 1 },    Su | Fa, 0, 10, 25,  CropKind.Grain,     C(0.90f, 0.80f, 0.45f)),
            new CropRow("sunflower",   new[] { 2, 3, 2, 3, 3 }, Su | Fa, 0, 150, 90, CropKind.Flower,    C(0.95f, 0.80f, 0.10f)),
            new CropRow("onion",       new[] { 1, 2, 2, 2 },    Sp | Fa, 0, 30, 50,  CropKind.Vegetable, C(0.80f, 0.65f, 0.45f)),
            new CropRow("spinach",     new[] { 1, 1, 2, 1 },    Sp | Fa, 0, 20, 40,  CropKind.Vegetable, C(0.15f, 0.55f, 0.20f)),
            // Fruit trees: 28 days to grow, then a fruit a day in their season. Seasons = when they bear fruit.
            new CropRow("tree_cherry",    new[] { 7, 7, 7, 7 }, Sp, 0, 350, 80,  CropKind.Fruit, C(0.85f, 0.15f, 0.30f), isTree: true),
            new CropRow("tree_peach",     new[] { 7, 7, 7, 7 }, Su, 0, 400, 90,  CropKind.Fruit, C(0.98f, 0.70f, 0.50f), isTree: true),
            new CropRow("tree_apple",     new[] { 7, 7, 7, 7 }, Fa, 0, 400, 90,  CropKind.Fruit, C(0.80f, 0.15f, 0.15f), isTree: true),
            new CropRow("tree_persimmon", new[] { 7, 7, 7, 7 }, SeasonMask.Winter, 0, 450, 120, CropKind.Fruit, C(0.95f, 0.50f, 0.10f), isTree: true),
        };

        public static CropRow Row(string id) => Rows.First(r => r.Id == id);

        public static IEnumerable<string> IdsOfKind(CropKind kind) => Rows.Where(r => !r.IsTree && r.Kind == kind).Select(r => r.Id);

        // The condition that lets a shop offer a seed only while it can be planted ("season:spring || season:fall").
        public static string SeasonCondition(SeasonMask seasons)
        {
            var parts = new List<string>();
            foreach (Season s in System.Enum.GetValues(typeof(Season)))
                if (seasons.Includes(s)) parts.Add("season:" + s.ToString().ToLowerInvariant());
            return string.Join(" || ", parts);
        }
    }
}
