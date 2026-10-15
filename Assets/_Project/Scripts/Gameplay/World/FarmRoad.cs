using System.Collections.Generic;

namespace Farm.Gameplay
{
    // The farm's path, one cell wide: from the farmhouse door south (down the map), then east to the village. Like the forest's trail (TrailShape) it bends instead of running as two straight lines;
    // its ends stay where the door, the spawn points and the way out to the village are. Pure, so that the scene builder and the tests share it.
    public static class FarmRoad
    {
        public const int Seed = 17;
        const int DoorRows = 5;                                    // rows from the road (y = FarmRoadY) down to the cell in front of the door

        // x offsets of the rows from the road row towards the door, then y offsets of the columns from the door's column eastwards.
        public static int[] South { get; } = TrailShape.Line(DoorRows, -1, 1, Seed, endCells: 1, ramp: 2f, amplitude: 2f);
        public static int[] East { get; } = TrailShape.Line(MapLayout.FarmW - MapLayout.FarmHouseDoorX, -1, 1, Seed + 5, endCells: 2, ramp: 2f, amplitude: 2f);

        public static IReadOnlyList<(int x, int y)> Cells { get; } = Build();

        public static bool Contains(int x, int y)
        {
            foreach (var c in Cells) if (c.x == x && c.y == y) return true;
            return false;
        }

        static List<(int x, int y)> Build()
        {
            var cells = new List<(int x, int y)>();
            var doorX = MapLayout.FarmHouseDoorX;
            var roadY = MapLayout.FarmRoadY;
            foreach (var c in TrailShape.Cells(South, 0, 0)) cells.Add((doorX + c.y, roadY + c.x));          // the rows from the road up to the door; a step sideways fills the cell beside
            foreach (var c in TrailShape.Cells(East, doorX, roadY)) cells.Add(c);
            return cells;
        }
    }
}
