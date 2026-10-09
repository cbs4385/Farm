using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The opening quest in the real game: Elara arrives at the farmhouse on the second morning of the first spring, asks the player for three dandelions and three wild
    // garlic, takes them when they are brought, and (with the horror on) the first ritual then lacks its fixed offering and fails.
    public class ElaraGardenFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-elara-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
        }

        // Starts a new game on the farm at `minute` of the second day of the first spring.
        IEnumerator StartAt(int minute)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 2, minute));
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        bool Seen => _session.State.EventsSeen.Contains("elara_garden_visit");

        [UnityTest]
        public IEnumerator ElaraArrivesAtEight_AsksForThePlants_AndTakesThemWhenTheyAreBrought()
        {
            yield return StartAt(7 * 60 + 30);
            yield return new WaitForSeconds(1f);
            Assert.IsFalse(Seen, "not before eight");

            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 2, 8 * 60));       // the clock reaches eight while the player is out on the farm
            var waited = 0f;
            while (!Seen && waited < 15f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsTrue(Seen, "Elara's visit played");

            var ui = ServiceLocator.Get<IUiService>();
            for (var i = 0; i < 90 && !_session.State.Quests.ContainsKey("elara_garden"); i++)     // read on and accept: the first choice is the default one
            {
                yield return Tap(_keyboard.enterKey);
                yield return new WaitForSeconds(0.25f);
            }
            Assert.IsTrue(_session.State.Quests.ContainsKey("elara_garden"), "the player accepted the quest");
            Assert.AreEqual(QuestStatus.Active, _session.State.Quests["elara_garden"].Status);
            for (var i = 0; i < 30 && ui.AnyModalOpen; i++) { yield return Tap(_keyboard.enterKey); yield return new WaitForSeconds(0.2f); }
            Assert.IsTrue(_session.HasFlag("elara.garden.visited"));
            Assert.IsTrue(_session.HasFlag("met.elara"), "she introduced herself: the player has met her");
            Assert.IsTrue(_session.State.Npcs["elara"].Met);

            // Bringing the plants and talking to her turns the quest in.
            var gold = _session.State.Gold;
            _session.Backpack.Add("forage.dandelion", 3);
            _session.Backpack.Add("forage.wildgarlic", 3);
            var elara = _session.Npcs.Get("elara");
            NpcInteractions.Talk(_session, elara);
            for (var i = 0; i < 30 && _session.State.Quests["elara_garden"].Status != QuestStatus.Done; i++) { yield return Tap(_keyboard.enterKey); yield return new WaitForSeconds(0.2f); }
            Assert.AreEqual(QuestStatus.Done, _session.State.Quests["elara_garden"].Status, "she took the plants");
            Assert.AreEqual(gold + 150, _session.State.Gold, "and paid for them");
            Assert.AreEqual(0, _session.Backpack.Count("forage.wildgarlic"), "the wild garlic is hers now");
            Assert.AreEqual(0, _session.Backpack.Count("forage.dandelion"));
            Assert.IsTrue(RitualDirector.IntroDelivered(_session), "the horror layer's fixed offering is gone");
        }

        [UnityTest]
        public IEnumerator ElaraWaitsAtTheDoor_UntilThePlayerComesOutOfTheHouse()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 2, 9 * 60));
            _session.State.CurrentMap = MapIds.FarmHouse;
            var op = SceneManager.LoadSceneAsync(MapIds.FarmHouse);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsFalse(Seen, "the player is indoors: the scene is on the farm");

            var load = SceneManager.LoadSceneAsync(MapIds.Farm);                            // stepping out
            while (!load.isDone) yield return null;
            var waited = 0f;
            while (!Seen && waited < 15f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsTrue(Seen, "she is at the door when the player comes out");
            for (var i = 0; i < 90 && EventDirector.Current != null && EventDirector.Current.IsPlaying; i++) { yield return Tap(_keyboard.enterKey); yield return new WaitForSeconds(0.2f); }   // let the scene end
        }

        [UnityTest]
        public IEnumerator TheNextDay_ThereIsNoVisit()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 9 * 60));
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            yield return new WaitForSeconds(2f);
            Assert.IsFalse(Seen, "only the second morning");
        }
    }
}
