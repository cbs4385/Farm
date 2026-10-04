using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;

namespace Farm.Tests
{
    // T-107: a birthday scene for each of the twelve villagers: on the day, yearly, one choice, a gift back, a thank-you letter after, and
    // a letter when the player missed it.
    public class BirthdayScenesTests
    {
        static readonly string[] All = { "wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione" };

        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public int Away = -1, Year = 1, Mark;
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => Mark;
            public GameDateTime Now => new GameDateTime(Year, Season.Spring, 10, 12 * 60);
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => 5;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
            public int BirthdayDaysAway(string npcId) => Away;
        }

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EveryVillager_HasABirthdayScene_ThatRepeatsEachYear()
        {
            foreach (var npc in All)
            {
                var ev = _story.Event(npc + "_birthday");
                Assert.IsNotNull(ev, npc);
                Assert.IsFalse(ev.Once, npc + ": a birthday comes every year");
                StringAssert.Contains($"birthday.in:{npc}==0", ev.Condition);
                StringAssert.Contains($"unseen:birthday.{npc}", ev.Condition);
                Assert.IsTrue(Memories.IsMemory(ev), npc);
                Assert.IsFalse(string.IsNullOrEmpty(ev.Tag), npc);
                Assert.IsTrue(ev.Steps.Any(s => s.Type == "emote"), npc + " has a visible beat");
            }
        }

        [Test]
        public void TheSceneEnds_ByMarkingTheYear_PayingFriendship_GivingAGift_AndSendingAThankYou()
        {
            var items = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
                .Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(i => i != null).Select(i => i.Id).ToHashSet();
            foreach (var npc in All)
            {
                var effects = _story.Event(npc + "_birthday").Steps.Where(s => s.Type == "effects").SelectMany(s => s.Effects).ToList();
                CollectionAssert.Contains(effects, $"mark:birthday.{npc}");
                Assert.IsTrue(effects.Any(e => e.StartsWith($"friend:{npc},")), npc);
                var gift = effects.FirstOrDefault(e => e.StartsWith("give:"));
                Assert.IsNotNull(gift, npc + " gives something back");
                Assert.IsTrue(items.Contains(gift.Substring(5).Split(',')[0]), npc + ": " + gift);
                CollectionAssert.Contains(effects, $"mail:{npc}_bday_thanks");
                Assert.IsNotNull(_story.Letters.FirstOrDefault(l => l.Id == npc + "_bday_thanks"), npc);
            }
        }

        [Test]
        public void TheSceneIsDue_OnlyOnTheDay_AndOnlyOncePerYear()
        {
            foreach (var npc in All)
            {
                var cond = _story.Event(npc + "_birthday").Condition.Replace(" && ", "&&").Split(new[] { "&&" }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Where(c => c.Contains("birthday.in") || c.Contains("unseen")).Aggregate((a, b) => a + " && " + b);
                var w = new World { Away = 0, Year = 1, Mark = 0 };
                Assert.IsTrue(Conditions.TryEvaluate(cond, w, out var today) && today, npc + " on the day");
                w.Away = 5;
                Assert.IsTrue(Conditions.TryEvaluate(cond, w, out var other) && !other, npc + " another day");
                w.Away = 0; w.Mark = 1;
                Assert.IsTrue(Conditions.TryEvaluate(cond, w, out var done) && !done, npc + " already celebrated this year");
                w.Year = 2;
                Assert.IsTrue(Conditions.TryEvaluate(cond, w, out var next) && next, npc + " next year");
            }
        }

        [Test]
        public void AMissedBirthday_GetsALetter_TheDayAfter()
        {
            foreach (var npc in All)
            {
                var letter = _story.Letters.First(l => l.Id == npc + "_bday_missed");
                var w = new World { Away = 111, Mark = 0 };
                w.Flags.Add("met." + npc);
                Assert.IsTrue(Conditions.TryEvaluate(letter.Condition, w, out var yes) && yes, npc + " the day after");
                w.Away = 40;
                Assert.IsTrue(Conditions.TryEvaluate(letter.Condition, w, out var no) && !no, npc + " mid-year");
                w.Away = 111; w.Mark = 1;
                Assert.IsTrue(Conditions.TryEvaluate(letter.Condition, w, out var attended) && !attended, npc + " attended");
                CollectionAssert.Contains(letter.Effects, $"mark:birthday.{npc}");
            }
        }

        [Test]
        public void EveryChoice_HasAReply_AndTheGiftLettersHaveText()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in All)
            {
                var d = _story.Dialogue(npc + ".birthday");
                Assert.AreEqual(3, d.Nodes.SelectMany(n => n.Choices).Count(), npc);
                foreach (var key in new[] { $"letter.{npc}_bday_thanks.body", $"letter.{npc}_bday_missed.body", $"event.{npc}_birthday.intro", $"event.{npc}_birthday.after" })
                    Assert.IsTrue(en.ContainsKey(key), key);
            }
        }
    }
}
