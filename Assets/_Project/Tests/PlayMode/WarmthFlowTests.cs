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
    // T-108 in the real game: after a week away a villager greets the player with their "missed you" line and the return is worth a bonus.
    public class WarmthFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-warmth-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        IEnumerator Talk(string npc)
        {
            Assert.IsTrue(NpcInteractions.Talk(_session, _session.Npcs.Get(npc)));
            var end = Time.realtimeSinceStartup + 20f;
            for (var i = 0; i < 6; i++) yield return null;
            while (ServiceLocator.Get<IUiService>().AnyModalOpen && Time.realtimeSinceStartup < end) yield return Tap(Key.Enter);
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AfterAWeekAway_TheVillagerSaysTheyMissedYou_OnceAndPaysTheBonus()
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
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            _session.Clock.SetTime(new GameDateTime(1, Season.Summer, 10, 12 * 60));            // far enough from day 0 for a week-old contact
            var state = NpcInteractions.StateOf(_session.State, "wren");
            state.Met = true;
            _session.SetFlag("met.wren");
            NpcInteractions.AddPoints(_session, "wren", 3 * FriendshipModel.PointsPerHeart);
            state.LastContactDay = _session.Clock.Now.TotalDays - 8;
            state.TalkedToday = false;
            var before = state.Points;

            yield return Talk("wren");
            Assert.AreEqual(before + FriendshipModel.TalkPoints + WarmthModel.ReturnBonusPoints, state.Points, $"the talk and the return bonus (was {before}, now {state.Points})");
            Assert.IsTrue(ServiceLocator.Get<GameSession>().Story.Dialogue(WarmthModel.DialogueId("wren")) != null);

            var after = state.Points;
            yield return Talk("wren");                                         // the same day: no second greeting, no second bonus
            Assert.AreEqual(after, state.Points, "no second bonus");
        }
    }
}
