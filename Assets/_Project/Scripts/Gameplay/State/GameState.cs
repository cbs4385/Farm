using System;
using System.Linq;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Where a fixture of a map (the bed, the kitchen) has been moved to; an absent entry means it stands where the scene put it.
    [Serializable]
    public sealed class FixtureState
    {
        public string Map, Id;
        public int X, Y;
        public int Turns;                 // quarter turns, 0 to 3 (the older saves have none: 0)
    }

    [Serializable]
    public sealed class MapState
    {
        public List<FarmTile> Tiles = new List<FarmTile>();
        public List<NodeInstance> Nodes = new List<NodeInstance>();   // trees, rocks, weeds standing on the map
        public bool ClutterSeeded;                                    // starting clutter has been scattered
        public int LastSpawnDay = -1;                                 // the last day (TotalDays) the spawn tables ran here
        public List<PlacedObject> Objects = new List<PlacedObject>(); // chests, machines, sprinklers, scarecrows the player placed
    }

    // The complete, serializable state of one playthrough. MonoBehaviours are views over this, never the source of truth.
    // Bump CurrentVersion and add an ISaveMigration whenever the shape changes.
    [Serializable]
    public sealed class GameState
    {
        public const int CurrentVersion = 1;
        public const int StartingBackpackSlots = 12;
        public const string HammerGivenFlag = "tutorial.hammer_given";      // saves from before the mallet existed get one, once
        public const int HotbarSlots = 12;

        public int SaveVersion = CurrentVersion;

        public string PlayerName = "Farmer";
        public string FarmName = "Farm";
        public AvatarData Avatar = AvatarOptions.Default();       // how the farmer looks (chosen when the game starts)

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

        // Where the greenhouse, coop and barn stand on the farm (empty in saves made before they could be moved: FarmBuildings.EnsureDefaults fills it).
        public List<FarmBuildingState> FarmBuildings = new List<FarmBuildingState>();
        public List<FixtureState> Fixtures = new List<FixtureState>();          // the bed, the kitchen: where the player has put them
        public string Weather = "sunny";

        // Tomorrow's weather, rolled a day ahead so it can be shown as a forecast. Empty until the first roll.
        public string ForecastWeather = "";

        // Fatigue carried over from a sleep that was not in a bed (collapsing at 06:00); 0..1. See FatigueState.
        public float FatigueCarried;

        // Seeds everything that must be random per playthrough but repeatable within it (weather so far).
        public int WorldSeed;
        public Dictionary<string, int> SkillXp = new Dictionary<string, int>();

        // Upgrade tier of each tool the player owns, by item id: 0 basic, 1 copper, 2 iron, 3 gold.
        public Dictionary<string, int> ToolTiers = new Dictionary<string, int>();

        // Tools handed in for upgrading, and the one-off upgrades (backpack, energy) already bought.
        public List<PendingUpgrade> PendingUpgrades = new List<PendingUpgrade>();
        public HashSet<string> UpgradesDone = new HashSet<string>();
        public HashSet<string> Flags = new HashSet<string>();

        // Open-ended story state. Additive fields like these need no save migration: older saves load with
        // empty collections.
        public Dictionary<string, int> Vars = new Dictionary<string, int>();

        // Private JSON blobs owned by modules, keyed by module id (see GameSession.GetModuleData).
        public Dictionary<string, string> ModuleData = new Dictionary<string, string>();

        // Village life (M2). All additive: older saves load with empty collections.
        public Dictionary<string, NpcState> Npcs = new Dictionary<string, NpcState>();
        public Dictionary<string, QuestProgress> Quests = new Dictionary<string, QuestProgress>();
        public List<string> Mailbox = new List<string>();        // letters waiting in the mailbox
        public List<string> MailKept = new List<string>();       // letters already taken, readable in the journal
        public HashSet<string> EventsSeen = new HashSet<string>();
        public HashSet<string> Recipes = new HashSet<string>();  // recipes the player has learned
        public List<BoardJob> Board = new List<BoardJob>();      // help-wanted jobs currently posted
        public MineState Mine = new MineState();                 // progress underground (M3)
        public List<AnimalState> Animals = new List<AnimalState>(); // the farm's animals (M3)
        public HashSet<string> Collected = new HashSet<string>();   // every item the player has ever held (collections tab)
        public Dictionary<string, int> ShippedTotals = new Dictionary<string, int>();   // items sold through the bin, by item id
        public int TotalEarned;                                     // gold earned from the bin, ever
        public HashSet<string> Professions = new HashSet<string>(); // chosen professions

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

        public static GameState NewGame(string playerName, string farmName, Func<string, int> maxStack, int? worldSeed = null, AvatarData avatar = null)
        {
            var state = new GameState
            {
                WorldSeed = worldSeed ?? Guid.NewGuid().GetHashCode(),
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Farmer" : playerName.Trim(),
                FarmName = string.IsNullOrWhiteSpace(farmName) ? "Farm" : farmName.Trim(),
                Avatar = AvatarOptions.Sanitize(avatar),
            };

            var pack = new Inventory(StartingBackpackSlots, maxStack);
            pack.Add(ItemIds.Hoe, 1);
            pack.Add(ItemIds.WateringCan, 1);
            pack.Add(ItemIds.Axe, 1);
            pack.Add(ItemIds.Pickaxe, 1);
            pack.Add(ItemIds.Scythe, 1);
            pack.Add(ItemIds.Seed("parsnip"), 15);
            pack.Add(ItemIds.Sword, 1);
            pack.Add(ItemIds.Hammer, 1);
            state.Flags.Add(HammerGivenFlag);
            pack.Add(ItemIds.Machine("chest"), 1);      // somewhere to put things: playtesters never found the recipe
            state.Backpack = pack.ToData();
            return state;
        }
    }

    public static class MapIds
    {
        public const string Farm = "Farm";
        public const string FarmHouse = "FarmHouse";
        public const string Village = "Village";
        public const string Forest = "Forest";
        public const string Beach = "Beach";
        public const string GeneralStore = "GeneralStore";
        public const string Blacksmith = "Blacksmith";
        public const string Carpenter = "Carpenter";
        public const string Saloon = "Saloon";
        public const string Clinic = "Clinic";
        public const string Library = "Library";
        public const string Greenhouse = "Greenhouse";
        public const string Coop = "Coop";
        public const string Barn = "Barn";
        public const string CommunityHall = "CommunityHall";
        public const string Mine = "Mine";

        // The villagers' homes, a cottage and a room inside it for each (NpcHomes).
        public const string HomeTilda = "HomeTilda", HomeBram = "HomeBram", HomeIone = "HomeIone", HomeMarcus = "HomeMarcus", HomeOdalys = "HomeOdalys",
            HomeWren = "HomeWren", HomeFelix = "HomeFelix", HomeJuno = "HomeJuno", HomeHazel = "HomeHazel", HomePiper = "HomePiper",
            HomeDorian = "HomeDorian", HomeElara = "HomeElara";

        public static readonly string[] Homes =
        {
            HomeTilda, HomeBram, HomeIone, HomeMarcus, HomeOdalys, HomeWren, HomeFelix, HomeJuno, HomeHazel, HomePiper, HomeDorian, HomeElara,
        };

        public static bool IsHome(string mapId) => System.Array.IndexOf(Homes, mapId) >= 0;

        // The gated slot at the top of the Forest. Nothing is behind it in the base game (the gate is brambles
        // while the flag `woods.open` is off); an optional layer ships the scene and opens the gate.
        public const string Woods = "Woods";
        public const string WoodsOpenFlag = "woods.open";

        // Set when the carpenter has built the greenhouse on the farm.
        public const string GreenhouseFlag = "farm.greenhouse";

        // The insides of buildings: going between one of these and anywhere else is a walk through a door (and so shows the door swinging).
        public static readonly string[] Interiors = new[]
        {
            FarmHouse, GeneralStore, Blacksmith, Carpenter, Saloon, Clinic, Library, Greenhouse, Coop, Barn, CommunityHall,
        }.Concat(Homes).ToArray();

        public static bool IsInterior(string mapId) => System.Array.IndexOf(Interiors, mapId) >= 0;

        // Dungeon scenes: built, but not part of the village's walkable world (no schedule routes lead to them).
        public static readonly string[] Dungeons = { Mine };

        // Every map scene that ships in the base game, in a stable order.
        public static readonly string[] All = new[]
        {
            Farm, FarmHouse, Village, Forest, Beach, GeneralStore, Blacksmith, Carpenter, Saloon, Clinic, Library, Greenhouse, Coop, Barn, CommunityHall,
        }.Concat(Homes).ToArray();
    }
}
