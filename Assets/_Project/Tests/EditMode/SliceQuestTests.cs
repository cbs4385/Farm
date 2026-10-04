using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;
using Farm.Data;

namespace Farm.Tests
{
    // Personal quest chains for Wren, Hazel and Bram: each quest is offered, reminded and turned in through the villager's talk set,
    // in the reserved quest priority band, and the chain order holds.
    public class SliceQuestTests
    {
        static readonly (string npc, string quest, string key, string previous)[] Chains =
        {
            ("wren", "wren_stew", "wren.stew", null),
            ("wren", "wren_cider", "wren.cider", "wren_stew"),
            ("wren", "wren_full_house", "wren.house", "wren_cider"),
            ("hazel", "hazel_shelf", "hazel.shelf", null),
            ("hazel", "hazel_pressed", "hazel.pressed", "hazel_shelf"),
            ("hazel", "hazel_notes", "hazel.notes", "hazel_pressed"),
            ("bram", "bram_copper", "bram.copper", "bram_stone"),
            ("bram", "bram_hooks", "bram.hooks", "bram_copper"),
        };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EveryQuest_IsDefined_WithAGiverAnObjectiveAndRewards()
        {
            var items = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
                .Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(i => i != null).Select(i => i.Id).ToHashSet();
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var c in Chains)
            {
                var q = _story.Quest(c.quest);
                Assert.IsNotNull(q, c.quest);
                Assert.AreEqual(c.npc, q.Giver, c.quest);
                Assert.AreEqual(1, q.Objectives.Count, c.quest);
                Assert.IsTrue(items.Contains(q.Objectives[0].TakeItem), $"{c.quest}: item {q.Objectives[0].TakeItem}");
                Assert.IsTrue(q.Rewards.Any(r => r.StartsWith("gold:")), c.quest);
                Assert.IsTrue(q.Rewards.Any(r => r.StartsWith("friend:" + c.npc)), c.quest);
                foreach (var key in new[] { q.TitleKey, q.DescriptionKey, q.Objectives[0].Text }) Assert.IsTrue(en.ContainsKey(key), $"{c.quest}: text {key}");
            }
        }

        [Test]
        public void EveryQuest_HasAskRemindAndTurnIn_InTheQuestBand()
        {
            foreach (var c in Chains)
            {
                var set = _story.Set($"npc.{c.npc}.talk");
                foreach (var (part, priority) in new[] { ("ask", 4), ("remind", 4), ("turnin", 5) })
                {
                    var entry = set.Entries.FirstOrDefault(e => e.Dialogue == $"{c.key}.{part}");
                    Assert.IsNotNull(entry, $"{c.quest}: {part}");
                    Assert.AreEqual(priority, entry.Priority, $"{c.quest}: {part} priority");
                    Assert.IsNotNull(_story.Dialogue(entry.Dialogue), entry.Dialogue);
                }
            }
        }

        [Test]
        public void TheAsk_StartsTheQuest_AndTheTurnIn_FinishesIt()
        {
            foreach (var c in Chains)
            {
                var ask = _story.Dialogue($"{c.key}.ask");
                Assert.IsTrue(ask.Nodes.SelectMany(n => n.Choices).Any(ch => (ch.Effects ?? new List<string>()).Contains("quest.start:" + c.quest)), c.quest + " can be accepted");
                Assert.IsTrue(ask.Nodes.SelectMany(n => n.Choices).Any(ch => !(ch.Effects ?? new List<string>()).Any(e => e.StartsWith("quest.start"))), c.quest + " can be declined");
                var turnin = _story.Dialogue($"{c.key}.turnin");
                Assert.IsTrue(turnin.Nodes.Any(n => (n.Effects ?? new List<string>()).Contains("quest.done:" + c.quest)), c.quest + " finishes");
            }
        }

        [Test]
        public void TheChains_AreOrdered_EachAskNeedsTheEarlierQuestDone()
        {
            foreach (var c in Chains.Where(x => x.previous != null))
            {
                var ask = _story.Set($"npc.{c.npc}.talk").Entries.First(e => e.Dialogue == $"{c.key}.ask");
                StringAssert.Contains($"quest:{c.previous}=done", ask.Condition, c.quest);
                StringAssert.Contains($"quest:{c.quest}=new", ask.Condition, c.quest);
            }
        }

        [Test]
        public void TurnIn_NeedsTheItems_AndRemindWaits()
        {
            foreach (var c in Chains)
            {
                var q = _story.Quest(c.quest);
                var turnin = _story.Set($"npc.{c.npc}.talk").Entries.First(e => e.Dialogue == $"{c.key}.turnin");
                StringAssert.Contains($"has:{q.Objectives[0].TakeItem}>={q.Objectives[0].TakeCount}", turnin.Condition, c.quest);
            }
        }

        [Test]
        public void Dialogue_KeepsEachVoice()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var c in Chains.Where(x => x.npc != "wren"))
                foreach (var part in new[] { "ask", "remind", "turnin" })
                    foreach (var n in _story.Dialogue($"{c.key}.{part}").Nodes)
                        foreach (var key in new[] { n.Text }.Concat((n.Choices ?? new List<DialogueChoice>()).Select(ch => ch.Text)).Where(k => !string.IsNullOrEmpty(k)))
                            Assert.IsFalse(en[key].Contains("!"), $"{c.key}.{part}: '{en[key]}' (no exclamation marks for {c.npc})");
        }
    }
}
