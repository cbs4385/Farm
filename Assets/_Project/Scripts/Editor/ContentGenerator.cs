using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
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
        const string DbPath = Root + "/Resources/GameDatabase.asset";
        const string ArtDir = Root + "/Art/Placeholders";

        struct CropRow
        {
            public string Id; public int[] Days; public SeasonMask Seasons; public int Regrow;
            public int SeedPrice; public int SellPrice;
        }

        // growth stages must match PlaceholderArtGenerator.Crops
        static readonly CropRow[] CropTable =
        {
            new CropRow { Id = "parsnip",     Days = new[] { 1, 1, 1, 1 },    Seasons = SeasonMask.Spring, SeedPrice = 20, SellPrice = 35 },
            new CropRow { Id = "potato",      Days = new[] { 1, 1, 1, 2, 1 }, Seasons = SeasonMask.Spring, SeedPrice = 50, SellPrice = 80 },
            new CropRow { Id = "cauliflower", Days = new[] { 2, 4, 4, 2 },    Seasons = SeasonMask.Spring, SeedPrice = 80, SellPrice = 175 },
            new CropRow { Id = "greenbean",   Days = new[] { 1, 1, 1, 3, 4 }, Seasons = SeasonMask.Spring, Regrow = 3, SeedPrice = 60, SellPrice = 40 },
            new CropRow { Id = "strawberry",  Days = new[] { 1, 1, 2, 2, 2 }, Seasons = SeasonMask.Spring, Regrow = 4, SeedPrice = 100, SellPrice = 120 },
            new CropRow { Id = "kale",        Days = new[] { 1, 2, 2, 1 },    Seasons = SeasonMask.Spring, SeedPrice = 70, SellPrice = 110 },
        };

        [MenuItem("Farm/Setup/Generate Content")]
        public static void GenerateAll()
        {
            PlaceholderArtGenerator.Generate();   // sprites must exist and be imported first
            Directory.CreateDirectory(ItemDir);
            Directory.CreateDirectory(CropDir);
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));

            var items = new List<ItemDefinition>();
            var crops = new List<CropDefinition>();

            Tool(items, ItemIds.Hoe, ToolType.Hoe, "item_tool_hoe");
            Tool(items, ItemIds.WateringCan, ToolType.WateringCan, "item_tool_wateringcan");
            Tool(items, ItemIds.Axe, ToolType.Axe, "item_tool_axe");
            Tool(items, ItemIds.Pickaxe, ToolType.Pickaxe, "item_tool_pickaxe");
            Tool(items, ItemIds.Scythe, ToolType.Scythe, "item_tool_scythe");

            Save(items, ItemDefinition.Create(ItemIds.Wood, ItemCategory.Resource, sellPrice: 2, icon: Sprite("item_resource_wood")));
            Save(items, ItemDefinition.Create(ItemIds.Stone, ItemCategory.Resource, sellPrice: 2, icon: Sprite("item_resource_stone")));
            Save(items, ItemDefinition.Create(ItemIds.Fiber, ItemCategory.Resource, sellPrice: 1, icon: Sprite("item_resource_fiber")));

            foreach (var row in CropTable)
            {
                var sprites = Enumerable.Range(0, row.Days.Length + 1).Select(i => Sprite($"crop_{row.Id}_{i}")).ToArray();
                SaveCrop(crops, CropDefinition.Create(row.Id, row.Days, row.Seasons, row.Regrow, sprites));
                Save(items, ItemDefinition.Create(ItemIds.Seed(row.Id), ItemCategory.Seed, buyPrice: row.SeedPrice,
                    cropId: row.Id, icon: Sprite($"item_seed_{row.Id}"), soldIn: new[] { "general" }));
                Save(items, ItemDefinition.Create(ItemIds.Crop(row.Id), ItemCategory.Crop, sellPrice: row.SellPrice,
                    icon: Sprite($"item_crop_{row.Id}")));
            }

            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DbPath);
            if (db == null)
            {
                db = GameDatabase.Create(items, crops);
                AssetDatabase.CreateAsset(db, DbPath);
            }
            else
            {
                db.SetContents(items, crops);
                EditorUtility.SetDirty(db);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ContentGenerator] {items.Count} items, {crops.Count} crops.");
        }

        public static void GenerateAllAndExit()
        {
            GenerateAll();
            EditorApplication.Exit(0);
        }

        static void Tool(List<ItemDefinition> items, string id, ToolType type, string sprite) =>
            Save(items, ItemDefinition.Create(id, ItemCategory.Tool, maxStack: 1, toolType: type, icon: Sprite(sprite)));

        static Sprite Sprite(string name)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");
            if (s == null) Debug.LogWarning($"[ContentGenerator] Missing placeholder sprite '{name}'.");
            return s;
        }

        // Writes the definition to disk (updating in place when the asset exists) and appends the persistent asset.
        static void Save(List<ItemDefinition> list, ItemDefinition fresh) => list.Add(Persist(fresh, $"{ItemDir}/{fresh.Id}.asset"));
        static void SaveCrop(List<CropDefinition> list, CropDefinition fresh) => list.Add(Persist(fresh, $"{CropDir}/{fresh.Id}.asset"));

        static T Persist<T>(T fresh, string path) where T : ScriptableObject
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
