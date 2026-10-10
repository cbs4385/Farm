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
    public class FarmBuildingsFlowTests
    {
        string _root;
        GameSession _s;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-buildings-" + Guid.NewGuid().ToString("N"));
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
                Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(cell.x - 1, at.Y, 0)), "a wall beside the door");
                Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(cell.x - 1, at.Y + type.H - 1, 0)), "up to the roof");
                Assert.IsNull(map.Walls.GetTile(cell), "no wall in the doorway");
                var spawn = UnityEngine.Object.FindObjectsByType<SpawnPoint>().FirstOrDefault(sp => sp.Id == type.ReturnSpawn);
                Assert.IsNotNull(spawn, type.Id + " has the spawn point where one comes out");
            }
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
            var oldCorner = new Vector3Int(oldDoor.x - 1, at.Y, 0);                                // beside the door: a wall of the picture
            var groundUnderTheOldDoor = map.Ground.GetTile(new Vector3Int(oldDoor.x, oldDoor.y, 0));
            Assert.IsNotNull(groundUnderTheOldDoor);

            at.X = 50;
            at.Y = 30;
            view.Rebuild(_s);

            Assert.IsNull(map.Walls.GetTile(oldCorner), "the old walls are gone");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(oldCorner.x, oldCorner.y + coop.H - 1, 0)), "and the old roof");
            Assert.AreNotEqual("tile_door", map.Ground.GetTile(new Vector3Int(oldDoor.x, oldDoor.y, 0))?.name, "the old doorway is ground again");
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(51, 30, 0)), "walls at the new place");
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
