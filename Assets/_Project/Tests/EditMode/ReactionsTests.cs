using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-091: the reaction queue (villagers reacting to what the player did).
    public class ReactionsTests
    {
        sealed class World : IWorldQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Village";
        }

        static ReactionDefinition Def(string id, string on = "flag:go", string npcs = "wren", string condition = null, int ttl = 3, int cooldown = 28, bool once = false) =>
            new ReactionDefinition { Id = id, On = on, Npcs = npcs, Condition = condition, TtlDays = ttl, CooldownDays = cooldown, Once = once, Dialogue = id + ".d" };

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        [Test]
        public void ATrigger_QueuesForEveryListedVillager()
        {
            var state = new ReactionState();
            var added = Reactions.Trigger(state, new[] { Def("r", npcs: "wren, tilda") }, new World(), "flag:go", 10);
            Assert.AreEqual(2, added);
            CollectionAssert.AreEquivalent(new[] { "wren", "tilda" }, state.Pending.Select(p => p.Npc));
            Assert.AreEqual(12, state.Pending[0].ExpiresDay, "three days: today and the next two");
        }

        [Test]
        public void OnlyMatchingTriggers_AndTrueConditions_Queue()
        {
            var state = new ReactionState();
            var w = new World();
            Assert.AreEqual(0, Reactions.Trigger(state, new[] { Def("r") }, w, "flag:other", 1));
            Assert.AreEqual(0, Reactions.Trigger(state, new[] { Def("r", condition: "flag:ready") }, w, "flag:go", 1));
            w.Flags.Add("ready");
            Assert.AreEqual(1, Reactions.Trigger(state, new[] { Def("r", condition: "flag:ready") }, w, "flag:go", 1));
        }

        [Test]
        public void AReactionIsNotQueuedTwice_WhilePending_OrWithinItsCooldown()
        {
            var state = new ReactionState();
            var defs = new[] { Def("r", cooldown: 10) };
            Assert.AreEqual(1, Reactions.Trigger(state, defs, new World(), "flag:go", 1));
            Assert.AreEqual(0, Reactions.Trigger(state, defs, new World(), "flag:go", 1), "still pending");
            state.Pending.Clear();
            Assert.AreEqual(0, Reactions.Trigger(state, defs, new World(), "flag:go", 5), "inside the cooldown");
            Assert.AreEqual(1, Reactions.Trigger(state, defs, new World(), "flag:go", 11), "after the cooldown");
        }

        [Test]
        public void OnceReactions_NeverReturn()
        {
            var state = new ReactionState();
            var defs = new[] { Def("r", once: true, cooldown: 0) };
            Assert.AreEqual(1, Reactions.Trigger(state, defs, new World(), "flag:go", 1));
            state.Pending.Clear();
            Assert.AreEqual(0, Reactions.Trigger(state, defs, new World(), "flag:go", 500));
        }

        [Test]
        public void Entries_AreForTheRightVillager_UntilTheyExpire()
        {
            var state = new ReactionState();
            Reactions.Trigger(state, new[] { Def("r", ttl: 2) }, new World(), "flag:go", 10);
            Assert.AreEqual(1, Reactions.EntriesFor(state, "wren", 10).Count);
            Assert.AreEqual(1, Reactions.EntriesFor(state, "wren", 11).Count);
            Assert.AreEqual(0, Reactions.EntriesFor(state, "wren", 12).Count, "expired");
            Assert.AreEqual(0, Reactions.EntriesFor(state, "tilda", 10).Count);
            var entry = Reactions.EntriesFor(state, "wren", 10)[0];
            Assert.AreEqual("r.d", entry.Dialogue);
            Assert.AreEqual(0, entry.Cooldown, "a reaction is never on cooldown");
        }

        [Test]
        public void ASpentReaction_StaysForTheDay_ThenGoes()
        {
            var state = new ReactionState();
            Reactions.Trigger(state, new[] { Def("r", ttl: 5) }, new World(), "flag:go", 10);
            Reactions.MarkConsumed(state, "wren", "r.d", 10);
            Assert.AreEqual(1, Reactions.EntriesFor(state, "wren", 10).Count, "talking again today repeats it");
            Assert.AreEqual(0, Reactions.EntriesFor(state, "wren", 11).Count, "but not tomorrow");
            Assert.AreEqual(1, Reactions.Prune(state, 11));
            Assert.IsEmpty(state.Pending);
        }

        [Test]
        public void Prune_DropsTheExpired()
        {
            var state = new ReactionState();
            Reactions.Trigger(state, new[] { Def("a", ttl: 1), Def("b", ttl: 4) }, new World(), "flag:go", 10);
            Assert.AreEqual(1, Reactions.Prune(state, 11));
            Assert.AreEqual("b", state.Pending.Single().Id);
        }

        [TestCase("flag:x", true)]
        [TestCase("quest.done:q", true)]
        [TestCase("quest.start:q", true)]
        [TestCase("skill.up:farming", true)]
        [TestCase("event:e", true)]
        [TestCase("season:fall", true)]
        [TestCase("gift:wren", true)]
        [TestCase("random:r", true)]
        [TestCase("day", true)]
        [TestCase("flag:", false)]
        [TestCase("dayy", false)]
        [TestCase("weather:rain", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void TriggerNames_AreChecked(string on, bool known) => Assert.AreEqual(known, Reactions.IsKnownTrigger(on));

        // ---- in a game ----

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        const string ReactionJson = @"{
          ""dialogues"": [
            { ""id"": ""wren.react1"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""dlg.test.react1"" } ] },
            { ""id"": ""wren.react2"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""dlg.test.react2"" } ] }
          ],
          ""reactions"": [
            { ""id"": ""r.flag"", ""on"": ""flag:first_harvest"", ""npcs"": ""wren"", ""dialogue"": ""wren.react1"" },
            { ""id"": ""r.quest"", ""on"": ""quest.done:q_test"", ""npcs"": ""wren"", ""priority"": 2, ""dialogue"": ""wren.react2"" },
            { ""id"": ""r.day"", ""on"": ""day"", ""npcs"": ""wren"", ""condition"": ""farm:crops>=0 && flag:morning_marker"", ""dialogue"": ""wren.react2"" }
          ]
        }";

        TestSessionFixture Fixture()
        {
            var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null);
            f.Session.Story = StoryContent.LoadFromResources();
            Assert.IsTrue(f.Session.Story.AddJson(ReactionJson, "test"));
            f.Session.SetFlag("met.wren");
            return f;
        }

        static string LastSaid(GameSession s) => LineMemory.Load(s).LastOf("npc.wren.talk")?.Dialogue;

        [Test]
        public void AFlag_QueuesAReaction_ThatBeatsIdleChat_RepeatsToday_AndIsGoneTomorrow()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                var wren = s.Npcs.Get("wren");
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                NpcInteractions.Talk(s, wren);
                Assert.AreNotEqual("wren.react1", LastSaid(s), "nothing has happened yet");

                s.SetFlag("first_harvest");                         // the bus carries FlagChanged to the reaction queue
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 7, 12 * 60));
                NpcInteractions.Talk(s, wren);
                Assert.AreEqual("wren.react1", LastSaid(s), "the reaction beats idle chat");
                NpcInteractions.Talk(s, wren);
                Assert.AreEqual("wren.react1", LastSaid(s), "the same line when talked to twice");

                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 8, 12 * 60));
                NpcInteractions.Talk(s, wren);
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                Assert.AreNotEqual("wren.react1", LastSaid(s), "spent: it is said once");
            }
        }

        [Test]
        public void AReaction_ExpiresIfThePlayerNeverComes()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                s.SetFlag("first_harvest");
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 12, 12 * 60));      // past the 3-day window
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                NpcInteractions.Talk(s, s.Npcs.Get("wren"));
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                Assert.AreNotEqual("wren.react1", LastSaid(s));
            }
        }

        [Test]
        public void OtherGameEvents_QueueReactions_AndStoryBeatsOutrankLowReactions()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                f.Bus.Publish(new QuestCompleted("q_test"));
                Assert.AreEqual(1, ReactionState.Load(s).Pending.Count(p => p.Id == "r.quest"));
                f.Bus.Publish(new QuestCompleted("q_other"));
                Assert.AreEqual(1, ReactionState.Load(s).Pending.Count, "an unrelated quest queues nothing");
            }
        }

        [Test]
        public void TheMorningTrigger_QueuesOnceWhileItsConditionHolds()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                Reactions.NewDay(s);
                Assert.AreEqual(0, ReactionState.Load(s).Pending.Count, "the condition does not hold yet");
                s.SetFlag("morning_marker");
                Reactions.NewDay(s);
                Reactions.NewDay(s);
                Assert.AreEqual(1, ReactionState.Load(s).Pending.Count(p => p.Id == "r.day"));
            }
        }

        [Test]
        public void TheValidator_ChecksReactions()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            BusinessHoursRegistry.RegisterConditionAtom();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var story = StoryContent.LoadFromResources();
            story.AddJson(@"{ ""reactions"": [
              { ""id"": ""bad1"", ""on"": ""weather:rain"", ""npcs"": ""nobody"", ""dialogue"": ""missing"", ""ttlDays"": 0, ""priority"": 9, ""cooldownDays"": -1, ""condition"": ""wat:1"" },
              { ""id"": ""bad2"", ""on"": ""day"", ""npcs"": """", ""dialogue"": ""tilda.chat1"" } ] }", "test");
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = story, Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey,
                RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            }).Where(p => p.Contains("reaction")).ToList();
            foreach (var needle in new[] { "unknown dialogue", "unknown villager", "unknown trigger", "ttlDays", "priority must be", "cooldownDays", "at least one villager" })
                Assert.IsTrue(problems.Any(p => p.Contains(needle)), $"expected a problem about '{needle}': {string.Join(" | ", problems)}");
        }
    }
}
