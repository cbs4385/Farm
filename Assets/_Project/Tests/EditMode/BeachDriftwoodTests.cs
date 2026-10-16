using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-16: "the drift wood and rocks on the beach are not collectable, nor do they have hover over labels, violating a core rule. Have the drift wood be collectable
    // with the axe and each day, a random amount of drift wood should be left on the beach in random locations near the water, between the shop and the water line."
    public class BeachDriftwoodTests
    {
        const int W = 36, H = 24, Stall = BeachDriftwood.StallRow;

        // A shore like the beach's: the water reaches up a little further in some columns.
        static bool Water(int x, int y) => y < 5 + (x % 5 == 0 ? 2 : x % 3 == 0 ? 1 : 0);
        static bool Sand(int x, int y) => y < H - 6;
        static List<(int x, int y)> Candidates(System.Func<int, int, bool> blocked = null) =>
            BeachDriftwood.Candidates(W, H, Water, Sand, blocked ?? ((x, y) => false), Stall);

        [Test]
        public void TheDriftwood_IsANodeAnAxeClears_ThatGivesWoodAndHasAName()
        {
            var def = NodeDefaults.CreateAll().First(n => n.Id == BeachDriftwood.NodeId);
            Assert.AreEqual(ToolType.Axe, def.Tool);
            Assert.AreEqual(ItemIds.Wood, def.DropItemId);
            Assert.That(def.DropMin, Is.InRange(1, 2)); Assert.That(def.DropMax, Is.InRange(1, 3));
            Assert.IsFalse(def.Solid, "it can be walked over: it lies on the sand");
            Assert.AreEqual(0, def.SpawnWeight, "the clutter generator never places it; BeachDriftwood does");
            Assert.IsTrue(NodeLabels.NamesItself(def), "an axe node names itself in the hover label");
            Assert.AreEqual("node.driftwood", def.NameKey);
        }

        [Test]
        public void AnAxeBreaksAPieceInOneSwing_AndItGivesWood()
        {
            var def = NodeDefaults.CreateAll().First(n => n.Id == BeachDriftwood.NodeId);
            var grid = new NodeGrid();
            Assert.IsTrue(grid.Add(5, 6, def));
            var result = grid.Hit(5, 6, ToolType.Axe, 0, id => id == def.Id ? def : null, 0.5f);
            Assert.AreEqual(NodeHit.Cleared, result.Outcome);
            Assert.AreEqual(ItemIds.Wood, result.DropItemId);
            Assert.GreaterOrEqual(result.DropCount, 1);
            Assert.IsFalse(grid.Has(5, 6));
            grid.Add(5, 6, def);
            Assert.AreEqual(NodeHit.WrongTool, grid.Hit(5, 6, ToolType.Pickaxe, 3, id => id == def.Id ? def : null, 0.5f).Outcome, "only the axe");
        }

        [Test]
        public void TheDayBringsAFewPieces_AndTheNumberVariesFromDayToDay()
        {
            var counts = new HashSet<int>();
            for (var day = 1; day <= 120; day++)
            {
                var n = BeachDriftwood.CountFor(day, 4242);
                Assert.That(n, Is.InRange(BeachDriftwood.MinPerDay, BeachDriftwood.MaxPerDay));
                counts.Add(n);
            }
            Assert.GreaterOrEqual(counts.Count, 5, "a random amount, not always the same");
            Assert.AreEqual(BeachDriftwood.CountFor(9, 4242), BeachDriftwood.CountFor(9, 4242), "the same day of the same world brings the same");
            Assert.IsTrue(Enumerable.Range(1, 20).Any(d => BeachDriftwood.CountFor(d, 1) != BeachDriftwood.CountFor(d, 2)), "another world, another amount");
        }

        [Test]
        public void ThePieces_LieOnTheSandNearTheWater_BetweenTheWaterLineAndTheStall()
        {
            var cands = Candidates();
            Assert.Greater(cands.Count, 40);
            foreach (var (x, y) in cands)
            {
                Assert.IsFalse(Water(x, y), $"({x},{y}) is in the water");
                Assert.IsTrue(Sand(x, y));
                Assert.LessOrEqual(y, Stall, $"({x},{y}) is beyond the stall");
                var line = Enumerable.Range(0, H).Where(yy => Water(x, yy)).Max();
                Assert.That(y - line, Is.InRange(1, BeachDriftwood.ReachRows), $"({x},{y}) is not near the water");
            }
        }

        [Test]
        public void AnyCellACandidateBlocks_IsLeftAlone()
        {
            var blocked = new HashSet<(int, int)> { (10, 11), (11, 11), (12, 11), (13, 9) };
            var cands = Candidates((x, y) => blocked.Contains((x, y)));
            foreach (var b in blocked) CollectionAssert.DoesNotContain(cands, b);
        }

        [Test]
        public void EachDaysPieces_AreDistinctSpacedAndDifferFromDayToDay_AndTheSameForTheSameDay()
        {
            var cands = Candidates();
            var seen = new HashSet<string>();
            for (var day = 1; day <= 40; day++)
            {
                var count = BeachDriftwood.CountFor(day, 77);
                var cells = BeachDriftwood.Choose(cands, count, day, 77);
                Assert.That(cells.Count, Is.InRange(1, count));
                Assert.AreEqual(cells.Count, cells.Distinct().Count());
                foreach (var c in cells) CollectionAssert.Contains(cands, c);
                foreach (var a in cells)
                    foreach (var b in cells)
                        if (a != b) Assert.IsFalse(System.Math.Abs(a.x - b.x) <= 1 && System.Math.Abs(a.y - b.y) <= 1, $"day {day}: {a} and {b} are side by side");
                CollectionAssert.AreEqual(cells, BeachDriftwood.Choose(cands, count, day, 77), "the same day gives the same places");
                seen.Add(string.Join(";", cells));
            }
            Assert.GreaterOrEqual(seen.Count, 35, "random places every day");
        }

        [Test]
        public void RefreshingTheBeach_TakesYesterdaysPiecesAway_LaysToday_AndLeavesEverythingElse()
        {
            var driftwood = NodeDefaults.CreateAll().First(n => n.Id == BeachDriftwood.NodeId);
            var rock = NodeDefaults.CreateAll().First(n => n.Id == NodeDefaults.Rock);
            var grid = new NodeGrid();
            var cands = Candidates();
            var first = BeachDriftwood.Refresh(grid, driftwood, cands, 10, 5);
            Assert.AreEqual(first, grid.Nodes.Count(n => n.TypeId == BeachDriftwood.NodeId));
            Assert.That(first, Is.InRange(1, BeachDriftwood.MaxPerDay));
            var yesterday = grid.Nodes.Where(n => n.TypeId == BeachDriftwood.NodeId).Select(n => (n.X, n.Y)).ToList();

            var free = cands.First(c => !yesterday.Contains(c) && !yesterday.Any(y => System.Math.Abs(y.Item1 - c.x) <= 2));
            grid.Add(free.x, free.y, rock);
            var second = BeachDriftwood.Refresh(grid, driftwood, cands.Where(c => c != free).ToList(), 11, 5);
            Assert.AreEqual(second, grid.Nodes.Count(n => n.TypeId == BeachDriftwood.NodeId), "only today's pieces lie there");
            Assert.IsTrue(grid.TryGet(free.x, free.y, out var kept) && kept.TypeId == NodeDefaults.Rock, "a rock is not washed away");
            var today = grid.Nodes.Where(n => n.TypeId == BeachDriftwood.NodeId).Select(n => (n.X, n.Y)).ToList();
            CollectionAssert.AreNotEquivalent(yesterday, today, "the tide brought other pieces");
        }

        [Test]
        public void TheBeachRocks_AreARealNodeTable_ThePickaxeBreaks()
        {
            var table = ForageDefaults.CreateTables().Single(t => t.Id == "beach.rocks");
            Assert.AreEqual("Beach", table.MapId);
            CollectionAssert.AreEqual(new[] { NodeDefaults.Rock }, table.Entries.Select(e => e.NodeId).ToArray());
            var rock = NodeDefaults.CreateAll().First(n => n.Id == NodeDefaults.Rock);
            Assert.AreEqual(ToolType.Pickaxe, rock.Tool);
            Assert.IsTrue(NodeLabels.NamesItself(rock), "and a rock has its name");
        }
    }
}
