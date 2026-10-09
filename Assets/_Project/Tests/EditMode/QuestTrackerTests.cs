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

        [Test]
        public void OnlyTheFirstThreeQuestsAreListed()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                foreach (var q in s.Story.Quests.Where(q => !q.AutoStart).Take(5)) s.State.Quests[q.Id] = new QuestProgress { Status = QuestStatus.Active };
                var lines = QuestTracker.Text(s).Split('\n').Count(l => !l.StartsWith("  - "));
                Assert.LessOrEqual(lines, QuestTracker.MaxQuests);
            }
        }
    }
}
