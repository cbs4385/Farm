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
    // The forest pond and the beach sea are built from the shore tiles: still water for fishing, drawn as one body of water.
    public class WaterShoreFlowTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-water-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static IEnumerator Load(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheForestPond_IsOneOvalOfWater_WithAShorelineOnItsEdge()
        {
            yield return Load(MapIds.Forest);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            Assert.IsTrue(map.IsWater(new Vector3Int(6, 10, 0)), "the middle of the pond is water");
            Assert.IsFalse(map.IsWater(new Vector3Int(1, 10, 0)), "beside it is land");

            var ground = map.Ground;
            var water = 0; var edge = 0;
            for (var x = 0; x < 14; x++)
                for (var y = 4; y < 17; y++)
                {
                    var tile = ground.GetTile(new Vector3Int(x, y, 0));
                    if (tile == null || !WaterShore.IsWaterTile(tile.name)) continue;
                    water++;
                    if (!tile.name.StartsWith("tile_water_m0v")) edge++;
                }
            Assert.Greater(water, 30, "a pond, not a puddle");
            Assert.Greater(edge, 8, "with edge tiles round its shore");
            Assert.Less(edge, water, "and open water inside");
        }

        [UnityTest]
        public IEnumerator TheBeachSea_IsStillWater_AndItsLandEdgeHasAShoreline()
        {
            yield return Load(MapIds.Beach);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            Assert.IsTrue(map.IsWater(new Vector3Int(10, 1, 0)), "the sea is water");
            Assert.IsFalse(map.IsWater(new Vector3Int(10, 10, 0)), "the sand is not");
            Assert.AreEqual("tile_water_m1", map.Ground.GetTile(new Vector3Int(10, 4, 0)).name, "the sea's land edge has a shoreline on its north side");
        }
    }
}
