using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The builder's mallet: lift a building, the bed or something set down, see where it may go, put it down or put it back.
    public class HammerFlowTests
    {
        string _root;
        GameSession _s;
        HammerMode _hammer;
        FarmMap _map;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-hammer-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Start(string map, string spawn = "default")
        {
            if (!ServiceLocator.TryGet<GameSession>(out _s) || !_s.InGame)
            {
                Bootstrapper.InitializeServices();
                yield return null;
                _s = ServiceLocator.Get<GameSession>();
                _s.BeginNewGame("Tester", "Test Farm", 0);
                _s.SetFlag(FatigueModel.WarnedFlag);
            }
            _s.State.CurrentMap = map;
            _s.State.SpawnPoint = spawn;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            _map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            _hammer = UnityEngine.Object.FindAnyObjectByType<PlayerActions>().GetComponent<HammerMode>();
            for (var i = 0; i < _s.Backpack.Capacity; i++)
                if (_s.Backpack.Get(i)?.ItemId == ItemIds.Hammer) _s.State.SelectedHotbar = i;
        }

        // The farm starts with trees, rocks and weeds scattered about: clear a patch so that the tests do not depend on chance.
        void ClearPatch(int x0, int y0, int x1, int y1)
        {
            var nodes = _s.GetNodes(MapIds.Farm);
            for (var x = x0; x <= x1; x++)
                for (var y = y0; y <= y1; y++) nodes.Remove(x, y);
        }

        [UnityTest]
        public IEnumerator ANewGame_StartsWithTheMallet()
        {
            yield return Start(MapIds.Farm);
            Assert.GreaterOrEqual(_s.Backpack.Count(ItemIds.Hammer), 1);
            Assert.IsNotNull(_hammer);
        }

        [UnityTest]
        public IEnumerator ABuilding_CanBeLifted_Refused_PutBack_AndPutDownElsewhere()
        {
            yield return Start(MapIds.Farm);
            ClearPatch(50, 25, 70, 40);
            var coop = FarmBuildings.Coop;
            var at = FarmBuildings.Find(_s.State, "coop");
            var wall = new Vector3Int(at.X, at.Y + 1, 0);
            Assert.IsNotNull(_map.Walls.GetTile(wall));

            _hammer.Use(wall);
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("coop", _hammer.CarryingBuilding);
            Assert.IsNull(_map.Walls.GetTile(wall), "its walls are lifted away");
            Assert.AreEqual(26, FarmBuildings.Find(_s.State, "coop").X, "the saved state still says where it stood");

            // On the farmhouse: refused, still carrying.
            _hammer.Use(new Vector3Int(8, 21, 0));
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("build.refused.onhouse", _hammer.LastRefusal);
            Assert.AreEqual(26, FarmBuildings.Find(_s.State, "coop").X);

            // Interact puts it back.
            Assert.IsTrue(_hammer.Cancel());
            Assert.IsFalse(_hammer.Carrying);
            Assert.IsNotNull(_map.Walls.GetTile(wall), "back where it was");

            // Lift again and put it down with its door at (60, 30).
            _hammer.Use(wall);
            yield return null;
            _hammer.Use(new Vector3Int(60, 30, 0));
            Assert.IsFalse(_hammer.Carrying, "put down: " + _hammer.LastRefusal);
            var moved = FarmBuildings.Find(_s.State, "coop");
            Assert.AreEqual((58, 30), (moved.X, moved.Y));
            Assert.IsNull(_map.Walls.GetTile(wall));
            Assert.IsNotNull(_map.Walls.GetTile(new Vector3Int(58, 30, 0)));
            Assert.AreEqual("tile_door", _map.Ground.GetTile(new Vector3Int(60, 30, 0)).name);
        }

        [UnityTest]
        public IEnumerator ABuilding_CannotBePutDown_OnATree_OnDugSoil_OrWithNoWayIn()
        {
            yield return Start(MapIds.Farm);
            ClearPatch(50, 25, 70, 40);
            var at = FarmBuildings.Find(_s.State, "barn");
            _hammer.Use(new Vector3Int(at.X, at.Y + 1, 0));
            Assert.IsTrue(_hammer.Carrying);

            _s.GetGrid(MapIds.Farm).Till(61, 33);
            _hammer.Use(new Vector3Int(60, 30, 0));                // the barn would stand on the dug square
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("build.refused.blocked", _hammer.LastRefusal);

            _hammer.Use(new Vector3Int(60, 1, 0));                 // too near the south edge for a door and the way in
            Assert.AreEqual("build.refused.offmap", _hammer.LastRefusal);
            Assert.IsTrue(_hammer.Cancel());
        }

        [UnityTest]
        public IEnumerator TheBed_CanBeMoved_AndStaysWhereItWasPut_AfterTheRoomIsLeftAndEnteredAgain()
        {
            yield return Start(MapIds.FarmHouse);
            var bed = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "bed");
            var home = bed.Cell(_map);
            _hammer.Use(home + new Vector3Int(1, 1, 0));                   // any of the four cells of the double bed lifts it
            Assert.IsTrue(_hammer.Carrying);
            var target = new Vector3Int(8, 4, 0);
            _hammer.Use(target);
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual(target, bed.Cell(_map));
            var sleepSpawn = UnityEngine.Object.FindObjectsByType<SpawnPoint>().First(sp => sp.Id == "bed");
            Assert.AreEqual(new Vector3Int(10, 4, 0), _map.WorldToCell(sleepSpawn.transform.position), "one wakes up beside the bed");

            yield return Start(MapIds.FarmHouse);                  // load the room again
            var bedAgain = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "bed");
            Assert.AreEqual(target, bedAgain.Cell(_map), "the bed is where it was put");
        }

        [UnityTest]
        public IEnumerator AChest_KeepsItsContents_WhenItIsMoved()
        {
            yield return Start(MapIds.FarmHouse);
            var objects = _s.GetObjects(MapIds.FarmHouse);
            var chest = objects.Place(_s.Placeables.Get(CraftingDefaults.Chest), 5, 4, "c1");
            PlacedObjectsView.Current.Spawn(chest);
            objects.ChestOf(chest).Add("resource.wood", 7);

            _hammer.Use(new Vector3Int(5, 4, 0));
            Assert.IsTrue(_hammer.Carrying);
            Assert.IsNull(PlacedObjectsView.Current.At(new Vector3Int(5, 4, 0)), "lifted off the floor");
            _hammer.Use(new Vector3Int(3, 3, 0));
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual((3, 3), (objects.ById("c1").X, objects.ById("c1").Y));
            Assert.AreEqual(7, objects.ChestOf(objects.ById("c1")).Count("resource.wood"));
            Assert.IsNotNull(PlacedObjectsView.Current.At(new Vector3Int(3, 3, 0)));
        }

        [UnityTest]
        public IEnumerator SwingingAtNothing_SaysSo()
        {
            yield return Start(MapIds.FarmHouse);
            _hammer.Use(new Vector3Int(6, 6, 0));
            Assert.IsFalse(_hammer.Carrying);
        }
    }
}
