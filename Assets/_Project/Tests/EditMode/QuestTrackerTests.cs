using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest chat 2026-10-09: the active quests and what they still need are listed on screen, not only in the journal.
    public class QuestTrackerTests
    {
        TestSessionFixture Fixture()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var f = new TestSessionFixture(Resources.Load<GameDatabase>(GameDatabase.ResourcePath).AllItems);
            f.Session.Story = StoryContent.LoadFromResources();
            return f;
        }

        [Test]
        public void NothingActive_NothingIsListed()
        {
            using (var f = Fixture()) Assert.AreEqual(string.Empty, QuestTracker.Text(f.Session));
        }

        [Test]
        public void AnActiveQuest_ListsItsUnmetObjectives_WithTheCountsOfWhatIsHeldOrGiven()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var def = s.Story.Quest("elara_garden");
                Assert.IsTrue(QuestLog.Start(s, def));
                s.Backpack.Add("forage.dandelion", 2);
                var text = QuestTracker.Text(s);
                StringAssert.Contains(L.Get(def.TitleKey), text);
                StringAssert.Contains("(2/3)", text, "two of three dandelions");
                StringAssert.Contains("(0/3)", text, "no wild garlic yet");
                s.Backpack.Add("forage.dandelion", 1);
                StringAssert.DoesNotContain("dandelion", QuestTracker.Text(s).ToLowerInvariant(), "a finished objective is left out");
            }
        }

        [Test]
        public void AnObjectiveThatIsMet_IsLeftOut_AndGivenItemsCountWhatWasGiven()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var room = s.Story.Quest("hall_pantry");
                QuestLog.Start(s, room);
                var first = room.Objectives.First(o => !string.IsNullOrEmpty(o.GiveItem));
                s.Backpack.Add(first.GiveItem, 1);
                HallRooms.Donate(s, "hall_pantry", first);
                StringAssert.Contains("(1/" + first.GiveCount + ")", QuestTracker.Text(s));
            }
        }

        // Playtest 2026-10-09: the screen follows one quest, chosen in the Journal; a lone quest is followed by itself.
        [Test]
        public void ALoneQuest_IsFollowedAtOnce_WithNothingToChoose()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var only = s.Story.Quest("elara_garden");
                QuestLog.Start(s, only);
                foreach (var q in QuestLog.Active(s).Where(q => q.Id != only.Id).ToList()) s.State.Quests.Remove(q.Id);
                Assert.AreEqual("elara_garden", QuestTracker.TrackedId(s));
                StringAssert.Contains(L.Get(only.TitleKey), QuestTracker.Text(s));
            }
        }

        [Test]
        public void OnlyTheFollowedQuestIsListed_AndTheJournalChoosesWhichOne()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var active = s.Story.Quests.Where(q => !q.AutoStart).Take(2).ToList();
                foreach (var q in active) s.State.Quests[q.Id] = new QuestProgress { Status = QuestStatus.Active };
                var first = QuestTracker.TrackedId(s);
                Assert.IsNotNull(first, "a quest is followed");
                var text = QuestTracker.Text(s);
                Assert.AreEqual(1, text.Split('\n').Count(l => !l.StartsWith("  - ")), "one title");

                var other = QuestLog.Active(s).First(q => q.Id != first);
                QuestTracker.Track(s, other.Id);
                Assert.AreEqual(other.Id, QuestTracker.TrackedId(s));
                StringAssert.Contains(L.Get(other.TitleKey), QuestTracker.Text(s));
                Assert.AreEqual(1, s.State.Flags.Count(x => x.StartsWith(QuestTracker.TrackedPrefix)), "only one is followed");
            }
        }

        [Test]
        public void WhenTheFollowedQuestIsDone_TheNextActiveOneIsFollowed()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var two = s.Story.Quests.Where(q => !q.AutoStart).Take(2).ToList();
                foreach (var q in two) s.State.Quests[q.Id] = new QuestProgress { Status = QuestStatus.Active };
                foreach (var q in QuestLog.Active(s).Where(q => !two.Any(t => t.Id == q.Id)).ToList()) s.State.Quests.Remove(q.Id);
                QuestTracker.Track(s, two[0].Id);
                s.State.Quests[two[0].Id].Status = QuestStatus.Done;
                Assert.AreEqual(two[1].Id, QuestTracker.TrackedId(s));
                s.State.Quests[two[1].Id].Status = QuestStatus.Done;
                CollectionAssert.DoesNotContain(new[] { two[0].Id, two[1].Id }, QuestTracker.TrackedId(s), "finished quests are not followed");
                StringAssert.DoesNotContain(L.Get(two[1].TitleKey), QuestTracker.Text(s));
            }
        }
    }
}
