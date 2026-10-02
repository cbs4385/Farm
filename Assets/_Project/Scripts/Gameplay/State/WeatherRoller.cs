using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Looks weather definitions up by id. Built from the game database (core assets plus module packs); when the
    // database has none, the built-in defaults are used so tests and bare projects still work.
    public sealed class WeatherCatalog
    {
        static WeatherCatalog _builtIn;
        readonly Dictionary<string, WeatherDefinition> _byId = new Dictionary<string, WeatherDefinition>();

        public static WeatherCatalog BuiltIn => _builtIn ?? (_builtIn = new WeatherCatalog(WeatherDefaults.CreateAll()));

        public WeatherCatalog(IEnumerable<WeatherDefinition> definitions)
        {
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id)) _byId[d.Id] = d;
        }

        public static WeatherCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllWeather.ToList() : new List<WeatherDefinition>();
            return all.Count == 0 ? BuiltIn : new WeatherCatalog(all);
        }

        public IEnumerable<WeatherDefinition> All => _byId.Values;
        public IEnumerable<string> Ids => _byId.Keys.OrderBy(k => k, StringComparer.Ordinal);
        public bool Contains(string id) => id != null && _byId.ContainsKey(id);

        // An id without a definition behaves like plain weather rather than failing.
        public WeatherDefinition Get(string id) =>
            id != null && _byId.TryGetValue(id, out var d) ? d : WeatherDefinition.Neutral(id);
    }

    // The odds for one day's roll, by weather id. Weight modifiers (hooks) reshape it: a module can make worse
    // weather likelier as dread rises, or add a weather of its own.
    public sealed class WeatherWeights
    {
        readonly SortedDictionary<string, float> _weights = new SortedDictionary<string, float>(StringComparer.Ordinal);

        public IEnumerable<KeyValuePair<string, float>> Entries => _weights;
        public float Get(string id) => id != null && _weights.TryGetValue(id, out var w) ? w : 0f;
        public void Set(string id, float weight) { if (!string.IsNullOrEmpty(id)) _weights[id] = Math.Max(0f, weight); }
        public void Add(string id, float weight) => Set(id, Get(id) + weight);
        public void Scale(string id, float factor) { if (_weights.ContainsKey(id)) _weights[id] = Math.Max(0f, _weights[id] * factor); }
        public float Total => _weights.Values.Sum();
    }

    // Deterministic, weighted weather: the same save seed and date always give the same weather.
    public static class WeatherRoller
    {
        // The first days are always sunny so a new player learns the loop first.
        public const int SunnyUntilTotalDay = 2;

        public static WeatherWeights BuildWeights(GameDateTime date, WeatherCatalog catalog, GameHooks hooks, GameState state)
        {
            var weights = new WeatherWeights();
            foreach (var def in catalog.All) weights.Set(def.Id, def.WeightFor((int)date.Season));
            hooks?.AdjustWeatherWeights(date, weights, state);
            return weights;
        }

        public static string Roll(GameDateTime date, WeatherCatalog catalog, int seed, GameHooks hooks = null, GameState state = null)
        {
            if (date.TotalDays < SunnyUntilTotalDay) return WeatherDefaults.Sunny;
            var weights = BuildWeights(date, catalog, hooks, state);
            var total = weights.Total;
            if (total <= 0f) return WeatherDefaults.Sunny;

            var pick = Unit(date.TotalDays, seed) * total;
            string last = WeatherDefaults.Sunny;
            foreach (var entry in weights.Entries)
            {
                if (entry.Value <= 0f) continue;
                last = entry.Key;
                if (pick < entry.Value) return entry.Key;
                pick -= entry.Value;
            }
            return last;
        }

        // A well-mixed hash of the day and the save seed, as a number in [0, 1).
        public static float Unit(int day, int seed)
        {
            var h = unchecked((uint)day * 2654435761u + (uint)seed * 40503u + 0x9E3779B9u);
            h ^= h >> 15; h = unchecked(h * 2246822519u);
            h ^= h >> 13; h = unchecked(h * 3266489917u);
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
