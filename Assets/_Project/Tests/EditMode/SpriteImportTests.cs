using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Farm.Tests
{
    // Guards the pixel-art import rules (see TextureImportPostprocessor) for every placeholder sprite.
    public class SpriteImportTests
    {
        const string Dir = "Assets/_Project/Art/Placeholders";

        static string[] Paths() => Directory.GetFiles(Dir, "*.png").Select(p => p.Replace(Path.DirectorySeparatorChar, '/')).ToArray();

        [Test]
        public void AllSprites_AreSingleSprites_WithPointFilterAndPpu16()
        {
            foreach (var path in Paths())
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode, path);
                Assert.AreEqual(16f, importer.spritePixelsPerUnit, path);
                Assert.AreEqual(FilterMode.Point, importer.filterMode, path);
                Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression, path);
            }
        }

        [Test]
        public void CharacterSprites_PivotAtFeet_OthersAtCentre()
        {
            foreach (var path in Paths())
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.IsNotNull(sprite, path);
                var name = Path.GetFileName(path);
                var isCharacter = name.StartsWith("player_") || name.StartsWith("npc_");
                // sprite.pivot is in pixels from the bottom-left corner of the sprite rect
                Assert.AreEqual(sprite.rect.width / 2f, sprite.pivot.x, 0.01f, path);
                // A tree is taller than its cell: its pivot is set so that the trunk stands at the foot of the cell (see TextureImportPostprocessor).
                var expected = isCharacter ? 0f : name == "obj_tree.png" ? sprite.rect.height * 0.375f : name == "obj_sunpatch.png" ? 8f : sprite.rect.height / 2f;
                Assert.AreEqual(expected, sprite.pivot.y, 0.01f, path);
            }
        }
    }
}
