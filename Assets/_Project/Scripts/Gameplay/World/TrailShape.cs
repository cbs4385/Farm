using System;
using UnityEngine;

namespace Farm.Gameplay
{
    // The cells of a trail that runs up a map (pure, so that it can be tested). Playtest 2026-10-15: "the paths in the forest are too regular and look like roads". A
    // straight column of the same width is replaced by a trail that wanders to either side and is wider in one place and narrower in another; the two ends stay where the
    // doors, spawn points and gates are. Every row overlaps the one before it, so the trail is always one walkable piece. TrailEdges softens its outline.
    public static class TrailShape
    {
        public const int MinWidth = 2;

        // The first and last cell of the trail on each row 0..rows-1. `x0`..`x1` is the straight trail it replaces (the cells that stay at both ends, for `endRows` rows);
        // `minX`..`maxX` the corridor it may wander in.
        public static (int x0, int x1)[] Build(int rows, int x0, int x1, int minX, int maxX, int seed, int endRows = 4)
        {
            var spans = new (int x0, int x1)[rows];
            var centre = (x0 + x1 + 1) * 0.5f;                       // the cell's left edge is its x, so a trail of cells 18..20 is centred on 19.5
            var half = (x1 - x0 + 1) * 0.5f;
            for (var y = 0; y < rows; y++)
            {
                var toEnd = Math.Min(y, rows - 1 - y);
                if (toEnd < endRows) { spans[y] = (x0, x1); continue; }
                var t = Mathf.SmoothStep(0f, 1f, (toEnd - endRows) / 4f);          // the wander grows from nothing at the ends
                var sway = (WoodLayout.Noise(y, 0f, 9f, seed) - 0.5f) * 2f + (WoodLayout.Noise(y, 0f, 3.5f, seed + 3) - 0.5f) * 0.8f;
                var c = centre + sway * 2.2f * t;
                var left = half + (WoodLayout.Noise(y, 0f, 4f, seed + 5) - 0.45f) * 2.4f * t;
                var right = half + (WoodLayout.Noise(y, 0f, 4f, seed + 9) - 0.45f) * 2.4f * t;
                var a = Mathf.RoundToInt(c - left);
                var b = Mathf.RoundToInt(c + right) - 1;
                a = Math.Max(a, minX);
                b = Math.Min(b, maxX);
                if (b - a + 1 < MinWidth)
                {
                    if (a + MinWidth - 1 <= maxX) b = a + MinWidth - 1; else { b = maxX; a = maxX - MinWidth + 1; }
                }
                var (pa, pb) = spans[y - 1];                          // overlap the row before (rows below endRows are never the first)
                if (a > pb) a = pb;
                if (b < pa) b = pa;
                spans[y] = (a, b);
            }
            return spans;
        }
    }
}
