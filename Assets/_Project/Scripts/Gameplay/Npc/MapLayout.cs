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
    }
}
