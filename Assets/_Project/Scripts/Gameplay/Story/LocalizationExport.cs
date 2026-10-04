using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Farm.Gameplay
{
    public sealed class LocalizationRow
    {
        public string Key, Text, Context, Speaker, Tokens;
        public int MaxWords, MaxChars;       // 0 = no budget
    }

    // T-139: the string table as a translator needs it: one row per key with the context the key does not show (who speaks, to whom, where
    // it appears), the length budget the layout can take, and the tokens and markup that must survive translation. Stable ids are the
    // keys themselves. Pure; the Editor menu `Farm > Localization > Export CSV` writes it.
    public static class LocalizationExport
    {
        static readonly Regex TokenPattern = new Regex(@"\[[a-z:_]+\]|\{\d+\}|\{[^{}]*\}|<[^<>]+>", RegexOptions.Compiled);

        public static List<LocalizationRow> Build(IDictionary<string, string> table, StoryContent story)
        {
            var dialogueOf = new Dictionary<string, (string dialogue, string node, string speaker, bool choice)>();
            var barkDialogues = new HashSet<string>();
            var giftDialogues = new HashSet<string>();
            foreach (var d in story.Dialogues)
            {
                if (d.Id.IndexOf(".gift.", StringComparison.Ordinal) >= 0) giftDialogues.Add(d.Id);
                foreach (var n in d.Nodes)
                {
                    if (!string.IsNullOrEmpty(n.Text) && !dialogueOf.ContainsKey(n.Text)) dialogueOf[n.Text] = (d.Id, n.Id, n.Speaker, false);
                    foreach (var c in n.Choices ?? new List<DialogueChoice>())
                        if (!string.IsNullOrEmpty(c.Text) && !dialogueOf.ContainsKey(c.Text)) dialogueOf[c.Text] = (d.Id, n.Id, "player", true);
                }
            }
            foreach (var set in story.Sets.Where(s => s.Id.EndsWith(".bark", StringComparison.Ordinal))) foreach (var e in set.Entries) barkDialogues.Add(e.Dialogue);
            var eventLine = new Dictionary<string, (string ev, string speaker)>();
            foreach (var e in story.Events)
                foreach (var s in e.Steps)
                    if (s.Type == "say" && !string.IsNullOrEmpty(s.Text) && !eventLine.ContainsKey(s.Text)) eventLine[s.Text] = (e.Id, s.Speaker);

            var rows = new List<LocalizationRow>();
            foreach (var kv in table.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                var row = new LocalizationRow { Key = kv.Key, Text = kv.Value, Tokens = string.Join(" ", TokenPattern.Matches(kv.Value ?? string.Empty).Cast<Match>().Select(m => m.Value).Distinct()) };
                if (dialogueOf.TryGetValue(kv.Key, out var dl))
                {
                    row.Speaker = dl.speaker ?? string.Empty;
                    if (dl.choice) { row.Context = $"A choice the player can pick in dialogue {dl.dialogue} (node {dl.node}). First person, short."; row.MaxWords = NarrativeLint.ChoiceWords; }
                    else
                    {
                        var who = string.IsNullOrEmpty(dl.speaker) ? "narration" : $"spoken by {dl.speaker}";
                        row.Context = $"Dialogue {dl.dialogue}, node {dl.node}, {who}. Keep the voice of the speaker.";
                        row.MaxWords = barkDialogues.Contains(dl.dialogue) ? NarrativeLint.BarkWords : giftDialogues.Contains(dl.dialogue) ? NarrativeLint.GiftWords : NarrativeLint.TalkWords;
                        if (barkDialogues.Contains(dl.dialogue)) row.Context += " A speech bubble over the villager's head.";
                    }
                }
                else if (eventLine.TryGetValue(kv.Key, out var ev))
                {
                    row.Speaker = ev.speaker ?? string.Empty;
                    row.Context = $"A line in the scene {ev.ev}, spoken by {ev.speaker}.";
                    row.MaxWords = NarrativeLint.EventWords;
                }
                else if (kv.Key.StartsWith("memory.", StringComparison.Ordinal)) { row.Context = "Title of a replayable scene, on a small button."; row.MaxChars = 22; }
                else if (kv.Key.StartsWith("letter.", StringComparison.Ordinal)) { row.Context = kv.Key.EndsWith(".subject", StringComparison.Ordinal) ? "A letter's subject line." : "A letter's body."; if (kv.Key.EndsWith(".body", StringComparison.Ordinal)) row.MaxWords = NarrativeLint.LetterWords; }
                else if (kv.Key.StartsWith("quest.", StringComparison.Ordinal)) row.Context = "Quest journal text (title, description or objective).";
                else if (kv.Key.StartsWith("event.", StringComparison.Ordinal)) row.Context = "Scene text.";
                else if (kv.Key.StartsWith("item.", StringComparison.Ordinal)) row.Context = "An item's name or description.";
                else if (kv.Key.StartsWith("options.", StringComparison.Ordinal) || kv.Key.StartsWith("ui.", StringComparison.Ordinal)) row.Context = "Interface label. Short.";
                else row.Context = "Game text.";
                rows.Add(row);
            }
            return rows;
        }

        public static string ToCsv(IEnumerable<LocalizationRow> rows)
        {
            var sb = new StringBuilder("key,text,context,speaker,max_words,max_chars,tokens\n");
            foreach (var r in rows)
                sb.Append(Escape(r.Key)).Append(',').Append(Escape(r.Text)).Append(',').Append(Escape(r.Context)).Append(',').Append(Escape(r.Speaker)).Append(',')
                  .Append(r.MaxWords).Append(',').Append(r.MaxChars).Append(',').Append(Escape(r.Tokens)).Append('\n');
            return sb.ToString();
        }

        // RFC 4180: a field with a comma, quote or line break is quoted and its quotes doubled.
        public static string Escape(string field)
        {
            field = field ?? string.Empty;
            return field.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + field.Replace("\"", "\"\"") + "\"" : field;
        }

        // The inverse of ToCsv for the first two columns, so a translator's file can be read back by key (used by tests and the import step).
        public static Dictionary<string, string> ReadKeyText(string csv)
        {
            var result = new Dictionary<string, string>();
            var rows = ParseCsv(csv);
            for (var i = 1; i < rows.Count; i++) if (rows[i].Count >= 2) result[rows[i][0]] = rows[i][1];
            return result;
        }

        static List<List<string>> ParseCsv(string csv)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < csv.Length; i++)
            {
                var c = csv[i];
                if (quoted)
                {
                    if (c == '"') { if (i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; } else quoted = false; }
                    else field.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n') { row.Add(field.ToString()); field.Clear(); rows.Add(row); row = new List<string>(); }
                else if (c != '\r') field.Append(c);
            }
            if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
            return rows;
        }
    }
}
