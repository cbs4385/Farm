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
        public override uint GetVersion() => 2;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
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
        }
    }
}
