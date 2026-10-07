using System;
using System.Collections.Generic;
using Farm.Data;

namespace Farm.Gameplay
{
    // Where each villager lives (playtest 2026-10-07: "npcs should live in homes; currently the only home is the player's"; before this a villager's home
    // was the shop they worked in). Twelve cottages on a street in the east of the village, one for each villager, with a room inside for each. The scene
    // builder, the route table, the schedules, the map picture and the tests all read this one table, so they cannot drift apart.
    //
    // The street: a lane runs north from the road at x 60..62; the cottages stand in two columns either side of it, five rows north of the road (doors on
    // their south walls, reached along two-cell alleys between the rows) and two south of it (doors on their north walls, straight onto the road).
    public static class NpcHomes
    {
        public sealed class Home
        {
            public string Npc, Map, Style;
            public int X0, X1, Y0, Y1, DoorX;
            public bool FacesSouth;                       // the door is on the south wall (north of the road) or the north wall (south of it)

            public int DoorY => FacesSouth ? Y0 : Y1;     // the door cell in the village
            public int OutsideY => FacesSouth ? Y0 - 1 : Y1 + 1;   // where one stands (and appears) outside it
        }

        public const int InteriorW = 10, InteriorH = 8, InteriorDoorX = 4;      // every room is the same shape: floor x 1..8, y 1..6, the door at (4, 0)
        public const int StandX = 3, StandY = 3;                                // where the owner stands inside, mornings and nights and when it rains
        public const string OpenCondition = "hour>=7 && hour<21";                  // the door is locked from 21:00 to 7:00: they are asleep
        public const string LockedKey = "home.locked";

        // The street's lane and alleys (cells that are cobbled), and the land the street takes (no scattered trees there).
        public const int LaneX0 = 60, LaneX1 = 62, LaneTopY = 50;
        public const int StreetX0 = 51, StreetX1 = 71, StreetY0 = 5, StreetY1 = 51;

        static readonly int[] NorthRows = { 22, 28, 34, 40, 46 };
        const int WestX0 = 53, EastX0 = 64, CottageW = 6, CottageH = 4, SouthY0 = 6;

        public static readonly IReadOnlyList<Home> All = Build();

        static List<Home> Build()
        {
            var order = new[]
            {
                (NpcIds.Tilda, MapIds.HomeTilda), (NpcIds.Bram, MapIds.HomeBram), (NpcIds.Ione, MapIds.HomeIone), (NpcRoster.Marcus, MapIds.HomeMarcus),
                (NpcRoster.Odalys, MapIds.HomeOdalys), (NpcRoster.Wren, MapIds.HomeWren), (NpcRoster.Felix, MapIds.HomeFelix), (NpcRoster.Juno, MapIds.HomeJuno),
                (NpcRoster.Hazel, MapIds.HomeHazel), (NpcRoster.Piper, MapIds.HomePiper), (NpcRoster.Dorian, MapIds.HomeDorian), (NpcRoster.Elara, MapIds.HomeElara),
            };
            var homes = new List<Home>();
            for (var i = 0; i < order.Length; i++)
            {
                var (npc, map) = order[i];
                var east = i % 2 == 1;
                var north = i < 10;
                var x0 = east ? EastX0 : WestX0;
                var y0 = north ? NorthRows[i / 2] : SouthY0;
                homes.Add(new Home
                {
                    Npc = npc, Map = map, Style = "cottage" + (1 + i % 4), FacesSouth = north,
                    X0 = x0, X1 = x0 + CottageW - 1, Y0 = y0, Y1 = y0 + CottageH - 1, DoorX = x0 + 2,
                });
            }
            return homes;
        }

        public static Home Of(string npcId)
        {
            foreach (var h in All) if (h.Npc == npcId) return h;
            return null;
        }

        public static Home ForMap(string mapId)
        {
            foreach (var h in All) if (h.Map == mapId) return h;
            return null;
        }

        public static string MapOf(string npcId) => Of(npcId)?.Map;

        // A stop inside the villager's own house (the morning and night place, and where they stay in when it rains).
        public static NpcStop Stop(int minute, string npcId, string facing = "down") =>
            new NpcStop { Minute = minute, Map = MapOf(npcId), X = StandX, Y = StandY, Facing = facing };

        // A stop that gets the villager to `map` at (x, y) by `arriveBy`: they leave home early enough for the walk (a stop's minute is when they leave
        // the place before it). Never before 06:01, the first minute after the day starts.
        public static NpcStop Depart(int arriveBy, string npcId, string map, int x, int y, string facing = "down")
        {
            var home = Of(npcId);
            var legs = home != null ? MapRoutes.Legs(home.Map, StandX, StandY, map, x, y) : null;
            var walk = 0f;
            if (legs != null) foreach (var leg in legs) walk += leg.Minutes;
            return new NpcStop { Minute = Math.Max(361, arriveBy - (int)Math.Ceiling(walk)), Map = map, X = x, Y = y, Facing = facing };
        }

        // The cells of the alleys between the rows of the north side and of the lane (cobbled in the village). (pure)
        public static bool IsCobbled(int x, int y)
        {
            if (x >= LaneX0 && x <= LaneX1 && y >= 19 && y <= LaneTopY) return true;
            foreach (var h in All)
            {
                if (h.FacesSouth && h.Y0 > NorthRows[0] && (y == h.Y0 - 1 || y == h.Y0 - 2) && x >= WestX0 + 2 && x <= EastX0 + 2) return true;
            }
            return false;
        }

        public static bool InStreet(int x, int y) => x >= StreetX0 && x <= StreetX1 && y >= StreetY0 && y <= StreetY1;
    }
}
