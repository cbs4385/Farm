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
        public void TheFountainsBlock_IsARoundedBlockInTheMiddle_AndNothingElseBlocksTheMiddleOfTheSquare()
        {
            var block = VillageSquare.FountainSolidCells().ToList();
            Assert.AreEqual(5 + 3 + 3, block.Count, "five cells in the middle row, three above and three below");
            foreach (var c in block) Assert.IsTrue(Mathf.Abs(c.x - 25) <= 2 && Mathf.Abs(c.y - 17) <= 1, c.ToString());
            Assert.IsFalse(VillageSquare.FountainSolid(23, 16), "the corners are rounded off");
            Assert.IsFalse(VillageSquare.FountainSolid(25, 15) || VillageSquare.FountainSolid(25, 19));
            var solid = VillageSquare.SolidPropCells().ToList();
            Assert.AreEqual(block.Count, solid.Count(c => c.x >= 22 && c.x <= 28 && c.y >= 15 && c.y <= 19), "nothing else blocks the middle of the square");
            Assert.IsFalse(Resources.FindObjectsOfTypeAll<Sprite>().Any(sp => sp.name == "prop_gazebo"), "the gazebo's picture is gone");
        }

        [Test]
        public void TheFountainsBasin_FillsNinetyFivePercentOfTheInsideOfTheRing()
        {
            int Widest(string file, out int height)
            {
                var tex = new Texture2D(2, 2);
                tex.LoadImage(System.IO.File.ReadAllBytes("Assets/_Project/Art/Placeholders/" + file));
                var widest = 0; int top = int.MaxValue, bottom = -1;
                for (var y = 0; y < tex.height; y++)
                {
                    int left = -1, right = -1;
                    for (var x = 0; x < tex.width; x++) if (tex.GetPixel(x, y).a > 0.5f) { if (left < 0) left = x; right = x; }
                    if (left >= 0) { widest = Mathf.Max(widest, right - left + 1); top = Mathf.Min(top, y); bottom = Mathf.Max(bottom, y); }
                }
                height = bottom - top + 1;
                Object.DestroyImmediate(tex);
                return widest;
            }
            var ring = Widest("obj_plaza_ring.png", out var ringHeight);
            var fountain = Widest("obj_fountain.png", out _);
            // the ring is an ellipse band whose inside is 70% of its outside (tools/art/build_square_ring.py)
            var inside = ring * 0.70f;
            Assert.That(fountain / inside, Is.InRange(0.93f, 0.97f), $"the basin is {fountain} pixels wide and the inside of the ring {inside:0}");
        }

        [Test]
        public void TheGroundRoundTheFountainsBase_IsCobbled_EverywhereTheRingOfPavingLies()
        {
            // the ring is an ellipse about 9 x 7 cells round the middle cell (tools/art/build_square_ring.py); every cell it touches is cobbles
            for (var y = 10; y <= 25; y++)
                for (var x = 15; x <= 35; x++)
                {
                    var dx = (x + 0.5f - 25.5f) / 4.5f; var dy = (y + 0.5f - 17.5f) / 3.5f;
                    if (dx * dx + dy * dy <= 1f) Assert.AreEqual(VillageRoads.Cobble, VillageRoads.Ground(x, y), $"({x},{y}) is inside the ring and is not cobbled");
                }
            foreach (var c in VillageSquare.FountainSolidCells()) Assert.AreEqual(VillageRoads.Cobble, VillageRoads.Ground(c.x, c.y), $"the fountain's base at {c} stands on cobbles");
            foreach (var d in new[] { new Vector2Int(-3, 0), new Vector2Int(3, 0), new Vector2Int(0, -2), new Vector2Int(0, 2), new Vector2Int(-3, -1), new Vector2Int(3, 1) })
                Assert.AreEqual(VillageRoads.Cobble, VillageRoads.Ground(25 + d.x, 17 + d.y), $"cobbles beside the base at {d}");
        }

        [Test]
        public void TheFountain_HasSixDifferentFramesOfSpoutingWater()
        {
            var dir = "Assets/_Project/Art/Placeholders/";
            var frames = new List<Color32[]>();
            for (var i = 0; i < 6; i++)
            {
                var tex = new Texture2D(2, 2);
                tex.LoadImage(System.IO.File.ReadAllBytes(dir + (i == 0 ? "obj_fountain.png" : $"obj_fountain_{i}.png")));
                Assert.AreEqual(new Vector2Int(96, 112), new Vector2Int(tex.width, tex.height), $"frame {i}");
                frames.Add(tex.GetPixels32());
                Object.DestroyImmediate(tex);
            }
            for (var i = 0; i < 6; i++)
                for (var j = i + 1; j < 6; j++)
                    Assert.IsFalse(frames[i].SequenceEqual(frames[j]), $"frames {i} and {j} look the same");
        }

        [Test]
        public void TheSquare_IsCentredOnTheFountain_SoTheSameAmountOfCobblesShowsOnEverySide()
        {
            Assert.AreEqual(VillageSquare.Centre.x + 0.5f, (VillageSquare.X0 + VillageSquare.X1 + 1) / 2f, "centred left to right");
            Assert.AreEqual(VillageSquare.Centre.y + 0.5f, (VillageSquare.Y0 + VillageSquare.Y1 + 1) / 2f, "and top to bottom");
            // the ring of paving stones is 9 cells wide and 7 tall round the middle cell: count the rows of cobbles beyond it on each side, down and up the lane and along the road
            int Beyond(int dx, int dy)
            {
                var n = 0;
                var x = 25 + dx * 4; var y = 17 + dy * 3;           // the ring's outer edge
                for (x += dx, y += dy; VillageSquare.Cobbled(x, y); x += dx, y += dy) n++;
                return n;
            }
            Assert.AreEqual(Beyond(0, 1), Beyond(0, -1), "north and south");
            Assert.AreEqual(Beyond(1, 0), Beyond(-1, 0), "east and west");
            Assert.GreaterOrEqual(Beyond(0, 1), 1, "and some cobbles do show beyond the ring");
        }

        [Test]
        public void BothBenchesNorthOfTheSquare_StandWhollyOnGrass_NotOnADirtRoadOrPath()
        {
            foreach (var bench in new[] { VillageSquare.BenchWest, VillageSquare.BenchEast })
                for (var dx = 0; dx <= 1; dx++)          // the two cells of the bench
                {
                    var x = bench.x + dx;
                    Assert.IsNull(VillageRoads.Ground(x, bench.y), $"({x},{bench.y}) under the bench at {bench} is not grass");
                    Assert.IsFalse(NpcHomes.IsStreetPaving(x, bench.y), $"({x},{bench.y}) is a street");
                }
        }

        [Test]
        public void TheFourLampPostsOfTheSquare_AreTheSameDistanceFromTheFountain_AndNoOtherLampStandsInIt()
        {
            var lamps = VillageSquare.LampNorthX.Select(x => new Vector2Int(x, VillageRoads.RowAboveRoad(x)))
                .Concat(VillageSquare.LampSouthX.Select(x => new Vector2Int(x, VillageRoads.RowBelowRoad(x))))
                .Where(c => VillageSquare.Near(c.x, c.y, 1)).ToList();
            Assert.AreEqual(4, lamps.Count, "four lamp posts at the square: " + string.Join(" ", lamps));
            var distances = lamps.Select(c => Vector2.Distance(c, VillageSquare.Centre)).ToList();
            foreach (var d in distances) Assert.AreEqual(distances[0], d, 0.0001f, "equidistant from the middle");
            CollectionAssert.AreEquivalent(new[] { new Vector2Int(20, 15), new Vector2Int(20, 19), new Vector2Int(30, 15), new Vector2Int(30, 19) }, lamps);
        }

        [Test]
        public void TheBoardBenchesAndClockTower_StandWhereTheOwnerPutThem()
        {
            Assert.AreEqual(new Vector2Int(19, 13), VillageSquare.Board, "the board by the pond");
            Assert.AreEqual(new Vector2Int(20, 22), VillageSquare.BenchWest);
            Assert.AreEqual(new Vector2Int(29, 22), VillageSquare.BenchEast);
            Assert.AreEqual(new Vector2Int(33, 12), MapLayout.ClockTower, "the clock tower, south-east of the square");
            Assert.IsFalse(VillageSquare.InRect(MapLayout.ClockTower.x, MapLayout.ClockTower.y));
            Assert.IsFalse(MapLayout.VillagePond.Contains(VillageSquare.Board.x, VillageSquare.Board.y), "and not in the water");
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
