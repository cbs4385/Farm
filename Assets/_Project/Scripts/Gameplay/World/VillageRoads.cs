using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // The village's dirt roads wind like the farm's road and the forest's trail (playtest 2026-10-16: "significant portions of the village have very straight paths and
    // they are not the dirt roads similar to the other maps"). The main road (three cells wide, west to east), the lane to the forest and the beach, the shops' paths to the
    // road and the links to the villagers' streets are all `tile_path` and bend. Only the cobbled square stays straight and cobbled (the clock tower, the fountain, the
    // festival grounds and the lamps are laid out round it), with the three ways out; the villagers' own streets keep their shape (their cottages stand beside them) but are
    // dirt too. Walking does not depend on the tile, so a bend changes how the village looks, not where anyone can go. Pure, so that the scene builder and the tests share it.
    public static class VillageRoads
    {
        public const int RoadY0 = 16, RoadY1 = 18, LaneX0 = 24, LaneX1 = 26;
        public const int PlazaX0 = 20, PlazaX1 = 38;        // the columns of the road that stay straight and cobbled (the square and the festival grounds)
        public const int PlazaLaneY0 = 14, PlazaLaneY1 = 22;
        public const int MinShift = -3, MaxShift = 2;       // how far the road moves down (-) or up (+) from its old rows; the lane moves west (-) or east (+) by the same
        public const string Dirt = "tile_path", Cobble = "tile_cobble";
        const int Seed = 41;

        // How many rows the road is moved up (+) for each column of the village, and how many columns the lane is moved east (+) for each row.
        public static int[] Road { get; } = Build(MapLayout.VillageW, new[] { (0, 4), (PlazaX0, PlazaX1) }, new[] { (5, 19, -3, 1), (13, 19, -1, 1), (50, 70, -2, 2) }, Seed);
        public static int[] Lane { get; } = Build(MapLayout.VillageH, new[] { (0, 3), (12, 26), (MapLayout.VillageH - 4, MapLayout.VillageH - 1) }, new[] { (5, 11, -1, 2), (27, 31, -2, 2) }, Seed + 7);

        public static int RoadShift(int x) => x < 0 || x >= Road.Length ? 0 : Road[x];
        public static int LaneShift(int y) => y < 0 || y >= Lane.Length ? 0 : Lane[y];

        public static bool OnRoad(int x, int y) { var o = RoadShift(x); return y >= RoadY0 + o && y <= RoadY1 + o; }
        public static bool OnLane(int x, int y) { var o = LaneShift(y); return x >= LaneX0 + o && x <= LaneX1 + o; }

        // The rows just outside the road's two edges (where the lamps stand), for a column.
        public static int RowAboveRoad(int x) => RoadY1 + 1 + RoadShift(x);
        public static int RowBelowRoad(int x) => RoadY0 - 1 + RoadShift(x);

        // The cobbled square: the road between the plaza columns, and the lane where it crosses.
        public static bool OnPlaza(int x, int y) =>
            OnRoad(x, y) && x >= PlazaX0 && x <= PlazaX1 || OnLane(x, y) && y >= PlazaLaneY0 && y <= PlazaLaneY1;

        // The tile for a cell of the village's roads, shop paths and links ("tile_cobble" on the square, "tile_path" elsewhere), or null when the cell is neither.
        public static string Ground(int x, int y)
        {
            if (OnPlaza(x, y)) return Cobble;
            return OnRoad(x, y) || OnLane(x, y) || _paths.Contains((x, y)) ? Dirt : null;
        }

        // The land the roads take, with a cell of margin (no scattered trees there).
        public static bool Near(int x, int y)
        {
            for (var d = -1; d <= 1; d++)
                for (var e = -1; e <= 1; e++)
                    if (Ground(x + d, y + e) != null) return true;
            return false;
        }

        // The cells of the shops' paths and the links to the streets (without the road and the lane themselves).
        public static IReadOnlyCollection<(int x, int y)> PathCells => _paths;
        static readonly HashSet<(int x, int y)> _paths = BuildPaths();

        // A shop's path from the road to its door: it keeps its first two cells (the flower boxes stand beside them) and its last two (the doorstep) and wanders between.
        // The hall stands between the carpenter and the library, with two free columns (its door and the one east of it), so its path may only lean east.
        public static List<(int x, int y)> ShopPath(VillageShops.Shop shop)
        {
            var from = RowAboveRoad(shop.DoorX);
            var rows = shop.Y0 - from;
            var cells = new List<(int x, int y)>();
            if (rows <= 0) return cells;
            var hall = shop.Business == null;
            var offsets = TrailShape.Line(rows, hall ? 0 : -1, 1, Seed + shop.DoorX, endCells: 2, ramp: 1.5f, amplitude: hall ? 1.2f : 2.4f);
            foreach (var c in TrailShape.Cells(offsets, 0, 0)) cells.Add((shop.DoorX + c.y, from + c.x));
            return cells;
        }

        // A link to a street running up the map from `y0` to `y1` (inclusive), three cells wide, starting and ending straight at x..x+2; or along the map from `x0` to `x1` at y..y+2.
        public static List<(int x, int y)> VerticalLink(int x, int y0, int y1, int seed, int minOff = -2, int maxOff = 2)
        {
            var cells = new List<(int x, int y)>();
            var rows = y1 - y0 + 1;
            if (rows <= 0) return cells;
            var offsets = rows < 5 ? new int[rows] : TrailShape.Line(rows, minOff, maxOff, seed, endCells: 1, ramp: 2f, amplitude: Math.Max(-minOff, maxOff) + 0.6f);
            for (var i = 0; i < rows; i++)
                for (var w = 0; w < 3; w++) cells.Add((x + offsets[i] + w, y0 + i));
            return cells;
        }

        public static List<(int x, int y)> HorizontalLink(int x0, int x1, int y, int seed, int reach = 1)
        {
            var cells = new List<(int x, int y)>();
            var cols = x1 - x0 + 1;
            if (cols <= 0) return cells;
            var offsets = cols < 5 ? new int[cols] : TrailShape.Line(cols, -reach, reach, seed, endCells: 1, ramp: 2f, amplitude: reach + 0.6f);
            for (var i = 0; i < cols; i++)
                for (var w = 0; w < 3; w++) cells.Add((x0 + i, y + offsets[i] + w));
            return cells;
        }

        static HashSet<(int x, int y)> BuildPaths()
        {
            var set = new HashSet<(int x, int y)>();
            foreach (var shop in VillageShops.All)
                foreach (var c in ShopPath(shop)) set.Add(c);
            // The links of NpcHomes.Links, drawn winding: the north-west street's across to the lane, the north-east street's down to the road, the south-east street's up to it.
            var nw = NpcHomes.Links[0];
            foreach (var c in HorizontalLink(nw.xMin, LaneX0 + LaneShift(nw.yMin + 1) + 1, nw.yMin, Seed + 3)) set.Add(c);
            var ne = NpcHomes.Links[1];
            foreach (var c in VerticalLink(ne.xMin, RoadY0 + RoadShift(ne.xMin + 1), ne.yMax - 1, Seed + 5, minOff: -2, maxOff: 0)) set.Add(c);          // east of it the clinic
            var se = NpcHomes.Links[2];
            foreach (var c in VerticalLink(se.xMin, se.yMin, RoadY1 + RoadShift(se.xMin + 1), Seed + 9, minOff: -1, maxOff: 1)) set.Add(c);
            return set;
        }

        // `still` are the stretches that keep their course; `limits` (from, to, min, max) hold a stretch inside a smaller range (the pond, the first festival stall, the cottages) (the pond on the road's and lane's north-west).
        static int[] Build(int length, (int from, int to)[] still, (int from, int to, int min, int max)[] limits, int seed)
        {
            var offsets = TrailShape.Line(length, MinShift, MaxShift, seed, endCells: 1, ramp: 4f, amplitude: 4f);
            for (var i = 0; i < length; i++)
            {
                var near = int.MaxValue;
                foreach (var (a, b) in still)
                {
                    if (i >= a && i <= b) { near = 0; break; }
                    near = Math.Min(near, i < a ? a - i : i - b);
                }
                var o = Math.Max(-near, Math.Min(near, offsets[i]));          // within a step of one for each column away from a stretch that keeps its course: always one piece
                foreach (var (a, b, min, max) in limits) if (i >= a && i <= b) o = Math.Max(min, Math.Min(max, o));
                offsets[i] = o;
            }
            for (var i = 1; i < length; i++)                                   // no jump of two (the clamps above can leave one)
                if (Math.Abs(offsets[i] - offsets[i - 1]) > 1) offsets[i] = offsets[i - 1] + Math.Sign(offsets[i] - offsets[i - 1]);
            return offsets;
        }
    }
}
