using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The coach card above the hotbar, in the real game: it tells a new player to till, moves on by itself when they do, and the Skip button removes it for good.
    public class TutorialCoachFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-coach-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator TheCard_ShowsTheFirstTip_MovesOnWhenItIsDone_AndSkipRemovesIt()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = GameContext.Current.Session;
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 30; i++) yield return null;

            var card = Resources.FindObjectsOfTypeAll<Image>().First(i => i.name == "TutorialCoach" && i.gameObject.scene.IsValid());
            var text = card.transform.Find("CoachText").GetComponent<TextMeshProUGUI>();
            Assert.IsTrue(card.gameObject.activeSelf, "a new player sees the first tip");
            StringAssert.Contains("hoe", text.text);

            s.AddVar(QuestLog.Stats.Tilled, 3);
            for (var i = 0; i < 30; i++) yield return null;
            StringAssert.Contains("seed", text.text, "it moved on when the tiles were tilled");

            card.transform.Find("CoachSkip").GetComponent<Button>().onClick.Invoke();
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsFalse(card.gameObject.activeSelf, "skipped");
            Assert.IsTrue(s.HasFlag(TutorialCoach.SkipFlag), "and it stays skipped (saved with the game)");
        }
    }
}
