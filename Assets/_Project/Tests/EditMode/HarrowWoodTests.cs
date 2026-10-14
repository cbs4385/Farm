using System.Collections.Generic;
using System.Linq;
using Farm.Editor;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    public class HarrowWoodTests
    {
        // Playtest report (2026-10-14): an object in Harrow Wood was "stuck behind a row of trees I cannot walk around or cut down".
        [Test]
        public void EveryCarvedStone_CanBeReachedOnFootFromTheWayIn()
        {
            var solid = new HashSet<(int, int)>(MapBuilder.WoodsTrees());
            foreach (var stone in MapBuilder.WoodsStones) solid.Add(stone);
            var seen = new HashSet<(int, int)> { (19, 2) };
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((19, 2));
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                foreach (var next in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) })
                    if (next.Item1 >= 2 && next.Item1 < 38 && next.Item2 >= 2 && next.Item2 < 28 && !solid.Contains(next) && seen.Add(next)) queue.Enqueue(next);
            }
            foreach (var (x, y) in MapBuilder.WoodsStones)
                Assert.IsTrue(new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) }.Any(seen.Contains), $"the stone at {x},{y} has a cell beside it that can be walked to");
        }

        [Test]
        public void TheRelicChest_StandsOnAFloorCellThatCanBeReached_WithoutCuttingTheFloorInTwo_AndMovesWithTheWorld()
        {
            var cells = new HashSet<(int, int)>();
            for (var seed = 1; seed <= 40; seed++)
            {
                var f = MineGenerator.Generate(seed * 977, MythosRelics.Floor);
                var cell = MythosRelics.ChestCell(f, seed);
                Assert.IsTrue(cell.HasValue, "seed " + seed);
                var (x, y) = cell.Value;
                Assert.IsFalse(f.IsWall(x, y));
                Assert.AreNotEqual(f.Up, cell.Value);
                Assert.AreNotEqual(f.Elevator, cell.Value);
                Assert.IsFalse(f.Nodes.Any(n => n.X == x && n.Y == y), "no ore under it");
                Assert.AreEqual(cell, MythosRelics.ChestCell(f, seed), "the same world and floor give the same place");
                cells.Add(cell.Value);
            }
            Assert.Greater(cells.Count, 10, "it is not always in the same place");
        }

        [Test]
        public void TheRelicFlags_AreTheOnesTheAltarAlwaysUsed()
        {
            CollectionAssert.AreEqual(new[] { "mythos.relic.found.1", "mythos.relic.found.2", "mythos.relic.found.3" }, Enumerable.Range(0, 3).Select(MythosRelics.FlagOf));
        }
    }
}
