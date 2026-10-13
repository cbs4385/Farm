using System;
using System.Collections.Generic;
using Farm.Data;

namespace Farm.Gameplay
{
    // Sprinklers and scarecrows: what their range covers. Pure arithmetic over cells.
    public static class Sprinklers
    {
        // A square around the sprinkler (playtest 2026-10-13: "the sprinklers do not water all adjacent crops": the first one left the diagonals dry). Range 1 waters all
        // eight tiles next to it, 2 the 24 around (5x5), 3 the 48 around (7x7).
        public static int CountFor(int range) { var side = 2 * Math.Max(1, range) + 1; return side * side - 1; }

        public static IEnumerable<(int x, int y)> Covered(int x, int y, int range)
        {
            var reach = Math.Max(1, range);
            for (var dy = -reach; dy <= reach; dy++)
                for (var dx = -reach; dx <= reach; dx++)
                    if (dx != 0 || dy != 0) yield return (x + dx, y + dy);
        }

        // Waters every tilled tile in range of every sprinkler placed on a map. Returns how many tiles were watered.
        public static int WaterAll(IEnumerable<PlacedObject> objects, PlaceableCatalog placeables, FarmGrid grid)
        {
            var watered = 0;
            foreach (var obj in objects)
            {
                var def = placeables.Get(obj.TypeId);
                if (def == null || def.Kind != PlaceableKind.Sprinkler) continue;
                foreach (var (x, y) in Covered(obj.X, obj.Y, def.Range))
                    if (grid.Water(x, y)) watered++;
            }
            return watered;
        }

        // Is the cell within a scarecrow's reach on this map?
        public static bool Protected(IEnumerable<PlacedObject> objects, PlaceableCatalog placeables, int x, int y)
        {
            foreach (var obj in objects)
            {
                var def = placeables.Get(obj.TypeId);
                if (def == null || def.Kind != PlaceableKind.Scarecrow) continue;
                var dx = obj.X - x; var dy = obj.Y - y;
                if (dx * dx + dy * dy <= def.Range * def.Range) return true;
            }
            return false;
        }
    }
}
