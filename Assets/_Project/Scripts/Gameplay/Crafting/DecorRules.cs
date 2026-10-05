using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Where furniture may be set down. Only the farmhouse and the farm, and never in a doorway (a piece of furniture on the way in or out
    // could lock the player in): not on a door cell, and, unless it can be walked over (a rug), not next to one either.
    public static class DecorRules
    {
        public static bool AllowedOn(string mapId) => mapId == MapIds.FarmHouse || mapId == MapIds.Farm;

        public static bool BlocksDoor(Vector3Int cell, bool walkable, IEnumerable<Vector3Int> doorCells)
        {
            foreach (var door in doorCells)
            {
                if (door == cell) return true;
                if (!walkable && Mathf.Abs(door.x - cell.x) <= 1 && Mathf.Abs(door.y - cell.y) <= 1) return true;
            }
            return false;
        }
    }
}
