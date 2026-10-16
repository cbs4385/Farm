using System.Collections.Generic;
using System.Linq;
using Farm.Editor;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-16 on the village: "the village square needs reworking: it does not read nor look like a village square. Use a gazebo as a center piece and build a
    // realistic cozy village square around it... soften the intersection of the village square and the dirt paths". And: copses of trees, no brown patches, brown roads, no
    // little decorations that are only obstacles.
    public class VillageSquareTests
    {
        static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        static bool Walkable(int x, int y, ISet<Vector2Int> solid) =>
            x > 0 && y > 0 && x < MapLayout.VillageW - 1 && y < MapLayout.VillageH - 1 && !solid.Contains(new Vector2Int(x, y));

        static HashSet<Vector2Int> Reach(Vector2Int from, ISet<Vector2Int> solid, System.Func<int, int, bool> inside)
        {
            var seen = new HashSet<Vector2Int> { from };
            var queue = new Queue<Vector2Int>(seen);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var d in Directions)
                {
                    var n = c + d;
                    if (inside(n.x, n.y) && Walkable(n.x, n.y, solid) && seen.Add(n)) queue.Enqueue(n);
                }
            }
            return seen;
        }

        [Test]
        public void TheSquare_IsACobbledPlazaWithRoundedCorners_AndTheGazeboStandsInItsMiddle()
        {
            for (var y = VillageSquare.Y0; y <= VillageSquare.Y1; y++)
                for (var x = VillageSquare.X0; x <= VillageSquare.X1; x++)
                {
                    var corner = (x == VillageSquare.X0 || x == VillageSquare.X1) && (y == VillageSquare.Y0 || y == VillageSquare.Y1);
                    Assert.AreEqual(corner ? null : VillageRoads.Cobble, VillageRoads.Ground(x, y), $"({x},{y})");
                    Assert.AreEqual(!corner, VillageSquare.Cobbled(x, y));
                }
            Assert.GreaterOrEqual(VillageSquare.X1 - VillageSquare.X0 + 1, 11, "a square is wide");
            Assert.GreaterOrEqual(VillageSquare.Y1 - VillageSquare.Y0 + 1, 8);
            Assert.AreEqual(25, (VillageSquare.GazeboX0 + VillageSquare.GazeboX1) / 2, "on the lane's axis");
            Assert.IsTrue(VillageRoads.OnLane(25, 17) && VillageRoads.OnRoad(25, 17), "where the road crosses the lane");
            for (var y = VillageSquare.GazeboY0; y <= VillageSquare.GazeboY1; y++)
                for (var x = VillageSquare.GazeboX0; x <= VillageSquare.GazeboX1; x++) Assert.IsTrue(VillageSquare.Cobbled(x, y), "the gazebo stands on the paving");
        }

        [Test]
        public void TheGazebo_IsSolidAtTheBackAndTheRails_ButItsFrontAndMiddleCanBeWalked()
        {
            var solid = VillageSquare.GazeboSolidCells().ToList();
            Assert.AreEqual(5 + 2, solid.Count, "the back row and a rail either side of the way in");
            for (var x = VillageSquare.GazeboX0; x <= VillageSquare.GazeboX1; x++)
            {
                Assert.IsTrue(VillageSquare.GazeboSolid(x, VillageSquare.GazeboY1), "the back");
                Assert.IsFalse(VillageSquare.GazeboSolid(x, VillageSquare.GazeboY0), "the way in along the front");
            }
            for (var x = VillageSquare.GazeboX0 + 1; x < VillageSquare.GazeboX1; x++) Assert.IsFalse(VillageSquare.GazeboSolid(x, VillageSquare.GazeboY0 + 1), "the inside");
            Assert.IsTrue(VillageSquare.GazeboSolid(VillageSquare.GazeboX0, VillageSquare.GazeboY0 + 1));
            Assert.IsTrue(VillageSquare.GazeboSolid(VillageSquare.GazeboX1, VillageSquare.GazeboY0 + 1));
            Assert.IsFalse(VillageSquare.GazeboSolid(0, 0));
        }

        [Test]
        public void TheRoadAndTheLane_RunInToTheSquareFromAllFourSides_AndTheYWalkRoundTheGazebo()
        {
            var solid = new HashSet<Vector2Int>(VillageSquare.SolidPropCells());
            var everything = Reach(new Vector2Int(2, 17), solid, (x, y) => true);
            foreach (var entry in new[] { new Vector2Int(VillageSquare.X0 - 2, 17), new Vector2Int(VillageSquare.X1 + 2, 17), new Vector2Int(25, VillageSquare.Y0 - 2), new Vector2Int(25, VillageSquare.Y1 + 2) })
            {
                Assert.IsNotNull(VillageRoads.Ground(entry.x, entry.y), $"{entry} is road");
                Assert.IsTrue(everything.Contains(entry), $"{entry} can be reached from the way in from the farm");
            }
            // Inside the plaza alone, west to east and south to north can be walked round the gazebo.
            bool InPlaza(int x, int y) => VillageSquare.Cobbled(x, y);
            var from = Reach(new Vector2Int(VillageSquare.X0, 17), solid, InPlaza);
            Assert.IsTrue(from.Contains(new Vector2Int(VillageSquare.X1, 17)), "west to east");
            Assert.IsTrue(from.Contains(new Vector2Int(25, VillageSquare.Y1)), "to the north");
            Assert.IsTrue(from.Contains(new Vector2Int(25, VillageSquare.Y0)), "to the south");
            Assert.IsTrue(from.Contains(new Vector2Int(25, VillageSquare.GazeboY0 + 1)), "and into the gazebo by its front");
        }

        [Test]
        public void WhereARoadMeetsTheSquare_TheWayWidensByACellEachSide_SoThereIsNoHardCorner()
        {
            foreach (var y in new[] { 15, 16, 17, 18, 19 })
            {
                Assert.AreEqual(VillageRoads.Dirt, VillageRoads.Ground(VillageSquare.X0 - 1, y), $"west, row {y}");
                Assert.AreEqual(VillageRoads.Dirt, VillageRoads.Ground(VillageSquare.X1 + 1, y), $"east, row {y}");
            }
            foreach (var x in new[] { 23, 24, 25, 26, 27 })
            {
                Assert.AreEqual(VillageRoads.Dirt, VillageRoads.Ground(x, VillageSquare.Y0 - 1), $"south, column {x}");
                Assert.AreEqual(VillageRoads.Dirt, VillageRoads.Ground(x, VillageSquare.Y1 + 1), $"north, column {x}");
            }
            Assert.IsNull(VillageRoads.Ground(VillageSquare.X0 - 1, 13), "and no further");
            Assert.IsTrue(VillageSquare.Flare(19, 15) && VillageSquare.Flare(19, 19) && !VillageSquare.Flare(19, 14));
        }

        [Test]
        public void TheRoadsOfTheVillage_AreTheBrownOfTheDirtTrail_NotTheSandOfThePath_AndCannotBeTilled()
        {
            Assert.AreEqual("tile_road", VillageRoads.Dirt);
            Assert.AreNotEqual("tile_dirt", VillageRoads.Dirt, "tile_dirt can be tilled in the village; a road must not be");
            Assert.IsTrue(TrailEdges.IsTrail("tile_road") && TrailEdges.IsRoad("tile_road"), "and its edges are softened like the other trails'");
            var seen = new HashSet<string>();
            for (var x = 0; x < MapLayout.VillageW; x++)
                for (var y = 0; y < MapLayout.VillageH; y++)
                {
                    var g = VillageRoads.Ground(x, y);
                    if (g != null) seen.Add(g);
                }
            CollectionAssert.AreEquivalent(new[] { "tile_road", "tile_cobble" }, seen, "the village's roads are brown and its square cobbled, and there is no sandy path");
        }

        [Test]
        public void NoFestivalGatheringCell_IsOnAGazeboRailOrAnyThingThatStandsInTheSquareForGood()
        {
            var solid = new HashSet<Vector2Int>(VillageSquare.SolidPropCells());
            foreach (var kv in Festivals.Gathering)
                foreach (var (x, y, _) in kv.Value)
                    Assert.IsFalse(solid.Contains(new Vector2Int(x, y)), $"{kv.Key}: ({x},{y}) is under something that stands there");
        }

        [Test]
        public void TheSpringDance_IsRoundTheMaypole_AndTheMaypoleIsClearOfTheGazebo()
        {
            var maypole = FestivalGrounds.Maypole;
            Assert.IsFalse(VillageSquare.OnGazebo(maypole.x, maypole.y));
            Assert.Greater(maypole.x, VillageSquare.GazeboX1, "east of the gazebo");
            foreach (var (x, y, _) in Festivals.Gathering["spring"])
            {
                Assert.IsTrue(FestivalGrounds.InDanceRing(new Vector3(x, y), new Vector3(maypole.x, maypole.y)), $"({x},{y}) dances");
                Assert.AreNotEqual(maypole, new Vector2Int(x, y));
            }
        }

        [Test]
        public void TheFountainAndTheClockTower_FlankTheLaneOnTheNorthEdge()
        {
            Assert.AreEqual(VillageSquare.Y1, MapLayout.ClockTower.y);
            Assert.AreEqual(VillageSquare.Y1, MapLayout.Fountain.y);
            Assert.Less(MapLayout.Fountain.x + 1, MapRoadsLaneX0(), "west of the lane");
            Assert.Greater(MapLayout.ClockTower.x, MapRoadsLaneX0() + 2, "east of it");
            Assert.AreEqual(25 - (MapLayout.Fountain.x + 1), MapLayout.ClockTower.x - 25, "the same distance either side");
        }

        static int MapRoadsLaneX0() => VillageRoads.LaneX0;

        // ---- the trees ----

        [Test]
        public void TheVillagesTrees_StandInCopses_NotInRowsOrScatter_AndOffTheRoadsBuildingsAndSquare()
        {
            var trees = MapBuilder.VillageTreeCells();
            Assert.GreaterOrEqual(trees.Count, 80, "there are trees");
            var set = new HashSet<(int x, int y)>(trees);
            foreach (var (x, y) in trees)
            {
                Assert.IsNull(VillageRoads.Ground(x, y), $"({x},{y}) is on a road");
                Assert.IsFalse(NpcHomes.IsStreetPaving(x, y) || NpcHomes.InStreet(x, y), $"({x},{y}) is on a street");
                Assert.IsFalse(VillageSquare.Near(x, y, 3), $"({x},{y}) is in the square");
                Assert.IsFalse(MapLayout.VillagePond.Contains(x, y), $"({x},{y}) is in the pond");
                foreach (var s in VillageShops.All) Assert.IsFalse(s.Footprint.Contains(new Vector2Int(x, y)), $"({x},{y}) is under {s.Map}");
                foreach (var kv in Festivals.Gathering)
                    foreach (var c in kv.Value) Assert.IsFalse(c.x == x && c.y == y, $"({x},{y}) is a gathering cell");
            }

            // Copses: nearly every tree has a neighbour within two cells, and the trees fall into a modest number of clumps of several trees each.
            var withNeighbour = trees.Count(t => trees.Any(o => o != t && System.Math.Abs(o.x - t.x) <= 2 && System.Math.Abs(o.y - t.y) <= 2));
            Assert.GreaterOrEqual(withNeighbour / (float)trees.Count, 0.95f, "scattered single trees");
            var clumps = new List<int>();
            var seen = new HashSet<(int x, int y)>();
            foreach (var t in trees)
            {
                if (!seen.Add(t)) continue;
                var queue = new Queue<(int x, int y)>(); queue.Enqueue(t);
                var size = 0;
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue(); size++;
                    for (var dx = -2; dx <= 2; dx++)
                        for (var dy = -2; dy <= 2; dy++)
                            if (set.Contains((c.x + dx, c.y + dy)) && seen.Add((c.x + dx, c.y + dy))) queue.Enqueue((c.x + dx, c.y + dy));
                }
                clumps.Add(size);
            }
            Assert.GreaterOrEqual(clumps.Count(s => s >= 8), 5, "several real copses");
            Assert.GreaterOrEqual(clumps.Where(s => s >= 8).Sum() / (float)trees.Count, 0.8f, "nearly all the trees are in them");
            Assert.LessOrEqual(clumps.Count, 40, "not a hundred little patches");
        }

        [Test]
        public void NoTreesStandInADiagonalRow()
        {
            var trees = new HashSet<(int x, int y)>(MapBuilder.VillageTreeCells());
            // the old scatter put a tree every 17th cell along a diagonal: look for four trees in a straight diagonal run, each exactly two cells from the next, with no tree between
            var runs = 0;
            foreach (var (x, y) in trees)
                foreach (var (dx, dy) in new[] { (2, 2), (2, -2), (3, 3), (3, -3) })
                {
                    var line = Enumerable.Range(0, 4).All(i => trees.Contains((x + dx * i, y + dy * i)));
                    var gaps = Enumerable.Range(0, 3).All(i => !trees.Contains((x + dx * i + dx / 2, y + dy * i + dy / 2)));
                    if (line && gaps) runs++;
                }
            Assert.AreEqual(0, runs, "four trees evenly spaced on a diagonal");
        }
    }
}
