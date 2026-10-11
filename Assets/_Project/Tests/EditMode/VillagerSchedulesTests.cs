using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The schedules agree with the bible (docs/narrative/BIBLE_WAVE2.md): Piper spends the day on the beach and in the square and plays in the saloon from seven until
    // it closes; Marcus's day off is a bench in the square, and Bram has lunch beside him on Wednesdays.
    public class VillagerSchedulesTests
    {
        TestSessionFixture _f;

        [SetUp]
        public void SetUp()
        {
            CellOccupants.ResetForTests();
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            _f = new TestSessionFixture(db.AllItems, null, NpcCatalog.From(db).All);
        }

        [TearDown]
        public void TearDown()
        {
            CellOccupants.ResetForTests();
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        NpcPlacement At(string npc, int day, int minute, string weather = WeatherIds.Sunny)
        {
            var s = _f.Session;
            s.State.Weather = weather;
            s.Clock.SetTime(new GameDateTime(2, Season.Spring, day, 12 * 60));
            var def = s.Npcs.Get(npc);
            return NpcSchedule.Where(def, NpcSchedule.PlanFor(def, s.World, s.Hooks.ScheduleEntriesFor(def)), minute);
        }

        [Test]
        public void Piper_IsOnTheBeachInTheMorning_TheSquareInTheAfternoon_AndTheSaloonFromSevenUntilItCloses()
        {
            Assert.AreEqual(MapIds.Beach, At("piper", 4, 12 * 60).Map, "Thursday morning");
            Assert.AreEqual(MapIds.Village, At("piper", 4, 16 * 60 + 30).Map, "and the afternoon");
            Assert.AreEqual(MapIds.Saloon, At("piper", 4, 20 * 60).Map, "the evening");
            Assert.AreEqual(MapIds.Saloon, At("piper", 4, 25 * 60 + 30).Map, "until the saloon closes at two");
            Assert.AreEqual(MapIds.Beach, At("piper", 4, 12 * 60, WeatherIds.Rain).Map, "rain does not keep her in on a workday");
        }

        [Test]
        public void OnWednesdays_MarcusSitsOnTheBenchInTheSquare_AndBramEatsLunchBesideHim()
        {
            var marcus = At("marcus", 3, 12 * 60 + 30);
            var bram = At("bram", 3, 12 * 60 + 30);
            Assert.AreEqual(MapIds.Village, marcus.Map);
            Assert.AreEqual(MapIds.Village, bram.Map);
            Assert.AreEqual(1, Mathf.Abs(marcus.ToX - bram.ToX), "side by side");
            Assert.AreEqual(marcus.ToY, bram.ToY);
            Assert.AreEqual(MapIds.Blacksmith, At("bram", 3, 15 * 60).Map, "and back to the forge after");
            Assert.AreEqual(MapIds.Blacksmith, At("bram", 4, 12 * 60 + 30).Map, "on other days he stays at the forge");
            Assert.AreNotEqual(MapIds.Village, At("bram", 3, 12 * 60 + 30, WeatherIds.Rain).Map, "no bench in the rain");
        }
    }
}
