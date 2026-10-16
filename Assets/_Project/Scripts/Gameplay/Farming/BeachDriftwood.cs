using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // The sea leaves driftwood on the beach (playtest 2026-10-16: the driftwood and rocks drawn on the sand could not be picked up and had no names, against the rule that
    // everything in the world can be named). Each day a random number of pieces lie on the sand near the water, between the fish stall and the water line, in random places;
    // the day before's pieces that were left are gone with the tide. An axe turns a piece into wood. Pure choices (the day and the world's seed decide), so that they can be tested.
    public static class BeachDriftwood
    {
        public const string NodeId = "driftwood";
        public const int MinPerDay = 2, MaxPerDay = 7;
        public const int StallRow = 11;          // the fish stall's row on the beach map (MapBuilder): nothing lies beyond it
        public const int ReachRows = 6;          // how far up the sand from the water line a piece may lie (the stall stands a little further up)

        // How many pieces the day brings.
        public static int CountFor(int day, int seed)
        {
            var span = MaxPerDay - MinPerDay + 1;
            return MinPerDay + Math.Min(span - 1, (int)(WeatherRoller.Unit(day * 1237 + 11, seed ^ 0x6D2B79F5) * span));
        }

        // The sand a piece may land on: for every column, the cells from the one above the highest water cell up `ReachRows` rows, never past `topRow` (the stall's row) and
        // never where something already is. `isWater` and `isSand` read the ground; `blocked` says what is taken.
        public static List<(int x, int y)> Candidates(int width, int height, Func<int, int, bool> isWater, Func<int, int, bool> isSand, Func<int, int, bool> blocked, int topRow)
        {
            var cells = new List<(int x, int y)>();
            for (var x = 1; x < width - 1; x++)
            {
                var line = -1;
                for (var y = 0; y < height; y++) if (isWater(x, y)) line = y;
                if (line < 0) continue;
                for (var y = line + 1; y <= Math.Min(line + ReachRows, topRow) && y < height; y++)
                    if (isSand(x, y) && !isWater(x, y) && !blocked(x, y)) cells.Add((x, y));
            }
            return cells;
        }

        // Where today's pieces lie: `count` different cells of the candidates, none beside another (so that it reads as pieces on a shore, not a heap).
        public static List<(int x, int y)> Choose(IReadOnlyList<(int x, int y)> candidates, int count, int day, int seed)
        {
            var ordered = candidates.OrderBy(c => c.y).ThenBy(c => c.x).ToList();
            var chosen = new List<(int x, int y)>();
            if (ordered.Count == 0) return chosen;
            for (var attempt = 0; attempt < count * 12 && chosen.Count < count; attempt++)
            {
                var pick = ordered[(int)(WeatherRoller.Unit(day * 7919 + attempt * 104729 + 3, seed ^ 0x1B873593) * ordered.Count) % ordered.Count];
                if (chosen.Any(c => Math.Abs(c.x - pick.x) <= 1 && Math.Abs(c.y - pick.y) <= 1)) continue;
                chosen.Add(pick);
            }
            return chosen;
        }

        // Takes the old pieces off the grid and lays today's. Returns how many were laid.
        public static int Refresh(NodeGrid grid, ResourceNodeDefinition driftwood, IReadOnlyList<(int x, int y)> candidates, int day, int seed)
        {
            if (grid == null || driftwood == null) return 0;
            foreach (var old in grid.Nodes.Where(n => n.TypeId == NodeId).ToList()) grid.Remove(old.X, old.Y);
            var laid = 0;
            foreach (var (x, y) in Choose(candidates, CountFor(day, seed), day, seed))
                if (grid.Add(x, y, driftwood)) laid++;
            return laid;
        }
    }
}
