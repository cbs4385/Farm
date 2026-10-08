using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The wake reactions: every villager has a grumpy line and a warmer one for a friend, and a story can wake a sleeper with `wake:<npc>`.
    public class NpcWakeTests
    {
        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            Effects.ResetForTests();
        }

        [Test]
        public void EveryVillager_HasAGrumpyAndAWarmWakeReaction()
        {
            var story = StoryContent.LoadFromResources();
            foreach (var npc in NpcIds.All)
            {
                var grumpy = story.Reactions.FirstOrDefault(r => r.Id == "wake." + npc);
                var warm = story.Reactions.FirstOrDefault(r => r.Id == "wake." + npc + ".friend");
                Assert.IsNotNull(grumpy, npc + " grumpy");
                Assert.IsNotNull(warm, npc + " warm");
                Assert.AreEqual("wake:" + npc, grumpy.On);
                Assert.IsTrue(Reactions.IsKnownTrigger(grumpy.On));
                Assert.AreEqual(npc, grumpy.Npcs);
                Assert.AreEqual($"hearts:{npc}>={NpcHomes.FriendHearts}", warm.Condition, "the warm one is for a friend");
                Assert.Greater(warm.Priority, grumpy.Priority, "and it outranks the grumpy one");
                Assert.AreEqual(0, grumpy.CooldownDays, "every waking is answered");
                Assert.IsNotNull(story.Dialogue(grumpy.Dialogue), npc);
                Assert.IsNotNull(story.Dialogue(warm.Dialogue), npc);
            }
        }

        [Test]
        public void TheWakeEffect_IsKnown_AndNeedsOneArgument()
        {
            Assert.IsTrue(Effects.IsKnown("wake"));
            Assert.IsTrue(Effects.Validate("wake:tilda", out _));
            Assert.IsFalse(Effects.Validate("wake", out _));
        }

        [Test]
        public void TheWakeTrigger_QueuesTheReaction_ForThatVillagerOnly()
        {
            var story = StoryContent.LoadFromResources();
            var state = new ReactionState();
            var added = Reactions.Trigger(state, story.Reactions, new NpcSleepWorld(), "wake:tilda", 100);
            Assert.AreEqual(1, added, "the grumpy one (the warm one needs hearts, which this world has none of)");
            Assert.AreEqual("tilda", state.Pending.Single().Npc);
        }

        sealed class NpcSleepWorld : IWorldQuery, IGameQuery
        {
            public bool HasFlag(string flag) => false;
            public int GetVar(string name) => 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 3, 23 * 60);
            public string Weather => "sunny";
            public string MapId => "HomeTilda";
            public int Hearts(string npcId) => 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }
    }
}
