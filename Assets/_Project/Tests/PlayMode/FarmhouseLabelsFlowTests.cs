using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-10: in the farmhouse, nothing said what it was when the mouse rested on it or a pad faced it. Every piece in the room has a name now.
    public class FarmhouseLabelsFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-houselabels-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator EveryPieceInTheFarmhouse_HasANameForTheHoverLabel()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.FarmHouse);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var fixtures = UnityEngine.Object.FindObjectsByType<MovableFixture>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(fixtures.Length, 15, "the room's furnishings");
            foreach (var f in fixtures)
            {
                var interactable = f.GetComponent<IInteractable>();
                var note = f.GetComponent<HoverNote>();
                var label = interactable?.HoverLabel ?? note?.HoverLabel;
                Assert.IsFalse(string.IsNullOrEmpty(label), f.name + " says what it is");
                Assert.IsFalse(label.StartsWith("hover."), f.name + ": the label text exists: " + label);
            }
        }
    }
}
