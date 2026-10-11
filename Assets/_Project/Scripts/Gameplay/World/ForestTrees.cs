using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The trees of the forest's thickets are trees you can chop (playtest 2026-10-11: "why can I not chop any of the trees in the forest"). They used to be pictures in the
    // map's scene; they are resource nodes now, put in the first time the forest is visited (WoodLayout decides where), so a tree felled stays felled and leaves a stump.
    // The band of trees round the edge of the map is still scenery.
    public static class ForestTrees
    {
        public const int Width = 40, Height = 30, PathX0 = 18, PathX1 = 20;
        public const string SeededFlag = "forest.trees.seeded";

        // The cells of the thickets: not the path, the pond and its shore, or the way to the mine.
        public static IEnumerable<(int x, int y)> Cells()
        {
            for (var y = 3; y < Height - WoodLayout.EdgeThickness; y++)
                for (var x = WoodLayout.EdgeThickness; x < Width - WoodLayout.EdgeThickness; x++)
                {
                    if (x >= PathX0 - 2 && x <= PathX1 + 2) continue;
                    if (x >= 2 && x <= 16 && y >= 3 && y <= 15) continue;
                    if (x >= 21 && x <= 36 && y >= 3 && y <= 7) continue;
                    if (!WoodLayout.ForestTreeAt(x, y)) continue;
                    yield return (x, y);
                }
        }

        public const string LastGrowthVar = "forest.trees.day";          // the day (counted from 1) trees were last grown
        public const int MostPerVisit = 3;                              // however long the player was away, at most this many days of growth are added

        // Pure: the thicket cell that grows a tree on a given day, chosen from the empty ones, or -1 when there is none.
        public static int GrowthChoice(int emptyCells, int day, int round)
        {
            if (emptyCells <= 0) return -1;
            return (int)(WoodLayout.Random01(day, round, 57) * emptyCells) % emptyCells;
        }

        // The forest slowly grows its trees back: one a day, in a place where a thicket was (an empty cell, never on a planted sapling or a stump), for as many days as
        // have passed since the last visit, up to MostPerVisit. Returns how many were grown.
        public static int Regrow(GameSession session, int today)
        {
            var tree = session.Nodes.Get(NodeDefaults.Tree);
            if (tree == null) return 0;
            var last = session.GetVar(LastGrowthVar) - 1;
            session.SetVar(LastGrowthVar, today + 1);
            if (last < 0) return 0;                                       // the first visit: the trees were just seeded
            var rounds = System.Math.Min(MostPerVisit, today - last);
            var nodes = session.GetNodes(MapIds.Forest);
            var grown = 0;
            for (var r = 0; r < rounds; r++)
            {
                var empty = new List<(int x, int y)>();
                foreach (var c in Cells()) if (!nodes.Has(c.x, c.y)) empty.Add(c);
                var pick = GrowthChoice(empty.Count, today - r, r);
                if (pick < 0) break;
                if (nodes.Add(empty[pick].x, empty[pick].y, tree)) grown++;
            }
            return grown;
        }

        // Puts the trees in, once for a game (the flag), so that felled ones are not replanted when the player comes back.
        public static int Seed(GameSession session)
        {
            if (session.HasFlag(SeededFlag)) return 0;
            session.SetFlag(SeededFlag);
            session.SetVar(LastGrowthVar, session.Clock.Now.TotalDays + 1);
            var tree = session.Nodes.Get(NodeDefaults.Tree);
            if (tree == null) return 0;
            var nodes = session.GetNodes(MapIds.Forest);
            var added = 0;
            foreach (var (x, y) in Cells())
                if (nodes.Add(x, y, tree)) added++;
            return added;
        }
    }
}
