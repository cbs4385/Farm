using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-06: villagers talked about things the player had never been told (the fisher on the missing pumpkin, on day one).
    // A storyline's talk lines may only come after one of its opening lines has been heard.
    public class StorylineIntroTests
    {
        static readonly string[] Ids = { "pie_feud", "anonymous_notes", "lost_umbrella", "rival_scarecrows", "mystery_whistler", "competing_band", "missing_pumpkin", "five_names_cat" };
        static readonly Regex Heard = new Regex(@"heard:([\w.]+)");

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        // The storyline's talk lines while it is running: not the ones after the payoff, not the gazette.
        List<(DialogueSet set, DialogueSetEntry entry)> Running(string id) =>
            _story.Sets.Where(s => s.Id.StartsWith("npc.") && s.Id.EndsWith(".talk"))
                .SelectMany(s => s.Entries.Select(e => (set: s, entry: e)))
                .Where(x => x.entry.Condition != null && x.entry.Condition.Contains("storyline:" + id) && x.entry.Condition.Contains("!flag:storydone." + id)).ToList();

        [Test]
        public void EveryStoryline_HasOpeningLines_AndEveryOtherTalkLineWaitsForOne()
        {
            foreach (var id in Ids)
            {
                var running = Running(id);
                var openers = running.Where(x => !x.entry.Condition.Contains("heard:")).Select(x => x.entry.Dialogue).ToList();
                Assert.IsNotEmpty(openers, id + ": no line opens the storyline");
                foreach (var (set, entry) in running.Where(x => x.entry.Condition.Contains("heard:")))
                    foreach (Match m in Heard.Matches(entry.Condition))
                        CollectionAssert.Contains(openers, m.Groups[1].Value, $"{entry.Dialogue} waits for {m.Groups[1].Value}, which is not an opening line of {id}");
            }
        }

        [Test]
        public void TheFisher_SaysNothingOfThePumpkin_UntilSomeoneHasToldThePlayer()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            using (var f = new TestSessionFixture(Resources.Load<GameDatabase>(GameDatabase.ResourcePath).AllItems, Resources.Load<GameDatabase>(GameDatabase.ResourcePath).AllCrops, null))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                s.SetFlag("storyline.missing_pumpkin");
                s.SetFlag("storyline.missing_pumpkin.felix");
                var felix = s.Story.Set("npc.felix.talk").Entries.First(e => e.Dialogue == "felix.pumpkin.1");
                Assert.IsFalse(Conditions.TryEvaluate(felix.Condition, s.World, out var before) && before, "day one: nobody has mentioned a pumpkin");

                var memory = LineMemory.Load(s);
                memory.Record("npc.tilda.talk", "tilda.pumpkin.1", 3, 2);
                LineMemory.Store(s, memory);
                Assert.IsTrue(Conditions.TryEvaluate(felix.Condition, s.World, out var after) && after, "once Tilda has told the player, the fisher may mention it");
            }
        }
    }
}
