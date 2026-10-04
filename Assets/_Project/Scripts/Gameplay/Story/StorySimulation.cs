using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Farm.Core;

namespace Farm.Gameplay
{
    // A stand-in for the game world that the simulation moves through the calendar: flags, hearts, weather and the few facts story
    // conditions ask about. Pure.
    public sealed class SimWorld : IWorldQuery, IGameQuery
    {
        public readonly HashSet<string> Flags = new HashSet<string>();
        public readonly Dictionary<string, int> HeartsOf = new Dictionary<string, int>();
        public readonly Dictionary<string, string> Quests = new Dictionary<string, string>();
        public GameDateTime Time = GameDateTime.NewGame;
        public string WeatherId = "sunny";
        public int Crops, Animals, FestivalAway = -1;
        public string Map = "Village";
        public readonly Dictionary<string, string> Moods = new Dictionary<string, string>();
        public readonly Dictionary<string, int> BirthdayAway = new Dictionary<string, int>();

        public bool HasFlag(string flag) => Flags.Contains(flag);
        public int GetVar(string name) => 0;
        public GameDateTime Now => Time;
        public string Weather => WeatherId;
        public string MapId => Map;
        public int Hearts(string npcId) => HeartsOf.TryGetValue(npcId, out var h) ? h : 0;
        public int ItemCount(string itemId) => 99;                                  // the bot always has what a quest asks for
        public string QuestState(string questId) => Quests.TryGetValue(questId, out var q) ? q : "new";
        public bool KnowsRecipe(string recipeId) => false;
        public int FestivalDaysAway() => FestivalAway;
        public int CropCount() => Crops;
        public int AnimalCount() => Animals;
        public string MoodOf(string npcId) => Moods.TryGetValue(npcId, out var m) ? m : "content";
        public int BirthdayDaysAway(string npcId) => BirthdayAway.TryGetValue(npcId, out var d) ? d : -1;
    }

    public sealed class VillagerSimReport
    {
        public string Villager;
        public int Visits, TalkEntries, DistinctHeard, TaggedHeard;
        public double Coverage;                  // distinct lines heard / talk entries, over the whole run
        public double CoverageYear1;             // the same after the first year (the plan's target is 60%)
        public int RepeatsWithin14Days;          // the same ordinary line twice inside 14 days at 3+ hearts
        public double TwoBackShare;              // visits that say what was said two visits earlier
        public double ConditionedShare;          // talk entries with a condition beyond hearts (static)
        public int LongestDeadAirDays;           // days with no line the villager had not said before
        public SortedDictionary<int, int> VisitsByPriority = new SortedDictionary<int, int>();   // priority band -> visits
        public List<string> MostHeard = new List<string>();      // "dialogue (priority) xN": what the visits were spent on
        public List<string> NeverHeard = new List<string>();     // "dialogue (condition)" of the lines the bot never heard
        public int StoryBeatVisits;              // visits that played a story beat (priority 4 and up) instead of chat
    }

    public sealed class SimReport
    {
        public int Years, Days;
        public List<VillagerSimReport> Villagers = new List<VillagerSimReport>();
        public double TaggedMomentsPerGameDay;   // across the villagers visited
        public int LongestGameDeadAirDays;       // days in a row with no line any villager had not said before (plan: at most 8)
        public int StorylinePool, StorylinesPerGame, MaxStorylineDifference;

        public string ToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Story simulation ({Years} year(s), {Days} days, every villager talked to daily)");
            sb.AppendLine();
            sb.AppendLine($"Year-one coverage: {string.Join(", ", Villagers.Select(v => $"{v.Villager} {v.CoverageYear1:P0}"))}. Longest stretch with nothing new from any villager: {LongestGameDeadAirDays} days.");
            sb.AppendLine();
            sb.AppendLine("| Villager | Talk entries | Distinct heard | Coverage | Repeats within 14 days | Two-back share | Conditioned share | Longest dead air (days) | Story beat visits | Tagged lines heard |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (var v in Villagers)
                sb.AppendLine($"| {v.Villager} | {v.TalkEntries} | {v.DistinctHeard} | {v.Coverage:P0} | {v.RepeatsWithin14Days} | {v.TwoBackShare:P0} | {v.ConditionedShare:P0} | {v.LongestDeadAirDays} | {v.StoryBeatVisits} | {v.TaggedHeard} |");
            sb.AppendLine();
            foreach (var v in Villagers)
            {
                sb.AppendLine();
                sb.AppendLine($"## Never heard: {v.Villager} ({v.NeverHeard.Count}); visits by priority band: {string.Join(", ", v.VisitsByPriority.Select(p => $"p{p.Key}={p.Value}"))}");
                sb.AppendLine("Most heard: " + string.Join("; ", v.MostHeard));
                foreach (var line in v.NeverHeard) sb.AppendLine("- " + line);
            }
            sb.AppendLine();
            sb.AppendLine($"Tagged moments heard per game day (all villagers): {TaggedMomentsPerGameDay:0.00}");
            sb.AppendLine($"Storylines: {StorylinesPerGame} drawn of {StorylinePool}; two games differ in at most {MaxStorylineDifference} storyline(s).");
            return sb.ToString();
        }
    }

    // T-138: the story simulation bot. It plays years of daily visits through the real variety engine (DialogueSet.PickVaried and
    // LineMemory), with hearts rising by talking and two gifts a week, the heart-event flags set as hearts are reached, quests
    // completed as they are offered, and weather and festival days from the calendar. It measures the targets of
    // docs/NPC_DIALOGUE_PLAN.md sections 2.2 and 2.3 that a bot can measure. Deterministic for a seed.
    public static class StorySimulation
    {
        public const int DaysPerYear = 112;
        static readonly int[] HeartEvents = { 2, 4, 5, 6, 8, 10 };
        static readonly (Season season, int day)[] Festivals = { (Season.Spring, 13), (Season.Summer, 11), (Season.Fall, 16), (Season.Winter, 25) };

        // `homes`: the map each villager is usually visited on (their workplace); `birthdays`: villager -> (season, day).
        public static SimReport Run(StoryContent story, IEnumerable<string> villagers, int years, int seed,
            IDictionary<string, string> homes = null, IDictionary<string, (Season season, int day)> birthdays = null)
        {
            var list = villagers.ToList();
            var report = new SimReport { Years = years, Days = years * DaysPerYear };
            var world = new SimWorld();
            var memory = new LineMemory();
            var points = list.ToDictionary(v => v, v => 0);
            var heardEver = list.ToDictionary(v => v, v => new HashSet<string>());
            var picks = list.ToDictionary(v => v, v => new List<(int day, string id, int priority, int hearts)>());
            var lastNew = list.ToDictionary(v => v, v => 0);
            var longestDead = list.ToDictionary(v => v, v => 0);
            var tagged = 0;
            var heardYear1 = list.ToDictionary(v => v, v => 0);
            var lastNewAny = 0;
            var longestDeadAny = 0;

            // This game's storylines, drawn the way a new game draws them.
            foreach (var id in Storylines.Draw(story.Storylines, seed, Storylines.PerGame))
            {
                world.Flags.Add(Storylines.FlagPrefix + id);
                var variant = Storylines.PickVariant(story.Storylines.FirstOrDefault(d => d.Id == id), seed);
                if (variant != null) world.Flags.Add($"{Storylines.FlagPrefix}{id}.{variant}");
            }
            foreach (var v in list) world.Flags.Add("met." + v);
            var giverQuests = story.Quests.Where(q => !string.IsNullOrEmpty(q.Giver)).GroupBy(q => q.Giver).ToDictionary(g => g.Key, g => g.Select(q => q.Id).OrderBy(i => i, StringComparer.Ordinal).ToList());

            for (var day = 0; day < report.Days; day++)
            {
                var year = day / DaysPerYear + 1;
                var inYear = day % DaysPerYear;
                var season = (Season)(inYear / 28);
                var dom = inYear % 28 + 1;
                var hour = new[] { 10, 12, 15, 18 }[(int)(DialogueSet.Unit(seed * 17 + day) * 4)];
                world.Time = new GameDateTime(year, season, dom, hour * 60);
                world.WeatherId = WeatherFor(season, seed, day);
                world.Crops = day >= 5 ? 12 : 0;
                world.Animals = day >= 30 ? 3 : 0;
                world.FestivalAway = FestivalDaysAway(inYear);
                if (birthdays != null)
                    foreach (var b in birthdays)
                    {
                        var away = (int)b.Value.season * 28 + b.Value.day - 1 - inYear;
                        world.BirthdayAway[b.Key] = away < 0 ? away + DaysPerYear : away;
                    }
                if (day > 40) foreach (var f in new[] { "pie_feud", "anonymous_notes", "lost_umbrella", "rival_scarecrows", "mystery_whistler" }) world.Flags.Add("storydone." + f);
                if (day > 10) world.Flags.Add("notes.began");

                foreach (var v in list)
                {
                    // Where the player finds them (mostly their workplace), and how they feel.
                    var roll = DialogueSet.Unit(seed * 53 + day * 7 + NpcInteractions.StableHash(v));
                    world.Map = homes != null && homes.TryGetValue(v, out var home) && roll < 0.7 ? home : "Village";
                    world.Moods[v] = Mood(DialogueSet.Unit(seed * 61 + day * 11 + NpcInteractions.StableHash(v)));
                    // Talking every day and a liked gift twice a week.
                    points[v] = FriendshipModel.Clamp(points[v] + FriendshipModel.TalkPoints + (day % 7 < 2 ? FriendshipModel.GiftPoints(GiftTaste.Liked, false) : 0));
                    world.HeartsOf[v] = FriendshipModel.Hearts(points[v]);
                    foreach (var h in HeartEvents) if (world.HeartsOf[v] >= h) world.Flags.Add($"event.{v}_heart{h}");
                    var set = story.Set($"npc.{v}.talk");
                    if (set == null) continue;
                    var id = set.PickVaried(world, day * 7919 + NpcInteractions.StableHash(v), memory, set.Id, day);
                    if (id == null) continue;
                    var entry = set.Entries.FirstOrDefault(e => e.Dialogue == id);
                    picks[v].Add((day, id, entry?.Priority ?? 0, world.HeartsOf[v]));
                    PlayEffects(story, id, world);
                    if (!string.IsNullOrEmpty(entry?.Tag)) tagged++;
                    if (heardEver[v].Add(id)) { lastNew[v] = day; lastNewAny = day; }
                    longestDead[v] = Math.Max(longestDead[v], day - lastNew[v]);
                }
                longestDeadAny = Math.Max(longestDeadAny, day - lastNewAny);
                if (day == DaysPerYear - 1) foreach (var v in list) heardYear1[v] = heardEver[v].Count;
            }

            foreach (var v in list)
            {
                var set = story.Set($"npc.{v}.talk");
                var entries = set?.Entries ?? new List<DialogueSetEntry>();
                var seq = picks[v];
                var r = new VillagerSimReport { Villager = v, Visits = seq.Count, TalkEntries = entries.Count, DistinctHeard = heardEver[v].Count, LongestDeadAirDays = longestDead[v] };
                var distinct = entries.Select(e => e.Dialogue).Distinct().Count();
                r.CoverageYear1 = distinct == 0 ? 0 : Math.Min(1.0, heardYear1[v] / (double)distinct);
                r.Coverage = entries.Count == 0 ? 0 : entries.Select(e => e.Dialogue).Distinct().Count(heardEver[v].Contains) / (double)entries.Select(e => e.Dialogue).Distinct().Count();
                r.MostHeard = seq.GroupBy(p => p.id).OrderByDescending(g => g.Count()).Take(10).Select(g => $"{g.Key} (p{g.First().priority}) x{g.Count()}").ToList();
                r.NeverHeard = entries.Where(e => !heardEver[v].Contains(e.Dialogue)).Select(e => $"{e.Dialogue} [{e.Rarity ?? "common"}, p{e.Priority}] {e.Condition}").ToList();
                r.TaggedHeard = seq.Count(p => !string.IsNullOrEmpty(entries.FirstOrDefault(e => e.Dialogue == p.id)?.Tag));
                r.StoryBeatVisits = seq.Count(p => p.priority >= 4);
                foreach (var g in seq.GroupBy(p => p.priority)) r.VisitsByPriority[g.Key] = g.Count();
                var last14 = new Dictionary<string, int>();
                foreach (var p in seq)
                {
                    if (p.hearts >= 3 && p.priority < 4 && last14.TryGetValue(p.id, out var d) && p.day - d < 14) r.RepeatsWithin14Days++;
                    last14[p.id] = p.day;
                }
                var twoBack = 0; var comparable = 0;
                for (var i = 2; i < seq.Count; i++) { comparable++; if (seq[i].id == seq[i - 2].id) twoBack++; }
                r.TwoBackShare = comparable == 0 ? 0 : twoBack / (double)comparable;
                r.ConditionedShare = entries.Count == 0 ? 0 : entries.Count(e => HasConditionBeyondHearts(e.Condition)) / (double)entries.Count;
                report.Villagers.Add(r);
            }
            report.LongestGameDeadAirDays = longestDeadAny;
            report.TaggedMomentsPerGameDay = tagged / (double)Math.Max(1, report.Days);

            var pool = story.Storylines.ToList();
            report.StorylinePool = pool.Count;
            report.StorylinesPerGame = Math.Min(Storylines.PerGame, pool.Count);
            report.MaxStorylineDifference = MaxDifference(pool, report.StorylinesPerGame);
            return report;
        }

        // The most storylines in which two games can differ (a game has `perGame` of the pool): the larger of the two sets' own extras.
        public static int MaxDifference(List<StorylineDefinition> pool, int perGame)
        {
            var seen = new List<HashSet<string>>();
            for (var s = 1; s <= 300; s++) seen.Add(new HashSet<string>(Storylines.Draw(pool, s * 977, perGame)));
            var max = 0;
            for (var i = 0; i < seen.Count; i++)
                for (var j = i + 1; j < seen.Count; j++)
                    max = Math.Max(max, seen[i].Count(x => !seen[j].Contains(x)));
            return max;
        }

        // The player takes the first choice in a conversation and accepts what is offered; flags, quests and the like are applied as the
        // game would (only the effects that story conditions can see).
        static void PlayEffects(StoryContent story, string dialogueId, SimWorld world)
        {
            var graph = story.Dialogue(dialogueId);
            var node = graph?.Node(graph.Start);
            for (var guard = 0; node != null && guard < 40; guard++)
            {
                foreach (var e in node.Effects ?? new List<string>()) Apply(e, world);
                string next;
                var choice = node.Choices != null && node.Choices.Count > 0 ? node.Choices[0] : null;
                if (choice != null) { foreach (var e in choice.Effects ?? new List<string>()) Apply(e, world); next = choice.Next; }
                else next = node.Next;
                node = graph.Node(next);
            }
        }

        static void Apply(string effect, SimWorld world)
        {
            var colon = effect.IndexOf(':');
            if (colon <= 0) return;
            var verb = effect.Substring(0, colon);
            var arg = effect.Substring(colon + 1);
            if (verb == "flag") world.Flags.Add(arg);
            else if (verb == "quest.start") world.Quests[arg] = "active";
            else if (verb == "quest.done") world.Quests[arg] = "done";
        }

        static string Mood(double r) => r < 0.62 ? "content" : r < 0.72 ? "tired" : r < 0.82 ? "delighted" : r < 0.92 ? "mischievous" : r < 0.97 ? "worried" : "lonely";

        static string WeatherFor(Season season, int seed, int day)
        {
            var r = DialogueSet.Unit(seed * 131 + day * 31);
            if (season == Season.Winter) return r < 0.30 ? "snow" : r < 0.40 ? "wind" : "sunny";
            return r < 0.55 ? "sunny" : r < 0.78 ? "rain" : r < 0.84 ? "storm" : r < 0.94 ? "wind" : "sunny";
        }

        static int FestivalDaysAway(int dayInYear)
        {
            var best = -1;
            foreach (var (season, day) in Festivals)
            {
                var target = (int)season * 28 + day - 1;
                var away = target - dayInYear;
                if (away < 0) away += DaysPerYear;
                if (best < 0 || away < best) best = away;
            }
            return best;
        }

        // A condition beyond hearts and flags that only say a villager was met.
        public static bool HasConditionBeyondHearts(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition) || condition == "true") return false;
            foreach (var part in condition.Split(new[] { "&&", "||" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = part.Trim().TrimStart('!', '(').Trim();
                if (t.StartsWith("hearts:", StringComparison.Ordinal) || t.StartsWith("flag:met.", StringComparison.Ordinal)) continue;
                return true;
            }
            return false;
        }
    }
}
