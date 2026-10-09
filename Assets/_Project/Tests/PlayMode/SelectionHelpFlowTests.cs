using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest chat 2026-10-09: a player on a controller could select tools but had no idea how to use them, because the help appeared only when the mouse rested on
    // a slot. Now the picked item's help shows above the item bar whenever the pick changes, with no mouse involved.
    public class SelectionHelpFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-selhelp-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static TextMeshProUGUI Help()
        {
            var panel = Resources.FindObjectsOfTypeAll<RectTransform>().FirstOrDefault(r => r.name == "SelectionHelp" && r.gameObject.scene.IsValid());
            return panel == null || !panel.gameObject.activeInHierarchy ? null : panel.GetComponentInChildren<TextMeshProUGUI>();
        }

        [UnityTest]
        public IEnumerator ThePickedTool_IsExplainedAboveTheItemBar_WithoutTheMouse()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            session.State.SelectedHotbar = Enumerable.Range(0, session.Backpack.Capacity).First(i => session.Backpack.Get(i)?.ItemId == ItemIds.Hoe);
            for (var i = 0; i < 5; i++) yield return null;
            var help = Help();
            Assert.IsNotNull(help, "the help is on screen");
            StringAssert.Contains("tilled soil", help.text);

            session.State.SelectedHotbar = Enumerable.Range(0, session.Backpack.Capacity).First(i => session.Backpack.Get(i)?.ItemId == ItemIds.WateringCan);
            for (var i = 0; i < 5; i++) yield return null;
            StringAssert.Contains("Face tilled soil", Help().text, "it follows the pick");
        }

        // The quest tracker (0.2.0 shipped without it being built: nothing called it).
        [UnityTest]
        public IEnumerator TheQuestTracker_ListsTheOpeningQuest_OnScreen()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 40; i++) yield return null;
            var tracker = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().FirstOrDefault(t => t.name == "QuestTracker" && t.gameObject.scene.IsValid());
            Assert.IsNotNull(tracker, "the tracker is built");
            StringAssert.Contains("hoe", tracker.text);
        }
    }
}
