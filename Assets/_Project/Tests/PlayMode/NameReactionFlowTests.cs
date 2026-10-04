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
    // T-148 in the real game: a farm called Farmy McFarmface gets Wren's reaction the next time she is talked to, once.
    public class NameReactionFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-names-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator AJokeFarmName_IsReactedTo_ByTheVillager_Once()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Farmy McFarmface", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var settings = ServiceLocator.Get<SettingsStore>().Current;
            settings.DialogueSpeed = DialoguePacing.InstantSpeed;
            settings.ChatMenu = false;
            _session.SetFlag("met.wren");
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            var id = "wren.namefarm.farmy_mcfarmface";
            yield return Talk("wren");
            Assert.IsTrue(LineMemory.Load(_session).WasHeard("npc.wren.talk", id), "Wren reacted to the farm's name");
            Assert.AreEqual(1, LineMemory.Load(_session).Count["npc.wren.talk|" + id]);

            _session.State.EventsSeen.Add("x");                              // a later day: the reaction does not come back
            NpcInteractions.StateOf(_session.State, "wren").TalkedToday = false;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 9, 12 * 60));
            yield return Talk("wren");
            Assert.AreEqual(1, LineMemory.Load(_session).Count["npc.wren.talk|" + id], "heard once");
        }
    }
}
