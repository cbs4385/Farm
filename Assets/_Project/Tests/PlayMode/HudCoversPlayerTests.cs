using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest report (2026-10-04): "the player becomes hidden when moving to the upper right of the screen". The camera stops at the edge of
    // the map, so near a corner the player stands under the HUD (the clock panel is opaque, top right). The HUD panel must get out of the way.
    public class HudCoversPlayerTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hud-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static Rect ScreenRect(RectTransform rt)
        {
            var c = new Vector3[4];
            rt.GetWorldCorners(c);                       // an overlay canvas: world corners are screen pixels
            return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
        }

        static Rect PlayerRect(PlayerController player, Camera cam)
        {
            var sr = player.GetComponentInChildren<SpriteRenderer>();
            var b = sr.bounds;
            var min = cam.WorldToScreenPoint(b.min);
            var max = cam.WorldToScreenPoint(b.max);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        IEnumerator Enter()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator InTheUpperRightCorner_TheClockPanelFadesSoThePlayerShowsThrough()
        {
            yield return Enter();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var cam = Camera.main;
            var clock = GameObject.Find("ClockPanel").GetComponent<RectTransform>();

            // Walk to the top right of the map: the camera stops at the edge, so the player ends up near the corner of the screen.
            var b = map.WorldBounds;
            player.Teleport(new Vector3(b.max.x - 1.2f, b.max.y - 2.2f, 0f));
            for (var i = 0; i < 40; i++) yield return null;

            var overlaps = ScreenRect(clock).Overlaps(PlayerRect(player, cam));
            Assert.IsTrue(overlaps, "the scenario: the player is under the clock panel (screen " + Screen.width + "x" + Screen.height + ")");
            var group = clock.GetComponent<CanvasGroup>();
            Assert.IsNotNull(group, "the panel can fade");
            var end = Time.realtimeSinceStartup + 4f;
            while (group.alpha >= 0.5f && Time.realtimeSinceStartup < end) yield return null;      // the fade takes a fraction of a second
            Assert.Less(group.alpha, 0.5f, "the panel fades while the player is behind it");

            // Back in the middle of the map the panel is solid again.
            player.Teleport(map.WorldBounds.center);
            end = Time.realtimeSinceStartup + 4f;
            while (group.alpha <= 0.95f && Time.realtimeSinceStartup < end) yield return null;
            Assert.Greater(group.alpha, 0.95f, "and returns when the player is clear of it");
        }
    }
}
