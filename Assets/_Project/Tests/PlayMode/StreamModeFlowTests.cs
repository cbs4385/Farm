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
    // T-145 in the real game: the content badge, and the choice timer for a chat that votes.
    public class StreamModeFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        SettingsData _settings;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-streamtests-" + Guid.NewGuid().ToString("N"));
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

        const string TestStory = @"{ ""dialogues"": [
          { ""id"": ""test.vote"", ""start"": ""n0"", ""nodes"": [
            { ""id"": ""n0"", ""speaker"": ""tilda"", ""text"": ""dlg.test.vote.0"", ""choices"": [
              { ""text"": ""dlg.test.vote.c0"", ""effects"": [ ""flag:test.first"" ] },
              { ""text"": ""dlg.test.vote.c1"", ""effects"": [ ""flag:test.second"" ] },
              { ""text"": ""dlg.test.vote.c2"", ""default"": true, ""effects"": [ ""flag:test.third"" ] } ] } ] },
          { ""id"": ""test.single"", ""start"": ""n0"", ""nodes"": [
            { ""id"": ""n0"", ""speaker"": ""tilda"", ""text"": ""dlg.test.vote.0"", ""choices"": [ { ""text"": ""dlg.test.vote.c0"", ""effects"": [ ""flag:test.only"" ] } ] } ] } ] }";

        IEnumerator Begin(int timer, bool stream = false)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            L.AddTable("en", new Dictionary<string, string>
            {
                { "dlg.test.vote.0", "Which way?" }, { "dlg.test.vote.c0", "Left" }, { "dlg.test.vote.c1", "Right" }, { "dlg.test.vote.c2", "Straight on" },
            });
            Assert.IsTrue(_session.Story.AddJson(TestStory, "test"));
            _settings = ServiceLocator.Get<SettingsStore>().Current;
            _settings.DialogueSpeed = DialoguePacing.InstantSpeed;
            _settings.ChoiceTimer = timer;
            _settings.StreamMode = stream;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        static IUiService Ui => ServiceLocator.Get<IUiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        static TextMeshProUGUI Label(string contains) =>
            UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.activeInHierarchy && t.text.Contains(contains));

        static IEnumerator WaitUntil(Func<bool> condition, float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
        }

        [UnityTest]
        public IEnumerator WhenTheTimerRunsOut_TheDefaultChoiceIsTaken()
        {
            yield return Begin(timer: 1);
            _session.BeginDialogue("test.vote");
            yield return WaitUntil(() => Label("Chat vote") != null, 3f);
            Assert.IsNotNull(Label("Chat vote"), "the countdown is on screen");
            Assert.IsNotNull(Label("Straight on"));
            yield return WaitUntil(() => !Ui.AnyModalOpen, 6f);
            Assert.IsFalse(Ui.AnyModalOpen, "the conversation ended by itself");
            Assert.IsTrue(_session.HasFlag("test.third"), "the choice marked default was taken");
            Assert.IsFalse(_session.HasFlag("test.first") || _session.HasFlag("test.second"));
        }

        [UnityTest]
        public IEnumerator APlayerWhoChoosesInTime_BeatsTheTimer()
        {
            yield return Begin(timer: 3);
            _session.BeginDialogue("test.vote");
            yield return WaitUntil(() => Label("Straight on") != null, 3f);
            yield return Tap(Key.Digit2);
            for (var i = 0; i < 4; i++) yield return null;
            Assert.IsTrue(_session.HasFlag("test.second"), "key 2 picked the second choice");
            Assert.IsFalse(_session.HasFlag("test.third"));
            yield return new WaitForSecondsRealtime(3.5f);
            Assert.IsFalse(_session.HasFlag("test.third"), "the stopped timer does not fire later");
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator WithoutATimer_TheChoiceWaitsForever()
        {
            yield return Begin(timer: 0);
            _session.BeginDialogue("test.vote");
            yield return WaitUntil(() => Label("Straight on") != null, 3f);
            Assert.IsNull(Label("Chat vote"), "no countdown");
            yield return new WaitForSecondsRealtime(2f);
            Assert.IsTrue(Ui.AnyModalOpen, "still waiting for the player");
            Assert.IsFalse(_session.HasFlag("test.third"));
        }

        [UnityTest]
        public IEnumerator ALoneOption_HasNoTimer_BecauseThereIsNothingToVoteOn()
        {
            yield return Begin(timer: 1);
            _session.BeginDialogue("test.single");
            yield return WaitUntil(() => Label("Left") != null, 3f);
            Assert.IsNull(Label("Chat vote"));
            yield return new WaitForSecondsRealtime(2f);
            Assert.IsTrue(Ui.AnyModalOpen);
            Assert.IsFalse(_session.HasFlag("test.only"));
        }

        // True when a panel's four corners are on the screen (a pixel of slack).
        static bool OnScreen(RectTransform panel)
        {
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners);
            return corners.All(c => c.x >= -1f && c.y >= -1f && c.x <= Screen.width + 1f && c.y <= Screen.height + 1f);
        }

        static RectTransform ActiveFrame() =>
            UnityEngine.Object.FindObjectsByType<RectTransform>().FirstOrDefault(r => r.name == "Frame" && r.gameObject.activeInHierarchy);

        [UnityTest]
        public IEnumerator AtAVeryLargeUiSize_TheDialogueBoxAndMenusStayOnTheScreen()
        {
            yield return Begin(timer: 0);
            Ui.ApplyUiScale(1.5f);                                  // the biggest "UI size" option
            for (var i = 0; i < 4; i++) yield return null;

            _session.BeginDialogue("test.vote");
            yield return WaitUntil(() => Label("Straight on") != null, 3f);
            for (var i = 0; i < 3; i++) yield return null;
            var box = ActiveFrame();
            Assert.IsNotNull(box, "the dialogue box is up");
            Assert.IsTrue(OnScreen(box), "the dialogue box fits the screen at the largest UI size");
            yield return Tap(Key.Digit1);
            for (var i = 0; i < 4; i++) yield return null;

            Ui.ShowGameMenu(MenuTabs.Skills);
            for (var i = 0; i < 4; i++) yield return null;
            var menu = ActiveFrame();
            Assert.IsNotNull(menu, "the game menu is open");
            Assert.IsTrue(OnScreen(menu), "the 900 by 500 game menu fits the screen at the largest UI size");
        }

        [UnityTest]
        public IEnumerator StreamMode_ShowsTheContentBadge_AndHidesItWhenOff()
        {
            yield return Begin(timer: 0, stream: true);
            yield return WaitUntil(() => Label("Content:") != null, 3f);
            var badge = Label("Content:");
            Assert.IsNotNull(badge, "the badge is on screen in stream mode");
            StringAssert.Contains("Full", badge.text, "the default horror level is full");

            _settings.HorrorLevel = 0;
            yield return WaitUntil(() => Label("Content: Off") != null, 3f);
            Assert.IsNotNull(Label("Content: Off"), "the badge follows the setting");

            _settings.StreamMode = false;
            yield return WaitUntil(() => Label("Content:") == null, 3f);
            Assert.IsNull(Label("Content:"), "no badge when stream mode is off");
        }
    }
}
