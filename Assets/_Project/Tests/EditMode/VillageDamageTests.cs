using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-11: the player may chop in the village, and the villagers mind: they mutter, one tells the player off, and they put the green back.
    public class VillageDamageTests
    {
        [Test]
        public void EveryMutterAndScold_HasItsWords()
        {
            for (var i = 0; i < VillageWatch.MutterLines; i++) Assert.IsTrue(L.Has("village.mutter." + i), "mutter " + i);
            for (var v = 0; v < VillageWatch.ScoldLines; v++)
                foreach (var part in new[] { 0, 1 }) Assert.IsTrue(L.Has($"village.scold.{v}.{part}"), $"scold {v}.{part}");
            Assert.IsTrue(L.Has("village.restored"));
            foreach (var kind in new[] { VillageDamage.Rock, VillageDamage.Soil })
            {
                for (var i = 0; i < VillageWatch.MinorMutterLines; i++) Assert.IsTrue(L.Has(VillageWatch.MutterKey(kind, i)), kind + " mutter " + i);
                for (var v = 0; v < VillageWatch.ScoldLines; v++)
                    foreach (var part in new[] { 0, 1 }) Assert.IsTrue(L.Has(VillageWatch.ScoldKey(kind, v, part)), $"{kind} scold {v}.{part}");
            }
        }

        [Test]
        public void TwoWitnesses_MutterDifferentThings_AndTheScoldIsTwoLinesLong()
        {
            Assert.AreNotEqual(VillageWatch.MutterFor("tilda", 3, 0), VillageWatch.MutterFor("tilda", 3, 1));
            for (var d = 0; d < 20; d++)
            {
                Assert.That(VillageWatch.MutterFor("bram", d), Is.InRange(0, VillageWatch.MutterLines - 1));
                Assert.That(VillageWatch.ScoldFor("marcus", d), Is.InRange(0, VillageWatch.ScoldLines - 1));
            }
            var g = VillageWatch.ScoldGraph("tilda", 1);
            Assert.AreEqual("n0", g.Start);
            Assert.AreEqual("n1", g.Node("n0").Next);
            Assert.AreEqual("village.scold.1.1", g.Node("n1").Text);
            Assert.AreEqual("tilda", g.Node("n1").Speaker);
        }

        [Test]
        public void Rocks_StandOnlyOnOpenGrass_NeverBesideAnythingElse_AndAreTheSameEveryTime()
        {
            System.Func<int, int, bool> allGrass = (x, y) => true;
            var rocks = new List<string>();
            for (var y = 0; y < 60; y++)
                for (var x = 0; x < 60; x++)
                {
                    var r = VillageTrees.RockAt(x, y, allGrass);
                    Assert.AreEqual(r, VillageTrees.RockAt(x, y, allGrass), "stable");
                    if (r != null) rocks.Add(r);
                }
            Assert.That(rocks.Count, Is.InRange(15, 60), "a scattering");
            Assert.IsTrue(rocks.Contains(NodeDefaults.Rock));
            System.Func<int, int, bool> noPath = (x, y) => !(y == 30 && x >= 0);          // a road along row 30
            for (var y = 0; y < 60; y++)
                for (var x = 0; x < 60; x++)
                    if (VillageTrees.RockAt(x, y, noPath) != null) Assert.That(System.Math.Abs(y - 30), Is.GreaterThan(1), "no rock beside the road");
        }

        [Test]
        public void ClearingTheGrid_LevelsEveryDugTile()
        {
            var grid = new FarmGrid();
            grid.Till(1, 1); grid.Till(2, 2);
            Assert.AreEqual(2, grid.Clear());
            Assert.IsFalse(grid.IsTilled(1, 1));
            Assert.AreEqual(0, grid.Count);
        }

        [Test]
        public void TheGreenIsPutBack_TheDayAfter_NotTheSameDay()
        {
            Assert.IsFalse(VillageTrees.ShouldRestore(0, 10), "no damage, nothing to do");
            Assert.IsFalse(VillageTrees.ShouldRestore(11, 10), "damaged today (day 10 is stored as 11)");
            Assert.IsTrue(VillageTrees.ShouldRestore(11, 11), "the next morning");
        }

        [Test]
        public void Restoring_ReplantsStumpsAndGaps_ButLeavesAnythingElseAlone()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var tree = s.Nodes.Get(NodeDefaults.Tree);
                var stump = s.Nodes.Get(NodeDefaults.Stump);
                var rock = s.Nodes.Get(NodeDefaults.Rock);
                var nodes = s.GetNodes(MapIds.Village);
                nodes.Add(5, 5, tree);                    // untouched
                nodes.Add(6, 5, stump);                   // felled and left a stump
                nodes.Add(8, 5, rock);                    // something else stands there now
                s.SetVar(VillageTrees.DamagedVar, 3);
                var cells = new List<Vector3Int> { new Vector3Int(5, 5, 0), new Vector3Int(6, 5, 0), new Vector3Int(7, 5, 0), new Vector3Int(8, 5, 0) };
                var restored = VillageTrees.Restore(s, cells, tree);
                Assert.AreEqual(2, restored, "the stump and the cleared cell");
                foreach (var x in new[] { 5, 6, 7 }) { Assert.IsTrue(nodes.TryGet(x, 5, out var n)); Assert.AreEqual(NodeDefaults.Tree, n.TypeId, "x " + x); }
                nodes.TryGet(8, 5, out var other);
                Assert.AreEqual(NodeDefaults.Rock, other.TypeId);
                Assert.AreEqual(0, s.GetVar(VillageTrees.DamagedVar), "the damage is forgiven");
            }
        }
    }
}
