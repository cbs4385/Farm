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
    // The farm's buildings are drawn from the saved game state: a moved building has its walls, roof and door at the new place and nothing is left
    // behind at the old one.
    public class FarmBuildingsFlowTests : PlayModeFixture
    {
        GameSession _s;

        IEnumerator Start()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.State.CurrentMap = MapIds.Farm;
            _s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static Warp DoorTo(string map) => UnityEngine.Object.FindObjectsByType<Warp>().FirstOrDefault(w => w.TargetMap == map);

        [UnityTest]
        public IEnumerator TheBuildings_StandAtTheirDefaultPlaces_WithDoorsThatLeadInside()
        {
            yield return Start();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var type in FarmBuildings.Types)
            {
                var at = FarmBuildings.Find(_s.State, type.Id);
                Assert.IsNotNull(at, type.Id);
                var door = DoorTo(type.InteriorMap);
                Assert.IsNotNull(door, type.Id + " has a door");
                var cell = map.WorldToCell(door.transform.position);
                Assert.AreEqual(FarmBuildings.DoorCell(type, at), (cell.x, cell.y));
                var (side, top) = WallCellsOf(type.Id, cell.x, cell.y, cell.x - 1, at.Y, cell.x - 1, at.Y + type.H - 1);
                Assert.IsNotNull(map.Walls.GetTile(side), "a wall beside the door");
                Assert.IsNotNull(map.Walls.GetTile(top), "up to the roof");
                Assert.IsNull(map.Walls.GetTile(cell), "no wall in the doorway");
                var spawn = UnityEngine.Object.FindObjectsByType<SpawnPoint>().FirstOrDefault(sp => sp.Id == type.ReturnSpawn);
                Assert.IsNotNull(spawn, type.Id + " has the spawn point where one comes out");
            }
        }

        // A cell beside the door and one at the top that a building blocks: from its picture's mask when it has one (the coop and the barn), else the given cells
        // (the greenhouse, drawn from tiles).
        static (Vector3Int side, Vector3Int top) WallCellsOf(string type, int doorX, int doorY, int sideX, int sideY, int topX, int topY)
        {
            var mask = BuildingMasks.Get(type);
            if (mask == null) return (new Vector3Int(sideX, sideY, 0), new Vector3Int(topX, topY, 0));
            var cells = mask.SolidCells().ToList();
            var side = cells.Where(c => c.y == 0).OrderBy(c => Mathf.Abs(c.x)).First();
            var top = cells.OrderByDescending(c => c.y).First();
            return (new Vector3Int(doorX + side.x, doorY + side.y, 0), new Vector3Int(doorX + top.x, doorY + top.y, 0));
        }

        [UnityTest]
        public IEnumerator AMovedBuilding_IsRedrawnThere_AndLeavesNothingBehind()
        {
            yield return Start();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var view = UnityEngine.Object.FindAnyObjectByType<FarmBuildingsView>();
            var coop = FarmBuildings.Coop;
            var at = FarmBuildings.Find(_s.State, "coop");
            var oldDoor = FarmBuildings.DoorCell(coop, at);
            var (oldCorner, oldTop) = WallCellsOf("coop", oldDoor.x, oldDoor.y, oldDoor.x - 1, at.Y, oldDoor.x - 1, at.Y + coop.H - 1);          // beside the door: a wall of the picture
            var groundUnderTheOldDoor = map.Ground.GetTile(new Vector3Int(oldDoor.x, oldDoor.y, 0));
            Assert.IsNotNull(groundUnderTheOldDoor);

            at.X = 50;
            at.Y = 30;
            view.Rebuild(_s);

            Assert.IsNull(map.Walls.GetTile(oldCorner), "the old walls are gone");
            Assert.IsNull(map.Walls.GetTile(oldTop), "and the old roof");
            Assert.AreNotEqual("tile_door", map.Ground.GetTile(new Vector3Int(oldDoor.x, oldDoor.y, 0))?.name, "the old doorway is ground again");
            Assert.IsNotNull(map.Walls.GetTile(WallCellsOf("coop", 52, 30, 51, 30, 51, 34).side), "walls at the new place");
            Assert.AreEqual("tile_door", map.Ground.GetTile(new Vector3Int(52, 30, 0)).name);
            var door = DoorTo(MapIds.Coop);
            Assert.AreEqual(new Vector3Int(52, 30, 0), map.WorldToCell(door.transform.position), "the door warp moved with it");
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsByType<Warp>().Count(w => w.TargetMap == MapIds.Coop), "only one coop door");
        }

        [UnityTest]
        public IEnumerator ALockedDoor_StaysShut_UntilTheBuildingIsBuilt_WhereverItStands()
        {
            yield return Start();
            var door = DoorTo(MapIds.Barn);
            Assert.IsFalse(door.IsOpen(out _), "the barn is not built yet");
            _s.SetFlag(AnimalRules.BuildingFlag(MapIds.Barn));
            Assert.IsTrue(door.IsOpen(out _));
            FarmBuildings.Find(_s.State, "barn").X = 60;
            UnityEngine.Object.FindAnyObjectByType<FarmBuildingsView>().Rebuild(_s);
            Assert.IsTrue(DoorTo(MapIds.Barn).IsOpen(out _), "still open after it is moved");
        }
    }
}
