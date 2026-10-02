using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Seasonal random events (T-041): each morning there is a chance that something happens overnight (a windfall, a loss,
    // crows in the field). The event is drawn from a weighted table: weights are shifted by the player's luck (good events
    // likelier when lucky, bad ones when unlucky) and by IEventWeightModifier hooks (dread). Everything is deterministic
    // for a given world seed and day, so it can be tested and replayed.
    public static class RandomEvents
    {
        public const float DailyChance = 0.35f;

        public static bool InSeason(RandomEventDefinition def, Season season)
        {
            if (string.IsNullOrWhiteSpace(def.Seasons)) return true;
            return def.Seasons.Split(',').Any(s => string.Equals(s.Trim(), season.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        // The weight an event has today, after luck and hooks. Good events gain weight with good luck and lose it with bad
        // luck; bad events the other way around; neutral ones are unchanged.
        public static float Weight(RandomEventDefinition def, float luck, GameState state, GameHooks hooks)
        {
            var w = def.Weight;
            if (def.Mood == "good") w *= 1f + luck;
            else if (def.Mood == "bad") w *= 1f - luck;
            w = Math.Max(0f, w);
            return hooks != null ? hooks.ComputeEventWeight(def, w, state) : w;
        }

        // Picks one of the candidates by weight; `roll` is in [0, 1). Null when no candidate has any weight.
        public static RandomEventDefinition Pick(IEnumerable<RandomEventDefinition> candidates, float luck, GameState state, GameHooks hooks, float roll)
        {
            var weighted = candidates.Select(c => (def: c, weight: Weight(c, luck, state, hooks))).Where(p => p.weight > 0f).ToList();
            var total = weighted.Sum(p => p.weight);
            if (total <= 0f) return null;
            var at = roll * total;
            foreach (var (def, weight) in weighted)
            {
                if (at < weight) return def;
                at -= weight;
            }
            return weighted[weighted.Count - 1].def;
        }

        // The first week is quiet, so a new player learns the loop first.
        public const int QuietDays = 7;

        public static bool HappensToday(GameState state, int today) =>
            today >= QuietDays && WeatherRoller.Unit(today * 41 + 13, state.WorldSeed) < DailyChance;

        // Called each morning. Runs the event's effects and adds a line to the day summary.
        public static RandomEventDefinition Draw(GameSession s, DaySummary summary)
        {
            if (!s.InGame || s.Story == null) return null;
            var today = s.Clock.Now.TotalDays;
            if (!HappensToday(s.State, today)) return null;

            var season = s.Clock.Now.Season;
            var candidates = s.Story.RandomEvents
                .Where(e => InSeason(e, season) && Conditions.TryEvaluate(e.Condition, s.World, out var ok) && ok)
                .OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            var picked = Pick(candidates, s.Luck, s.State, s.Hooks, WeatherRoller.Unit(today * 53 + 29, s.State.WorldSeed ^ 0x5DEECE6));
            if (picked == null) return null;

            Effects.RunAll(s, picked.Effects);
            summary?.Notes.Add(new SummaryNote(picked.TextKey, new object[0]));
            s.Publish(new RandomEventHappened(picked.Id));
            return picked;
        }
    }

    public readonly struct RandomEventHappened
    {
        public readonly string EventId;
        public RandomEventHappened(string eventId) { EventId = eventId; }
    }

    // Crows eat crops that no scarecrow protects (the `crows:<n>` effect).
    public static class Crows
    {
        public static int Strike(GameSession s, int count)
        {
            var candidates = new List<(string map, FarmTile tile)>();
            foreach (var pair in s.Grids.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                if (pair.Key == MapIds.Greenhouse) continue;            // indoors
                var objects = s.GetObjects(pair.Key).All;
                foreach (var tile in pair.Value.Tiles.OrderBy(t => t.X).ThenBy(t => t.Y))
                    if (tile.Crop != null && !Sprinklers.Protected(objects, s.Placeables, tile.X, tile.Y)) candidates.Add((pair.Key, tile));
            }
            var eaten = 0;
            var day = s.Clock.Now.TotalDays;
            for (var i = 0; i < count && candidates.Count > 0; i++)
            {
                var pick = (int)(WeatherRoller.Unit(day * 19 + i * 7 + 1, s.State.WorldSeed ^ 0x1B873593) * candidates.Count) % candidates.Count;
                var (map, tile) = candidates[pick];
                s.Grids[map].ClearCrop(tile.X, tile.Y);
                candidates.RemoveAt(pick);
                eaten++;
            }
            return eaten;
        }
    }
}
