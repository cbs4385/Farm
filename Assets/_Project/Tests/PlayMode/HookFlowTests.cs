using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Runs a test module that uses every extension point inside the real game, to prove the hooks are wired up.
    public class HookFlowTests
    {
        sealed class NightNote : DayCycleHook
        {
            public override void OnNightFalls(DayCycleContext c) => c.Note("test.dream", 3);
        }

        sealed class FogOnFullMoon : IWeatherModifier
        {
            public int Order => 0;
            public string Modify(GameDateTime d, string w, GameState s) => d.MoonPhase == MoonPhase.Full ? "fog" : w;
        }

        sealed class Widget : IHudWidget
        {
            public int Built, Refreshed;
            public void Build(Transform parent) => Built++;
            public void Refresh(GameSession session) => Refreshed++;
        }

        sealed class TestModule : IGameModule
        {
            public string Id => "testmod";
            public readonly Widget Widget = new Widget();
            public string LoadedMap;

            public void Initialize(ModuleContext ctx)
            {
                ctx.Hooks.AddDayCycleHook(new NightNote());
                ctx.Hooks.AddWeatherModifier(new FogOnFullMoon());
                ctx.Hooks.AddHudWidget(() => Widget);
                ctx.Hooks.MapLoaded += m =>
                {
                    LoadedMap = m.MapId;
                    ServiceLocator.Get<AtmosphereService>().Stack.Set("test.dark", new Color(0f, 0f, 0f), 1f);
                };
            }
        }

        string _dataRoot;
        TestModule _module;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hooktests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            GameModules.ClearForTests();
            _module = new TestModule();
            GameModules.Register(_module);
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameModules.ClearForTests();
            GameModules.Register(new MythosModule());   // restore the normally registered module
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static IEnumerator LoadFarm(GameSession session)
        {
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;   // random clutter would make tile positions unpredictable
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator Module_Uses_Every_Hook_In_The_Real_Game()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            yield return LoadFarm(session);

            // MapLoaded fired, and the atmosphere layer it pushed darkens the real scene light.
            Assert.AreEqual(MapIds.Farm, _module.LoadedMap);
            var light = UnityEngine.Object.FindFirstObjectByType<Light2D>();
            Assert.Less(light.color.r, 0.05f, "atmosphere layer should tint the day/night light");

            // The HUD widget was built and refreshed by the UI layer.
            Assert.AreEqual(1, _module.Widget.Built);
            Assert.Greater(_module.Widget.Refreshed, 0);

            // Day cycle: night note appears in the summary, and a full-moon dawn is foggy.
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 14, 1000));
            session.State.Mailbox.Clear();
            session.Story = new StoryContent();      // the story's own morning notes are tested elsewhere
            var summary = session.EndDay(false);
            Assert.AreEqual(1, summary.Notes.Count);
            Assert.AreEqual("test.dream", summary.Notes[0].Key);
            Assert.AreEqual("fog", summary.NewWeather);
            Assert.AreEqual("fog", session.State.Weather);
        }

        [UnityTest]
        public IEnumerator Warp_Condition_Blocks_And_Allows_Travel()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            yield return LoadFarm(session);

            var warp = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == MapIds.FarmHouse);
            Assert.IsTrue(warp.IsOpen(out _), "no condition means always open");

            warp.Condition = "flag:test.open && hour>=6";
            warp.BlockedMessageKey = "toast.too_tired";
            Assert.IsFalse(warp.IsOpen(out _));

            var toasts = new System.Collections.Generic.List<string>();
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(e => toasts.Add(e.Message));
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var enter = typeof(Warp).GetMethod("OnTriggerEnter2D", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enter.Invoke(warp, new object[] { player.GetComponent<Collider2D>() });
            yield return null;
            Assert.AreEqual(MapIds.Farm, SceneManager.GetActiveScene().name, "a blocked warp must not travel");
            CollectionAssert.Contains(toasts, L.Get("toast.too_tired"));

            session.SetFlag("test.open");
            Assert.IsTrue(warp.IsOpen(out _));
        }

        [UnityTest]
        public IEnumerator ConditionalObject_Follows_Flags_Without_Polling_Code()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            yield return LoadFarm(session);

            var parent = new GameObject("altar");
            var child = new GameObject("symbol");
            child.transform.SetParent(parent.transform);
            var conditional = parent.AddComponent<ConditionalObject>();
            conditional.Condition = "flag:mythos.cult_revealed && var:dread>=2";
            yield return null;
            Assert.IsFalse(child.activeSelf);

            session.SetFlag(MythosIds.Flags.CultRevealed);
            Assert.IsFalse(child.activeSelf, "both parts of the condition are required");
            session.SetVar(MythosIds.Vars.Dread, 2);
            Assert.IsTrue(child.activeSelf);

            session.SetFlag(MythosIds.Flags.CultRevealed, false);
            Assert.IsFalse(child.activeSelf);

            conditional.Invert = true;
            conditional.Refresh();
            Assert.IsTrue(child.activeSelf);
        }
    }
}
