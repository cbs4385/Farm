using System;
using System.Collections;
using System.IO;
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
    // Not a check: a way to look at the hover labels (a built player cannot be hovered by a script). With FARM_HOVER_CAPTURE_DIR set it rests a simulated mouse on a bush, a log, the
    // mine entrance and some HUD icons, and saves a screenshot of each. Without it the test is skipped.
    public class HoverCaptureFlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hovercap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            HoverInspector.ResetForTests();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Load(string map)
        {
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        IEnumerator Shot(string dir, string name, Vector2 screen)
        {
            Set(_mouse.position, screen);
            yield return null;
            Set(_mouse.position, screen + new Vector2(1f, 0f));
            for (var i = 0; i < 5; i++) yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
            yield return null;
            Debug.Log("[HoverCapture] " + name + ": " + ((UiService)GameContext.Current.Ui).HoverText);
        }

        [UnityTest]
        public IEnumerator CaptureHoverLabels()
        {
            var dir = Environment.GetEnvironmentVariable("FARM_HOVER_CAPTURE_DIR");
            if (string.IsNullOrEmpty(dir)) Assert.Ignore("set FARM_HOVER_CAPTURE_DIR to take the screenshots");
            Directory.CreateDirectory(dir);
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            session.SetFlag(HudView.HealthBarFlag);
            GameContext.Current.Settings.Current.HoverLabels = true;
            yield return Load(MapIds.Farm);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var cam = Camera.main;
            var nodes = session.GetNodes(MapIds.Farm);
            Vector2 At(int x, int y) { var p = cam.WorldToScreenPoint(map.CellCenter(new Vector3Int(x, y, 0))); return new Vector2(p.x, p.y); }
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var home = player != null ? map.WorldToCell(player.transform.position) : new Vector3Int(20, 8, 0);
            var bush = new Vector3Int(home.x + 2, home.y + 1, 0);
            var log = new Vector3Int(home.x + 4, home.y + 1, 0);
            nodes.Add(bush.x, bush.y, session.Nodes.Get("bush"));
            nodes.Add(log.x, log.y, session.Nodes.Get("log"));
            for (var i = 0; i < 6; i++) yield return null;
            yield return Shot(dir, "hover_bush", At(bush.x, bush.y));
            yield return Shot(dir, "hover_log", At(log.x, log.y));
            yield return Shot(dir, "hover_hud_time", new Vector2(150f, Screen.height - 17f));
            yield return Shot(dir, "hover_hud_energy", new Vector2(Screen.width - 29f, 70f));
            yield return Shot(dir, "hover_hotbar_slot", new Vector2(Screen.width * 0.5f - 200f, 30f));

            session.State.CurrentMap = MapIds.Forest;
            session.State.SpawnPoint = "default";
            yield return Load(MapIds.Forest);
            map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            cam = Camera.main;
            var walker = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (walker != null) walker.transform.position = new Vector3(36f, 9f, 0f);
            for (var i = 0; i < 20; i++) yield return null;
            var mine = cam.WorldToScreenPoint(new Vector3(37f, 6f, 0f));
            yield return Shot(dir, "hover_mine_entrance", new Vector2(mine.x, mine.y));
        }
    }
}
