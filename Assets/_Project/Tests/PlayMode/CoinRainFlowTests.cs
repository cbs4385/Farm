using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Owner, 2026-10-10: gaining gold rains coins into the counter at the top right; losing gold sends coins out of it.
    public class CoinRainFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-coinrain-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [UnityTest]
        public IEnumerator Gold_GainedRainsCoinsIn_AndGoldLostSendsThemOut_AndNothingChangesWhenTheGoldDoesNot()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = ServiceLocator.Get<GameSession>();
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            s.SetFlag(TutorialCoach.SkipFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 30; i++) yield return null;

            var rain = ServiceLocator.Get<UiService>().Hud.Coins;
            Assert.IsNotNull(rain);
            for (var i = 0; i < 10; i++) yield return null;
            Assert.AreEqual(0, rain.Active, "no coins while the gold is steady");

            s.ChangeGold(120);
            for (var i = 0; i < 5; i++) yield return null;
            Assert.Greater(rain.Active, 0, "a gain starts a rain");
            yield return new WaitForSecondsRealtime(2.5f);
            Assert.AreEqual(0, rain.Active, "it ends");
            Assert.Greater(rain.Arrived, 0, "the coins landed in the counter");
            var landed = rain.Arrived;

            s.ChangeGold(-60);
            for (var i = 0; i < 5; i++) yield return null;
            Assert.Greater(rain.Active, 0, "a loss sends coins out");
            yield return new WaitForSecondsRealtime(2.5f);
            Assert.AreEqual(0, rain.Active);
            Assert.AreEqual(landed, rain.Arrived, "coins leaving the counter do not land in it");
        }
    }
}
