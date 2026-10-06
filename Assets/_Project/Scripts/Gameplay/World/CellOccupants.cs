using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Something that stands on a map cell and can be talked to, petted or collected from: a villager, the village cat, a farm animal.
    // At most one of them may occupy a cell (playtest report: two on one tile made it hard to choose what to interact with).
    public interface ICellOccupant
    {
        Vector3Int Cell { get; }
        bool IsPlaced { get; }          // false until it has a real position (a new actor starts at the origin)
        Vector3Int? Claim => null;      // the cell it is walking into, held so that nobody else walks into it too
    }

    // Who is where on the loaded map. Villagers follow their schedule, so they never give way; the cat and the animals choose their steps
    // around whoever is there and step aside when someone arrives on their cell.
    public static class CellOccupants
    {
        static readonly List<ICellOccupant> All = new List<ICellOccupant>();

        public static int Count => All.Count;

        public static void Add(ICellOccupant occupant)
        {
            if (occupant != null && !All.Contains(occupant)) All.Add(occupant);
        }

        public static void Remove(ICellOccupant occupant) => All.Remove(occupant);

        public static void ResetForTests() => All.Clear();

        // Is another occupant on the cell?
        public static bool IsTaken(Vector3Int cell, ICellOccupant except = null)
        {
            foreach (var o in All)
                if (o != except && o.IsPlaced && (o.Cell == cell || o.Claim == cell)) return true;
            return false;
        }

        // The occupants that share `self`'s cell (not including it).
        public static int SharingWith(ICellOccupant self)
        {
            var n = 0;
            foreach (var o in All)
                if (o != self && o.IsPlaced && o.Cell == self.Cell) n++;
            return n;
        }

        static readonly Vector3Int[] Around = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // The nearest cell (by steps, up to `radius`) that `isFloor` accepts and nobody holds; false when there is none.
        public static bool TryFindFree(Vector3Int from, Func<Vector3Int, bool> isFloor, ICellOccupant self, out Vector3Int found, int radius = 4)
        {
            var seen = new HashSet<Vector3Int> { from };
            var frontier = new List<Vector3Int> { from };
            for (var step = 0; step < radius; step++)
            {
                var next = new List<Vector3Int>();
                foreach (var cell in frontier)
                    foreach (var d in Around)
                    {
                        var c = cell + d;
                        if (!seen.Add(c) || !isFloor(c)) continue;
                        if (!IsTaken(c, self)) { found = c; return true; }
                        next.Add(c);
                    }
                frontier = next;
            }
            found = from;
            return false;
        }
    }
}
