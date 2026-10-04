using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;

namespace Farm.Tests
{
    // T-106 and T-144: relationship stages, the Neighbours page's data and the Gossip Book's data.
    public class NeighbourJournalTests
    {
        StoryContent _story;
        NpcDefinition _wren;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
            _wren = AssetDatabase.LoadAssetAtPath<NpcDefinition>("Assets/_Project/Data/Npcs/wren.asset");
        }

        static GameState State(int points, bool met = true)
        {
            var s = new GameState();
            s.Npcs["wren"] = new NpcState { Met = met, Points = points };
            return s;
        }

        [Test]
        public void TheStages_LineUpWithTheTalkTiers()
        {
            Assert.AreEqual(RelationshipStage.Acquaintance, RelationshipStages.For(0));
            Assert.AreEqual(RelationshipStage.Acquaintance, RelationshipStages.For(1));
            Assert.AreEqual(RelationshipStage.Neighbour, RelationshipStages.For(2));
            Assert.AreEqual(RelationshipStage.Friend, RelationshipStages.For(3));
            Assert.AreEqual(RelationshipStage.Friend, RelationshipStages.For(5));
            Assert.AreEqual(RelationshipStage.CloseFriend, RelationshipStages.For(6));
            Assert.AreEqual(RelationshipStage.CloseFriend, RelationshipStages.For(8));
            Assert.AreEqual(RelationshipStage.Confidant, RelationshipStages.For(9));
            Assert.AreEqual(RelationshipStage.Confidant, RelationshipStages.For(10));
        }

        [Test]
        public void EveryStage_HasAName()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (RelationshipStage stage in System.Enum.GetValues(typeof(RelationshipStage))) Assert.IsTrue(en.ContainsKey(RelationshipStages.Key(stage)), stage.ToString());
        }

        [Test]
        public void AnUnmetVillager_ShowsNothing()
        {
            var info = NeighbourJournal.Build(_wren, State(1000, met: false), _story, new InteractionState());
            Assert.IsFalse(info.Met);
            Assert.IsEmpty(info.Loves);
            Assert.IsNull(info.Hint);
        }

        [Test]
        public void Tastes_AreShownOnlyOnceTheExactItemWasGiven()
        {
            var inter = new InteractionState();
            var none = NeighbourJournal.Build(_wren, State(500), _story, inter);
            Assert.IsEmpty(none.Loves); Assert.IsEmpty(none.Likes); Assert.IsEmpty(none.Dislikes);

            inter.GiftDay["wren|" + _wren.Loved[0]] = 3;
            inter.GiftDay["wren|" + _wren.Disliked[0]] = 4;
            inter.GiftDay["wren|crop.potato"] = 5;                      // not one of her listed tastes
            var some = NeighbourJournal.Build(_wren, State(500), _story, inter);
            CollectionAssert.AreEqual(new[] { _wren.Loved[0] }, some.Loves);
            CollectionAssert.AreEqual(new[] { _wren.Disliked[0] }, some.Dislikes);
            Assert.IsEmpty(some.Likes);
        }

        [Test]
        public void Topics_AreNotedOnceAsked()
        {
            var inter = new InteractionState();
            Assert.IsEmpty(NeighbourJournal.Build(_wren, State(500), _story, inter).Told);
            inter.TopicTimes["wren.gossip"] = 1;
            var info = NeighbourJournal.Build(_wren, State(500), _story, inter);
            Assert.AreEqual(1, info.Told.Count);
            StringAssert.Contains("topic", info.Told[0] + "topic");
        }

        [Test]
        public void TheHint_NamesTheHeartsNeeded_ThenSaysSomethingIsWaiting_ThenDone()
        {
            var inter = new InteractionState();
            var low = NeighbourJournal.Build(_wren, State(0), _story, inter);
            Assert.AreEqual("hint.at", low.Hint);
            Assert.AreEqual(2, low.HintHearts, "the first scene is at two hearts");

            var ready = NeighbourJournal.Build(_wren, State(2 * FriendshipModel.PointsPerHeart), _story, inter);
            Assert.AreEqual("hint.ready", ready.Hint);

            var state = State(10 * FriendshipModel.PointsPerHeart);
            foreach (var n in new[] { 2, 4, 5, 6, 8, 10 }) state.EventsSeen.Add($"wren_heart{n}");
            Assert.AreEqual("hint.done", NeighbourJournal.Build(_wren, state, _story, inter).Hint);
        }

        [Test]
        public void AnOpenQuestFromTheVillager_IsListed()
        {
            var state = State(500);
            Assert.IsNull(NeighbourJournal.Build(_wren, state, _story, new InteractionState()).QuestTitleKey);
            state.Quests["wren_stew"] = new QuestProgress { Status = "active" };
            Assert.AreEqual("quest.wren_stew.title", NeighbourJournal.Build(_wren, state, _story, new InteractionState()).QuestTitleKey);
        }

        // ---- the Gossip Book ----

        [Test]
        public void TheGossipBook_CountsOnlyRareLinesHeard()
        {
            var villagers = new[] { "wren", "hazel", "bram" };
            var memory = new LineMemory();
            var none = GossipBook.Build(villagers, _story, memory, new HashSet<string>());
            Assert.AreEqual(0, none.FoundTotal);
            Assert.AreEqual(3, none.Villagers.Count);
            Assert.GreaterOrEqual(none.RareTotal, 12, "three rare lines and a legendary each");

            var set = _story.Set("npc.wren.talk");
            var rare = set.Entries.First(GossipBook.IsRare).Dialogue;
            memory.Record(set.Id, rare, 10, 0);
            memory.Record(set.Id, set.Entries.First(e => !GossipBook.IsRare(e)).Dialogue, 11, 0);     // an ordinary line does not count
            var some = GossipBook.Build(villagers, _story, memory, new HashSet<string>());
            Assert.AreEqual(1, some.FoundTotal);
            CollectionAssert.Contains(some.Villagers.First(v => v.Npc == "wren").Found, rare);
        }

        [Test]
        public void TheGossipBook_ListsSolvedStorylines_OnlyForThisGame()
        {
            var flags = new HashSet<string> { "storyline.pie_feud", "storyline.lost_umbrella", "storydone.pie_feud", "storydone.five_names_cat" };
            var data = GossipBook.Build(new string[0], _story, null, flags);
            Assert.AreEqual(2, data.StorylinesThisGame, "only the drawn ones count");
            Assert.AreEqual(1, data.StorylinesSolved, "a finished story that was not drawn does not count");
            CollectionAssert.Contains(data.SolvedTitleKeys, "storyline.pie_feud");
        }
    }
}
