using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // RELATIONSHIPS.md section 6.2: eleven ties that had no writing, each askable in both directions, and the relationship table that backs them.
    public class RelationshipGossipTests
    {
        static readonly (string who, string about)[] NewPairs =
        {
            ("felix", "wren"), ("wren", "felix"), ("felix", "ione"), ("ione", "felix"), ("ione", "juno"), ("juno", "ione"),
            ("marcus", "wren"), ("wren", "marcus"), ("marcus", "hazel"), ("hazel", "marcus"), ("odalys", "wren"), ("wren", "odalys"),
            ("odalys", "bram"), ("bram", "odalys"), ("elara", "tilda"), ("tilda", "elara"), ("dorian", "juno"), ("juno", "dorian"),
            ("piper", "ione"), ("ione", "piper"), ("piper", "odalys"), ("odalys", "piper"),
        };

        [Test]
        public void EveryNewTie_HasATopicInBothDirections_ThatPlaysARealDialogue()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var (who, about) in NewPairs)
            {
                var topic = story.Topics.FirstOrDefault(t => t.Id == $"{who}.gossip.{about}");
                Assert.IsNotNull(topic, $"{who} about {about}");
                Assert.AreEqual(who, topic.Npc);
                StringAssert.Contains($"flag:met.{about}", topic.Condition);
                StringAssert.Contains($"hearts:{who}>=3", topic.Condition);
                Assert.IsNotNull(story.Dialogue(topic.Dialogue), topic.Dialogue);
                Assert.IsTrue(en.ContainsKey(topic.LabelKey), topic.LabelKey);
                Assert.GreaterOrEqual(topic.CooldownDays, 6, "gossip is not asked every day");
                Assert.IsTrue(VillagerRelations.IsWritten(who, about), $"the relationship table has no attitude for {who} about {about}");
            }
            Assert.AreEqual(22, NewPairs.Length);
        }

        [Test]
        public void EveryGossipTopic_IsAboutSomeoneTheSpeakerHasAWrittenAttitudeTo()
        {
            var story = StoryContent.LoadFromResources();
            var topics = story.Topics.Where(t => t.Id.Contains(".gossip.")).ToList();
            Assert.GreaterOrEqual(topics.Count, 46);
            foreach (var t in topics)
            {
                var about = t.Id.Substring(t.Id.LastIndexOf('.') + 1);
                if (!NpcIds.All.Contains(about)) continue;     // the single-word gossip topics (wren.gossip) have no target
                Assert.IsTrue(VillagerRelations.IsWritten(t.Npc, about), t.Id);
            }
        }

        [Test]
        public void TheTable_IsSquare_AndEveryCellIsKnown()
        {
            var rows = VillagerRelations.RawRows;
            Assert.AreEqual(NpcIds.All.Length, rows.Length);
            for (var r = 0; r < rows.Length; r++)
            {
                Assert.AreEqual(NpcIds.All.Length, rows[r].Length, NpcIds.All[r]);
                for (var c = 0; c < rows[r].Length; c++)
                {
                    if (r == c) Assert.AreEqual('-', rows[r][c], NpcIds.All[r]);
                    else StringAssert.Contains(rows[r][c].ToString(), "0123RS.", $"{NpcIds.All[r]} about {NpcIds.All[c]}");
                }
            }
        }

        [Test]
        public void EveryVillager_HasAnAttitudeToAtLeastSixOthers_AndTheDocumentedRivalriesAreMild()
        {
            foreach (var who in NpcIds.All)
                Assert.GreaterOrEqual(NpcIds.All.Count(o => VillagerRelations.IsWritten(who, o)), 6, who);
            Assert.AreEqual(Attitude.Rivalry, VillagerRelations.Of("wren", "marcus"));
            Assert.AreEqual(Attitude.Rivalry, VillagerRelations.Of("odalys", "elara"));
            Assert.AreEqual(Attitude.Close, VillagerRelations.Of("bram", "juno"));
        }
    }
}
