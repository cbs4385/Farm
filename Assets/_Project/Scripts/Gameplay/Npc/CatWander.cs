using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Where the village cat goes next: a nearby walkable cell it can actually reach, so it ambles about without crossing the whole map.
    public static class CatWander
    {
        public static List<(int x, int y)> PickRoute(WalkGrid grid, int cx, int cy, System.Random rng, int range = 7, int tries = 16)
        {
            for (var i = 0; i < tries; i++)
            {
                var gx = cx + rng.Next(-range, range + 1);
                var gy = cy + rng.Next(-range, range + 1);
                if ((gx == cx && gy == cy) || !grid.IsWalkable(gx, gy)) continue;
                var path = grid.FindPath(cx, cy, gx, gy);
                if (path != null && path.Count > 1 && path.Count <= range * 3) return path;
            }
            return null;
        }
    }
}
