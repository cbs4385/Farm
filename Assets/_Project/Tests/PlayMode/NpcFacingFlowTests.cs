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
    // Playtest 2026-10-08: "several NPCs do not face the correct direction when moving". In the real game: a walking villager turns to the way they are going,
    // in all four directions, and the picture on screen is the one for that direction.
    public class NpcFacingFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npcfacing-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator EveryVillager_FacesTheWayTheyWalk_InAllFourDirections()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            var nodes = session.GetNodes(MapIds.Farm);
            for (var x = 14; x <= 36; x++) for (var y = 6; y <= 18; y++) nodes.Remove(x, y);
            UnityEngine.Object.FindAnyObjectByType<FarmMapView>().RefreshAll();
            yield return null;
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var manager = NpcManager.Current;
            var grid = manager.Grid();

            var legs = new[]
            {
                (from: new Vector2Int(20, 10), to: new Vector2Int(28, 10), facing: Vector2Int.right),
                (from: new Vector2Int(28, 10), to: new Vector2Int(28, 15), facing: Vector2Int.up),
                (from: new Vector2Int(28, 15), to: new Vector2Int(20, 15), facing: Vector2Int.left),
                (from: new Vector2Int(20, 15), to: new Vector2Int(20, 10), facing: Vector2Int.down),
            };
            foreach (var id in NpcIds.All)
            {
                var actor = manager.Take(id, new Vector3Int(20, 10, 0));
                Assert.IsNotNull(actor, id);
                actor.SetCell(map, new Vector3Int(20, 10, 0));
                var renderer = actor.GetComponent<SpriteRenderer>();
                foreach (var leg in legs)
                {
                    actor.SetCell(map, new Vector3Int(leg.from.x, leg.from.y, 0));
                    var place = new NpcPlacement(MapIds.Farm, leg.from.x, leg.from.y, leg.to.x, leg.to.y, 0.5f, true, "down", MapIds.Farm);
                    for (var i = 0; i < 3; i++) { actor.Apply(place, map, grid); yield return null; }
                    Assert.AreEqual(leg.facing, actor.Facing, $"{id} walking {leg.facing}");
                    // While walking the picture alternates with the same picture one pixel higher (the walking bob, named "<picture>_up").
                    var expected = actor.Definition.SpriteFor(leg.facing).name;
                    Assert.IsTrue(renderer.sprite.name == expected || renderer.sprite.name == expected + "_up", $"{id} shows the {leg.facing} picture while walking {leg.facing}: shows {renderer.sprite.name}");
                }
            }
        }
    }
}
