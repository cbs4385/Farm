using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-123: every villager has a line the day before, on, and after each festival; one festival scene is made to be clipped.
    public class FestivalContentTests
    {
        static readonly string[] All = { "wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione" };
        static readonly (string season, int day)[] Festivals = { ("spring", 13), ("summer", 11), ("fall", 16), ("winter", 25) };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EveryVillager_HasABeforeDayAndAfterLine_ForEveryFestival()
        {
            foreach (var npc in All)
            {
                var ids = _story.Set($"npc.{npc}.talk").Entries.Select(e => e.Dialogue).ToHashSet();
                foreach (var (season, _) in Festivals)
                    foreach (var part in new[] { "before", "day", "after" })
                        Assert.IsTrue(ids.Contains($"{npc}.festival.{season}.{part}"), $"{npc} {season} {part}");
            }
        }

        [Test]
        public void TheLines_AreTiedToTheRightCalendarDays()
        {
            foreach (var npc in All)
            {
                var entries = _story.Set($"npc.{npc}.talk").Entries;
                foreach (var (season, day) in Festivals)
                {
                    StringAssert.Contains($"season:{season} && day=={day - 1}", entries.First(e => e.Dialogue == $"{npc}.festival.{season}.before").Condition);
                    StringAssert.Contains($"day=={day}", entries.First(e => e.Dialogue == $"{npc}.festival.{season}.day").Condition);
                    StringAssert.Contains($"day=={day + 1}", entries.First(e => e.Dialogue == $"{npc}.festival.{season}.after").Condition);
                }
            }
        }

        [Test]
        public void TheLanternRelease_IsAYearlyMemory_WithAVisibleBeat_AfterJoiningTheFeast()
        {
            var ev = _story.Event("festival_winter_lanterns");
            Assert.IsNotNull(ev);
            Assert.IsFalse(ev.Once);
            Assert.IsTrue(Memories.IsMemory(ev));
            Assert.IsTrue(ev.Steps.Any(s => s.Type == "emote"));
            StringAssert.Contains("flag:festival.winter.joined", ev.Condition);
            Assert.GreaterOrEqual(ev.Steps.Count(s => s.Type == "say"), 4);
        }
    }
}
