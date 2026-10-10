using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Bug report "Stuck?" (2026-10-06): along the bottom of the farm a few spaces catch the player in one direction and not the other.
    // Walks the rows next to the south wall with a simulated keyboard, both ways, and lists every place where the player stops.
    public class FarmEdgeWalkTests : InputTestFixture
    {
        string _dataRoot;
        Gamepad _pad;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-edgewalk-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _pad = InputSystem.AddDevice<Gamepad>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        IEnumerator StartFarm()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            var nodes = session.GetNodes(MapIds.Farm);
            for (var x = 0; x < 90; x++) for (var y = 0; y < 5; y++) nodes.Remove(x, y);      // only the walls and the ground are being tested
            UnityEngine.Object.FindAnyObjectByType<FarmMapView>().RefreshAll();
            yield return null;
        }

        // Walks from one end of the row to the other and returns the x positions where the player made no headway for a quarter of a second.
        IEnumerator Walk(float y, bool east, bool pushDown, List<float> stalls)
        {
            var player = Player;
            var from = east ? 2.6f : 85.4f;
            var to = east ? 85.4f : 2.6f;
            player.Teleport(new Vector3(from, y, 0f));
            yield return null;
            // The stick held toward the east or west and, with pushDown, into the wall as well (how a person hugs the edge).
            Set(_pad.leftStick, new Vector2(east ? 1f : -1f, pushDown ? -1f : 0f).normalized);
            var last = player.transform.position.x;
            var lastTime = Time.time;
            var started = Time.time;
            while (Time.time - started < 40f && (east ? player.transform.position.x < to : player.transform.position.x > to))
            {
                yield return null;
                if (Time.time - lastTime < 0.25f) continue;
                var x = player.transform.position.x;
                if (Mathf.Abs(x - last) < 0.2f) stalls.Add((float)Math.Round(x, 1));
                last = x; lastTime = Time.time;
            }
            Set(_pad.leftStick, Vector2.zero);
            yield return null;
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator WalkingAlongTheSouthWall_NeverCatchesThePlayer_InEitherDirection()
        {
            yield return StartFarm();
            var report = new List<string>();
            foreach (var y in new[] { 2.05f, 2.5f })
                foreach (var pushDown in new[] { false, true })
                    foreach (var east in new[] { true, false })
                    {
                        var stalls = new List<float>();
                        yield return Walk(y, east, pushDown, stalls);
                        if (stalls.Count > 0) report.Add($"y={y} down={pushDown} {(east ? "east" : "west")}: stopped near x = {string.Join(", ", stalls.Distinct())}");
                    }
            Assert.IsEmpty(report, string.Join("\n", report));
        }
    }
}
