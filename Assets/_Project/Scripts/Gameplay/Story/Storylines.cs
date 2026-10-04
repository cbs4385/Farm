using System;
using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // A small village story that some games have and others do not (T-143). Story JSON: `storylines`. Its content is ordinary
    // dialogue, topics and reactions that carry the condition `storyline:<id>`.
    [Serializable]
    public sealed class StorylineDefinition
    {
        public string Id;
        public float Weight = 1f;            // a higher weight is drawn more often
        public string TitleKey;              // optional, for the journal and reports
        public List<string> Variants = new List<string>();     // optional: one is drawn per game, as the flag `storyline.<id>.<variant>` (for example who the whistler is)
    }

    // Each new game draws a few storylines from the whole set, from the game's own seed, so two games differ and a game never
    // changes its mind (the draw is stored as the flags `storyline.<id>`, which the save already keeps).
    public static class Storylines
    {
        public const int PerGame = 4;
        public const string FlagPrefix = "storyline.";

        // Weighted draw without replacement: deterministic for a seed.
        public static List<string> Draw(IEnumerable<StorylineDefinition> all, int seed, int count)
        {
            var pool = all.Where(s => s != null && !string.IsNullOrEmpty(s.Id) && s.Weight > 0f).OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
            var chosen = new List<string>();
            for (var round = 0; round < count && pool.Count > 0; round++)
            {
                var total = pool.Sum(s => s.Weight);
                var roll = DialogueSet.Unit(seed * 31 + round * 7919 + 17) * total;
                var picked = pool[pool.Count - 1];
                foreach (var s in pool)
                {
                    roll -= s.Weight;
                    if (roll < 0f) { picked = s; break; }
                }
                chosen.Add(picked.Id);
                pool.Remove(picked);
            }
            return chosen;
        }

        // The variant a game gets for a storyline, from the game's seed: deterministic, null when the storyline has none.
        public static string PickVariant(StorylineDefinition def, int seed)
        {
            if (def?.Variants == null || def.Variants.Count == 0) return null;
            var ordered = def.Variants.OrderBy(v => v, StringComparer.Ordinal).ToList();
            var index = (int)(DialogueSet.Unit(seed * 131 + NpcInteractions.StableHash(def.Id) + 29) * ordered.Count);
            return ordered[Math.Min(index, ordered.Count - 1)];
        }

        // Called when a game starts: sets `storyline.<id>` for the drawn ones. Does nothing for a game that already has any.
        public static List<string> Assign(GameSession s)
        {
            if (s.Story == null) return new List<string>();
            var existing = s.State.Flags.Where(f => f.StartsWith(FlagPrefix, StringComparison.Ordinal)).ToList();
            if (existing.Count > 0) return existing.Select(f => f.Substring(FlagPrefix.Length)).Where(f => f.IndexOf('.') < 0).ToList();
            var drawn = Draw(s.Story.Storylines, s.State.WorldSeed, PerGame);
            foreach (var id in drawn)
            {
                s.SetFlag(FlagPrefix + id);
                var variant = PickVariant(s.Story.Storylines.FirstOrDefault(d => d.Id == id), s.State.WorldSeed);
                if (variant != null) s.SetFlag($"{FlagPrefix}{id}.{variant}");
            }
            return drawn;
        }
    }
}
