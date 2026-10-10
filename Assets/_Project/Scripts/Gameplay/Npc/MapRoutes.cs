using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // The doors between maps, as data: where you leave one map and where you arrive on the next. NPC schedules use it to
    // route a villager from one map to another (and to time the walk). A test opens every scene and checks that each
    // edge matches the real warps and spawn points, so this table cannot drift from MapBuilder.
    public readonly struct RouteEdge
    {
        public readonly string From, To;
        public readonly int ExitX, ExitY;        // cell to walk to on `From` (the warp)
        public readonly int ArriveX, ArriveY;    // cell where one appears on `To`
        public readonly string Spawn;            // spawn point id on `To` that this arrival corresponds to

        public RouteEdge(string from, int exitX, int exitY, string to, int arriveX, int arriveY, string spawn)
        {
            From = from; ExitX = exitX; ExitY = exitY; To = to; ArriveX = arriveX; ArriveY = arriveY; Spawn = spawn;
        }
    }

    // One stretch of a journey that stays on one map.
    public readonly struct RouteLeg
    {
        public readonly string Map;
        public readonly int FromX, FromY, ToX, ToY;
        public readonly float Minutes;

        public RouteLeg(string map, int fromX, int fromY, int toX, int toY, float minutes)
        {
            Map = map; FromX = fromX; FromY = fromY; ToX = toX; ToY = toY; Minutes = minutes;
        }
    }

    public static class MapRoutes
    {
        // A villager covers this many cells per game minute (about 2.3 cells per real second at the default clock).
        public const float CellsPerMinute = 1.6f;

        static readonly List<RouteEdge> Edges = BuildEdges();

        public static IReadOnlyList<RouteEdge> All => Edges;

        static List<RouteEdge> BuildEdges()
        {
            var e = new List<RouteEdge>();
            void Pair(RouteEdge a, RouteEdge b) { e.Add(a); e.Add(b); }

            // Farm <-> village road, farm <-> farmhouse.
            Pair(new RouteEdge(MapIds.Farm, MapLayout.FarmExitX, MapLayout.FarmRoadY, MapIds.Village, 2, 17, "fromFarm"),
                 new RouteEdge(MapIds.Village, 0, 17, MapIds.Farm, MapLayout.FarmArriveX, MapLayout.FarmRoadY, "fromVillage"));
            Pair(new RouteEdge(MapIds.Farm, 7, 20, MapIds.FarmHouse, 5, 2, "default"),
                 new RouteEdge(MapIds.FarmHouse, 5, 0, MapIds.Farm, 7, 18, "fromHouse"));

            // The greenhouse, coop and barn doors are on buildings the player can move (FarmBuildingsView), so they are not fixed route edges.

            // Forest <-> Harrow Wood (the gated path at the top of the forest; only an optional layer ships the scene).
            Pair(new RouteEdge(MapIds.Forest, 19, 27, MapIds.Woods, 19, 2, "default"),
                 new RouteEdge(MapIds.Woods, 19, 0, MapIds.Forest, 19, 26, "fromWoods"));

            // Village <-> forest (south end of the lane) and beach (north end).
            Pair(new RouteEdge(MapIds.Village, MapLayout.VillageLaneX, MapLayout.VillageForestExitY, MapIds.Forest, 19, 2, "fromVillage"),
                 new RouteEdge(MapIds.Forest, 19, 0, MapIds.Village, MapLayout.VillageLaneX, MapLayout.VillageForestArriveY, "fromForest"));
            Pair(new RouteEdge(MapIds.Village, 25, 0, MapIds.Beach, 17, 21, "fromVillage"),
                 new RouteEdge(MapIds.Beach, 17, 23, MapIds.Village, 25, 2, "fromBeach"));

            // Village <-> the six buildings: the door cell in the village, the interior's door at (doorX, 0).
            void Building(string map, int villageDoorX, int villageDoorY, int outsideY, int interiorDoorX)
            {
                Pair(new RouteEdge(MapIds.Village, villageDoorX, villageDoorY, map, interiorDoorX, 2, "default"),
                     new RouteEdge(map, interiorDoorX, 0, MapIds.Village, villageDoorX, outsideY, "from" + map));
            }
            Building(MapIds.GeneralStore, 8, 24, 23, 5);
            Building(MapIds.Blacksmith, 17, 24, 23, 4);
            Building(MapIds.Carpenter, 32, 24, 23, 4);
            Building(MapIds.Library, 42, 24, 23, 5);
            Building(MapIds.Saloon, 52, 24, 23, 6);
            Building(MapIds.Clinic, 67, 24, 23, 4);
            Building(MapIds.CommunityHall, 37, 31, 30, 7);
            foreach (var home in NpcHomes.All) Building(home.Map, home.DoorX, home.DoorY, home.OutsideY, NpcHomes.InteriorDoorX);          // the villagers' cottages
            return e;
        }

        // The maps to pass through (inclusive of both ends), fewest doors first; null when there is no way.
        public static List<RouteEdge> Path(string from, string to)
        {
            var result = new List<RouteEdge>();
            if (from == to) return result;
            var previous = new Dictionary<string, RouteEdge> { { from, default } };
            var queue = new Queue<string>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var map = queue.Dequeue();
                foreach (var edge in Edges)
                {
                    if (edge.From != map || previous.ContainsKey(edge.To)) continue;
                    previous[edge.To] = edge;
                    if (edge.To == to)
                    {
                        for (var at = to; at != from; at = previous[at].From) result.Insert(0, previous[at]);
                        return result;
                    }
                    queue.Enqueue(edge.To);
                }
            }
            return null;
        }

        static float Minutes(int x0, int y0, int x1, int y1) =>
            Math.Max(0.25f, (Math.Abs(x1 - x0) + Math.Abs(y1 - y0)) / CellsPerMinute);

        // The legs of a journey (empty when the start and end are the same cell; null when there is no route).
        public static List<RouteLeg> Legs(string fromMap, int fromX, int fromY, string toMap, int toX, int toY)
        {
            var legs = new List<RouteLeg>();
            var path = Path(fromMap, toMap);
            if (path == null) return null;

            var map = fromMap; var x = fromX; var y = fromY;
            foreach (var edge in path)
            {
                legs.Add(new RouteLeg(map, x, y, edge.ExitX, edge.ExitY, Minutes(x, y, edge.ExitX, edge.ExitY)));
                map = edge.To; x = edge.ArriveX; y = edge.ArriveY;
            }
            if (map != toMap || x != toX || y != toY) legs.Add(new RouteLeg(map, x, y, toX, toY, Minutes(x, y, toX, toY)));
            return legs;
        }
    }
}
