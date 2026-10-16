using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Farm.Core;
using Farm.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-16: every item has an asset of its own and a name to show on hover. No two items share a picture (by file name or by what is drawn), none lacks one, and each
    // has a name that is in en.json and tells it apart from every other item.
    public class ItemAssetsTests
    {
        static readonly Dictionary<string, string> En = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        static List<ItemDefinition> Items() => AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(i => i != null).OrderBy(i => i.Id, System.StringComparer.Ordinal).ToList();

        static string Hash(Sprite sprite)
        {
            var path = AssetDatabase.GetAssetPath(sprite);          // the picture file the sprite comes from
            using (var md5 = MD5.Create()) return System.BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(path)));
        }

        [Test]
        public void EveryItem_IsAnAssetOfItsOwn_WithAnId()
        {
            var items = Items();
            Assert.GreaterOrEqual(items.Count, 200, "the item assets are found");
            var dupes = items.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(dupes, "two assets with the same id: " + string.Join(", ", dupes));
            foreach (var item in items) Assert.IsFalse(string.IsNullOrEmpty(item.Id), item.name);
        }

        [Test]
        public void EveryItem_HasAPicture()
        {
            var missing = Items().Where(i => i.Icon == null).Select(i => i.Id).ToList();
            Assert.IsEmpty(missing, "items without a picture: " + string.Join(", ", missing));
        }

        [Test]
        public void NoTwoItems_ShareAPicture()
        {
            var items = Items().Where(i => i.Icon != null).ToList();
            var byName = items.GroupBy(i => i.Icon.name).Where(g => g.Count() > 1).Select(g => g.Key + " (" + string.Join(", ", g.Select(i => i.Id)) + ")").ToList();
            Assert.IsEmpty(byName, "the same picture file on several items: " + string.Join("; ", byName));
            var byLook = items.GroupBy(i => Hash(i.Icon)).Where(g => g.Count() > 1).Select(g => string.Join(" = ", g.Select(i => i.Id))).ToList();
            Assert.IsEmpty(byLook, "pictures that look exactly the same: " + string.Join("; ", byLook));
        }

        [Test]
        public void EveryItem_HasAHoverName_ThatIsInEnJson_AndIsNotSharedWithAnotherItem()
        {
            var items = Items();
            var missing = items.Where(i => !En.TryGetValue(i.NameKey, out var n) || string.IsNullOrWhiteSpace(n)).Select(i => i.NameKey).ToList();
            Assert.IsEmpty(missing, "names missing from en.json: " + string.Join(", ", missing));
            var same = items.GroupBy(i => En[i.NameKey].Trim().ToLowerInvariant()).Where(g => g.Count() > 1).Select(g => g.Key + " (" + string.Join(", ", g.Select(i => i.Id)) + ")").ToList();
            Assert.IsEmpty(same, "two items with the same name: " + string.Join("; ", same));
        }

        // What stands in the world when an item is set down is named on hover with the item's own name (PlacedObjectActor).
        [Test]
        public void EveryThingThatCanBeSetDown_HasAHoverName()
        {
            var ids = Farm.Gameplay.CraftingDefaults.CreatePlaceables().Select(p => p.Id).Distinct().ToList();
            Assert.Greater(ids.Count, 20, "the placeable items are found");
            foreach (var id in ids)
                Assert.IsTrue(En.TryGetValue("item.machine." + id + ".name", out var name) && !string.IsNullOrWhiteSpace(name), "item.machine." + id + ".name is the hover name of the placed " + id);
        }

        // The generator's flat squares (an outline, one or two fill colours, a solid rectangle) were the placeholders; none is left in an item or object picture.
        [Test]
        public void NoItemOrObjectPicture_IsALeftOverFlatPlaceholder()
        {
            var flat = new List<string>();
            foreach (var file in Directory.GetFiles("Assets/_Project/Art/Placeholders", "*.png").OrderBy(f => f, System.StringComparer.Ordinal))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!name.StartsWith("item_") && !name.StartsWith("obj_") && name != "ui_cursor") continue;
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(file));
                var px = tex.GetPixels32();
                // A flat square is a solid rectangle in which every row inside the one-pixel outline is a single colour (bands of flat fill).
                int minX = tex.width, minY = tex.height, maxX = -1, maxY = -1, solid = 0;
                for (var y = 0; y < tex.height; y++)
                    for (var x = 0; x < tex.width; x++)
                        if (px[y * tex.width + x].a > 0) { solid++; minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                var rect = solid > 0 && solid == (maxX - minX + 1) * (maxY - minY + 1) && solid >= 36;
                var bands = rect;
                for (var y = minY + 1; rect && y < maxY; y++)
                    for (var x = minX + 2; x < maxX - 1; x++)
                        if (!px[y * tex.width + x].Equals(px[y * tex.width + minX + 1])) { bands = false; break; }
                if (rect && bands) flat.Add(name);
                Object.DestroyImmediate(tex);
            }
            Assert.IsEmpty(flat, "flat placeholder squares are still used: " + string.Join(", ", flat));
        }

        [Test]
        public void EveryItem_HasADescription_ForTheTooltip()
        {
            var missing = Items().Where(i => !En.TryGetValue(i.DescriptionKey, out var d) || string.IsNullOrWhiteSpace(d)).Select(i => i.DescriptionKey).ToList();
            Assert.IsEmpty(missing, "descriptions missing from en.json: " + string.Join(", ", missing));
        }
    }
}
