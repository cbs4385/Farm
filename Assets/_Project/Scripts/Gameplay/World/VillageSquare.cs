using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The village square (playtest 2026-10-16: "it does not read nor look like a village square"). A cobbled plaza where the main road crosses the lane, with a fountain for a
    // centrepiece; the road and the lane come in from the four sides and go round it, the clock tower stands on its north edge, and the ways in widen a
    // little as they reach it, so that the plaza has no hard corners where a road meets it. Pure (cells in, cells out), so that the scene builder, the roads and the tests share
    // it. Cells are village cells; y grows to the north.
    public static class VillageSquare
    {
        // The cobbled plaza, inclusive. Its four corner cells are not paved, which rounds it off.
        public const int X0 = 20, X1 = 30, Y0 = 14, Y1 = 21;

        // The fountain stands in the middle of the plaza, where the road crosses the lane (the first version had a gazebo here; it was hard to walk round and was taken out, 2026-10-16).
        // Its basin is drawn 93 x 70 pixels, filling 95% of the ring of paving stones round it (about 5.8 x 4.4 cells); what blocks is a rounded block of cells in the middle of it,
        // five wide in the middle row and three wide in the rows above and below, so that the road and the lane go round it.
        public static readonly Vector2Int Centre = new Vector2Int(25, 17);

        public static bool FountainSolid(int x, int y)
        {
            var dy = y - Centre.y;
            if (dy < -1 || dy > 1) return false;
            return System.Math.Abs(x - Centre.x) <= (dy == 0 ? 2 : 1);
        }

        public static IEnumerable<Vector2Int> FountainSolidCells()
        {
            for (var y = Centre.y - 1; y <= Centre.y + 1; y++)
                for (var x = Centre.x - 2; x <= Centre.x + 2; x++)
                    if (FountainSolid(x, y)) yield return new Vector2Int(x, y);
        }

        public static bool InRect(int x, int y) => x >= X0 && x <= X1 && y >= Y0 && y <= Y1;

        static bool Corner(int x, int y) => (x == X0 || x == X1) && (y == Y0 || y == Y1);

        // Is the cell paved with cobbles?
        public static bool Cobbled(int x, int y) => InRect(x, y) && !Corner(x, y);

        // The cells where a road or the lane widens as it reaches the plaza: one extra cell each side of the three-wide road, right at the plaza's edge.
        public static bool Flare(int x, int y)
        {
            if (InRect(x, y)) return false;
            if ((x == X0 - 1 || x == X1 + 1) && y >= 15 && y <= 19) return true;           // the road, west and east
            if ((y == Y0 - 1 || y == Y1 + 1) && x >= 23 && x <= 27) return true;           // the lane, south and north
            return false;
        }

        // What stands in the plaza for good (the festivals bring their own props on their days; Festivals.Gathering keeps clear of all of these). Cells are the left cell of a
        // prop that is two cells wide.
        public static readonly Vector2Int FlagWest = new Vector2Int(23, 20), FlagEast = new Vector2Int(27, 20);
        public static readonly Vector2Int BenchWest = new Vector2Int(20, 22), BenchEast = new Vector2Int(30, 22);          // two cells wide, north of the square (owner, 2026-10-16)
        public static readonly Vector2Int Board = new Vector2Int(19, 13), MerchantStall = new Vector2Int(29, 19);          // the board by the pond
        // The four lamp posts of the square stand at (20 or 30, 15 or 19): the same distance from the fountain, each. The others line the road.
        public static readonly int[] LampNorthX = { 3, 11, 20, 30, 34, 40, 46 }, LampSouthX = { 4, 12, 20, 30, 38, 46 };          // along the road, a row outside each edge of it

        // Every cell of the plaza that something permanent blocks (the fountain, flags, benches, the merchant's stall and the
        // lamp posts; the clock tower, benches and board are outside it now).
        public static IEnumerable<Vector2Int> SolidPropCells()
        {
            foreach (var c in FountainSolidCells()) yield return c;
            yield return new Vector2Int(MapLayout.ClockTower.x, MapLayout.ClockTower.y); yield return new Vector2Int(MapLayout.ClockTower.x + 1, MapLayout.ClockTower.y);
            yield return FlagWest; yield return FlagEast; yield return Board; yield return MerchantStall;
            yield return BenchWest; yield return new Vector2Int(BenchWest.x + 1, BenchWest.y);
            yield return BenchEast; yield return new Vector2Int(BenchEast.x + 1, BenchEast.y);
            foreach (var x in LampNorthX) yield return new Vector2Int(x, VillageRoads.RowAboveRoad(x));
            foreach (var x in LampSouthX) yield return new Vector2Int(x, VillageRoads.RowBelowRoad(x));
        }

        // The plaza and a margin round it: no trees stand there, whatever else is decided.
        public static bool Near(int x, int y, int margin = 2) => x >= X0 - margin && x <= X1 + margin && y >= Y0 - margin && y <= Y1 + margin;
    }
}
