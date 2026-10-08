using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Villagers sleep in their beds (they used to stand in their rooms all night), and a friend is let in at night.
    public class NpcSleepTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public int Friendship;
            public GameDateTime Time = new GameDateTime(1, Season.Spring, 3, 23 * 60);
            public bool HasFlag(string flag) => false;
            public int GetVar(string name) => 0;
            public GameDateTime Now => Time;
            public string Weather => "sunny";
            public string MapId => MapIds.Village;
            public int Hearts(string npcId) => Friendship;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        [Test]
        public void EveryDay_ThatStartsAtHome_StartsInBed_AndRises_AndEndsInBed()
        {
            foreach (var npc in NpcDefaults.CreateAll())
                foreach (var plan in npc.Schedule)
                {
                    var first = plan.Stops.First();
                    Assert.AreEqual(NpcSchedule.SleepFacing, first.Facing, $"{npc.Id} {plan.Id} starts asleep");
                    Assert.AreEqual((NpcHomes.BedX, NpcHomes.BedY), (first.X, first.Y), $"{npc.Id} {plan.Id} in the bed");
                    var second = plan.Stops[1];
                    Assert.LessOrEqual(second.Minute, NpcHomes.RiseMinute, $"{npc.Id} {plan.Id} rises");
                    Assert.AreEqual((NpcHomes.StandX, NpcHomes.StandY), (second.X, second.Y));
                    var last = plan.Stops.Last();
                    // The latest closers (a saloon till two) are still out when the day's schedule ends; everyone else is in bed.
                    if (last.Map == NpcHomes.MapOf(npc.Id))
                        Assert.AreEqual(NpcSchedule.SleepFacing, last.Facing, $"{npc.Id} {plan.Id} ends asleep");
                    Assert.IsTrue(NpcSchedule.IsValid(plan, out var error), $"{npc.Id} {plan.Id}: {error}");
                }
        }

        [Test]
        public void ARainyDayOff_IsSleptThrough_ThenSpentUp_ThenSleptAgain()
        {
            var bram = NpcDefaults.CreateAll().First(n => n.Id == NpcIds.Bram);
            var rainy = bram.Schedule.First(e => e.Id == "rainy_day_off");
            Assert.AreEqual(NpcSchedule.SleepFacing, NpcSchedule.Where(bram, rainy, 6 * 60 + 5).Facing, "asleep at six past six");
            Assert.AreNotEqual(NpcSchedule.SleepFacing, NpcSchedule.Where(bram, rainy, 12 * 60).Facing, "up at noon");
            Assert.AreEqual(NpcSchedule.SleepFacing, NpcSchedule.Where(bram, rainy, 23 * 60).Facing, "asleep at eleven");
        }

        [Test]
        public void EveryHomeHasADoubleBed_TwoCellsSquare_AndTheSleepersCellCanBeReached()
        {
            foreach (var home in NpcHomes.All)
            {
                EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{home.Map}.unity", OpenSceneMode.Single);
                var bed = GameObject.Find("Bed");
                Assert.IsNotNull(bed, home.Map + " has a bed");
                Assert.AreEqual("obj_bed_double", bed.GetComponent<SpriteRenderer>().sprite.name, home.Map);
                var box = bed.GetComponent<Collider2D>().bounds;
                Assert.AreEqual(new Vector2(NpcHomes.BedSize, NpcHomes.BedSize), (Vector2)box.size, "two cells square, solid");
                Assert.IsTrue(box.Contains(new Vector3(NpcHomes.BedX + 0.5f, NpcHomes.BedY + 0.5f, box.center.z)), "the sleeper's cell is on the bed");
                Assert.IsNotNull(MapRoutes.Legs(home.Map, NpcHomes.StandX, NpcHomes.StandY, home.Map, NpcHomes.BedX, NpcHomes.BedY), "and they can walk to it");
            }
        }

        [Test]
        public void TheSleepingIcon_IsInTheUiArt_AndNamedByFx()
        {
            var art = Resources.Load<Farm.Data.UiArt>(Farm.Data.UiArt.ResourcePath);
            Assert.IsNotNull(art);
            Assert.IsNotNull(art.Find(Fx.SleepZzz), "the Z z z picture is collected into the UI art");
        }

        [Test]
        public void TheDoor_IsOpenByDay_LockedAtNight_ButOpenToAFriend()
        {
            var tilda = NpcHomes.OpenConditionFor(NpcIds.Tilda);
            var w = new World { Friendship = 0 };
            w.Time = new GameDateTime(1, Season.Spring, 3, 12 * 60);
            Assert.IsTrue(Conditions.Evaluate(tilda, w), "open at noon to anyone");
            w.Time = new GameDateTime(1, Season.Spring, 3, 23 * 60);
            Assert.IsFalse(Conditions.Evaluate(tilda, w), "locked at night to a stranger");
            w.Friendship = NpcHomes.FriendHearts - 1;
            Assert.IsFalse(Conditions.Evaluate(tilda, w), "and to someone who is not yet a friend");
            w.Friendship = NpcHomes.FriendHearts;
            Assert.IsTrue(Conditions.Evaluate(tilda, w), "open to a friend at night");
            w.Time = new GameDateTime(1, Season.Spring, 4, 25 * 60);       // one in the morning
            Assert.IsTrue(Conditions.Evaluate(tilda, w), "and in the small hours");
        }
    }
}
