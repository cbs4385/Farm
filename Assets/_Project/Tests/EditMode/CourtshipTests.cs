using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-109: optional courtship without marriage for the eight romanceable villagers.
    public class CourtshipTests
    {
        static readonly string[] Romanceable = { "wren", "hazel", "juno", "piper", "felix", "dorian", "elara", "ione" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EveryRomanceableVillager_HasAnAskAScheduledSecondAskAndAPartnersScene()
        {
            foreach (var npc in Romanceable)
            {
                Assert.IsNotNull(_story.Event(npc + "_courtship"), npc);
                Assert.IsNotNull(_story.Event(npc + "_courtship2"), npc);
                var partners = _story.Event(npc + "_partners");
                Assert.IsNotNull(partners, npc);
                StringAssert.Contains($"flag:choice.courtship.{npc}.yes", partners.Condition);
                Assert.IsTrue(partners.Steps.Any(s => s.Type == "emote"), npc + " has a visible beat");
                Assert.AreEqual(3, _story.Dialogue(npc + ".courtship").Nodes.SelectMany(n => n.Choices).Count(), npc);
                Assert.AreEqual(3, _story.Dialogue(npc + ".partners").Nodes.SelectMany(n => n.Choices).Count(), npc);
            }
        }

        [Test]
        public void TheAsk_OffersYesFriendsAndNotYet_AndNeverRequiresAnswering()
        {
            foreach (var npc in Romanceable)
            {
                var flags = _story.Dialogue(npc + ".courtship").Nodes.SelectMany(n => n.Choices).SelectMany(c => c.Effects).Where(e => e.StartsWith("flag:")).ToList();
                CollectionAssert.Contains(flags, $"flag:choice.courtship.{npc}.yes");
                CollectionAssert.Contains(flags, $"flag:choice.courtship.{npc}.friends");
                CollectionAssert.Contains(flags, $"flag:choice.courtship.{npc}.later");
                // Staying friends closes the offer for good; the cozy game never nags.
                StringAssert.Contains($"!flag:choice.courtship.{npc}.friends", _story.Event(npc + "_courtship").Condition);
            }
        }

        [Test]
        public void OnlyOneCourtshipAtATime()
        {
            foreach (var npc in Romanceable)
                foreach (var other in Romanceable.Where(o => o != npc))
                    StringAssert.Contains($"!flag:choice.courtship.{other}.yes", _story.Event(npc + "_courtship").Condition);
        }

        [Test]
        public void TheStage_IsPartners_OnlyOnceTheSceneHasSetTheFlag()
        {
            Assert.AreEqual(RelationshipStage.Confidant, RelationshipStages.For(10, false));
            Assert.AreEqual(RelationshipStage.Partners, RelationshipStages.For(10, true));
            Assert.AreEqual(RelationshipStage.Partners, RelationshipStages.For(8, true));
        }

        [Test]
        public void PartnerLines_ExistForEveryRomanceableVillager_AndOnlyShowForPartners()
        {
            foreach (var npc in Romanceable)
            {
                var lines = _story.Set($"npc.{npc}.talk").Entries.Where(e => e.Dialogue.StartsWith(npc + ".partners.talk")).ToList();
                Assert.GreaterOrEqual(lines.Count, 3, npc);
                Assert.IsTrue(lines.All(e => e.Condition == $"flag:partners.{npc}"), npc);
            }
        }

        [Test]
        public void TextExists_ForEveryScene()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in Romanceable)
                foreach (var key in new[] { $"event.{npc}_courtship.intro", $"event.{npc}_courtship.after", $"memory.{npc}_courtship", $"event.{npc}_partners.intro", $"event.{npc}_partners.after", $"memory.{npc}_partners", "stage.partners" })
                    Assert.IsTrue(en.ContainsKey(key), key);
        }
    }
}
