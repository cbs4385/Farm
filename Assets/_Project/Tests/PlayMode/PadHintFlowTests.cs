using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-10: a player with a pad did not know that View opens the menu and the map. A line at the bottom left names the main buttons while a pad is in use.
    public class PadHintFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-padhint-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [UnityTest]
        public IEnumerator WhileAPadIsInUse_TheHudNamesTheMainButtons_AndForTheKeyboardItSaysNothing()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var hint = GameObject.Find("PadHint").GetComponent<TextMeshProUGUI>();
            Assert.IsEmpty(hint.text, "no pad in hand: no line");

            ControlPrompts.SetKind(InputKind.Gamepad);
            for (var i = 0; i < 30; i++) yield return null;
            StringAssert.Contains("View", hint.text, "the button for the menu and the map");
            StringAssert.Contains("X", hint.text);
            StringAssert.Contains("A", hint.text);

            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsEmpty(hint.text);
        }

    }
}
