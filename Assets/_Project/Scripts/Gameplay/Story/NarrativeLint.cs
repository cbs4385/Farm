using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Farm.Gameplay
{
    public enum LintSeverity { Warning, Error }

    public sealed class LintIssue
    {
        public LintSeverity Severity;
        public string Rule;
        public string Villager;      // may be empty
        public string Where;
        public string Message;
        public override string ToString() => $"[{Severity}] {Rule}{(string.IsNullOrEmpty(Villager) ? "" : " (" + Villager + ")")}: {Where}: {Message}";
    }

    // Per-villager voice settings from the narrative bible (T-136). Everything is optional.
    public sealed class VoiceRule
    {
        public int MaxSentenceWords = 22;
        public bool AllowExclamation = true;
        public List<string> Banned = new List<string>();
        public string TicOpener;                 // a word that should open at least TicMinShare of the talk lines (Bram's "Hm")
        public float TicMinShare;
        public List<string> AllowedRepeatedOpeners = new List<string>();
    }

    // Rules and settings for the narrative validator. Loaded from Narrative/narrative_rules.json.
    public sealed class NarrativeConfig
    {
        public List<string> Locked = new List<string>();     // villagers a human has signed off: every warning is an error for them
        public List<string> Full = new List<string>();       // villagers at full depth (quotas are reported for them)
        public VoiceRule Defaults = new VoiceRule();
        public Dictionary<string, VoiceRule> Voices = new Dictionary<string, VoiceRule>();

        public VoiceRule VoiceOf(string villager) => Voices.TryGetValue(villager, out var v) ? v : Defaults;
        public static NarrativeConfig Parse(string json) => JsonConvert.DeserializeObject<NarrativeConfig>(json) ?? new NarrativeConfig();
    }

    // The narrative validator (T-136): spelling, length budgets, reserved priority bands, moment-tag and rarity quotas,
    // voice rules and duplicates. Errors fail the data tests; warnings are reported, and become errors for a villager
    // once a human has locked them (`NarrativeConfig.Locked`). Rules and budgets are in docs/narrative/STYLE.md.
    public static class NarrativeLint
    {
        public const int TalkWords = 28, GiftWords = 20, EventWords = 40, ChoiceWords = 9, BarkWords = 12, LetterWords = 80;
        public const int QuotaFunny = 3, QuotaWholesome = 2, QuotaSurprise = 1, QuotaRare = 3, QuotaLegendary = 1;
        public const int MythosBandMin = 8, MythosBandMax = 10, FirstMeetingPriority = 100;

        // Unambiguous British spellings. Words with an American meaning too (plaster, biscuit, spelt, timber) are left out.
        static readonly Regex British = new Regex(
            @"\b(neighbour\w*|colour\w*|cosy|cosier|cosiest|favourites?|fibres?|grey\w*|practis(?:e|es|ed|ing)|sunburnt|catalogues?|autumn\w*|" +
            @"centres?|metres?|litres?|theatres?|defence|offence|licence|organis\w+|realis\w+|recognis\w+|apologis\w+|jewellery|moulds?|ploughs?|programmes?|tyres?|storeys?|whilst|amongst|" +
            @"learnt|dreamt|burnt|cheques?|pyjamas|mum|mummy|aluminium|ageing|judgement|honour\w*|humour\w*|rumour\w*|labour\w*|flavour\w*|harbour\w*|savour\w*|vapour\w*|behaviour\w*|" +
            @"endeavour\w*|armour\w*|odour\w*|vigour\w*|fulfil|enrol|skilful|travell\w+|cancell\w+|labell\w+|modell\w+|levell\w+|marvellous|woollen|kerb|gaol|draught|sceptic\w*|artefacts?|" +
            @"manoeuvr\w+|pretence|analyse[sd]?|paralyse[sd]?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Modern slang, internet speak and mild swearing do not belong in a cozy village.
        static readonly Regex OffTone = new Regex(
            @"\b(lol|omg|bro|dude|vibes?|literally|cringe|yeet|poggers|sus|selfie|hashtag|emoji|wifi|app|google|facebook|twitter|tiktok|youtube|twitch|damn|hell|crap|bloody|sucks?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool HasBritishSpelling(string text, out string word)
        {
            var m = British.Match(text ?? string.Empty);
            word = m.Success ? m.Value : null;
            return m.Success;
        }

        public static int WordCount(string text) =>
            string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

        sealed class Line
        {
            public string Villager, Where, Key, Text, Kind;     // Kind: talk, gift, event, choice, letter
            public int Limit;
        }

        // `table` is the whole English string table (the spelling rule covers all of it); `text` resolves a key.
        public static List<LintIssue> Run(StoryContent story, IReadOnlyDictionary<string, string> table, NarrativeConfig config, IEnumerable<string> villagers)
        {
            config = config ?? new NarrativeConfig();
            var issues = new List<LintIssue>();
            var villagerList = villagers.ToList();
            string Text(string key) => key != null && table.TryGetValue(key, out var t) ? RichText.Plain(t) : null;

            void Add(LintSeverity severity, string rule, string villager, string where, string message)
            {
                if (severity == LintSeverity.Warning && !string.IsNullOrEmpty(villager) && config.Locked.Contains(villager)) severity = LintSeverity.Error;
                issues.Add(new LintIssue { Severity = severity, Rule = rule, Villager = villager, Where = where, Message = message });
            }

            // ---- spelling: every string in the game ----
            foreach (var kv in table)
                if (HasBritishSpelling(kv.Value, out var word))
                    Add(LintSeverity.Error, "spelling", "", kv.Key, $"'{word}' is a British spelling; the game uses American English");

            // ---- markup: every string that uses {braces} must be well formed ----
            foreach (var kv in table)
                if (kv.Value.IndexOf('{') >= 0 || kv.Value.IndexOf('}') >= 0)
                    foreach (var problem in RichText.Validate(kv.Value))
                        Add(LintSeverity.Error, "markup", "", kv.Key, problem);

            // ---- priority bands ----
            foreach (var set in story.Sets)
                foreach (var e in set.Entries)
                {
                    var mythos = e.Dialogue != null && e.Dialogue.StartsWith("mythos.", StringComparison.Ordinal);
                    var at = $"set {set.Id}: {e.Dialogue}";
                    if (e.Priority >= MythosBandMin && e.Priority <= MythosBandMax && !mythos)
                        Add(LintSeverity.Error, "band", "", at, $"priority {e.Priority} is reserved for the mythos layer (8-10)");
                    else if (e.Priority > MythosBandMax && e.Priority != FirstMeetingPriority)
                        Add(LintSeverity.Error, "band", "", at, $"priority {e.Priority} is outside the bands (0-7, 8-10 mythos, 100 first meeting)");
                    if (e.Priority < 0) Add(LintSeverity.Error, "band", "", at, "priority must not be negative");
                }

            // ---- collect lines ----
            var lines = new List<Line>();
            var talkDialogues = new Dictionary<string, string>();                    // dialogue id -> villager
            var barkDialogues = new HashSet<string>();
            foreach (var v in villagerList)
            {
                var set = story.Set($"npc.{v}.talk");
                if (set != null) foreach (var e in set.Entries) talkDialogues[e.Dialogue] = v;
                var barkSet = story.Set(Barks.SetId(v));
                if (barkSet != null) foreach (var e in barkSet.Entries) { talkDialogues[e.Dialogue] = v; barkDialogues.Add(e.Dialogue); }
            }
            foreach (var t in story.Topics)
                if (!string.IsNullOrEmpty(t.Dialogue) && villagerList.Contains(t.Npc ?? string.Empty)) talkDialogues[t.Dialogue] = t.Npc;
            foreach (var d in story.Dialogues)
            {
                string owner = null; var kind = "talk";
                var socialOwner = villagerList.FirstOrDefault(v => d.Id.StartsWith($"social.{v}.", StringComparison.Ordinal));
                if (talkDialogues.TryGetValue(d.Id, out var tv)) { owner = tv; if (barkDialogues.Contains(d.Id)) kind = "bark"; }
                else if (socialOwner != null) owner = socialOwner;
                else
                {
                    var gift = villagerList.FirstOrDefault(v => d.Id.StartsWith($"npc.{v}.gift.", StringComparison.Ordinal));
                    if (gift != null) { owner = gift; kind = "gift"; }
                }
                if (owner == null) continue;
                foreach (var n in d.Nodes)
                {
                    if (!string.IsNullOrEmpty(n.Text) && n.Speaker == owner)
                        lines.Add(new Line { Villager = owner, Where = $"{d.Id}/{n.Id}", Key = n.Text, Text = Text(n.Text), Kind = kind, Limit = kind == "gift" ? GiftWords : kind == "bark" ? BarkWords : TalkWords });
                    foreach (var c in n.Choices)
                        lines.Add(new Line { Villager = owner, Where = $"{d.Id}/{n.Id} choice", Key = c.Text, Text = Text(c.Text), Kind = "choice", Limit = ChoiceWords });
                }
            }
            foreach (var ev in story.Events)
                foreach (var step in ev.Steps)
                    if (step.Type == "say" && villagerList.Contains(step.Speaker ?? string.Empty))
                        lines.Add(new Line { Villager = step.Speaker, Where = $"event {ev.Id}", Key = step.Text, Text = Text(step.Text), Kind = "event", Limit = EventWords });
            foreach (var l in story.Letters)
                if (!string.IsNullOrEmpty(l.Sender) && villagerList.Contains(l.Sender))
                    lines.Add(new Line { Villager = l.Sender, Where = $"letter {l.Id}", Key = l.BodyKey, Text = Text(l.BodyKey), Kind = "letter", Limit = LetterWords });

            // ---- per-line rules ----
            foreach (var line in lines)
            {
                if (line.Text == null) continue;      // a missing key is the data validator's job
                var words = WordCount(line.Text);
                if (words > line.Limit)
                    Add(LintSeverity.Warning, "length", line.Villager, line.Where, $"{words} words; the {line.Kind} budget is {line.Limit}");
                if (OffTone.Match(line.Text) is Match m && m.Success)
                    Add(LintSeverity.Warning, "tone", line.Villager, line.Where, $"'{m.Value}' does not fit the village (slang, brand or swearing)");
                var voice = config.VoiceOf(line.Villager);
                if (line.Kind != "choice")
                {
                    if (!voice.AllowExclamation && line.Text.Contains("!"))
                        Add(LintSeverity.Warning, "voice", line.Villager, line.Where, "exclamation mark in a voice that does not use them");
                    foreach (var sentence in Regex.Split(line.Text, @"(?<=[.!?])\s+"))
                        if (WordCount(sentence) > voice.MaxSentenceWords)
                            Add(LintSeverity.Warning, "voice", line.Villager, line.Where, $"a sentence of {WordCount(sentence)} words; this voice stops at {voice.MaxSentenceWords}");
                    foreach (var banned in voice.Banned)
                        if (line.Text.IndexOf(banned, StringComparison.OrdinalIgnoreCase) >= 0)
                            Add(LintSeverity.Warning, "voice", line.Villager, line.Where, $"uses '{banned}', which this villager never says");
                }
            }

            // ---- tagged scenes against the clip-worthiness checklist ----
            foreach (var ev in story.Events.Where(e => !string.IsNullOrEmpty(e.Tag)))
            {
                var group = Memories.GroupOf(ev);
                var owner = villagerList.Contains(group) ? group : string.Empty;
                foreach (var problem in MomentChecklist.Problems(ev, Text))
                    Add(LintSeverity.Warning, "moment", owner, "scene " + ev.Id, problem);
            }

            // ---- per-villager rules ----
            foreach (var v in villagerList)
            {
                var talk = lines.Where(l => l.Villager == v && l.Kind == "talk" && l.Text != null).ToList();
                var voice = config.VoiceOf(v);

                foreach (var dup in talk.GroupBy(l => l.Text.Trim().ToLowerInvariant()).Where(g => g.Count() > 1))
                    Add(LintSeverity.Warning, "duplicate", v, string.Join(", ", dup.Select(l => l.Where)), $"the same text appears {dup.Count()} times: \"{dup.First().Text}\"");

                // Three per 24 lines (an eighth of the pool, never fewer than three), so a Full villager's 150 lines are held to the same variety as a small one's 24.
                var openerLimit = Math.Max(3, (talk.Count + 7) / 8);
                foreach (var group in talk.GroupBy(l => FirstWord(l.Text)).Where(g => g.Count() > openerLimit))
                    if (!voice.AllowedRepeatedOpeners.Contains(group.Key))
                        Add(LintSeverity.Warning, "opener", v, group.Key, $"{group.Count()} talk lines start with '{group.Key}' (limit {openerLimit})");

                if (!string.IsNullOrEmpty(voice.TicOpener) && talk.Count > 0)
                {
                    var share = talk.Count(l => FirstWord(l.Text) == voice.TicOpener.ToLowerInvariant()) / (float)talk.Count;
                    if (share < voice.TicMinShare)
                        Add(LintSeverity.Warning, "voice", v, "talk lines", $"'{voice.TicOpener}' opens {share:P0} of talk lines; the voice needs {voice.TicMinShare:P0}");
                }

                var playerShare = talk.Count == 0 ? 0f : talk.Count(l => l.Text.Contains("[player]")) / (float)talk.Count;
                if (playerShare > 0.25f)
                    Add(LintSeverity.Warning, "voice", v, "talk lines", $"[player] appears in {playerShare:P0} of talk lines (limit 25%)");

                // Quotas: only a locked villager must meet them; Full villagers get them in the report.
                var tags = TagCounts(story, v, villagerList);
                void Quota(string name, int have, int need)
                {
                    if (have >= need) return;
                    var severity = config.Locked.Contains(v) ? LintSeverity.Error : LintSeverity.Warning;
                    if (severity == LintSeverity.Warning && !config.Full.Contains(v)) return;
                    Add(severity, "quota", v, $"set npc.{v}.talk", $"{name}: {have} of the {need} required");
                }
                Quota("funny lines", tags.Funny, QuotaFunny);
                Quota("wholesome lines", tags.Wholesome, QuotaWholesome);
                Quota("surprise lines", tags.Surprise, QuotaSurprise);
                Quota("rare lines", tags.Rare, QuotaRare);
                Quota("legendary lines", tags.Legendary, QuotaLegendary);
            }
            return issues;
        }

        public struct TagTotals { public int Funny, Wholesome, Surprise, Mystery, Rare, Legendary, Uncommon; }

        // Moments of every kind (talk lines, tagged nodes, tagged scenes) count towards a villager's quotas; rarity is for talk lines.
        public static TagTotals TagCounts(StoryContent story, string villager, IEnumerable<string> villagers = null)
        {
            var t = new TagTotals();
            var moments = MomentCatalog.Build(story, villagers ?? new[] { villager });
            foreach (var m in moments.Where(m => m.Villager == villager))
                switch (m.Tag) { case "funny": t.Funny++; break; case "wholesome": t.Wholesome++; break; case "surprise": t.Surprise++; break; case "mystery": t.Mystery++; break; }
            var set = story.Set($"npc.{villager}.talk");
            if (set != null)
                foreach (var e in set.Entries)
                    switch (e.Rarity) { case LineRarity.Rare: t.Rare++; break; case LineRarity.Legendary: t.Legendary++; break; case LineRarity.Uncommon: t.Uncommon++; break; }
            return t;
        }

        static string FirstWord(string text)
        {
            var m = Regex.Match(text ?? string.Empty, @"[A-Za-z']+");
            return m.Success ? m.Value.ToLowerInvariant() : string.Empty;
        }

        // A Markdown report: per villager, the numbers writers and reviewers need (Farm > Narrative Report writes it).
        public static string Report(StoryContent story, IReadOnlyDictionary<string, string> table, NarrativeConfig config, IEnumerable<string> villagers, List<LintIssue> issues)
        {
            config = config ?? new NarrativeConfig();
            var sb = new StringBuilder();
            sb.AppendLine("# Narrative report").AppendLine();
            sb.AppendLine("| Villager | Talk entries | Distinct talk lines | Avg words | Funny | Wholesome | Surprise | Rare | Legendary | Warnings | Errors | Locked |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var v in villagers)
            {
                var set = story.Set($"npc.{v}.talk");
                var texts = new List<string>();
                if (set != null)
                    foreach (var e in set.Entries)
                    {
                        var d = story.Dialogue(e.Dialogue);
                        if (d == null) continue;
                        foreach (var n in d.Nodes)
                            if (n.Speaker == v && n.Text != null && table.TryGetValue(n.Text, out var t)) texts.Add(t);
                    }
                var tags = TagCounts(story, v);
                var avg = texts.Count == 0 ? 0 : texts.Average(WordCount);
                sb.AppendLine($"| {v} | {set?.Entries.Count ?? 0} | {texts.Distinct().Count()} | {avg:0.0} | {tags.Funny} | {tags.Wholesome} | {tags.Surprise} | {tags.Rare} | {tags.Legendary} | " +
                              $"{issues.Count(i => i.Villager == v && i.Severity == LintSeverity.Warning)} | {issues.Count(i => i.Villager == v && i.Severity == LintSeverity.Error)} | {(config.Locked.Contains(v) ? "yes" : "no")} |");
            }
            var moments = MomentCatalog.Build(story, villagers);
            sb.AppendLine().AppendLine("## Moments");
            sb.AppendLine($"{moments.Count} tagged moment(s): {moments.Count(m => m.Kind == MomentKind.Line)} talk lines, {moments.Count(m => m.Kind == MomentKind.Node)} dialogue nodes, {MomentCatalog.Scenes(moments)} scenes. Target: at least 25 tagged set pieces.");
            foreach (var tag in DialogueVocabulary.MomentTags)
                sb.AppendLine($"- {tag}: {moments.Count(m => m.Tag == tag)}");
            sb.AppendLine().AppendLine("## Issues");
            foreach (var group in issues.GroupBy(i => i.Rule).OrderBy(g => g.Key))
            {
                sb.AppendLine().AppendLine($"### {group.Key} ({group.Count()})");
                foreach (var i in group.Take(60)) sb.AppendLine("- " + i);
                if (group.Count() > 60) sb.AppendLine($"- ... and {group.Count() - 60} more");
            }
            return sb.ToString();
        }
    }
}
