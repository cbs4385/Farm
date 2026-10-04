using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-103 in the real game: every template, filled in for Wren, plays start to finish on her map (as a memory replay, so she
    // is always on stage) and leaves the world as it found it.
    public class EventTemplatesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        static readonly string[] Library = { "shared_activity", "confession", "heirloom", "shared_meal", "helping_scene", "prank", "performance" };

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-tmpltests-" + Guid.NewGuid().ToString("N"));
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

        static string K(string n) => "dlg.wren.chat" + n + ".0";

        JObject Args(string template, EventStep stage)
        {
            var args = new JObject { ["npc"] = "wren", ["map"] = "Saloon", ["px"] = stage.X, ["py"] = stage.Y, ["reward"] = new JArray("friend:wren,40") };
            switch (template)
            {
                case "shared_activity": args["intro"] = K("1"); args["doing"] = K("2"); args["wrap"] = K("3"); break;
                case "confession": args["intro"] = K("1"); args["talk"] = "wren.first"; args["after"] = K("2"); break;
                case "heirloom": args["nx"] = stage.X; args["ny"] = stage.Y + 1; args["item"] = "crop.strawberry"; args["intro"] = K("1"); args["give"] = K("2"); args["thanks"] = K("3"); break;
                case "shared_meal": args["nx"] = stage.X; args["ny"] = stage.Y + 1; args["dish"] = "food.mashed_potato"; args["intro"] = K("1"); args["bite"] = K("2"); args["wrap"] = K("3"); break;
                case "helping_scene": args["need"] = "flag:never_set"; args["ask"] = K("1"); args["success"] = K("2"); args["fallback"] = K("3"); break;
                case "prank": args["setup"] = K("1"); args["prank"] = K("2"); args["talk"] = "wren.first"; args["reaction"] = K("3"); break;
                case "performance": args["intro"] = K("1"); args["cue"] = "open_mic"; args["outro"] = K("2"); break;
            }
            return args;
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

        [UnityTest]
        public IEnumerator EveryTemplate_PlaysThrough_AndLeavesTheWorldAsItFoundIt()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.DialogueSpeed = DialoguePacing.InstantSpeed;
            var stage = _session.Story.Event("wren_heart2").Steps.First(s => s.Type == "move" && s.Actor == "player");

            var instances = new JArray(Library.Select(t => new JObject
            {
                ["id"] = "test_" + t, ["template"] = t, ["args"] = Args(t, stage),
                ["event"] = new JObject { ["titleKey"] = "memory.wren_heart2", ["condition"] = "true" },
            }));
            Assert.IsTrue(_session.Story.AddJson(new JObject { ["eventsFromTemplates"] = instances }.ToString(), "instances"), "added");
            Assert.IsEmpty(_session.Story.Errors, string.Join("\n", _session.Story.Errors));
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            var points = NpcInteractions.StateOf(_session.State, "wren").Points;
            var gold = _session.State.Gold;
            foreach (var template in Library)
            {
                var id = "test_" + template;
                _session.State.EventsSeen.Add(id);
                var loader = ServiceLocator.Get<SceneLoader>();
                var settle = Time.realtimeSinceStartup + 10f;
                while (loader.IsLoading && Time.realtimeSinceStartup < settle) yield return null;      // the fade back in from the last scene
                Assert.IsTrue(Memories.Start(_session, id), template);
                yield return WaitForScene(MapIds.Saloon);
                Assert.AreEqual(MapIds.Saloon, SceneManager.GetActiveScene().name, template + " plays on the saloon map");

                var end = Time.realtimeSinceStartup + 60f;
                while (_session.MemoryId != null && Time.realtimeSinceStartup < end)
                {
                    if (Ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null;
                }
                Assert.IsNull(_session.MemoryId, template + " finished");
                yield return WaitForScene(MapIds.Farm);
                for (var i = 0; i < 12; i++) yield return null;

                Assert.AreEqual(MapIds.Farm, SceneManager.GetActiveScene().name, template + ": back on the farm");
                Assert.IsFalse(((UiService)Ui).LetterboxOn, template + ": the bars are gone");
                Assert.IsNull(GameObject.Find("Prop_heirloom"), template);
                Assert.IsNull(GameObject.Find("Prop_meal"), template);
                Assert.AreEqual(points, NpcInteractions.StateOf(_session.State, "wren").Points, template + ": a replay awards nothing");
                Assert.AreEqual(gold, _session.State.Gold, template);
            }
        }
    }
}
