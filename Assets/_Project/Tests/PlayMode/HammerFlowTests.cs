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
    public class HammerFlowTests : PlayModeFixture
    {
        GameSession _s;
        HammerMode _hammer;
        FarmMap _map;

        IEnumerator Start(string map, string spawn = "default")
        {
            if ((_s = GameContext.Current?.Session) == null || !_s.InGame)
            {
                Bootstrapper.InitializeServices();
                yield return null;
                _s = GameContext.Current.Session;
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

        // The farm starts with trees, rocks and weeds scattered about at random: clear a patch so that the tests do not depend on chance. The cleared cells are redrawn
        // (their colliders go with the tiles) and a couple of frames pass, or a tree that stood under the target would still block it.
        IEnumerator ClearPatch(int x0, int y0, int x1, int y1)
        {
            var nodes = _s.GetNodes(MapIds.Farm);
            var view = UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
            for (var x = x0; x <= x1; x++)
                for (var y = y0; y <= y1; y++)
                {
                    nodes.Remove(x, y);
                    if (view != null) view.RefreshNode(new Vector3Int(x, y, 0));
                }
            yield return null;
            yield return null;
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
            yield return ClearPatch(50, 25, 70, 40);
            var coop = FarmBuildings.Coop;
            var at = FarmBuildings.Find(_s.State, "coop");
            var wall = new Vector3Int(at.X + 1, at.Y + 1, 0);                       // a cell the coop's picture covers
            Assert.IsNotNull(_map.Walls.GetTile(wall));

            _hammer.Use(wall);
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("coop", _hammer.CarryingBuilding);
            Assert.IsNull(_map.Walls.GetTile(wall), "its walls are lifted away");
            Assert.AreEqual(FarmBuildings.Coop.DefaultX, FarmBuildings.Find(_s.State, "coop").X, "the saved state still says where it stood");

            // On the farmhouse: refused, still carrying.
            _hammer.Use(new Vector3Int(FarmBuildings.HouseX0 + 1, 21, 0));
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("build.refused.onhouse", _hammer.LastRefusal);
            Assert.AreEqual(FarmBuildings.Coop.DefaultX, FarmBuildings.Find(_s.State, "coop").X);

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
            Assert.IsNotNull(_map.Walls.GetTile(new Vector3Int(59, 31, 0)), "its picture stands on the new place");
            Assert.AreEqual("tile_door", _map.Ground.GetTile(new Vector3Int(60, 30, 0)).name);
        }

        // The greenhouse is the widest and has the deepest footprint (9 x 5): it is lifted, refused on the house and on its neighbours, and put down like the others,
        // its picture, its cells and its door moving with it.
        [UnityTest]
        public IEnumerator TheGreenhouse_IsLifted_RefusedOnTheHouseAndOnTheBarn_AndPutDownWithItsPictureAndDoorMoved()
        {
            yield return Start(MapIds.Farm);
            yield return ClearPatch(40, 25, 75, 45);
            var type = FarmBuildings.Greenhouse;
            var at = FarmBuildings.Find(_s.State, "greenhouse");
            var oldDoor = FarmBuildings.DoorCell(type, at);
            var mask = BuildingMasks.Get("greenhouse");
            var wall = new Vector3Int(oldDoor.x + 2, oldDoor.y + 1, 0);                  // a cell its picture covers
            Assert.IsTrue(mask.Blocks(2, 1));
            Assert.IsNotNull(_map.Walls.GetTile(wall));

            _hammer.Use(wall);
            Assert.IsTrue(_hammer.Carrying);
            Assert.AreEqual("greenhouse", _hammer.CarryingBuilding);
            Assert.IsNull(_map.Walls.GetTile(wall), "its walls are lifted away");
            Assert.IsNull(GameObject.Find("Picture_greenhouse"), "and its picture");

            _hammer.Use(new Vector3Int(FarmBuildings.HouseX0 + 1, 21, 0));                // the door cell on the farmhouse
            Assert.AreEqual("build.refused.onhouse", _hammer.LastRefusal);
            var barn = FarmBuildings.Find(_s.State, "barn");
            _hammer.Use(new Vector3Int(barn.X + FarmBuildings.Barn.DoorX + 2, barn.Y, 0));          // so that it would overlap the barn
            Assert.AreEqual("build.refused.onbuilding", _hammer.LastRefusal);
            Assert.IsTrue(_hammer.Carrying);

            yield return null;
            _hammer.Use(new Vector3Int(52, 33, 0));
            Assert.IsFalse(_hammer.Carrying, "put down: " + _hammer.LastRefusal);
            var moved = FarmBuildings.Find(_s.State, "greenhouse");
            Assert.AreEqual((48, 33), (moved.X, moved.Y), "its door stands on the cell that was aimed at");
            Assert.IsNotNull(GameObject.Find("Picture_greenhouse"), "the picture stands at the new place");
            Assert.IsNull(_map.Walls.GetTile(wall), "nothing is left at the old place");
            foreach (var c in mask.SolidCells())
                Assert.IsNotNull(_map.Walls.GetTile(new Vector3Int(52 + c.x, 33 + c.y, 0)), $"the cell {c.x},{c.y} of the picture blocks at the new place");
            Assert.AreEqual("tile_door", _map.Ground.GetTile(new Vector3Int(52, 33, 0)).name);
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsByType<Warp>().Count(w => w.TargetMap == MapIds.Greenhouse), "one greenhouse door");
        }

        [UnityTest]
        public IEnumerator ABuilding_CannotBePutDown_OnATree_OnDugSoil_OrWithNoWayIn()
        {
            yield return Start(MapIds.Farm);
            yield return ClearPatch(50, 25, 70, 40);
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
        public IEnumerator EveryPieceOfFurniture_CanBeMoved_AndTheRugCanLieUnderAnotherPiece()
        {
            yield return Start(MapIds.FarmHouse);
            var couch = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_couch");
            _hammer.Use(couch.Cell(_map));
            Assert.IsTrue(_hammer.Carrying);
            var target = new Vector3Int(3, 5, 0);
            _hammer.Use(target);
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual(target, couch.Cell(_map));

            // The rug goes under the couch (a walkable piece ignores the solid one), and the couch is lifted before the rug.
            var rug = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_ruglarge");
            _hammer.Use(rug.Cell(_map));
            Assert.IsTrue(_hammer.Carrying);
            _hammer.Use(new Vector3Int(3, 4, 0));
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            _hammer.Use(target);
            Assert.IsTrue(_hammer.Carrying);
            Assert.IsTrue(_hammer.Cancel());
            Assert.AreEqual(target, couch.Cell(_map), "the couch, not the rug, was lifted and went back");

            yield return Start(MapIds.FarmHouse);
            var again = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_couch");
            Assert.AreEqual(target, again.Cell(_map), "the couch is where it was put");
        }

        [UnityTest]
        public IEnumerator TheShippingBin_CanBeMovedNextToTheHouse_AndStaysThere()
        {
            yield return Start(MapIds.Farm);
            // The farm's clutter is random: put a tree and a rock exactly where the bin is to go, so that clearing the patch has to really clear them (this used to fail by chance).
            var view = UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
            foreach (var (x, y, id) in new[] { (9, 16, NodeDefaults.Tree), (10, 17, NodeDefaults.Rock) })
            {
                _s.GetNodes(MapIds.Farm).Add(x, y, _s.Nodes.Get(id));
                view.RefreshNode(new Vector3Int(x, y, 0));
            }
            yield return null;
            yield return ClearPatch(6, 13, 12, 19);
            var bin = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "shipping_bin");
            var home = bin.Cell(_map);
            _hammer.Use(home + new Vector3Int(1, 1, 0));                     // any of its four cells lifts it
            Assert.IsTrue(_hammer.Carrying);
            var target = new Vector3Int(9, 16, 0);
            _hammer.Use(target);
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual(target, bin.Cell(_map));
            Physics2D.SyncTransforms();                                       // the physics world learns of the move at the next step otherwise
            Assert.IsNotNull(Physics2D.OverlapPoint(_map.CellCenter(new Vector3Int(10, 17, 0))), "its collider moved with it");

            yield return Start(MapIds.Farm);                                  // leave and come back
            var again = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "shipping_bin");
            Assert.AreEqual(target, again.Cell(_map), "the bin is where it was put");
        }

        [UnityTest]
        public IEnumerator TheCouch_CanBeTurned_WhileCarried_AndStaysTurned()
        {
            yield return Start(MapIds.FarmHouse);
            var couch = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_couch");
            _hammer.Use(couch.Cell(_map));
            Assert.IsTrue(_hammer.Carrying);
            Assert.IsTrue(_hammer.Rotate(), "R turns what is carried");
            var target = new Vector3Int(7, 5, 0);                               // a free spot: the turned couch stands two cells high
            _hammer.Use(target);
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual(1, couch.Turns);
            Assert.AreEqual(new Vector2Int(1, 2), couch.CurrentSize);
            Assert.AreEqual(target, couch.Cell(_map));
            Assert.AreEqual(90f, couch.transform.eulerAngles.z, 0.1f, "its picture turned too");
            Physics2D.SyncTransforms();
            Assert.IsNotNull(Physics2D.OverlapPoint(_map.CellCenter(new Vector3Int(7, 6, 0))), "and so did its collider: it now covers the cell above");
            Assert.IsNull(Physics2D.OverlapPoint(_map.CellCenter(new Vector3Int(6, 5, 0))), "and no longer the cell beside");

            yield return Start(MapIds.FarmHouse);
            var again = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_couch");
            Assert.AreEqual(1, again.Turns, "the turn is saved");
            Assert.AreEqual(target, again.Cell(_map));
        }

        [UnityTest]
        public IEnumerator ABuilding_CannotBeTurned()
        {
            yield return Start(MapIds.Farm);
            yield return ClearPatch(50, 25, 70, 40);
            var at = FarmBuildings.Find(_s.State, "barn");
            _hammer.Use(new Vector3Int(at.X, at.Y + 1, 0));
            Assert.IsTrue(_hammer.Carrying);
            Assert.IsFalse(_hammer.Rotate());
            Assert.IsTrue(_hammer.Cancel());
        }

        [UnityTest]
        public IEnumerator FurnitureFromTheHotbar_IsSetDownTurned_AfterR()
        {
            yield return Start(MapIds.FarmHouse);
            var actions = _hammer.GetComponent<PlayerActions>();
            var id = ItemIds.Machine(CraftingDefaults.Decor[0].id);
            _s.Backpack.Add(id, 2);
            var slot = Enumerable.Range(0, _s.Backpack.Capacity).First(i => _s.Backpack.Get(i)?.ItemId == id);
            _s.State.SelectedHotbar = slot;
            actions.TurnPlacement();
            Assert.AreEqual(1, actions.PlacementTurns);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            player.Teleport(new Vector3(6.5f, 2.5f, 0f));
            player.Face(Vector2Int.right);
            yield return null;
            actions.UseSelected();
            var placed = _s.GetObjects(MapIds.FarmHouse).At(7, 2);
            Assert.IsNotNull(placed, "it was set down in front of the player");
            Assert.AreEqual(1, placed.Turns);
            Assert.AreEqual(90f, GameContext.Current.Map.Placed.At(new Vector3Int(7, 2, 0)).transform.eulerAngles.z, 0.1f);

            // The mallet takes it into the backpack (owner, 2026-10-12), and it can be set down again, turned as before.
            var before = _s.Backpack.Count(id);
            _hammer.Use(new Vector3Int(7, 2, 0));
            Assert.IsFalse(_hammer.Carrying, "it is not carried: it is in the backpack");
            Assert.IsNull(_s.GetObjects(MapIds.FarmHouse).At(7, 2));
            Assert.IsNull(GameContext.Current.Map.Placed.At(new Vector3Int(7, 2, 0)));
            Assert.AreEqual(before + 1, _s.Backpack.Count(id));
        }

        [UnityTest]
        public IEnumerator AChest_IsStillCarried_WhileADecorationGoesIntoTheBackpack()
        {
            yield return Start(MapIds.FarmHouse);
            var objects = _s.GetObjects(MapIds.FarmHouse);
            var decor = objects.Place(_s.Placeables.Get(CraftingDefaults.Decor[0].id), 3, 3, "d1");
            GameContext.Current.Map.Placed.Spawn(decor);
            Assert.IsTrue(HammerMode.TakesToBackpack(_s.Placeables.Get(decor.TypeId).Kind));
            var chest = objects.Place(_s.Placeables.Get(CraftingDefaults.Chest), 5, 4, "c2");
            GameContext.Current.Map.Placed.Spawn(chest);
            Assert.IsFalse(HammerMode.TakesToBackpack(_s.Placeables.Get(chest.TypeId).Kind), "a chest holds things, so it is carried");
            _hammer.Use(new Vector3Int(5, 4, 0));
            Assert.IsTrue(_hammer.Carrying);
            _hammer.Cancel();
        }

        [UnityTest]
        public IEnumerator WithAFullBackpack_ADecoration_StaysWhereItIs()
        {
            yield return Start(MapIds.FarmHouse);
            var def = _s.Placeables.Get(CraftingDefaults.Decor[0].id);
            var objects = _s.GetObjects(MapIds.FarmHouse);
            var decor = objects.Place(def, 7, 2, "d2");
            GameContext.Current.Map.Placed.Spawn(decor);
            _s.Backpack.Remove(def.ItemId, _s.Backpack.Count(def.ItemId));
            for (var i = 0; i < 500 && _s.Backpack.CanAdd(def.ItemId, 1); i++)
                _s.Backpack.Add(ItemIds.Wood, 1, 0, "fill" + i);          // each mark is a stack of its own: one slot each
            Assert.IsFalse(_s.Backpack.CanAdd(def.ItemId, 1), "the backpack is full");
            _hammer.Use(new Vector3Int(7, 2, 0));
            Assert.IsFalse(_hammer.Carrying);
            Assert.IsNotNull(objects.At(7, 2), "it stays where it is");
            Assert.IsNotNull(GameContext.Current.Map.Placed.At(new Vector3Int(7, 2, 0)));
            Assert.AreEqual(0, _s.Backpack.Count(def.ItemId));
        }

        [UnityTest]
        public IEnumerator TheFarmsHayBale_GoesIntoTheBackpack_AndStaysGone()
        {
            yield return Start(MapIds.Farm);
            var hay = UnityEngine.Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "farm_hay");
            var before = _s.Backpack.Count(ItemIds.Machine("hay"));
            _hammer.Use(hay.Cell(_map));
            Assert.IsFalse(_hammer.Carrying, "it is taken into the backpack, not carried");
            Assert.AreEqual(before + 1, _s.Backpack.Count(ItemIds.Machine("hay")));
            Assert.IsFalse(hay.gameObject.activeSelf, "gone from the farm");
            Assert.IsTrue(hay.IsTaken(_s, _map), "and remembered, so that it stays gone");
        }

        [UnityTest]
        public IEnumerator AChest_KeepsItsContents_WhenItIsMoved()
        {
            yield return Start(MapIds.FarmHouse);
            var objects = _s.GetObjects(MapIds.FarmHouse);
            var chest = objects.Place(_s.Placeables.Get(CraftingDefaults.Chest), 5, 4, "c1");
            GameContext.Current.Map.Placed.Spawn(chest);
            objects.ChestOf(chest).Add("resource.wood", 7);

            _hammer.Use(new Vector3Int(5, 4, 0));
            Assert.IsTrue(_hammer.Carrying);
            Assert.IsNull(GameContext.Current.Map.Placed.At(new Vector3Int(5, 4, 0)), "lifted off the floor");
            _hammer.Use(new Vector3Int(3, 3, 0));
            Assert.IsFalse(_hammer.Carrying, _hammer.LastRefusal);
            Assert.AreEqual((3, 3), (objects.ById("c1").X, objects.ById("c1").Y));
            Assert.AreEqual(7, objects.ChestOf(objects.ById("c1")).Count("resource.wood"));
            Assert.IsNotNull(GameContext.Current.Map.Placed.At(new Vector3Int(3, 3, 0)));
        }

        [UnityTest]
        public IEnumerator SwingingAtNothing_SaysSo()
        {
            yield return Start(MapIds.FarmHouse);
            _hammer.Use(new Vector3Int(4, 5, 0));      // bare floor: the fireplace stands at (5..6, 6..7)
            Assert.IsFalse(_hammer.Carrying);
        }
    }
}
