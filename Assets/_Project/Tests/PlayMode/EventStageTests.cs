using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-100 in the real game: a scripted scene uses the new steps on a real map, and always leaves the world as it found it.
    public class EventStageTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _music = new List<string>();
        bool _finished, _finishedSkipped;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-stagetests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _music.Clear();
            _finished = _finishedSkipped = false;
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        const string Scenes = @"{ ""events"": [
          { ""id"": ""test.stage"", ""trigger"": ""manual"", ""steps"": [
            { ""type"": ""place"", ""actor"": ""tilda"", ""x"": 10, ""y"": 10 },
            { ""type"": ""letterbox"", ""name"": ""on"", ""seconds"": 0.1 },
            { ""type"": ""lighting"", ""name"": ""night"", ""seconds"": 0.2 },
            { ""type"": ""camera"", ""name"": ""focus"", ""actor"": ""tilda"", ""seconds"": 0.2 },
            { ""type"": ""spawn"", ""id"": ""gift"", ""name"": ""crop.strawberry"", ""x"": 11, ""y"": 10 },
            { ""type"": ""emote"", ""actor"": ""tilda"", ""name"": ""heart"", ""seconds"": 0.6, ""async"": true },
            { ""type"": ""anim"", ""actor"": ""tilda"", ""name"": ""hop"", ""seconds"": 0.3 },
            { ""type"": ""waitFor"", ""actor"": ""tilda"" },
            { ""type"": ""branch"", ""target"": ""b"", ""condition"": ""flag:take_b"" },
            { ""type"": ""effects"", ""effects"": [ ""flag:path_a"" ] },
            { ""type"": ""branch"", ""target"": ""end"" },
            { ""type"": ""label"", ""label"": ""b"" },
            { ""type"": ""effects"", ""effects"": [ ""flag:path_b"" ] },
            { ""type"": ""label"", ""label"": ""end"" },
            { ""type"": ""effects"", ""condition"": ""flag:never"", ""effects"": [ ""flag:never_runs"" ] },
            { ""type"": ""parallel"", ""steps"": [ { ""type"": ""move"", ""actor"": ""tilda"", ""x"": 12, ""y"": 10 }, { ""type"": ""wait"", ""seconds"": 0.2 } ] },
            { ""type"": ""sfx"", ""name"": ""coin"" },
            { ""type"": ""music"", ""name"": ""festival_theme"" },
            { ""type"": ""wait"", ""seconds"": 2.0 },
            { ""type"": ""camera"", ""name"": ""reset"", ""seconds"": 0.1 },
            { ""type"": ""despawn"", ""id"": ""gift"" },
            { ""type"": ""letterbox"", ""name"": ""off"", ""seconds"": 0.1 },
            { ""type"": ""lighting"", ""name"": ""reset"", ""seconds"": 0.1 } ] },
          { ""id"": ""test.skip"", ""trigger"": ""manual"", ""skipEffects"": [ ""flag:skip_effects"" ], ""steps"": [
            { ""type"": ""letterbox"", ""name"": ""on"", ""seconds"": 0.05 },
            { ""type"": ""camera"", ""name"": ""focus"", ""actor"": ""player"", ""seconds"": 0.1 },
            { ""type"": ""lighting"", ""name"": ""night"", ""seconds"": 0.1 },
            { ""type"": ""spawn"", ""id"": ""gift"", ""name"": ""crop.strawberry"", ""x"": 11, ""y"": 10 },
            { ""type"": ""wait"", ""seconds"": 30 },
            { ""type"": ""effects"", ""effects"": [ ""flag:late"" ] } ] } ] }";

        IEnumerator Begin()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            Assert.IsTrue(_session.Story.AddJson(Scenes, "test"));
            var bus = ServiceLocator.Get<EventBus>();
            bus.Subscribe<MusicCue>(c => _music.Add(c.Name));
            bus.Subscribe<EventFinished>(e => { _finished = true; _finishedSkipped = e.Skipped; });
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        static UiService Ui => (UiService)ServiceLocator.Get<IUiService>();
        static CameraFollow Cam => UnityEngine.Object.FindAnyObjectByType<CameraFollow>();
        static DayNightLighting Light => UnityEngine.Object.FindAnyObjectByType<DayNightLighting>();
        static bool PropExists => GameObject.Find("Prop_gift") != null;
        static bool BubbleShowing => UnityEngine.Object.FindObjectsByType<TextMesh>().Any(t => t.gameObject.activeInHierarchy && t.text == "<3");

        IEnumerator WaitUntil(Func<bool> condition, float seconds = 15f)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(condition(), "timed out waiting");
        }

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AScene_UsesTheNewSteps_AndLeavesTheWorldAsItFoundIt()
        {
            yield return Begin();
            _session.SetFlag("take_b");
            EventRunner.Trigger(_session, "test.stage");

            // In the long wait near the end every effect should be on show at the same time.
            yield return WaitUntil(() => _music.Contains("festival_theme"));
            Assert.IsTrue(EventDirector.Current.IsPlaying);
            Assert.IsTrue(Ui.LetterboxOn, "letterbox on");
            Assert.IsTrue(Cam.IsFocused, "the camera is on Tilda");
            Assert.Greater(Light.OverrideWeight, 0.5f, "the lighting mood");
            Assert.IsTrue(PropExists, "the spawned prop");
            Assert.IsFalse(BubbleShowing, "the async emote ended before the scene went on (waitFor)");
            Assert.IsTrue(_session.HasFlag("path_b"), "the branch jumped to b");
            Assert.IsFalse(_session.HasFlag("path_a"), "and skipped a");
            Assert.IsFalse(_session.HasFlag("never_runs"), "a step with a false condition did not run");

            yield return WaitUntil(() => _finished);
            Assert.IsFalse(_finishedSkipped);
            Assert.IsFalse(Ui.LetterboxOn, "letterbox off again");
            Assert.IsFalse(Cam.IsFocused, "the camera is back on the player");
            Assert.AreEqual(0f, Light.OverrideWeight, 0.0001f, "normal lighting");
            Assert.IsFalse(PropExists, "the prop is gone");
        }

        [UnityTest]
        public IEnumerator AnEmote_ShowsABubble_OverTheActor()
        {
            yield return Begin();
            EventRunner.Trigger(_session, "test.stage");
            yield return WaitUntil(() => BubbleShowing, 8f);
            yield return WaitUntil(() => _finished, 20f);
            Assert.IsFalse(BubbleShowing, "the bubble is removed");
        }

        [UnityTest]
        public IEnumerator TheCamera_NeverLeavesTheMap_EvenWhenToldToLookFarAway()
        {
            yield return Begin();
            var bounds = UnityEngine.Object.FindAnyObjectByType<FarmMap>().WorldBounds;
            foreach (var far in new[] { new Vector3(-500f, -500f, 0f), new Vector3(500f, 500f, 0f) })
            {
                Cam.FocusOn(far, 0f);
                for (var i = 0; i < 5; i++) yield return null;
                var p = Cam.transform.position;
                Assert.GreaterOrEqual(p.x, bounds.min.x - 0.01f, "left edge");
                Assert.LessOrEqual(p.x, bounds.max.x + 0.01f, "right edge");
                Assert.GreaterOrEqual(p.y, bounds.min.y - 0.01f, "bottom edge");
                Assert.LessOrEqual(p.y, bounds.max.y + 0.01f, "top edge");
            }
            Cam.ReleaseFocus(0f);
        }

        [UnityTest]
        public IEnumerator SkippingAScene_StillRunsItsEffects_AndClearsTheStage()
        {
            yield return Begin();
            EventRunner.Trigger(_session, "test.skip");
            yield return WaitUntil(() => Ui.LetterboxOn && Cam.IsFocused && PropExists && Light.OverrideWeight > 0.5f);

            yield return Tap(Key.Escape);                               // skip the 30-second wait
            yield return WaitUntil(() => _finished, 5f);
            Assert.IsTrue(_finishedSkipped);
            Assert.IsTrue(_session.HasFlag("late"), "the effects after the wait still ran");
            Assert.IsTrue(_session.HasFlag("skip_effects"), "and the scene's skip effects");
            Assert.IsFalse(Ui.LetterboxOn, "the bars are gone");
            Assert.IsFalse(Cam.IsFocused);
            Assert.AreEqual(0f, Light.OverrideWeight, 0.0001f);
            Assert.IsFalse(PropExists);
        }
    }
}
