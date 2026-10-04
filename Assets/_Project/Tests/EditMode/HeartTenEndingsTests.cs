using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The closing line of each heart-10 scene depends on an earlier choice. Every branch must resolve to exactly one clean line.
    public class HeartTenEndingsTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 5, 14 * 60);
            public string Weather => "sunny";
            public string MapId => "Saloon";
            public int Hearts(string npcId) => 10;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        Dictionary<string, string> _en;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _en = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
        }

        string Ending(string eventId, string part, params string[] choices)
        {
            var w = new World();
            foreach (var c in choices) w.Flags.Add("choice." + c);
            var line = RichText.Process(_en[$"event.{eventId}.{part}"], w, 1);
            Assert.IsFalse(line.Text.Contains("{") || line.Text.Contains("}"), $"{eventId}: markup left over in '{line.Text}'");
            Assert.IsFalse(string.IsNullOrWhiteSpace(line.Text), eventId + " is empty");
            return line.Text;
        }

        [Test]
        public void Wren_OpeningNight_HasThreeDistinctEndings()
        {
            var go = Ending("wren_heart10", "outro", "wren.bag.go");
            var stay = Ending("wren_heart10", "outro", "wren.bag.stay");
            var neither = Ending("wren_heart10", "outro");
            StringAssert.Contains("map", go);
            StringAssert.Contains("stay", stay);
            StringAssert.Contains("quiet part", neither);
            Assert.AreEqual(3, new HashSet<string> { go, stay, neither }.Count);
        }

        [Test]
        public void Hazel_TheReading_HasTwoDistinctEndings()
        {
            var critique = Ending("hazel_heart10", "outro", "hazel.rehearsal.critique");
            var other = Ending("hazel_heart10", "outro", "hazel.rehearsal.praise");
            StringAssert.Contains("cut the middle", critique);
            StringAssert.Contains("kept the lamp", other);
        }

        [Test]
        public void Bram_YourTool_HasTwoDistinctEndings_AndNoExclamationMarks()
        {
            var ask = Ending("bram_heart10", "thanks", "bram.juno.ask");
            var other = Ending("bram_heart10", "thanks", "bram.juno.tease");
            StringAssert.Contains("see you", ask);
            StringAssert.Contains("not much", other);
            Assert.IsFalse(ask.Contains("!") || other.Contains("!"));
        }

        [Test]
        public void HazelAndBram_EndingsHaveNoExclamationMarks()
        {
            Assert.IsFalse(Ending("hazel_heart10", "outro", "hazel.rehearsal.critique").Contains("!"));
            Assert.IsFalse(Ending("hazel_heart10", "outro").Contains("!"));
        }

        [Test]
        public void StorylineScenes_EveryBranchResolvesToOneCleanClosingLine()
        {
            // eventId -> the choice flags that select each closing line; the last entry (none) is the fallback.
            var cases = new (string id, string[] branches)[]
            {
                ("story_pie_payoff", new[] { "pie.honest", "pie.improve", "pie.eat" }),
                ("story_umbrella", new[] { "umbrella.return", "umbrella.hang", "umbrella.keep" }),
                ("story_scarecrows", new[] { "scarecrow.tilda", "scarecrow.dorian", "scarecrow.tie", "scarecrow.crows" }),
            };
            foreach (var (id, branches) in cases)
            {
                var lines = branches.Select(b => Ending(id, "after", b)).ToList();
                Assert.AreEqual(branches.Length, new HashSet<string>(lines).Count, id + ": every choice has its own closing line");
            }
        }

        [Test]
        public void StoryScenes_KeepVoiceRules_ForBramAndNoUnresolvedMarkup()
        {
            foreach (var key in _en.Keys.Where(k => k.StartsWith("event.story_whistler_bram.")))
                Assert.IsFalse(_en[key].Contains("!"), key);
            foreach (var key in _en.Keys.Where(k => k.StartsWith("event.story_")))
            {
                var line = RichText.Process(_en[key], new World(), 1).Text;
                Assert.IsFalse(line.Contains("{") || line.Contains("}"), key + ": " + line);
            }
        }
    }
}
