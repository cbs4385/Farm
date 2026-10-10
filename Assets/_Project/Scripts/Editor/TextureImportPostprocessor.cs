using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-007: every texture under Assets/_Project/Art is imported as crisp pixel art (PPU 16, point filter, uncompressed).
    public sealed class TextureImportPostprocessor : AssetPostprocessor
    {
        const string ArtRoot = "Assets/_Project/Art/";
        public const int PixelsPerUnit = 16;

        // Bump when import rules change so existing art is reimported.
        public override uint GetVersion() => 9;

        // Full-screen pictures (the ending illustrations) are loaded as plain textures, kept at their exact size and crisp.
        const string PictureRoot = "Assets/_Project/Resources/Endings/";

        // The title picture is moved by the breeze in less than a pixel steps, so it is smooth-filtered, and readable so that the game can work out which
        // parts of it are foliage (TitleSway.Weights).
        const string TitleRoot = "Assets/_Project/Resources/Title/";

        // The ground decoration (tufts and flowers laid over the grass at run time): crisp sprites like the rest of the art.
        const string DecorRoot = "Assets/_Project/Resources/Decor/";
        const string TreesRoot = "Assets/_Project/Resources/Trees/";          // the seasonal trees SeasonalTrees swaps in

        void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(TitleRoot))
            {
                var title = (TextureImporter)assetImporter;
                title.textureType = TextureImporterType.Default;
                title.filterMode = FilterMode.Bilinear;
                title.isReadable = true;
                title.textureCompression = TextureImporterCompression.Uncompressed;
                title.mipmapEnabled = false;
                title.npotScale = TextureImporterNPOTScale.None;
                title.alphaSource = TextureImporterAlphaSource.None;
                title.maxTextureSize = 2048;
                return;
            }
            if (assetPath.StartsWith(PictureRoot))
            {
                var picture = (TextureImporter)assetImporter;
                picture.textureType = TextureImporterType.Default;
                picture.filterMode = FilterMode.Point;
                picture.textureCompression = TextureImporterCompression.Uncompressed;
                picture.mipmapEnabled = false;
                picture.npotScale = TextureImporterNPOTScale.None;
                picture.alphaSource = TextureImporterAlphaSource.None;
                return;
            }
            if (!assetPath.StartsWith(ArtRoot) && !assetPath.StartsWith(DecorRoot) && !assetPath.StartsWith(TreesRoot)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            // The project's default texture preset imports as Multiple, which ignores the single-sprite pivot below.
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;

            // Characters pivot at their feet so transform.position is the tile they stand on.
            var file = System.IO.Path.GetFileName(assetPath);
            if (file.StartsWith("player_") || file.StartsWith("npc_"))
            {
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(settings);
                importer.spritePivot = new Vector2(0.5f, 0f);
            }
            else if (assetPath.StartsWith(TreesRoot))
            {
                // The same pivot as obj_tree.png (the trunk's foot at the foot of the cell), so that a seasonal tree can take a tree's place exactly.
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(settings);
                importer.spritePivot = new Vector2(0.5f, 0.375f);
            }
            else if (file.StartsWith("prop_"))
            {
                // Things that stand in the village and on the farm (a lamp post, a clock tower, a stall): the foot of the picture at the foot of the cell, however tall it is.
                importer.GetSourceTextureWidthAndHeight(out var width, out var height);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(settings);
                importer.spritePivot = new Vector2(0.5f, Mathf.Clamp01(8f / Mathf.Max(8, height)));
            }
            else if (file == "obj_tree.png" || file == "obj_sapling.png" || file == "obj_sunpatch.png")
            {
                // A tree is taller than its cell: the pivot puts the foot of the trunk at the foot of the cell (a Tilemap centres the pivot in the cell, and a
                // scene object stands on its position), and the crown rises over the cells above.
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(settings);
                importer.spritePivot = new Vector2(0.5f, file == "obj_tree.png" ? 0.375f : file == "obj_sunpatch.png" ? 8f / 48f : 0.5f);
            }
        }
    }
}
