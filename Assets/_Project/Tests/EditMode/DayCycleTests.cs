using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class DayCycleTests
    {
        ItemDefinition _parsnip;
        CropDefinition _crop;
        GameState _state;
        GameClock _clock;
        Dictionary<string, FarmGrid> _grids;

        [SetUp]
        public void SetUp()
        {
            _parsnip = ItemDefinition.Create("crop.parsnip", ItemCategory.Crop, sellPrice: 35);
            _crop = CropDefinition.Create("parsnip", new[] { 1, 1 }, SeasonMask.Spring);
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 0);
            _clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 1000));
            _state.SetDate(_clock.Now);
            _grids = new Dictionary<string, FarmGrid> { { MapIds.Farm, new FarmGrid() } };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parsnip);
            Object.DestroyImmediate(_crop);
        }

        ItemDefinition Item(string id) => id == "crop.parsnip" ? _parsnip : null;
        CropDefinition Crop(string id) => id == "parsnip" ? _crop : null;

        DaySummary EndDay(bool passedOut = false) => DayCycle.EndDay(_state, _clock, _grids, Item, Crop, passedOut);

        [Test]
        public void ShippingBin_PaysOutWithQualityMultiplier_AndEmpties()
        {
            _state.ShippingBin.Add(new ItemStack("crop.parsnip", 4, 0));   // 4 * 35 = 140
            _state.ShippingBin.Add(new ItemStack("crop.parsnip", 2, 2));   // 2 * 52 = 104 (gold x1.5)
            var before = _state.Gold;

            var summary = EndDay();

            Assert.AreEqual(244, summary.Earnings);
            Assert.AreEqual(before + 244, _state.Gold);
            Assert.IsEmpty(_state.ShippingBin);
            Assert.AreEqual(2, summary.Shipped.Count);
        }

        [Test]
        public void Day_Advances_AndPlayerWakesInBed()
        {
            EndDay();
            Assert.AreEqual(new GameDateTime(1, Season.Spring, 11), _clock.Now);
            Assert.AreEqual(MapIds.FarmHouse, _state.CurrentMap);
            Assert.AreEqual(DayCycle.BedSpawn, _state.SpawnPoint);
            Assert.AreEqual(_clock.Now, _state.GetDate());
        }

        [Test]
        public void Sleeping_RestoresFullEnergy_PassingOutDoesNot()
        {
            _state.Energy = 10;
            EndDay();
            Assert.AreEqual(_state.MaxEnergy, _state.Energy);

            _state.Energy = 10;
            _state.Gold = 1000;
            var summary = EndDay(passedOut: true);
            Assert.AreEqual((int)(_state.MaxEnergy * 0.75f), _state.Energy);
            Assert.AreEqual(50, summary.PassOutGoldLoss);
            Assert.AreEqual(950, _state.Gold);
        }

        [Test]
        public void PassOutGoldLoss_IsCapped()
        {
            _state.Gold = 1_000_000;
            Assert.AreEqual(500, EndDay(true).PassOutGoldLoss);
        }

        [Test]
        public void WateredCrops_Grow_AndSeasonChangeKillsThem()
        {
            var grid = _grids[MapIds.Farm];
            grid.Till(0, 0);
            grid.Plant(0, 0, _crop, Season.Spring);
            grid.Water(0, 0);
            EndDay();
            grid.TryGetTile(0, 0, out var tile);
            Assert.AreEqual(1, tile.Crop.Stage);

            _clock.SetTime(new GameDateTime(1, Season.Spring, 28, 1000));
            _state.SetDate(_clock.Now);
            var summary = EndDay();
            Assert.AreEqual(1, summary.CropsDied);
            Assert.AreEqual(Season.Summer, _clock.Now.Season);
        }

        [Test]
        public void Weather_IsDeterministic_SunnyEarly_NeverRainsInWinter()
        {
            Assert.AreEqual(WeatherIds.Sunny, DayCycle.RollWeather(new GameDateTime(1, Season.Spring, 1)));
            var d = new GameDateTime(1, Season.Summer, 9);
            Assert.AreEqual(DayCycle.RollWeather(d), DayCycle.RollWeather(d));
            for (var day = 1; day <= 28; day++)
                CollectionAssert.DoesNotContain(new[] { WeatherIds.Rain, WeatherIds.Storm }, DayCycle.RollWeather(new GameDateTime(2, Season.Winter, day)));
        }

        [Test]
        public void Weather_RainsSometimes_ButNotMostly()
        {
            var rain = 0;
            for (var day = 1; day <= 28; day++)
                for (var s = 0; s < 3; s++)
                    if (DayCycle.RollWeather(new GameDateTime(3, (Season)s, day)) == WeatherIds.Rain) rain++;
            Assert.Greater(rain, 5);
            Assert.Less(rain, 40);
        }

        [Test]
        public void RainTomorrow_WateresAllTilledSoil()
        {
            var grid = _grids[MapIds.Farm];
            grid.Till(1, 1);
            // find a date whose NEXT day rains
            var date = new GameDateTime(1, Season.Summer, 1);
            while (WeatherRoller.Roll(date.StartOfNextDay(), WeatherCatalog.BuiltIn, _state.WorldSeed) != WeatherIds.Rain) date = date.StartOfNextDay();
            _clock.SetTime(date.WithMinuteOfDay(1000));
            _state.SetDate(_clock.Now);

            EndDay();

            Assert.AreEqual(WeatherIds.Rain, _state.Weather);
            grid.TryGetTile(1, 1, out var tile);
            Assert.IsTrue(tile.Watered);
        }
    }
}
