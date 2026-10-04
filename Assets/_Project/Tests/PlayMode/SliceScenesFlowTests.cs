using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The QA walkthrough for the Phase 2 slice (docs/narrative/QA_HEART_EVENTS.md), automated: every heart event and friend-day
    // scene of Wren, Hazel and Bram plays start to finish in the real game on its own map, taking every choice with real key
    // presses, and must end cleanly (bars gone, props gone, scene spent), award its friendship and set its choice flag.
    public class SliceScenesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-slicescenes-" + Guid.NewGuid().ToString("N"));
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

        static IEnumerator WaitForScene(string name, float seconds = 20f)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < end) yield return null;
        }

        IEnumerator StartGame()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.DialogueSpeed = DialoguePacing.InstantSpeed;
            ServiceLocator.Get<SettingsStore>().Current.ChatMenu = false;
        }

        // Plays one scene on its map. `choiceKey` is the number key pressed whenever a choice list is up (0 = none).
        // Returns the friendship points gained.
        IEnumerator Play(string npc, string map, string eventId, int choiceKey, Action<int> gained)
        {
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
            var loader = ServiceLocator.Get<SceneLoader>();
            var settle = Time.realtimeSinceStartup + 10f;
            while (loader.IsLoading && Time.realtimeSinceStartup < settle) yield return null;

            _session.State.EventsSeen.Remove(eventId);
            var before = NpcInteractions.StateOf(_session.State, npc).Points;
            var finished = false;
            ServiceLocator.Get<EventBus>().Subscribe<EventFinished>(e => { if (e.EventId == eventId) finished = true; });
            Assert.IsTrue(EventRunner.Trigger(_session, eventId), eventId);

            var end = Time.realtimeSinceStartup + 90f;
            while (!finished && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen)
                {
                    if (choiceKey > 0) yield return Tap(Key.Digit1 + choiceKey - 1);
                    yield return Tap(Key.Enter);
                }
                else yield return null;
            }
            Assert.IsTrue(finished, eventId + " ran to its end");
            Assert.IsTrue(_session.State.EventsSeen.Contains(eventId), eventId + " is spent");
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsFalse(((UiService)Ui).LetterboxOn, eventId + ": the bars are gone");
            Assert.IsNull(GameObject.Find("Prop_heirloom"), eventId);
            Assert.IsEmpty(_session.PendingEvents, eventId + ": nothing left queued");
            gained(NpcInteractions.StateOf(_session.State, npc).Points - before);
        }

        // (npc, map, scene, choice dialogue key or null, choice names in key order, minimum points)
        static readonly object[][] Scenes =
        {
            new object[] { "wren", MapIds.Saloon, "wren_heart4", "wren.openmic", new[] { "cheer", "heckle", "hide" }, 50 },
            new object[] { "wren", MapIds.Saloon, "wren_heart6", "wren.kitchen", new[] { "pepper", "cider", "plain" }, 60 },
            new object[] { "wren", MapIds.Saloon, "wren_heart8", "wren.bag", new[] { "go", "stay", "listen" }, 70 },
            new object[] { "wren", MapIds.Saloon, "wren_heart10", null, null, 80 },
            new object[] { "wren", MapIds.Saloon, "wren_friend", null, null, 15 },
            new object[] { "hazel", MapIds.Library, "hazel_heart4", "hazel.swap", new[] { "book", "recipe", "honest" }, 50 },
            new object[] { "hazel", MapIds.Library, "hazel_heart6", "hazel.hiding", new[] { "ask", "wait", "joke" }, 60 },
            new object[] { "hazel", MapIds.Library, "hazel_heart8", "hazel.rehearsal", new[] { "praise", "critique", "laugh" }, 70 },
            new object[] { "hazel", MapIds.Library, "hazel_heart10", null, null, 80 },
            new object[] { "bram", MapIds.Blacksmith, "bram_heart4", "bram.repair", new[] { "hold", "hammer", "watch" }, 50 },
            new object[] { "bram", MapIds.Blacksmith, "bram_heart6", "bram.long", new[] { "nod", "glad", "word" }, 60 },
            new object[] { "bram", MapIds.Blacksmith, "bram_heart8", "bram.juno", new[] { "praise", "tease", "ask" }, 70 },
            new object[] { "bram", MapIds.Blacksmith, "bram_heart10", null, null, 80 },
            new object[] { "bram", MapIds.Blacksmith, "bram_friend", null, null, 15 },
        };

        IEnumerator RunVillager(string villager)
        {
            yield return StartGame();
            foreach (var s in Scenes.Where(x => (string)x[0] == villager))
            {
                var npc = (string)s[0]; var map = (string)s[1]; var id = (string)s[2];
                var dialogue = (string)s[3]; var names = (string[])s[4]; var minimum = (int)s[5];
                var branches = names == null ? new[] { 0 } : new[] { 1, 2, 3 };
                foreach (var branch in branches)
                {
                    var label = names == null ? id : $"{id}/{names[branch - 1]}";
                    var points = 0;
                    yield return Play(npc, map, id, branch, p => points = p);
                    Assert.GreaterOrEqual(points, minimum, label + ": friendship");
                    if (names != null)
                    {
                        var key = $"choice.{dialogue}.{names[branch - 1]}";
                        Assert.IsTrue(_session.HasFlag(key), label + ": sets " + key);
                        _session.SetFlag(key, false);
                    }
                }
            }
        }

        [UnityTest] public IEnumerator Wren_AllScenesAndEveryChoice_PlayThrough() { yield return RunVillager("wren"); }
        [UnityTest] public IEnumerator Hazel_AllScenesAndEveryChoice_PlayThrough() { yield return RunVillager("hazel"); }
        [UnityTest] public IEnumerator Bram_AllScenesAndEveryChoice_PlayThrough() { yield return RunVillager("bram"); }

        [UnityTest]
        public IEnumerator HazelsOverdue_SucceedsWithBlackberries_AndIsOfferedAgainWithout()
        {
            yield return StartGame();
            var op = SceneManager.LoadSceneAsync(MapIds.Library);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
            Assert.IsTrue(EventRunner.Trigger(_session, "hazel_friend"));
            var end = Time.realtimeSinceStartup + 40f;
            while (_session.PendingEvents.Count > 0 && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null;
            }
            for (var i = 0; i < 40; i++) { if (Ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null; }
            Assert.IsFalse(_session.HasFlag("event.hazel_friend"), "without blackberries the scene is not spent");

            _session.GiveItem("forage.blackberry", 1);
            Assert.IsTrue(EventRunner.Trigger(_session, "hazel_friend"));
            end = Time.realtimeSinceStartup + 40f;
            while (!_session.HasFlag("event.hazel_friend") && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null;
            }
            Assert.IsTrue(_session.HasFlag("event.hazel_friend"), "with blackberries it succeeds");
        }
    }
}
