using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // An item the crafting systems add (machines, ore, artisan goods, dishes, fertilizer). The content tools write the
    // assets and placeholder icons from these rows, so the table is the one place to tune them.
    public readonly struct ExtraItemRow
    {
        public readonly string Id;
        public readonly ItemCategory Category;
        public readonly int Sell, Buy, Energy;
        public readonly Color Color;
        public readonly string PlaceableId;
        public readonly string[] SoldIn;
        public readonly string SaleCondition;

        public ExtraItemRow(string id, ItemCategory category, int sell, Color color, int buy = 0, int energy = 0, string placeableId = null, string[] soldIn = null,
            string saleCondition = null)
        {
            Id = id; Category = category; Sell = sell; Buy = buy; Energy = energy; Color = color; PlaceableId = placeableId; SoldIn = soldIn; SaleCondition = saleCondition;
        }

        // The icon sprite name: item_<id with dots as underscores>.
        public string IconName => "item_" + Id.Replace('.', '_');
    }

    // The base game's placeable objects, recipes and the items they use. Built-in defaults for tests and bare projects.
    public static class CraftingDefaults
    {
        public const string Chest = "chest", Furnace = "furnace", Keg = "keg", Jar = "jar", Scarecrow = "scarecrow";
        public const string Sprinkler1 = "sprinkler1", Sprinkler2 = "sprinkler2", Sprinkler3 = "sprinkler3";

        public static readonly string[] PlaceableIds = { Chest, Furnace, Keg, Jar, Scarecrow, Sprinkler1, Sprinkler2, Sprinkler3 };

        static Color C(float r, float g, float b) => new Color(r, g, b);

        public static PlaceableDefinition[] CreatePlaceables() => new[]
        {
            PlaceableDefinition.Create(Chest, PlaceableKind.Chest, capacity: 36),
            PlaceableDefinition.Create(Furnace, PlaceableKind.Machine, Stations.Furnace),
            PlaceableDefinition.Create(Keg, PlaceableKind.Machine, Stations.Keg),
            PlaceableDefinition.Create(Jar, PlaceableKind.Machine, Stations.Jar),
            PlaceableDefinition.Create(Scarecrow, PlaceableKind.Scarecrow, farmingOnly: true, range: 8),
            PlaceableDefinition.Create(Sprinkler1, PlaceableKind.Sprinkler, farmingOnly: true, range: 1),
            PlaceableDefinition.Create(Sprinkler2, PlaceableKind.Sprinkler, farmingOnly: true, range: 2),
            PlaceableDefinition.Create(Sprinkler3, PlaceableKind.Sprinkler, farmingOnly: true, range: 3),
        };

        // Dishes: id, ingredients, energy restored, sell price, skill that teaches it (null = known from the start).
        static readonly (string id, (string item, int count)[] needs, int energy, int sell, string skill, int level)[] Dishes =
        {
            ("salad",          new[] { ("crop.kale", 1), ("crop.parsnip", 1) },        40, 90,  null, 0),
            ("mashed_potato",  new[] { ("crop.potato", 2) },                           55, 130, null, 0),
            ("roasted_roots",  new[] { ("crop.parsnip", 1), ("crop.potato", 1) },      60, 140, null, 0),
            ("bean_stew",      new[] { ("crop.greenbean", 3) },                        70, 130, "farming", 2),
            ("forager_plate",  new[] { ("*forage", 3) },                               60, 120, "foraging", 2),
            ("berry_tart",     new[] { ("crop.strawberry", 3) },                       85, 220, "farming", 3),
            ("gratin",         new[] { ("crop.cauliflower", 2) },                      95, 300, "farming", 4),
            ("pumpkin_pie",    new[] { ("crop.pumpkin", 1), ("crop.wheat", 2) },       120, 400, "farming", 5),
            ("corn_chowder",   new[] { ("crop.corn", 2), ("crop.potato", 1) },         100, 260, "farming", 6),
        };

        public static IEnumerable<string> DishIds => Dishes.Select(d => "food." + d.id);

        public static ExtraItemRow[] CreateItems()
        {
            var rows = new List<ExtraItemRow>
            {
                // The traveling merchant sells ore and coal (when the stall is up; the ore rotates).
                new ExtraItemRow(ItemIds.Coal, ItemCategory.Resource, 15, C(0.15f, 0.15f, 0.18f), buy: 60, soldIn: new[] { "merchant" }, saleCondition: "merchant:today"),
                new ExtraItemRow(ItemIds.CopperOre, ItemCategory.Resource, 10, C(0.75f, 0.45f, 0.30f), buy: 40, soldIn: new[] { "merchant" }, saleCondition: "merchant:today && rotate:0"),
                new ExtraItemRow(ItemIds.IronOre, ItemCategory.Resource, 25, C(0.55f, 0.58f, 0.65f), buy: 90, soldIn: new[] { "merchant" }, saleCondition: "merchant:today && rotate:1"),
                new ExtraItemRow(ItemIds.GoldOre, ItemCategory.Resource, 50, C(0.90f, 0.75f, 0.25f), buy: 200, soldIn: new[] { "merchant" }, saleCondition: "merchant:today && rotate:2"),
                new ExtraItemRow("artisan.juice", ItemCategory.Artisan, 150, C(0.85f, 0.55f, 0.20f)),
                new ExtraItemRow("artisan.wine", ItemCategory.Artisan, 300, C(0.55f, 0.15f, 0.35f)),
                new ExtraItemRow("artisan.pickles", ItemCategory.Artisan, 100, C(0.45f, 0.65f, 0.30f)),
                new ExtraItemRow("artisan.jam", ItemCategory.Artisan, 160, C(0.70f, 0.20f, 0.30f)),
                new ExtraItemRow(ItemIds.FertilizerQuality, ItemCategory.Fertilizer, 20, C(0.45f, 0.35f, 0.20f), buy: 100, soldIn: new[] { "general" }),
                new ExtraItemRow(ItemIds.FertilizerSpeed, ItemCategory.Fertilizer, 30, C(0.35f, 0.55f, 0.25f), buy: 120, soldIn: new[] { "general" }),
            };
            foreach (var id in PlaceableIds)
                rows.Add(new ExtraItemRow(ItemIds.Machine(id), ItemCategory.Machine, 0, MachineColor(id), placeableId: id));
            foreach (var d in Dishes)
                rows.Add(new ExtraItemRow("food." + d.id, ItemCategory.Food, d.sell, C(0.85f, 0.60f, 0.35f), energy: d.energy));
            return rows.ToArray();
        }

        static Color MachineColor(string id)
        {
            switch (id)
            {
                case Chest: return C(0.60f, 0.40f, 0.22f);
                case Furnace: return C(0.50f, 0.50f, 0.54f);
                case Keg: return C(0.55f, 0.35f, 0.20f);
                case Jar: return C(0.65f, 0.75f, 0.60f);
                case Scarecrow: return C(0.80f, 0.70f, 0.30f);
                default: return C(0.35f, 0.60f, 0.80f);
            }
        }

        static RecipeIngredient Need(string item, int count) => new RecipeIngredient { ItemId = item, Count = count };
        static RecipeIngredient Any(string label, IEnumerable<string> items, int count) =>
            new RecipeIngredient { ItemId = label, AnyOf = items.ToArray(), Count = count };

        static IEnumerable<string> Crops(CropKind kind) => CropDefaults.IdsOfKind(kind).Select(ItemIds.Crop);

        public static RecipeDefinition[] CreateRecipes()
        {
            var r = new List<RecipeDefinition>();
            RecipeDefinition Hand(string id, string output, int count, int skillLevel, string skill, params RecipeIngredient[] needs) =>
                RecipeDefinition.Create(id, Stations.Hand, output, count, needs, 0, skill, skillLevel);

            // Crafted by hand from the menu.
            r.Add(Hand("chest", ItemIds.Machine(Chest), 1, 0, null, Need(ItemIds.Wood, 25)));
            r.Add(Hand("furnace", ItemIds.Machine(Furnace), 1, 0, null, Need(ItemIds.Stone, 30), Need(ItemIds.Wood, 10)));
            r.Add(Hand("keg", ItemIds.Machine(Keg), 1, 0, null, Need(ItemIds.Wood, 30), Need(ItemIds.Stone, 10), Need(ItemIds.Fiber, 10)));
            r.Add(Hand("jar", ItemIds.Machine(Jar), 1, 0, null, Need(ItemIds.Wood, 20), Need(ItemIds.Stone, 10)));
            r.Add(Hand("scarecrow", ItemIds.Machine(Scarecrow), 1, 0, null, Need(ItemIds.Wood, 30), Need(ItemIds.Fiber, 20)));
            r.Add(Hand("sprinkler1", ItemIds.Machine(Sprinkler1), 1, 2, "farming", Need(ItemIds.CopperBar, 1), Need(ItemIds.Stone, 10)));
            r.Add(Hand("sprinkler2", ItemIds.Machine(Sprinkler2), 1, 5, "farming", Need(ItemIds.IronBar, 1), Need(ItemIds.CopperBar, 1)));
            r.Add(Hand("sprinkler3", ItemIds.Machine(Sprinkler3), 1, 8, "farming", Need(ItemIds.GoldBar, 1), Need(ItemIds.IronBar, 1)));
            r.Add(Hand("fertilizer_quality", ItemIds.FertilizerQuality, 2, 1, "farming", Need(ItemIds.Fiber, 8), Need(ItemIds.Stone, 2)));
            r.Add(Hand("fertilizer_speed", ItemIds.FertilizerSpeed, 2, 3, "farming", Need(ItemIds.Fiber, 12), Need(ItemIds.Wood, 4)));

            // Furnace: ore and coal become bars (hours of game time).
            RecipeDefinition Smelt(string id, string ore, string bar, int minutes) =>
                RecipeDefinition.Create(id, Stations.Furnace, bar, 1, new[] { Need(ore, 5), Need(ItemIds.Coal, 1) }, minutes);
            r.Add(Smelt("smelt_copper", ItemIds.CopperOre, ItemIds.CopperBar, 30));
            r.Add(Smelt("smelt_iron", ItemIds.IronOre, ItemIds.IronBar, 120));
            r.Add(Smelt("smelt_gold", ItemIds.GoldOre, ItemIds.GoldBar, 300));

            // Keg and jar: a crop in, a preserved good out, after days.
            r.Add(RecipeDefinition.Create("keg_juice", Stations.Keg, "artisan.juice", 1, new[] { Any("recipe.any_vegetable", Crops(CropKind.Vegetable), 1) }, 2 * 1440));
            r.Add(RecipeDefinition.Create("keg_wine", Stations.Keg, "artisan.wine", 1, new[] { Any("recipe.any_fruit", Crops(CropKind.Fruit), 1) }, 4 * 1440));
            r.Add(RecipeDefinition.Create("jar_pickles", Stations.Jar, "artisan.pickles", 1, new[] { Any("recipe.any_vegetable", Crops(CropKind.Vegetable), 1) }, 1440));
            r.Add(RecipeDefinition.Create("jar_jam", Stations.Jar, "artisan.jam", 1, new[] { Any("recipe.any_fruit", Crops(CropKind.Fruit), 1) }, 1440));

            // The kitchen.
            var forage = ForageDefaults.Rows.Select(row => row.ItemId);
            foreach (var d in Dishes)
            {
                var needs = d.needs.Select(n => n.item == "*forage" ? Any("recipe.any_forage", forage, n.count) : Need(n.item, n.count)).ToArray();
                r.Add(RecipeDefinition.Create("cook_" + d.id, Stations.Kitchen, "food." + d.id, 1, needs, 0, d.skill, d.level));
            }
            return r.ToArray();
        }
    }
}
