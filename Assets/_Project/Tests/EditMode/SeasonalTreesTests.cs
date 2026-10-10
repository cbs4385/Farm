using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Owner, 2026-10-09 (the Cozy Village kit): the trees of the outdoor maps change with the season and no two neighbours need be alike.
    public class SeasonalTreesTests
    {
        [SetUp]
        public void SetUp() => SeasonalTrees.ResetForTests();

        [Test]
        public void EverySeasonHasSeveralKindsOfTree_AllTheSizeOfTheOriginalTree_WithTheSamePivot()
        {
            var all = Resources.LoadAll<Sprite>("Trees");
            foreach (var set in new[] { "spring", "fall", "winter" })
            {
                var kinds = all.Where(s => s.name.StartsWith("tree_" + set + "_")).ToList();
                Assert.GreaterOrEqual(kinds.Count, 3, set + " has a choice");
                foreach (var tree in kinds)
                {
                    Assert.AreEqual(32f, tree.rect.width, tree.name);
                    Assert.AreEqual(32f, tree.rect.height, tree.name);
                    Assert.AreEqual(32f * 0.375f, tree.pivot.y, 0.01f, tree.name + ": the same pivot as obj_tree, so that it can take its place");
                }
            }
            Assert.AreEqual("spring", SeasonalTrees.SetFor(Season.Summer), "summer is green too");
        }

        [Test]
        public void TheKindOfATree_IsTheSameEveryTime_AndMixesTheKindsAndTheOriginalPine()
        {
            var kinds = 7;
            var picks = Enumerable.Range(0, 2000).Select(i => SeasonalTrees.KindAt(11, "Forest", i % 50, i / 50, kinds)).ToList();
            CollectionAssert.AreEqual(picks, Enumerable.Range(0, 2000).Select(i => SeasonalTrees.KindAt(11, "Forest", i % 50, i / 50, kinds)).ToList());
            Assert.IsTrue(picks.All(p => p >= -1 && p < kinds));
            Assert.AreEqual(kinds + 1, picks.Distinct().Count(), "every kind and the pine appear");
            var pines = picks.Count(p => p < 0) / (float)picks.Count;
            Assert.That(pines, Is.InRange(0.1f, 0.3f), "about " + SeasonalTrees.PineSlots + " in " + (kinds + SeasonalTrees.PineSlots) + " keep the pine");
            Assert.AreEqual(-1, SeasonalTrees.KindAt(11, "Forest", 3, 4, 0), "no kinds: the pine");
        }

        [Test]
        public void TheVillageProps_AreInTheGame_WithTheirFootAtTheFootOfTheCell()
        {
            foreach (var name in new[] { "prop_clock_tower", "prop_fountain", "prop_lamp", "prop_bench", "prop_flag", "prop_flowerbox_1", "prop_stall", "prop_barrels", "prop_hay", "prop_sacks" })
            {
                var path = "Assets/_Project/Art/Placeholders/" + name + ".png";
                var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.IsNotNull(sprite, name);
                Assert.AreEqual(8f, sprite.pivot.y, 0.01f, name + ": the pivot is half a cell above the foot, which is where a cell's centre is");
            }
        }
    }
}
