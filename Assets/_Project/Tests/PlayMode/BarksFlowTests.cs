using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-125 in the real game: in the saloon at an open hour a villager near the player speaks a bubble, and the option turns it off.
    public class BarksFlowTests : PlayModeFixture
    {
        GameSession _session;

        [SetUp]
        public void SetUpMore()
        {
            BarkDirector.ResetForTests();
        }

        IEnumerator Enter(bool barks)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.Barks = barks;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, 14 * 60));      // a Friday afternoon: the saloon is open
            var op = SceneManager.LoadSceneAsync(MapIds.Saloon);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AVillagerNearby_SpeaksABubble()
        {
            yield return Enter(true);
            var end = Time.realtimeSinceStartup + 60f;
            while (BarkDirector.Spoken == 0 && Time.realtimeSinceStartup < end) yield return null;
            Assert.Greater(BarkDirector.Spoken, 0, "a bark was spoken");
            Assert.IsFalse(string.IsNullOrWhiteSpace(BarkDirector.LastText));
            Assert.IsNotNull(GameObject.Find("Bark"), "the bubble is on screen");
            Assert.IsFalse(BarkDirector.LastText.Contains("{"), "no raw markup");
            for (var i = 0; i < 20; i++) yield return null;
            Directory.CreateDirectory("Builds");
            ScreenCapture.CaptureScreenshot("Builds/bark_bubble.png");     // an Editor render for a person to look at
            for (var i = 0; i < 10; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator WithBarksOff_NobodySpeaks()
        {
            yield return Enter(false);
            var end = Time.realtimeSinceStartup + 25f;
            while (Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(0, BarkDirector.Spoken);
            Assert.IsNull(GameObject.Find("Bark"));
        }
    }
}
