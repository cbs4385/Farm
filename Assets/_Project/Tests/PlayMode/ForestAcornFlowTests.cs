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
    // Playtest 2026-10-11: felled forest trees must be replaceable: acorns can be planted on the forest floor (and on the farm), nowhere else.
    public class ForestAcornFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-acorn-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Load(string map)
        {
            var session = GameContext.Current.Session;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheForestFloor_TakesAnAcorn_ThePathAndTheVillageDoNot()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);

            yield return Load(MapIds.Forest);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var nodes = session.GetNodes(MapIds.Forest);
            Vector3Int floor = default, path = default;
            var foundFloor = false;
            foreach (var c in map.Ground.cellBounds.allPositionsWithin)
            {
                var tile = map.Ground.GetTile(c);
                if (tile == null) continue;
                if (!foundFloor && tile.name == FarmMap.ForestFloorTile && !nodes.Has(c.x, c.y) && map.IsTreeGround(c)) { floor = c; foundFloor = true; }
                if (tile.name == "tile_path" && path == default) path = c;
            }
            Assert.IsTrue(foundFloor, "somewhere on the forest floor an acorn can go");
            Assert.IsFalse(map.IsTreeGround(path), "not on the path");

            yield return Load(MapIds.Village);
            var village = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var c in village.Ground.cellBounds.allPositionsWithin)
                if (village.HasTillableGround(c)) { Assert.IsFalse(village.IsTreeGround(c), "not on the village green"); break; }

            yield return Load(MapIds.Farm);
            var farm = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var c in farm.Ground.cellBounds.allPositionsWithin)
                if (farm.IsTillable(c)) { Assert.IsTrue(farm.IsTreeGround(c), "and still on the farm"); break; }
        }
    }
}
