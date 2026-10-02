using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-030: weather is data (WeatherDefinition), rolled from a weighted, seeded table that modules can reshape.
    public class WeatherTests
    {
        GameState _state;
        GameClock _clock;
        Dictionary<string, FarmGrid> _grids;
        GameHooks _hooks;
        readonly List<Object> _objects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 7);
            _clock = new GameClock(new GameDateTime(1, Season.Summer, 10, 1000));
            _state.SetDate(_clock.Now);
            _grids = new Dictionary<string, FarmGrid> { { MapIds.Farm, new FarmGrid() } };
            _hooks = new GameHooks();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _objects) Object.DestroyImmediate(o);
        }

        DaySummary EndDay(WeatherCatalog catalog = null) =>
            DayCycle.EndDay(_state, _clock, _grids, id => null, id => null, false, _hooks, catalog);

        static IEnumerable<GameDateTime> Days(int year, Season season)
        {
            for (var day = 1; day <= GameDateTime.DaysPerSeason; day++) yield return new GameDateTime(year, season, day);
        }

        static string Roll(GameDateTime date, int seed = 1, GameHooks hooks = null) =>
            WeatherRoller.Roll(date, WeatherCatalog.BuiltIn, seed, hooks);

        // ---- the roll ------------------------------------------------------------------------------------------

        [Test]
        public void Roll_IsDeterministicForASeed_AndDiffersBetweenSeeds()
        {
            var date = new GameDateTime(1, Season.Summer, 9);
            Assert.AreEqual(Roll(date, 5), Roll(date, 5));

            var differing = Days(2, Season.Spring).Count(d => Roll(d, 1) != Roll(d, 2));
            Assert.Greater(differing, 3, "different saves should get different weather");
        }

        [Test]
        public void FirstDays_AreAlwaysSunny()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                Assert.AreEqual(WeatherIds.Sunny, Roll(new GameDateTime(1, Season.Spring, 1), seed));
                Assert.AreEqual(WeatherIds.Sunny, Roll(new GameDateTime(1, Season.Spring, 2), seed));
            }
        }

        [Test]
        public void SeasonsOnlyRollTheirOwnWeather()
        {
            foreach (var d in Days(2, Season.Winter).Concat(Days(3, Season.Winter)))
                CollectionAssert.DoesNotContain(new[] { WeatherIds.Rain, WeatherIds.Storm }, Roll(d), "no rain in winter");
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall })
                foreach (var d in Days(2, season))
                    Assert.AreNotEqual(WeatherIds.Snow, Roll(d), "snow is for winter only");
        }

        [Test]
        public void EveryDefinedWeather_TurnsUpInItsSeasonOverTheYears()
        {
            var seen = new HashSet<string>();
            for (var year = 2; year < 12; year++)
                foreach (Season season in System.Enum.GetValues(typeof(Season)))
                    foreach (var d in Days(year, season)) seen.Add(Roll(d));
            foreach (var id in WeatherCatalog.BuiltIn.Ids) Assert.Contains(id, seen.ToList(), id);
        }

        [Test]
        public void SunnyIsTheMostCommonWeather_AndRainIsNotMostOfTheTime()
        {
            var counts = new Dictionary<string, int>();
            for (var year = 2; year < 12; year++)
                foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall })
                    foreach (var d in Days(year, season))
                    {
                        var w = Roll(d);
                        counts[w] = counts.TryGetValue(w, out var n) ? n + 1 : 1;
                    }
            var total = counts.Values.Sum();
            Assert.Greater(counts[WeatherIds.Sunny], total * 0.45f);
            Assert.Less(counts[WeatherIds.Rain], total * 0.35f);
        }

        // ---- weight modifiers ----------------------------------------------------------------------------------

        sealed class NoSunshine : IWeatherWeightModifier
        {
            public int Order => 0;
            public void Adjust(GameDateTime date, WeatherWeights weights, GameState state) => weights.Set(WeatherIds.Sunny, 0f);
        }

        sealed class AddFog : IWeatherWeightModifier
        {
            public int Order => 1;
            public void Adjust(GameDateTime date, WeatherWeights weights, GameState state) => weights.Add("fog", 1000f);
        }

        sealed class Boom : IWeatherWeightModifier
        {
            public int Order => 0;
            public void Adjust(GameDateTime date, WeatherWeights weights, GameState state) => throw new System.InvalidOperationException("x");
        }

        [Test]
        public void WeightModifiers_ReshapeTheOdds()
        {
            _hooks.AddWeatherWeightModifier(new NoSunshine());
            foreach (var d in Days(2, Season.Summer))
                Assert.AreNotEqual(WeatherIds.Sunny, Roll(d, 1, _hooks));
        }

        [Test]
        public void WeightModifiers_CanAddAWeatherOfTheirOwn()
        {
            _hooks.AddWeatherWeightModifier(new AddFog());
            var fog = Days(2, Season.Summer).Count(d => Roll(d, 1, _hooks) == "fog");
            Assert.Greater(fog, 20, "a weight of 1000 should dominate");
        }

        [Test]
        public void AFailingWeightModifier_IsSkipped()
        {
            _hooks.AddWeatherWeightModifier(new Boom());
            LogAssert.Expect(LogType.Error, new Regex("Weather weight modifier"));
            var date = new GameDateTime(2, Season.Summer, 5);
            Assert.AreEqual(Roll(date, 1), Roll(date, 1, _hooks));
        }

        // ---- the forecast --------------------------------------------------------------------------------------

        [Test]
        public void EndDay_ForecastsTheNextDay_AndTheForecastComesTrue()
        {
            EndDay();
            Assert.IsNotEmpty(_state.ForecastWeather);
            var forecast = _state.ForecastWeather;
            var expectedFromRoll = WeatherRoller.Roll(_clock.Now.StartOfNextDay(), WeatherCatalog.BuiltIn, _state.WorldSeed);
            Assert.AreEqual(expectedFromRoll, forecast);

            var summary = EndDay();
            Assert.AreEqual(forecast, summary.NewWeather);
            Assert.AreEqual(forecast, _state.Weather);
        }

        sealed class ForceFog : IWeatherModifier
        {
            public int Order => 0;
            public string Modify(GameDateTime date, string weather, GameState state) => "fog";
        }

        [Test]
        public void ModulesCanStillOverrideTheForecast()
        {
            EndDay();
            _hooks.AddWeatherModifier(new ForceFog());
            EndDay();
            Assert.AreEqual("fog", _state.Weather);
        }

        [Test]
        public void TheForecastSurvivesSaveAndLoad()
        {
            EndDay();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_state);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
            Assert.AreEqual(_state.ForecastWeather, back.ForecastWeather);
            Assert.AreEqual(_state.WorldSeed, back.WorldSeed);
        }

        [Test]
        public void ASaveWithoutAForecast_RollsOneWhenTheDayEnds()
        {
            _state.ForecastWeather = null;
            var summary = EndDay();
            Assert.IsNotEmpty(summary.NewWeather);
            Assert.IsNotEmpty(_state.ForecastWeather);
        }

        // ---- what a definition does ----------------------------------------------------------------------------

        [TestCase(WeatherIds.Rain, true)]
        [TestCase(WeatherIds.Storm, true)]
        [TestCase(WeatherIds.Snow, false)]
        [TestCase(WeatherIds.Wind, false)]
        [TestCase(WeatherIds.Sunny, false)]
        [TestCase("some-unknown-weather", false)]
        public void OvernightWatering_FollowsTheDefinition(string tomorrow, bool watered)
        {
            _grids[MapIds.Farm].Till(1, 1);
            _state.ForecastWeather = tomorrow;
            EndDay();
            _grids[MapIds.Farm].TryGetTile(1, 1, out var tile);
            Assert.AreEqual(watered, tile.Watered);
        }

        [Test]
        public void ANewWeatherDefinedAsData_WateringCrops_NeedsNoCode()
        {
            var drizzle = WeatherDefinition.Create("drizzle", Color.gray, 0.2f, true, WeatherParticles.Rain, 0.3f, 0f, false, 1f, 1f, 1f, 1f);
            _objects.Add(drizzle);
            var catalog = new WeatherCatalog(new[] { drizzle });
            _grids[MapIds.Farm].Till(1, 1);
            _state.ForecastWeather = "drizzle";
            EndDay(catalog);
            _grids[MapIds.Farm].TryGetTile(1, 1, out var tile);
            Assert.IsTrue(tile.Watered);
            Assert.AreEqual("drizzle", _state.Weather);
        }

        [Test]
        public void Tint_DarkensStormsMoreThanRain_AndLeavesSunnyAlone()
        {
            var sunny = WeatherCatalog.BuiltIn.Get(WeatherIds.Sunny);
            var rain = WeatherCatalog.BuiltIn.Get(WeatherIds.Rain);
            var storm = WeatherCatalog.BuiltIn.Get(WeatherIds.Storm);
            Assert.AreEqual(Color.white, sunny.Apply(Color.white));
            Assert.Less(storm.Apply(Color.white).grayscale, rain.Apply(Color.white).grayscale);
            Assert.Less(rain.Apply(Color.white).grayscale, 1f);
        }

        [Test]
        public void OnlyStormsHaveLightning_AndParticlesMatchTheWeather()
        {
            var c = WeatherCatalog.BuiltIn;
            Assert.IsTrue(c.Get(WeatherIds.Storm).Lightning);
            Assert.IsFalse(c.Get(WeatherIds.Rain).Lightning);
            Assert.AreEqual(WeatherParticles.Snow, c.Get(WeatherIds.Snow).Particles);
            Assert.AreEqual(WeatherParticles.Wind, c.Get(WeatherIds.Wind).Particles);
            Assert.AreEqual(WeatherParticles.None, c.Get(WeatherIds.Sunny).Particles);
        }

        [Test]
        public void AnUnknownId_BehavesLikePlainWeather()
        {
            var def = WeatherCatalog.BuiltIn.Get("fog");
            Assert.AreEqual("weather.fog", def.NameKey);
            Assert.IsFalse(def.WateringCrops);
            Assert.AreEqual(Color.white, def.Apply(Color.white));
            _objects.Add(def);
        }

        // ---- the database and packs ----------------------------------------------------------------------------

        [Test]
        public void ACatalogFromAnEmptyDatabase_UsesTheBuiltInWeather()
        {
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]);
            _objects.Add(db);
            Assert.AreSame(WeatherCatalog.BuiltIn, WeatherCatalog.From(db));
        }

        [Test]
        public void PacksCanAddWeather_ButNotRedefineIt()
        {
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0], WeatherDefaults.CreateAll());
            var fog = WeatherDefinition.Create("fog", Color.gray, 0.3f, false, WeatherParticles.None, 0f, 0f, false, 0f, 0f, 0f, 0f);
            var clash = WeatherDefinition.Create("rain", Color.red, 1f, false, WeatherParticles.None, 0f, 0f, false, 1f, 1f, 1f, 1f);
            var pack = ContentPack.Create("test", null, null, new[] { fog, clash });
            _objects.AddRange(new Object[] { db, fog, clash, pack });
            foreach (var w in db.AllWeather) _objects.Add(w);

            LogAssert.Expect(LogType.Error, new Regex("redefines weather 'rain'"));
            Assert.AreEqual(1, db.Merge(pack));

            var catalog = WeatherCatalog.From(db);
            Assert.IsTrue(catalog.Contains("fog"));
            Assert.IsTrue(catalog.Get("rain").WateringCrops, "the core rain stays as it was");
        }

        [Test]
        public void TheDefaultTable_HasTheFiveBaseWeathers()
        {
            var defaults = WeatherDefaults.CreateAll();
            foreach (var d in defaults) _objects.Add(d);
            CollectionAssert.AreEquivalent(
                new[] { "sunny", "rain", "storm", "snow", "wind" }, defaults.Select(d => d.Id).ToArray());
        }
    }
}
