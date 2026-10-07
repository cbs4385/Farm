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
    // Playtest 2026-10-07 (the report that found it): the player waited in the middle of the route to the forest, the village lane, for Dorian to
    // come out of his building and head into the forest. The real village, his real schedule and the real clock: the player must never be moved,
    // and Dorian must still get past.
    public class NpcLaneFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npclane-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator DorianWalkingUpTheLaneToTheForest_NeverMovesAPlayerWaitingInTheLane_AndGetsPast()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = MapIds.Village;
            session.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            // The first stretch of his morning in which he walks up the village toward the forest.
            var dorian = session.Npcs.Get("dorian");
            var plan = NpcSchedule.PlanFor(dorian, session.World);
            var minute = -1;
            var leg = default(NpcPlacement);
            for (var m = 360; m < 1200 && minute < 0; m++)
            {
                var place = NpcSchedule.Where(dorian, plan, m);
                if (place.Walking && place.Map == MapIds.Village && place.ToY - place.FromY >= 10) { minute = m; leg = place; }
            }
            Assert.GreaterOrEqual(minute, 0, "Dorian walks up the village on his way to the forest");

            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var waitAt = new Vector3Int(leg.ToX, (leg.FromY + leg.ToY) / 2, 0);             // the middle of his way
            player.Teleport(map.CellCenter(waitAt));
            player.Stop();
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, minute - 3));
            Time.timeScale = 3f;
            yield return null;
            var standing = player.transform.position;

            NpcActor actor = null;
            var worst = 0f;
            var passed = false;
            var end = Time.realtimeSinceStartup + 150f;
            while (!passed && Time.realtimeSinceStartup < end)
            {
                if (actor == null) NpcManager.Current.Actors.TryGetValue("dorian", out actor);
                worst = Mathf.Max(worst, Vector3.Distance(player.transform.position, standing));
                if (actor != null && actor.transform.position.y > standing.y + 1.5f && session.Clock.Now.MinuteOfDay >= minute) passed = true;
                yield return null;
            }
            Assert.Less(worst, 0.01f, "the player waiting in the lane was never moved");
            Assert.IsTrue(passed, "and Dorian got past on his way to the forest");
        }
    }
}
