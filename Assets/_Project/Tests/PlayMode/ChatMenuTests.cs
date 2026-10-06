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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-097 and T-141 in the real game: after a chat the menu offers topics and social actions, driven with a
    // simulated keyboard.
    public class ChatMenuTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-chatmenu-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Begin(bool chatMenu = true)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var settings = ServiceLocator.Get<SettingsStore>().Current;
            settings.DialogueSpeed = DialoguePacing.InstantSpeed;
            settings.ChatMenu = chatMenu;
            _session.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
            _session.State.SetDate(_session.Clock.Now);
            _session.SetFlag("met.wren");
            NpcInteractions.StateOf(_session.State, "wren").Points = 300;       // one heart: her gossip topic is unlocked
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

        static TextMeshProUGUI Label(string contains) =>
            UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.activeInHierarchy && t.text.Contains(contains));

        // Taps Enter until a label is on screen (the one-line chat ends and the menu appears).
        IEnumerator WaitFor(string contains, int maxTaps = 12)
        {
            for (var i = 0; i < maxTaps && Label(contains) == null; i++) yield return Tap(Key.Enter);
            Assert.IsNotNull(Label(contains), $"'{contains}' appears");
        }

        // The digit shown in front of a choice ("3. Tell a joke").
        IEnumerator PickByLabel(string contains)
        {
            var label = Label(contains);
            Assert.IsNotNull(label, contains);
            var number = int.Parse(label.text.Substring(0, label.text.IndexOf('.')));
            yield return Tap(Key.Digit1 + number - 1);
            for (var i = 0; i < 3; i++) yield return null;
        }

        IEnumerator Chat()
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            NpcInteractions.Talk(_session, _session.Npcs.Get("wren"));
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AfterAChat_TheMenuAppears_AndAHabitualEnterJustSaysGoodbye()
        {
            yield return Begin();
            yield return Chat();
            yield return WaitFor("Goodbye for now");
            Assert.IsNotNull(Label("Another round"), "Wren's own prompt");
            var pointsBefore = _session.State.Npcs["wren"].Points;
            yield return Tap(Key.Enter);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen, "Enter on the highlighted Goodbye ended the conversation");
            Assert.AreEqual(pointsBefore, _session.State.Npcs["wren"].Points, "nothing was done by accident");
        }

        [UnityTest]
        public IEnumerator KeepChatting_GivesAnotherLine_AndTheMenuComesBack_SoAConversationNeverJustStops()
        {
            yield return Begin();
            yield return Chat();
            yield return WaitFor("Keep chatting");
            var pointsBefore = _session.State.Npcs["wren"].Points;
            yield return PickByLabel("Keep chatting");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "another line is on screen, not the end of the conversation");
            Assert.IsNull(Label("Keep chatting"), "it is a line, not the menu");
            yield return WaitFor("Keep chatting");                          // Enter through the line and the menu is back
            Assert.IsNotNull(Label("Goodbye for now"));
            Assert.AreEqual(pointsBefore, _session.State.Npcs["wren"].Points, "talking more does not farm friendship");
            yield return Tap(Key.Escape);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator Escape_OnTheMenu_SaysGoodbye()
        {
            yield return Begin();
            yield return Chat();
            yield return WaitFor("Goodbye for now");
            yield return Tap(Key.Escape);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator ASocialAction_CanBeDone_WithTheNumberKeys_AndEarnsFriendship()
        {
            yield return Begin();
            yield return Chat();
            yield return WaitFor("Do something together");
            var before = _session.State.Npcs["wren"].Points;
            yield return PickByLabel("Do something together");
            Assert.IsNotNull(Label("Tell a joke"), "the social submenu");
            Assert.IsNotNull(Label("Ask for advice"));
            yield return PickByLabel("Tell a joke");                          // Wren loves jokes: never a flop
            for (var i = 0; i < 12 && Ui.AnyModalOpen; i++) yield return Tap(Key.Enter);
            Assert.IsFalse(Ui.AnyModalOpen);
            Assert.Greater(_session.State.Npcs["wren"].Points, before, "a loved joke earns friendship");
            Assert.AreEqual(1, InteractionState.Load(_session).SocialToday("wren", _session.Clock.Now.TotalDays));
        }

        [UnityTest]
        public IEnumerator ATopic_Plays_AndIsRemembered()
        {
            yield return Begin();
            yield return Chat();
            yield return WaitFor("Ask about the latest gossip");
            yield return PickByLabel("Ask about the latest gossip");
            Assert.IsNotNull(Label("between us"), "Wren starts the gossip");
            for (var i = 0; i < 12 && Ui.AnyModalOpen; i++) yield return Tap(Key.Enter);
            Assert.IsFalse(Ui.AnyModalOpen, "the topic ends the conversation");
            Assert.AreEqual(1, InteractionState.Load(_session).TopicTimes["wren.gossip"]);
        }

        [UnityTest]
        public IEnumerator WithTheSettingOff_ThereIsNoMenu()
        {
            yield return Begin(chatMenu: false);
            yield return Chat();
            for (var i = 0; i < 12 && Ui.AnyModalOpen; i++) yield return Tap(Key.Enter);
            Assert.IsNull(Label("Goodbye for now"));
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator TheMenu_IsNotOfferedAfterAFirstMeeting()
        {
            yield return Begin();
            _session.State.Flags.Remove("met.wren");                           // so the introduction (a story beat) plays
            NpcInteractions.StateOf(_session.State, "wren").Points = 0;
            yield return Chat();
            for (var i = 0; i < 30 && Ui.AnyModalOpen; i++) yield return Tap(Key.Enter);
            Assert.IsNull(Label("Goodbye for now"), "no menu straight after meeting someone");
        }
    }
}
