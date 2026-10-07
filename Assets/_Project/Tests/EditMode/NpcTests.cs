using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-035: friendship rules, schedules (a pure function of the clock), routes between maps, the path finder, and the
    // talking and gifting rules. Scene-side checks are in NpcSceneTests.
    public class NpcTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public GameDateTime Now { get; set; } = GameDateTime.NewGame;
            public string Weather { get; set; } = "sunny";
            public string MapId => "Farm";
            public HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string f) => Flags.Contains(f);
            public int GetVar(string n) => 0;
            public int Hearts(string id) => 0;
            public int ItemCount(string id) => 0;
            public string QuestState(string id) => "new";
            public bool KnowsRecipe(string id) => false;
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        // ---- friendship ------------------------------------------------------------------------------------------

        [Test]
        public void Hearts_AreEveryTwoHundredFiftyPoints_UpToTen()
        {
            Assert.AreEqual(0, FriendshipModel.Hearts(0));
            Assert.AreEqual(0, FriendshipModel.Hearts(249));
            Assert.AreEqual(1, FriendshipModel.Hearts(250));
            Assert.AreEqual(10, FriendshipModel.Hearts(2500));
            Assert.AreEqual(10, FriendshipModel.Hearts(99999));
            Assert.AreEqual(2500, FriendshipModel.Clamp(5000));
            Assert.AreEqual(0, FriendshipModel.Clamp(-30));
        }

        [Test]
        public void GiftPoints_FollowTasteAndBirthdaysCountEightTimes()
        {
            Assert.AreEqual(80, FriendshipModel.GiftPoints(GiftTaste.Loved, false));
            Assert.AreEqual(45, FriendshipModel.GiftPoints(GiftTaste.Liked, false));
            Assert.AreEqual(20, FriendshipModel.GiftPoints(GiftTaste.Neutral, false));
            Assert.AreEqual(-20, FriendshipModel.GiftPoints(GiftTaste.Disliked, false));
            Assert.AreEqual(640, FriendshipModel.GiftPoints(GiftTaste.Loved, true));
            Assert.AreEqual(8 * 45, FriendshipModel.GiftPoints(GiftTaste.Liked, true));
        }

        [Test]
        public void Decay_StartsAfterSomeDaysOfNeglect()
        {
            Assert.AreEqual(0, FriendshipModel.DecayFor(2, 2f));
            Assert.AreEqual(2, FriendshipModel.DecayFor(3, 2f));
            Assert.AreEqual(5, FriendshipModel.DecayFor(10, 5f));
        }

        sealed class Doubler : IFriendshipDecayModifier
        {
            public int Order => 0;
            public float Modify(string npcId, float rate, GameState state) => rate * 2f;
        }

        sealed class Thrower : IFriendshipDecayModifier
        {
            public int Order => -1;
            public float Modify(string npcId, float rate, GameState state) => throw new InvalidOperationException("boom");
        }

        [Test]
        public void NewDay_ResetsDailyCounters_AndDecaysIgnoredNeighboursThroughTheHook()
        {
            var state = GameState.NewGame("a", "b", id => 99, 1);
            state.Npcs["tilda"] = new NpcState { Points = 500, Met = true, TalkedToday = true, GiftsToday = 1, LastContactDay = 0 };
            state.Npcs["bram"] = new NpcState { Points = 500, Met = true, LastContactDay = 8 };

            var hooks = new GameHooks();
            hooks.AddFriendshipDecayModifier(new Doubler());
            NpcInteractions.NewDay(state, new GameDateTime(1, Season.Spring, 10), hooks);

            Assert.IsFalse(state.Npcs["tilda"].TalkedToday);
            Assert.AreEqual(0, state.Npcs["tilda"].GiftsToday);
            Assert.AreEqual(496, state.Npcs["tilda"].Points, "ten days ignored: base 2 doubled to 4");
            Assert.AreEqual(500, state.Npcs["bram"].Points, "two days is not neglect yet");
        }

        [Test]
        public void NewDay_NeverDropsBelowZero_AndIgnoresPeopleNeverMet()
        {
            var state = GameState.NewGame("a", "b", id => 99, 1);
            state.Npcs["a"] = new NpcState { Points = 1, Met = true, LastContactDay = 0 };
            state.Npcs["b"] = new NpcState { Points = 100, Met = false, LastContactDay = -1 };
            NpcInteractions.NewDay(state, new GameDateTime(1, Season.Spring, 20), null);
            Assert.AreEqual(0, state.Npcs["a"].Points);
            Assert.AreEqual(100, state.Npcs["b"].Points);
        }

        [Test]
        public void DecayHook_AFailingModifierIsIsolated()
        {
            var hooks = new GameHooks();
            hooks.AddFriendshipDecayModifier(new Thrower());
            hooks.AddFriendshipDecayModifier(new Doubler());
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("decay modifier Thrower failed"));
            Assert.AreEqual(4f, hooks.ComputeFriendshipDecay("x", 2f, new GameState()));
        }

        // ---- talking and gifts -----------------------------------------------------------------------------------

        static NpcDefinition Tilda() => NpcDefaults.CreateAll().First(n => n.Id == NpcIds.Tilda);

        TestSessionFixture Fixture() => new TestSessionFixture(
            new[]
            {
                TestSessionFixture.Item("crop.strawberry"), TestSessionFixture.Item("crop.parsnip"), TestSessionFixture.Item("resource.stone", ItemCategory.Resource),
            },
            npcs: new[] { Tilda() });

        [Test]
        public void TasteOf_ChecksItemsThenCategories()
        {
            var npc = Tilda();
            Assert.AreEqual(GiftTaste.Loved, NpcInteractions.TasteOf(npc, TestSessionFixture.Item("crop.strawberry")));
            Assert.AreEqual(GiftTaste.Liked, NpcInteractions.TasteOf(npc, TestSessionFixture.Item("crop.potato")));
            Assert.AreEqual(GiftTaste.Disliked, NpcInteractions.TasteOf(npc, TestSessionFixture.Item("resource.stone", ItemCategory.Resource)));
            Assert.AreEqual(GiftTaste.Disliked, NpcInteractions.TasteOf(npc, TestSessionFixture.Item("fish.bass", ItemCategory.Fish)), "she dislikes fish as a category");
            Assert.AreEqual(GiftTaste.Neutral, NpcInteractions.TasteOf(npc, TestSessionFixture.Item("crop.parsnip")));
        }

        [Test]
        public void Gift_TakesOneItem_AndAddsPoints_LimitedPerDayAndWeek()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Backpack.Add("crop.strawberry", 5);
                var slot = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i)?.ItemId == "crop.strawberry");
                var npc = Tilda();

                Assert.AreEqual(GiftResult.Given, NpcInteractions.Gift(s, npc, slot));
                Assert.AreEqual(4, s.Backpack.Count("crop.strawberry"));
                Assert.AreEqual(80, s.State.Npcs["tilda"].Points);
                Assert.IsTrue(s.State.Npcs["tilda"].Met);
                Assert.AreEqual(GiftResult.AlreadyToday, NpcInteractions.Gift(s, npc, slot), "one gift a day");
                Assert.AreEqual(4, s.Backpack.Count("crop.strawberry"));

                s.State.Npcs["tilda"].GiftsToday = 0;      // next day
                Assert.AreEqual(GiftResult.Given, NpcInteractions.Gift(s, npc, slot));
                s.State.Npcs["tilda"].GiftsToday = 0;
                Assert.AreEqual(GiftResult.WeekLimit, NpcInteractions.Gift(s, npc, slot), "two gifts a week");
            }
        }

        [Test]
        public void Gift_RejectsToolsAndEmptySlots()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var hoe = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i)?.ItemId == ItemIds.Hoe);
                var empty = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i) == null);
                Assert.AreEqual(GiftResult.NotGiftable, NpcInteractions.Gift(s, Tilda(), hoe));
                Assert.AreEqual(GiftResult.NothingSelected, NpcInteractions.Gift(s, Tilda(), empty));
                Assert.IsTrue(s.Backpack.Has(ItemIds.Hoe));
            }
        }

        [Test]
        public void Gift_OnABirthdayIsWorthEightTimesAsMuch_AndTheCountersRollOverByWeek()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var npc = Tilda();                                           // birthday Spring 12
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 12, 600));
                s.Backpack.Add("crop.strawberry", 1);
                var slot = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i)?.ItemId == "crop.strawberry");
                NpcInteractions.Gift(s, npc, slot);
                Assert.AreEqual(640, s.State.Npcs["tilda"].Points);

                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 20, 600));  // a later week
                s.Backpack.Add("crop.strawberry", 1);
                slot = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i)?.ItemId == "crop.strawberry");
                s.State.Npcs["tilda"].GiftsToday = 0;
                Assert.AreEqual(GiftResult.Given, NpcInteractions.Gift(s, npc, slot));
                Assert.AreEqual(1, s.State.Npcs["tilda"].GiftsThisWeek, "the weekly count restarted");
            }
        }

        [Test]
        public void Points_AnnounceANewHeart()
        {
            using (var f = Fixture())
            {
                f.Session.Backpack.Add("crop.strawberry", 1);
                NpcInteractions.AddPoints(f.Session, "tilda", 249);
                Assert.IsEmpty(f.Toasts);
                NpcInteractions.AddPoints(f.Session, "tilda", 5);
                Assert.AreEqual(1, f.Toasts.Count);
                Assert.AreEqual(254, f.Session.State.Npcs["tilda"].Points);
            }
        }

        [Test]
        public void Talk_GivesDailyPointsOnce_AndRemembersTheContact()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 600));
                var talked = 0;
                f.Bus.Subscribe<NpcTalked>(_ => talked++);
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                NpcInteractions.Talk(s, Tilda());
                NpcInteractions.Talk(s, Tilda());
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;

                var state = s.State.Npcs["tilda"];
                Assert.AreEqual(FriendshipModel.TalkPoints, state.Points, "only the first talk of the day counts");
                Assert.IsTrue(state.Met);
                Assert.AreEqual(s.Clock.Now.TotalDays, state.LastContactDay);
                Assert.AreEqual(2, talked);
            }
        }

        // ---- schedules -------------------------------------------------------------------------------------------

        static NpcDefinition Npc(params NpcScheduleEntry[] entries) =>
            NpcDefinition.Create("test", Season.Spring, 1, MapIds.Village, 5, 17).WithSchedule(entries);

        static NpcStop Stop(int minute, string map, int x, int y, string facing = "down") =>
            new NpcStop { Minute = minute, Map = map, X = x, Y = y, Facing = facing };

        static NpcScheduleEntry Day(string id, string condition, int priority, params NpcStop[] stops) =>
            new NpcScheduleEntry { Id = id, Condition = condition, Priority = priority, Stops = stops.ToList() };

        [Test]
        public void PlanFor_PicksTheHighestPriorityEntryWhoseConditionHolds()
        {
            var npc = Npc(Day("default", null, 0, Stop(360, MapIds.Village, 5, 17)),
                          Day("sunday", "weekday:sun", 5, Stop(360, MapIds.Village, 6, 17)),
                          Day("rainy_sunday", "weekday:sun && weather:rain", 10, Stop(360, MapIds.Village, 7, 17)));
            var w = new World { Now = new GameDateTime(1, Season.Spring, 2) };
            Assert.AreEqual("default", NpcSchedule.PlanFor(npc, w).Id);
            w.Now = new GameDateTime(1, Season.Spring, 7);
            Assert.AreEqual("sunday", NpcSchedule.PlanFor(npc, w).Id);
            w.Weather = "rain";
            Assert.AreEqual("rainy_sunday", NpcSchedule.PlanFor(npc, w).Id);
        }

        [Test]
        public void PlanFor_ConditionsCanUseFlags_ForHiddenSchedules()
        {
            var npc = Npc(Day("default", null, 0, Stop(360, MapIds.Village, 5, 17)),
                          Day("secret", "flag:secret_night && moon:new", 20, Stop(360, MapIds.Forest, 19, 10)));
            var w = new World();
            Assert.AreEqual("default", NpcSchedule.PlanFor(npc, w).Id);
            w.Flags.Add("secret_night");
            Assert.AreEqual("secret", NpcSchedule.PlanFor(npc, w).Id, "day 1 of a season is a new moon");
            w.Now = new GameDateTime(1, Season.Spring, 15);
            Assert.AreEqual("default", NpcSchedule.PlanFor(npc, w).Id);
        }

        [Test]
        public void PlanFor_NoScheduleOrNoMatch_IsNull_AndTheNpcStaysHome()
        {
            var npc = Npc();
            Assert.IsNull(NpcSchedule.PlanFor(npc, new World()));
            var place = NpcSchedule.Where(npc, null, 600);
            Assert.AreEqual((MapIds.Village, 5, 17, false), (place.Map, place.ToX, place.ToY, place.Walking));
        }

        [Test]
        public void Where_StartsTheDayAtTheFirstStop_AndStandsUntilTheNextDeparture()
        {
            var plan = Day("d", null, 0, Stop(360, MapIds.Village, 10, 17), Stop(600, MapIds.Village, 30, 17, "left"));
            var npc = Npc(plan);
            var before = NpcSchedule.Where(npc, plan, 300);
            Assert.AreEqual((10, 17, false), (before.ToX, before.ToY, before.Walking));
            var morning = NpcSchedule.Where(npc, plan, 500);
            Assert.AreEqual((10, 17, false), (morning.ToX, morning.ToY, morning.Walking));
        }

        [Test]
        public void Where_WalksTheLegAtTheRouteSpeed_ThenArrives()
        {
            var plan = Day("d", null, 0, Stop(360, MapIds.Village, 10, 17), Stop(600, MapIds.Village, 26, 17, "left"));
            var npc = Npc(plan);
            var minutes = 16 / MapRoutes.CellsPerMinute;
            var walking = NpcSchedule.Where(npc, plan, 600 + minutes / 2f);
            Assert.IsTrue(walking.Walking);
            Assert.AreEqual(0.5f, walking.T, 0.01f);
            Assert.AreEqual((10, 17, 26, 17), (walking.FromX, walking.FromY, walking.ToX, walking.ToY));

            var arrived = NpcSchedule.Where(npc, plan, 600 + minutes + 1f);
            Assert.IsFalse(arrived.Walking);
            Assert.AreEqual((26, 17, "left"), (arrived.ToX, arrived.ToY, arrived.Facing));
        }

        [Test]
        public void Where_CrossingMaps_WalksToTheDoor_ThenAppearsOnTheNextMap()
        {
            var plan = Day("d", null, 0, Stop(360, MapIds.GeneralStore, 6, 3), Stop(540, MapIds.Village, 20, 17));
            var npc = Npc(plan);
            var legs = MapRoutes.Legs(MapIds.GeneralStore, 6, 3, MapIds.Village, 20, 17);
            Assert.AreEqual(2, legs.Count);
            Assert.AreEqual(MapIds.GeneralStore, legs[0].Map);
            Assert.AreEqual(MapIds.Village, legs[1].Map);

            var leaving = NpcSchedule.Where(npc, plan, 540 + legs[0].Minutes * 0.5f);
            Assert.AreEqual(MapIds.GeneralStore, leaving.Map);
            Assert.IsTrue(leaving.Walking);
            Assert.AreEqual((5, 0), (leaving.ToX, leaving.ToY), "walking to the shop door");

            var outside = NpcSchedule.Where(npc, plan, 540 + legs[0].Minutes + legs[1].Minutes * 0.25f);
            Assert.AreEqual(MapIds.Village, outside.Map);
            Assert.IsTrue(outside.Walking);
            Assert.AreEqual((8, 23), (outside.FromX, outside.FromY), "stepping out of the door");
        }

        [Test]
        public void Where_IsContinuous_NoTeleportingBetweenOneMinuteAndTheNext()
        {
            // Within a map the position moves by at most the walking speed per minute; at a door the NPC changes map only
            // when standing at the exit/arrival cells.
            foreach (var npc in NpcDefaults.CreateAll())
                foreach (var entry in npc.Schedule)
                {
                    NpcPlacement? last = null;
                    for (var minute = 360f; minute < 1800f; minute += 1f)
                    {
                        var p = NpcSchedule.Where(npc, entry, minute);
                        if (last.HasValue && last.Value.Map == p.Map)
                        {
                            var a = Position(last.Value); var b = Position(p);
                            Assert.LessOrEqual(Vector2.Distance(a, b), MapRoutes.CellsPerMinute * 1.2f + 0.01f,
                                $"{npc.Id}/{entry.Id} jumped at minute {minute}");
                        }
                        last = p;
                    }
                }
        }

        static Vector2 Position(NpcPlacement p) =>
            new Vector2(Mathf.Lerp(p.FromX, p.ToX, p.T), Mathf.Lerp(p.FromY, p.ToY, p.T));

        [Test]
        public void DefaultNpcs_HaveValidSchedules_ForEveryDay()
        {
            foreach (var npc in NpcDefaults.CreateAll())
            {
                Assert.IsTrue(npc.Schedule.Any(e => string.IsNullOrWhiteSpace(e.Condition)), $"{npc.Id} needs a default day");
                foreach (var entry in npc.Schedule)
                {
                    Assert.IsTrue(NpcSchedule.IsValid(entry, out var error), $"{npc.Id}/{entry.Id}: {error}");
                    Assert.AreEqual(npc.HomeMap, entry.Stops[0].Map, $"{npc.Id}/{entry.Id} starts the day at home");
                    var last = entry.Stops[entry.Stops.Count - 1];
                    Assert.AreEqual(npc.HomeMap, last.Map, $"{npc.Id}/{entry.Id} ends the day at home");
                }
            }
        }

        [Test]
        public void DefaultNpcs_FullWeek_EveryDayOfEverySeasonResolves()
        {
            foreach (var npc in NpcDefaults.CreateAll())
                for (var day = 1; day <= 28; day++)
                    foreach (var season in new[] { Season.Spring, Season.Summer, Season.Winter })
                        foreach (var weather in new[] { "sunny", "rain" })
                        {
                            var w = new World { Now = new GameDateTime(1, season, day), Weather = weather };
                            Assert.IsNotNull(NpcSchedule.PlanFor(npc, w), $"{npc.Id} {season} {day} {weather}");
                        }
        }

        [Test]
        public void IsValid_CatchesBadOrdering_AndUnroutableStops()
        {
            Assert.IsFalse(NpcSchedule.IsValid(Day("x", null, 0, Stop(600, "Village", 1, 1), Stop(500, "Village", 2, 2)), out var order));
            StringAssert.Contains("not after", order);
            Assert.IsFalse(NpcSchedule.IsValid(Day("x", null, 0, Stop(100, "Village", 1, 1)), out var range));
            StringAssert.Contains("outside the day", range);
            Assert.IsFalse(NpcSchedule.IsValid(Day("x", null, 0, Stop(400, "Village", 1, 1), Stop(500, "Nowhere", 2, 2)), out var route));
            StringAssert.Contains("no route", route);
            // A stop's minute is when they leave the place before it, so they must have arrived somewhere before leaving it: the walk from (1, 17) to
            // (40, 17) takes 24 minutes from the 410 departure, so leaving there at 420 is leaving before they have arrived.
            Assert.IsFalse(NpcSchedule.IsValid(Day("x", null, 0, Stop(400, "Village", 1, 17), Stop(410, "Village", 40, 17), Stop(420, "Village", 1, 17)), out var rushed));
            StringAssert.Contains("before arriving", rushed);
            Assert.IsTrue(NpcSchedule.IsValid(Day("x", null, 0, Stop(400, "Village", 1, 17), Stop(405, "Village", 40, 17)), out _), "leaving at once is fine: they start the day at the first stop");
        }

        // ---- routes ----------------------------------------------------------------------------------------------

        [Test]
        public void Routes_EveryMapCanReachEveryOtherMap()
        {
            foreach (var from in MapIds.All.Where(m => !FarmBuildings.IsInteriorMap(m)))        // the movable buildings have no fixed route
                foreach (var to in MapIds.All.Where(m => !FarmBuildings.IsInteriorMap(m)))
                    Assert.IsNotNull(MapRoutes.Path(from, to), $"{from} -> {to}");
        }

        [Test]
        public void Routes_TakeTheFewestDoors()
        {
            Assert.AreEqual(0, MapRoutes.Path(MapIds.Farm, MapIds.Farm).Count);
            Assert.AreEqual(1, MapRoutes.Path(MapIds.Farm, MapIds.Village).Count);
            Assert.AreEqual(2, MapRoutes.Path(MapIds.FarmHouse, MapIds.Village).Count);
            Assert.AreEqual(3, MapRoutes.Path(MapIds.FarmHouse, MapIds.Library).Count);
            Assert.IsNull(MapRoutes.Path(MapIds.Farm, "Atlantis"));
        }

        [Test]
        public void Routes_SameCellOnTheSameMap_HasNoLegs()
        {
            Assert.AreEqual(0, MapRoutes.Legs(MapIds.Village, 5, 5, MapIds.Village, 5, 5).Count);
            Assert.IsNull(MapRoutes.Legs(MapIds.Village, 5, 5, "Atlantis", 1, 1));
        }

        // ---- path finding ----------------------------------------------------------------------------------------

        static WalkGrid Grid(string[] rows)
        {
            return new WalkGrid(0, 0, rows[0].Length, rows.Length, (x, y) => rows[rows.Length - 1 - y][x] == '.');
        }

        [Test]
        public void WalkGrid_FindsAShortestPathAroundWalls()
        {
            var grid = Grid(new[]
            {
                ".....",
                ".###.",
                ".....",
            });
            var path = grid.FindPath(0, 0, 4, 0);       // bottom-left to bottom-right: straight along the bottom row
            Assert.AreEqual(5, path.Count);
            var around = grid.FindPath(0, 1, 4, 1);      // the middle row is walled except the ends
            Assert.IsNotNull(around);
            Assert.AreEqual((0, 1), around.First());
            Assert.AreEqual((4, 1), around.Last());
            foreach (var (x, y) in around.Skip(1).Take(around.Count - 2)) Assert.IsTrue(grid.IsWalkable(x, y));
            for (var i = 1; i < around.Count; i++)
                Assert.AreEqual(1, Math.Abs(around[i].x - around[i - 1].x) + Math.Abs(around[i].y - around[i - 1].y), "four-way steps");
        }

        [Test]
        public void WalkGrid_NoWay_ReturnsNull_AndOutsideIsNull()
        {
            var grid = Grid(new[]
            {
                "..#..",
                "..#..",
            });
            Assert.IsNull(grid.FindPath(0, 0, 4, 0));
            Assert.IsNull(grid.FindPath(0, 0, 9, 9));
        }

        [Test]
        public void WalkGrid_ABlockedStartOrGoalIsAllowed()
        {
            var grid = Grid(new[]
            {
                "..#",
            });
            Assert.IsNotNull(grid.FindPath(2, 0, 0, 0), "an NPC standing on furniture can still leave");
            Assert.IsNotNull(grid.FindPath(0, 0, 2, 0), "and can walk up to it");
            Assert.AreEqual(1, grid.FindPath(1, 0, 1, 0).Count);
        }

        // ---- data ------------------------------------------------------------------------------------------------

        [Test]
        public void NpcCatalog_FallsBackToTheDefaults_AndLooksUpById()
        {
            var catalog = NpcCatalog.From(GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]));
            CollectionAssert.AreEquivalent(NpcIds.All, catalog.All.Select(n => n.Id));
            Assert.IsNotNull(catalog.Get("ione"));
            Assert.IsNull(catalog.Get("nobody"));
        }

        [Test]
        public void Birthdays_AreRecognised()
        {
            var tilda = Tilda();
            Assert.IsTrue(tilda.IsBirthday(new GameDateTime(2, Season.Spring, 12)));
            Assert.IsFalse(tilda.IsBirthday(new GameDateTime(1, Season.Summer, 12)));
        }

        [Test]
        public void Npcs_CarryAnAllegianceFieldThatTheBaseGameIgnores()
        {
            foreach (var npc in NpcDefaults.CreateAll()) Assert.IsTrue(string.IsNullOrEmpty(npc.Allegiance));
        }

        [Test]
        public void GameState_NpcStateSurvivesASaveRoundTrip()
        {
            var state = GameState.NewGame("a", "b", id => 99, 1);
            state.Npcs["tilda"] = new NpcState { Points = 321, Met = true, GiftsThisWeek = 1, GiftWeek = 3, LastContactDay = 9 };
            state.Quests["q"] = new QuestProgress { Status = QuestStatus.Done, StartedDay = 2, EndedDay = 4 };
            state.Mailbox.Add("l1");
            state.EventsSeen.Add("e1");
            state.Recipes.Add("r1");
            state.Board.Add(new BoardJob { Id = "j", ItemId = "crop.parsnip", Count = 2, Reward = 100, PostedDay = 1, ExpiresDay = 4 });
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(state);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
            Assert.AreEqual(321, back.Npcs["tilda"].Points);
            Assert.AreEqual(QuestStatus.Done, back.Quests["q"].Status);
            CollectionAssert.AreEqual(new[] { "l1" }, back.Mailbox);
            Assert.IsTrue(back.EventsSeen.Contains("e1"));
            Assert.IsTrue(back.Recipes.Contains("r1"));
            Assert.AreEqual(100, back.Board[0].Reward);
        }

        [Test]
        public void OlderSaves_LoadWithEmptyStoryState()
        {
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>("{\"SaveVersion\":1,\"Gold\":12}");
            Assert.AreEqual(12, back.Gold);
            Assert.IsNotNull(back.Npcs);
            Assert.IsNotNull(back.Quests);
            Assert.IsEmpty(back.Mailbox);
        }
    }
}
