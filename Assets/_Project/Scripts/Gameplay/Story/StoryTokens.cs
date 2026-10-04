using System;
using System.Text.RegularExpressions;

namespace Farm.Gameplay
{
    // Replaces [name] and [name:argument] placeholders in story text (T-094): [player], [farm], [season], [weekday] and
    // [npc:<id>] (the short name of a villager). `lookup` returns the replacement, or null to leave the placeholder alone.
    public static class StoryTokens
    {
        static readonly Regex Token = new Regex(@"\[([a-z]+)(?::([a-z0-9_]+))?\]", RegexOptions.Compiled);
        public static readonly string[] Weekdays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public static string Apply(string text, Func<string, string> lookup)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('[') < 0) return text;
            return Token.Replace(text, m =>
            {
                var key = m.Groups[2].Success ? m.Groups[1].Value + ":" + m.Groups[2].Value : m.Groups[1].Value;
                return lookup(key) ?? m.Value;
            });
        }

        // "Wren Calloway" becomes "Wren"; "Dr. Odalys Penn" becomes "Dr. Penn".
        public static string ShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            var parts = fullName.Trim().Split(' ');
            if (parts[0].EndsWith(".") && parts.Length > 1) return parts[0] + " " + parts[parts.Length - 1];
            return parts[0];
        }
    }
}
