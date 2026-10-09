using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The content of Elara's visit and her herb garden quest: when the scene can play, what she asks for, and that it is only a clue (not a different quest) with the
    // horror off.
    public class ElaraGardenContentTests
    {
        sealed class World : IWorldQuery
        {
            public GameDateTime Time;
            public bool HasFlag(string flag) => false;
            public int GetVar(string name) => 0;
            public GameDateTime Now => Time;
            public string Weather => "sunny";
            public string MapId => MapIds.Farm;
        }

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            _story = StoryContent.LoadFromResources();
        }

        [TearDown]
        public void TearDown() => Conditions.ClearCustomForTests();

        [Test]
        public void TheVisit_IsOnTheFarm_OnTheSecondMorningOfTheFirstSpring_AndPlaysOnce()
        {
            var visit = _story.Event("elara_garden_visit");
            Assert.IsNotNull(visit);
            Assert.AreEqual("map", visit.Trigger);
            Assert.AreEqual(MapIds.Farm, visit.Map);
            Assert.IsTrue(visit.Once);
            Assert.IsTrue(visit.Recheck, "she arrives when the clock reaches eight, even if the player is already out on the farm");

            bool Plays(int year, Season season, int day, int minute) =>
                Conditions.TryEvaluate(visit.Condition, new World { Time = new GameDateTime(year, season, day, minute) }, out var ok) && ok;
            Assert.IsFalse(Plays(1, Season.Spring, 2, 7 * 60 + 50), "not before eight");
            Assert.IsTrue(Plays(1, Season.Spring, 2, 8 * 60), "at eight");
            Assert.IsTrue(Plays(1, Season.Spring, 2, 15 * 60), "any time that day she can still be met");
            Assert.IsFalse(Plays(1, Season.Spring, 2, 18 * 60), "not in the evening");
            Assert.IsFalse(Plays(1, Season.Spring, 1, 9 * 60), "not the day the player arrives");
            Assert.IsFalse(Plays(1, Season.Spring, 3, 9 * 60), "and not later");
            Assert.IsFalse(Plays(2, Season.Spring, 2, 9 * 60), "not in the second year");
            Assert.IsFalse(Plays(1, Season.Summer, 2, 9 * 60), "not in the summer");
        }

        [Test]
        public void TheQuest_AsksForThreeDandelionsAndThreeWildGarlic_AndPaysForThem()
        {
            var quest = _story.Quest("elara_garden");
            Assert.IsNotNull(quest);
            Assert.AreEqual("elara", quest.Giver);
            Assert.IsFalse(quest.AutoStart, "the visit offers it");
            Assert.AreEqual(2, quest.Objectives.Count);
            var take = quest.Objectives.ToDictionary(o => o.TakeItem, o => o.TakeCount);
            Assert.AreEqual(3, take["forage.dandelion"]);
            Assert.AreEqual(3, take["forage.wildgarlic"]);
            Assert.IsTrue(quest.Rewards.Any(r => r.StartsWith("gold:")));
            Assert.IsTrue(quest.Rewards.Contains("friend:elara,100"));
            Assert.AreEqual(Farm.Mythos.MythosIds.Intro.Item, "forage.wildgarlic", "the ritual's fixed offering is one of the plants she asks for");
            Assert.AreEqual(Farm.Mythos.MythosIds.Intro.Quest, quest.Id);
        }

        [Test]
        public void ElarasTalk_OffersTheQuestAgain_Reminds_AndTurnsItIn()
        {
            var set = _story.Set("npc.elara.talk");
            Assert.IsNotNull(set);
            foreach (var id in new[] { "elara.garden.ask", "elara.garden.remind", "elara.garden.turnin" })
                Assert.IsTrue(set.Entries.Any(e => e.Dialogue == id), id);
            foreach (var id in new[] { "elara.garden.visit", "elara.garden.ask", "elara.garden.remind", "elara.garden.turnin" })
                Assert.IsNotNull(_story.Dialogue(id), id);
        }

        [Test]
        public void TildasHint_ComesOnlyWithTheHorrorOn()
        {
            Farm.Mythos.MythosContent.Load(_story);                       // the hint is the horror layer's own data
            Assert.IsFalse(StoryContent.LoadFromResources().Reactions.Any(r => r.Id == "elara_garden.tilda"), "not in the core story data");
            var reaction = _story.Reactions.First(r => r.Id == "elara_garden.tilda");
            Assert.AreEqual("quest.done:elara_garden", reaction.On);
            Assert.AreEqual("tilda", reaction.Npcs);
            Assert.AreEqual("horror:1", reaction.Condition);
            Assert.IsTrue(Reactions.IsKnownTrigger(reaction.On));
        }
    }
}
