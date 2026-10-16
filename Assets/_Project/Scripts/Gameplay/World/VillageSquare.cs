using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The village square (playtest 2026-10-16: "it does not read nor look like a village square"). A cobbled plaza where the main road crosses the lane, with a gazebo for a
    // centrepiece; the road and the lane come in from the four sides and go round it, the clock tower and the fountain stand on its north edge, and the ways in widen a
    // little as they reach it, so that the plaza has no hard corners where a road meets it. Pure (cells in, cells out), so that the scene builder, the roads and the tests share
    // it. Cells are village cells; y grows to the north.
    public static class VillageSquare
    {
        // The cobbled plaza, inclusive. Its four corner cells are not paved, which rounds it off.
        public const int X0 = 20, X1 = 30, Y0 = 14, Y1 = 21;

        // The gazebo's floor, inclusive: five cells wide and three deep, standing in the middle of the plaza where the road and the lane cross. Its back row and the two
        // rails beside the entrance are solid; the front row and the middle of the floor can be walked on (the roof is drawn over whoever stands in it).
        public const int GazeboX0 = 23, GazeboX1 = 27, GazeboY0 = 16, GazeboY1 = 18;
        public static readonly Vector2Int Gazebo = new Vector2Int((GazeboX0 + GazeboX1) / 2, GazeboY0);          // the middle of its front edge: the foot of the picture

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

        public static bool OnGazebo(int x, int y) => x >= GazeboX0 && x <= GazeboX1 && y >= GazeboY0 && y <= GazeboY1;

        // The cells of the gazebo that block walking.
        public static bool GazeboSolid(int x, int y)
        {
            if (!OnGazebo(x, y)) return false;
            if (y == GazeboY1) return true;                                                     // the back
            return y == GazeboY0 + 1 && (x == GazeboX0 || x == GazeboX1);                       // the rails either side of the way in
        }

        public static IEnumerable<Vector2Int> GazeboSolidCells()
        {
            for (var y = GazeboY0; y <= GazeboY1; y++)
                for (var x = GazeboX0; x <= GazeboX1; x++)
                    if (GazeboSolid(x, y)) yield return new Vector2Int(x, y);
        }

        // What stands in the plaza for good (the festivals bring their own props on their days; Festivals.Gathering keeps clear of all of these). Cells are the left cell of a
        // prop that is two cells wide.
        public static readonly Vector2Int FlagWest = new Vector2Int(23, 20), FlagEast = new Vector2Int(27, 20);
        public static readonly Vector2Int BenchWest = new Vector2Int(20, 20), BenchEast = new Vector2Int(28, 20);          // two cells wide, facing the gazebo
        public static readonly Vector2Int Board = new Vector2Int(21, 19), MerchantStall = new Vector2Int(30, 19);
        public static readonly int[] LampNorthX = { 3, 11, 20, 34, 40, 46 }, LampSouthX = { 4, 12, 20, 30, 38, 46 };          // along the road, a row outside each edge of it

        // Every cell of the plaza that something permanent blocks (the gazebo's back and rails, the clock tower, the fountain, flags, benches, the board, the merchant's stall and the
        // lamp posts).
        public static IEnumerable<Vector2Int> SolidPropCells()
        {
            foreach (var c in GazeboSolidCells()) yield return c;
            yield return new Vector2Int(MapLayout.ClockTower.x, MapLayout.ClockTower.y); yield return new Vector2Int(MapLayout.ClockTower.x + 1, MapLayout.ClockTower.y);
            yield return new Vector2Int(MapLayout.Fountain.x, MapLayout.Fountain.y); yield return new Vector2Int(MapLayout.Fountain.x + 1, MapLayout.Fountain.y);
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
