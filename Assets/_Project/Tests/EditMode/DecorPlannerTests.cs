using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Owner's feedback, 2026-10-09: "the world looks flat". Tufts and flowers are scattered over the grass, in the colours of the season.
    public class DecorPlannerTests
    {
        [Test]
        public void ThePlan_IsTheSameForTheSameMapAndSeed_AndDiffersBetweenMaps()
        {
            var a = Enumerable.Range(0, 400).Select(i => DecorPlanner.At(7, "Farm", i % 20, i / 20, 10, 5, out _)).ToList();
            var b = Enumerable.Range(0, 400).Select(i => DecorPlanner.At(7, "Farm", i % 20, i / 20, 10, 5, out _)).ToList();
            var other = Enumerable.Range(0, 400).Select(i => DecorPlanner.At(7, "Village", i % 20, i / 20, 10, 5, out _)).ToList();
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreNotEqual(a, other);
        }

        [Test]
        public void AboutTheRightShareOfCellsCarrySomething_MostlyTufts_AndEveryIndexIsInRange()
        {
            int cells = 0, placed = 0, flowers = 0;
            for (var x = 0; x < 100; x++)
                for (var y = 0; y < 100; y++)
                {
                    cells++;
                    var i = DecorPlanner.At(3, "Farm", x, y, 14, 7, out var flower);
                    if (i < 0) continue;
                    placed++;
                    if (flower) { flowers++; Assert.Less(i, 7); } else Assert.Less(i, 14);
                }
            Assert.That(placed / (float)cells, Is.InRange(DecorPlanner.Density - 0.03f, DecorPlanner.Density + 0.03f));
            Assert.That(flowers / (float)placed, Is.InRange(DecorPlanner.FlowerShare - 0.06f, DecorPlanner.FlowerShare + 0.06f));
        }

        [Test]
        public void WithNoFlowers_OnlyTuftsAppear_AndWithNothing_NothingDoes()
        {
            for (var x = 0; x < 60; x++)
                for (var y = 0; y < 60; y++)
                {
                    var i = DecorPlanner.At(1, "Farm", x, y, 4, 0, out var flower);
                    if (i >= 0) { Assert.IsFalse(flower); Assert.Less(i, 4); }
                    Assert.AreEqual(-1, DecorPlanner.At(1, "Farm", x, y, 0, 0, out _));
                }
        }

        [Test]
        public void EverySeason_HasItsPictures()
        {
            GroundDecor.ResetForTests();
            var names = Resources.LoadAll<Sprite>("Decor").Select(s => s.name).ToList();
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter })
            {
                var set = GroundDecor.SetFor(season);
                Assert.IsTrue(names.Any(n => n.StartsWith("decor_" + set + "_t")), set + " tufts");
                Assert.IsTrue(names.Any(n => n.StartsWith("decor_" + set + "_f")), set + " flowers");
            }
            Assert.IsTrue(names.Any(n => n.StartsWith("decor_sand_t")), "pebbles and driftwood for the beach");
            Assert.AreEqual("spring", GroundDecor.SetFor(Season.Summer), "summer is green too");
            Assert.AreEqual("fall", GroundDecor.SetFor(Season.Fall));
            Assert.AreEqual("winter", GroundDecor.SetFor(Season.Winter));
        }
    }
}
