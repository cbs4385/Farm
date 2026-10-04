using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-092: the narrative condition atoms (festival.in, birthday.in, farm, heard, choice, storyline, farmname) and the
    // calendar arithmetic behind them.
    public class NarrativeAtomsTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public int Festival = -1, Crops, Animals;
            public readonly Dictionary<string, int> Birthdays = new Dictionary<string, int>();
            public readonly HashSet<string> Heard = new HashSet<string>();
            public string Farm = "Sunny Acres";
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Farm";
            public int Hearts(string npcId) => 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
            public int FestivalDaysAway() => Festival;
            public int BirthdayDaysAway(string npcId) => Birthdays.TryGetValue(npcId, out var d) ? d : -1;
            public int CropCount() => Crops;
            public int AnimalCount() => Animals;
            public bool HeardLine(string dialogueId) => Heard.Contains(dialogueId);
            public string FarmName() => Farm;
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        static bool Eval(string expr, IWorldQuery w) => Conditions.Evaluate(expr, w);

        [Test]
        public void FestivalIn_ComparesTheDaysAway()
        {
            var w = new World { Festival = 2 };
            Assert.IsTrue(Eval("festival.in:<=3", w));
            Assert.IsTrue(Eval("festival.in:==2", w));
            Assert.IsFalse(Eval("festival.in:<2", w));
            Assert.IsFalse(Eval("festival.in:==0", w));
            w.Festival = 0;
            Assert.IsTrue(Eval("festival.in:==0", w), "today");
            w.Festival = -1;
            Assert.IsFalse(Eval("festival.in:<=100", w), "no festival at all is never 'soon'");
        }

        [Test]
        public void BirthdayIn_IsPerVillager()
        {
            var w = new World();
            w.Birthdays["wren"] = 3;
            Assert.IsTrue(Eval("birthday.in:wren<=3", w));
            Assert.IsFalse(Eval("birthday.in:wren<3", w));
            Assert.IsFalse(Eval("birthday.in:bram<=30", w), "unknown birthday");
            Assert.IsTrue(Eval("birthday.in:wren>0 && birthday.in:wren<7", w));
        }

        [Test]
        public void Farm_CountsCropsAndAnimals()
        {
            var w = new World { Crops = 4, Animals = 1 };
            Assert.IsTrue(Eval("farm:crops>=4", w));
            Assert.IsFalse(Eval("farm:crops>4", w));
            Assert.IsTrue(Eval("farm:animals>=1", w));
            Assert.IsFalse(Eval("farm:animals>=2", w));
            Assert.IsFalse(Eval("farm:chickens>=0", w), "an unknown counter is false");
        }

        [Test]
        public void Heard_ChoiceStorylineAndFarmName()
        {
            var w = new World();
            w.Heard.Add("wren.sea_fall");
            Assert.IsTrue(Eval("heard:wren.sea_fall", w));
            Assert.IsFalse(Eval("heard:wren.sea_spring", w));
            w.Flags.Add("choice.wren.bag.listen");
            Assert.IsTrue(Eval("choice:wren.bag.listen", w));
            Assert.IsFalse(Eval("choice:wren.bag.stay", w));
            w.Flags.Add("storyline.pie_feud");
            Assert.IsTrue(Eval("storyline:pie_feud", w));
            Assert.IsFalse(Eval("storyline:notes", w));
            Assert.IsTrue(Eval("farmname:sunny_acres", w), "case-insensitive; underscore for a space");
            Assert.IsFalse(Eval("farmname:other", w));
        }

        [Test]
        public void ADifferentWorldWithoutTheQuery_SeesNothing()
        {
            var plain = new PlainWorld();
            Assert.IsFalse(Eval("festival.in:<=100", plain));
            Assert.IsFalse(Eval("farm:crops>=1", plain));
            Assert.IsFalse(Eval("heard:x", plain));
            Assert.IsFalse(Eval("birthday.in:wren<=100", plain));
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
        public void MalformedArguments_AreErrorsNotCrashes()
        {
            Assert.IsFalse(Conditions.TryEvaluate("festival.in:soon", new World(), out var ok) && ok);
            Assert.IsFalse(Conditions.TryEvaluate("farm:crops", new World(), out ok) && ok);
        }

        [Test]
        public void TheCalendar_CountsDays_AndWrapsTheYear()
        {
            var now = new GameDateTime(1, Season.Fall, 15);
            Assert.AreEqual(0, StoryCalendar.DaysUntil(now, Season.Fall, 15));
            Assert.AreEqual(2, StoryCalendar.DaysUntil(now, Season.Fall, 17));
            Assert.AreEqual(GameDateTime.DaysPerYear - 1, StoryCalendar.DaysUntil(now, Season.Fall, 14), "yesterday's date is almost a year away");
            Assert.AreEqual(14 + 28 + 5, StoryCalendar.DaysUntil(now, Season.Spring, 6), "across the new year");
        }

        [Test]
        public void TheNearestFestival_IsChosen()
        {
            var events = new[]
            {
                new EventDefinition { Id = "a", Calendar = "k", CalendarSeason = 0, CalendarDay = 10 },
                new EventDefinition { Id = "b", Calendar = "k", CalendarSeason = 2, CalendarDay = 20 },
                new EventDefinition { Id = "c" },   // not a festival
            };
            Assert.AreEqual(5, StoryCalendar.DaysUntilFestival(events, new GameDateTime(1, Season.Fall, 15)));
            Assert.AreEqual(9, StoryCalendar.DaysUntilFestival(events, new GameDateTime(1, Season.Spring, 1)));
            Assert.AreEqual(-1, StoryCalendar.DaysUntilFestival(new[] { new EventDefinition { Id = "c" } }, GameDateTime.NewGame));
            Assert.AreEqual(-1, StoryCalendar.DaysUntilFestival(null, GameDateTime.NewGame));
        }

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void InAGame_TheAtomsReadTheRealSession()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 15, 600));
                // Wren's birthday is Fall 17.
                Assert.IsTrue(Conditions.Evaluate("birthday.in:wren==2", s.World));
                Assert.IsFalse(Conditions.Evaluate("birthday.in:wren==0", s.World));
                // The shipped festivals exist, so one is always ahead.
                Assert.IsTrue(Conditions.Evaluate("festival.in:>=0", s.World));
                // Crops and animals.
                Assert.IsFalse(Conditions.Evaluate("farm:crops>=1", s.World));
                var grid = s.GetGrid(MapIds.Farm);
                grid.Till(3, 3);
                grid.TryGetTile(3, 3, out var tile);
                tile.Crop = new CropInstance { CropId = "crop.parsnip" };
                Assert.IsTrue(Conditions.Evaluate("farm:crops>=1", s.World));
                Assert.IsFalse(Conditions.Evaluate("farm:animals>=1", s.World));
                s.State.Animals.Add(new AnimalState { Id = "a1", Type = "chicken" });
                Assert.IsTrue(Conditions.Evaluate("farm:animals>=1", s.World));
                // The farm name, and what a villager has said.
                s.State.FarmName = "Turnip Hollow";
                Assert.IsTrue(Conditions.Evaluate("farmname:turnip_hollow", s.World));
                Assert.IsFalse(Conditions.Evaluate("heard:wren.chat1", s.World));
                var memory = LineMemory.Load(s);
                memory.Record("npc.wren.talk", "wren.chat1", 5, 0);
                LineMemory.Store(s, memory);
                Assert.IsTrue(Conditions.Evaluate("heard:wren.chat1", s.World));
            }
        }

        [Test]
        public void TheValidator_KnowsTheNewAtoms()
        {
            var story = new StoryContent();
            story.AddJson("{\"dialogues\":[{\"id\":\"x\",\"start\":\"n0\",\"nodes\":[{\"id\":\"n0\",\"speaker\":\"wren\",\"text\":\"k\",\"condition\":\"festival.in:<=3 && farm:crops>=1 && storyline:pie && choice:a.b && heard:x\"}]}]}", "t");
            foreach (var d in story.Dialogues)
                foreach (var n in d.Nodes)
                    Assert.IsTrue(Conditions.TryEvaluate(n.Condition, new World(), out _), "parses");
        }
    }
}
