using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Farm.Core
{
    // All user-facing text goes through L.Get(key). Strings live in Resources/Localization/<lang>.json
    // (flat key -> text). Missing keys return the key itself and log once, so gaps are obvious but never crash.
    // Text may contain {0}, {1} ... placeholders filled from the args.
    public static class L
    {
        public const string DefaultLanguage = "en";
        const string ResourceFolder = "Localization/";

        static Dictionary<string, string> _table = new Dictionary<string, string>();
        static readonly HashSet<string> Reported = new HashSet<string>();

        // Strings added by modules (e.g. extra weather names), re-applied whenever the language changes.
        static readonly Dictionary<string, Dictionary<string, string>> ExtraTables = new Dictionary<string, Dictionary<string, string>>();

        // Post-processing of every looked-up string: key and text in, text out. Used for effects such as
        // text corruption at high dread. Filters run in registration order and must be cheap.
        static readonly List<Func<string, string, string>> Filters = new List<Func<string, string, string>>();

        public static void AddFilter(Func<string, string, string> filter)
        {
            if (filter != null && !Filters.Contains(filter)) Filters.Add(filter);
        }

        public static void RemoveFilter(Func<string, string, string> filter) => Filters.Remove(filter);

        // Adds (or overrides) strings for one language. Safe to call before or after SetLanguage.
        public static void AddTable(string language, Dictionary<string, string> table)
        {
            if (!ExtraTables.TryGetValue(language, out var existing)) ExtraTables[language] = existing = new Dictionary<string, string>();
            foreach (var kv in table) existing[kv.Key] = kv.Value;
            if (language == Language || language == DefaultLanguage)
                foreach (var kv in table) _table[kv.Key] = kv.Value;
        }

        public static string Language { get; private set; } = DefaultLanguage;
        public static int EntryCount => _table.Count;

        public static event Action LanguageChanged;

        public static void SetLanguage(string language)
        {
            Language = string.IsNullOrEmpty(language) ? DefaultLanguage : language;
            var asset = Resources.Load<TextAsset>(ResourceFolder + Language)
                        ?? Resources.Load<TextAsset>(ResourceFolder + DefaultLanguage);
            _table = asset != null ? Parse(asset.text) : new Dictionary<string, string>();
            if (asset == null) Log.Error($"Localization table '{Language}' not found.");
            foreach (var lang in new[] { DefaultLanguage, Language })
                if (ExtraTables.TryGetValue(lang, out var extra))
                    foreach (var kv in extra) _table[kv.Key] = kv.Value;
            Reported.Clear();
            LanguageChanged?.Invoke();
        }

        public static Dictionary<string, string> Parse(string json) =>
            JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        // Used by tests and tools.
        public static void SetTable(Dictionary<string, string> table)
        {
            _table = table;
            Reported.Clear();
        }

        public static bool Has(string key) => _table.ContainsKey(key);

        public static string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (!_table.TryGetValue(key, out var text))
            {
                if (Reported.Add(key)) Log.Warn($"Missing localization key '{key}' ({Language}).");
                return key;
            }
            if (args != null && args.Length > 0)
            {
                try { text = string.Format(text, args); }
                catch (FormatException) { /* keep the unformatted text */ }
            }
            for (var i = 0; i < Filters.Count; i++) text = Filters[i](key, text);
            return text;
        }
    }
}
