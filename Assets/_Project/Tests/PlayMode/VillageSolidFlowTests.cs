using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Farm.Tests
{
    // Playtest 2026-10-07: "the player was able to walk through the buildings in the village" (on the village map, not from inside a building). In the real
    // village with real key presses: the player walks at the front of every kind of building and is stopped by it.
    public class VillageSolidFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-villagesolid-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        [UnityTest]
        public IEnumerator ThePlayer_WalkingIntoABuilding_FromTheVillageStreet_IsStopped()
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
            yield return new WaitForSeconds(0.5f);

            var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            var map = tilemaps.First(t => t.name == "Walls");
            var ground = tilemaps.First(t => t.name == "Ground");
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();

            // For each kind of building: a wall cell on its street side (the cell below it is open ground) with wall on both sides and two more wall cells above.
            var styles = new Dictionary<string, Vector3Int>();
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var tile = map.GetTile(cell) as Tile;
                if (tile == null || !tile.name.StartsWith("bld_") || !tile.name.EndsWith("_wall")) continue;
                var style = tile.name.Substring(0, tile.name.Length - "_wall".Length);
                var below = cell + Vector3Int.down;
                if (map.GetTile(below) != null || ground.GetTile(below) == null) continue;
                if (map.GetTile(cell + Vector3Int.up) == null || map.GetTile(cell + Vector3Int.up * 2) == null) continue;
                if (map.GetTile(cell + Vector3Int.left) == null || map.GetTile(cell + Vector3Int.right) == null) continue;
                if (!styles.ContainsKey(style)) styles[style] = cell;
            }
            Assert.GreaterOrEqual(styles.Count, 8, "the seven shops and the cottages are all there: " + string.Join(", ", styles.Keys));

            var through = new List<string>();
            foreach (var pair in styles)
            {
                var wall = pair.Value;
                player.Teleport(map.GetCellCenterWorld(wall + Vector3Int.down * 2));
                yield return null;
                Press(_keyboard[Key.W]);
                var until = Time.realtimeSinceStartup + 1.2f;
                while (Time.realtimeSinceStartup < until) yield return null;
                Release(_keyboard[Key.W]);
                yield return null;
                var wallBottom = map.GetCellCenterWorld(wall).y - 0.5f;
                if (player.transform.position.y > wallBottom + 0.05f)
                    through.Add($"{pair.Key} wall at {wall.x},{wall.y}: the player reached y {player.transform.position.y:F2} (the wall starts at {wallBottom:F2})");
            }
            Assert.AreEqual(0, through.Count, "the player walked into: " + string.Join("; ", through));
        }

        // Every map saved with walls in place has its wall collider built once loaded (an empty one let the player through the village's buildings).
        [UnityTest]
        public IEnumerator EveryMapWithWalls_HasAWallColliderOnceLoaded([ValueSource(nameof(Maps))] string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            yield return new WaitForSeconds(0.3f);
            var walls = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None).First(t => t.name == "Walls");
            Assert.Greater(walls.GetComponent<CompositeCollider2D>().pathCount, 0, map + ": the walls are solid");
        }

        static readonly string[] Maps =
        {
            MapIds.Farm, MapIds.FarmHouse, MapIds.Village, MapIds.Forest, MapIds.Beach, MapIds.Mine, MapIds.GeneralStore, MapIds.Clinic, MapIds.Library,
            MapIds.Saloon, MapIds.Blacksmith, MapIds.Carpenter, MapIds.CommunityHall, MapIds.HomeTilda, MapIds.HomeWren, MapIds.Barn,
        };
    }
}
