using System.IO;
using System.Linq;
using Farm.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Farm.Tests
{
    // T-008: every placeholder sprite lives in exactly one sprite atlas, and the atlases are set up for pixel art.
    public class AtlasTests
    {
        const string SpriteDir = "Assets/_Project/Art/Placeholders";
        const string AtlasDir = "Assets/_Project/Art/Atlases";

        static string[] Sprites() => Directory.GetFiles(SpriteDir, "*.png").Select(Path.GetFileName).ToArray();

        [Test]
        public void EveryPlaceholderSpriteBelongsToExactlyOneAtlasGroup()
        {
            foreach (var file in Sprites())
            {
                var matches = AtlasBuilder.Groups.Count(g => g.prefixes.Any(file.StartsWith));
                Assert.AreEqual(1, matches, $"{file} must match exactly one atlas group");
            }
        }

        [Test]
        public void AnAtlasExistsPerGroup_AndPacksExactlyItsSprites()
        {
            foreach (var group in AtlasBuilder.Groups)
            {
                var path = $"{AtlasDir}/{group.name}.spriteatlasv2";
                Assert.IsTrue(File.Exists(path), $"{path} missing: run Farm/Setup/Create Sprite Atlases");
                // The imported main asset of a V2 atlas is a SpriteAtlas; its packables are what gets packed.
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                Assert.IsNotNull(atlas, path);

                var packed = atlas.GetPackables()
                    .Select(o => o is Sprite || o is Texture2D ? o.name : null)
                    .Where(n => n != null)
                    .OrderBy(n => n).ToArray();
                var expected = Sprites().Where(f => AtlasBuilder.AtlasNameFor(f) == group.name)
                    .Select(Path.GetFileNameWithoutExtension).OrderBy(n => n).ToArray();
                CollectionAssert.AreEqual(expected, packed, $"{group.name} contents");
            }
        }

        [Test]
        public void AtlasesAreConfiguredForPixelArt()
        {
            foreach (var group in AtlasBuilder.Groups)
            {
                var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath($"{AtlasDir}/{group.name}.spriteatlasv2");
                Assert.IsFalse(importer.packingSettings.enableRotation, group.name);
                Assert.IsFalse(importer.packingSettings.enableTightPacking, group.name);
                Assert.GreaterOrEqual(importer.packingSettings.padding, 2, $"{group.name}: padding stops neighbours bleeding");
                Assert.AreEqual(FilterMode.Point, importer.textureSettings.filterMode, group.name);
                Assert.IsFalse(importer.textureSettings.generateMipMaps, group.name);
            }
        }

        [Test]
        public void SpritePackerIsSetToV2()
        {
            Assert.AreEqual(SpritePackerMode.SpriteAtlasV2, EditorSettings.spritePackerMode);
        }
    }
}
