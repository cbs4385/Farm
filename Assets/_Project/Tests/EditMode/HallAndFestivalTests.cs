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

        [Test]
        public void Donating_NeedsEverything_TakesItemsAndPays_ThenTheLastRoomRestoresTheHall()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Backpack.Resize(80);
                HallRooms.StartAll(s);
                Assert.AreEqual(0, HallRooms.RestoredCount(s.State));
                Assert.IsFalse(HallRooms.Donate(s, "hall_pantry"), "nothing to give yet");

                foreach (var id in new[] { "crop.parsnip", "crop.potato", "crop.kale", "crop.cauliflower" }) s.Backpack.Add(id, 3);
                var gold = s.State.Gold;
                Assert.IsTrue(HallRooms.Donate(s, "hall_pantry"));
                Assert.AreEqual(gold + 500, s.State.Gold);
                Assert.AreEqual(0, s.Backpack.Count("crop.kale"));
                Assert.IsTrue(s.HasFlag("hall.pantry"));
                Assert.IsFalse(s.HasFlag(HallRooms.RestoredFlag));

                // The other five, by giving what they ask for.
                foreach (var room in HallRooms.Rooms.Where(r => r != "hall_pantry"))
                {
                    var q = s.Story.Quest(room);
                    foreach (var o in q.Objectives)
                    {
                        if (!string.IsNullOrEmpty(o.TakeItem)) s.Backpack.Add(o.TakeItem, o.TakeCount);
                        else { var npc = o.Condition.Split(':')[1].Split('>')[0]; s.State.Npcs[npc] = new NpcState { Met = true, Points = 800 }; }
                    }
                    Assert.IsTrue(HallRooms.Donate(s, room), room);
                }
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
                foreach (var o in story.Quest(room).Objectives.Where(o => !string.IsNullOrEmpty(o.TakeItem)))
                    Assert.IsTrue(db.TryGetItem(o.TakeItem, out _), o.TakeItem);
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
