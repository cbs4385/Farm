using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-07: "npcs should live in homes; currently the only home is the player's". Every villager has a cottage on a street in the east of
    // the village and a room in it; their days start and end there.
    public class NpcHomesTests
    {
        static NpcDefinition[] Everyone() => NpcDefaults.CreateAll();

        [Test]
        public void EveryVillager_HasOneOwnCottage_AndARoomInIt()
        {
            var all = Everyone();
            Assert.AreEqual(12, all.Length);
            foreach (var npc in all)
            {
                var home = NpcHomes.Of(npc.Id);
                Assert.IsNotNull(home, npc.Id + " has a home");
                Assert.AreEqual(home.Map, npc.HomeMap, npc.Id + ": the villager's home is their cottage, not the shop they work in");
                Assert.AreEqual(NpcHomes.StandX, npc.HomeX); Assert.AreEqual(NpcHomes.StandY, npc.HomeY);
                Assert.IsTrue(MapIds.IsHome(home.Map) && MapIds.IsInterior(home.Map) && MapIds.All.Contains(home.Map));
            }
            Assert.AreEqual(12, NpcHomes.All.Select(h => h.Map).Distinct().Count(), "one map each");
            Assert.AreEqual(12, NpcHomes.All.Select(h => h.Npc).Distinct().Count());
            Assert.AreEqual(12, NpcHomes.All.Select(h => (h.DoorX, h.DoorY)).Distinct().Count(), "and one door each");
        }

        [Test]
        public void TheCottages_DoNotOverlap_AndStayInsideTheVillage_ClearOfTheRoadTheLaneAndTheOldShops()
        {
            var homes = NpcHomes.All;
            var shops = Farm.Editor.MapBuilder.ShopFootprints();
            for (var i = 0; i < homes.Count; i++)
            {
                var a = homes[i];
                Assert.Greater(a.X0, 0, a.Map + " is inside the west wall");
                Assert.Less(a.X1, MapLayout.VillageW - 1, a.Map + " is inside the east wall");
                Assert.Greater(a.Y0 - 2, 0, a.Map + " leaves room for its alley below");
                Assert.Less(a.Y1 + 1, MapLayout.VillageH - 1);
                Assert.IsTrue(a.Y0 > 19 || a.Y1 < 15, a.Map + " is off the road (y 16 to 18)");
                Assert.IsTrue(a.X1 < 24 || a.X0 > 26, a.Map + " is off the lane to the forest and the beach (x 24 to 26)");
                foreach (var shop in shops)
                    Assert.IsTrue(a.X1 < shop.xMin - 1 || a.X0 > shop.xMax || a.Y1 < shop.yMin - 1 || a.Y0 > shop.yMax, a.Map + " is clear of a shop at " + shop);
                for (var j = i + 1; j < homes.Count; j++)
                {
                    var b = homes[j];
                    var apart = a.X1 < b.X0 - 1 || b.X1 < a.X0 - 1 || a.Y1 < b.Y0 - 1 || b.Y1 < a.Y0 - 1;
                    Assert.IsTrue(apart, $"{a.Map} and {b.Map} overlap or touch");
                }
                Assert.IsFalse(NpcHomes.IsCobbled(a.DoorX, a.DoorY), "a door is not on the cobbles");
            }
        }

        // Playtest 2026-10-08: "all the npc homes are on one path in the village". Three streets of four, spread round the village at about the same distance
        // from its middle.
        [Test]
        public void TheHomes_AreOnThreeStreetsOfFour_SpreadRoundTheVillage_AtAboutTheSameDistanceFromItsMiddle()
        {
            Assert.AreEqual(3, NpcHomes.Streets.Length);
            for (var s = 0; s < NpcHomes.Streets.Length; s++)
                Assert.AreEqual(NpcHomes.HomesPerStreet, NpcHomes.All.Count(h => h.Street == s), "homes on street " + s);
            Assert.LessOrEqual(NpcHomes.HomesPerStreet, 4, "at most four to a street");

            var centres = NpcHomes.Streets.Select(NpcHomes.StreetCentre).ToArray();
            var distances = centres.Select(c => Vector2.Distance(c, NpcHomes.VillageCentre)).ToArray();
            foreach (var d in distances) Assert.That(d, Is.InRange(24f, 33f), "a street's distance from the middle: " + string.Join(", ", distances));
            Assert.LessOrEqual(distances.Max() - distances.Min(), 5f, "the streets are about equally far out");
            foreach (var home in NpcHomes.All) Assert.That(NpcHomes.DistanceFromCentre(home), Is.InRange(18f, 38f), home.Map);

            // Spread, not bunched: the streets are far from each other (a street is 17 cells wide).
            for (var i = 0; i < centres.Length; i++)
                for (var j = i + 1; j < centres.Length; j++)
                    Assert.Greater(Vector2.Distance(centres[i], centres[j]), 30f, $"streets {i} and {j} are well apart");
            // And they are on different sides of the middle: not all in one half of the village.
            Assert.IsTrue(centres.Any(c => c.x < NpcHomes.VillageCentre.x) && centres.Any(c => c.x > NpcHomes.VillageCentre.x), "west and east");
            Assert.IsTrue(centres.Any(c => c.y < NpcHomes.VillageCentre.y) && centres.Any(c => c.y > NpcHomes.VillageCentre.y), "north and south");
        }

        [Test]
        public void TheStreets_KeepClearOfTheRoad_TheLane_AndTheShops()
        {
            var shops = Farm.Editor.MapBuilder.ShopFootprints();
            foreach (var street in NpcHomes.Streets)
                for (var x = street.LaneX - 7; x <= street.LaneX + 9; x++)
                    for (var y = street.RowY - 2; y <= street.RowY + 9; y++)
                    {
                        var cell = new Vector2Int(x, y);
                        Assert.IsTrue(x > 0 && x < MapLayout.VillageW - 1 && y > 0 && y < MapLayout.VillageH - 1, $"{cell} is inside the village walls");
                        Assert.IsFalse(y >= 16 && y <= 18, $"{cell} is on the road");
                        Assert.IsFalse(x >= 24 && x <= 26, $"{cell} is on the lane");
                        foreach (var shop in shops) Assert.IsFalse(shop.Contains(cell), $"{cell} is in a shop");
                    }
        }

        [Test]
        public void EveryDay_OfEveryVillager_StartsAtHome_AndEndsAtHome_AndIsValid()
        {
            foreach (var npc in Everyone())
            {
                var home = NpcHomes.Of(npc.Id);
                foreach (var plan in npc.Schedule)
                {
                    Assert.IsTrue(NpcSchedule.IsValid(plan, out var error), $"{npc.Id} {plan.Id}: {error}");
                    var first = plan.Stops.First(); var last = plan.Stops.Last();
                    Assert.AreEqual(home.Map, first.Map, $"{npc.Id} {plan.Id} starts at home");
                    Assert.AreEqual(home.Map, last.Map, $"{npc.Id} {plan.Id} ends at home");
                    Assert.IsTrue(plan.Stops.Count(s => s.Map == home.Map) >= 2 || plan.Id.StartsWith("rainy"), $"{npc.Id} {plan.Id} goes out and comes back");
                }
            }
        }

        [Test]
        public void EveryHome_CanBeReached_FromTheFarm_AndFromTheShops()
        {
            foreach (var home in NpcHomes.All)
            {
                Assert.IsNotNull(MapRoutes.Path(MapIds.Farm, home.Map), home.Map + " from the farm");
                Assert.IsNotNull(MapRoutes.Path(home.Map, MapIds.GeneralStore), home.Map + " to the general store");
            }
        }

        [Test]
        public void ShopkeepersLeaveEarlyEnoughToBeAtWork_WhenTheyUsedToBeThereAlready()
        {
            var tilda = Everyone().First(n => n.Id == NpcIds.Tilda);
            var workday = tilda.Schedule.First(e => e.Id == "workday");
            var before = NpcSchedule.Where(tilda, workday, 8 * 60 + 30);
            Assert.IsTrue(before.Walking, "at 8:30 she is on her way from home");
            Assert.AreEqual(MapIds.Village, before.Map == MapIds.Village || before.Map == MapIds.HomeTilda ? MapIds.Village : before.Map);
            var at = NpcSchedule.Where(tilda, workday, 8 * 60 + 45);
            Assert.AreEqual(MapIds.GeneralStore, at.Map, "and by 8:45, before the shop opens at 9:00, she is in her shop");
            Assert.IsFalse(at.Walking);
        }

        [Test]
        public void ARainyDayOff_IsSpentAtHome()
        {
            var bram = Everyone().First(n => n.Id == NpcIds.Bram);
            var rainy = bram.Schedule.First(e => e.Id == "rainy_day_off");
            foreach (var minute in new[] { 7 * 60, 12 * 60, 20 * 60 })
                Assert.AreEqual(MapIds.HomeBram, NpcSchedule.Where(bram, rainy, minute).Map, "at " + minute);
        }

        [Test]
        public void TheVillageScene_HasEachCottageDoor_Spawn_AndNoTreeInTheStreet()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Village.unity", OpenSceneMode.Single);
            var warps = Object.FindObjectsByType<Warp>(FindObjectsSortMode.None);
            var spawns = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var home in NpcHomes.All)
            {
                var warp = warps.FirstOrDefault(w => w.TargetMap == home.Map);
                Assert.IsNotNull(warp, home.Map + " has a door");
                Assert.AreEqual(new Vector2(home.DoorX + 0.5f, home.DoorY + 0.5f), (Vector2)warp.transform.position, home.Map);
                Assert.AreEqual(NpcHomes.OpenConditionFor(home.Npc), warp.Condition, "locked at night, except to a friend");
                Assert.AreEqual(NpcHomes.LockedKey, warp.BlockedMessageKey);
                var spawn = spawns.FirstOrDefault(s => s.Id == "from" + home.Map);
                Assert.IsNotNull(spawn, home.Map + " has a place to stand outside the door");
                Assert.AreEqual(new Vector2(home.DoorX + 0.5f, home.OutsideY + 0.5f), (Vector2)spawn.transform.position, home.Map);
            }
            foreach (var go in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Tree_")))
            {
                var cell = new Vector2Int(Mathf.FloorToInt(go.position.x), Mathf.FloorToInt(go.position.y));
                Assert.IsFalse(NpcHomes.InStreet(cell.x, cell.y), $"{go.name} stands in the villagers' street");
            }
        }

        [Test]
        public void EveryHomeRoom_ExistsAsAScene_WithADoorBackToTheVillage_AndTheOwnersSpotIsFree()
        {
            foreach (var home in NpcHomes.All)
            {
                EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{home.Map}.unity", OpenSceneMode.Single);
                var warp = Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).Single();
                Assert.AreEqual(MapIds.Village, warp.TargetMap, home.Map);
                Assert.AreEqual("from" + home.Map, warp.TargetSpawn);
                var stand = new Vector2(NpcHomes.StandX + 0.5f, NpcHomes.StandY + 0.5f);
                foreach (var col in Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
                    Assert.IsFalse(!col.isTrigger && col.OverlapPoint(stand), $"{home.Map}: something stands where {home.Npc} stands");
            }
        }
    }
}
