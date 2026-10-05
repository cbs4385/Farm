using System;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Farm.Tests
{
    // The village cat (owner request, 2026-10-05): a black cat that walks about the village, with its own pictures and strings.
    public class CatTests
    {
        static WalkGrid Open(int size, Func<int, int, bool> blocked = null) => new WalkGrid(0, 0, size, size, (x, y) => blocked == null || !blocked(x, y));

        [Test]
        public void TheRoute_StartsWhereTheCatIs_EndsNearby_AndStaysOnWalkableCells()
        {
            var grid = Open(30, (x, y) => x == 15 && y < 25);        // a long fence
            var rng = new Random(7);
            for (var i = 0; i < 200; i++)
            {
                var route = CatWander.PickRoute(grid, 10, 10, rng);
                Assert.IsNotNull(route);
                Assert.AreEqual((10, 10), route[0]);
                var end = route.Last();
                Assert.LessOrEqual(Math.Abs(end.x - 10), 7);
                Assert.LessOrEqual(Math.Abs(end.y - 10), 7);
                foreach (var cell in route.Skip(1)) Assert.IsTrue(grid.IsWalkable(cell.x, cell.y), "walkable " + cell);
            }
        }

        [Test]
        public void ACatBoxedInOnAllSides_HasNowhereToGo()
        {
            var grid = Open(20, (x, y) => Math.Abs(x - 10) <= 1 && Math.Abs(y - 10) <= 1 && !(x == 10 && y == 10));
            Assert.IsNull(CatWander.PickRoute(grid, 10, 10, new Random(3)));
        }

        [Test]
        public void TheCatGrids_AreWellFormed_AndEveryFacingHasTwoFrames()
        {
            foreach (var grid in CatSprites.AllGrids())
            {
                Assert.AreEqual(16, grid.Length);
                foreach (var row in grid) Assert.AreEqual(16, row.Length);
                Assert.Greater(CatSprites.Pixels(grid, false).Count(p => p.a > 0), 40, "a cat, not a speck");
                Assert.IsTrue(grid.All(r => r.All(c => ".bhen".IndexOf(c) >= 0)));
            }
            foreach (var facing in new[] { CatSprites.Down, CatSprites.Up, CatSprites.Left, CatSprites.Right })
            {
                var frames = CatSprites.Frames(facing);
                Assert.AreEqual(2, frames.Length);
                CollectionAssert.AreNotEqual(frames[0].texture.GetPixels32(), frames[1].texture.GetPixels32(), facing + ": the walk changes the picture");
            }
        }

        [Test]
        public void FacingRight_IsFacingLeftMirrored_AndTheCatIsBlack()
        {
            CatSprites.ClearCache();
            var left = CatSprites.Frames(CatSprites.Left)[0].texture.GetPixels32();
            var right = CatSprites.Frames(CatSprites.Right)[0].texture.GetPixels32();
            for (var y = 0; y < 16; y++)
                for (var x = 0; x < 16; x++)
                    Assert.AreEqual(left[y * 16 + x], right[y * 16 + 15 - x]);
            var body = left.Where(p => p.a > 0).GroupBy(p => p.GetHashCode()).OrderByDescending(g => g.Count()).First().First();
            Assert.Less(body.r + body.g + body.b, 120, "the commonest colour is near black");

            // A preview for a person to look at.
            var sheet = new Texture2D(16 * 8, 16 * 2, TextureFormat.RGBA32, false);
            var clear = Enumerable.Repeat(new Color32(120, 170, 100, 255), sheet.width * sheet.height).ToArray();
            sheet.SetPixels32(clear);
            var facings = new[] { CatSprites.Down, CatSprites.Up, CatSprites.Left, CatSprites.Right };
            for (var f = 0; f < 4; f++)
                for (var k = 0; k < 2; k++)
                {
                    var px = CatSprites.Frames(facings[f])[k].texture.GetPixels32();
                    for (var y = 0; y < 16; y++)
                        for (var x = 0; x < 16; x++)
                            if (px[y * 16 + x].a > 0) sheet.SetPixel((f * 2 + k) * 16 + x, 16 + y - 0, px[y * 16 + x]);
                }
            sheet.Apply();
            var dir = Path.Combine(Application.dataPath, "..", "Builds");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "cat_preview.png"), sheet.EncodeToPNG());
        }

        [Test]
        public void TheCatStrings_Exist()
        {
            L.SetLanguage("en");
            foreach (var key in new[] { "cat.name", "cat.pet.0", "cat.pet.1", "cat.pet.2" }) Assert.AreNotEqual(key, L.Get(key), key);
        }

        [Test]
        public void EveryVillager_HasATopicAboutEdmund_WithTwoLines()
        {
            L.SetLanguage("en");
            var story = StoryContent.LoadFromResources();
            foreach (var npc in new[] { "tilda", "marcus", "odalys", "dorian", "wren", "hazel", "bram", "juno", "piper", "elara", "felix", "ione" })
            {
                var topic = story.Topics.FirstOrDefault(t => t.Id == "edmund." + npc);
                Assert.IsNotNull(topic, npc);
                Assert.AreEqual(npc, topic.Npc);
                var dialogue = story.Dialogue(topic.Dialogue);
                Assert.IsNotNull(dialogue, topic.Dialogue);
                Assert.AreEqual(2, dialogue.Nodes.Count, npc);
            }
        }
    }
}
