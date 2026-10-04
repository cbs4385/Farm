using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;
using Farm.Data;

namespace Farm.Tests
{
    // The six Enhanced villagers (docs/narrative/BIBLE_ENHANCED.md): about 75 new talk lines each on top of the shipped ones, 15 barks,
    // a heart-8 scene, a quest, a birthday letter, world reactions, a social profile, a topic, tastes and their own voice and sound.
    public class EnhancedContentTests
    {
        static readonly string[] Six = { "marcus", "odalys", "felix", "dorian", "elara", "ione" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EachVillager_HasRichTalk_AndEveryLineExists()
        {
            foreach (var npc in Six)
            {
                var set = _story.Set($"npc.{npc}.talk");
                Assert.GreaterOrEqual(set.Entries.Count, 85, npc + " talk entries");
                foreach (var e in set.Entries) Assert.IsNotNull(_story.Dialogue(e.Dialogue), $"{npc}: {e.Dialogue}");
            }
        }

        [Test]
        public void EachVillager_HasFifteenBarks_ShortAndInVoice()
        {
            var en = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in Six)
            {
                var set = _story.Set(Barks.SetId(npc));
                Assert.GreaterOrEqual(set.Entries.Count, 15, npc);
                foreach (var e in set.Entries)
                {
                    var text = en[_story.Dialogue(e.Dialogue).Nodes[0].Text];
                    Assert.LessOrEqual(text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length, NarrativeLint.BarkWords, e.Dialogue + ": " + text);
                    if (npc == "marcus" || npc == "dorian") Assert.IsFalse(text.Contains("!"), e.Dialogue);
                }
            }
        }

        [Test]
        public void EachVillager_HasAHeartEightScene_ChainedOnHeartFive()
        {
            foreach (var npc in Six)
            {
                var ev = _story.Event($"{npc}_heart8");
                Assert.IsNotNull(ev, npc);
                StringAssert.Contains($"hearts:{npc}>=8", ev.Condition);
                StringAssert.Contains($"flag:event.{npc}_heart5", ev.Condition);
                Assert.IsTrue(Memories.IsMemory(ev), npc + " is a memory");
                Assert.IsFalse(string.IsNullOrEmpty(ev.Tag), npc + " is tagged");
                Assert.IsTrue(ev.Steps.Any(s => s.Type == "emote"), npc + " has a visible beat");
            }
        }

        [Test]
        public void EachVillager_HasABirthdayLetter_ReactionsASocialProfileAndATopic()
        {
            foreach (var npc in Six)
            {
                Assert.IsTrue(_story.Letters.Any(l => l.Id == npc + "_bday" && l.Sender == npc), npc + " birthday letter");
                Assert.GreaterOrEqual(_story.Reactions.Count(r => r.Npcs == npc), 3, npc + " reactions");
                Assert.IsNotNull(_story.Dialogue($"social.{npc}.joke.great"), npc + " social lines");
                Assert.IsTrue(_story.Topics.Any(t => t.Npc == npc), npc + " topic");
            }
        }

        [Test]
        public void EachVillager_HasThreeLovedLikedAndDislikedItems()
        {
            foreach (var npc in Six)
            {
                var def = AssetDatabase.LoadAssetAtPath<NpcDefinition>($"Assets/_Project/Data/Npcs/{npc}.asset");
                Assert.AreEqual(3, def.Loved.Count, npc + " loved");
                Assert.AreEqual(3, def.Liked.Count, npc + " liked");
                Assert.AreEqual(3, def.Disliked.Count, npc + " disliked");
            }
        }

        [Test]
        public void EachVillager_HasTheirOwnVoiceAndSignatureSound()
        {
            var pitches = Six.Select(n => VoiceProfiles.For(n).Pitch).ToList();
            Assert.AreEqual(Six.Length, pitches.Distinct().Count(), "every voice has its own pitch");
            foreach (var npc in Six) Assert.Greater(VoiceSynth.Signature(npc).Length, 1000, npc);
        }

        [Test]
        public void TheGiftLines_NameRealItems()
        {
            var items = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
                .Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(i => i != null).Select(i => i.Id).ToHashSet();
            foreach (var npc in Six)
                foreach (var d in _story.Dialogues.Where(x => x.Id.StartsWith($"npc.{npc}.gift.item.")))
                    Assert.IsTrue(items.Contains(d.Id.Substring($"npc.{npc}.gift.item.".Length)), d.Id);
        }
    }
}
