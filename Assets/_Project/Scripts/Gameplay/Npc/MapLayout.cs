using UnityEngine;

namespace Farm.Gameplay
{
    // The sizes of the two big outdoor maps and the cells of the doors between them, shared by the scene builder and the route table so that
    // they cannot drift apart. Both maps grow to the east and north only: everything that was already there keeps its cell (saves, crops,
    // schedules and scenes stay valid).
    public static class MapLayout
    {
        // The farm (was 44 x 32): about twice the land. The road to the village leaves at the middle of the east edge.
        public const int FarmW = 88, FarmH = 64;
        public const int FarmRoadY = 15;
        public const int FarmExitX = FarmW - 1;
        public const int FarmArriveX = FarmW - 3;        // where one appears on the farm coming from the village

        // The village (was 50 x 36): about one and a half times the land. The lane to the forest leaves at the top of the lane.
        public const int VillageW = 75, VillageH = 54;
        public const int VillageLaneX = 25;
        public const int VillageForestExitY = VillageH - 1;
        public const int VillageForestArriveY = VillageH - 3;

        // A pond: an oval of water. The scene builder paints it and the map in the menu draws it, from these numbers.
        public readonly struct Pond
        {
            public readonly float X, Y, RadiusX, RadiusY;
            public Pond(float x, float y, float rx, float ry) { X = x; Y = y; RadiusX = rx; RadiusY = ry; }
            public bool Contains(int px, int py) { var dx = (px - X) / RadiusX; var dy = (py - Y) / RadiusY; return dx * dx + dy * dy <= 1f; }
        }

        public static readonly Pond FarmPond = new Pond(58f, 39f, 6.2f, 4.2f);
        public static readonly Pond VillagePond = new Pond(19.5f, 8.5f, 3.2f, 3.6f);
        public static readonly Pond ForestPond = new Pond(6.5f, 10f, 3.8f, 2.9f);

        // The village's landmarks in the square (cells; each is two cells wide).
        public static readonly Vector2Int ClockTower = new Vector2Int(27, 21);
        public static readonly Vector2Int Fountain = new Vector2Int(20, 21);
    }
}
