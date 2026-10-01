using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class FarmGridTests
    {
        CropDefinition _parsnip;      // 1,1,1,1 -> mature after 4 watered days, spring only
        CropDefinition _beans;        // 1,1,1,2 -> regrows every 3 days, spring + summer
        Dictionary<string, CropDefinition> _crops;

        [SetUp]
        public void SetUp()
        {
            _parsnip = CropDefinition.Create("parsnip", new[] { 1, 1, 1, 1 }, SeasonMask.Spring);
            _beans = CropDefinition.Create("beans", new[] { 1, 1, 1, 2 }, SeasonMask.Spring | SeasonMask.Summer, regrowDays: 3);
            _crops = new Dictionary<string, CropDefinition> { { "parsnip", _parsnip }, { "beans", _beans } };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parsnip);
            Object.DestroyImmediate(_beans);
        }

        CropDefinition Lookup(string id) => _crops.TryGetValue(id, out var c) ? c : null;

        static void WaterAndSleep(FarmGrid grid, System.Func<string, CropDefinition> lookup, Season season = Season.Spring)
        {
            foreach (var t in grid.Tiles) grid.Water(t.X, t.Y);
            grid.AdvanceDay(season, false, lookup);
        }

        [Test]
        public void Till_OnlyOnce()
        {
            var grid = new FarmGrid();
            Assert.IsTrue(grid.Till(2, 3));
            Assert.IsFalse(grid.Till(2, 3));
            Assert.IsTrue(grid.IsTilled(2, 3));
        }

        [Test]
        public void Water_RequiresTilledSoil_AndOnlyOnce()
        {
            var grid = new FarmGrid();
            Assert.IsFalse(grid.Water(0, 0));
            grid.Till(0, 0);
            Assert.IsTrue(grid.Water(0, 0));
            Assert.IsFalse(grid.Water(0, 0));
        }

        [Test]
        public void Plant_RequiresTilledEmptyTile_AndCorrectSeason()
        {
            var grid = new FarmGrid();
            Assert.IsFalse(grid.Plant(0, 0, _parsnip, Season.Spring));
            grid.Till(0, 0);
            Assert.IsFalse(grid.Plant(0, 0, _parsnip, Season.Summer));
            Assert.IsTrue(grid.Plant(0, 0, _parsnip, Season.Spring));
            Assert.IsFalse(grid.Plant(0, 0, _parsnip, Season.Spring));
        }

        [Test]
        public void Crop_MaturesAfterFourWateredDays()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _parsnip, Season.Spring);

            for (var day = 1; day <= 3; day++)
            {
                WaterAndSleep(grid, Lookup);
                Assert.IsFalse(grid.IsMature(0, 0, Lookup), $"day {day}");
            }
            WaterAndSleep(grid, Lookup);
            Assert.IsTrue(grid.IsMature(0, 0, Lookup));
        }

        [Test]
        public void UnwateredDays_DoNotAdvanceGrowth()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _parsnip, Season.Spring);
            for (var i = 0; i < 10; i++) grid.AdvanceDay(Season.Spring, false, Lookup);
            grid.TryGetTile(0, 0, out var tile);
            Assert.AreEqual(0, tile.Crop.Stage);
        }

        [Test]
        public void Rain_GrowsCropsWithoutWatering()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _parsnip, Season.Spring);
            for (var i = 0; i < 4; i++) grid.AdvanceDay(Season.Spring, true, Lookup);
            Assert.IsTrue(grid.IsMature(0, 0, Lookup));
        }

        [Test]
        public void AdvanceDay_ResetsWatering()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Water(0, 0);
            grid.AdvanceDay(Season.Spring, false, Lookup);
            grid.TryGetTile(0, 0, out var tile);
            Assert.IsFalse(tile.Watered);
            grid.WaterAll();
            Assert.IsTrue(tile.Watered);
        }

        [Test]
        public void Harvest_RemovesNonRegrowingCrop()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _parsnip, Season.Spring);
            Assert.IsFalse(grid.TryHarvest(0, 0, Lookup, out _));
            for (var i = 0; i < 4; i++) WaterAndSleep(grid, Lookup);

            Assert.IsTrue(grid.TryHarvest(0, 0, Lookup, out var result));
            Assert.AreEqual("crop.parsnip", result.ItemId);
            grid.TryGetTile(0, 0, out var tile);
            Assert.IsNull(tile.Crop);
            Assert.IsTrue(grid.IsTilled(0, 0));
        }

        [Test]
        public void Harvest_RegrowingCrop_MaturesAgainAfterRegrowDays()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _beans, Season.Spring);
            for (var i = 0; i < 5; i++) WaterAndSleep(grid, Lookup);   // 1+1+1+2
            Assert.IsTrue(grid.IsMature(0, 0, Lookup));

            Assert.IsTrue(grid.TryHarvest(0, 0, Lookup, out _));
            Assert.IsFalse(grid.IsMature(0, 0, Lookup));
            WaterAndSleep(grid, Lookup);
            WaterAndSleep(grid, Lookup);
            Assert.IsFalse(grid.IsMature(0, 0, Lookup));
            WaterAndSleep(grid, Lookup);
            Assert.IsTrue(grid.IsMature(0, 0, Lookup));
        }

        [Test]
        public void SeasonChange_KillsOutOfSeasonCrops_KeepsInSeason()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Till(1, 0);
            grid.Plant(0, 0, _parsnip, Season.Spring);
            grid.Plant(1, 0, _beans, Season.Spring);

            var died = grid.AdvanceDay(Season.Summer, false, Lookup);

            Assert.AreEqual(1, died);
            grid.TryGetTile(0, 0, out var a);
            grid.TryGetTile(1, 0, out var b);
            Assert.IsNull(a.Crop);
            Assert.IsNotNull(b.Crop);
        }

        [Test]
        public void ListRoundTrip_PreservesTiles()
        {
            var grid = new FarmGrid();
            grid.Till(4, 5);
            grid.Plant(4, 5, _parsnip, Season.Spring);
            grid.Till(1, 1);
            grid.Water(1, 1);

            var copy = FarmGrid.FromTiles(grid.ToList());

            Assert.AreEqual(2, copy.Count);
            copy.TryGetTile(4, 5, out var t);
            Assert.AreEqual("parsnip", t.Crop.CropId);
            copy.TryGetTile(1, 1, out var w);
            Assert.IsTrue(w.Watered);
            Assert.AreEqual(2, copy.Tiles.Count());
        }
    }
}
