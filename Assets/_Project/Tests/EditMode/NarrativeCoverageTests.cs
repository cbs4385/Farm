using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Spec WP10 (docs/NPC_INTERCONNECTED_GAMEPLAY_SPEC.md): how many different lines a player can hear from each villager at each friendship tier, in each season, in rain and at
    // night, and whether a line that names a time has the condition for it. `CoverageReport` writes the table to Builds/narrative_coverage.txt for the writers; the other tests fail
    // when a villager falls below the floors. Existing lines are never changed to meet them: new lines are written where a floor is missed.
    public class NarrativeCoverageTests
    {
        public static readonly string[] Npcs = { "tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara" };
        static readonly Season[] Seasons = { Season.Spring, Season.Summer, Season.Fall, Season.Winter };
        static readonly int[] Tiers = { 0, 2, 4, 6, 8, 10 };

        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public readonly Dictionary<string, int> Hearts_ = new Dictionary<string, int>();
            public readonly Dictionary<string, string> Quests = new Dictionary<string, string>();
            public GameDateTime Time = new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string WeatherId = "sunny";
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => Time;
            public string Weather => WeatherId;
            public string MapId => "Village";
            public int Hearts(string npcId) => Hearts_.TryGetValue(npcId, out var h) ? h : 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => Quests.TryGetValue(questId, out var q) ? q : "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        // A player who has met the villager and has spent every one-shot scene and quest offer, so only the ordinary pool is measured.
        World Met(string npc, int hearts, Season season = Season.Spring, string weather = "sunny", int hour = 12, int day = 3)
        {
            var w = new World { Time = new GameDateTime(1, season, day, hour * 60), WeatherId = weather };
            w.Flags.Add("met." + npc);
            w.Flags.Add("hazel.book");          // a one-shot scene that would hide the ordinary pool
            w.Hearts_[npc] = hearts;
            foreach (var q in _story.Quests.Select(q => q.Id)) w.Quests[q] = "done";
            return w;
        }

        int Count(string npc, World w)
        {
            var set = _story.Set($"npc.{npc}.talk");
            var heard = new HashSet<string>();
            for (var seed = 0; seed < 300; seed++) heard.Add(set.Pick(w, seed));
            return heard.Count;
        }

        // The fewest different lines over the four seasons for one situation.
        int Floor(string npc, int hearts, string weather = "sunny", int hour = 12) => Seasons.Min(s => Count(npc, Met(npc, hearts, s, weather, hour)));

        // Floors: the fewest different lines a player can hear from a villager in a situation, over the four seasons (the first lines of a friendship must not run dry, and neither
        // must the later ones). Raise these when a batch of lines is written; never lower them.
        [Test]
        public void EveryVillager_HasEnoughLines_AtEveryFriendshipTier()
        {
            var short_ = new List<string>();
            foreach (var npc in Npcs)
                foreach (var t in Tiers)
                {
                    var need = t <= 2 ? 20 : 36;
                    var got = Floor(npc, t);
                    if (got < need) short_.Add($"{npc} at {t} hearts: {got} lines, needs {need}");
                }
            Assert.IsEmpty(short_, string.Join("\n", short_));
        }

        [Test]
        public void EveryVillager_HasRainLines_AtEveryFriendshipTier_AndTheyAreBeatenByNothing()
        {
            var short_ = new List<string>();
            foreach (var npc in Npcs)
                foreach (var t in Tiers)
                {
                    var got = Floor(npc, t, "rain");
                    if (got < 4) short_.Add($"{npc} at {t} hearts in rain: {got} lines, needs 4");
                    var set = _story.Set($"npc.{npc}.talk");
                    var heard = new HashSet<string>();
                    for (var seed = 0; seed < 100; seed++) heard.Add(set.Pick(Met(npc, t, Season.Spring, "rain"), seed));
                    if (!heard.All(id => id.Contains(".rain"))) short_.Add($"{npc} at {t} hearts: a line that is not a rain line outranks the rain lines on a rainy day");
                }
            Assert.IsEmpty(short_, string.Join("\n", short_));
        }

        [Test]
        public void EveryVillager_HasMorningEveningAndNightLines()
        {
            var missing = new List<string>();
            foreach (var npc in Npcs)
            {
                var set = _story.Set($"npc.{npc}.talk");
                HashSet<string> At(int hour) { var h = new HashSet<string>(); for (var seed = 0; seed < 300; seed++) h.Add(set.Pick(Met(npc, 3, Season.Spring, "sunny", hour), seed)); return h; }
                var noon = At(12);
                foreach (var (name, hour) in new[] { ("morning", 8), ("evening", 18), ("night", 21) })
                    if (!At(hour).Except(noon).Any()) missing.Add($"{npc} has no {name}-only line");
            }
            Assert.IsEmpty(missing, string.Join("\n", missing));
        }

        // The states of the world the spec asks the talk to answer (a festival coming, a deed just done, the harvest shipped, the farm's animals) each have a line for every villager.
        [Test]
        public void EveryVillager_AnswersTheWorldStatesOfTheReactiveBacklog()
        {
            var missing = new List<string>();
            foreach (var npc in Npcs)
            {
                var conds = _story.Set($"npc.{npc}.talk").Entries.Select(e => e.Condition ?? string.Empty).ToList();
                foreach (var (atom, need) in new[] { ("festival.in", 1), ("recent:quest", 1), ("recent:gift", 1), ("shipped:", 2), ("farm:", 1) })
                    if (conds.Count(c => c.Contains(atom)) < need) missing.Add($"{npc} has fewer than {need} line(s) for {atom}");
            }
            Assert.IsEmpty(missing, string.Join("\n", missing));
        }

        // A line that names a time must carry the condition for it (docs/narrative: "time-bound statements"). Lines written before this check that break it are listed, with the reason
        // they stay (existing dialogue is never edited to satisfy a test); a new line is not allowed to.
        static readonly string[] TimeWords = { "tonight", "this morning", "this evening", "this afternoon", "after dark" };
        static readonly string[] Weekdays = { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
        // Lines from before this check: festival-day and after-festival lines, quest turn-ins and a few goal lines that say "tonight" or "this morning" about the day they are heard
        // on. They are left as written (the owner: leave all existing dialogue in the project); a new line may not join them.
        static readonly HashSet<string> KnownTimeBound = new HashSet<string>
        {
            "tilda: tilda.co.3 says 'tonight' with no hour condition",
            "bram: bram.festival.winter.day says 'tonight' with no hour condition",
            "ione: ione.festival.spring.after says 'this morning' with no hour condition",
            "wren: wren.festival.winter.day says 'tonight' with no hour condition",
            "felix: felix.festival.summer.day says 'this morning' with no hour condition",
            "piper: piper.festival.winter.day says 'tonight' with no hour condition",
            "piper: piper.goal.r2 says 'this morning' with no hour condition",
            "piper: piper.namefarm.spud_life says 'tonight' with no hour condition",
            "piper: piper.summer.5 says 'after dark' with no hour condition",
            "piper: piper.strings.turnin says 'tonight' with no hour condition",
            "elara: elara.garden.turnin says 'tonight' with no hour condition",
        };

        static readonly Dictionary<string, string> En = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        [Test]
        public void ALineThatNamesATime_HasTheConditionForIt()
        {
            var broken = new List<string>();
            foreach (var npc in Npcs)
                foreach (var e in _story.Set($"npc.{npc}.talk").Entries)
                {
                    var graph = _story.Dialogue(e.Dialogue);
                    var key = graph?.Nodes.FirstOrDefault()?.Text;
                    if (key == null || !En.TryGetValue(key, out var text)) continue;
                    var lower = text.ToLowerInvariant();
                    var cond = e.Condition ?? string.Empty;
                    bool Has(string needle) => cond.Contains(needle);
                    foreach (var w in TimeWords)
                        if (lower.Contains(w) && !Has("hour") && !Has("festival.in") && !Has("birthday.in")) broken.Add($"{npc}: {e.Dialogue} says '{w}' with no hour condition");
                    foreach (var d in Weekdays)
                        if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\b(it's|it is|today is|today's|this) " + d + @"\b") && !Has("weekday") && !Has("festival.in") && !Has("birthday.in")) broken.Add($"{npc}: {e.Dialogue} says it is {d} with no weekday condition");          // a standing remark ("closed Sundays") is not time-bound
                }
            var fresh = broken.Where(b => !KnownTimeBound.Contains(b)).ToList();
            Assert.IsEmpty(fresh, string.Join("\n", fresh));
        }

        [Test]
        public void CoverageReport()
        {
            var sb = new StringBuilder("villager  tier(hearts): lines heard (the fewest over the four seasons)  |  sunny noon / rain / night 21:00\n");
            foreach (var npc in Npcs)
            {
                sb.Append(npc.PadRight(8));
                foreach (var t in Tiers) sb.Append($"  {t}: {Floor(npc, t)}/{Floor(npc, t, "rain")}/{Floor(npc, t, "sunny", 21)}");
                sb.AppendLine();
            }
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/narrative_coverage.txt", sb.ToString());
            TestContext.WriteLine(sb.ToString());
            Assert.Pass();
        }
    }
}
