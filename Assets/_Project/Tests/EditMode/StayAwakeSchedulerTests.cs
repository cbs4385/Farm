using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The trigger rule for the late-night stay-awake check: after 20:00, after each energy action or every half hour,
    // whichever comes first.
    public class StayAwakeSchedulerTests
    {
        static GameDateTime At(int hour, int minute = 0, int day = 5) =>
            new GameDateTime(1, Season.Spring, day, hour * 60 + minute);

        [Test]
        public void NothingHappensBeforeTwentyHundred()
        {
            var s = new StayAwakeScheduler();
            Assert.IsFalse(StayAwakeScheduler.IsActive(At(19, 59)));
            Assert.IsFalse(s.ShouldCheck(At(19, 59), energyActionFinished: true), "even an energy action");
            Assert.IsFalse(s.ShouldCheck(At(12), false));
            Assert.IsTrue(StayAwakeScheduler.IsActive(At(20)));
        }

        [Test]
        public void AnEnergyActionAfterTwentyHundred_TriggersImmediately()
        {
            var s = new StayAwakeScheduler();
            Assert.IsTrue(s.ShouldCheck(At(20, 0), true));
            Assert.IsTrue(s.ShouldCheck(At(20, 10), true));
            Assert.IsFalse(s.ShouldCheck(At(20, 10), false), "no energy action and not yet half an hour");
        }

        [Test]
        public void WithoutActions_TheFirstTimedCheckIsAtTwentyThirty()
        {
            var s = new StayAwakeScheduler();
            Assert.AreEqual(20 * 60 + 30, s.NextTimedCheckMinute(At(20, 0)));
            Assert.IsFalse(s.ShouldCheck(At(20, 29), false));
            Assert.IsTrue(s.ShouldCheck(At(20, 30), false));
        }

        [Test]
        public void EveryCheck_RestartsTheHalfHourTimer()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(20, 35));                // triggered by an action
            Assert.IsFalse(s.ShouldCheck(At(21, 4), false));
            Assert.IsTrue(s.ShouldCheck(At(21, 5), false), "30 minutes after the last check");
            Assert.AreEqual(21 * 60 + 5, s.NextTimedCheckMinute(At(20, 40)));
        }

        [Test]
        public void ActionsStillTriggerACheckRightAfterOne_AsSpecified()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(21, 0));
            Assert.IsTrue(s.ShouldCheck(At(21, 0), true), "the rule is one check per energy action");
        }

        [Test]
        public void ShouldCheck_DoesNotChangeState()
        {
            var s = new StayAwakeScheduler();
            Assert.IsTrue(s.ShouldCheck(At(20, 30), false));
            Assert.IsTrue(s.ShouldCheck(At(20, 30), false), "still due until MarkChecked");
            s.MarkChecked(At(20, 30));
            Assert.IsFalse(s.ShouldCheck(At(20, 31), false));
        }

        [Test]
        public void TheSmallHoursCountAsActive_AndTheTimerKeepsRunning()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(23, 50));
            Assert.IsFalse(s.ShouldCheck(At(24, 15), false));
            Assert.IsTrue(s.ShouldCheck(At(24, 20), false), "00:20");
            Assert.IsTrue(s.ShouldCheck(At(25, 59), false));
        }

        [Test]
        public void ANewDay_StartsTheNightAgain()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(25, 30, day: 5));
            // Next evening: the previous night's last check must not delay the first check at 20:30.
            Assert.AreEqual(20 * 60 + 30, s.NextTimedCheckMinute(At(20, 0, day: 6)));
            Assert.IsTrue(s.ShouldCheck(At(20, 30, day: 6), false));
        }

        [Test]
        public void StartTimerAt_ForALoadedGame_DoesNotAmbushThePlayer()
        {
            var s = new StayAwakeScheduler();
            s.StartTimerAt(At(22, 10));
            Assert.IsFalse(s.ShouldCheck(At(22, 11), false));
            Assert.IsTrue(s.ShouldCheck(At(22, 40), false));
        }

        [Test]
        public void ResetForgetsEverything()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(22, 0));
            s.Reset();
            Assert.AreEqual(20 * 60 + 30, s.NextTimedCheckMinute(At(22, 5)));
        }
    }
}
