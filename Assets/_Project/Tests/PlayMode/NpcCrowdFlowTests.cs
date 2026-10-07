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
    // Villagers walked through each other. In the real game, with the real physics: a villager standing in another's path is gone round, and two
    // villagers walking at each other never overlap and both get where they were going.
    public class NpcCrowdFlowTests
    {
        // Two bodies 0.7 wide touch when their centres are closer than 0.7 on both axes; allow a little for the probe.
        const float Clear = 0.66f;

        string _dataRoot;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npccrowd-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator EnterFarm()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            var nodes = _session.GetNodes(MapIds.Farm);
            for (var x = 14; x <= 36; x++) for (var y = 6; y <= 14; y++) nodes.Remove(x, y);       // a clear patch of ground
            UnityEngine.Object.FindAnyObjectByType<FarmMapView>().RefreshAll();
            yield return null;
        }

        static bool Touching(NpcActor a, NpcActor b)
        {
            var d = a.transform.position - b.transform.position;
            return Mathf.Abs(d.x) < Clear && Mathf.Abs(d.y) < Clear;
        }

        [UnityTest]
        public IEnumerator AVillagerStandingInTheWay_IsGoneRound_NotWalkedThrough()
        {
            yield return EnterFarm();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var manager = NpcManager.Current;
            var walker = manager.Take("wren", new Vector3Int(20, 10, 0));
            var standing = manager.Take("dorian", new Vector3Int(25, 10, 0));
            walker.SetCell(map, new Vector3Int(20, 10, 0));
            standing.SetCell(map, new Vector3Int(25, 10, 0));
            var grid = manager.Grid();

            var t = 0f;
            var touched = false;
            var end = Time.realtimeSinceStartup + 40f;
            while (t < 1f && Time.realtimeSinceStartup < end)
            {
                standing.Apply(NpcPlacement.Standing(MapIds.Farm, 25, 10, "down"), map, grid);
                walker.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, t, true, "down", MapIds.Farm), map, grid);
                if (!walker.Waiting) t += Time.deltaTime / 8f;
                if (Touching(walker, standing)) touched = true;
                yield return null;
            }

            Assert.IsFalse(touched, "the walker never overlapped the villager standing in its way");
            walker.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, 1f, false, "down", MapIds.Farm), map, grid);
            Assert.AreEqual(new Vector3Int(30, 10, 0), walker.Cell, "and got where it was going");
        }

        [UnityTest]
        public IEnumerator TwoVillagersWalkingAtEachOther_DoNotOverlap_AndBothArrive()
        {
            yield return EnterFarm();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var manager = NpcManager.Current;
            var a = manager.Take("wren", new Vector3Int(20, 10, 0));
            var b = manager.Take("dorian", new Vector3Int(30, 10, 0));
            a.SetCell(map, new Vector3Int(20, 10, 0));
            b.SetCell(map, new Vector3Int(30, 10, 0));
            var grid = manager.Grid();

            float ta = 0f, tb = 0f;
            var touched = false;
            var end = Time.realtimeSinceStartup + 60f;
            while ((ta < 1f || tb < 1f) && Time.realtimeSinceStartup < end)
            {
                a.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, ta, true, "down", MapIds.Farm), map, grid);
                b.Apply(new NpcPlacement(MapIds.Farm, 30, 10, 20, 10, tb, true, "down", MapIds.Farm), map, grid);
                if (!a.Waiting) ta += Time.deltaTime / 8f;
                if (!b.Waiting) tb += Time.deltaTime / 8f;
                if (Touching(a, b)) touched = true;
                yield return null;
            }

            Assert.IsFalse(touched, "the two never overlapped");
            a.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, 1f, false, "down", MapIds.Farm), map, grid);
            b.Apply(new NpcPlacement(MapIds.Farm, 30, 10, 20, 10, 1f, false, "down", MapIds.Farm), map, grid);
            Assert.AreEqual(new Vector3Int(30, 10, 0), a.Cell);
            Assert.AreEqual(new Vector3Int(20, 10, 0), b.Cell);
        }
    }
}
