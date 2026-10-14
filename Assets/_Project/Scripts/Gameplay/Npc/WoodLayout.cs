using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Where trees and rocks stand (pure, so that it can be tested). Playtest 2026-10-09: the forest trees were too regular and the edges of the maps were plain walls.
    // The forest has thickets and clearings (smooth noise decides how thick the wood is, a hash decides which cells hold a tree); the edge of an outdoor map is a
    // band of trees (or rocks on the beach), a few cells thick, with the ways out left open.
    public static class WoodLayout
    {
        public const int EdgeThickness = 2;

        static uint Hash(int x, int y, int seed)
        {
            unchecked
            {
                var h = (uint)(x * 374761393 + y * 668265263 + seed * 1274126177);
                h = (h ^ (h >> 13)) * 1274126177u;
                return h ^ (h >> 16);
            }
        }

        // 0..1, constant per cell and seed.
        public static float Random01(int x, int y, int seed) => (Hash(x, y, seed) & 0xFFFFFF) / (float)0x1000000;

        // Smooth value noise in 0..1 (cells `scale` apart are unrelated).
        public static float Noise(float x, float y, float scale, int seed)
        {
            var fx = x / scale;
            var fy = y / scale;
            var x0 = Mathf.FloorToInt(fx);
            var y0 = Mathf.FloorToInt(fy);
            var tx = fx - x0;
            var ty = fy - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            var a = Mathf.Lerp(Random01(x0, y0, seed), Random01(x0 + 1, y0, seed), tx);
            var b = Mathf.Lerp(Random01(x0, y0 + 1, seed), Random01(x0 + 1, y0 + 1, seed), tx);
            return Mathf.Lerp(a, b, ty);
        }

        // How thick the wood is around a cell: 0 (a clearing) to 1 (a thicket).
        public static float Thickness(int x, int y, int seed) => Noise(x, y, 7f, seed);

        // Does a forest cell hold a tree? Thickets are crowded, clearings nearly bare, and nothing lines up in rows.
        public static bool ForestTreeAt(int x, int y, int seed = 11)
        {
            var thick = Thickness(x, y, seed);
            var chance = thick > 0.62f ? 0.62f : thick < 0.32f ? 0.015f : Mathf.Lerp(0.04f, 0.3f, Mathf.InverseLerp(0.32f, 0.62f, thick));
            return Random01(x, y, seed + 1) < chance;
        }

        // A small offset from the middle of the cell, so that the trees do not stand on a grid either.
        public static Vector2 Jitter(int x, int y, int seed = 5) => new Vector2((Random01(x, y, seed) - 0.5f) * 0.6f, (Random01(x, y, seed + 7) - 0.5f) * 0.4f);

        public static bool InBand(int x, int y, int w, int h) => x < EdgeThickness || y < EdgeThickness || x >= w - EdgeThickness || y >= h - EdgeThickness;

        // The cell of the map's outer ring that a band cell lies behind (so that a way out through the ring stays open through the whole band).
        public static Vector2Int Border(int x, int y, int w, int h)
        {
            var dx = Math.Min(x, w - 1 - x);
            var dy = Math.Min(y, h - 1 - y);
            if (dx <= dy) return new Vector2Int(x < w - 1 - x ? 0 : w - 1, y);
            return new Vector2Int(x, y < h - 1 - y ? 0 : h - 1);
        }

        // The cells of the band that are closed (a tree or a rock stands there and nothing can walk through). `open` tells which cells of the outer ring are ways out.
        public static List<Vector2Int> BandCells(int w, int h, Func<int, int, bool> open)
        {
            var cells = new List<Vector2Int>();
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    if (!InBand(x, y, w, h)) continue;
                    var b = Border(x, y, w, h);
                    if (open != null && open(b.x, b.y)) continue;
                    cells.Add(new Vector2Int(x, y));
                }
            return cells;
        }

        // Does this closed band cell show a tree (a checkerboard with a few more, so it never reads as a pattern)? The others are plain collision under the crowns.
        public static bool BandShowsTree(int x, int y, int seed = 3) => (x + y) % 2 == 0 || Random01(x, y, seed) < 0.18f;

        // Playtest 2026-10-14: the farm began too large and bare. Choppable trees crowd in from the wood's edge (thick at the edge band, thinning over `FringeDepth`
        // cells, in clumps rather than a line) and a few small groves stand further in; nothing lines up.
        public const int FringeDepth = 12;

        public static float FarmTreeChance(int x, int y, int w, int h, int seed = 21)
        {
            var d = Math.Min(Math.Min(x, w - 1 - x), Math.Min(y, h - 1 - y)) - EdgeThickness;
            if (d < 0) return 0f;
            var fringe = Mathf.Clamp01(1f - d / (float)FringeDepth);
            var clump = Mathf.Lerp(0.35f, 1f, Noise(x, y, 6f, seed));
            var chance = 0.55f * fringe * fringe * clump;
            var c = CopseReach(x, y, seed);
            if (c < 1f) chance = Math.Max(chance, 0.75f * (1f - c * c * 0.6f));
            return Mathf.Clamp01(chance);
        }

        // The farm is derelict, reclaimed by nature (owner, 2026-10-14): copses of trees stand all over it. One copse per 12 x 12 block (most blocks), its centre
        // jittered in the block, radius 2 to 3.5 cells. Returns the distance to the nearest copse's centre in units of its radius (under 1 is inside it).
        public const int CopseBlock = 12;

        public static float CopseReach(int x, int y, int seed = 21)
        {
            var best = float.MaxValue;
            var bx = Mathf.FloorToInt(x / (float)CopseBlock);
            var by = Mathf.FloorToInt(y / (float)CopseBlock);
            for (var j = by - 1; j <= by + 1; j++)
                for (var i = bx - 1; i <= bx + 1; i++)
                {
                    if (Random01(i, j, seed + 31) > 0.8f) continue;
                    var cx = i * CopseBlock + 2f + Random01(i, j, seed + 32) * (CopseBlock - 4f);
                    var cy = j * CopseBlock + 2f + Random01(i, j, seed + 33) * (CopseBlock - 4f);
                    var r = 2f + Random01(i, j, seed + 34) * 1.5f;
                    var d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / r;
                    if (d < best) best = d;
                }
            return best;
        }

        // Undergrowth: bushes ring the copses and sprinkle the open ground, fallen logs lie by the trees and alone. Returns "bush", "log" or null.
        public static string DecorAt(int x, int y, int seed = 21)
        {
            var reach = CopseReach(x, y, seed);
            var roll = Random01(x, y, seed + 51);
            if (reach < 1f) return null;                      // the trees' own cells
            if (reach < 1.8f) return roll < 0.32f ? "bush" : roll < 0.4f ? "log" : null;
            return roll < 0.018f ? "bush" : roll < 0.03f ? "log" : null;
        }
    }
}
