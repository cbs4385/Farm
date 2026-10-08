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
    // In the real game: a villager whose schedule puts them in bed is drawn lying down and cannot be talked to; once they walk they stand again.
    public class NpcSleepFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npcsleep-" + Guid.NewGuid().ToString("N"));
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

        [UnityTest]
        public IEnumerator ASleepingVillager_LiesDown_AndIsNotTalkedTo_ThenStandsWhenWalking()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var manager = NpcManager.Current;
            var actor = manager.Take("wren", new Vector3Int(20, 10, 0));
            var grid = manager.Grid();

            actor.Apply(NpcPlacement.Standing(MapIds.Farm, 20, 10, NpcSchedule.SleepFacing), map, grid);
            Assert.IsTrue(actor.Sleeping);
            var whole = actor.Definition.SpriteFor(Vector2Int.down);
            var shown = actor.GetComponent<SpriteRenderer>().sprite;
            Assert.Less(shown.rect.height, whole.rect.height, "only the head and shoulders show, tucked in");
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, actor.transform.eulerAngles.z), 0.01f, "the sleeper is not turned across the north-south bed");
            Assert.IsTrue(actor.SleepIconShown, "with the Z z z over them");

            actor.Apply(NpcPlacement.Standing(MapIds.Farm, 20, 10, "down"), map, grid);
            Assert.IsFalse(actor.Sleeping, "awake once the schedule says so");
            Assert.IsFalse(actor.SleepIconShown, "and the icon gone");
            Assert.AreEqual(whole.rect.height, actor.GetComponent<SpriteRenderer>().sprite.rect.height, "and the whole picture is back"); 

            actor.Apply(NpcPlacement.Standing(MapIds.Farm, 20, 10, NpcSchedule.SleepFacing), map, grid);
            actor.Apply(new NpcPlacement(MapIds.Farm, 20, 10, 24, 10, 0.1f, true, "down", MapIds.Farm), map, grid);
            Assert.IsFalse(actor.Sleeping, "and up when walking");
        }
    }
}
