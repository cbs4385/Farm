using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Looks spawn tables up by map (the database's, or the built-in tables when it has none).
    public sealed class SpawnCatalog
    {
        static SpawnCatalog _builtIn;
        readonly List<SpawnTableDefinition> _all = new List<SpawnTableDefinition>();

        public static SpawnCatalog BuiltIn => _builtIn ?? (_builtIn = new SpawnCatalog(BuiltInAssets.Keep(ForageDefaults.CreateTables())));

        public SpawnCatalog(IEnumerable<SpawnTableDefinition> tables) { _all.AddRange(tables.Where(t => t != null)); }

        public static SpawnCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllSpawnTables.ToList() : new List<SpawnTableDefinition>();
            return all.Count == 0 ? BuiltIn : new SpawnCatalog(all);
        }

        public IReadOnlyList<SpawnTableDefinition> All => _all;
        public IEnumerable<SpawnTableDefinition> For(string mapId) => _all.Where(t => t.MapId == mapId);
    }

    // Puts new things on the ground from a spawn table, the same way for the same seed and day.
    public static class SpawnModel
    {
        // Runs one morning of a table. `candidates` are the free-looking cells the scene offers (any order is fine,
        // they are sorted here). Returns how many nodes were placed.
        public static int Spawn(NodeGrid grid, SpawnTableDefinition table, IEnumerable<(int x, int y)> candidates, NodeCatalog nodes,
            IWorldQuery world, Season season, float luck, int seed, int day)
        {
            var cells = candidates.OrderBy(c => c.y).ThenBy(c => c.x).ToList();
            if (cells.Count == 0) return 0;

            var entries = table.Entries
                .Where(e => e.Seasons.Includes(season) && nodes.Get(e.NodeId) != null && Holds(e, world))
                .Select(e => (entry: e, weight: e.Weight * (e.LuckSensitive ? Math.Max(0f, 1f + luck) : 1f)))
                .Where(p => p.weight > 0f)
                .ToList();
            if (entries.Count == 0) return 0;
            var total = entries.Sum(p => p.weight);

            var kinds = new HashSet<string>(table.Entries.Select(e => e.NodeId));
            var present = grid.Nodes.Count(n => kinds.Contains(n.TypeId));
            if (table.Spreads)
            {
                // Plants spread from the ones still standing: a kind that is on the map comes back more readily, one picked clean only rarely.
                var standing = new HashSet<string>(grid.Nodes.Select(n => n.TypeId));
                entries = entries.Select(p => (p.entry, weight: p.weight * (standing.Contains(p.entry.NodeId) ? ForageRules.StandingBoost : ForageRules.PickedCleanFactor))).ToList();
                total = entries.Sum(p => p.weight);
            }
            var salt = StableHash(table.Id);
            var placed = 0;

            for (var attempt = 0; attempt < table.DailyAttempts && present < table.MaxNodes; attempt++)
            {
                var start = (int)(WeatherRoller.Unit(day * 7919 + attempt * 104729 + salt, seed) * cells.Count) % cells.Count;
                var pick = WeatherRoller.Unit(day * 31337 + attempt * 8191 + salt, seed ^ 0x2545F491) * total;

                ResourceNodeDefinition chosen = null;
                foreach (var (entry, weight) in entries)
                {
                    if (pick < weight) { chosen = nodes.Get(entry.NodeId); break; }
                    pick -= weight;
                }
                if (chosen == null) continue;

                // The cell may be taken: try the next few.
                for (var step = 0; step < 4; step++)
                {
                    var (x, y) = cells[(start + step) % cells.Count];
                    if (grid.Add(x, y, chosen)) { placed++; present++; break; }
                }
            }
            return placed;
        }

        static bool Holds(SpawnEntry e, IWorldQuery world) =>
            string.IsNullOrWhiteSpace(e.Condition) || (world != null && Conditions.TryEvaluate(e.Condition, world, out var ok) && ok);

        static int StableHash(string s)
        {
            unchecked
            {
                var h = 23;
                foreach (var c in s ?? string.Empty) h = h * 31 + c;
                return h;
            }
        }
    }

    // How good a foraged thing is: the Foraging level and luck raise the chance of a better quality.
    public static class ForageModel
    {
        public const int Normal = 0, Silver = 1, Gold = 2;

        public static float GoldChance(int level, float luck) => 0.015f * (Math.Max(1, level) - 1) + 0.10f * Math.Max(0f, luck);
        public static float SilverChance(int level, float luck) => 0.05f * (Math.Max(1, level) - 1) + 0.15f * Math.Max(0f, luck);

        // `roll` is a number in [0, 1).
        public static int Quality(int level, float luck, float roll)
        {
            var gold = GoldChance(level, luck);
            if (roll < gold) return Gold;
            return roll < gold + SilverChance(level, luck) ? Silver : Normal;
        }
    }
}
