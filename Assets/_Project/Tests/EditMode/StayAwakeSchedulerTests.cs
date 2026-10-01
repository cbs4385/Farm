using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The late-night stay-awake check: from 20:00, at most once per 20 game minutes, no energy-action trigger.
    public class StayAwakeSchedulerTests
    {
        static GameDateTime At(int hour, int minute = 0, int day = 5) =>
            new GameDateTime(1, Season.Spring, day, hour * 60 + minute);

        [Test]
        public void NothingHappensBeforeTwentyHundred()
        {
            var s = new StayAwakeScheduler();
            Assert.IsFalse(StayAwakeScheduler.IsActive(At(19, 59)));
            Assert.IsFalse(s.ShouldCheck(At(19, 59)));
            Assert.IsFalse(s.ShouldCheck(At(12)));
            Assert.IsTrue(StayAwakeScheduler.IsActive(At(20)));
        }

        [Test]
        public void TheFirstCheckIsDueAtTwentyHundred()
        {
            var s = new StayAwakeScheduler();
            Assert.AreEqual(20 * 60, s.NextCheckMinute(At(20, 0)));
            Assert.IsTrue(s.ShouldCheck(At(20, 0)));
        }

        [Test]
        public void AfterACheck_TheNextOneIsTwentyMinutesLater_NotSooner()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(20, 0));
            Assert.IsFalse(s.ShouldCheck(At(20, 10)));
            Assert.IsFalse(s.ShouldCheck(At(20, 19)));
            Assert.IsTrue(s.ShouldCheck(At(20, 20)));
            Assert.AreEqual(20 * 60 + 20, s.NextCheckMinute(At(20, 10)));
        }

        [Test]
        public void ALateCheck_RestartsTheTimerFromWhenItWasPresented()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(20, 0));
            s.MarkChecked(At(20, 30));                // presented late (e.g. the clock was paused in a menu)
            Assert.IsFalse(s.ShouldCheck(At(20, 49)));
            Assert.IsTrue(s.ShouldCheck(At(20, 50)));
        }

        [Test]
        public void ShouldCheck_DoesNotChangeState()
        {
            var s = new StayAwakeScheduler();
            Assert.IsTrue(s.ShouldCheck(At(20, 0)));
            Assert.IsTrue(s.ShouldCheck(At(20, 0)), "still due until MarkChecked");
            s.MarkChecked(At(20, 0));
            Assert.IsFalse(s.ShouldCheck(At(20, 1)));
        }

        [Test]
        public void ThirtyChecksFillTheNightOnceTheDayRunsToSix()
        {
            // The full night (20:00-06:00) has thirty checks. The calendar cannot yet represent times after 02:00
            // (decision U, day length, is part of T-046), so the walk below covers 20:00 to 02:00 only.
            Assert.AreEqual(30, StayAwakeScheduler.ChecksPerNight);

            var s = new StayAwakeScheduler();
            var times = new List<int>();
            for (var minute = StayAwakeScheduler.StartMinuteOfDay; minute <= GameDateTime.DayEndMinute; minute += 10)
            {
                var now = new GameDateTime(1, Season.Spring, 5, minute);
                if (s.ShouldCheck(now)) { times.Add(minute); s.MarkChecked(now); }
            }
            Assert.AreEqual(StayAwakeScheduler.StartMinuteOfDay, times.First());
            Assert.AreEqual(20 * 60 + 20, times[1]);
            Assert.AreEqual((GameDateTime.DayEndMinute - StayAwakeScheduler.StartMinuteOfDay) / 20 + 1, times.Count);
            Assert.IsTrue(times.Zip(times.Skip(1), (x, y) => y - x).All(d => d == 20), "exactly every 20 minutes");
        }

        [Test]
        public void ChecksTodayCountsRepetitionsInTheDay_AndRestartsOnANewDay()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(20, 0));
            s.MarkChecked(At(20, 20));
            s.MarkChecked(At(20, 40));
            Assert.AreEqual(3, s.ChecksToday);

            Assert.AreEqual(20 * 60, s.NextCheckMinute(At(20, 0, day: 6)), "a new day starts the night again");
            s.MarkChecked(At(20, 0, day: 6));
            Assert.AreEqual(1, s.ChecksToday);
        }

        [Test]
        public void StartTimerAt_ForALoadedGame_DoesNotAmbushThePlayer_AndRestoresTheCount()
        {
            var s = new StayAwakeScheduler();
            s.StartTimerAt(At(22, 10), checksAlreadyDone: 7);
            Assert.AreEqual(7, s.ChecksToday);
            Assert.IsFalse(s.ShouldCheck(At(22, 11)));
            Assert.IsTrue(s.ShouldCheck(At(22, 30)));
        }

        [Test]
        public void ResetForgetsEverything()
        {
            var s = new StayAwakeScheduler();
            s.MarkChecked(At(22, 0));
            s.Reset();
            Assert.AreEqual(0, s.ChecksToday);
            Assert.AreEqual(20 * 60, s.NextCheckMinute(At(22, 5)));
        }
    }

    public class StayAwakeChallengeTests
    {
        [Test]
        public void ResponseWindowShrinksWithEachRepetitionInTheDay_ButHasAFloor()
        {
            var previous = float.MaxValue;
            for (var i = 0; i < 30; i++)
            {
                var w = StayAwakeChallenge.WindowSeconds(i);
                Assert.LessOrEqual(w, previous, $"check {i}");
                Assert.GreaterOrEqual(w, StayAwakeChallenge.MinWindowSeconds);
                previous = w;
            }
            Assert.AreEqual(StayAwakeChallenge.StartWindowSeconds, StayAwakeChallenge.WindowSeconds(0), 0.0001f);
            Assert.Less(StayAwakeChallenge.WindowSeconds(29), StayAwakeChallenge.WindowSeconds(0) / 3f);
            Assert.AreEqual(StayAwakeChallenge.StartWindowSeconds, StayAwakeChallenge.WindowSeconds(-3), 0.0001f);
        }

        [Test]
        public void PromptsAreSeeded_AndAFullSeries()
        {
            var a = StayAwakeChallenge.GeneratePrompts(42, 3);
            var b = StayAwakeChallenge.GeneratePrompts(42, 3);
            CollectionAssert.AreEqual(a, b, "same seed and check give the same prompts");
            Assert.AreEqual(StayAwakeChallenge.PromptsPerChallenge, a.Count);

            var different = Enumerable.Range(0, 12).Select(i => string.Join(",", StayAwakeChallenge.GeneratePrompts(42, i))).Distinct().Count();
            Assert.Greater(different, 1, "later checks get different series");
        }

        [Test]
        public void PromptsUseTheWholeSet_OverManyChecks()
        {
            var seen = new HashSet<QtePrompt>();
            for (var i = 0; i < 60; i++)
                foreach (var p in StayAwakeChallenge.GeneratePrompts(7, i)) seen.Add(p);
            Assert.AreEqual(6, seen.Count);
        }

        static List<StayAwakeChallenge.Response> Answers(IEnumerable<QtePrompt> prompts, float seconds) =>
            prompts.Select(p => new StayAwakeChallenge.Response(p, seconds)).ToList();

        [Test]
        public void CorrectAndInTime_Passes()
        {
            var prompts = StayAwakeChallenge.GeneratePrompts(1, 0);
            Assert.AreEqual(StayAwakeChallenge.Outcome.Passed, StayAwakeChallenge.Evaluate(prompts, Answers(prompts, 0.5f), 1f));
        }

        [Test]
        public void AWrongPrompt_Fails()
        {
            var prompts = new List<QtePrompt> { QtePrompt.Up, QtePrompt.Left, QtePrompt.Interact };
            var answers = new List<StayAwakeChallenge.Response>
            {
                new StayAwakeChallenge.Response(QtePrompt.Up, 0.3f),
                new StayAwakeChallenge.Response(QtePrompt.Right, 0.3f),
                new StayAwakeChallenge.Response(QtePrompt.Interact, 0.3f),
            };
            Assert.AreEqual(StayAwakeChallenge.Outcome.WrongPrompt, StayAwakeChallenge.Evaluate(prompts, answers, 1f));
        }

        [Test]
        public void TooSlow_And_MissingResponses_Fail()
        {
            var prompts = new List<QtePrompt> { QtePrompt.Down, QtePrompt.UseTool };
            Assert.AreEqual(StayAwakeChallenge.Outcome.TooSlow,
                StayAwakeChallenge.Evaluate(prompts, Answers(prompts, 1.01f), 1f));
            Assert.AreEqual(StayAwakeChallenge.Outcome.Passed,
                StayAwakeChallenge.Evaluate(prompts, Answers(prompts, 1.0f), 1f), "exactly on the limit passes");
            Assert.AreEqual(StayAwakeChallenge.Outcome.Incomplete,
                StayAwakeChallenge.Evaluate(prompts, Answers(prompts.Take(1), 0.2f), 1f));
        }

        [Test]
        public void ALaterCheckIsHarder_ForTheSameReactionTime()
        {
            var prompts = StayAwakeChallenge.GeneratePrompts(5, 0);
            var answers = Answers(prompts, 0.9f);
            Assert.AreEqual(StayAwakeChallenge.Outcome.Passed,
                StayAwakeChallenge.Evaluate(prompts, answers, StayAwakeChallenge.WindowSeconds(0)));
            Assert.AreEqual(StayAwakeChallenge.Outcome.TooSlow,
                StayAwakeChallenge.Evaluate(prompts, answers, StayAwakeChallenge.WindowSeconds(25)));
        }
    }

    public class StayAwakePenaltyTests
    {
        [Test]
        public void AllNightChecks_GiveNoEnergyRecovery_AndHalfEffectiveLuck()
        {
            var all = StayAwakeScheduler.ChecksPerNight;
            Assert.AreEqual(1.0f, StayAwakePenalty.EnergyRecoveryPenalty(all), 0.0001f);
            Assert.AreEqual(0.5f, StayAwakePenalty.LuckPenalty(all), 0.0001f);
            Assert.AreEqual(0.5f, StayAwakePenalty.LuckEffectiveness(all), 0.0001f);
        }

        [Test]
        public void ThePenaltyGrowsWithEachCheckPassed_AndIsZeroBeforeAny()
        {
            Assert.AreEqual(0f, StayAwakePenalty.EnergyRecoveryPenalty(0));
            Assert.AreEqual(1f, StayAwakePenalty.LuckEffectiveness(0));
            var previous = 0f;
            for (var i = 1; i <= StayAwakeScheduler.ChecksPerNight; i++)
            {
                var p = StayAwakePenalty.EnergyRecoveryPenalty(i);
                Assert.Greater(p, previous);
                previous = p;
            }
            Assert.AreEqual(0.5f, StayAwakePenalty.EnergyRecoveryPenalty(15), 0.0001f, "half way through the night, half the recovery");
            Assert.AreEqual(1f, StayAwakePenalty.EnergyRecoveryPenalty(99), "never more than 100%");
            Assert.AreEqual(0f, StayAwakePenalty.EnergyRecoveryPenalty(-4));
        }

        [Test]
        public void SleepRestoresEnergyTowardsTheTarget_ScaledByThePenalty()
        {
            Assert.AreEqual(270, StayAwakePenalty.EnergyAfterSleep(20, 270, 0), "no penalty: full recovery");
            Assert.AreEqual(145, StayAwakePenalty.EnergyAfterSleep(20, 270, 15), "half the recovery");
            Assert.AreEqual(20, StayAwakePenalty.EnergyAfterSleep(20, 270, StayAwakeScheduler.ChecksPerNight), "an all-nighter recovers nothing");
            Assert.AreEqual(202, StayAwakePenalty.EnergyAfterSleep(10, 202, 0), "pass-out target is 75%");
            Assert.AreEqual(250, StayAwakePenalty.EnergyAfterSleep(250, 202, 5), "never lowers energy");
        }
    }
}
