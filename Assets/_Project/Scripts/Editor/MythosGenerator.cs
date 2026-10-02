using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Mythos;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // X-007/X-009: writes the horror layer's content pack (Resources/Packs/mythos): horror and mutant crops, relics and the two
    // extra weathers. The rows live in Farm.Mythos (MythosData); the pack is merged into the database at startup and sold by no shop.
    public static class MythosGenerator
    {
        const string Dir = "Assets/_Project/Data/Mythos";
        const string PackPath = "Assets/_Project/Resources/Packs/mythos.asset";

        public static void Generate()
        {
            Directory.CreateDirectory(Dir);
            Directory.CreateDirectory(Path.GetDirectoryName(PackPath));

            var items = new List<ItemDefinition>();
            var crops = new List<CropDefinition>();

            foreach (var row in MythosData.Crops)
            {
                var sprites = Enumerable.Range(0, row.Days.Length + 1).Select(i => ContentGenerator.Sprite($"crop_{row.Id}_{i}")).ToArray();
                var crop = CropDefinition.Create(row.Id, row.Days, SeasonMask.All, 0, sprites).WithKind(CropKind.Vegetable);
                if (row.GrowCondition != null) crop.SetGrowCondition(row.GrowCondition);
                crops.Add(ContentGenerator.Persist(crop, $"{Dir}/crop_{row.Id}.asset"));
                items.Add(ContentGenerator.Persist(ItemDefinition.Create($"seed.{row.Id}", ItemCategory.Seed, cropId: row.Id,
                    icon: ContentGenerator.Sprite($"item_seed_{row.Id}")), $"{Dir}/seed_{row.Id}.asset"));
                items.Add(ContentGenerator.Persist(ItemDefinition.Create($"crop.{row.Id}", ItemCategory.Crop, sellPrice: row.SellPrice,
                    icon: ContentGenerator.Sprite($"item_crop_{row.Id}")), $"{Dir}/item_{row.Id}.asset"));
            }
            foreach (var relic in MythosData.Relics)
                items.Add(ContentGenerator.Persist(ItemDefinition.Create(relic, ItemCategory.Misc, 1,
                    icon: ContentGenerator.Sprite("item_" + relic.Replace('.', '_'))), $"{Dir}/{relic}.asset"));

            var fog = Weather(MythosIds.Weather.Fog, new Color(0.72f, 0.74f, 0.78f), 0.40f, WeatherParticles.None, 0f, 0f, 0f);
            var blood = Weather(MythosIds.Weather.BloodMoon, new Color(0.75f, 0.20f, 0.20f), 0.35f, WeatherParticles.None, 0f, 0f, 0f);

            var pack = ContentPack.Create("mythos", items, crops, new[] { fog, blood });
            var existing = AssetDatabase.LoadAssetAtPath<ContentPack>(PackPath);
            if (existing == null) AssetDatabase.CreateAsset(pack, PackPath);
            else { EditorUtility.CopySerialized(pack, existing); EditorUtility.SetDirty(existing); Object.DestroyImmediate(pack); }
            Debug.Log($"[MythosGenerator] pack with {items.Count} items, {crops.Count} crops, 2 weathers.");
        }

        // Weather in the pack is never rolled by the base weights (weight 0): modifiers add it.
        static WeatherDefinition Weather(string id, Color tint, float strength, WeatherParticles particles, float density, float slant, float w)
        {
            var path = $"{Dir}/weather_{id}.asset";
            var fresh = WeatherDefinition.Create(id, tint, strength, false, particles, density, slant, false, w, w, w, w);
            return ContentGenerator.Persist(fresh, path);
        }
    }
}
