using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-122: villagers talk to each other. Six pairings, three overheard scenes each, unlocked in order as the player gets to know both.
    public class OverheardScenesTests
    {
        static readonly string[] Pairs = { "juno_bram", "hazel_ione", "elara_odalys", "marcus_dorian", "wren_piper", "tilda_felix", "bram_marcus", "tilda_wren", "piper_juno", "dorian_hazel", "felix_elara", "odalys_ione" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void EveryPairing_HasThreeScenes_InOrder_EachWithAVisibleBeatAndAMemory()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var pair in Pairs)
                for (var k = 1; k <= 4; k++)
                {
                    var id = $"overheard_{pair}_{k}";
                    var ev = _story.Event(id);
                    if (k == 4 && ev == null) continue;      // only four of the pairings have a fourth scene
                    Assert.IsNotNull(ev, id);
                    Assert.IsTrue(Memories.IsMemory(ev), id);
                    Assert.IsTrue(ev.Steps.Any(s => s.Type == "emote"), id);
                    Assert.AreEqual(4, ev.Steps.Count(s => s.Type == "say"), id);
                    Assert.LessOrEqual(en["memory." + id].Length, 22, id);
                    if (k > 1) StringAssert.Contains($"flag:event.overheard_{pair}_{k - 1}", ev.Condition);
                    // Both villagers must be known first: nobody overhears strangers.
                    foreach (var who in pair.Split('_')) StringAssert.Contains("flag:met." + who, ev.Condition);
                    for (var i = 1; i <= 4; i++) Assert.IsTrue(en.ContainsKey($"event.{id}.l{i}"), id);
                }
        }

        [Test]
        public void TheVillage_HasFortyOverheardScenes_AsThePlanAsks()
        {
            Assert.AreEqual(40, _story.Events.Count(e => e.Id.StartsWith("overheard_")));
        }

        [Test]
        public void TheScenes_NeverPlayOnFestivalDays_OrAtNight()
        {
            foreach (var ev in _story.Events.Where(e => e.Id.StartsWith("overheard_")))
            {
                StringAssert.Contains("!festival.in:==0", ev.Condition);
                StringAssert.Contains("hour<19", ev.Condition);
            }
        }
    }
}
