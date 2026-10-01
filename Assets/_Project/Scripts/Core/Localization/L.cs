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
            if (args == null || args.Length == 0) return text;
            try { return string.Format(text, args); }
            catch (FormatException) { return text; }
        }
    }
}
