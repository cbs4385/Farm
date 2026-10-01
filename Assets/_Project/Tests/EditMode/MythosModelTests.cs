using System;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Encodes the owner's wakefulness numbers (GDD section 9, A and C).
    public class WakefulnessModelTests
    {
        [Test]
        public void ASeasonAddsExactlyTwentyFivePercent_AndAYearReachesFullWakefulness()
        {
            var w = 0;
            for (var day = 1; day <= GameDateTime.DaysPerSeason; day++) w = WakefulnessModel.AfterDay(w, day);
            Assert.AreEqual(250, w, "25% per season");

            w = 0;
            for (var season = 0; season < 4; season++)
                for (var day = 1; day <= GameDateTime.DaysPerSeason; day++) w = WakefulnessModel.AfterDay(w, day);
            Assert.AreEqual(WakefulnessModel.Max, w, "full wakefulness after one game year");
            Assert.IsTrue(WakefulnessModel.IsFullyAwake(w));
        }

        [Test]
        public void DailyRise_NeverNegative_AndNeverExceedsTheMaximum()
        {
            for (var day = 1; day <= GameDateTime.DaysPerSeason; day++)
                Assert.That(WakefulnessModel.RiseForDay(day), Is.InRange(8, 9), $"day {day}");
            Assert.AreEqual(WakefulnessModel.Max, WakefulnessModel.AfterDay(WakefulnessModel.Max, 5));
        }

        [Test]
        public void ARitualLowersWakefulnessByThirtyToFortyPercent()
        {
            Assert.AreEqual(300, WakefulnessModel.RitualReduction(0.0));
            Assert.AreEqual(400, WakefulnessModel.RitualReduction(0.999999));
            for (var r = 0.0; r < 1.0; r += 0.01)
                Assert.That(WakefulnessModel.RitualReduction(r), Is.InRange(300, 400));
            Assert.AreEqual(700, WakefulnessModel.AfterSuccessfulRitual(1000, 0.0));
            Assert.AreEqual(500, WakefulnessModel.AfterSuccessfulRitual(900, 0.999999));
        }

        [Test]
        public void RitualsFloorAtZero()
        {
            Assert.AreEqual(0, WakefulnessModel.AfterSuccessfulRitual(100, 0.5));
            Assert.AreEqual(0, WakefulnessModel.AfterSuccessfulRitual(0, 0.5));
        }

        [Test]
        public void SeveralSuccessfulRitualsRecoverFromAHighWakefulness()
        {
            var w = 900;
            w = WakefulnessModel.AfterSuccessfulRitual(w, 0.0);   // -30%
            w = WakefulnessModel.AfterSuccessfulRitual(w, 0.0);
            w = WakefulnessModel.AfterSuccessfulRitual(w, 0.0);
            Assert.AreEqual(0, w);
        }

        [Test]
        public void StepsAdvanceEveryFivePercent_FromZeroToTwenty()
        {
            Assert.AreEqual(0, WakefulnessModel.Step(0));
            Assert.AreEqual(0, WakefulnessModel.Step(49));
            Assert.AreEqual(1, WakefulnessModel.Step(50));
            Assert.AreEqual(10, WakefulnessModel.Step(500));
            Assert.AreEqual(19, WakefulnessModel.Step(999));
            Assert.AreEqual(20, WakefulnessModel.Step(1000));
            Assert.AreEqual(20, WakefulnessModel.Step(5000));
            Assert.AreEqual(0, WakefulnessModel.Step(-5));
            Assert.AreEqual(WakefulnessModel.StepCount, WakefulnessModel.Step(WakefulnessModel.Max));
        }

        [Test]
        public void PercentIsReadableForUi()
        {
            Assert.AreEqual(25f, WakefulnessModel.Percent(250), 0.001f);
        }

        [Test]
        public void ConditionsCanReadWakefulness_ThroughTheStoryVariable()
        {
            // 50% or more: var:mythos.wakefulness>=500
            var state = GameState.NewGame("T", "F", _ => 1);
            state.Vars[MythosIds.Vars.Wakefulness] = 600;
            var world = new StateWorldQuery(state, new GameClock(GameDateTime.NewGame));
            Assert.IsTrue(Conditions.Evaluate($"var:{MythosIds.Vars.Wakefulness}>=500", world));
            Assert.IsFalse(Conditions.Evaluate($"var:{MythosIds.Vars.Wakefulness}>=700", world));
        }
    }

    public class MythosNamesTests
    {
        [Test]
        public void ChosenNamesAreRegistered()
        {
            L.AddTable("en", MythosModule.Strings);
            L.SetLanguage("en");
            Assert.AreEqual("Nharoth", L.Get("mythos.god.name"));
            Assert.AreEqual("Keepers of the Covenant", L.Get("mythos.cult.name"));
            Assert.AreEqual("Harrow Wood", L.Get("mythos.woods.name"));
            Assert.AreEqual("Bellweather", L.Get("village.name"), "the village is part of the base game");
            Assert.AreEqual("HarrowWood", MythosIds.Maps.Woods);
        }
    }

    public class LuckHookTests
    {
        sealed class Shift : ILuckModifier
        {
            readonly float _by; readonly int _order;
            public Shift(float by, int order = 0) { _by = by; _order = order; }
            public int Order => _order;
            public float Modify(float luck, GameState state) => luck + _by;
        }

        sealed class Scale : ILuckModifier
        {
            public int Order => 10;
            public float Modify(float luck, GameState state) => luck * 0.5f;
        }

        sealed class Boom : ILuckModifier
        {
            public int Order => 0;
            public float Modify(float luck, GameState state) => throw new InvalidOperationException("x");
        }

        static GameState State() => GameState.NewGame("T", "F", _ => 1);

        [Test]
        public void NeutralByDefault()
        {
            Assert.AreEqual(0f, new GameHooks().ComputeLuck(State()), 0.0001f);
        }

        [Test]
        public void ModifiersApplyInOrder_AndTheResultIsClamped()
        {
            var hooks = new GameHooks();
            hooks.AddLuckModifier(new Scale());          // order 10: applied second
            hooks.AddLuckModifier(new Shift(-0.6f));     // order 0: applied first
            Assert.AreEqual(-0.3f, hooks.ComputeLuck(State()), 0.0001f);

            hooks.AddLuckModifier(new Shift(-5f, order: 5));
            Assert.AreEqual(-0.5f, hooks.ComputeLuck(State()), 0.0001f, "clamped to -1, then halved");
        }

        [Test]
        public void AFailingModifierIsLoggedAndSkipped()
        {
            var hooks = new GameHooks();
            hooks.AddLuckModifier(new Boom());
            hooks.AddLuckModifier(new Shift(0.25f, order: 1));
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex("Luck modifier"));
            Assert.AreEqual(0.25f, hooks.ComputeLuck(State()), 0.0001f);
        }

        [Test]
        public void DreadStyleModifier_UsesStoryVariables()
        {
            // Example of the intended use: more dread -> worse luck.
            var hooks = new GameHooks();
            hooks.AddLuckModifier(new DreadLuck());
            var s = State();
            Assert.AreEqual(0f, hooks.ComputeLuck(s), 0.0001f);
            s.Vars["dread"] = 50;
            Assert.AreEqual(-0.5f, hooks.ComputeLuck(s), 0.0001f);
            s.Vars["dread"] = 100;
            Assert.AreEqual(-1f, hooks.ComputeLuck(s), 0.0001f);
        }

        sealed class DreadLuck : ILuckModifier
        {
            public int Order => 0;
            public float Modify(float luck, GameState state) =>
                luck - (state.Vars.TryGetValue("dread", out var d) ? d / 100f : 0f);
        }
    }
}
