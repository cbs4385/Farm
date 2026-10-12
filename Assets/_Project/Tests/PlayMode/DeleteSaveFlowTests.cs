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
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-10: "I couldn't find a way to delete a save file". The Load Game screen has a Delete button for each saved game, which asks first.
    public class DeleteSaveFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-deletesave-" + Guid.NewGuid().ToString("N"));
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

        static Button Named(string name) =>
            Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(b => b.gameObject.name == name && b.gameObject.scene.IsValid() && b.gameObject.activeInHierarchy);

        [UnityTest]
        public IEnumerator DeletingASave_AsksFirst_ThenRemovesOnlyThatSlot_AndTheOthersStay()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            var saves = GameContext.Current.Saves;
            session.BeginNewGame("One", "Farm One", 0); session.Save();
            session.BeginNewGame("Two", "Farm Two", 1); session.Save();
            Assert.IsTrue(saves.Exists(0) && saves.Exists(1));

            var ui = ((UiService)GameContext.Current.Ui);
            var load = new LoadGameScreen(ui, null);
            load.Open();
            yield return null;
            Assert.IsFalse(Named("Delete2").interactable, "an empty slot has nothing to delete");

            Named("Delete1").onClick.Invoke();
            yield return null;
            Assert.IsTrue(saves.Exists(1), "nothing is deleted before the answer");
            var no = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().First(t => t.text == L.Get("ui.no") && t.gameObject.activeInHierarchy);
            no.GetComponentInParent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsTrue(saves.Exists(1), "No keeps it");

            Named("Delete1").onClick.Invoke();
            yield return null;
            var yes = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().First(t => t.text == L.Get("ui.yes") && t.gameObject.activeInHierarchy);
            yes.GetComponentInParent<Button>().onClick.Invoke();
            yield return null;

            Assert.IsFalse(saves.Exists(1), "Yes removes the slot");
            Assert.IsTrue(saves.Exists(0), "the other slot is untouched");
            Assert.IsFalse(Named("Delete1").interactable, "and the list shows it empty");
        }
    }
}
