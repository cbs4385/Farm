using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-093: villager mood (pure, deterministic, nothing saved) and the mood: condition.
    public class MoodModelTests
    {
        static MoodInputs In(int today = 10, string weather = "sunny", int hearts = 0, int sinceContact = 0) =>
            new MoodInputs { NpcId = "wren", Today = today, Weather = weather, Hearts = hearts, DaysSinceContact = sinceContact };

        [Test]
        public void Birthdays_Gifts_AndFestivals_MakeThemDelighted()
        {
            var a = In(); a.BirthdayToday = true;
            var b = In(); b.GiftedToday = true;
            var c = In(); c.FestivalToday = true;
            Assert.AreEqual(Mood.Delighted, MoodModel.Compute(a));
            Assert.AreEqual(Mood.Delighted, MoodModel.Compute(b));
            Assert.AreEqual(Mood.Delighted, MoodModel.Compute(c));
        }

        [Test]
        public void NeglectMakesThemLonely_ButNeverMeetingDoesNot()
        {
            Assert.AreEqual(Mood.Lonely, MoodModel.Compute(In(sinceContact: MoodModel.LonelyAfterDays)));
            Assert.AreNotEqual(Mood.Lonely, MoodModel.Compute(In(sinceContact: MoodModel.LonelyAfterDays - 1)));
            Assert.AreNotEqual(Mood.Lonely, MoodModel.Compute(In(sinceContact: -1)), "a stranger is not lonely for you");
        }

        [Test]
        public void ForcedMood_WinsOverEverything()
        {
            var i = In(sinceContact: 10); i.BirthdayToday = true; i.Forced = (int)Mood.Worried + 1;
            Assert.AreEqual(Mood.Worried, MoodModel.Compute(i));
            i.Forced = 99;
            Assert.AreEqual(Mood.Delighted, MoodModel.Compute(i), "an out-of-range value is ignored");
        }

        [Test]
        public void TheMood_IsTheSameAllDay_AndVariesAcrossDays()
        {
            var seen = new HashSet<Mood>();
            for (var day = 0; day < 300; day++)
            {
                var m = MoodModel.Compute(In(day, "rain", 5));
                Assert.AreEqual(m, MoodModel.Compute(In(day, "rain", 5)));
                seen.Add(m);
            }
            Assert.GreaterOrEqual(seen.Count, 4, "content, tired, worried and mischievous all happen: " + string.Join(",", seen));
        }

        [Test]
        public void Rain_SometimesTires_ButSunNever()
        {
            var tiredRain = Enumerable.Range(0, 1000).Count(d => MoodModel.Compute(In(d, "rain")) == Mood.Tired);
            var tiredSun = Enumerable.Range(0, 1000).Count(d => MoodModel.Compute(In(d, "sunny")) == Mood.Tired);
            Assert.Greater(tiredRain, 250);
            Assert.Less(tiredRain, 450);
            Assert.AreEqual(0, tiredSun);
        }

        [Test]
        public void Mischief_NeedsFriendship()
        {
            Assert.AreEqual(0, Enumerable.Range(0, 1000).Count(d => MoodModel.Compute(In(d, "sunny", 0)) == Mood.Mischievous));
            var friends = Enumerable.Range(0, 1000).Count(d => MoodModel.Compute(In(d, "sunny", 4)) == Mood.Mischievous);
            Assert.Greater(friends, 60);
            Assert.Less(friends, 200);
        }

        [Test]
        public void MostDays_AreContent() =>
            Assert.Greater(Enumerable.Range(0, 1000).Count(d => MoodModel.Compute(In(d, "sunny", 4)) == Mood.Content), 750);

        [TestCase("content", true)]
        [TestCase("Lonely", true)]
        [TestCase("mischievous", true)]
        [TestCase("grumpy", false)]
        [TestCase("", false)]
        public void MoodNames_Parse(string name, bool ok) => Assert.AreEqual(ok, MoodModel.TryParse(name, out _));

        // ---- the condition ----

        sealed class World : IWorldQuery, IGameQuery
        {
            public string Mood = "content";
            public bool HasFlag(string flag) => false;
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
            public string MoodOf(string npcId) => npcId == "wren" ? Mood : "content";
        }

        [Test]
        public void TheMoodAtom_MatchesTheVillagersMood()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var w = new World { Mood = "lonely" };
            Assert.IsTrue(Conditions.Evaluate("mood:wren=lonely", w));
            Assert.IsTrue(Conditions.Evaluate("mood:wren=LONELY", w));
            Assert.IsFalse(Conditions.Evaluate("mood:wren=content", w));
            Assert.IsTrue(Conditions.Evaluate("mood:bram=content && !mood:wren=content", w));
            Assert.IsFalse(Conditions.TryEvaluate("mood:wren=grumpy", w, out var ok) && ok, "an unknown mood never matches");
            Assert.IsFalse(Conditions.TryEvaluate("mood:wren", w, out ok) && ok);
        }

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void InAGame_TheMoodFollowsContactGiftsAndTheForceVariable()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 12, 600));
                var state = NpcInteractions.StateOf(s.State, "wren");
                state.Met = true;
                state.LastContactDay = s.Clock.Now.TotalDays - 6;
                Assert.IsTrue(Conditions.Evaluate("mood:wren=lonely", s.World), "six days without a visit");
                state.LastContactDay = s.Clock.Now.TotalDays;
                Assert.IsFalse(Conditions.Evaluate("mood:wren=lonely", s.World));
                state.GiftsToday = 1;
                Assert.IsTrue(Conditions.Evaluate("mood:wren=delighted", s.World), "a gift today");
                s.SetVar("mood.wren.force", (int)Mood.Worried + 1);
                Assert.IsTrue(Conditions.Evaluate("mood:wren=worried", s.World), "a story arc can set the mood");
            }
        }
    }
}
