namespace Farm.Gameplay
{
    // Where the small things lying on the grass go (tufts, flowers): the same cells for the same map and seed, and the same share of them on every map. Pure, so
    // that it can be tested; GroundDecor draws what it says. Playtest 2026-10-09: "the world looks flat".
    public static class DecorPlanner
    {
        public const float Density = 0.14f;            // the share of grass cells that carry something
        public const float FlowerShare = 0.22f;        // of those, the share that are flowers rather than tufts

        // The decoration at a cell: -1 for none, else the index of a tuft (flower false) or of a flower (flower true) among `tufts` or `flowers` pictures.
        public static int At(int seed, string mapId, int x, int y, int tufts, int flowers, out bool flower)
        {
            flower = false;
            if (tufts <= 0 && flowers <= 0) return -1;
            var h = Hash(seed, mapId, x, y);
            if (Unit(h) >= Density) return -1;
            var h2 = Hash(seed ^ 0x2545F491, mapId, x, y);
            flower = flowers > 0 && (tufts <= 0 || Unit(h2) < FlowerShare);
            var count = flower ? flowers : tufts;
            return (int)((h2 >> 8) % (uint)count);
        }

        // A pick among `count` for a place: the same every time for the same map, seed and cell (-1 when there is nothing to pick from).
        public static int Slot(int seed, string mapId, int x, int y, int count) => count <= 0 ? -1 : (int)((Hash(seed, mapId, x, y) >> 8) % (uint)count);

        static uint Hash(int seed, string mapId, int x, int y)
        {
            unchecked
            {
                var h = (uint)seed * 2654435761u;
                foreach (var c in mapId ?? string.Empty) h = (h ^ c) * 16777619u;
                h ^= (uint)x * 73856093u;
                h ^= (uint)y * 19349663u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return h;
            }
        }

        static float Unit(uint h) => (h & 0xFFFFFF) / 16777216f;
    }
}
