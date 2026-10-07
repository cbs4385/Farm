using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Where an NPC is at one moment: standing at a cell, or walking one leg of a route (From -> To on `Map`, T = how far).
    public readonly struct NpcPlacement
    {
        public readonly string Map;
        public readonly int FromX, FromY, ToX, ToY;
        public readonly float T;
        public readonly bool Walking;
        public readonly string Facing;       // how they stand once they are there (standing), or empty
        public readonly string DestinationMap;

        public NpcPlacement(string map, int fromX, int fromY, int toX, int toY, float t, bool walking, string facing, string destinationMap)
        {
            Map = map; FromX = fromX; FromY = fromY; ToX = toX; ToY = toY; T = t; Walking = walking; Facing = facing; DestinationMap = destinationMap;
        }

        public static NpcPlacement Standing(string map, int x, int y, string facing) =>
            new NpcPlacement(map, x, y, x, y, 1f, false, facing, map);
    }

    // NPC positions are a pure function of the clock (ADR 0003): there is no per-NPC movement state to save, and the
    // "simulation" of an NPC the player cannot see is the same call. A stop's minute is when the NPC leaves the previous
    // stop; they arrive after the walk time MapRoutes computes.
    public static class NpcSchedule
    {
        // The day's plan: the highest-priority entry whose condition holds (list order breaks ties); null when the NPC
        // has no schedule. Callers keep the answer for the whole day so a flag changing mid-day cannot make an NPC jump.
        public static NpcScheduleEntry PlanFor(NpcDefinition npc, IWorldQuery world, IEnumerable<NpcScheduleEntry> extra = null)
        {
            NpcScheduleEntry best = null;
            foreach (var entry in extra == null ? npc.Schedule : System.Linq.Enumerable.Concat(npc.Schedule, extra))
            {
                if (entry == null || entry.Stops.Count == 0) continue;
                if (best != null && entry.Priority <= best.Priority) continue;
                if (Conditions.TryEvaluate(entry.Condition, world, out var ok) && ok) best = entry;
            }
            return best;
        }

        public static NpcPlacement Where(NpcDefinition npc, NpcScheduleEntry plan, float minuteOfDay)
        {
            if (plan == null || plan.Stops.Count == 0) return NpcPlacement.Standing(npc.HomeMap, npc.HomeX, npc.HomeY, "down");

            var stops = plan.Stops;
            var index = -1;
            for (var i = 0; i < stops.Count; i++)
                if (stops[i].Minute <= minuteOfDay) index = i;

            // Before the first stop (and at the first stop) they simply start the day there.
            if (index <= 0) return Stand(stops[0]);

            var from = stops[index - 1];
            var to = stops[index];
            var legs = MapRoutes.Legs(from.Map, from.X, from.Y, to.Map, to.X, to.Y);
            if (legs == null || legs.Count == 0) return Stand(to);            // no route (data error): they just are there

            var elapsed = minuteOfDay - to.Minute;
            foreach (var leg in legs)
            {
                if (elapsed < leg.Minutes)
                    return new NpcPlacement(leg.Map, leg.FromX, leg.FromY, leg.ToX, leg.ToY, elapsed / leg.Minutes, true, to.Facing, to.Map);
                elapsed -= leg.Minutes;
            }
            return Stand(to);
        }

        static NpcPlacement Stand(NpcStop stop) => NpcPlacement.Standing(stop.Map, stop.X, stop.Y, stop.Facing);

        // For tests and the validator: stops are in time order, inside the day, and every consecutive pair has a route.
        public static bool IsValid(NpcScheduleEntry entry, out string error)
        {
            error = null;
            var last = int.MinValue;
            var previousArrival = 0f;
            NpcStop previous = null;
            foreach (var stop in entry.Stops)
            {
                if (stop.Minute < GameDateTime.DayStartMinute || stop.Minute >= GameDateTime.DayEndMinute) { error = $"stop at minute {stop.Minute} is outside the day"; return false; }
                if (stop.Minute <= last) { error = $"stop at minute {stop.Minute} is not after the previous stop"; return false; }
                var arrival = (float)stop.Minute;               // when they are there: the first stop is where the day starts; after that, the walk to it takes time
                if (previous != null)
                {
                    var legs = MapRoutes.Legs(previous.Map, previous.X, previous.Y, stop.Map, stop.X, stop.Y);
                    if (legs == null) { error = $"no route from {previous.Map} to {stop.Map}"; return false; }
                    var walk = 0f;
                    foreach (var leg in legs) walk += leg.Minutes;
                    arrival = stop.Minute + walk;
                    // They must have arrived at the previous stop before they leave it, or the walk would start from a place they never reached.
                    if (stop.Minute < previousArrival)
                    {
                        error = $"leaves {previous.Map} at minute {stop.Minute} before arriving there (at {previousArrival:0})";
                        return false;
                    }
                }
                last = stop.Minute;
                previous = stop;
                previousArrival = arrival;
            }
            return true;
        }

        public static string FacingName(UnityEngine.Vector2Int facing) =>
            facing == UnityEngine.Vector2Int.up ? "up" : facing == UnityEngine.Vector2Int.left ? "left" : facing == UnityEngine.Vector2Int.right ? "right" : "down";

        public static UnityEngine.Vector2Int FacingVector(string facing)
        {
            switch (facing)
            {
                case "up": return UnityEngine.Vector2Int.up;
                case "left": return UnityEngine.Vector2Int.left;
                case "right": return UnityEngine.Vector2Int.right;
                default: return UnityEngine.Vector2Int.down;
            }
        }
    }
}
