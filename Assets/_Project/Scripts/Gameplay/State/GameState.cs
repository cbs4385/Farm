using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    [Serializable]
    public sealed class MapState
    {
        public List<FarmTile> Tiles = new List<FarmTile>();
    }

    // The complete, serializable state of one playthrough. MonoBehaviours are views over this, never the source of truth.
    // Bump CurrentVersion and add an ISaveMigration whenever the shape changes.
    [Serializable]
    public sealed class GameState
    {
        public const int CurrentVersion = 1;
        public const int StartingBackpackSlots = 12;
        public const int HotbarSlots = 12;

        public int SaveVersion = CurrentVersion;

        public string PlayerName = "Farmer";
        public string FarmName = "Farm";

        // Calendar (flattened so the save format does not depend on GameDateTime's layout)
        public int Year = 1;
        public int SeasonIndex;
        public int Day = 1;
        public int MinuteOfDay = GameDateTime.DayStartMinute;

        public int Gold = 500;
        public int Energy = 270;
        public int MaxEnergy = 270;
        public int Health = 100;
        public int MaxHealth = 100;

        public InventoryData Backpack;
        public int SelectedHotbar;

        public string CurrentMap = MapIds.Farm;
        public string SpawnPoint = "default";
        public Dictionary<string, MapState> Maps = new Dictionary<string, MapState>();

        public List<ItemStack> ShippingBin = new List<ItemStack>();
        public string Weather = "sunny";

        // Tomorrow's weather, rolled a day ahead so it can be shown as a forecast. Empty until the first roll.
        public string ForecastWeather = "";

        // Fatigue carried over from a sleep that was not in a bed (collapsing at 06:00); 0..1. See FatigueState.
        public float FatigueCarried;

        // Seeds everything that must be random per playthrough but repeatable within it (weather so far).
        public int WorldSeed;
        public Dictionary<string, int> SkillXp = new Dictionary<string, int>();
        public HashSet<string> Flags = new HashSet<string>();

        // Open-ended story state. Additive fields like these need no save migration: older saves load with
        // empty collections.
        public Dictionary<string, int> Vars = new Dictionary<string, int>();

        // Private JSON blobs owned by modules, keyed by module id (see GameSession.GetModuleData).
        public Dictionary<string, string> ModuleData = new Dictionary<string, string>();

        public GameDateTime GetDate() =>
            new GameDateTime(Year, (Season)SeasonIndex, Day, MinuteOfDay);

        public void SetDate(GameDateTime d)
        {
            Year = d.Year;
            SeasonIndex = (int)d.Season;
            Day = d.Day;
            MinuteOfDay = d.MinuteOfDay;
        }

        public MapState GetMap(string mapId)
        {
            if (!Maps.TryGetValue(mapId, out var map))
            {
                map = new MapState();
                Maps[mapId] = map;
            }
            return map;
        }

        public static GameState NewGame(string playerName, string farmName, Func<string, int> maxStack, int? worldSeed = null)
        {
            var state = new GameState
            {
                WorldSeed = worldSeed ?? Guid.NewGuid().GetHashCode(),
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Farmer" : playerName.Trim(),
                FarmName = string.IsNullOrWhiteSpace(farmName) ? "Farm" : farmName.Trim(),
            };

            var pack = new Inventory(StartingBackpackSlots, maxStack);
            pack.Add(ItemIds.Hoe, 1);
            pack.Add(ItemIds.WateringCan, 1);
            pack.Add(ItemIds.Axe, 1);
            pack.Add(ItemIds.Pickaxe, 1);
            pack.Add(ItemIds.Scythe, 1);
            pack.Add(ItemIds.Seed("parsnip"), 15);
            state.Backpack = pack.ToData();
            return state;
        }
    }

    public static class MapIds
    {
        public const string Farm = "Farm";
        public const string FarmHouse = "FarmHouse";
    }
}
