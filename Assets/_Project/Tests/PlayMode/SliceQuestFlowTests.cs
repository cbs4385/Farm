using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // A personal quest in the real game, start to finish: Wren asks (the player accepts with Enter), the reminder plays while the items
    // are missing, and with three mushrooms in the pack the turn-in finishes the quest and pays gold and friendship.
    public class SliceQuestFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-slicequest-" + Guid.NewGuid().ToString("N"));
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

        static IUiService Ui => ServiceLocator.Get<IUiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        IEnumerator Talk()
        {
            Assert.IsTrue(NpcInteractions.Talk(_session, _session.Npcs.Get("wren")));
            var end = Time.realtimeSinceStartup + 20f;
            for (var i = 0; i < 6; i++) yield return null;
            while (Ui.AnyModalOpen && Time.realtimeSinceStartup < end) yield return Tap(Key.Enter);
            for (var i = 0; i < 4; i++) yield return null;
        }

        string Quest(string id) => _session.State.Quests.TryGetValue(id, out var q) ? q.Status : "new";

        [UnityTest]
        public IEnumerator WrensFirstQuest_OfferedAcceptedRemindedAndPaid()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var settings = ServiceLocator.Get<SettingsStore>().Current;
            settings.DialogueSpeed = DialoguePacing.InstantSpeed;
            settings.ChatMenu = false;
            _session.SetFlag("met.wren");
            _session.State.EventsSeen.Add("wren_heart2");
            NpcInteractions.AddPoints(_session, "wren", 2 * FriendshipModel.PointsPerHeart);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            Assert.AreEqual("new", Quest("wren_stew"));
            yield return Talk();                                   // the ask; Enter takes the first choice: accept
            Assert.AreEqual("active", Quest("wren_stew"), "accepted");

            yield return Talk();                                   // the reminder: items are still missing
            Assert.AreEqual("active", Quest("wren_stew"), "still active");

            _session.GiveItem("forage.mushroom", 3);
            var gold = _session.State.Gold;
            var points = NpcInteractions.StateOf(_session.State, "wren").Points;
            yield return Talk();                                   // the turn-in
            Assert.AreEqual("done", Quest("wren_stew"), "finished");
            Assert.AreEqual(gold + 120, _session.State.Gold, "paid");
            Assert.GreaterOrEqual(NpcInteractions.StateOf(_session.State, "wren").Points - points, 40, "friendship");
            Assert.AreEqual(0, ((IGameQuery)_session.World).ItemCount("forage.mushroom"), "the mushrooms were taken");
        }
    }
}
