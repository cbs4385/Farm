using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-10: "when you click save game, there is no pop up or anything". The pause menu stays open and its button says the game is saved.
    public class PauseSaveFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-pausesave-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator SavingFromThePauseMenu_SavesTheGame_KeepsTheMenuOpen_AndSaysSo()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var ui = GameContext.Current.Ui;
            ui.ShowPause();
            yield return null;
            var label = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().First(t => t.text == L.Get("pause.save") && t.gameObject.scene.IsValid());
            label.GetComponentInParent<Button>().onClick.Invoke();
            yield return null;

            Assert.AreEqual(L.Get("pause.saved"), label.text, "the button says it is saved");
            Assert.IsTrue(ui.AnyModalOpen, "the menu stays open");
            Assert.IsTrue(GameContext.Current.Saves.Exists(0), "and the file is on the disk");
        }
    }
}
