using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-15: "the paths in the forest are too regular and look like roads".
    public class TrailShapeTests
    {
        static (int x0, int x1)[] Forest => ForestTrees.Trail;

        [Test]
        public void TheForestTrail_KeepsItsEnds_SoTheSpawnsWarpsAndBramblesStillStandOnIt()
        {
            Assert.AreEqual(ForestTrees.Height, Forest.Length);
            for (var y = 0; y < 4; y++) Assert.AreEqual((ForestTrees.PathX0, ForestTrees.PathX1), Forest[y], $"row {y}");
            for (var y = ForestTrees.Height - 4; y < ForestTrees.Height; y++) Assert.AreEqual((ForestTrees.PathX0, ForestTrees.PathX1), Forest[y], $"row {y}");
        }

        [Test]
        public void TheForestTrail_IsOneWalkablePiece_InsideItsCorridor_AtLeastTwoCellsWide()
        {
            for (var y = 0; y < Forest.Length; y++)
            {
                var (a, b) = Forest[y];
                Assert.GreaterOrEqual(a, ForestTrees.TrailMinX, $"row {y}");
                Assert.LessOrEqual(b, ForestTrees.TrailMaxX, $"row {y}");
                Assert.GreaterOrEqual(b - a + 1, TrailShape.MinWidth, $"row {y}");
                if (y == 0) continue;
                var (pa, pb) = Forest[y - 1];
                Assert.IsTrue(a <= pb && b >= pa, $"rows {y - 1} and {y} share no column, so the trail would break");
            }
        }

        [Test]
        public void TheForestTrail_IsNotARoad_ItBendsAndItsWidthVaries()
        {
            var widths = Forest.Select(s => s.x1 - s.x0 + 1).Distinct().ToList();
            Assert.GreaterOrEqual(widths.Count, 3, "at least three different widths");
            var lefts = Forest.Select(s => s.x0).Distinct().Count();
            var rights = Forest.Select(s => s.x1).Distinct().Count();
            Assert.GreaterOrEqual(lefts, 3, "the left edge moves");
            Assert.GreaterOrEqual(rights, 3, "the right edge moves");
            var directions = 0;
            var last = 0;
            for (var y = 1; y < Forest.Length; y++)
            {
                var d = (Forest[y].x0 + Forest[y].x1).CompareTo(Forest[y - 1].x0 + Forest[y - 1].x1);
                if (d != 0 && d != last) { directions++; last = d; }
            }
            Assert.GreaterOrEqual(directions, 3, "it turns left and right along its route");
        }

        [Test]
        public void ADifferentSeed_GivesADifferentTrail_AndTheSameSeedTheSameTrail()
        {
            var a = TrailShape.Build(30, 18, 20, 16, 22, 1);
            var b = TrailShape.Build(30, 18, 20, 16, 22, 1);
            var c = TrailShape.Build(30, 18, 20, 16, 22, 2);
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreNotEqual(a, c);
        }

        [Test]
        public void TheThickets_NeverGrowOnTheTrail()
        {
            foreach (var (x, y) in ForestTrees.Cells())
                Assert.IsFalse(x >= Forest[y].x0 && x <= Forest[y].x1, $"a tree on the trail at {x},{y}");
        }

        [Test]
        public void TheEdge_FollowsNoGridLine_AndIsSolidAwayFromIt()
        {
            // a straight trail of cells 5..7 in every row: the edge between trail and forest at x = 5 would be the same on every row without the noise
            bool Trail(int x, int y) => x >= 5 && x <= 7;
            Assert.Greater(TrailEdges.Coverage(Trail, 6.5f, 10.5f, 3), TrailEdges.Threshold, "the middle of the trail");
            Assert.Less(TrailEdges.Coverage(Trail, 2.5f, 10.5f, 3), TrailEdges.Threshold, "well into the forest");
            Assert.Less(TrailEdges.Coverage(Trail, 9.5f, 10.5f, 3), TrailEdges.Threshold, "well into the forest on the other side");

            var crossings = new System.Collections.Generic.HashSet<int>();
            for (var y = 0; y < 40; y++)
            {
                for (var px = 0; px < 32; px++)
                {
                    var x = 4f + px / 16f;
                    if (TrailEdges.Coverage(Trail, x, y + 0.5f, 3) >= TrailEdges.Threshold) { crossings.Add(px); break; }
                }
            }
            Assert.GreaterOrEqual(crossings.Count, 4, "the left edge does not stay on one line");
            Assert.IsFalse(crossings.All(c => c == 16), "the edge is not the cell border");
        }

        // ---- the farm's path: one cell wide, and it bends ---------------------------------------------------------------

        [Test]
        public void TheFarmPath_IsOneCellWide_OnePiece_AndStaysClearOfTheHouseTheBinAndTheMailbox()
        {
            var cells = FarmRoad.Cells.ToHashSet();
            var doorX = MapLayout.FarmHouseDoorX;
            Assert.IsTrue(cells.Contains((doorX, MapLayout.FarmRoadY)), "the corner by the door's path");
            Assert.IsTrue(cells.Contains((doorX, FarmBuildings.HouseY0 - 1)), "the cell in front of the farmhouse door");
            Assert.IsTrue(cells.Contains((MapLayout.FarmExitX, MapLayout.FarmRoadY)), "the way out to the village");
            Assert.IsTrue(cells.Contains((MapLayout.FarmArriveX, MapLayout.FarmRoadY)), "where villagers arrive");
            Assert.IsFalse(cells.Contains((doorX, FarmBuildings.HouseY0)), "the door cell is the floor tile");

            // one cell wide: no 2 x 2 block, and no row or column of the path (apart from a step sideways) is wider than two cells
            foreach (var (x, y) in cells)
                Assert.IsFalse(cells.Contains((x + 1, y)) && cells.Contains((x, y + 1)) && cells.Contains((x + 1, y + 1)), $"a 2 x 2 block at {x},{y}: the path is wider than one cell");
            foreach (var column in FarmRoad.Cells.Where(c => c.x > doorX).GroupBy(c => c.x))
                Assert.LessOrEqual(column.Count(), 2, $"column {column.Key}");

            // one piece
            var seen = new System.Collections.Generic.HashSet<(int, int)> { (MapLayout.FarmExitX, MapLayout.FarmRoadY) };
            var queue = new System.Collections.Generic.Queue<(int x, int y)>(seen);
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                foreach (var n in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) })
                    if (cells.Contains(n) && seen.Add(n)) queue.Enqueue(n);
            }
            Assert.AreEqual(cells.Count, seen.Count, "the path breaks into pieces");

            foreach (var (x, y) in cells)
            {
                Assert.IsFalse(x >= 73 && x <= 74 && y >= 17 && y <= 18, $"the shipping bin stands on the path at {x},{y}");
                Assert.IsFalse(x == 77 && y == 19, "the mailbox stands on the path");
                Assert.IsFalse(x >= FarmBuildings.HouseX0 && x <= FarmBuildings.HouseX1 && y >= FarmBuildings.HouseY0 && y <= FarmBuildings.HouseY1, $"the house stands on the path at {x},{y}");
            }
        }

        [Test]
        public void TheFarmPath_Bends_ItIsNotTwoStraightLines()
        {
            Assert.IsTrue(FarmRoad.South.Any(o => o != 0), "the way to the door bends");
            Assert.IsTrue(FarmRoad.East.Any(o => o != 0), "the road to the village bends");
            for (var i = 1; i < FarmRoad.East.Length; i++) Assert.LessOrEqual(System.Math.Abs(FarmRoad.East[i] - FarmRoad.East[i - 1]), 1);
        }

        [Test]
        public void TheEdges_AlsoCoverTheVillagesRoadsAndTheGrass()
        {
            Assert.IsTrue(TrailEdges.IsTrail("tile_cobble"));
            Assert.IsTrue(TrailEdges.IsTrail("tile_path"));
            Assert.IsTrue(TrailEdges.IsWood("tile_grass"));
            Assert.IsTrue(TrailEdges.IsWood("tile_forest"));
            Assert.IsFalse(TrailEdges.IsTrail("tile_grass"));
        }
    }
}
