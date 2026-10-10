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
    // Playtest 2026-10-09: the player can sit on a chair, couch or bench, and rests there.
    public class SitFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-sit-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator The_player_sits_rests_and_stands_up()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = MapIds.FarmHouse;
            var op = SceneManager.LoadSceneAsync(MapIds.FarmHouse);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;

            var spot = SitSpot.All.FirstOrDefault();
            Assert.IsNotNull(spot, "the farmhouse has something to sit on");
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            Assert.IsTrue(player.Sit(spot));
            Assert.IsTrue(player.Seated);
            Assert.AreSame(player, spot.Occupant);

            var before = session.State.Energy;
            session.State.Energy = Mathf.Max(0, before - 30);
            var low = session.State.Energy;
            session.Clock.AdvanceMinutes(60);
            for (var i = 0; i < 5; i++) yield return null;
            Assert.Greater(session.State.Energy, low, "sitting brings energy back");

            player.StandUp();
            Assert.IsFalse(player.Seated);
            Assert.IsNull(spot.Occupant);
        }
    }
}
