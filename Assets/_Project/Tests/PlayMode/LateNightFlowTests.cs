using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-046 in the real game: the one-time 22:00 message, the fatigue meter, and a late night's cost, with a
    // simulated keyboard dismissing the message.
    public class LateNightFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-latenight-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator StartFarm(Action<GameSession> ready)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.Story = new StoryContent();      // no letters, quests or random events: these tests are about fatigue
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;   // random clutter would make tile positions unpredictable
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            ready(session);
        }

        static void SetClock(GameSession session, int hour, int minute = 0)
        {
            session.Clock.SetTime(session.Clock.Now.WithMinuteOfDay(hour * 60 + minute));
            session.State.SetDate(session.Clock.Now);
        }

        static bool MeterShown() =>
            Resources.FindObjectsOfTypeAll<RectTransform>().Any(r => r.name == "FatigueBar" && r.gameObject.activeInHierarchy);

        [UnityTest]
        public IEnumerator PastTenPm_TheWarningShowsOnce_AndTheMeterAppears()
        {
            GameSession session = null;
            yield return StartFarm(s => session = s);
            var ui = ServiceLocator.Get<IUiService>();

            SetClock(session, 21, 50);
            yield return null;
            yield return null;
            Assert.IsFalse(ui.AnyModalOpen);
            Assert.IsFalse(MeterShown(), "no fatigue meter before 22:00");

            session.Clock.AdvanceMinutes(10);   // 22:00
            yield return null;
            yield return null;
            Assert.IsTrue(ui.AnyModalOpen, "the warning is shown");
            Assert.IsTrue(session.HasFlag(FatigueModel.WarnedFlag));
            StringAssert.Contains("grow tired", UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None)
                .First(t => t.gameObject.activeInHierarchy && t.text.Contains("getting late")).text);

            Press(_keyboard.escapeKey);
            yield return null;
            Release(_keyboard.escapeKey);
            yield return null;
            yield return null;
            Assert.IsFalse(ui.AnyModalOpen, "Esc dismisses it");

            session.Clock.AdvanceMinutes(60);   // 23:00
            yield return null;
            yield return null;
            Assert.IsFalse(ui.AnyModalOpen, "the warning is not repeated");
            Assert.IsTrue(MeterShown(), "the meter shows while fatigue is above zero");
            Assert.Greater(session.FatigueRating, 0.1f);
        }

        [UnityTest]
        public IEnumerator ASaveAlreadyWarned_NeverSeesTheMessageAgain()
        {
            GameSession session = null;
            yield return StartFarm(s => session = s);
            session.SetFlag(FatigueModel.WarnedFlag);
            SetClock(session, 21, 50);
            session.Clock.AdvanceMinutes(30);
            yield return null;
            yield return null;
            Assert.IsFalse(ServiceLocator.Get<IUiService>().AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator Luck_FallsAsTheNightGoesOn_AndRecoversAfterABedSleep()
        {
            GameSession session = null;
            yield return StartFarm(s => session = s);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Hooks.AddLuckModifier(new TestLuck(0.8f));

            SetClock(session, 20);
            Assert.AreEqual(0.8f, session.Luck, 0.001f);
            SetClock(session, 26);
            Assert.AreEqual(0.6f, session.Luck, 0.001f);

            SetClock(session, 26);
            session.EndDay(false);
            Assert.AreEqual(0.8f, session.Luck, 0.001f, "a night in bed clears the fatigue");
        }

        sealed class TestLuck : ILuckModifier
        {
            readonly float _value;
            public TestLuck(float value) { _value = value; }
            public int Order => 0;
            public float Modify(float luck, GameState state) => _value;
        }

        [UnityTest]
        public IEnumerator StayingUpUntilDawn_CollapsesTheFarmer_WhoWakesExhaustedInBed()
        {
            GameSession session = null;
            yield return StartFarm(s => session = s);
            session.SetFlag(FatigueModel.WarnedFlag);
            var ui = ServiceLocator.Get<IUiService>();
            session.State.Energy = 100;
            session.State.Gold = 1000;

            SetClock(session, 29, 50);
            session.Clock.AdvanceMinutes(10);   // 06:00
            var start = Time.realtimeSinceStartup;
            while (!ui.AnyModalOpen && Time.realtimeSinceStartup - start < 10f) yield return null;
            Assert.IsTrue(session.IsSleeping, "collapsing starts the sleep flow");
            Assert.IsTrue(ui.AnyModalOpen, "the day summary is shown");

            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(b => b.gameObject.activeInHierarchy && b.name == L.Get("ui.continue")).onClick.Invoke();
            start = Time.realtimeSinceStartup;
            while (!(!session.IsSleeping && SceneManager.GetActiveScene().name == MapIds.FarmHouse) && Time.realtimeSinceStartup - start < 10f)
                yield return null;

            Assert.AreEqual(MapIds.FarmHouse, SceneManager.GetActiveScene().name, "carried home");
            Assert.AreEqual(950, session.State.Gold, "a collapse costs gold");
            Assert.AreEqual(100, session.State.Energy, "nothing recovered after a whole night awake");
            Assert.AreEqual(1f, session.State.FatigueCarried, 0.001f);
            Assert.AreEqual(1f, session.FatigueRating, 0.001f, "still exhausted in the morning");
        }
    }
}
