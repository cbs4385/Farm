using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // X-006: small events tied to the moon (new, waxing, full, waning), shipped as random events in the layer's story data.
    public class MoonPhaseEventsTests
    {
        TestSessionFixture _f;
        GameSession S => _f.Session;
        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            _f = new TestSessionFixture(db.AllItems);
            _story = StoryContent.LoadFromResources();
            MythosContent.Load(_story);
            MythosModule.RegisterConditions();
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        System.Collections.Generic.List<RandomEventDefinition> MoonEvents() =>
            _story.RandomEvents.Where(e => e.Id.StartsWith("moon_")).ToList();

        bool Fires(RandomEventDefinition e, int day, int level, int lore = 5)
        {
            GameSession.HorrorLevelOverride = level;
            MythosModule.RegisterConditions();
            S.SetVar(MythosIds.Vars.Lore, lore);
            S.Clock.SetTime(new GameDateTime(1, Season.Spring, day, 12 * 60));
            return Conditions.TryEvaluate(e.Condition, S.World, out var ok) && ok;
        }

        [Test]
        public void TheLayer_ShipsTwoOrThreeEventsForEveryPhaseGroup()
        {
            var events = MoonEvents();
            Assert.AreEqual(8, events.Count);
            foreach (var group in new[] { "moon_new", "moon_waxing", "moon_full", "moon_waning" })
                Assert.That(events.Count(e => e.Id.StartsWith(group)), Is.InRange(2, 3), group);
        }

        [Test]
        public void EveryEvent_HasAParsableCondition_ValidEffects_AndAString()
        {
            foreach (var e in MoonEvents())
            {
                Assert.IsTrue(Conditions.Validate(e.Condition, out var error), e.Id + ": " + error);
                Assert.IsTrue(L.Has(e.TextKey), e.Id + " has text");
                Assert.IsNotEmpty(e.Effects, e.Id);
            }
        }

        [Test]
        public void AtLevel0_NoMoonEventCanFire_OnAnyDay()
        {
            foreach (var e in MoonEvents())
                for (var day = 1; day <= 28; day++) Assert.IsFalse(Fires(e, day, 0), $"{e.Id} day {day}");
        }

        [Test]
        public void EachEvent_FiresOnlyInItsOwnPhases_AndEveryPhaseHasSomething()
        {
            string Group(int day) => new GameDateTime(1, Season.Spring, day, 12 * 60).MoonPhase switch
            {
                MoonPhase.New => "moon_new",
                MoonPhase.Full => "moon_full",
                MoonPhase.WaxingCrescent => "moon_waxing",
                MoonPhase.FirstQuarter => "moon_waxing",
                MoonPhase.WaxingGibbous => "moon_waxing",
                _ => "moon_waning",
            };
            for (var day = 1; day <= 28; day++)
            {
                var firing = MoonEvents().Where(e => Fires(e, day, 2)).ToList();
                Assert.IsNotEmpty(firing, "day " + day + " has a moon event at full intensity");
                foreach (var e in firing) StringAssert.StartsWith(Group(day), e.Id, "day " + day);
            }
        }

        [Test]
        public void TheDeeperOnes_NeedLoreOrFullIntensity()
        {
            var lanterns = MoonEvents().First(e => e.Id == "moon_new_lanterns");
            var humming = MoonEvents().First(e => e.Id == "moon_full_humming");
            Assert.IsFalse(Fires(lanterns, 1, 2, lore: 0), "lanterns need lore");
            Assert.IsTrue(Fires(lanterns, 1, 2, lore: 1));
            var fullDay = Enumerable.Range(1, 28).First(d => new GameDateTime(1, Season.Spring, d, 12 * 60).MoonPhase == MoonPhase.Full);
            Assert.IsFalse(Fires(humming, fullDay, 1), "humming is full intensity only");
            Assert.IsTrue(Fires(humming, fullDay, 2));
        }

        [Test]
        public void TheEffects_Run_AndMildIsGentler()
        {
            var restless = MoonEvents().First(e => e.Id == "moon_waning_restless");
            GameSession.HorrorLevelOverride = 2;
            MythosEffects.Register();
            S.SetVar(MythosIds.Vars.Dread, 0);
            Effects.RunAll(S, restless.Effects);
            var full = S.GetVar(MythosIds.Vars.Dread);
            GameSession.HorrorLevelOverride = 1;
            S.SetVar(MythosIds.Vars.Dread, 0);
            Effects.RunAll(S, restless.Effects);
            Assert.Greater(full, 0);
            Assert.LessOrEqual(S.GetVar(MythosIds.Vars.Dread), full);
        }
    }
}
