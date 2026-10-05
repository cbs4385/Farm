using System;

namespace Farm.Gameplay
{
    // Which water tile goes where: open water gets one of four ripple variants, and a cell beside land gets the tile with the shoreline on
    // the sides (and inside corners) where the land is, so a pond or the sea reads as one body of water. The tile art is built by
    // tools/art/build_water_tiles.py; this names it.
    public static class WaterShore
    {
        public const string Prefix = "tile_water";
        public const int Variants = 4;

        const int N = 1, E = 2, S = 4, W = 8, NE = 16, SE = 32, SW = 64, NW = 128;

        // The mask for the water cell at (x, y): a bit for each side with land beside it, and for each corner where only the diagonal is land.
        // `isWater` answers for any cell (cells outside the body of water are land). Up is +y, as on the map.
        public static int Mask(Func<int, int, bool> isWater, int x, int y)
        {
            var mask = 0;
            if (!isWater(x, y + 1)) mask |= N;
            if (!isWater(x + 1, y)) mask |= E;
            if (!isWater(x, y - 1)) mask |= S;
            if (!isWater(x - 1, y)) mask |= W;
            if ((mask & (N | E)) == 0 && !isWater(x + 1, y + 1)) mask |= NE;
            if ((mask & (S | E)) == 0 && !isWater(x + 1, y - 1)) mask |= SE;
            if ((mask & (S | W)) == 0 && !isWater(x - 1, y - 1)) mask |= SW;
            if ((mask & (N | W)) == 0 && !isWater(x - 1, y + 1)) mask |= NW;
            return mask;
        }

        // The sprite and tile name for a mask; open water picks its ripple variant from the cell so neighbours differ.
        public static string TileName(int mask, int x, int y) =>
            mask == 0 ? $"{Prefix}_m0v{(x * 7 + y * 13 & 0x7fffffff) % Variants}" : $"{Prefix}_m{mask}";

        public static string TileNameAt(Func<int, int, bool> isWater, int x, int y) => TileName(Mask(isWater, x, y), x, y);

        // Is this tile one of the water tiles (the old single tile or any of the shore set)?
        public static bool IsWaterTile(string tileName) => tileName != null && tileName.StartsWith(Prefix, StringComparison.Ordinal);
    }
}
