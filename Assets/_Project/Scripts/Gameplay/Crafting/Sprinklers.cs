using System.Collections.Generic;
using Farm.Data;

namespace Farm.Gameplay
{
    // Sprinklers and scarecrows: what their range covers. Pure arithmetic over cells.
    public static class Sprinklers
    {
        // Range 1 waters the four neighbours, 2 the eight around, 3 the 24 around (a 5x5 square).
        public static IEnumerable<(int x, int y)> Covered(int x, int y, int range)
        {
            if (range <= 1)
            {
                yield return (x + 1, y); yield return (x - 1, y); yield return (x, y + 1); yield return (x, y - 1);
                yield break;
            }
            var reach = range == 2 ? 1 : 2;
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
