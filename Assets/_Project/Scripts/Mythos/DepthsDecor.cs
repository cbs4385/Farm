using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;

namespace Farm.Mythos
{
    public enum DepthsKind { Vein, LittleEyes, Eye, Urn, Statue, ColumnShort, ColumnTall, Arc, BookAltar, FloorPatch }

    // One piece of the dressing of the deep mine: what, where (a cell), which picture of its kind, and whether it blocks the way.
    public readonly struct DepthsPiece
    {
        public readonly DepthsKind Kind;
        public readonly int X, Y, Variant;
        public readonly bool Solid;

        public DepthsPiece(DepthsKind kind, int x, int y, int variant, bool solid = false) { Kind = kind; X = x; Y = y; Variant = variant; Solid = solid; }
    }

    // How the horror layer dresses the mine from the floor of the relic chest (30) down to the last (40) with the pictures of "Crawling Depths", scaled by the intensity
    // setting, by how deep the floor is and by the player's dread: more of it, and stranger, as any of the three goes up. Pure (a floor in, a list of pieces out), so it can be
    // tested without a scene; MythosDepths builds the list into the scene.
    //   horror off:  nothing, the mine is the plain mine.
    //   mild:        veins, urns, short columns and a cracked floor round the chest, at half strength; no eyes, statues, arcs or altar.
    //   full:        everything: eyes in the wall, statues and arcs in niches (in the corners of the boss hall), the book altar and two statues round the chest.
    public static class DepthsDecor
    {
        public const int FirstFloor = MythosRelics.Floor;

        // 0 (nothing) to 1 (the deepest floor at full dread, full intensity). The floor and dread can each hold it back, but never cancel it: 0.35 at the first floor with no dread.
        public static float Intensity(int level, int floor, int dread)
        {
            if (level <= 0 || floor < FirstFloor) return 0f;
            var depth = Math.Min(1f, (floor - FirstFloor) / (float)(MineGenerator.Floors - FirstFloor));
            var fear = Math.Max(0f, Math.Min(1f, dread / (float)DreadModel.Max));
            var scale = level == 1 ? 0.5f : 1f;
            return scale * (0.35f + 0.35f * depth + 0.3f * fear);
        }

        public static int Count(float intensity, float perUnit) => (int)Math.Round(intensity * perUnit, MidpointRounding.AwayFromZero);

        public static List<DepthsPiece> Plan(MineFloor f, int worldSeed, int level, int dread)
        {
            var plan = new List<DepthsPiece>();
            var k = Intensity(level, f.Floor, dread);
            if (k <= 0f) return plan;
            var full = level >= 2;
            var rng = new DetRandom(unchecked(worldSeed * 7 + f.Floor * 104729 + 0x5EED));
            var taken = MythosRelics.Taken(f);
            var used = new HashSet<(int, int)>();               // wall cells already under a piece
            var shrine = new HashSet<(int, int)>();             // open cells the shrine round the chest has used

            if (f.Floor == FirstFloor) Shrine(f, worldSeed, full, ref rng, taken, shrine, plan);

            var niches = new List<(int x, int y)>();            // a wall cell with open floor south of it and wall either side and two cells north: a big piece stands in it
            var faces = new List<(int x, int y)>();             // any wall cell with open floor next to it
            var edges = new List<(int x, int y)>();             // open cells beside a wall, free of ore, monsters and stairs
            for (var y = 1; y < f.Height - 1; y++)
                for (var x = 1; x < f.Width - 1; x++)
                {
                    if (f.IsWall(x, y))
                    {
                        if (TouchesOpen(f, x, y)) faces.Add((x, y));
                        if (!f.IsWall(x, y - 1) && f.IsWall(x - 1, y) && f.IsWall(x + 1, y) && y + 2 < f.Height && f.IsWall(x, y + 1) && f.IsWall(x, y + 2)
                            && f.IsWall(x - 1, y + 1) && f.IsWall(x + 1, y + 1) && f.IsWall(x - 1, y + 2) && f.IsWall(x + 1, y + 2)) niches.Add((x, y));
                    }
                    else if (!taken.Contains((x, y)) && !shrine.Contains((x, y)) && (f.IsWall(x - 1, y) || f.IsWall(x + 1, y) || f.IsWall(x, y + 1) || f.IsWall(x, y - 1))) edges.Add((x, y));
                }
            Shuffle(niches, ref rng); Shuffle(faces, ref rng); Shuffle(edges, ref rng);

            // Big pieces first, each reserving the 3 x 3 wall cells it covers so that nothing is drawn through it.
            void Niche(DepthsKind kind, int count, int variants)
            {
                for (var i = 0; i < niches.Count && count > 0; i++)
                {
                    var (x, y) = niches[i];
                    if (Overlaps(used, x, y)) continue;
                    Reserve(used, x, y);
                    plan.Add(new DepthsPiece(kind, x, y, rng.Range(0, variants)));
                    count--;
                }
            }
            Niche(DepthsKind.ColumnShort, Count(k, 2.5f), 2);
            if (full)
            {
                Niche(DepthsKind.Statue, Count(k, 3f), 6);
                Niche(DepthsKind.ColumnTall, Count(k, 2f), 2);
                Niche(DepthsKind.Arc, Count(k, 1.5f), 2);
            }

            // The boss floor is one open hall with walls only two cells thick (too thin for a niche): its statues stand in the corners of the hall instead.
            if (full && f.IsBoss)
            {
                var corners = new List<(int x, int y)> { (3, 3), (f.Width - 4, 3), (3, f.Height - 4), (f.Width - 4, f.Height - 4) };
                Shuffle(corners, ref rng);
                foreach (var (x, y) in corners.Take(Count(k, 3f)))
                    if (!f.IsWall(x, y) && !taken.Contains((x, y))) plan.Add(new DepthsPiece(DepthsKind.Statue, x, y, rng.Range(0, 6), true));
            }

            // Small pieces on the wall face, never on a cell a big piece covers.
            void Face(DepthsKind kind, int count, int variants)
            {
                for (var i = 0; i < faces.Count && count > 0; i++)
                {
                    var (x, y) = faces[i];
                    if (used.Contains((x, y))) continue;
                    used.Add((x, y));
                    plan.Add(new DepthsPiece(kind, x, y, rng.Range(0, variants)));
                    count--;
                }
            }
            Face(DepthsKind.Vein, Count(k, 16f), 4);
            if (full)
            {
                Face(DepthsKind.LittleEyes, Count(k, 10f), 2);
                if (k >= 0.6f) Face(DepthsKind.Eye, Count(k, 2f), 2);
            }

            for (var i = 0; i < edges.Count && i < Count(k, 5f); i++) plan.Add(new DepthsPiece(DepthsKind.Urn, edges[i].x, edges[i].y, rng.Range(0, 2)));
            return plan;
        }

        // The place of the relic chest, dressed: cracked floor within two steps, an urn or two beside it and, at full, two statues and the book altar standing in the cave
        // round it. The solid pieces are only set down where the floor stays in one piece (the chest is solid too).
        static void Shrine(MineFloor f, int worldSeed, bool full, ref DetRandom rng, HashSet<(int, int)> taken, HashSet<(int, int)> shrine, List<DepthsPiece> plan)
        {
            var chest = MythosRelics.ChestCell(f, worldSeed);
            if (!chest.HasValue) return;
            var (cx, cy) = chest.Value;
            for (var dy = -2; dy <= 2; dy++)
                for (var dx = -2; dx <= 2; dx++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) > 2 || f.IsWall(cx + dx, cy + dy) || taken.Contains((cx + dx, cy + dy))) continue;
                    plan.Add(new DepthsPiece(DepthsKind.FloorPatch, cx + dx, cy + dy, rng.Range(0, DepthsArt.TileColumns * DepthsArt.TileRows)));
                    shrine.Add((cx + dx, cy + dy));
                }
            var reach = MythosRelics.ReachableAvoiding(f, null);
            var blocked = new List<(int x, int y)> { chest.Value };
            var around = new List<(int x, int y)>();
            foreach (var (dx, dy) in new[] { (-1, 1), (1, 1), (-1, -1), (1, -1), (0, 1), (-1, 0), (1, 0), (0, -1) })
                if (!f.IsWall(cx + dx, cy + dy) && !taken.Contains((cx + dx, cy + dy)) && reach.Contains((cx + dx, cy + dy))) around.Add((cx + dx, cy + dy));
            var wanted = full ? new[] { DepthsKind.Statue, DepthsKind.Statue, DepthsKind.BookAltar } : new DepthsKind[0];
            var next = 0;
            foreach (var kind in wanted)
            {
                for (; next < around.Count; next++)
                {
                    var cell = around[next];
                    blocked.Add(cell);
                    // the floor must still be one piece: every reachable cell is still reached except the blocked ones
                    if (MythosRelics.ReachableAvoiding(f, blocked).Count != reach.Count - blocked.Count) { blocked.RemoveAt(blocked.Count - 1); continue; }
                    plan.Add(new DepthsPiece(kind, cell.x, cell.y, kind == DepthsKind.Statue ? rng.Range(0, 6) : 0, true));
                    next++;
                    break;
                }
            }
            // one urn each side of the chest where there is room (they do not block)
            foreach (var (dx, dy) in new[] { (-2, 0), (2, 0) })
            {
                var cell = (x: cx + dx, y: cy + dy);
                if (!f.IsWall(cell.x, cell.y) && !taken.Contains(cell) && !blocked.Contains(cell)) plan.Add(new DepthsPiece(DepthsKind.Urn, cell.x, cell.y, rng.Range(0, 2)));
            }
        }

        static bool TouchesOpen(MineFloor f, int x, int y) => !f.IsWall(x + 1, y) || !f.IsWall(x - 1, y) || !f.IsWall(x, y + 1) || !f.IsWall(x, y - 1);

        static bool Overlaps(HashSet<(int, int)> used, int x, int y)
        {
            for (var dy = 0; dy <= 2; dy++)
                for (var dx = -1; dx <= 1; dx++) if (used.Contains((x + dx, y + dy))) return true;
            return false;
        }

        static void Reserve(HashSet<(int, int)> used, int x, int y)
        {
            for (var dy = 0; dy <= 2; dy++)
                for (var dx = -1; dx <= 1; dx++) used.Add((x + dx, y + dy));
        }

        static void Shuffle<T>(List<T> list, ref DetRandom rng)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Range(0, i + 1);
                var t = list[i]; list[i] = list[j]; list[j] = t;
            }
        }
    }
}
