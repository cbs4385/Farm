using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Editor;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-137: the narrative developer commands, run through the real console processor against the real story.
    public class NarrativeDebugTests
    {
        TestSessionFixture _fixture;
        GameSession _s;
        DebugCommandProcessor _cmd;

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _fixture = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null);
            _s = _fixture.Session;
            _s.Story = StoryContent.LoadFromResources();
            _s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
            _cmd = new DebugCommandProcessor(_s, name => true);
        }

        [TearDown]
        public void TearDown() => _fixture.Dispose();

        string Run(string line, bool expectOk = true)
        {
            var r = _cmd.Execute(line);
            Assert.AreEqual(expectOk, r.Ok, $"{line} -> {r.Message}");
            return r.Message;
        }

        [Test]
        public void Help_ListsTheNarrativeCommands()
        {
            var help = Run("help");
            foreach (var name in new[] { "hearts", "mood", "storyline", "choice", "heard", "pool", "pick", "say", "scene", "memory", "reactions", "fire", "topics", "social", "coverage" })
                StringAssert.Contains(name + " ", help, name);
        }

        [Test]
        public void TheReleaseGuard_KnowsTheNarrativeTools() =>
            CollectionAssert.Contains(ReleaseGuard.DebugMarkers, "NarrativeDebug");

        [Test]
        public void Hearts_SetsFriendship()
        {
            StringAssert.Contains("4 heart", Run("hearts wren 4"));
            Assert.AreEqual(4 * FriendshipModel.PointsPerHeart, _s.State.Npcs["wren"].Points);
            Assert.IsTrue(_s.State.Npcs["wren"].Met);
            Assert.IsTrue(Conditions.Evaluate("hearts:wren>=4", _s.World));
            Run("hearts wren 11", false);
            Run("hearts nobody 3", false);
            Run("hearts wren", false);
        }

        [Test]
        public void Mood_ShowsForcesAndClears()
        {
            StringAssert.Contains("wren:", Run("mood"));
            StringAssert.Contains("lonely (forced)", Run("mood wren lonely"));
            Assert.IsTrue(Conditions.Evaluate("mood:wren=lonely", _s.World));
            Run("mood wren clear");
            Assert.AreEqual(0, _s.GetVar("mood.wren.force"));
            Run("mood wren grumpy", false);
            Run("mood nobody lonely", false);
        }

        [Test]
        public void StorylineAndChoice_SetTheirFlags()
        {
            Run("storyline pie_feud");
            Assert.IsTrue(Conditions.Evaluate("storyline:pie_feud", _s.World));
            Run("storyline pie_feud off");
            Assert.IsFalse(Conditions.Evaluate("storyline:pie_feud", _s.World));
            Run("choice wren.bag.listen on");
            Assert.IsTrue(Conditions.Evaluate("choice:wren.bag.listen", _s.World));
            Run("choice x maybe", false);
            Run("storyline", false);
        }

        [Test]
        public void Pool_ExplainsEveryEntry_AndWhatWouldBeSaid()
        {
            Run("hearts wren 1");
            _s.SetFlag("met.wren");
            var text = Run("pool wren");
            StringAssert.Contains("entries eligible today", text);
            StringAssert.Contains("READY", text);
            StringAssert.Contains("wren.friend1", text);
            StringAssert.Contains("condition is false: hearts:wren>=3", text, "the friend lines need 3 hearts");
            StringAssert.Contains("condition is false: !flag:met.wren", text, "the introduction is over");
            StringAssert.Contains("Would say:", text);
            var rows = text.Split('\n').Count(l => l.StartsWith("[READY") || l.StartsWith("[no") || l.StartsWith("[wait"));
            Assert.AreEqual(_s.Story.Set("npc.wren.talk").Entries.Count, rows, "one row per entry");
        }

        [Test]
        public void Pool_ShowsALineThatWasJustSaid_AsWaiting()
        {
            Run("hearts wren 1");
            _s.SetFlag("met.wren");
            var memory = LineMemory.Load(_s);
            memory.Record("npc.wren.talk", "wren.chat2", _s.Clock.Now.TotalDays - 3, 0);
            LineMemory.Store(_s, memory);
            var line = Run("pool wren").Split('\n').First(l => l.Contains("wren.chat2"));
            StringAssert.Contains("[wait ]", line);
            StringAssert.Contains("11 day(s) until it can repeat", line);
        }

        [Test]
        public void Pick_IsADryRun_ThatNothingRecords_AndMatchesTheRealPick()
        {
            Run("hearts wren 1");
            _s.SetFlag("met.wren");
            Assert.IsNull(LineMemory.Load(_s).LastOf("npc.wren.talk"));
            var first = Run("pick wren");
            var second = Run("pick wren");
            Assert.AreEqual(first, second, "asking twice changes nothing");
            Assert.IsNull(LineMemory.Load(_s).LastOf("npc.wren.talk"), "nothing was recorded");

            var today = _s.Clock.Now.TotalDays;
            var memory = LineMemory.Load(_s);
            var set = _s.Story.Set("npc.wren.talk");
            var real = set.PickVaried(_s.World, today * 7919 + NpcInteractions.StableHash("wren"), memory, set.Id, today, Reactions.EntriesFor(ReactionState.Load(_s), "wren", today));
            StringAssert.Contains(real, first, "the dry run names the line the real talk would use");
            StringAssert.Contains("would say " + real, first);
            Run("pick wren 123");
            Run("pick wren notanumber", false);
        }

        [Test]
        public void Pick_ShowsAPendingReaction_AboveIdleChat()
        {
            _s.Story.AddJson(@"{ ""dialogues"": [ { ""id"": ""wren.react_dbg"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""dlg.wren.chat1.0"" } ] } ],
              ""reactions"": [ { ""id"": ""r.dbg"", ""on"": ""flag:go"", ""npcs"": ""wren"", ""dialogue"": ""wren.react_dbg"" } ] }", "t");
            _s.SetFlag("met.wren");
            StringAssert.Contains("1 reaction(s) queued", Run("fire flag:go"));
            StringAssert.Contains("wren.react_dbg", Run("reactions"));
            StringAssert.Contains("(reaction)", Run("pool wren"));
            StringAssert.Contains("would say wren.react_dbg", Run("pick wren"));
            Run("fire weather:rain", false);
            Run("fire", false);
        }

        [Test]
        public void HeardAndCoverage_FollowWhatWasSaid()
        {
            StringAssert.Contains("not said anything", Run("heard wren"));
            var memory = LineMemory.Load(_s);
            memory.Record("npc.wren.talk", "wren.chat1", 3, 0);
            memory.Record("npc.wren.talk", "wren.chat2", 5, 0);
            memory.Record("npc.wren.talk", "wren.chat2", 6, 0);
            LineMemory.Store(_s, memory);
            var heard = Run("heard wren").Split('\n');
            StringAssert.StartsWith("wren.chat2", heard[0], "newest first");
            StringAssert.Contains("x2", heard[0]);
            var coverage = Run("coverage wren");
            var total = _s.Story.Set("npc.wren.talk").Entries.Select(e => e.Dialogue).Distinct().Count();
            StringAssert.Contains($"2 of {total} talk lines heard", coverage);
            StringAssert.DoesNotContain("wren.chat1,", coverage.Split('\n')[1].Replace("wren.chat1", "wren.chat1,"), "heard lines are not listed as missing");
            Assert.IsFalse(coverage.Split('\n')[1].Contains("wren.chat2"));
        }

        [Test]
        public void Topics_SaysWhyEachIsOrIsNotOffered()
        {
            Effects.Run(_s, "topic.done:edmund.wren");   // the one-off question about Edmund would take a slot
            Run("hearts wren 3");
            var text = Run("topics wren");
            StringAssert.Contains("wren.gossip (p2): OFFERED", text);
            StringAssert.Contains("wren.stool (p3): OFFERED", text);
            Run("hearts wren 0");
            var low = Run("topics wren");
            StringAssert.Contains("condition is false: hearts:wren>=1", low.Split('\n').First(l => l.StartsWith("wren.gossip")));
            Effects.Run(_s, "topic.done:wren.stool");
            Run("hearts wren 3");
            StringAssert.Contains("asked already (once only)", Run("topics wren").Split('\n').First(l => l.StartsWith("wren.stool")));
            StringAssert.Contains("marcus.joinery", Run("topics marcus"));       // every villager has a topic now
        }

        [Test]
        public void Social_ShowsTheProfile_AndTodaysOutcomes()
        {
            var text = Run("social wren");
            StringAssert.Contains("loves [joke]", text);
            StringAssert.Contains("Actions today: 0 of 2", text);
            foreach (var action in SocialActions.All) StringAssert.Contains(action + ":", text);
            StringAssert.Contains("loves [advice]", Run("social marcus"));       // every villager has a profile now
            Effects.Run(_s, "social.done:wren,joke,great");
            StringAssert.Contains("Actions today: 1 of 2", Run("social wren"));
        }

        [Test]
        public void SayAndScene_RejectUnknownIds_AndQueueKnownOnes()
        {
            Run("say no.such.dialogue", false);
            Run("scene no_such_scene", false);
            StringAssert.Contains("Queued scene", Run("scene wren_heart2"));
            CollectionAssert.Contains(_s.PendingEvents, "wren_heart2");
            Run("memory no_such_scene", false);
            Run("memory", false);
        }
    }
}
