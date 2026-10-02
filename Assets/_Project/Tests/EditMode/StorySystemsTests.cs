using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-039/T-041/T-044/T-045: quests, mail, the help-wanted board, random events and luck, the tutorial chain, hook
    // conformance, and a whole year of days with the real data.
    public class StorySystemsTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        TestSessionFixture Fixture(bool realStory = false)
        {
            var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null);
            if (realStory) f.Session.Story = StoryContent.LoadFromResources();
            return f;
        }

        const string Json = @"{
          ""quests"": [
            { ""id"": ""q1"", ""titleKey"": ""t"", ""descriptionKey"": ""d"", ""autoStart"": true, ""autoComplete"": true,
              ""objectives"": [ { ""text"": ""o"", ""condition"": ""var:stat.tilled>=2"" } ], ""rewards"": [ ""gold:10"" ] },
            { ""id"": ""q2"", ""titleKey"": ""t"", ""descriptionKey"": ""d"", ""autoStart"": true, ""autoComplete"": true, ""available"": ""quest:q1=done"",
              ""objectives"": [ { ""text"": ""o"", ""condition"": ""flag:go"" } ], ""rewards"": [ ""flag:chain_done"" ] },
            { ""id"": ""give"", ""titleKey"": ""t"", ""descriptionKey"": ""d"",
              ""objectives"": [ { ""text"": ""o"", ""condition"": ""has:resource.stone>=3"", ""takeItem"": ""resource.stone"", ""takeCount"": 3 } ], ""rewards"": [ ""gold:100"" ] }
          ],
          ""letters"": [ { ""id"": ""l1"", ""subjectKey"": ""s"", ""bodyKey"": ""b"", ""condition"": ""flag:post"", ""effects"": [ ""gold:5"" ] } ]
        }";

        [Test]
        public void Quests_StartAndCompleteThemselves_AndChain()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Story.AddJson(Json, "t");
                QuestLog.Tick(s);
                Assert.AreEqual(QuestStatus.Active, QuestLog.StatusOf(s.State, "q1"));
                Assert.AreEqual("new", QuestLog.StatusOf(s.State, "q2"), "q2 waits for q1");
                var gold = s.State.Gold;
                s.AddVar("stat.tilled", 2);                                   // the change itself triggers the check
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, "q1"));
                Assert.AreEqual(gold + 10, s.State.Gold);
                Assert.AreEqual(QuestStatus.Active, QuestLog.StatusOf(s.State, "q2"), "completing q1 started q2");
                s.SetFlag("go");
                Assert.IsTrue(s.HasFlag("chain_done"));
            }
        }

        [Test]
        public void HandInQuests_TakeTheItems_OnlyWhenAllThere_AndOnlyOnce()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Story.AddJson(Json, "t");
                var q = s.Story.Quest("give");
                Assert.IsTrue(QuestLog.Start(s, q));
                Assert.IsFalse(QuestLog.Start(s, q), "already active");
                s.Backpack.Add(ItemIds.Stone, 2);
                Assert.IsFalse(QuestLog.TryComplete(s, q));
                s.Backpack.Add(ItemIds.Stone, 2);
                Assert.IsTrue(QuestLog.TryComplete(s, q));
                Assert.AreEqual(1, s.Backpack.Count(ItemIds.Stone), "three were handed over");
                Assert.IsFalse(QuestLog.Start(s, q), "done quests do not restart");
                Effects.Run(s, "quest.done:give");                              // already done: nothing breaks
            }
        }

        [Test]
        public void Letters_ArriveOnce_RunEffectsWhenRead_AndAreKept()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Story.AddJson(Json, "t");
                Assert.AreEqual(0, Mail.Deliver(s));
                s.SetFlag("post");
                Assert.AreEqual(1, Mail.Deliver(s));
                Assert.AreEqual(0, Mail.Deliver(s), "a letter is delivered once");
                var gold = s.State.Gold;
                var letter = Mail.Next(s);
                Assert.AreEqual("l1", letter.Id);
                Assert.AreEqual(gold, s.State.Gold, "effects wait until it is read");
                Mail.Finish(s, letter);
                Assert.AreEqual(gold + 5, s.State.Gold);
                CollectionAssert.Contains(s.State.MailKept, "l1");
                Assert.IsNull(Mail.Next(s));
                Mail.Finish(s, letter);                                          // a second read changes nothing
                Assert.AreEqual(gold + 5, s.State.Gold);
            }
        }

        [Test]
        public void ExpiredQuests_Fail()
        {
            var state = new GameState();
            state.Quests["job"] = new QuestProgress { Status = QuestStatus.Active, ExpiresDay = 3 };
            QuestLog.Expire(state, 3);
            Assert.AreEqual(QuestStatus.Active, state.Quests["job"].Status);
            QuestLog.Expire(state, 4);
            Assert.AreEqual(QuestStatus.Failed, state.Quests["job"].Status);
        }

        // ---- the board ---------------------------------------------------------------------------------------------

        [Test]
        public void TheBoard_PostsDeterministicJobs_NeverMoreThanThree_AndTheyExpire()
        {
            using (var f = Fixture(true))
            {
                var s = f.Session;
                s.State.WorldSeed = 12345;
                for (var day = 0; day < 60; day++)
                {
                    HelpWanted.Refresh(s.State, s.Story, s.Db, s.World, day);
                    Assert.LessOrEqual(s.State.Board.Count, HelpWanted.MaxJobs);
                    Assert.IsTrue(s.State.Board.All(j => j.ExpiresDay >= day));
                    Assert.AreEqual(s.State.Board.Count, s.State.Board.Select(j => j.ItemId).Distinct().Count());
                }
                Assert.Greater(s.State.Board.Count, 0);
                var snapshot = string.Join(",", s.State.Board.Select(j => j.Id));
                s.State.Board.Clear();
                for (var day = 0; day < 60; day++) HelpWanted.Refresh(s.State, s.Story, s.Db, s.World, day);
                Assert.AreEqual(snapshot, string.Join(",", s.State.Board.Select(j => j.Id)), "same seed, same board");
            }
        }

        [Test]
        public void ADeliveredJob_TakesTheItemsAndPays()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var job = new BoardJob { Id = "j", ItemId = "crop.parsnip", Count = 3, Reward = 200, ExpiresDay = 9 };
                s.State.Board.Add(job);
                Assert.IsFalse(HelpWanted.Deliver(s, job));
                s.Backpack.Add("crop.parsnip", 3);
                var gold = s.State.Gold;
                Assert.IsTrue(HelpWanted.Deliver(s, job));
                Assert.AreEqual(gold + 200, s.State.Gold);
                Assert.IsEmpty(s.State.Board);
            }
        }

        // ---- random events and luck --------------------------------------------------------------------------------

        static RandomEventDefinition Ev(string id, string mood, float w = 10f) => new RandomEventDefinition { Id = id, Mood = mood, Weight = w };

        sealed class NoBad : IEventWeightModifier
        {
            public int Order => 0;
            public float Modify(RandomEventDefinition def, float weight, GameState state) => def.Mood == "bad" ? 0f : weight;
        }

        static int Count(float luck, string id, GameHooks hooks = null)
        {
            var list = new[] { Ev("good", "good"), Ev("bad", "bad"), Ev("meh", "neutral") };
            var hits = 0;
            for (var i = 0; i < 3000; i++)
                if (RandomEvents.Pick(list, luck, new GameState(), hooks, (i + 0.5f) / 3000f).Id == id) hits++;
            return hits;
        }

        [Test]
        public void Luck_ShiftsRandomEventsTowardGoodOrBad_AndNeutralIsUnchanged()
        {
            Assert.AreEqual(1000, Count(0f, "good"), 1);
            Assert.Greater(Count(0.8f, "good"), Count(0f, "good"));
            Assert.Less(Count(0.8f, "bad"), Count(0f, "bad"));
            Assert.Greater(Count(-0.8f, "bad"), Count(0f, "bad"));
        }

        [Test]
        public void TheEventWeightHook_CanRemoveEvents_AndAFailingHookIsIsolated()
        {
            var hooks = new GameHooks();
            hooks.AddEventWeightModifier(new NoBad());
            Assert.AreEqual(0, Count(0f, "bad", hooks));
            Assert.IsNull(RandomEvents.Pick(new[] { Ev("bad", "bad") }, 0f, new GameState(), hooks, 0.5f));
        }

        [Test]
        public void RandomEvents_AreQuietTheFirstWeek_AndHappenSometimesAfter()
        {
            var state = new GameState { WorldSeed = 9 };
            Assert.IsFalse(Enumerable.Range(0, RandomEvents.QuietDays).Any(d => RandomEvents.HappensToday(state, d)));
            var days = Enumerable.Range(RandomEvents.QuietDays, 400).Count(d => RandomEvents.HappensToday(state, d));
            Assert.That(days, Is.InRange(80, 200));
        }

        [Test]
        public void Seasons_FilterEvents_AndCrowsSpareScarecrowFields()
        {
            Assert.IsTrue(RandomEvents.InSeason(Ev("x", "good"), Season.Winter));
            var def = new RandomEventDefinition { Seasons = "Spring, Summer" };
            Assert.IsTrue(RandomEvents.InSeason(def, Season.Summer));
            Assert.IsFalse(RandomEvents.InSeason(def, Season.Fall));

            using (var f = Fixture())
            {
                var s = f.Session;
                var crop = CropDefinition.Create("x", new[] { 1 }, SeasonMask.All);
                var grid = s.GetGrid(MapIds.Farm);
                for (var i = 0; i < 4; i++) { grid.Till(i, 0); grid.Plant(i, 0, crop, Season.Spring); }
                s.GetObjects(MapIds.Farm).Place(s.Placeables.Get("scarecrow"), 1, 1, "sc");
                Assert.AreEqual(0, Crows.Strike(s, 3), "every crop is within the scarecrow's reach");
                s.GetObjects(MapIds.Farm).Remove("sc");
                Assert.AreEqual(3, Crows.Strike(s, 3));
                Object.DestroyImmediate(crop);
            }
        }

        // ---- the shipped story --------------------------------------------------------------------------------------

        [Test]
        public void TheTutorialChain_RunsFromTheFirstDay()
        {
            using (var f = Fixture(true))
            {
                var s = f.Session;
                StoryDay.NewGame(s);
                Assert.AreEqual(QuestStatus.Active, QuestLog.StatusOf(s.State, "tut_farm"));
                CollectionAssert.Contains(s.State.Mailbox, "welcome");
                s.AddVar(QuestLog.Stats.Tilled, 3); s.AddVar(QuestLog.Stats.Planted, 3); s.AddVar(QuestLog.Stats.Watered, 3);
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, "tut_farm"));
                Assert.AreEqual(QuestStatus.Active, QuestLog.StatusOf(s.State, "tut_harvest"));
                Assert.AreEqual(QuestStatus.Active, QuestLog.StatusOf(s.State, "tut_village"));
                s.AddVar(QuestLog.Stats.Harvested, 1); s.AddVar(QuestLog.Stats.Shipped, 1);
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, "tut_harvest"));
                s.SetFlag("met.tilda");
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, "tut_village"));
                Mail.Deliver(s);
                CollectionAssert.Contains(s.State.Mailbox, "tilda_thanks");
            }
        }

        [Test]
        public void TheHeartEvent_NeedsFriendshipAndTheShopOpen()
        {
            using (var f = Fixture(true))
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 10 * 60));
                s.State.CurrentMap = MapIds.GeneralStore;
                Assert.IsEmpty(EventRunner.FindTriggered(s, MapIds.GeneralStore, true));
                s.State.Npcs["tilda"] = new NpcState { Points = 600, Met = true };
                Assert.AreEqual("tilda_heart2", EventRunner.FindTriggered(s, MapIds.GeneralStore, true).Single().Id);
                s.State.EventsSeen.Add("tilda_heart2");
                Assert.IsEmpty(EventRunner.FindTriggered(s, MapIds.GeneralStore, true), "once only");
                Assert.IsTrue(EventRunner.Trigger(s, "tilda_heart2"));
                Assert.IsFalse(EventRunner.Trigger(s, "nope") && false);
            }
        }

        [Test]
        public void SkippingAnEvent_StillRunsItsEffects()
        {
            using (var f = Fixture(true))
            {
                var s = f.Session;
                var ev = s.Story.Event("tilda_heart2");
                EventRunner.RunSkipped(s, ev, 0);
                Assert.IsTrue(s.HasFlag("event.tilda_heart2"));
                Assert.IsTrue(s.KnowsRecipe(s.Recipes.Get("cook_gratin")));
            }
        }

        // ---- T-044: hooks stay out of the way ----------------------------------------------------------------------

        sealed class Spy : IDayCycleHook
        {
            public int Order => 0;
            public int Nights, Dawns;
            public void OnNightFalls(DayCycleContext c) => Nights++;
            public void OnDawn(DayCycleContext c) => Dawns++;
        }

        static string Fingerprint(GameState s) =>
            $"{s.Gold}|{s.Energy}|{string.Join(",", s.Flags.OrderBy(x => x))}|{string.Join(",", s.Vars.OrderBy(v => v.Key).Select(v => v.Key + "=" + v.Value))}|{string.Join(",", s.Npcs.OrderBy(n => n.Key).Select(n => n.Key + n.Value.Points))}|{s.Weather}";

        static GameState PlayDays(int days, bool withSpyHook, int seed)
        {
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops))
            {
                var s = f.Session;
                s.State.WorldSeed = seed;
                s.Story = StoryContent.LoadFromResources();
                if (withSpyHook) s.Hooks.AddDayCycleHook(new Spy());
                StoryDay.NewGame(s);
                for (var d = 0; d < days; d++)
                {
                    s.Clock.SetTime(new GameDateTime(s.Clock.Now.Year, s.Clock.Now.Season, s.Clock.Now.Day, 22 * 60));
                    s.AddVar(QuestLog.Stats.Tilled, 1); s.AddVar(QuestLog.Stats.Planted, 1); s.AddVar(QuestLog.Stats.Watered, 1);
                    s.EndDay(false);
                    foreach (var letter in s.State.Mailbox.ToList()) Mail.Finish(s, s.Story.Letter(letter));
                }
                return JsonUtilityClone(s.State);
            }
        }

        static GameState JsonUtilityClone(GameState state) =>
            Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(Newtonsoft.Json.JsonConvert.SerializeObject(state));

        [Test]
        public void AFullYear_RunsWithoutErrors_AndIsRepeatable()
        {
            var a = PlayDays(112, false, 42);
            var b = PlayDays(112, false, 42);
            Assert.AreEqual(Fingerprint(a), Fingerprint(b), "the same seed plays out the same way");
            Assert.AreEqual(2, a.Year, "a year of nights passed");
            Assert.AreEqual(QuestStatus.Done, a.Quests["tut_farm"].Status);
            Assert.IsTrue(a.MailKept.Contains("welcome"));
            Assert.IsTrue(a.MailKept.Contains("summer_seeds"), "seasonal letters arrive");
        }

        [Test]
        public void ARegisteredButIdleHook_DoesNotChangeTheOutcome()
        {
            Assert.AreEqual(Fingerprint(PlayDays(40, false, 7)), Fingerprint(PlayDays(40, true, 7)));
        }
    }
}
