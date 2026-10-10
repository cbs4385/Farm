using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Owner, 2026-10-09: the map tab must show the new buildings and the other things of the world. The map's landmarks and ponds come from the same numbers as the
    // scenes; this checks the built village and its edges against them.
    public class MapLandmarksFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-maplm-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Open(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;
        }

        static Transform Named(string name) => Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t => t.name == name && t.gameObject.scene.IsValid());

        [UnityTest]
        public IEnumerator The_villages_landmarks_and_pond_stand_where_the_map_draws_them()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindFirstObjectByType<FarmMap>();
            foreach (var (cell, key) in WorldMapLayout.Landmarks())
            {
                var name = key.EndsWith("fountain") ? "Fountain" : "ClockTower";
                var thing = Named(name);
                Assert.IsNotNull(thing, name + " stands in the village");
                Assert.AreEqual(cell.x + 1f, thing.position.x, 0.01f, name + " is two cells wide from the cell the map uses");
                Assert.AreEqual(cell.y + 0.5f, thing.position.y, 0.6f);
            }
            var pond = MapLayout.VillagePond;
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(Mathf.RoundToInt(pond.X), Mathf.RoundToInt(pond.Y), 0)), "the village pond is water, walled");
            foreach (var row in WorldMapLayout.PondRows(WorldMapLayout.Village, pond))
            {
                Assert.GreaterOrEqual(row.xMin, WorldMapLayout.Village.X0);
                Assert.LessOrEqual(row.xMax, WorldMapLayout.Village.X1);
            }
        }

        [UnityTest]
        public IEnumerator The_edge_of_the_village_is_woods_with_the_ways_out_open()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindFirstObjectByType<FarmMap>();
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(0, 5, 0)), "the western edge is closed");
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(1, 5, 0)), "two cells thick");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(2, 5, 0)), "and no thicker");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(0, 17, 0)), "the road to the farm is open");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(1, 17, 0)), "through both cells");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(25, 0, 0)), "the lane to the beach is open");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(25, 1, 0)));
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(25, MapLayout.VillageH - 2, 0)), "the lane to the forest is open");
            Assert.Greater(Named("EdgeBand").childCount, 100, "trees stand along the edge");
        }
    }
}
