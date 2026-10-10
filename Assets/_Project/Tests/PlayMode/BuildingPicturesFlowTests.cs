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
    // Owner, 2026-10-09: "the buildings still look very flat". Every building is a picture from the Cozy Village kit standing on its footprint, with collision under
    // it and an opening at its door. In the real maps.
    public class BuildingPicturesFlowTests : PlayModeFixture
    {

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

        static void AssertStands(FarmMap map, string pictureName, int doorX, int doorY)
        {
            var picture = Named(pictureName);
            Assert.IsNotNull(picture, pictureName + " is a picture in the scene");
            var renderer = picture.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(renderer.sprite, pictureName + " has its art");
            Assert.GreaterOrEqual(renderer.sortingOrder, 11, "drawn over a player who stands behind it");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(doorX, doorY, 0)), pictureName + ": the door cell is open");
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(doorX - 1, doorY, 0)), pictureName + ": solid beside the door on the left");
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(doorX + 1, doorY, 0)), pictureName + ": and on the right");
            Assert.IsNotNull(map.Walls.GetTile(new Vector3Int(doorX, doorY + 2, 0)), pictureName + ": and above the door");
            Assert.IsNull(map.Walls.GetTile(new Vector3Int(doorX, doorY - 1, 0)), pictureName + ": the cell outside is free");
        }

        [UnityTest]
        public IEnumerator EveryShop_StandsAsAPicture_WithItsDoorOpen_AllNorthOfTheRoad()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var (id, x, y) in new[]
            {
                (MapIds.GeneralStore, 8, 24), (MapIds.Blacksmith, 17, 24), (MapIds.Carpenter, 32, 24), (MapIds.Library, 42, 24),
                (MapIds.Saloon, 52, 24), (MapIds.Clinic, 67, 24), (MapIds.CommunityHall, 37, 31),
            })
                AssertStands(map, id + "_Building", x, y);
        }

        [UnityTest]
        public IEnumerator EveryVillagersCottage_StandsAsAPicture()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var home in NpcHomes.All) AssertStands(map, home.Map + "_Building", home.DoorX, home.DoorY);
        }

        [UnityTest]
        public IEnumerator OnTheFarm_TheFarmhouseTheCoopAndTheBarn_AreKitPictures()
        {
            yield return Open(MapIds.Farm);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            AssertStands(map, "Farmhouse_Building", 7, 20);
            Assert.IsNotNull(Named("Picture_coop"), "the chicken coop");
            Assert.IsNotNull(Named("Picture_barn"), "the barn");
        }
    }
}
