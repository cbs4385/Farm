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
    // Playtest 2026-10-07: walking villagers pushed the player, and walked through whatever stood in their way. In the real game, with the real
    // physics: the player stands still in a villager's path; the villager stops short, never pushes, finds a way round and gets where it was going.
    public class NpcYieldFlowTests
    {
        string _dataRoot;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npcyield-" + Guid.NewGuid().ToString("N"));
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

        [UnityTest]
        public IEnumerator AVillagerWalkingIntoThePlayer_StopsShort_NeverPushes_GoesRound_AndArrives()
        {
            yield return EnterFarm();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var manager = NpcManager.Current;
            var actor = manager.Take("wren", new Vector3Int(20, 10, 0));
            Assert.IsNotNull(actor);
            actor.SetCell(map, new Vector3Int(20, 10, 0));

            player.Teleport(map.CellCenter(new Vector3Int(25, 10, 0)));
            player.Stop();
            yield return null;
            var standing = player.transform.position;
            var grid = manager.Grid();

            var t = 0f;
            var waited = false;
            var wentRound = false;
            var end = Time.realtimeSinceStartup + 40f;
            while (t < 1f && Time.realtimeSinceStartup < end)
            {
                actor.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, t, true, "down", MapIds.Farm), map, grid);
                if (actor.Waiting) waited = true; else t += Time.deltaTime / 8f;          // the manager holds the schedule back while the villager waits
                if (Mathf.Abs(actor.transform.position.y - map.CellCenter(new Vector3Int(25, 10, 0)).y) > 0.6f) wentRound = true;
                Assert.Less(Vector3.Distance(player.transform.position, standing), 0.05f, "the villager did not push the player");
                yield return null;
            }

            Assert.IsTrue(waited, "the villager stopped when the player was in the way");
            Assert.IsTrue(wentRound, "and found a way round");
            actor.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 30, 10, 1f, false, "down", MapIds.Farm), map, grid);
            Assert.AreEqual(new Vector3Int(30, 10, 0), actor.Cell, "and got where it was going");
            Assert.Less(Vector3.Distance(player.transform.position, standing), 0.05f);
        }

        [UnityTest]
        public IEnumerator AVillagerArrivingOnThePlayersCell_IsNotSolid_UntilThePlayerSteps_Out()
        {
            yield return EnterFarm();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actor = NpcManager.Current.Take("wren", new Vector3Int(20, 10, 0));
            var cell = new Vector3Int(25, 10, 0);
            player.Teleport(map.CellCenter(cell));
            player.Stop();
            yield return null;
            var standing = player.transform.position;

            actor.Apply(NpcPlacement.Standing(MapIds.Farm, cell.x, cell.y, "down"), map, NpcManager.Current.Grid());     // the schedule puts it exactly there
            for (var i = 0; i < 10; i++) { actor.Apply(NpcPlacement.Standing(MapIds.Farm, cell.x, cell.y, "down"), map, NpcManager.Current.Grid()); yield return null; }
            Assert.Less(Vector3.Distance(player.transform.position, standing), 0.05f, "the player was not shoved out of the way");
            Assert.IsFalse(actor.transform.Find(ActorBody.BodyName).GetComponent<BoxCollider2D>().enabled, "it is not solid while it overlaps the player");

            player.Teleport(map.CellCenter(new Vector3Int(28, 10, 0)));
            yield return null;
            actor.Apply(NpcPlacement.Standing(MapIds.Farm, cell.x, cell.y, "down"), map, NpcManager.Current.Grid());
            Assert.IsTrue(actor.transform.Find(ActorBody.BodyName).GetComponent<BoxCollider2D>().enabled, "and solid again once the player is clear");
        }
    }
}
