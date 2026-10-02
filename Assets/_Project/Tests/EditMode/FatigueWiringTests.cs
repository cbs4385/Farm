using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-046: the late-night fatigue model wired into sleeping, luck, saving and the 06:00 day end.
    public class FatigueWiringTests
    {
        GameState _state;
        GameClock _clock;
        Dictionary<string, FarmGrid> _grids;
        GameHooks _hooks;

        [SetUp]
        public void SetUp()
        {
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 0);
            _clock = new GameClock(new GameDateTime(1, Season.Summer, 10, 1000));
            _state.SetDate(_clock.Now);
            _grids = new Dictionary<string, FarmGrid> { { MapIds.Farm, new FarmGrid() } };
            _hooks = new GameHooks();
        }

        void At(int hour, int minute = 0)
        {
            _clock.SetTime(_clock.Now.WithMinuteOfDay(hour * 60 + minute));
            _state.SetDate(_clock.Now);
        }

        DaySummary Sleep(bool passedOut = false) =>
            DayCycle.EndDay(_state, _clock, _grids, id => null, id => null, passedOut, _hooks);

        // ---- the day ----------------------------------------------------------------------------------------------

        [Test]
        public void TheDayRunsFromSixToSix()
        {
            Assert.AreEqual(6 * 60, GameDateTime.DayStartMinute);
            Assert.AreEqual(30 * 60, GameDateTime.DayEndMinute);
            Assert.IsFalse(new GameDateTime(1, Season.Spring, 3, 29 * 60 + 50).IsDayOver);
            Assert.IsTrue(new GameDateTime(1, Season.Spring, 3, 30 * 60).IsDayOver);
            Assert.AreEqual("6:00 AM", new GameDateTime(1, Season.Spring, 3, 30 * 60).ClockString());
            Assert.AreEqual("2:00 AM", new GameDateTime(1, Season.Spring, 3, 26 * 60).ClockString());
        }

        [Test]
        public void TheClockStopsAndPassesOutAtSixInTheMorning()
        {
            var bus = new EventBus();
            var passOuts = 0;
            bus.Subscribe<PassOutTimeReached>(_ => passOuts++);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 3, 26 * 60), bus);
            clock.AdvanceMinutes(200);
            Assert.AreEqual(0, passOuts, "02:00 is no longer the end of the day");
            clock.AdvanceMinutes(200);
            Assert.AreEqual(30 * 60, clock.Now.MinuteOfDay);
            Assert.AreEqual(1, passOuts);
        }

        // ---- sleeping ---------------------------------------------------------------------------------------------

        [Test]
        public void SleepingBeforeTenRestoresAllEnergy_AndSaysNothingAboutFatigue()
        {
            _state.Energy = 20;
            At(21, 30);
            var summary = Sleep();
            Assert.AreEqual(_state.MaxEnergy, _state.Energy);
            Assert.AreEqual(0f, summary.FatigueAtSleep);
            Assert.IsFalse(summary.Notes.Any(n => n.Key == "summary.late_night"));
            Assert.AreEqual(0f, _state.FatigueCarried);
        }

        [Test]
        public void SleepingLate_RestoresLessEnergy_InProportionToTheFatigue()
        {
            _state.Energy = 50;
            At(26);   // 02:00, half way through the fatigue period
            var summary = Sleep();
            Assert.AreEqual(0.5f, summary.FatigueAtSleep, 0.001f);
            Assert.AreEqual(50 + (_state.MaxEnergy - 50) / 2, _state.Energy);
            Assert.IsTrue(summary.Notes.Any(n => n.Key == "summary.late_night"));
        }

        [Test]
        public void SleepingAfterAWholeNight_RestoresNoEnergy()
        {
            _state.Energy = 30;
            At(29, 50);
            Sleep();
            Assert.That(_state.Energy, Is.InRange(30, 36), "an all-nighter recovers almost nothing");

            _state.Energy = 30;
            At(30);
            Sleep();
            Assert.AreEqual(30, _state.Energy, "and nothing at all at 06:00");
        }

        [Test]
        public void SleepingInABed_ClearsFatigue_EvenAfterAnAllNighter()
        {
            _state.FatigueCarried = 1f;
            At(23);
            Sleep();
            Assert.AreEqual(0f, _state.FatigueCarried);
        }

        // ---- collapsing at 06:00 ------------------------------------------------------------------------------------

        [Test]
        public void CollapsingAtDawn_CostsGold_CarriesFatigueOver_AndWakesInTheFarmhouse()
        {
            _state.Gold = 1000;
            _state.Energy = 40;
            At(30);
            var summary = Sleep(passedOut: true);

            Assert.AreEqual(50, summary.PassOutGoldLoss);
            Assert.AreEqual(950, _state.Gold);
            Assert.AreEqual(40, _state.Energy, "75% energy, less a full night's fatigue: nothing recovered");
            Assert.AreEqual(1f, _state.FatigueCarried, 0.001f, "the fatigue is carried into the next day");
            Assert.AreEqual(MapIds.FarmHouse, _state.CurrentMap);
            Assert.AreEqual(DayCycle.BedSpawn, _state.SpawnPoint);
            Assert.IsTrue(summary.Notes.Any(n => n.Key == "summary.collapsed"));
        }

        [Test]
        public void CarriedFatigue_StaysUntilTheNextBedSleep()
        {
            At(30);
            Sleep(passedOut: true);
            Assert.AreEqual(1f, new FatigueState(_state.FatigueCarried).LuckRating(_clock.Now.MinuteOfDay), 0.001f,
                "the next morning is still exhausting");

            // A second collapse keeps it; a bed sleep clears it.
            At(29);
            Sleep(passedOut: true);
            Assert.Greater(_state.FatigueCarried, 0.8f);
            At(20);
            Sleep();
            Assert.AreEqual(0f, _state.FatigueCarried);
        }

        [Test]
        public void CarriedFatigue_DoesNotCostASecondNightsRecovery()
        {
            _state.FatigueCarried = 1f;
            _state.Energy = 10;
            At(20);
            Sleep();
            Assert.AreEqual(_state.MaxEnergy, _state.Energy);
        }

        // ---- luck -------------------------------------------------------------------------------------------------

        sealed class FixedLuck : ILuckModifier
        {
            readonly float _value;
            public FixedLuck(float value) { _value = value; }
            public int Order => 0;
            public float Modify(float luck, GameState state) => _value;
        }

        float Luck(float moduleLuck, int hour, int minute = 0)
        {
            var hooks = new GameHooks();
            hooks.AddLuckModifier(new FixedLuck(moduleLuck));
            hooks.AddLuckModifier(new FatigueLuckModifier(() => hour * 60 + minute));
            return hooks.ComputeLuck(_state);
        }

        [Test]
        public void GoodLuck_ShrinksWithFatigue_ByUpToHalf()
        {
            Assert.AreEqual(0.8f, Luck(0.8f, 21), 0.001f);
            Assert.AreEqual(0.8f, Luck(0.8f, 22), 0.001f);
            Assert.AreEqual(0.6f, Luck(0.8f, 26), 0.001f);
            Assert.AreEqual(0.4f, Luck(0.8f, 30), 0.001f);
        }

        [Test]
        public void BadLuck_IsNeverMadeBetterOrWorseByFatigue()
        {
            Assert.AreEqual(-0.6f, Luck(-0.6f, 29), 0.001f);
            Assert.AreEqual(0f, Luck(0f, 29), 0.001f);
        }

        [Test]
        public void CarriedFatigue_ReducesGoodLuckTheNextMorning()
        {
            _state.FatigueCarried = 1f;
            Assert.AreEqual(0.4f, Luck(0.8f, 6), 0.001f);
        }

        // ---- the warning ------------------------------------------------------------------------------------------

        [Test]
        public void TheWarning_IsDueFromTenPmUntilItHasBeenSeen()
        {
            Assert.IsFalse(FatigueModel.NeedsWarning(21 * 60 + 50, false));
            Assert.IsTrue(FatigueModel.NeedsWarning(22 * 60, false));
            Assert.IsTrue(FatigueModel.NeedsWarning(25 * 60, false), "also when a save is loaded after 22:00");
            Assert.IsFalse(FatigueModel.NeedsWarning(25 * 60, true));
            Assert.IsFalse(FatigueModel.NeedsWarning(7 * 60, false), "never in the morning");
        }

        [Test]
        public void FatigueCarried_IsSaved()
        {
            _state.FatigueCarried = 0.75f;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_state);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
            Assert.AreEqual(0.75f, back.FatigueCarried, 0.0001f);
        }
    }
}
