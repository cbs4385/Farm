using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-102: which scenes are memories, how they are grouped and unlocked, and who is on stage in a replay.
    public class MemoriesTests
    {
        static EventDefinition Ev(string id, string title = null, string calendar = null) =>
            new EventDefinition { Id = id, TitleKey = title, Calendar = calendar };

        [Test]
        public void ATitle_OrACalendarName_MakesAMemory()
        {
            Assert.IsTrue(Memories.IsMemory(Ev("a_heart2", "memory.a_heart2")));
            Assert.IsTrue(Memories.IsMemory(Ev("festival_x", calendar: "calendar.x")));
            Assert.IsFalse(Memories.IsMemory(Ev("tutorial")));
            Assert.IsFalse(Memories.IsMemory(null));
            Assert.AreEqual("memory.a", Memories.TitleKey(Ev("a", "memory.a", "calendar.a")), "an explicit title wins");
            Assert.AreEqual("calendar.x", Memories.TitleKey(Ev("f", calendar: "calendar.x")));
        }

        [Test]
        public void Groups_AreFestival_Villager_OrOther()
        {
            Assert.AreEqual(Memories.FestivalGroup, Memories.GroupOf(Ev("festival_spring", calendar: "c")));
            Assert.AreEqual("wren", Memories.GroupOf(Ev("wren_heart5", "t")));
            Assert.AreEqual(Memories.OtherGroup, Memories.GroupOf(Ev("hall_ending", "t")));
        }

        [Test]
        public void ASceneIsUnlocked_OnceSeen()
        {
            var state = new GameState();
            var ev = Ev("wren_heart2", "t");
            Assert.IsFalse(Memories.Unlocked(state, ev));
            state.EventsSeen.Add("wren_heart2");
            Assert.IsTrue(Memories.Unlocked(state, ev));
        }

        [Test]
        public void TheCast_IsEveryoneButThePlayer_IncludingParallelSteps()
        {
            var ev = new EventDefinition
            {
                Id = "x",
                Steps =
                {
                    new EventStep { Type = "face", Actor = "wren" },
                    new EventStep { Type = "move", Actor = "player" },
                    new EventStep { Type = "say", Speaker = "wren" },
                    new EventStep { Type = "say", Speaker = "piper" },
                    new EventStep { Type = "parallel", Steps = { new EventStep { Type = "move", Actor = "bram" } } },
                },
            };
            CollectionAssert.AreEqual(new[] { "wren", "piper", "bram" }, Memories.Cast(ev));
        }

        [Test]
        public void AVillagerWhoIsAway_IsPlacedBesideWherePlayerIsMoved()
        {
            var ev = new EventDefinition { Id = "x", Steps = { new EventStep { Type = "move", Actor = "player", X = 6, Y = 3 } } };
            Assert.AreEqual(new Vector3Int(6, 4, 0), Memories.CastCell(ev, 0));
            Assert.AreEqual(new Vector3Int(7, 4, 0), Memories.CastCell(ev, 1), "a second person stands one cell along");
            Assert.AreEqual(new Vector3Int(8, 8, 0), Memories.CastCell(new EventDefinition { Id = "y" }, 0), "no player move: a default spot");
        }

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void EveryShippedHeartEventAndFestival_IsAMemory_WithAnExistingTitle()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var all = Memories.All(story);
            foreach (var ev in story.Events.Where(e => e.Id.Contains("_heart") || e.Id.StartsWith("festival_") || e.Id == "hall_ending"))
            {
                Assert.IsTrue(Memories.IsMemory(ev), ev.Id + " has a title");
                Assert.IsTrue(table.ContainsKey(Memories.TitleKey(ev)), ev.Id + ": the title text exists");
            }
            Assert.AreEqual(91, all.Count, "24 heart events, 36 Full and Enhanced scenes, 12 birthdays, 14 storyline scenes, 4 festivals and the hall ending");
            foreach (var group in all.GroupBy(Memories.GroupOf).Where(g => g.Key != Memories.FestivalGroup && g.Key != Memories.OtherGroup))
                Assert.IsTrue(NpcCatalog.From(RealDb()).All.Any(n => n.Id == group.Key), group.Key + " is a villager");
        }

        [Test]
        public void TheTitles_AreShortEnoughForTheirButtons()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var ev in Memories.All(story)) Assert.LessOrEqual(table[Memories.TitleKey(ev)].Length, 22, ev.Id);
        }

        [Test]
        public void UnlockedCount_CountsSeenMemories()
        {
            var story = StoryContent.LoadFromResources();
            var state = new GameState();
            Assert.AreEqual(0, Memories.UnlockedCount(story, state));
            state.EventsSeen.Add("wren_heart2");
            state.EventsSeen.Add("festival_spring");
            state.EventsSeen.Add("not_a_memory_event");
            Assert.AreEqual(2, Memories.UnlockedCount(story, state));
        }

        [Test]
        public void TheValidator_ChecksATitleExists()
        {
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var db = RealDb();
            var story = StoryContent.LoadFromResources();
            story.AddJson(@"{ ""events"": [ { ""id"": ""bad_memory"", ""trigger"": ""manual"", ""titleKey"": ""memory.no_such_title"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 } ] } ] }", "t");
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = story, Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey, RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            });
            Assert.IsTrue(problems.Any(p => p.Contains("bad_memory") && p.Contains("memory.no_such_title")), string.Join("\n", problems.Take(5)));
        }
    }
}
