using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Farm.Editor
{
    // T-008: groups the placeholder sprites into Sprite Atlas (V2) assets by category, so a map draws from a few
    // textures instead of dozens. Pixel-art safe: no rotation, no tight packing, point filtering, uncompressed.
    // Re-running regenerates the atlases from the sprites currently in Art/Placeholders.
    public static class AtlasBuilder
    {
        const string SpriteDir = "Assets/_Project/Art/Placeholders";
        const string AtlasDir = "Assets/_Project/Art/Atlases";

        // Atlas name -> file-name prefixes that belong in it. Every placeholder sprite must match exactly one atlas
        // (checked by a test), so a new category forces a decision here.
        public static readonly (string name, string[] prefixes)[] Groups =
        {
            ("Tiles", new[] { "tile_", "bld_" }),
            ("Characters", new[] { "player_", "npc_" }),
            ("Crops", new[] { "crop_" }),
            ("Items", new[] { "item_" }),
            ("World", new[] { "obj_" }),
            ("UI", new[] { "ui_", "hud_", "fx_" }),
        };

        public static string AtlasNameFor(string spriteFileName)
        {
            foreach (var g in Groups)
                foreach (var prefix in g.prefixes)
                    if (spriteFileName.StartsWith(prefix)) return g.name;
            return null;
        }

        [MenuItem("Farm/Setup/Create Sprite Atlases")]
        public static void Build()
        {
            Directory.CreateDirectory(AtlasDir);

            // Pack in the Editor and in builds with Sprite Atlas V2.
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;

            var sprites = Directory.GetFiles(SpriteDir, "*.png")
                .Select(p => p.Replace('\\', '/'))
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null)
                .ToList();

            foreach (var g in Groups)
            {
                var members = sprites.Where(s => AtlasNameFor(s.name + ".png") == g.name).Cast<Object>().ToArray();
                var path = $"{AtlasDir}/{g.name}.spriteatlasv2";

                var asset = new SpriteAtlasAsset();
                asset.SetIncludeInBuild(true);
                if (members.Length > 0) asset.Add(members);
                SpriteAtlasAsset.Save(asset, path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                ConfigurePixelArt(path);
                Debug.Log($"[AtlasBuilder] {g.name}: {members.Length} sprites");
            }

            AssetDatabase.SaveAssets();
        }

        public static void BuildAndExit()
        {
            Build();
            EditorApplication.Exit(0);
        }

        // Pixel-art packing: no rotation or tight packing (they blur and bleed), padding between sprites so scaling
        // never samples a neighbour, point filtering, no mipmaps, and uncompressed colour.
        static void ConfigurePixelArt(string atlasPath)
        {
            var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(atlasPath);
            importer.packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                padding = 4,
                enableRotation = false,
                enableTightPacking = false,
                enableAlphaDilation = true,
            };
            importer.textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Point,
            };
            importer.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "DefaultTexturePlatform",
                maxTextureSize = 2048,
                textureCompression = TextureImporterCompression.Uncompressed,
                format = TextureImporterFormat.RGBA32,
            });
            importer.SaveAndReimport();
        }
    }
}
