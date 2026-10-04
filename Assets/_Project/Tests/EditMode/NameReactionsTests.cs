using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-148: villagers react to well-known farm and player names. Each reaction is a talk-set entry in the reaction band, conditioned on
    // farmname:<slug> or playername:<slug>, and plays once.
    public class NameReactionsTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public string Farm = "Plain Acres", Player = "Alex";
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
            public string FarmName() => Farm;
            public string PlayerName() => Player;
        }

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            NameFilter.ResetForTests();
            _story = StoryContent.LoadFromResources();
        }

        IEnumerable<(string npc, DialogueSetEntry entry, string kind, string slug)> NameEntries() =>
            _story.Sets.Where(s => s.Id.StartsWith("npc.") && s.Id.EndsWith(".talk"))
                .SelectMany(s => s.Entries.Where(e => e.Condition != null && (e.Condition.StartsWith("farmname:") || e.Condition.StartsWith("playername:")))
                    .Select(e => (s.Id.Split('.')[1], e, e.Condition.StartsWith("farmname:") ? "farm" : "player", e.Condition.Substring(e.Condition.IndexOf(':') + 1))));

        [Test]
        public void ThereAreAtLeastSixtyReactions_EachInTheReactionBand_PlayingOnce()
        {
            var all = NameEntries().ToList();
            Assert.GreaterOrEqual(all.Count, 60);
            foreach (var (npc, e, kind, slug) in all)
            {
                Assert.AreEqual(3, e.Priority, e.Dialogue);
                Assert.GreaterOrEqual(e.Cooldown, 1000, e.Dialogue + " plays once");
                Assert.IsNotNull(_story.Dialogue(e.Dialogue), e.Dialogue);
            }
        }

        [Test]
        public void EachName_IsHandledByOneVillagerOnly_AndTheSlugIsPlain()
        {
            var all = NameEntries().ToList();
            foreach (var g in all.GroupBy(x => (x.kind, x.slug))) Assert.AreEqual(1, g.Count(), $"{g.Key.kind}:{g.Key.slug} has one reaction");
            foreach (var x in all) StringAssert.IsMatch("^[a-z0-9_]+$", x.slug);
        }

        [Test]
        public void TheNamedFarm_IsReactedTo_ByItsVillager_AtFirstTalk()
        {
            foreach (var (npc, e, kind, slug) in NameEntries())
            {
                var w = new World();
                w.Flags.Add("met." + npc);
                if (kind == "farm") w.Farm = slug.Replace('_', ' '); else w.Player = slug.Replace('_', ' ');
                var set = _story.Set($"npc.{npc}.talk");
                var pick = set.PickVaried(w, 4242, new LineMemory(), set.Id, 10);
                Assert.AreEqual(e.Dialogue, pick, $"{npc} on {kind} '{slug}'");
            }
        }

        [Test]
        public void TheReaction_PlaysOnce_ThenTheOrdinaryPoolTakesOver()
        {
            var w = new World { Farm = "Farmy McFarmface" };
            w.Flags.Add("met.wren");
            var set = _story.Set("npc.wren.talk");
            var memory = new LineMemory();
            Assert.AreEqual("wren.namefarm.farmy_mcfarmface", set.PickVaried(w, 1, memory, set.Id, 10));
            Assert.AreNotEqual("wren.namefarm.farmy_mcfarmface", set.PickVaried(w, 2, memory, set.Id, 11));
            Assert.AreNotEqual("wren.namefarm.farmy_mcfarmface", set.PickVaried(w, 3, memory, set.Id, 120), "not even months later");
        }

        [Test]
        public void AnOrdinaryName_GetsNoSpecialLine()
        {
            var w = new World { Farm = "Quiet Hollow Acres", Player = "Marguerite" };
            w.Flags.Add("met.tilda");
            var set = _story.Set("npc.tilda.talk");
            var pick = set.PickVaried(w, 7, new LineMemory(), set.Id, 10);
            StringAssert.DoesNotContain("name", pick);
        }

        [Test]
        public void NoReactedName_IsOneThatTheNameFilterRefuses()
        {
            foreach (var (npc, e, kind, slug) in NameEntries()) Assert.IsTrue(NameFilter.IsAllowed(slug.Replace('_', ' ')), $"{kind} '{slug}'");
        }

        [Test]
        public void ThePlayerNameAtom_IsCaseInsensitive_AndNeedsTheWholeName()
        {
            var w = new World { Player = "BOB" };
            Assert.IsTrue(Conditions.TryEvaluate("playername:bob", w, out var yes) && yes);
            w.Player = "Bobby";
            Assert.IsTrue(Conditions.TryEvaluate("playername:bob", w, out var no) && !no);
        }
    }
}
