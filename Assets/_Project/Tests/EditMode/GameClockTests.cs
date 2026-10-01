using System;
using System.Collections.Generic;
using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    public class GameClockTests
    {
        [Test]
        public void NewGame_StartsAtSixAm()
        {
            var d = GameDateTime.NewGame;
            Assert.AreEqual(360, d.MinuteOfDay);
            Assert.AreEqual("6:00 AM", d.ClockString());
            Assert.AreEqual(0, d.TotalDays);
        }

        [Test]
        public void ClockString_HandlesPastMidnight()
        {
            Assert.AreEqual("12:00 AM", new GameDateTime(1, Season.Spring, 1, 24 * 60).ClockString());
            Assert.AreEqual("1:30 AM", new GameDateTime(1, Season.Spring, 1, 25 * 60 + 30).ClockString());
            Assert.AreEqual("12:00 PM", new GameDateTime(1, Season.Spring, 1, 12 * 60).ClockString());
        }

        [Test]
        public void StartOfNextDay_RollsDayAndSeasonAndYear()
        {
            Assert.AreEqual(new GameDateTime(1, Season.Spring, 2), new GameDateTime(1, Season.Spring, 1).StartOfNextDay());
            Assert.AreEqual(new GameDateTime(1, Season.Summer, 1), new GameDateTime(1, Season.Spring, 28).StartOfNextDay());
            Assert.AreEqual(new GameDateTime(2, Season.Spring, 1), new GameDateTime(1, Season.Winter, 28).StartOfNextDay());
        }

        [Test]
        public void TotalDays_AndDayOfWeek()
        {
            Assert.AreEqual(28, new GameDateTime(1, Season.Summer, 1).TotalDays);
            Assert.AreEqual(112, new GameDateTime(2, Season.Spring, 1).TotalDays);
            Assert.AreEqual(0, new GameDateTime(1, Season.Fall, 1).DayOfWeek);
            Assert.AreEqual(6, new GameDateTime(1, Season.Fall, 7).DayOfWeek);
            Assert.AreEqual(0, new GameDateTime(1, Season.Fall, 8).DayOfWeek);
        }

        [Test]
        public void Tick_AdvancesTenMinutesPerStep()
        {
            var clock = new GameClock(GameDateTime.NewGame);
            clock.Tick(6.9f);
            Assert.AreEqual(360, clock.Now.MinuteOfDay);
            clock.Tick(0.2f);
            Assert.AreEqual(370, clock.Now.MinuteOfDay);
        }

        [Test]
        public void Tick_DoesNothingWhilePaused_AndPauseIsRefCounted()
        {
            var clock = new GameClock(GameDateTime.NewGame);
            clock.Pause();
            clock.Pause();
            clock.Tick(100f);
            Assert.AreEqual(360, clock.Now.MinuteOfDay);
            clock.Resume();
            Assert.IsTrue(clock.IsPaused);
            clock.Resume();
            Assert.IsFalse(clock.IsPaused);
            clock.Tick(7f);
            Assert.AreEqual(370, clock.Now.MinuteOfDay);
            Assert.Throws<InvalidOperationException>(() => clock.Resume());
        }

        [Test]
        public void ReachingTwoAm_StopsClockAndPublishesPassOutOnce()
        {
            var bus = new EventBus();
            var passOuts = 0;
            bus.Subscribe<PassOutTimeReached>(_ => passOuts++);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 1, GameDateTime.DayEndMinute - 10), bus);
            clock.Tick(7f);
            clock.Tick(1000f);
            Assert.AreEqual(GameDateTime.DayEndMinute, clock.Now.MinuteOfDay);
            Assert.AreEqual(1, passOuts);
        }

        [Test]
        public void StartNextDay_PublishesEventsInOrder_AndResetsToSixAm()
        {
            var bus = new EventBus();
            var log = new List<string>();
            bus.Subscribe<DayEnded>(e => log.Add($"ended:{e.Date.Day}:{e.PassedOut}"));
            bus.Subscribe<SeasonChanged>(e => log.Add($"season:{e.Season}"));
            bus.Subscribe<DayStarted>(e => log.Add($"started:{e.Date.Season}{e.Date.Day}"));
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 28, 900), bus);

            clock.StartNextDay(passedOut: true);

            CollectionAssert.AreEqual(new[] { "ended:28:True", "season:Summer", "started:Summer1" }, log);
            Assert.AreEqual(GameDateTime.DayStartMinute, clock.Now.MinuteOfDay);
        }

        [Test]
        public void SetTime_PublishesNothing_AndKeepsDayOverState()
        {
            var bus = new EventBus();
            var published = 0;
            bus.Subscribe<MinuteChanged>(_ => published++);
            bus.Subscribe<PassOutTimeReached>(_ => published++);
            var clock = new GameClock(GameDateTime.NewGame, bus);
            clock.SetTime(new GameDateTime(1, Season.Fall, 3, GameDateTime.DayEndMinute));
            clock.Tick(100f);
            Assert.AreEqual(0, published);
        }
    }
}
