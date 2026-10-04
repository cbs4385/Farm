using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-012/T-016: single source of truth for the M1 item and crop content. Writes ScriptableObject assets and the
    // GameDatabase registry. Idempotent; existing assets are updated in place so GUIDs stay stable.
    public static class ContentGenerator
    {
        const string Root = "Assets/_Project";
        const string ItemDir = Root + "/Data/Items";
        const string CropDir = Root + "/Data/Crops";
        const string WeatherDir = Root + "/Data/Weather";
        const string NodeDir = Root + "/Data/Nodes";
        const string UpgradeDir = Root + "/Data/Upgrades";
        const string SpawnDir = Root + "/Data/Spawns";
        const string NpcDir = Root + "/Data/Npcs";
        const string RecipeDir = Root + "/Data/Recipes";
        const string PlaceableDir = Root + "/Data/Placeables";
        const string DbPath = Root + "/Resources/GameDatabase.asset";
        const string ArtDir = Root + "/Art/Placeholders";

        [MenuItem("Farm/Setup/Generate Content")]
        public static void GenerateAll()
        {
            PlaceholderArtGenerator.Generate();   // sprites must exist and be imported first
            Directory.CreateDirectory(ItemDir);
            Directory.CreateDirectory(CropDir);
            Directory.CreateDirectory(WeatherDir);
            Directory.CreateDirectory(NodeDir);
            Directory.CreateDirectory(UpgradeDir);
            Directory.CreateDirectory(SpawnDir);
            Directory.CreateDirectory(NpcDir);
            Directory.CreateDirectory(RecipeDir);
            Directory.CreateDirectory(PlaceableDir);
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));

            var items = new List<ItemDefinition>();
            var crops = new List<CropDefinition>();

            Tool(items, ItemIds.Hoe, ToolType.Hoe, "item_tool_hoe");
            Tool(items, ItemIds.WateringCan, ToolType.WateringCan, "item_tool_wateringcan");
            Tool(items, ItemIds.Axe, ToolType.Axe, "item_tool_axe");
            Tool(items, ItemIds.Pickaxe, ToolType.Pickaxe, "item_tool_pickaxe");
            Tool(items, ItemIds.Scythe, ToolType.Scythe, "item_tool_scythe");

            // Story props (cat, umbrella, notes ...) are shown by scenes through an item icon; they are never sold or given.
            foreach (var prop in new[] { "cat", "umbrella", "note", "lantern", "trophy", "pumpkin", "scarecrow", "shell" })
                Save(items, ItemDefinition.Create("prop." + prop, ItemCategory.Misc, maxStack: 1, icon: Sprite("item_prop_" + prop)));

            Save(items, ItemDefinition.Create(ItemIds.Wood, ItemCategory.Resource, sellPrice: 2, icon: Sprite("item_resource_wood")));
            Save(items, ItemDefinition.Create(ItemIds.Stone, ItemCategory.Resource, sellPrice: 2, icon: Sprite("item_resource_stone")));
            Save(items, ItemDefinition.Create(ItemIds.CopperBar, ItemCategory.Resource, sellPrice: 60, icon: Sprite("item_resource_copperbar")));
            Save(items, ItemDefinition.Create(ItemIds.IronBar, ItemCategory.Resource, sellPrice: 120, icon: Sprite("item_resource_ironbar")));
            Save(items, ItemDefinition.Create(ItemIds.GoldBar, ItemCategory.Resource, sellPrice: 250, icon: Sprite("item_resource_goldbar")));
            Save(items, ItemDefinition.Create(ItemIds.Fiber, ItemCategory.Resource, sellPrice: 1, icon: Sprite("item_resource_fiber")));

            foreach (var row in ForageDefaults.Rows)
                Save(items, ItemDefinition.Create(row.ItemId, ItemCategory.Forage, sellPrice: row.Price, icon: Sprite("item_forage_" + row.Id)));

            foreach (var row in CropDefaults.Rows)
            {
                var sprites = Enumerable.Range(0, row.Stages + 1).Select(i => Sprite($"crop_{row.Id}_{i}")).ToArray();
                var crop = CropDefinition.Create(row.Id, row.Days, row.Seasons, row.Regrow, sprites).WithKind(row.Kind);
                if (row.IsTree) crop.AsTree(row.Seasons);
                SaveCrop(crops, crop);
                // Seeds are sold only while they can be planted (saplings any time).
                Save(items, ItemDefinition.Create(ItemIds.Seed(row.Id), ItemCategory.Seed, buyPrice: row.SeedPrice,
                    cropId: row.Id, icon: Sprite($"item_seed_{row.Id}"), soldIn: new[] { "general" },
                    saleCondition: row.IsTree ? null : CropDefaults.SeasonCondition(row.Seasons)));
                Save(items, ItemDefinition.Create(ItemIds.Crop(row.Id), ItemCategory.Crop, sellPrice: row.SellPrice,
                    icon: Sprite($"item_crop_{row.Id}")));
            }

            // Combat: weapons, monster drops.
            foreach (var w in CombatModel.Weapons)
                Save(items, ItemDefinition.Create(w.ItemId, ItemCategory.Tool, 1, buyPrice: w.ItemId == ItemIds.Sword ? 0 : w.Price,
                    toolType: ToolType.Sword, icon: Sprite("item_" + w.ItemId.Replace('.', '_')),
                    soldIn: w.ItemId == ItemIds.Sword ? null : new[] { "blacksmith" }));
            foreach (var row in EnemyDefaults.CreateItems())
                Save(items, ItemDefinition.Create(row.Id, row.Category, sellPrice: row.Sell, icon: Sprite(row.IconName)));

            // Animals and what they make.
            foreach (var row in AnimalDefaults.CreateItems())
                Save(items, ItemDefinition.Create(row.Id, row.Category, sellPrice: row.Sell, buyPrice: row.Buy, icon: Sprite(row.IconName),
                    soldIn: row.SoldIn, placeableId: row.PlaceableId));

            // Fishing: the rod, bait and every fish.
            Save(items, ItemDefinition.Create(FishDefaults.Rod, ItemCategory.Tool, 1, buyPrice: 100, toolType: ToolType.Rod,
                icon: Sprite("item_tool_rod"), soldIn: new[] { "fish" }));
            foreach (var row in FishDefaults.CreateItems())
                Save(items, ItemDefinition.Create(row.Id, row.Category, sellPrice: row.Sell, buyPrice: row.Buy, icon: Sprite(row.IconName), soldIn: row.SoldIn));

            // Crafting: ore, machines, artisan goods, dishes, fertilizer.
            foreach (var row in CraftingDefaults.CreateItems())
                Save(items, ItemDefinition.Create(row.Id, row.Category, sellPrice: row.Sell, buyPrice: row.Buy, energyRestore: row.Energy,
                    icon: Sprite(row.IconName), soldIn: row.SoldIn, saleCondition: row.SaleCondition, placeableId: row.PlaceableId));

            var weather = WeatherDefaults.CreateAll().Select(SaveWeather).ToList();
            var nodes = NodeDefaults.CreateAll().Concat(MineGenerator.CreateNodes()).Select(SaveNode).ToList();
            var upgrades = UpgradeDefaults.CreateAll().Select(SaveUpgrade).ToList();
            var spawns = ForageDefaults.CreateTables().Select(SaveSpawnTable).ToList();
            var npcs = NpcDefaults.CreateAll().Select(SaveNpc).ToList();
            var placeables = CraftingDefaults.CreatePlaceables().Select(SavePlaceable).ToList();
            var recipes = CraftingDefaults.CreateRecipes().Select(SaveRecipe).ToList();

            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DbPath);
            if (db == null)
            {
                db = GameDatabase.Create(items, crops, weather, nodes, upgrades, spawns);
                AssetDatabase.CreateAsset(db, DbPath);
            }
            else
            {
                db.SetContents(items, crops, weather, nodes, upgrades, spawns);
                EditorUtility.SetDirty(db);
            }

            db.SetNpcs(npcs);
            db.SetPlaceables(placeables);
            db.SetRecipes(recipes);
            EditorUtility.SetDirty(db);

            MythosGenerator.Generate();

            AssetDatabase.SaveAssets();
            Debug.Log($"[ContentGenerator] {items.Count} items, {crops.Count} crops, {weather.Count} weathers, {nodes.Count} resource nodes, {upgrades.Count} upgrades, {spawns.Count} spawn tables, {npcs.Count} npcs, {recipes.Count} recipes, {placeables.Count} placeables.");
        }

        public static void GenerateAllAndExit()
        {
            GenerateAll();
            EditorApplication.Exit(0);
        }

        static void Tool(List<ItemDefinition> items, string id, ToolType type, string sprite) =>
            Save(items, ItemDefinition.Create(id, ItemCategory.Tool, maxStack: 1, toolType: type, icon: Sprite(sprite)));

        internal static Sprite Sprite(string name)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");
            if (s == null) Debug.LogWarning($"[ContentGenerator] Missing placeholder sprite '{name}'.");
            return s;
        }

        // Writes the definition to disk (updating in place when the asset exists) and appends the persistent asset.
        static void Save(List<ItemDefinition> list, ItemDefinition fresh) => list.Add(Persist(fresh, $"{ItemDir}/{fresh.Id}.asset"));
        static void SaveCrop(List<CropDefinition> list, CropDefinition fresh) => list.Add(Persist(fresh, $"{CropDir}/{fresh.Id}.asset"));

        // Weather is tuned by hand in the inspector, so an existing asset is kept as it is.
        static WeatherDefinition SaveWeather(WeatherDefinition fresh)
        {
            var path = $"{WeatherDir}/{fresh.Id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WeatherDefinition>(path);
            if (existing != null)
            {
                Object.DestroyImmediate(fresh);
                return existing;
            }
            AssetDatabase.CreateAsset(fresh, path);
            return fresh;
        }

        // Villagers: schedules, tastes and birthdays are written in NpcDefaults and applied on every run; the sprites are
        // looked up by name.
        static NpcDefinition SaveNpc(NpcDefinition fresh)
        {
            fresh.SetSprites(Sprite($"npc_{fresh.Id}_idle_down"), Sprite($"npc_{fresh.Id}_idle_up"),
                Sprite($"npc_{fresh.Id}_idle_left"), Sprite($"npc_{fresh.Id}_idle_right"), Sprite($"ui_portrait_{fresh.Id}"));
            // Expression portraits (T-130): ui_portrait_<id>_<expression>, for the villagers that have them. Neutral is the base portrait.
            var expressions = new Sprite[NpcDefinition.ExpressionNames.Length];
            var any = false;
            for (var i = 0; i < expressions.Length; i++)
            {
                expressions[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ui_portrait_{fresh.Id}_{NpcDefinition.ExpressionNames[i]}.png");
                any |= expressions[i] != null;
            }
            fresh.SetExpressions(any ? expressions : null);
            // Pose sprites (T-131): npc_<id>_pose_<name>, for the villagers that have them.
            var poses = new Sprite[NpcDefinition.PoseNames.Length];
            var anyPose = false;
            for (var i = 0; i < poses.Length; i++)
            {
                poses[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/npc_{fresh.Id}_pose_{NpcDefinition.PoseNames[i]}.png");
                anyPose |= poses[i] != null;
            }
            fresh.SetPoses(anyPose ? poses : null);
            return Persist(fresh, $"{NpcDir}/{fresh.Id}.asset");
        }

        // Recipes and placeables are written in CraftingDefaults and applied on every run.
        static RecipeDefinition SaveRecipe(RecipeDefinition fresh) => Persist(fresh, $"{RecipeDir}/{fresh.Id}.asset");

        static PlaceableDefinition SavePlaceable(PlaceableDefinition fresh)
        {
            fresh.SetSprite(Sprite("obj_" + fresh.Id));
            return Persist(fresh, $"{PlaceableDir}/{fresh.Id}.asset");
        }

        // Spawn tables are tuned in the inspector: an existing asset is kept.
        static SpawnTableDefinition SaveSpawnTable(SpawnTableDefinition fresh)
        {
            var path = $"{SpawnDir}/{fresh.Id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<SpawnTableDefinition>(path);
            if (existing != null)
            {
                Object.DestroyImmediate(fresh);
                return existing;
            }
            AssetDatabase.CreateAsset(fresh, path);
            return fresh;
        }

        // Upgrade prices are tuned in the inspector: an existing asset is kept.
        static UpgradeDefinition SaveUpgrade(UpgradeDefinition fresh)
        {
            var path = $"{UpgradeDir}/{fresh.Id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            if (existing != null)
            {
                Object.DestroyImmediate(fresh);
                return existing;
            }
            AssetDatabase.CreateAsset(fresh, path);
            return fresh;
        }

        // Resource nodes are tuned in the inspector too: keep an existing asset, but make sure it has its sprite.
        static ResourceNodeDefinition SaveNode(ResourceNodeDefinition fresh)
        {
            var path = $"{NodeDir}/{fresh.Id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(path);
            var sprite = Sprite("obj_" + fresh.Id);
            if (existing != null)
            {
                Object.DestroyImmediate(fresh);
                if (existing.Sprite == null && sprite != null) { existing.SetSprite(sprite); EditorUtility.SetDirty(existing); }
                return existing;
            }
            fresh.SetSprite(sprite);
            AssetDatabase.CreateAsset(fresh, path);
            return fresh;
        }

        internal static T Persist<T>(T fresh, string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                return fresh;
            }
            EditorUtility.CopySerialized(fresh, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(fresh);
            return existing;
        }
    }
}
