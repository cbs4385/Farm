using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-07: the map did not match the world. The picture follows the real geography and puts each building where its door is.
    public class WorldMapLayoutTests
    {
        static WorldMapLayout.Region Farm => WorldMapLayout.Farm;
        static WorldMapLayout.Region Village => WorldMapLayout.Village;

        [Test]
        public void TheRegions_LieWhereTheMapsConnect()
        {
            Assert.Less(Farm.X1, Village.X0, "the farm is west of the village");
            Assert.GreaterOrEqual(WorldMapLayout.Forest.Y0, Village.Y1, "the forest is north of the village");
            Assert.GreaterOrEqual(WorldMapLayout.Woods.Y0, WorldMapLayout.Forest.Y1, "and the woods north of the forest");
            Assert.LessOrEqual(WorldMapLayout.Beach.Y1, Village.Y0, "the beach is south of the village");
            Assert.GreaterOrEqual(WorldMapLayout.MineHill.X0, WorldMapLayout.Forest.X1, "the mine is east of the forest");
        }

        [Test]
        public void TheLaneRunsStraight_FromTheBeachThroughTheVillageToTheForest()
        {
            var lane = Village.At(MapLayout.VillageLaneX, 0).x;
            Assert.AreEqual(lane, WorldMapLayout.Forest.At(19, 0).x, 6f, "the forest path meets the lane");
            Assert.AreEqual(lane, WorldMapLayout.Beach.At(17, 0).x, 6f, "and so does the beach path");
        }

        [Test]
        public void TheRoadFromTheFarm_MeetsTheVillageRoadAtTheSameHeight()
        {
            Assert.AreEqual(Farm.At(0, MapLayout.FarmRoadY).y, Village.At(0, 17).y, 8f);
        }

        [Test]
        public void EveryBuildingAndPlace_IsOnThePicture_InsideTheCanvas_WithAPicture_AndNothingOverlaps()
        {
            var spots = WorldMapLayout.Spots(new GameState(), woodsOpen: true);
            foreach (var interior in MapIds.Interiors) Assert.IsTrue(spots.Any(s => s.Map == interior), interior + " is on the map");
            foreach (var place in new[] { MapIds.Forest, MapIds.Woods, MapIds.Beach, MapIds.Mine }) Assert.IsTrue(spots.Any(s => s.Map == place), place + " is on the map");
            Assert.IsFalse(WorldMapLayout.Spots(new GameState(), woodsOpen: false).Any(s => s.Map == MapIds.Woods), "the woods show only once they are open");

            var art = Resources.Load<UiArt>(UiArt.ResourcePath);
            foreach (var s in spots)
            {
                Assert.IsTrue(s.Position.x > 0f && s.Position.x < WorldMapLayout.Width && s.Position.y > 0f && s.Position.y < WorldMapLayout.Height, s.Map);
                Assert.IsNotNull(art.Find(s.Icon), s.Map + ": " + s.Icon + " is in UiArt (rebuild it)");
            }
            Assert.IsNotNull(art.Find("ui_map_here"));
            for (var i = 0; i < spots.Count; i++)
                for (var j = i + 1; j < spots.Count; j++)
                {
                    var d = spots[i].Position - spots[j].Position;
                    Assert.IsTrue(Mathf.Abs(d.x) >= WorldMapLayout.IconSize || Mathf.Abs(d.y) >= WorldMapLayout.IconSize, $"{spots[i].Map} and {spots[j].Map} overlap");
                }
        }

        [Test]
        public void EveryBuildingIsDrawnAtItsRealDoor_AsTheBuiltScenesHaveIt()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Village.unity", OpenSceneMode.Single);
            var warps = Object.FindObjectsByType<Warp>(FindObjectsSortMode.None);
            foreach (var (map, _, _, x, y) in WorldMapLayout.VillageBuildingDoors)
            {
                var warp = warps.FirstOrDefault(w => w.TargetMap == map);
                Assert.IsNotNull(warp, map);
                Assert.AreEqual(new Vector2(x + 0.5f, y + 0.5f), (Vector2)warp.transform.position, map + ": the door is where the map table says");
            }
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Farm.unity", OpenSceneMode.Single);
            var house = Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == MapIds.FarmHouse);
            Assert.AreEqual(new Vector2(WorldMapLayout.FarmHouseDoorX + 0.5f, WorldMapLayout.FarmHouseDoorY + 0.5f), (Vector2)house.transform.position);
        }

        [Test]
        public void TheFarmer_IsShownWhereTheyStand_OutdoorsAndAtTheBuildingIndoors()
        {
            var spots = WorldMapLayout.Spots(new GameState(), woodsOpen: false);
            var inVillage = WorldMapLayout.PlayerPosition(MapIds.Village, new Vector2(10, 17), spots).Value;
            Assert.IsTrue(inVillage.x >= Village.X0 && inVillage.x <= Village.X1 && inVillage.y >= Village.Y0 && inVillage.y <= Village.Y1);
            var inClinic = WorldMapLayout.PlayerPosition(MapIds.Clinic, Vector2.zero, spots).Value;
            Assert.AreEqual(spots.First(s => s.Map == MapIds.Clinic).Position, inClinic);
            Assert.IsNull(WorldMapLayout.PlayerPosition("Nowhere", Vector2.zero, spots));
        }
    }
}
