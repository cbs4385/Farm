using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-125: ambient barks. The rules (range, gaps, wrapping) and the shipped bark lines for the slice villagers.
    public class BarksTests
    {
        static readonly string[] Slice = { "wren", "hazel", "bram" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void Range_IsMeasuredInCells()
        {
            Assert.IsTrue(Barks.InRange(3f, 3f));
            Assert.IsTrue(Barks.InRange(Barks.Range, 0f));
            Assert.IsFalse(Barks.InRange(5f, 5f));
        }

        [Test]
        public void Cooldowns_LimitTheWholeMapAndEachVillager()
        {
            var c = new Barks.Cooldowns();
            Assert.IsTrue(c.Ready("wren", 0f));
            c.Mark("wren", 0f);
            Assert.IsFalse(c.Ready("hazel", Barks.SecondsBetweenAny - 1f), "anyone must wait the map-wide gap");
            Assert.IsTrue(c.Ready("hazel", Barks.SecondsBetweenAny + 1f));
            Assert.IsFalse(c.Ready("wren", Barks.SecondsBetweenAny + 1f), "the same villager waits longer");
            Assert.IsTrue(c.Ready("wren", Barks.SecondsPerVillager + 1f));
        }

        [Test]
        public void Wrap_BreaksAtSpacesAndKeepsEveryWord()
        {
            var text = "Fresh cider, fresh gossip, fresh start every Tuesday and then some more words";
            var wrapped = SpeechBubbleFactory.Wrap(text, 22);
            foreach (var row in wrapped.Split('\n')) Assert.LessOrEqual(row.Length, 22, row);
            Assert.AreEqual(text, wrapped.Replace('\n', ' '));
            Assert.AreEqual(string.Empty, SpeechBubbleFactory.Wrap(null));
        }

        [Test]
        public void EachSliceVillager_HasTwentyFourOrMoreBarks_EachADialogue()
        {
            foreach (var npc in Slice)
            {
                var set = _story.Set(Barks.SetId(npc));
                Assert.IsNotNull(set, npc + " has a bark set");
                Assert.GreaterOrEqual(set.Entries.Count, 24, npc);
                foreach (var e in set.Entries)
                {
                    var d = _story.Dialogue(e.Dialogue);
                    Assert.IsNotNull(d, e.Dialogue);
                    Assert.AreEqual(1, d.Nodes.Count, e.Dialogue + " is a single line");
                    Assert.AreEqual(npc, d.Nodes[0].Speaker, e.Dialogue);
                }
            }
        }

        [Test]
        public void Barks_AreShortAndInVoice()
        {
            var en = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in Slice)
                foreach (var e in _story.Set(Barks.SetId(npc)).Entries)
                {
                    var text = en[_story.Dialogue(e.Dialogue).Nodes[0].Text];
                    var words = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                    Assert.LessOrEqual(words, NarrativeLint.BarkWords, $"{e.Dialogue}: '{text}'");
                    if (npc != "wren") Assert.IsFalse(text.Contains("!"), $"{e.Dialogue} (no exclamation marks for {npc})");
                }
        }

        [Test]
        public void EveryVillagerHasSomeUnconditionalBarks_SoASilentGameIsRare()
        {
            foreach (var npc in Slice)
                Assert.GreaterOrEqual(_story.Set(Barks.SetId(npc)).Entries.Count(e => string.IsNullOrEmpty(e.Condition) || e.Condition == "true"), 6, npc);
        }

        [Test]
        public void BarksSetting_DefaultsOn()
        {
            Assert.IsTrue(new SettingsData().Barks);
        }
    }
}
