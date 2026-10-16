using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-16: the village's paths were still very straight and cobbled, not the winding dirt roads of the other maps. They wind now, stay one connected
    // network, and keep clear of the buildings; only the square is cobbled and straight.
    public class VillageRoadsTests
    {
        static bool Surface(int x, int y) =>
            x > 0 && y > 0 && x < MapLayout.VillageW - 1 && y < MapLayout.VillageH - 1 && (VillageRoads.Ground(x, y) != null || NpcHomes.IsStreetPaving(x, y));

        [Test]
        public void TheRoad_Bends_AndNeverMovesMoreThanOneRowBetweenColumns()
        {
            Assert.AreEqual(MapLayout.VillageW, VillageRoads.Road.Length);
            Assert.GreaterOrEqual(VillageRoads.Road.Count(o => o != 0), 30, "most of the road outside the square should be off its old line");
            for (var x = 0; x < VillageRoads.Road.Length; x++)
            {
                Assert.GreaterOrEqual(VillageRoads.Road[x], VillageRoads.MinShift, $"column {x}");
                Assert.LessOrEqual(VillageRoads.Road[x], VillageRoads.MaxShift, $"column {x}");
                if (x > 0) Assert.LessOrEqual(Math.Abs(VillageRoads.Road[x] - VillageRoads.Road[x - 1]), 1, $"column {x}");
            }
        }

        [Test]
        public void TheLane_Bends_AndNeverMovesMoreThanOneColumnBetweenRows()
        {
            Assert.AreEqual(MapLayout.VillageH, VillageRoads.Lane.Length);
            Assert.GreaterOrEqual(VillageRoads.Lane.Count(o => o != 0), 10, "the lane should leave its old line in the stretches that are free");
            for (var y = 0; y < VillageRoads.Lane.Length; y++)
            {
                Assert.GreaterOrEqual(VillageRoads.Lane[y], VillageRoads.MinShift, $"row {y}");
                Assert.LessOrEqual(VillageRoads.Lane[y], VillageRoads.MaxShift, $"row {y}");
                if (y > 0) Assert.LessOrEqual(Math.Abs(VillageRoads.Lane[y] - VillageRoads.Lane[y - 1]), 1, $"row {y}");
            }
        }

        [Test]
        public void TheSquare_AndTheWaysOut_KeepTheirCourse()
        {
            for (var x = 0; x <= 4; x++) Assert.AreEqual(0, VillageRoads.RoadShift(x), $"road, column {x}");
            for (var x = VillageRoads.PlazaX0; x <= VillageRoads.PlazaX1; x++) Assert.AreEqual(0, VillageRoads.RoadShift(x), $"road, column {x}");
            foreach (var y in new[] { 0, 2, 3, 12, 17, 21, 26, MapLayout.VillageH - 4, MapLayout.VillageH - 1 })
                Assert.AreEqual(0, VillageRoads.LaneShift(y), $"lane, row {y}");
            Assert.IsTrue(VillageRoads.OnRoad(0, 17));
            Assert.IsTrue(VillageRoads.OnRoad(2, 17));
            Assert.IsTrue(VillageRoads.OnLane(MapLayout.VillageLaneX, 0));
            Assert.IsTrue(VillageRoads.OnLane(MapLayout.VillageLaneX, MapLayout.VillageH - 1));
        }

        [Test]
        public void OnlyTheSquareIsCobbled_TheRestIsDirtLikeTheOtherMaps()
        {
            Assert.AreEqual(VillageRoads.Cobble, VillageRoads.Ground(30, 17));
            Assert.AreEqual(VillageRoads.Cobble, VillageRoads.Ground(25, 18));
            var cobbled = 0;
            for (var x = 0; x < MapLayout.VillageW; x++)
                for (var y = 0; y < MapLayout.VillageH; y++)
                {
                    var ground = VillageRoads.Ground(x, y);
                    if (ground == VillageRoads.Cobble) { cobbled++; Assert.IsTrue(VillageRoads.OnPlaza(x, y), $"({x},{y}) is cobbled outside the square"); }
                }
            Assert.Greater(cobbled, 0);
            Assert.AreEqual(VillageRoads.Dirt, VillageRoads.Ground(50, 17 + VillageRoads.RoadShift(50)), "east of the square the road is dirt");
        }

        [Test]
        public void TheRoadsAndPaths_StayOffTheBuildingsAndThePond()
        {
            var shops = VillageShops.All.Select(s => s.Footprint).ToList();
            var homes = NpcHomes.All.Select(h => new RectInt(h.X0, h.Y0, h.X1 - h.X0 + 1, h.Y1 - h.Y0 + 1)).ToList();
            for (var x = 0; x < MapLayout.VillageW; x++)
                for (var y = 0; y < MapLayout.VillageH; y++)
                {
                    if (VillageRoads.Ground(x, y) == null) continue;
                    var cell = new Vector2Int(x, y);
                    foreach (var r in shops) Assert.IsFalse(r.Contains(cell), $"({x},{y}) is under a shop");
                    foreach (var r in homes) Assert.IsFalse(r.Contains(cell), $"({x},{y}) is under a cottage");
                    Assert.IsFalse(MapLayout.VillagePond.Contains(x, y), $"({x},{y}) is in the pond");
                }
        }

        [Test]
        public void EverythingIsJoined_TheWaysOut_TheShopDoors_TheStreetsAndTheHomes()
        {
            var seen = new HashSet<Vector2Int> { new Vector2Int(2, 17) };
            var queue = new Queue<Vector2Int>(seen);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var n = c + d;
                    if (Surface(n.x, n.y) && seen.Add(n)) queue.Enqueue(n);
                }
            }
            Assert.IsTrue(seen.Contains(new Vector2Int(MapLayout.VillageLaneX, 1)), "the lane to the beach");
            Assert.IsTrue(seen.Contains(new Vector2Int(MapLayout.VillageLaneX, MapLayout.VillageH - 2)), "the lane to the forest");
            foreach (var shop in VillageShops.All)
                Assert.IsTrue(seen.Contains(new Vector2Int(shop.DoorX, shop.OutsideY)), $"the door of {shop.Map} is joined to the road");
            foreach (var street in NpcHomes.Streets)
                Assert.IsTrue(seen.Contains(new Vector2Int(street.LaneX + 1, street.RowY + 3)), $"the street at x {street.LaneX} is joined");
            foreach (var home in NpcHomes.All)
                Assert.IsTrue(seen.Contains(new Vector2Int(home.DoorX, home.OutsideY)), $"the door of {home.Map} is joined");
        }

        [Test]
        public void TheShopPaths_Wind_ButEndOnTheDoorstep()
        {
            var winding = 0;
            foreach (var shop in VillageShops.All)
            {
                var cells = VillageRoads.ShopPath(shop);
                CollectionAssert.Contains(cells, (shop.DoorX, shop.OutsideY), $"{shop.Map}: the cell in front of the door");
                CollectionAssert.Contains(cells, (shop.DoorX, VillageRoads.RowAboveRoad(shop.DoorX)), $"{shop.Map}: the cell by the road");
                if (cells.Any(c => c.x != shop.DoorX)) winding++;
            }
            Assert.GreaterOrEqual(winding, 1, "at least one shop's path leaves its straight line");
        }
    }
}
