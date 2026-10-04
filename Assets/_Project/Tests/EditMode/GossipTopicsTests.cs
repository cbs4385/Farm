using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-122: every villager in a pairing can be asked about the person they are paired with.
    public class GossipTopicsTests
    {
        static readonly (string who, string about)[] Pairs =
        {
            ("juno", "bram"), ("bram", "juno"), ("hazel", "ione"), ("ione", "hazel"), ("elara", "odalys"), ("odalys", "elara"), ("marcus", "dorian"), ("dorian", "marcus"),
            ("wren", "piper"), ("piper", "wren"), ("tilda", "felix"), ("felix", "tilda"), ("bram", "marcus"), ("marcus", "bram"), ("tilda", "wren"), ("wren", "tilda"),
            ("piper", "juno"), ("juno", "piper"), ("dorian", "hazel"), ("hazel", "dorian"), ("felix", "elara"), ("elara", "felix"), ("odalys", "ione"), ("ione", "odalys"),
        };

        [Test]
        public void EveryPairing_HasATopicInBothDirections_ThatPlaysARealDialogue()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var (who, about) in Pairs)
            {
                var topic = story.Topics.FirstOrDefault(t => t.Id == $"{who}.gossip.{about}");
                Assert.IsNotNull(topic, $"{who} about {about}");
                Assert.AreEqual(who, topic.Npc);
                StringAssert.Contains($"flag:met.{about}", topic.Condition);
                StringAssert.Contains($"hearts:{who}>=3", topic.Condition);
                Assert.IsNotNull(story.Dialogue(topic.Dialogue), topic.Dialogue);
                Assert.IsTrue(en.ContainsKey(topic.LabelKey), topic.LabelKey);
                Assert.GreaterOrEqual(topic.CooldownDays, 6, "gossip is not asked every day");
            }
            Assert.AreEqual(24, Pairs.Length);
        }
    }
}
