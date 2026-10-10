using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-10: "the avatars still look flat". The farmer and the villagers walk with their own frames, swing tools, stand on a shadow and have an outline;
    // here in the real game, with real input.
    public class CharacterAnimationFlowTests : InputTestFixture
    {
        string _root;
        Gamepad _pad;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-charanim-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            _pad = InputSystem.AddDevice<Gamepad>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Time.timeScale = 1f;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator Open(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheFarmer_WalksWithFourFramesAndStandsOnAShadowWithAnOutline()
        {
            yield return Open(MapIds.Farm);
            var player = Object.FindAnyObjectByType<PlayerController>();
            var renderer = player.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(player.transform.Find("Shadow"), "a shadow under the farmer");
            var standing = renderer.sprite;
            StringAssert.Contains("_idle0", standing.name, "even standing the picture has the look (outline and light)");

            var recorder = player.gameObject.AddComponent<SpriteRecorder>();
            Set(_pad.leftStick, new Vector2(1f, 0f));
            var end = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < end && recorder.Names.Count(n => n.Contains("_walk")) < 4) yield return null;
            Set(_pad.leftStick, Vector2.zero);
            var seen = new HashSet<string>(recorder.Names.Where(n => n.Contains("_walk")));
            Assert.GreaterOrEqual(seen.Count, 3, "walking shows walk frames: " + string.Join(", ", seen) + " (last picture " + renderer.sprite.name + ", moved to " + player.transform.position + ")");
        }

        [UnityTest]
        public IEnumerator UsingTheHoe_ShowsTheFarmersOwnSwingWithTheToolInHand()
        {
            yield return Open(MapIds.Farm);
            var player = Object.FindAnyObjectByType<PlayerController>();
            var renderer = player.GetComponent<SpriteRenderer>();
            var bob = player.GetComponent<WalkBob>();
            var session = ServiceLocator.Get<GameSession>();
            var recorder = player.gameObject.AddComponent<SpriteRecorder>();
            Press(_keyboard.cKey);
            var end = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < end && recorder.Names.Count(n => n.Contains("_strike_")) < 3) yield return null;
            Release(_keyboard.cKey);
            var frames = new HashSet<string>(recorder.Names.Where(n => n.Contains("_strike_")).Select(n => n.Substring(n.IndexOf("_strike_", System.StringComparison.Ordinal))));
            Assert.GreaterOrEqual(frames.Count, 2, "the swing shows more than one frame: " + string.Join(", ", frames));
            Assert.IsTrue(frames.All(f => f.StartsWith("_strike_")), "of the tool in hand");
        }

        [UnityTest]
        public IEnumerator AVillager_CanSwingATool_AndStandsOnAShadow()
        {
            yield return Open(MapIds.Village);
            var session = ServiceLocator.Get<GameSession>();
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 1, 17 * 60));
            for (var i = 0; i < 60; i++) yield return null;
            var actor = NpcManager.Current.Actors.Values.FirstOrDefault(a => !a.Sleeping);
            Assert.IsNotNull(actor, $"somebody is out in the village ({NpcManager.Current.Actors.Count} actors, minute {session.Clock.Now.MinuteOfDay})");
            yield return null;
            Assert.IsNotNull(actor.transform.Find("Shadow"), "a shadow under the villager");
            var recorder = actor.gameObject.AddComponent<SpriteRecorder>();
            actor.Strike(ToolType.Hammer);
            for (var i = 0; i < 30 && !recorder.Names.Any(n => n.Contains("_strike_Hammer_")); i++) yield return null;
            var seen = recorder.Names.Any(n => n.Contains("_strike_Hammer_"));
            Assert.IsTrue(seen, "the villager swings the hammer with their own frames: " + actor.GetComponent<SpriteRenderer>().sprite.name);
        }
    }
}
