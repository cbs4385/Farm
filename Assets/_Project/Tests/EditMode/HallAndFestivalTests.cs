using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-055/T-056: the Community Hall's six bundles and the four festivals.
    public class HallAndFestivalTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        TestSessionFixture Fixture()
        {
            var f = new TestSessionFixture(RealDb().AllItems);
            f.Session.Story = StoryContent.LoadFromResources();
            return f;
        }

        [Test]
        public void SixRooms_EachWithItemsOrFriendshipObjectives()
        {
            using (var f = Fixture())
            {
                foreach (var room in HallRooms.Rooms)
                {
                    var q = f.Session.Story.Quest(room);
                    Assert.IsNotNull(q, room);
                    Assert.IsNotEmpty(q.Objectives, room);
                    Assert.IsFalse(q.AutoStart, "rooms start when the hall is first visited");
                }
                Assert.IsNotNull(f.Session.Story.Quest(HallRooms.CompleteQuest));
            }
        }

        static QuestObjective Need(GameSession s, string room, string itemId) => s.Story.Quest(room).Objectives.First(o => o.GiveItem == itemId);

        // Playtest 2026-10-09: "could not donate my parsnips". A room's items are given a few at a time, whenever the player has some; the room is restored once
        // everything has been given.
        [Test]
        public void Donating_IsOneThingAtATime_KeepsTheProgress_AndTheLastGiftRestoresTheRoomAndPays()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Backpack.Resize(80);
                HallRooms.StartAll(s);
                Assert.AreEqual(0, HallRooms.RestoredCount(s.State));
                var gold = s.State.Gold;
                Assert.AreEqual(0, HallRooms.Donate(s, "hall_pantry", Need(s, "hall_pantry", "crop.parsnip")), "nothing to give yet");

                s.Backpack.Add("crop.parsnip", 10);
                Assert.AreEqual(3, HallRooms.Donate(s, "hall_pantry", Need(s, "hall_pantry", "crop.parsnip")), "only what the room still needs");
                Assert.AreEqual(7, s.Backpack.Count("crop.parsnip"), "the rest stays with the player");
                Assert.AreEqual(0, HallRooms.Donate(s, "hall_pantry", Need(s, "hall_pantry", "crop.parsnip")), "that part is done");
                Assert.AreEqual(gold, s.State.Gold, "the room is not restored yet");
                Assert.IsFalse(HallRooms.IsRestored(s.State, "hall_pantry"));

                s.Backpack.Add("crop.potato", 2);
                Assert.AreEqual(2, HallRooms.Donate(s, "hall_pantry", Need(s, "hall_pantry", "crop.potato")), "two of three potatoes");
                s.Backpack.Add("crop.potato", 5);
                Assert.AreEqual(1, HallRooms.Donate(s, "hall_pantry", Need(s, "hall_pantry", "crop.potato")), "and the last one another day");
                var pantry = s.Story.Quest("hall_pantry");
                Assert.AreEqual(3, QuestLog.Given(s, pantry, Need(s, "hall_pantry", "crop.potato")));
                Assert.IsTrue(s.State.Vars.ContainsKey(QuestLog.GiveKey("hall_pantry", "crop.potato")), "the progress is saved with the game");

                s.Backpack.Add("crop.kale", 3); s.Backpack.Add("crop.cauliflower", 3);
                Assert.AreEqual(6, HallRooms.DonateAll(s), "'donate everything' gives what is left to give: three kale and three cauliflower");
                Assert.AreEqual(gold + 500, s.State.Gold, "paid when the last item was given");
                Assert.IsTrue(s.HasFlag("hall.pantry"));
                Assert.IsTrue(HallRooms.IsRestored(s.State, "hall_pantry"));
                Assert.AreEqual(0, s.Backpack.Count("crop.kale"));
                Assert.AreEqual(7, s.Backpack.Count("crop.parsnip") + s.Backpack.Count("crop.potato") - 4, "what was over is still in the backpack");
                Assert.IsFalse(s.HasFlag(HallRooms.RestoredFlag));
            }
        }

        [Test]
        public void TheWholeHall_IsRestoredByGivingEachRoomsThings_ThenTheEndingComes()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Backpack.Resize(80);
                HallRooms.StartAll(s);
                foreach (var room in HallRooms.Rooms)
                {
                    var q = s.Story.Quest(room);
                    foreach (var o in q.Objectives)
                    {
                        if (!string.IsNullOrEmpty(o.GiveItem)) s.Backpack.Add(o.GiveItem, o.GiveCount);
                        else { var npc = o.Condition.Split(':')[1].Split('>')[0]; s.State.Npcs[npc] = new NpcState { Met = true, Points = 800 }; }
                    }
                }
                HallRooms.DonateAll(s);
                QuestLog.Tick(s);
                foreach (var room in HallRooms.Rooms.Where(r => !HallRooms.IsRestored(s.State, r)))
                    foreach (var o in s.Story.Quest(room).Objectives) HallRooms.Donate(s, room, o);              // a room that only asked for friendship
                foreach (var room in HallRooms.Rooms.Where(r => !HallRooms.IsRestored(s.State, r))) QuestLog.TryComplete(s, s.Story.Quest(room));
                Assert.AreEqual(6, HallRooms.RestoredCount(s.State));
                Assert.IsTrue(s.HasFlag(HallRooms.RestoredFlag), "the hall is restored");
                CollectionAssert.Contains(s.PendingEvents, "hall_ending");
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, HallRooms.CompleteQuest));
            }
        }

        [Test]
        public void EveryHallItem_CanBeObtained()
        {
            var db = RealDb();
            var story = StoryContent.LoadFromResources();
            foreach (var room in HallRooms.Rooms)
                foreach (var o in story.Quest(room).Objectives.Where(o => !string.IsNullOrEmpty(o.GiveItem)))
                    Assert.IsTrue(db.TryGetItem(o.GiveItem, out _), o.GiveItem);
        }

        // ---- festivals ---------------------------------------------------------------------------------------------------

        [Test]
        public void ThereAreFourFestivals_OnePerSeason_OnTheCalendar()
        {
            var fests = StoryContent.LoadFromResources().Events.Where(e => !string.IsNullOrEmpty(e.Calendar)).ToList();
            Assert.AreEqual(4, fests.Count);
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, fests.Select(e => e.CalendarSeason).ToArray());
            foreach (var e in fests) { Assert.IsFalse(e.Once, "annual"); Assert.AreEqual("map", e.Trigger); }
        }

        [Test]
        public void AFestival_PlaysOnItsDay_OnceAYear_AndAgainNextYear()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.State.CurrentMap = MapIds.Village;
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 12, 10 * 60));
                Assert.IsEmpty(EventRunner.FindTriggered(s, MapIds.Village, true), "not the day");
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 13, 10 * 60));
                Assert.AreEqual("festival_spring", EventRunner.FindTriggered(s, MapIds.Village, true).Single().Id);
                Assert.IsEmpty(EventRunner.FindTriggered(s, MapIds.Forest, true), "only in the village");

                Effects.Run(s, "mark:festival.spring");
                Assert.IsEmpty(EventRunner.FindTriggered(s, MapIds.Village, true), "already seen this year");
                s.Clock.SetTime(new GameDateTime(2, Season.Spring, 13, 10 * 60));
                Assert.AreEqual(1, EventRunner.FindTriggered(s, MapIds.Village, true).Count, "a new year, a new festival");
            }
        }

        [Test]
        public void JoiningAFestival_PaysAndSetsTheParticipationFlag()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var graph = s.Story.Dialogue("festival.winter.join");
                var runner = new DialogueRunner(graph, s.World, s.StoryText, e => Effects.Run(s, e));
                s.SetEnergy(10);
                runner.Choose(0);
                Assert.IsTrue(s.HasFlag("festival.winter.joined"));
                Assert.Greater(s.State.Energy, 100);
                Assert.AreEqual(s.Clock.Now.Year, s.GetVar("festival.winter"));
                Assert.Greater(s.State.Npcs["tilda"].Points, 0);
            }
        }

        [Test]
        public void YearlyAtom_AndEnergyEffect_Work()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                Assert.IsTrue(Conditions.Evaluate("unseen:x", s.World));
                Effects.Run(s, "mark:x");
                Assert.IsFalse(Conditions.Evaluate("unseen:x", s.World));
                Assert.IsTrue(Effects.Validate("energy:50", out _));
            }
        }

        [Test]
        public void TheCalendar_MarksFestivalDays()
        {
            var fest = StoryContent.LoadFromResources().Event("festival_fall");
            Assert.AreEqual(16, fest.CalendarDay);
            Assert.AreEqual(2, fest.CalendarSeason);
        }
    }
}
