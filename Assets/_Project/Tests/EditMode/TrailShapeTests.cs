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
    }
}
