using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-09: "new crop fields do not self water in the rain". Soil tilled on a rainy or stormy day starts out watered, as the soil that was already
    // tilled at dawn was; on a dry day it does not; the greenhouse stays dry.
    public class RainWateringTests
    {
        [Test]
        public void SoilTilledWhileItRains_StartsWatered_AndOtherwiseDry()
        {
            var grid = new FarmGrid();
            Assert.IsTrue(grid.Till(1, 1));
            Assert.IsTrue(grid.TryGetTile(1, 1, out var dry)); Assert.IsFalse(dry.Watered, "a dry day");
            grid.RainsToday = true;
            Assert.IsTrue(grid.Till(2, 1));
            Assert.IsTrue(grid.TryGetTile(2, 1, out var wet)); Assert.IsTrue(wet.Watered, "in the rain");
            Assert.IsFalse(dry.Watered, "what was tilled earlier on a dry day is not made wet by this alone");
        }

        [TestCase(WeatherDefaults.Rain, true)]
        [TestCase(WeatherDefaults.Storm, true)]
        [TestCase(WeatherDefaults.Sunny, false)]
        public void TheSession_MarksTheFarmGridsFromTheDaysWeather_NotTheGreenhouse(string weather, bool wet)
        {
            using (var f = new TestSessionFixture())
            {
                f.Session.State.Weather = weather;
                var farm = new FarmGrid { RainsToday = false };
                Assert.AreEqual(wet, f.Session.GetGrid(MapIds.Farm + "_new").RainsToday, "a map's grid made today follows the weather");
                Assert.IsFalse(f.Session.GetGrid(MapIds.Greenhouse).RainsToday, "the greenhouse stays dry");
                Assert.IsNotNull(farm);
            }
        }

        [Test]
        public void ARainyDawn_WaterstheSoilAndTheNextTillingToo()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var grid = s.GetGrid(MapIds.Farm);
                grid.Till(5, 5);
                s.State.ForecastWeather = WeatherDefaults.Rain;
                s.EndDay(false);                                          // the next day is rainy
                Assert.AreEqual(WeatherDefaults.Rain, s.State.Weather);
                Assert.IsTrue(grid.RainsToday, "the day's rain is known to the grid");
                Assert.IsTrue(grid.TryGetTile(5, 5, out var old)); Assert.IsTrue(old.Watered, "tilled before: watered at dawn");
                Assert.IsTrue(grid.Till(6, 5));
                Assert.IsTrue(grid.TryGetTile(6, 5, out var fresh)); Assert.IsTrue(fresh.Watered, "tilled today: watered too");
                Assert.IsFalse(s.GetGrid(MapIds.Greenhouse).RainsToday);

                s.State.ForecastWeather = WeatherDefaults.Sunny;
                s.EndDay(false);                                          // and the day after is fine
                Assert.IsFalse(grid.RainsToday);
                Assert.IsTrue(grid.Till(7, 5));
                Assert.IsTrue(grid.TryGetTile(7, 5, out var later)); Assert.IsFalse(later.Watered);
            }
        }
    }
}
