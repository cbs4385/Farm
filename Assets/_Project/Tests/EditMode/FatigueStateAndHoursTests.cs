using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class FatigueLuckTests
    {
        [Test]
        public void FatigueScalesGoodLuck_AndLeavesBadLuckAlone()
        {
            Assert.AreEqual(0.4f, FatigueModel.ApplyToLuck(0.8f, 1f), 0.0001f, "half effectiveness at full fatigue");
            Assert.AreEqual(0.6f, FatigueModel.ApplyToLuck(0.8f, 0.5f), 0.0001f);
            Assert.AreEqual(0.8f, FatigueModel.ApplyToLuck(0.8f, 0f), 0.0001f);
            Assert.AreEqual(-0.5f, FatigueModel.ApplyToLuck(-0.5f, 1f), 0.0001f, "bad luck is not made better or worse");
            Assert.AreEqual(0f, FatigueModel.ApplyToLuck(0f, 1f), "neutral luck has nothing to scale");
        }
    }

    public class FatigueStateTests
    {
        const int TenPm = 22 * 60;
        const int Midnight = 24 * 60;
        const int SixAm = 30 * 60;

        [Test]
        public void ARestedPlayer_HasNoRating_AndNoMeter()
        {
            var s = new FatigueState();
            Assert.AreEqual(0f, s.LuckRating(12 * 60));
            Assert.IsFalse(s.MeterVisible(12 * 60));
            Assert.IsFalse(s.MeterVisible(TenPm));
            Assert.IsTrue(s.MeterVisible(TenPm + 10));
        }

        [Test]
        public void SleepingInABed_ClearsEverything()
        {
            var s = new FatigueState(0.8f);
            s.AfterSleep(inBed: true, Midnight);
            Assert.AreEqual(0f, s.Carried);
            Assert.IsFalse(s.MeterVisible(12 * 60));
        }

        [Test]
        public void CollapsingAtSixAm_CarriesTheFatigueIntoTheNextDay()
        {
            var s = new FatigueState();
            s.AfterSleep(inBed: false, SixAm);
            Assert.AreEqual(1f, s.Carried, 0.0001f);

            // The next morning, still fatigued: the meter is visible and luck is halved all day.
            Assert.IsTrue(s.MeterVisible(7 * 60));
            Assert.AreEqual(1f, s.LuckRating(12 * 60), 0.0001f);
            Assert.AreEqual(0.4f, s.Luck(0.8f, 12 * 60), 0.0001f);
        }

        [Test]
        public void ACarriedPenalty_DoesNotCostASecondNightsEnergyRecovery()
        {
            var s = new FatigueState(1f);
            Assert.AreEqual(0f, s.RecoveryRating(21 * 60), "carried fatigue is about luck, not recovery");
            Assert.AreEqual(270, s.EnergyAfterSleep(40, 270, 21 * 60), "an early bed night recovers fully");
            s.AfterSleep(inBed: true, 21 * 60);
            Assert.AreEqual(0f, s.Carried, "and clears the carried penalty");
        }

        [Test]
        public void CollapsingRecoversNoEnergy_ButAnEarlyBedNightDoes()
        {
            var s = new FatigueState();
            Assert.AreEqual(100, s.EnergyAfterSleep(100, 270, SixAm), "all night awake: no recovery");
            Assert.AreEqual(100 + (int)System.Math.Round(170 * 0.75f), s.EnergyAfterSleep(100, 270, Midnight), "midnight bed: 75%");
            Assert.AreEqual(270, s.EnergyAfterSleep(100, 270, 20 * 60), "bed before 22:00: full");
        }

        [Test]
        public void ACollapseBeforeSixAm_CarriesOnlyWhatWasBuiltUp()
        {
            var s = new FatigueState();
            s.AfterSleep(inBed: false, Midnight);
            Assert.AreEqual(0.25f, s.Carried, 0.0001f);
        }

        [Test]
        public void TheCarriedValueNeverShrinksWithoutABed()
        {
            var s = new FatigueState(0.9f);
            s.AfterSleep(inBed: false, 23 * 60);   // a smaller fatigue
            Assert.AreEqual(0.9f, s.Carried, 0.0001f);
        }

        [Test]
        public void TheRatingIsTheLargerOfCarriedAndTonight()
        {
            var s = new FatigueState(0.3f);
            Assert.AreEqual(0.3f, s.LuckRating(10 * 60), 0.0001f);
            Assert.AreEqual(0.5f, s.LuckRating(26 * 60), 0.0001f, "tonight overtakes the carried value");
        }

        [Test]
        public void TheConstructorClampsItsInput()
        {
            Assert.AreEqual(1f, new FatigueState(7f).Carried);
            Assert.AreEqual(0f, new FatigueState(-1f).Carried);
        }
    }

    public class BusinessHoursTests
    {
        [SetUp] public void SetUp() { BusinessHoursRegistry.Clear(); Conditions.ClearCustomForTests(); }
        [TearDown] public void TearDown() { BusinessHoursRegistry.Clear(); Conditions.ClearCustomForTests(); }

        // Day 1 of a season is a Monday (DayOfWeek 0); day 7 is a Sunday.
        static GameDateTime At(int day, int hour, int minute = 0) =>
            new GameDateTime(1, Season.Spring, day, hour * 60 + minute);

        sealed class World : IWorldQuery
        {
            public GameDateTime Time;
            public bool HasFlag(string f) => false;
            public int GetVar(string n) => 0;
            public GameDateTime Now => Time;
            public string Weather => "sunny";
            public string MapId => "Village";
        }

        [Test]
        public void StandardHoursAreNineToFive()
        {
            var h = BusinessHours.Standard();
            Assert.IsFalse(h.IsOpen(At(2, 8, 59)));
            Assert.IsTrue(h.IsOpen(At(2, 9, 0)));
            Assert.IsTrue(h.IsOpen(At(2, 16, 59)));
            Assert.IsFalse(h.IsOpen(At(2, 17, 0)));
            Assert.IsFalse(h.IsOpen(At(2, 23)));
        }

        [Test]
        public void AShopkeeperHasARegularDayOff()
        {
            var h = BusinessHours.Standard(dayOff: 6);       // closed Sundays
            Assert.IsTrue(h.IsOpen(At(1, 12)), "Monday");
            Assert.IsFalse(h.IsOpen(At(7, 12)), "Sunday");
            Assert.IsFalse(h.IsOpen(At(14, 12)), "the next Sunday");
            Assert.IsTrue(h.IsOpen(At(8, 12)), "Monday again");
        }

        [Test]
        public void ALatePlaceKeepsEveningHours_EvenPastMidnight()
        {
            var saloon = new BusinessHours(12 * 60, 26 * 60, dayOff: 0);   // 12:00 to 02:00, closed Mondays
            Assert.IsFalse(saloon.IsOpen(At(2, 11, 59)));
            Assert.IsTrue(saloon.IsOpen(At(2, 20)));
            Assert.IsTrue(saloon.IsOpen(At(2, 25, 30)), "01:30 the same day");
            Assert.IsFalse(saloon.IsOpen(At(1, 20)), "Monday is the day off");
        }

        [Test]
        public void TheRegistryAnswersByShopId_AndUnknownShopsAreAlwaysOpen()
        {
            BusinessHoursRegistry.Register("general", BusinessHours.Standard(dayOff: 6));
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("general", At(2, 10)));
            Assert.IsFalse(BusinessHoursRegistry.IsOpen("general", At(2, 18)));
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("unfinished", At(2, 7)), "no hours registered: never lock the player out");
        }

        [Test]
        public void TheAgreedTable_IsRegistered_WithStaggeredDaysOff()
        {
            BusinessHoursRegistry.RegisterDefaults();
            string[] shops = { "general", "blacksmith", "carpenter", "fish", "clinic", "saloon" };

            // Noon on each weekday of the first week (Monday = day 1): the table's day-off pattern.
            var closedOn = new Dictionary<string, int>();
            foreach (var id in shops)
                for (var day = 1; day <= 7; day++)
                    if (!BusinessHoursRegistry.IsOpen(id, At(day, 12, 30))) closedOn[id] = day - 1;   // DayOfWeek 0..6

            Assert.AreEqual(6, closedOn["general"], "Sunday");
            Assert.AreEqual(0, closedOn["blacksmith"], "Monday");
            Assert.AreEqual(2, closedOn["carpenter"], "Wednesday");
            Assert.AreEqual(3, closedOn["fish"], "Thursday");
            Assert.AreEqual(5, closedOn["clinic"], "Saturday");
            Assert.AreEqual(1, closedOn["saloon"], "Tuesday");

            for (var day = 1; day <= 7; day++)
            {
                var openCount = 0;
                foreach (var id in shops) if (BusinessHoursRegistry.IsOpen(id, At(day, 12, 30))) openCount++;
                Assert.GreaterOrEqual(openCount, shops.Length - 1, $"day {day}: at most one business closed, so something is always open");
            }
        }

        [Test]
        public void TheAgreedHours_SuitEachBusiness()
        {
            BusinessHoursRegistry.RegisterDefaults();
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("fish", At(2, 6, 30)), "the fish shop opens early");
            Assert.IsFalse(BusinessHoursRegistry.IsOpen("fish", At(2, 15)), "and closes in the afternoon");
            Assert.IsFalse(BusinessHoursRegistry.IsOpen("saloon", At(3, 11)), "the saloon opens at noon");
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("saloon", At(3, 21)), "and is busy in the evening");
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("saloon", At(3, 25, 30)), "until 02:00");
            Assert.IsFalse(BusinessHoursRegistry.IsOpen("general", At(3, 18)), "the general store closes at 17:00");
            Assert.IsTrue(BusinessHoursRegistry.IsOpen("merchant", At(3, 20)), "the merchant keeps late hours on the days it visits");
        }

        [Test]
        public void TheOpenConditionAtom_WorksInExpressions()
        {
            BusinessHoursRegistry.RegisterConditionAtom();
            BusinessHoursRegistry.Register("general", BusinessHours.Standard(dayOff: 6));
            var world = new World { Time = At(2, 10) };

            Assert.IsTrue(Conditions.Evaluate("open:general", world));
            Assert.IsFalse(Conditions.Evaluate("!open:general", world));
            world.Time = At(2, 18);
            Assert.IsFalse(Conditions.Evaluate("open:general", world));
            Assert.IsTrue(Conditions.Evaluate("!open:general && hour>=17", world), "a villager can head home when the shop closes");
            Assert.IsTrue(Conditions.Validate("open:blacksmith", out _));
        }
    }
}
