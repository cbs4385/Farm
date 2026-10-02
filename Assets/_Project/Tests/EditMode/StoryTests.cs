using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-034: story conditions and effects, the dialogue runner, dialogue sets, and loading story JSON.
    public class StoryTests
    {
        sealed class FakeWorld : IWorldQuery, IGameQuery
        {
            public HashSet<string> Flags = new HashSet<string>();
            public Dictionary<string, int> Vars = new Dictionary<string, int>();
            public Dictionary<string, int> Hearts_ = new Dictionary<string, int>();
            public Dictionary<string, int> Items = new Dictionary<string, int>();
            public Dictionary<string, string> Quests = new Dictionary<string, string>();
            public GameDateTime Now { get; set; } = GameDateTime.NewGame;
            public string Weather { get; set; } = "sunny";
            public string MapId { get; set; } = "Farm";
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => Vars.TryGetValue(name, out var v) ? v : 0;
            public int Hearts(string npcId) => Hearts_.TryGetValue(npcId, out var v) ? v : 0;
            public int ItemCount(string itemId) => Items.TryGetValue(itemId, out var v) ? v : 0;
            public string QuestState(string id) => Quests.TryGetValue(id, out var v) ? v : "new";
            public bool KnowsRecipe(string id) => false;
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        // ---- condition atoms -------------------------------------------------------------------------------------

        [Test]
        public void HeartsAtom_ComparesFriendship()
        {
            var w = new FakeWorld();
            w.Hearts_["tilda"] = 4;
            Assert.IsTrue(Conditions.Evaluate("hearts:tilda>=4", w));
            Assert.IsFalse(Conditions.Evaluate("hearts:tilda>=5", w));
            Assert.IsTrue(Conditions.Evaluate("hearts:bram==0", w));
        }

        [Test]
        public void HasAtom_ChecksItemCounts()
        {
            var w = new FakeWorld();
            w.Items["crop.parsnip"] = 3;
            Assert.IsTrue(Conditions.Evaluate("has:crop.parsnip>=3", w));
            Assert.IsFalse(Conditions.Evaluate("has:crop.parsnip>=4", w));
        }

        [Test]
        public void QuestAtom_ReadsQuestState_AndDefaultsToNew()
        {
            var w = new FakeWorld();
            Assert.IsTrue(Conditions.Evaluate("quest:first=new", w));
            w.Quests["first"] = "active";
            Assert.IsTrue(Conditions.Evaluate("quest:first=active", w));
            Assert.IsFalse(Conditions.Evaluate("quest:first=done", w));
        }

        [Test]
        public void StoryAtoms_AreFalseWithAPlainWorldQuery()
        {
            var plain = new PlainWorld();
            Assert.IsFalse(Conditions.Evaluate("hearts:tilda>=1", plain));
            Assert.IsFalse(Conditions.Evaluate("has:crop.parsnip>=1", plain));
            Assert.IsTrue(Conditions.Evaluate("quest:x=new", plain));
        }

        sealed class PlainWorld : IWorldQuery
        {
            public bool HasFlag(string flag) => false;
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Farm";
        }

        [Test]
        public void WeekdayAtom_MatchesTheDayOfTheWeek()
        {
            var w = new FakeWorld { Now = new GameDateTime(1, Season.Spring, 7) };    // day 7 = Sunday
            Assert.IsTrue(Conditions.Evaluate("weekday:sun", w));
            Assert.IsFalse(Conditions.Evaluate("weekday:mon", w));
            w.Now = new GameDateTime(1, Season.Summer, 1);
            Assert.IsTrue(Conditions.Evaluate("weekday:mon", w));
            Assert.IsFalse(Conditions.Validate("weekday:someday", out _));
        }

        [Test]
        public void SplitHelper_ParsesTheOperator()
        {
            Assert.IsTrue(StoryConditions.Split("tilda>=4", out var id, out var op, out var n));
            Assert.AreEqual(("tilda", ">=", 4), (id, op, n));
            Assert.IsFalse(StoryConditions.Split("tilda", out _, out _, out _));
            Assert.IsFalse(StoryConditions.Split("tilda>=x", out _, out _, out _));
        }

        // ---- effects ---------------------------------------------------------------------------------------------

        [Test]
        public void Effects_ParseVerbAndArguments()
        {
            Assert.IsTrue(Effects.TrySplit("give: crop.parsnip , 3", out var verb, out var args));
            Assert.AreEqual("give", verb);
            CollectionAssert.AreEqual(new[] { "crop.parsnip", "3" }, args);
            Assert.IsFalse(Effects.TrySplit("  ", out _, out _));
        }

        [Test]
        public void Effects_Validate_RejectsUnknownVerbsAndBadArgumentCounts()
        {
            Assert.IsTrue(Effects.Validate("flag:met_tilda", out _));
            Assert.IsFalse(Effects.Validate("explode:now", out var unknown));
            StringAssert.Contains("unknown effect", unknown);
            Assert.IsFalse(Effects.Validate("flag", out _));
            Assert.IsFalse(Effects.Validate("setvar:dread", out _));
        }

        [Test]
        public void Effects_ChangeTheGame()
        {
            using (var f = new TestSessionFixture(new[] { TestSessionFixture.Item("crop.parsnip") }))
            {
                var s = f.Session;
                Effects.Run(s, "flag:a");
                Assert.IsTrue(s.HasFlag("a"));
                Effects.Run(s, "unflag:a");
                Assert.IsFalse(s.HasFlag("a"));
                Effects.Run(s, "setvar:dread,7");
                Effects.Run(s, "addvar:dread,5,0,10");
                Assert.AreEqual(10, s.GetVar("dread"));
                var gold = s.State.Gold;
                Effects.Run(s, "gold:50");
                Effects.Run(s, "gold:-20");
                Assert.AreEqual(gold + 30, s.State.Gold);
                Effects.Run(s, "gold:-100000");
                Assert.AreEqual(0, s.State.Gold, "gold never goes below zero");
                Effects.Run(s, "give:crop.parsnip,4");
                Assert.AreEqual(4, s.Backpack.Count("crop.parsnip"));
                Effects.Run(s, "take:crop.parsnip,3");
                Assert.AreEqual(1, s.Backpack.Count("crop.parsnip"));
                Effects.Run(s, "xp:farming,120");
                Assert.AreEqual(2, s.GetSkillLevel(SkillIds.Farming));
            }
        }

        [Test]
        public void Effects_ABadEffectIsLoggedAndSkipped()
        {
            using (var f = new TestSessionFixture())
            {
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Effect rejected"));
                Effects.Run(f.Session, "frobnicate:1");
                Effects.Run(f.Session, "flag:still_works");
                Assert.IsTrue(f.Session.HasFlag("still_works"));
            }
        }

        [Test]
        public void Effects_ModulesCanAddVerbs()
        {
            using (var f = new TestSessionFixture())
            {
                Effects.Register("dread", 1, 1, (s, a) => s.AddVar("dread", Effects.Int(a[0])));
                Effects.Run(f.Session, "dread:3");
                Assert.AreEqual(3, f.Session.GetVar("dread"));
                Assert.IsTrue(Effects.Validate("dread:3", out _));
            }
        }

        // ---- the dialogue runner ---------------------------------------------------------------------------------

        static DialogueGraph Graph(params DialogueNode[] nodes) => new DialogueGraph { Id = "t", Start = nodes[0].Id, Nodes = nodes.ToList() };

        static DialogueNode Node(string id, string text, string next = null, string condition = null, params string[] effects) =>
            new DialogueNode { Id = id, Text = text, Next = next, Condition = condition, Speaker = "tilda", Effects = effects.ToList() };

        static DialogueRunner Run(DialogueGraph g, IWorldQuery w, List<string> effects = null) =>
            new DialogueRunner(g, w, (k, a) => k, e => effects?.Add(e));

        [Test]
        public void Runner_PlaysLinesInOrder_ThenFinishes()
        {
            var r = Run(Graph(Node("a", "one", "b"), Node("b", "two")), new FakeWorld());
            Assert.AreEqual("one", r.Current.Text);
            r.Advance();
            Assert.AreEqual("two", r.Current.Text);
            Assert.IsFalse(r.Finished);
            r.Advance();
            Assert.IsTrue(r.Finished);
            Assert.IsNull(r.Current);
        }

        [Test]
        public void Runner_SkipsNodesWhoseConditionFails()
        {
            var r = Run(Graph(Node("a", "one", "b"), Node("b", "secret", "c", "flag:x"), Node("c", "three")), new FakeWorld());
            r.Advance();
            Assert.AreEqual("three", r.Current.Text);
        }

        [Test]
        public void Runner_RunsNodeEffectsWhenShown_InOrder()
        {
            var log = new List<string>();
            var r = Run(Graph(Node("a", "one", "b", null, "flag:1", "flag:2"), Node("b", "two", null, null, "flag:3")), new FakeWorld(), log);
            CollectionAssert.AreEqual(new[] { "flag:1", "flag:2" }, log);
            r.Advance();
            CollectionAssert.AreEqual(new[] { "flag:1", "flag:2", "flag:3" }, log);
        }

        [Test]
        public void Runner_HidesChoicesWhoseConditionFails_AndRunsTheChosenEffects()
        {
            var w = new FakeWorld();
            w.Vars["dread"] = 60;
            var log = new List<string>();
            var ask = new DialogueNode
            {
                Id = "a", Text = "q", Speaker = "tilda",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice { Text = "kind", Condition = "var:dread<50", Effects = { "friend:tilda,10" }, Next = "k" },
                    new DialogueChoice { Text = "curt", Effects = { "friend:tilda,-5" }, Next = "c" },
                },
            };
            var r = Run(Graph(ask, Node("k", "kind reply"), Node("c", "curt reply")), w, log);
            Assert.AreEqual(1, r.Current.Options.Count, "the kind choice is hidden at high dread");
            Assert.AreEqual("curt", r.Current.Options[0].Text);
            r.Choose(0);
            CollectionAssert.AreEqual(new[] { "friend:tilda,-5" }, log);
            Assert.AreEqual("curt reply", r.Current.Text);

            w.Vars["dread"] = 0;
            var r2 = Run(Graph(ask, Node("k", "kind reply"), Node("c", "curt reply")), w);
            Assert.AreEqual(2, r2.Current.Options.Count);
        }

        [Test]
        public void Runner_NodeWithNoTextOnlyRunsEffects()
        {
            var log = new List<string>();
            var r = Run(Graph(Node("a", "hello", "b"), Node("b", "", "c", null, "flag:quiet"), Node("c", "bye")), new FakeWorld(), log);
            r.Advance();
            Assert.AreEqual("bye", r.Current.Text);
            CollectionAssert.Contains(log, "flag:quiet");
        }

        [Test]
        public void Runner_SurvivesLoopsOfSkippedNodes()
        {
            var g = Graph(Node("a", "x", "a", "flag:never"));
            var r = Run(g, new FakeWorld());
            Assert.IsTrue(r.Finished, "a cycle of skipped nodes ends the conversation instead of hanging");
        }

        [Test]
        public void Runner_UnknownStartFinishesImmediately()
        {
            var r = Run(new DialogueGraph { Id = "x", Start = "missing" }, new FakeWorld());
            Assert.IsTrue(r.Finished);
        }

        [Test]
        public void Runner_ChoicesAreIndexedAgainstTheNodeNotTheVisibleList()
        {
            var w = new FakeWorld();
            var ask = new DialogueNode
            {
                Id = "a", Text = "q",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice { Text = "hidden", Condition = "flag:x", Next = "h" },
                    new DialogueChoice { Text = "shown", Next = "s" },
                },
            };
            var r = Run(Graph(ask, Node("h", "H"), Node("s", "S")), w);
            Assert.AreEqual(1, r.Current.Options[0].Index);
            r.Choose(0);
            Assert.AreEqual("S", r.Current.Text);
        }

        // ---- dialogue sets ---------------------------------------------------------------------------------------

        static DialogueSet Set(params (string cond, int priority, string dialogue)[] entries) => new DialogueSet
        {
            Id = "s",
            Entries = entries.Select(e => new DialogueSetEntry { Condition = e.cond, Priority = e.priority, Dialogue = e.dialogue }).ToList(),
        };

        [Test]
        public void Set_PicksTheHighestPriorityEntryThatHolds()
        {
            var w = new FakeWorld();
            var set = Set((null, 0, "chat"), ("hearts:tilda>=3", 2, "friend"), ("!flag:met", 100, "first"));
            Assert.AreEqual("first", set.Pick(w, 0));
            w.Flags.Add("met");
            Assert.AreEqual("chat", set.Pick(w, 0));
            w.Hearts_["tilda"] = 3;
            Assert.AreEqual("friend", set.Pick(w, 0));
        }

        [Test]
        public void Set_ChoosesFromThePoolDeterministically()
        {
            var w = new FakeWorld();
            var set = Set((null, 0, "a"), (null, 0, "b"), (null, 0, "c"));
            Assert.AreEqual(set.Pick(w, 7), set.Pick(w, 7));
            var seen = Enumerable.Range(0, 30).Select(i => set.Pick(w, i)).Distinct().ToList();
            Assert.AreEqual(3, seen.Count, "different days give different lines");
        }

        [Test]
        public void Set_WithNothingThatHolds_ReturnsNull()
        {
            Assert.IsNull(Set(("flag:x", 0, "a")).Pick(new FakeWorld(), 1));
            Assert.IsNull(Set().Pick(new FakeWorld(), 1));
        }

        // ---- loading JSON ----------------------------------------------------------------------------------------

        const string Json = @"{
          ""dialogues"": [ { ""id"": ""d1"", ""start"": ""n0"", ""nodes"": [
              { ""id"": ""n0"", ""speaker"": ""tilda"", ""text"": ""k.a"", ""next"": ""n1"" },
              { ""id"": ""n1"", ""text"": ""k.b"", ""choices"": [ { ""text"": ""k.c"", ""condition"": ""flag:x"", ""effects"": [""flag:y""], ""next"": ""n0"" } ] } ] } ],
          ""sets"": [ { ""id"": ""npc.tilda.talk"", ""entries"": [ { ""dialogue"": ""d1"", ""priority"": 1 } ] } ]
        }";

        [Test]
        public void StoryContent_LoadsDialoguesAndSets()
        {
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(Json, "test"));
            var d = story.Dialogue("d1");
            Assert.IsNotNull(d);
            Assert.AreEqual(2, d.Nodes.Count);
            Assert.AreEqual("flag:y", d.Node("n1").Choices[0].Effects[0]);
            Assert.AreEqual("d1", story.Set("npc.tilda.talk").Entries[0].Dialogue);
            CollectionAssert.IsEmpty(story.Errors);
        }

        [Test]
        public void StoryContent_RejectsDuplicateIds_AndBadJson()
        {
            var story = new StoryContent();
            story.AddJson(Json, "one");
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("redefines 'd1'"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("redefines 'npc.tilda.talk'"));
            story.AddJson(Json, "two");
            Assert.AreEqual(2, story.Errors.Count);
            Assert.AreEqual(1, story.Dialogues.Count());

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("not valid JSON"));
            Assert.IsFalse(story.AddJson("{ nope", "bad"));
        }

        [Test]
        public void StoryContent_EntriesWithoutAnIdAreReported()
        {
            var story = new StoryContent();
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("has no id"));
            story.AddJson(@"{ ""dialogues"": [ { ""start"": ""x"" } ] }", "noid");
            Assert.AreEqual(1, story.Errors.Count);
        }

        // ---- running through the session -------------------------------------------------------------------------

        [Test]
        public void Session_StoryText_FillsPlayerAndFarmNames()
        {
            using (var f = new TestSessionFixture())
            {
                L.SetTable(new Dictionary<string, string> { { "k", "Hello [player] of [farm]" } });
                Assert.AreEqual("Hello Dev of Dev Farm", f.Session.StoryText("k", new object[0]));
                L.SetLanguage("en");
            }
        }

        [Test]
        public void Session_BeginDialogue_WithoutUi_ReturnsFalse()
        {
            using (var f = new TestSessionFixture())
            {
                f.Session.Story.AddJson(Json, "t");
                Assert.IsFalse(f.Session.BeginDialogue("d1"), "no UI service is registered in an EditMode test");
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unknown dialogue"));
                Assert.IsFalse(f.Session.BeginDialogue("nope"));
            }
        }

        [Test]
        public void StateWorldQuery_ExposesStoryState()
        {
            using (var f = new TestSessionFixture(new[] { TestSessionFixture.Item("crop.parsnip") }))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 3);
                s.State.Npcs["tilda"] = new NpcState { Points = FriendshipModel.PointsPerHeart * 2 };
                s.State.Quests["q"] = new QuestProgress { Status = QuestStatus.Active };
                Assert.IsTrue(Conditions.Evaluate("has:crop.parsnip>=3 && hearts:tilda>=2 && quest:q=active", s.World));
                Assert.IsFalse(Conditions.Evaluate("hearts:tilda>=3", s.World));
            }
        }
    }
}
