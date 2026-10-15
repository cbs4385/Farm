using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-15: the trees of the outdoor maps are sorted with the farmer by height, in every map that has them.
    public class TreeSortingFlowTests : PlayModeFixture
    {
        [UnityTest]
        public IEnumerator InEveryOutdoorMap_TheTreesAndTheFarmerShareOneOrder_AndTheCameraSortsByHeight([ValueSource(nameof(Maps))] string map)
        {
            if (GameContext.Current?.Session == null || !GameContext.Current.Session.InGame)
            {
                Bootstrapper.InitializeServices();
                yield return null;
                var s = GameContext.Current.Session;
                s.BeginNewGame("Tester", "Test Farm", 0);
                s.SetFlag(FatigueModel.WarnedFlag);
            }
            GameContext.Current.Session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var player = Object.FindAnyObjectByType<PlayerController>().GetComponent<SpriteRenderer>();
            var nodes = Object.FindAnyObjectByType<FarmMap>().Nodes.GetComponent<TilemapRenderer>();
            Assert.AreEqual(player.sortingOrder, nodes.sortingOrder, map + ": the node trees (tiles) sort with the player");
            Assert.AreEqual(TilemapRenderer.Mode.Individual, nodes.mode, map + ": one tile at a time, not a chunk");
            Assert.AreEqual(TransparencySortMode.CustomAxis, Camera.main.transparencySortMode);
            Assert.That(Vector3.Distance(WorldSorting.Axis.normalized, Camera.main.transparencySortAxis.normalized), Is.LessThan(1e-4f), "the camera keeps the axis (it stores it normalised)");

            foreach (var tree in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include).Where(r => r.enabled && r.sprite != null && (r.sprite.name.StartsWith("tree_") || r.sprite.name == SeasonalTrees.TreeSprite)))
                Assert.AreEqual(player.sortingOrder, tree.sortingOrder, map + ": the picture of a tree at " + tree.transform.position + " sorts with the player");
        }

        static readonly string[] Maps = { MapIds.Forest, MapIds.Farm, MapIds.Village, MapIds.Beach };
    }
}
