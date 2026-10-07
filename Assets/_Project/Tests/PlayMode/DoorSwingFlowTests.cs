using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // In the real game: walking onto the farm's road exit shows no door; walking onto the farmhouse door does.
    public class DoorSwingFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-doorswing-" + Guid.NewGuid().ToString("N"));
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
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        static void Walk(string targetMap)
        {
            var warp = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == targetMap);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            warp.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
        }

        [UnityTest]
        public IEnumerator TheRoadToTheVillage_ShowsNoDoor()
        {
            yield return EnterFarm();
            Walk(MapIds.Village);
            Assert.AreEqual(0, UnityEngine.Object.FindObjectsByType<DoorFlash>(FindObjectsSortMode.None).Length);
            for (var i = 0; i < 60; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheFarmhouseDoor_StillSwings()
        {
            yield return EnterFarm();
            Walk(MapIds.FarmHouse);
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsByType<DoorFlash>(FindObjectsSortMode.None).Length);
            for (var i = 0; i < 60; i++) yield return null;
        }
    }
}
