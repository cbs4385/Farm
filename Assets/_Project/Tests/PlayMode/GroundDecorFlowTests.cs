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
    // Owner's feedback, 2026-10-09: "the world looks flat". In the real maps: tufts and flowers over the grass and not on paths or water, in the colours of the
    // season; none indoors; and the farm and the village have a pond.
    public class GroundDecorFlowTests : PlayModeFixture
    {

        [SetUp]
        public void SetUpMore()
        {
            GroundDecor.ResetForTests();
        }

        IEnumerator Open(string map, Season season = Season.Spring)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, season, 10, 8 * 60));
            session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;
        }

        static System.Collections.Generic.IEnumerable<Vector3Int> All(BoundsInt bounds) { foreach (var c in bounds.allPositionsWithin) yield return c; }

        static GroundDecor Decor => UnityEngine.Object.FindAnyObjectByType<GroundDecor>();

        [UnityTest]
        public IEnumerator TheFarm_HasTuftsAndFlowers_OnlyOnGrass_AndAPond()
        {
            yield return Open(MapIds.Farm);
            Assert.IsNotNull(Decor, "the farm has decoration");
            var layer = Decor.Layer;
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var cells = All(layer.cellBounds).Where(c => layer.GetTile(c) != null).ToList();
            Assert.Greater(cells.Count, 300, "plenty of it");
            foreach (var c in cells)
            {
                Assert.AreEqual("tile_grass", map.Ground.GetTile(c).name, "only on grass: " + c);
                Assert.IsNull(map.Walls.GetTile(c), "not in a wall: " + c);
            }
            Assert.IsTrue(cells.Any(c => layer.GetTile(c).name.Contains("_f")), "some flowers");
            Assert.IsTrue(cells.All(c => layer.GetTile(c).name.StartsWith("decor_spring_")), "spring colours");

            var water = All(map.Ground.cellBounds).Count(c => map.IsWater(c));
            Assert.Greater(water, 40, "the farm has a pond");
        }

        [UnityTest]
        public IEnumerator InTheFall_TheColoursAreFallColours()
        {
            yield return Open(MapIds.Farm, Season.Fall);
            var layer = Decor.Layer;
            var names = All(layer.cellBounds).Select(c => layer.GetTile(c)).Where(t => t != null).Select(t => t.name).ToList();
            Assert.IsNotEmpty(names);
            Assert.IsTrue(names.All(n => n.StartsWith("decor_fall_")));
        }

        [UnityTest]
        public IEnumerator TheVillage_HasADecoratedGreen_APond_AndPlantersAtTheShopDoors()
        {
            yield return Open(MapIds.Village);
            Assert.IsNotNull(Decor);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            Assert.Greater(All(map.Ground.cellBounds).Count(c => map.IsWater(c)), 20, "a pond");
            var planters = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.Contains("_Planter"));
            Assert.GreaterOrEqual(planters, 10, "a planter on each side of every shop's door");
        }

        [UnityTest]
        public IEnumerator TheBeach_HasPebblesAndDriftwoodOnTheSand_AndAShoreThatIsNotARuler()
        {
            yield return Open(MapIds.Beach);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var layer = Decor.Layer;
            var sandCells = All(layer.cellBounds).Where(c => layer.GetTile(c) != null && map.Ground.GetTile(c).name == "tile_sand").ToList();
            Assert.Greater(sandCells.Count, 20, "decoration on the sand");
            Assert.IsTrue(sandCells.All(c => layer.GetTile(c).name.StartsWith("decor_sand_")), "pebbles and driftwood, whatever the season");
            // The waterline wanders: the highest water row differs from column to column.
            var edge = Enumerable.Range(2, 30).Select(x => Enumerable.Range(0, 12).Count(y => map.IsWater(new Vector3Int(x, y, 0)))).Distinct().Count();
            Assert.GreaterOrEqual(edge, 3, "the shoreline has bays and points");
        }

        [UnityTest]
        public IEnumerator TheVillage_HasItsClockTowerFountainLampsAndBenches_DrawnOverAPlayerBehindThem()
        {
            yield return Open(MapIds.Village);
            Transform Named(string n) => Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t => t.name == n && t.gameObject.scene.IsValid());
            foreach (var n in new[] { "ClockTower", "Fountain", "Bench_1", "Lamp_N11", "Lamp_S12", "Flag_West" }) Assert.IsNotNull(Named(n), n);
            Assert.GreaterOrEqual(Named("ClockTower").GetComponent<SpriteRenderer>().sortingOrder, 11, "the tower hides a player who stands behind it (the player is at 10)");
            Assert.GreaterOrEqual(Resources.FindObjectsOfTypeAll<Transform>().Count(t => t.name.StartsWith("Lamp_") && t.gameObject.scene.IsValid()), 10, "lamps along the road");
        }

        [UnityTest]
        public IEnumerator TheTreesOfTheForest_AreOfTheSeasonsKinds()
        {
            yield return Open(MapIds.Forest, Season.Fall);
            var names = Resources.FindObjectsOfTypeAll<SpriteRenderer>().Where(r => r.gameObject.scene.IsValid() && r.name.StartsWith("Tree_") && r.sprite != null).Select(r => r.sprite.name).ToList();
            Assert.Greater(names.Count, 50, "a wood");
            Assert.IsTrue(names.Any(n => n.StartsWith("tree_fall_")), "fall trees");
            Assert.IsFalse(names.Any(n => n.StartsWith("tree_spring_") || n.StartsWith("tree_winter_")), "and no others");
            Assert.IsTrue(names.Any(n => n == "obj_tree"), "some pines stay");
        }

        [UnityTest]
        public IEnumerator Indoors_ThereIsNoDecoration()
        {
            yield return Open(MapIds.FarmHouse);
            Assert.IsNull(Decor);
        }
    }
}
