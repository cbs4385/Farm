using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-007: every texture under Assets/_Project/Art is imported as crisp pixel art (PPU 16, point filter, uncompressed).
    public sealed class TextureImportPostprocessor : AssetPostprocessor
    {
        const string ArtRoot = "Assets/_Project/Art/";
        public const int PixelsPerUnit = 16;

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
        }
    }
}
