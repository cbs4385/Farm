using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Farm.Core
{
    // Keeps player, farm and pet names clean (T-145): a chat that suggests names must not be able to put slurs or worse on a
    // stream. The check is forgiving about disguises (digits for letters, spaces and dots between letters) and careful not to
    // block innocent names (Scunthorpe, Assassin, Dickens). The list is a plain text file, Resources/Filters/blocked_names.txt:
    //   word   blocks the word wherever it stands alone in the name
    //   ~stem  blocks any name that contains the stem once spaces and punctuation are removed (only for stems no ordinary word contains)
    // It is a seed list of common profanity; extend it with a vetted list before release (docs/narrative/STREAMING.md).
    public static class NameFilter
    {
        public const string ResourcePath = "Filters/blocked_names";

        static HashSet<string> _words;
        static List<string> _stems;

        public static void SetBlocklist(IEnumerable<string> lines)
        {
            _words = new HashSet<string>(StringComparer.Ordinal);
            _stems = new List<string>();
            foreach (var raw in lines ?? Enumerable.Empty<string>())
            {
                var line = (raw ?? string.Empty).Trim().ToLowerInvariant();
                if (line.Length == 0 || line[0] == '#') continue;
                if (line[0] == '~') { if (line.Length > 1) _stems.Add(line.Substring(1)); }
                else _words.Add(line);
            }
        }

        static void EnsureLoaded()
        {
            if (_words != null) return;
            var asset = Resources.Load<TextAsset>(ResourcePath);
            SetBlocklist(asset != null ? asset.text.Replace("\r", "").Split('\n') : new string[0]);
        }

        // Digits and symbols people use for letters, lower case, nothing else changed.
        public static string Normalize(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text.ToLowerInvariant())
            {
                switch (c)
                {
                    case '0': sb.Append('o'); break;
                    case '1': case '!': case '|': sb.Append('i'); break;
                    case '3': sb.Append('e'); break;
                    case '4': case '@': sb.Append('a'); break;
                    case '5': case '$': sb.Append('s'); break;
                    case '7': sb.Append('t'); break;
                    case '8': sb.Append('b'); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        public static bool IsAllowed(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            EnsureLoaded();
            var normalized = Normalize(name);

            // Whole words, as typed ("Big Dick Farm") ...
            var words = new List<string>();
            var current = new StringBuilder();
            foreach (var c in normalized)
            {
                if (char.IsLetter(c)) current.Append(c);
                else if (current.Length > 0) { words.Add(current.ToString()); current.Clear(); }
            }
            if (current.Length > 0) words.Add(current.ToString());
            if (words.Any(_words.Contains)) return false;

            // ... the letters run together ("d i c k", "d.i.c.k") ...
            var squashed = new string(normalized.Where(char.IsLetter).ToArray());
            if (squashed.Length >= 3 && words.Count > 1 && _words.Any(w => w.Length >= 4 && squashed == w)) return false;
            if (_words.Contains(squashed)) return false;

            // ... and stems that no innocent word contains.
            return !_stems.Any(stem => squashed.Contains(stem));
        }

        // The name itself when it is clean, otherwise the fallback.
        public static string Sanitize(string name, string fallback) =>
            string.IsNullOrWhiteSpace(name) || !IsAllowed(name) ? fallback : name.Trim();

        public static void ResetForTests() { _words = null; _stems = null; }
    }
}
