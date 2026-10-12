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
        public void EveryKindOfDamage_HasMutteringsAndTellingsOff_InTheStoryFiles_SpokenByWhoeverSawIt()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            foreach (var kind in new[] { VillageDamage.Tree, VillageDamage.Rock, VillageDamage.Soil })
            {
                var mutters = story.Set(VillageWatch.MutterSet(kind));
                var scolds = story.Set(VillageWatch.ScoldSet(kind));
                Assert.IsNotNull(mutters, kind + " mutters");
                Assert.IsNotNull(scolds, kind + " telling-offs");
                Assert.GreaterOrEqual(mutters.Entries.Count, 3, kind + " has a few things to mutter");
                Assert.AreEqual(3, scolds.Entries.Count, kind + " has three telling-offs");
                foreach (var e in mutters.Entries.Concat(scolds.Entries))
                {
                    var graph = story.Dialogue(e.Dialogue);
                    Assert.IsNotNull(graph, e.Dialogue);
                    foreach (var n in graph.Nodes)
                    {
                        Assert.AreEqual("*", n.Speaker, e.Dialogue + ": spoken by whoever saw it");
                        Assert.IsTrue(L.Has(n.Text), n.Text);
                    }
                }
                foreach (var e in scolds.Entries) Assert.AreEqual(2, story.Dialogue(e.Dialogue).Nodes.Count, "a telling-off is two lines");
            }
            Assert.IsTrue(L.Has("village.restored"));
        }

        [Test]
        public void ALineSpokenByWhoeverSawIt_IsSpokenByThatVillager()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var graph = story.Dialogue("village.scold.rock.1");
            var spoken = graph.WithSpeaker("tilda");
            Assert.AreEqual(graph.Nodes.Count, spoken.Nodes.Count);
            Assert.IsTrue(spoken.Nodes.All(n => n.Speaker == "tilda"), "every line is Tilda's");
            Assert.IsTrue(graph.Nodes.All(n => n.Speaker == "*"), "and the story's own graph is untouched");
            Assert.AreEqual(graph.Start, spoken.Start);
            Assert.AreEqual(graph.Nodes[0].Next, spoken.Nodes[0].Next);
            Assert.AreEqual(VillageWatch.SeedFor("bram", 5), VillageWatch.SeedFor("bram", 5), "the same villager and day pick the same line");
            Assert.AreNotEqual(VillageWatch.SeedFor("bram", 5, 0), VillageWatch.SeedFor("bram", 5, 1), "two witnesses say different things");
        }

        [Test]
        public void Rocks_StandOnlyOnOpenGrass_NeverBesideAnythingElse_AndAreTheSameEveryTime()
        {
            System.Func<int, int, bool> allGrass = (x, y) => true;
            var rocks = new List<string>();
            for (var y = 0; y < 60; y++)
                for (var x = 0; x < 60; x++)
                {
                    var r = VillageGreen.RockAt(x, y, allGrass);
                    Assert.AreEqual(r, VillageGreen.RockAt(x, y, allGrass), "stable");
                    if (r != null) rocks.Add(r);
                }
            Assert.That(rocks.Count, Is.InRange(15, 60), "a scattering");
            Assert.IsTrue(rocks.Contains(NodeDefaults.Rock));
            System.Func<int, int, bool> noPath = (x, y) => !(y == 30 && x >= 0);          // a road along row 30
            for (var y = 0; y < 60; y++)
                for (var x = 0; x < 60; x++)
                    if (VillageGreen.RockAt(x, y, noPath) != null) Assert.That(System.Math.Abs(y - 30), Is.GreaterThan(1), "no rock beside the road");
        }

        [Test]
        public void ARocksFlag_RoundTrips_AndOtherFlagsAreNotRocks()
        {
            Assert.IsTrue(VillageGreen.TryParseRockFlag(VillageGreen.RockFlag(12, -3, NodeDefaults.Boulder), out var x, out var y, out var type));
            Assert.AreEqual((12, -3, NodeDefaults.Boulder), (x, y, type));
            Assert.IsFalse(VillageGreen.TryParseRockFlag("village.trees.seeded", out _, out _, out _));
            Assert.IsFalse(VillageGreen.TryParseRockFlag("village.rock:1,2", out _, out _, out _));
            Assert.IsFalse(VillageGreen.TryParseRockFlag("village.rock:a,2,rock", out _, out _, out _));
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
            Assert.IsFalse(VillageGreen.ShouldRestore(0, 10), "no damage, nothing to do");
            Assert.IsFalse(VillageGreen.ShouldRestore(11, 10), "damaged today (day 10 is stored as 11)");
            Assert.IsTrue(VillageGreen.ShouldRestore(11, 11), "the next morning");
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
                s.SetVar(VillageGreen.DamagedVar, 3);
                var cells = new List<Vector3Int> { new Vector3Int(5, 5, 0), new Vector3Int(6, 5, 0), new Vector3Int(7, 5, 0), new Vector3Int(8, 5, 0) };
                var restored = VillageGreen.Restore(s, cells, tree);
                Assert.AreEqual(2, restored, "the stump and the cleared cell");
                foreach (var x in new[] { 5, 6, 7 }) { Assert.IsTrue(nodes.TryGet(x, 5, out var n)); Assert.AreEqual(NodeDefaults.Tree, n.TypeId, "x " + x); }
                nodes.TryGet(8, 5, out var other);
                Assert.AreEqual(NodeDefaults.Rock, other.TypeId);
                Assert.AreEqual(0, s.GetVar(VillageGreen.DamagedVar), "the damage is forgiven");
            }
        }
    }
}
