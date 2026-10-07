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
    // Playtest 2026-10-07 (second report): "the npc is still pushing the player aside after a moment of contact". The first fix held a villager back
    // from the player, but the first stretch of every route was exempt from the check (so a villager can stand up from a seat), and after the villager
    // had waited and found a way round, its first step could slide into the player and shove them. A villager must never move the player at all.
    public class NpcPushFlowTests
    {
        string _dataRoot;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npcpush-" + Guid.NewGuid().ToString("N"));
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
            for (var x = 14; x <= 36; x++) for (var y = 6; y <= 14; y++) nodes.Remove(x, y);
            UnityEngine.Object.FindAnyObjectByType<FarmMapView>().RefreshAll();
            yield return null;
        }

        // Runs a villager along a leg for up to `seconds` of real time with the player standing at `playerCell`; returns the most the player was moved.
        IEnumerator Run(Vector3Int playerCell, float seconds, Action<float> report)
        {
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var manager = NpcManager.Current;
            var actor = manager.Take("wren", new Vector3Int(20, 10, 0));
            actor.SetCell(map, new Vector3Int(20, 10, 0));
            player.Teleport(map.CellCenter(playerCell));
            player.Stop();
            yield return null;
            var standing = player.transform.position;
            var grid = manager.Grid();

            var t = 0f;
            var worst = 0f;
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                actor.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, Mathf.Min(1f, t), t < 1f, "down", MapIds.Farm), map, grid);
                if (!actor.Waiting) t += Time.deltaTime / 8f;
                worst = Mathf.Max(worst, Vector3.Distance(player.transform.position, standing));
                yield return null;
            }
            report(worst);
        }

        [UnityTest]
        public IEnumerator ThePlayer_IsNeverMoved_WhenStandingRightAtTheStartOfAVillagersRoute()
        {
            yield return EnterFarm();
            var worst = -1f;
            yield return Run(new Vector3Int(21, 10, 0), 10f, w => worst = w);       // one cell from where the villager starts walking
            Assert.Less(worst, 0.01f, "the player was not moved at all");
        }

        [UnityTest]
        public IEnumerator ThePlayer_IsNeverMoved_WhileAVillagerWaits_ReroutesAndGoesRound()
        {
            yield return EnterFarm();
            var worst = -1f;
            yield return Run(new Vector3Int(25, 10, 0), 14f, w => worst = w);      // long enough for the wait, the new route and the walk round
            Assert.Less(worst, 0.01f, "the player was not moved at all, not even after the villager found a way round");
        }
    }
}
