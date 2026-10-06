using System;
using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // A kind of building the player's farm can hold (greenhouse, coop, barn): how big it is, where its door is, which map it opens and what
    // has to be true before the door opens. Types are data; where they stand is saved game state (FarmBuildingState).
    public sealed class FarmBuildingType
    {
        public string Id;
        public int W, H;                 // footprint in cells; the top row is the roof
        public int DoorX;                // the door's offset from the left edge, in the bottom row
        public string InteriorMap;       // the map the door leads to
        public string UnlockFlag;        // the door opens once this flag is set (the carpenter built it)
        public string LockedKey;         // what is said at a door that does not open yet
        public string ReturnSpawn;       // the spawn point on the farm where one arrives coming back out
        public int DefaultX, DefaultY;   // where a new game puts it
    }

    [Serializable]
    public sealed class FarmBuildingState
    {
        public string TypeId;
        public int X, Y;                 // the bottom-left cell of the footprint
    }

    public static class FarmBuildings
    {
        public static readonly FarmBuildingType Greenhouse = new FarmBuildingType
        {
            Id = "greenhouse", W = 9, H = 5, DoorX = 4, InteriorMap = MapIds.Greenhouse, UnlockFlag = MapIds.GreenhouseFlag,
            LockedKey = "greenhouse.locked", ReturnSpawn = "fromGreenhouse", DefaultX = 15, DefaultY = 20,
        };

        public static readonly FarmBuildingType Coop = new FarmBuildingType
        {
            Id = "coop", W = 5, H = 5, DoorX = 2, InteriorMap = MapIds.Coop, UnlockFlag = AnimalRules.BuildingFlag(MapIds.Coop),
            LockedKey = "coop.locked", ReturnSpawn = "fromCoop", DefaultX = 26, DefaultY = 20,
        };

        public static readonly FarmBuildingType Barn = new FarmBuildingType
        {
            Id = "barn", W = 7, H = 5, DoorX = 3, InteriorMap = MapIds.Barn, UnlockFlag = AnimalRules.BuildingFlag(MapIds.Barn),
            LockedKey = "barn.locked", ReturnSpawn = "fromBarn", DefaultX = 32, DefaultY = 20,
        };

        public static readonly IReadOnlyList<FarmBuildingType> Types = new[] { Greenhouse, Coop, Barn };

        // The farmhouse never moves (it holds the bed and is where one wakes up): its footprint is reserved.
        public const int HouseX0 = 4, HouseX1 = 11, HouseY0 = 20, HouseY1 = 24;

        public static FarmBuildingType Type(string id) => Types.FirstOrDefault(t => t.Id == id);

        // A new game, or a save made before buildings could be moved: every kind stands at its default place.
        public static void EnsureDefaults(GameState state)
        {
            foreach (var type in Types)
                if (!state.FarmBuildings.Any(b => b.TypeId == type.Id))
                    state.FarmBuildings.Add(new FarmBuildingState { TypeId = type.Id, X = type.DefaultX, Y = type.DefaultY });
        }

        // The maps that are inside a movable building: no fixed route reaches them (their doors move).
        public static bool IsInteriorMap(string mapId) => Types.Any(t => t.InteriorMap == mapId);

        public static List<FarmBuildingState> Defaults() =>
            Types.Select(t => new FarmBuildingState { TypeId = t.Id, X = t.DefaultX, Y = t.DefaultY }).ToList();

        public static FarmBuildingState Find(GameState state, string typeId) => state.FarmBuildings.FirstOrDefault(b => b.TypeId == typeId);

        public static bool Covers(FarmBuildingType type, FarmBuildingState at, int x, int y) =>
            x >= at.X && x < at.X + type.W && y >= at.Y && y < at.Y + type.H;

        public static (int x, int y) DoorCell(FarmBuildingType type, FarmBuildingState at) => (at.X + type.DoorX, at.Y);

        public static bool IsRoof(FarmBuildingType type, FarmBuildingState at, int y) => y == at.Y + type.H - 1;

        public static bool IsOpen(FarmBuildingType type, GameSession session) =>
            string.IsNullOrEmpty(type.UnlockFlag) || session.HasFlag(type.UnlockFlag);

        public enum Placement { Ok, OffMap, Blocked, OnHouse, OnBuilding, NoWayIn }

        // May `type` stand with its bottom-left cell at (x, y)? `isOpen(x, y)` says whether a cell is open ground for a building (grass or dirt, no wall,
        // no water, no crop, no soil that was dug, nothing placed or growing on it); the cells of the building that is being moved must report
        // open too, so the caller treats them as such. The door needs a free cell in front of it so that one can walk in. (pure)
        public static Placement CanPlace(FarmBuildingType type, int x, int y, int mapW, int mapH, IEnumerable<FarmBuildingState> others, Func<int, int, bool> isOpen)
        {
            if (x < 1 || y < 2 || x + type.W > mapW - 1 || y + type.H > mapH - 1) return Placement.OffMap;
            if (x <= HouseX1 + 1 && x + type.W - 1 >= HouseX0 - 1 && y <= HouseY1 + 1 && y + type.H - 1 >= HouseY0 - 1) return Placement.OnHouse;
            foreach (var other in others)
            {
                if (other.TypeId == type.Id) continue;
                var ot = Type(other.TypeId);
                if (ot == null) continue;
                if (x <= other.X + ot.W && x + type.W - 1 >= other.X - 1 && y <= other.Y + ot.H && y + type.H - 1 >= other.Y - 1) return Placement.OnBuilding;       // keeps a cell between buildings
            }
            for (var cy = y; cy < y + type.H; cy++)
                for (var cx = x; cx < x + type.W; cx++)
                    if (!isOpen(cx, cy)) return Placement.Blocked;
            var doorX = x + type.DoorX;
            if (!isOpen(doorX, y - 1) || !isOpen(doorX, y - 2)) return Placement.NoWayIn;
            return Placement.Ok;
        }
    }
}
