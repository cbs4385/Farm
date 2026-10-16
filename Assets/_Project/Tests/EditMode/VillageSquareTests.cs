using System.Collections.Generic;
using System.Linq;
using Farm.Editor;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-16 on the village: "the village square needs reworking: it does not read nor look like a village square. Use a gazebo as a center piece ... (a day later: the gazebo is difficult to navigate around, remove it and move the fountain to the center) and build a
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
        public void TheSquare_IsACobbledPlazaWithRoundedCorners_AndTheFountainStandsInItsMiddle()
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
            Assert.AreEqual(VillageSquare.Centre, MapLayout.Fountain, "the fountain is the centrepiece");
            Assert.AreEqual(25, VillageSquare.Centre.x, "on the lane's axis");
            Assert.IsTrue(VillageRoads.OnLane(VillageSquare.Centre.x, VillageSquare.Centre.y) && VillageRoads.OnRoad(VillageSquare.Centre.x, VillageSquare.Centre.y), "where the road crosses the lane");
            Assert.IsTrue(VillageSquare.Cobbled(VillageSquare.Centre.x, VillageSquare.Centre.y), "on the paving");
        }

        [Test]
        public void ThereIsNoGazebo_AndOnlyTheFountainBlocksTheMiddleOfTheSquare()
        {
            var solid = VillageSquare.SolidPropCells().ToList();
            CollectionAssert.Contains(solid, MapLayout.Fountain);
            // the seven columns and four rows round the middle: the fountain's own cell is the only thing that blocks
            Assert.AreEqual(1, solid.Count(c => c.x >= 22 && c.x <= 28 && c.y >= 15 && c.y <= 18), "one cell");
        }

        [Test]
        public void TheRoadAndTheLane_RunInToTheSquareFromAllFourSides_AndTheyWalkRoundTheFountain()
        {
            var solid = new HashSet<Vector2Int>(VillageSquare.SolidPropCells());
            bool InPlaza(int x, int y) => x >= VillageSquare.X0 - 1 && x <= VillageSquare.X1 + 1 && y >= VillageSquare.Y0 - 1 && y <= VillageSquare.Y1 + 1;          // the square and the open ground one cell round it (the corners are grass)
            var from = Reach(new Vector2Int(VillageSquare.X0, 17), solid, InPlaza);
            Assert.IsTrue(from.Contains(new Vector2Int(VillageSquare.X1, 17)), "west to east");
            Assert.IsTrue(from.Contains(new Vector2Int(25, VillageSquare.Y1)), "to the north");
            Assert.IsTrue(from.Contains(new Vector2Int(25, VillageSquare.Y0)), "to the south");
            // and every walkable cell of the plaza can be reached from every other: nothing is walled off by the furniture
            var all = new List<Vector2Int>();
            for (var y = VillageSquare.Y0; y <= VillageSquare.Y1; y++)
                for (var x = VillageSquare.X0; x <= VillageSquare.X1; x++)
                    if (VillageSquare.Cobbled(x, y) && !solid.Contains(new Vector2Int(x, y))) all.Add(new Vector2Int(x, y));
            var unreachable = all.Where(c => !from.Contains(c)).ToList();
            Assert.IsEmpty(unreachable, "cells of the plaza that cannot be walked to: " + string.Join(" ", unreachable));
            foreach (var entry in new[] { new Vector2Int(VillageSquare.X0 - 2, 17), new Vector2Int(VillageSquare.X1 + 2, 17), new Vector2Int(25, VillageSquare.Y0 - 2), new Vector2Int(25, VillageSquare.Y1 + 2) })
                Assert.IsNotNull(VillageRoads.Ground(entry.x, entry.y), $"{entry} is road");
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
        public void NoFestivalGatheringCell_IsOnAnythingThatStandsInTheSquareForGood()
        {
            var solid = new HashSet<Vector2Int>(VillageSquare.SolidPropCells());
            foreach (var kv in Festivals.Gathering)
                foreach (var (x, y, _) in kv.Value)
                    Assert.IsFalse(solid.Contains(new Vector2Int(x, y)), $"{kv.Key}: ({x},{y}) is under something that stands there");
        }

        [Test]
        public void TheSpringDance_IsRoundTheMaypole_AndTheMaypoleIsEastOfTheFountain()
        {
            var maypole = FestivalGrounds.Maypole;
            Assert.AreNotEqual(MapLayout.Fountain, maypole);
            Assert.Greater(maypole.x, MapLayout.Fountain.x + 1, "east of the fountain, with room to walk between");
            foreach (var (x, y, _) in Festivals.Gathering["spring"])
            {
                Assert.IsTrue(FestivalGrounds.InDanceRing(new Vector3(x, y), new Vector3(maypole.x, maypole.y)), $"({x},{y}) dances");
                Assert.AreNotEqual(maypole, new Vector2Int(x, y));
            }
        }

        [Test]
        public void TheClockTower_StandsOnTheNorthEdge_EastOfTheLane()
        {
            Assert.AreEqual(VillageSquare.Y1, MapLayout.ClockTower.y);
            Assert.Greater(MapLayout.ClockTower.x, VillageRoads.LaneX0 + 2, "east of the lane");
        }

        [Test]
        public void EveryTrailTile_IsTheBrownOfTileDirt_TheWhiteTrailIsGoneFromAllMaps()
        {
            var dir = "Assets/_Project/Art/Placeholders/";
            var brown = System.IO.File.ReadAllBytes(dir + "tile_dirt.png");
            foreach (var tile in new[] { "tile_path", "tile_road" })
                CollectionAssert.AreEqual(brown, System.IO.File.ReadAllBytes(dir + tile + ".png"), tile + " is the same picture as tile_dirt");
        }

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
