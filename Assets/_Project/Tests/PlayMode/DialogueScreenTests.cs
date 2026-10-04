using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-096 in the real game: the dialogue box with simulated keys (number-key choices, the log, text speed,
    // auto-advance, markup, emotes and tone tags).
    public class DialogueScreenTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-dlgtests-" + Guid.NewGuid().ToString("N"));
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

        const string TestStory = @"{ ""dialogues"": [ { ""id"": ""test.markup"", ""start"": ""n0"", ""nodes"": [
            { ""id"": ""n0"", ""speaker"": ""tilda"", ""text"": ""dlg.test.markup.0"", ""expression"": ""happy"", ""emote"": ""heart"", ""next"": ""n1"" },
            { ""id"": ""n1"", ""speaker"": ""tilda"", ""text"": ""dlg.test.markup.1"", ""choices"": [
                { ""text"": ""dlg.test.markup.c0"", ""tone"": ""kind"", ""effects"": [ ""flag:test.kind"" ] },
                { ""text"": ""dlg.test.markup.c1"", ""tone"": ""playful"", ""effects"": [ ""flag:test.playful"" ] } ] } ] } ] }";

        IEnumerator Begin(int speed = DialoguePacing.DefaultSpeed, bool auto = false)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            L.AddTable("en", new Dictionary<string, string>
            {
                { "dlg.test.markup.0", "Hello{pause=0.2}, <b>friend</b>." },
                { "dlg.test.markup.1", "Pick one." },
                { "dlg.test.markup.c0", "Be kind" },
                { "dlg.test.markup.c1", "Be playful" },
            });
            Assert.IsTrue(_session.Story.AddJson(TestStory, "test"));
            var settings = ServiceLocator.Get<SettingsStore>().Current;
            settings.DialogueSpeed = speed;
            settings.AutoAdvance = auto;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static IUiService Ui => ServiceLocator.Get<IUiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        static GameObject Find(string name) => GameObject.Find(name);
        static bool ChoicesShown => Find("Choice0") != null;

        // Presses Enter until the choice buttons are on screen.
        IEnumerator ReachTheChoices()
        {
            for (var i = 0; i < 30 && !ChoicesShown; i++) yield return Tap(Key.Enter);
            Assert.IsTrue(ChoicesShown, "the choices appear");
        }

        static TextMeshProUGUI Label(string contains) =>
            UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.activeInHierarchy && t.text.Contains(contains));

        [UnityTest]
        public IEnumerator MarkupIsProcessed_TheEmoteAppears_AndChoicesAreNumberedWithTones()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            Assert.IsTrue(_session.BeginDialogue("test.markup"));
            for (var i = 0; i < 3; i++) yield return null;
            var line = Label("friend");
            Assert.IsNotNull(line, "the line is on screen");
            Assert.AreEqual("Hello, <b>friend</b>.", line.text, "{pause} is not shown as text");
            var bubble = Find("Emote");
            Assert.IsTrue(bubble != null && bubble.activeInHierarchy, "the heart emote shows");
            Assert.IsNotNull(Label("<3"));

            yield return ReachTheChoices();
            var first = Label("Be kind");
            var second = Label("Be playful");
            StringAssert.Contains("1. Be kind", first.text);
            StringAssert.Contains("(kind)", first.text);
            StringAssert.Contains("2. Be playful", second.text);
            StringAssert.Contains("(playful)", second.text);
        }

        [UnityTest]
        public IEnumerator ANumberKey_PicksThatChoice()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            yield return ReachTheChoices();
            yield return Tap(Key.Digit2);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(_session.HasFlag("test.playful"), "key 2 picked the second choice");
            Assert.IsFalse(_session.HasFlag("test.kind"));
            Assert.IsFalse(Ui.AnyModalOpen, "the conversation ended");
        }

        [UnityTest]
        public IEnumerator TheNumpadWorksToo()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            yield return ReachTheChoices();
            yield return Tap(Key.Numpad1);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(_session.HasFlag("test.kind"));
        }

        [UnityTest]
        public IEnumerator ANumberWithNoSuchChoice_DoesNothing()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            yield return ReachTheChoices();
            yield return Tap(Key.Digit5);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "still waiting for a real choice");
            Assert.IsFalse(_session.HasFlag("test.kind") || _session.HasFlag("test.playful"));
        }

        [UnityTest]
        public IEnumerator TheTextSpeedSetting_ControlsWhetherAnEnterJustFinishesTheTyping()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            for (var i = 0; i < 3; i++) yield return null;
            yield return Tap(Key.Enter);                                    // instant: the whole first line was already shown, so one Enter moves on
            Assert.IsTrue(ChoicesShown, "instant text: one press reaches the choices");
            yield return Tap(Key.Digit1);
            for (var i = 0; i < 3; i++) yield return null;

            _session.BeginDialogue("test.markup");
            ServiceLocator.Get<SettingsStore>().Current.DialogueSpeed = 0;   // slow
            _session.BeginDialogue("test.markup");
            for (var i = 0; i < 3; i++) yield return null;
            yield return Tap(Key.Enter);                                    // slow: this only finishes the typing
            Assert.IsFalse(ChoicesShown, "slow text: the first press only finishes typing");
        }

        [UnityTest]
        public IEnumerator TheLog_OpensWithL_ShowsTheLines_AndClosesWithEscape_WithoutLeavingTheConversation()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            yield return Tap(Key.Enter);                                    // on to the second line: two lines are in the log
            for (var i = 0; i < 2; i++) yield return null;
            Assert.IsTrue(Find("DialogueLog") == null || !Find("DialogueLog").activeSelf, "closed to begin with");

            yield return Tap(Key.L);
            var log = GameObject.FindObjectsByType<RectTransform>().FirstOrDefault(r => r.name == "DialogueLog" && r.gameObject.activeInHierarchy);
            Assert.IsNotNull(log, "L opens the log");
            var text = string.Join("\n", log.GetComponentsInChildren<TextMeshProUGUI>().Select(t => t.text));
            StringAssert.Contains("friend", text, "the first line is in the log");
            StringAssert.Contains("Pick one.", text, "and the second");

            yield return Tap(Key.Escape);
            Assert.IsFalse(GameObject.FindObjectsByType<RectTransform>().Any(r => r.name == "DialogueLog" && r.gameObject.activeInHierarchy), "Escape closes the log");
            Assert.IsTrue(Ui.AnyModalOpen, "the conversation goes on");
            Assert.IsTrue(ChoicesShown);
        }

        [UnityTest]
        public IEnumerator TheLog_ShowsNoChoiceBeforeOneIsMade()
        {
            yield return Begin(DialoguePacing.InstantSpeed);
            _session.BeginDialogue("test.markup");
            yield return ReachTheChoices();
            // Use a second conversation step: the log only exists while the box is open, so check it before choosing.
            yield return Tap(Key.L);
            var log = GameObject.FindObjectsByType<RectTransform>().First(r => r.name == "DialogueLog" && r.gameObject.activeInHierarchy);
            Assert.IsFalse(string.Join("\n", log.GetComponentsInChildren<TextMeshProUGUI>().Select(t => t.text)).Contains("You:"), "nothing chosen yet");
            yield return Tap(Key.L);
        }

        [UnityTest]
        public IEnumerator AutoAdvance_MovesALineOnByItself_WhenSwitchedOn()
        {
            yield return Begin(DialoguePacing.InstantSpeed, auto: true);
            _session.BeginDialogue("tilda.chat1");                          // a single line
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the line is up");
            yield return new WaitForSecondsRealtime(DialoguePacing.AutoAdvanceSeconds(120) + 1.0f);
            Assert.IsFalse(Ui.AnyModalOpen, "it moved on and ended without a key press");
        }

        [UnityTest]
        public IEnumerator WithoutAutoAdvance_TheLineWaitsForThePlayer()
        {
            yield return Begin(DialoguePacing.InstantSpeed, auto: false);
            _session.BeginDialogue("tilda.chat1");
            yield return new WaitForSecondsRealtime(DialoguePacing.AutoAdvanceSeconds(120) + 1.0f);
            Assert.IsTrue(Ui.AnyModalOpen, "still waiting");
            yield return Tap(Key.Enter);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }
    }
}
