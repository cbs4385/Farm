using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Where each villager lives (playtest 2026-10-07: "npcs should live in homes; currently the only home is the player's"; before this a villager's home
    // was the shop they worked in). Twelve cottages on three streets round the village, one for each villager, with a room inside for each. The scene
    // builder, the route table, the schedules, the map picture and the tests all read this one table, so they cannot drift apart.
    //
    // The streets: three short cobbled lanes, each with four cottages (two rows of two, either side of the lane, the doors on the south walls looking onto
    // an alley that leads to the lane). They are spread round the village so that the homes are all about the same distance from its middle (the village
    // is 75 x 54 cells, the middle about (37, 27)): one in the north-west, one in the north-east and one in the south-east, clear of the old buildings
    // (x 5 to 48, y 6 to 29), the main road (y 16 to 18) and the lane to the forest and the beach (x 24 to 26).
    public static class NpcHomes
    {
        public sealed class Home
        {
            public string Npc, Map, Style;
            public int X0, X1, Y0, Y1, DoorX;
            public int Street;                            // the index in Streets
            public bool FacesSouth;                       // the door is on the south wall (north of the road) or the north wall (south of it)

            public int DoorY => FacesSouth ? Y0 : Y1;     // the door cell in the village
            public int OutsideY => FacesSouth ? Y0 - 1 : Y1 + 1;   // where one stands (and appears) outside it
        }

        public const int InteriorW = 10, InteriorH = 8, InteriorDoorX = 4;      // every room is the same shape: floor x 1..8, y 1..6, the door at (4, 0)
        public const int StandX = 3, StandY = 3;                                // where the owner stands inside, mornings and nights and when it rains
        public const string OpenCondition = "hour>=7 && hour<21";                  // the door is locked from 21:00 to 7:00: they are asleep
        public const string LockedKey = "home.locked";

        // Sleeping: a villager's nights start and end in their bed (a stop on the bed's cell, facing "sleep"), after rising at RiseMinute they stand at
        // the usual place. A friend (FriendHearts or more) is let in at any hour: the door is only locked for everyone else.
        // The bed is a double bed, two cells square, covering x 1..2, y 5..6 (the same piece the farmhouse has). The villager's stop is on the one cell of it
        // that can be reached from the room (BedX, BedY), and they are drawn lying in the middle of the bed (SleepOffset from that cell's centre).
        public const int BedX = 2, BedY = 5, BedCellsX = 1, BedCellsY = 5, BedSize = 2;
        public static readonly Vector2 SleepOffset = new Vector2(-0.5f, 0.5f);
        public const float PillowLift = 0.4f;                  // the sleeper's head and shoulders lie this far above the middle of the bed, on the pillow
        public const int RiseMinute = 6 * 60 + 45, BedtimeAfter = 45;     // the usual rising time, and how long after getting home they go to bed
        public const int FriendHearts = 4;

        // The condition for a villager's door: open by day to everyone, and at any hour to a friend.
        public static string OpenConditionFor(string npcId) => $"({OpenCondition}) || hearts:{npcId}>={FriendHearts}";

        // A street: the left cell of its three-cell lane and the y of the south row of cottages (the second row is RowRise above it).
        public sealed class Street { public int LaneX, RowY; }

        public const int HomesPerStreet = 4, CottageW = 6, CottageH = 4, RowRise = 6, LaneWidth = 3;
        public static readonly Street[] Streets =
        {
            new Street { LaneX = 13, RowY = 38 },       // north-west
            new Street { LaneX = 61, RowY = 38 },       // north-east
            new Street { LaneX = 59, RowY = 4 },        // south-east
        };

        // The middle of the village map, and how far a street (the middle of its lane) and a home (its door) are from it, in cells. (pure)
        public static readonly Vector2 VillageCentre = new Vector2(MapLayout.VillageW * 0.5f, MapLayout.VillageH * 0.5f);
        public static Vector2 StreetCentre(Street street) => new Vector2(street.LaneX + LaneWidth * 0.5f, street.RowY + (RowRise + CottageH) * 0.5f - 1f);
        public static float DistanceFromCentre(Home home) => Vector2.Distance(new Vector2(home.DoorX + 0.5f, home.DoorY + 0.5f), VillageCentre);

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
                var street = i / HomesPerStreet;
                var slot = i % HomesPerStreet;                  // 0 and 1: the south row (west, east); 2 and 3: the north row
                var east = slot % 2 == 1;
                var x0 = east ? Streets[street].LaneX + LaneWidth + 1 : Streets[street].LaneX - CottageW - 1;
                var y0 = Streets[street].RowY + slot / 2 * RowRise;
                homes.Add(new Home
                {
                    Npc = npc, Map = map, Style = "cottage" + (1 + i % 4), Street = street, FacesSouth = true,
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

        public static NpcStop Bed(int minute, string npcId) =>
            new NpcStop { Minute = minute, Map = MapOf(npcId), X = BedX, Y = BedY, Facing = NpcSchedule.SleepFacing };

        // Puts the nights into a villager's days: a day that starts at home starts asleep and rises at RiseMinute, and a day that ends at home goes to
        // bed an hour after getting there (or at once when that would be after the day's end). Edits the entries in place and returns them.
        public static List<NpcScheduleEntry> WithSleep(string npcId, IEnumerable<NpcScheduleEntry> entries)
        {
            var result = new List<NpcScheduleEntry>(entries);
            var home = Of(npcId);
            if (home == null) return result;
            bool AtHome(NpcStop s) => s.Map == home.Map && s.X == StandX && s.Y == StandY;
            foreach (var entry in result)
            {
                var stops = entry.Stops;
                if (stops.Count == 0) continue;
                if (AtHome(stops[0]) && stops[0].Minute < RiseMinute)
                {
                    // They rise at the usual time, or earlier if they have to leave earlier (after getting out of bed and over to the usual place).
                    var first = stops[0];
                    var rise = RiseMinute;
                    if (stops.Count > 1) rise = Math.Min(rise, stops[1].Minute - 1 - (int)Math.Ceiling(Walk(home.Map, BedX, BedY, home.Map, StandX, StandY)));
                    stops[0] = Bed(first.Minute, npcId);
                    stops.Insert(1, Stop(Math.Max(first.Minute + 1, rise), npcId, first.Facing));
                }
                var last = stops[stops.Count - 1];
                if (stops.Count > 1 && AtHome(last))
                {
                    var previous = stops[stops.Count - 2];
                    var arrives = last.Minute + (int)Math.Ceiling(Walk(previous.Map, previous.X, previous.Y, last.Map, last.X, last.Y));
                    if (arrives + BedtimeAfter < GameDateTime.DayEndMinute) stops.Add(Bed(arrives + BedtimeAfter, npcId));
                }
            }
            return result;
        }

        static float Walk(string fromMap, int fromX, int fromY, string toMap, int toX, int toY)
        {
            var legs = MapRoutes.Legs(fromMap, fromX, fromY, toMap, toX, toY);
            var walk = 0f;
            if (legs != null) foreach (var leg in legs) walk += leg.Minutes;
            return walk;
        }

        // A stop that gets the villager to `map` at (x, y) by `arriveBy`: they leave home early enough for the walk (a stop's minute is when they leave
        // the place before it). Never before 06:01, the first minute after the day starts.
        public static NpcStop Depart(int arriveBy, string npcId, string map, int x, int y, string facing = "down")
        {
            var home = Of(npcId);
            var legs = home != null ? MapRoutes.Legs(home.Map, StandX, StandY, map, x, y) : null;
            var walk = 0f;
            if (legs != null) foreach (var leg in legs) walk += leg.Minutes;
            return new NpcStop { Minute = Math.Max(GameDateTime.DayStartMinute + 5, arriveBy - (int)Math.Ceiling(walk)), Map = map, X = x, Y = y, Facing = facing };
        }

        // The cells that are cobbled in the village: each street's lane and the two-cell alleys in front of its rows of doors. (pure)
        public static bool IsCobbled(int x, int y)
        {
            foreach (var street in Streets)
            {
                if (x >= street.LaneX && x < street.LaneX + LaneWidth && y >= street.RowY - 2 && y <= street.RowY + RowRise + CottageH - 1) return true;
                for (var row = 0; row < 2; row++)
                {
                    var door = street.RowY + row * RowRise;
                    if ((y == door - 1 || y == door - 2) && x >= street.LaneX - CottageW + 1 && x <= street.LaneX + LaneWidth + 3) return true;
                }
            }
            return false;
        }

        // The land a street takes, with a margin (no scattered trees there).
        public static bool InStreet(int x, int y)
        {
            foreach (var street in Streets)
                if (x >= street.LaneX - CottageW - 2 && x <= street.LaneX + LaneWidth + CottageW + 1 && y >= street.RowY - 3 && y <= street.RowY + RowRise + CottageH + 1) return true;
            return false;
        }
    }
}
