using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public readonly struct StatsChanged { }
    public readonly struct ToastRequested
    {
        public readonly string Message;
        public ToastRequested(string message) { Message = message; }
    }
    // Published after the player finishes an action that spent energy (a tool use that worked).
    public readonly struct EnergyActionCompleted
    {
        public readonly string Action; public readonly int Cost;
        public EnergyActionCompleted(string action, int cost) { Action = action; Cost = cost; }
    }
    public readonly struct FlagChanged
    {
        public readonly string Flag; public readonly bool Value;
        public FlagChanged(string flag, bool value) { Flag = flag; Value = value; }
    }
    public readonly struct VarChanged
    {
        public readonly string Name; public readonly int OldValue; public readonly int NewValue;
        public VarChanged(string name, int oldValue, int newValue) { Name = name; OldValue = oldValue; NewValue = newValue; }
    }
    public readonly struct SkillLevelUp
    {
        public readonly string Skill; public readonly int Level;
        public SkillLevelUp(string skill, int level) { Skill = skill; Level = level; }
    }
    public readonly struct DayCycleFinished
    {
        public readonly DaySummary Summary;
        public DayCycleFinished(DaySummary summary) { Summary = summary; }
    }

    // The running game: state + clock + backpack + per-map farm grids. A persistent service.
    // Scene objects read from and write to this; it is the only thing that touches GameState directly.
    public sealed partial class GameSession : MonoBehaviour
    {
        readonly Dictionary<string, FarmGrid> _grids = new Dictionary<string, FarmGrid>();
        readonly Dictionary<string, NodeGrid> _nodeGrids = new Dictionary<string, NodeGrid>();
        readonly Dictionary<string, ObjectGrid> _objectGrids = new Dictionary<string, ObjectGrid>();
        RecipeCatalog _recipeCatalog;
        PlaceableCatalog _placeableCatalog;
        NodeCatalog _nodeCatalog;
        NpcCatalog _npcCatalog;
        UpgradeCatalog _upgradeCatalog;
        SpawnCatalog _spawnCatalog;

        EventBus _bus;
        GameDatabase _db;
        SaveService _saves;
        WeatherCatalog _weatherCatalog;
        bool _endingDay;
        bool _sleeping;

        public GameState State { get; private set; }
        public GameClock Clock { get; private set; }
        public Inventory Backpack { get; private set; }
        public int ActiveSlot { get; private set; } = -1;
        public bool InGame => State != null;

        // The whole current state as JSON (for a bug report: the same data a save holds).
        public string StateJson() => Newtonsoft.Json.JsonConvert.SerializeObject(State);
        public bool IsSleeping => _sleeping;
        public GameDatabase Db => _db;

        // Weather definitions (core assets plus module packs). Built on first use, after packs have merged.
        // Resource node definitions (core assets plus module packs), built on first use.
        public SpawnCatalog SpawnTables => _spawnCatalog ?? (_spawnCatalog = SpawnCatalog.From(_db));

        // Runs the map's spawn tables for the days since the player was last here (at most three; three on a first
        // visit, so a new map has something on it). `candidatesFor` gives the free cells a table may use.
        public int RunSpawns(string mapId, Func<SpawnTableDefinition, IReadOnlyList<(int x, int y)>> candidatesFor)
        {
            if (!InGame) return 0;
            var map = State.GetMap(mapId);
            var today = Clock.Now.TotalDays;
            var rounds = map.LastSpawnDay < 0 ? 3 : Mathf.Min(3, today - map.LastSpawnDay);
            if (rounds <= 0) return 0;
            map.LastSpawnDay = today;

            var placed = 0;
            var grid = GetNodes(mapId);
            foreach (var table in SpawnTables.For(mapId))
            {
                var cells = candidatesFor(table);
                for (var r = 0; r < rounds; r++)
                    placed += SpawnModel.Spawn(grid, table, cells, Nodes, World, Clock.Now.Season, Luck, State.WorldSeed, today - r);
            }
            return placed;
        }


        // Events waiting to play (the `event:` effect); the event director on the loaded map plays them in order. Not saved.
        public List<string> PendingEvents { get; } = new List<string>();

        // A memory being replayed (T-102): nothing here is saved.
        public string MemoryId { get; set; }
        public string MemoryReturnMap { get; set; }
        public Vector3 MemoryReturnPosition { get; set; }
        public bool MemoryHasReturnPosition { get; set; }
        public bool MemoryRestorePosition { get; set; }

        // Set by the new-game screen: the farm scene tells how the player came by the farm once, on arrival.
        public bool PendingIntro { get; set; }

        // Authored story data (dialogue, quests, letters, events); loaded from Resources/Story by GameServices.
        public StoryContent Story { get; set; } = new StoryContent();

        public RecipeCatalog Recipes => _recipeCatalog ?? (_recipeCatalog = RecipeCatalog.From(_db));
        public PlaceableCatalog Placeables => _placeableCatalog ?? (_placeableCatalog = PlaceableCatalog.From(_db));

        // The villagers (core assets plus module packs). Built on first use, after packs have merged.
        public NpcCatalog Npcs => _npcCatalog ?? (_npcCatalog = NpcCatalog.From(_db));

        public NodeCatalog Nodes => _nodeCatalog ?? (_nodeCatalog = NodeCatalog.From(_db));

        public WeatherCatalog Weather => _weatherCatalog ?? (_weatherCatalog = WeatherCatalog.From(_db));
        public IDictionary<string, FarmGrid> Grids => _grids;

        // Extension points for optional layers; see GameHooks.
        public GameHooks Hooks { get; } = new GameHooks();

        // What conditions (Conditions.Evaluate) see. Live: always reflects the current game.
        public IWorldQuery World => BuildWorld();

        StateWorldQuery BuildWorld()
        {
            var memory = new System.Lazy<LineMemory>(() => LineMemory.Load(this));
            return new StateWorldQuery(State, Clock)
            {
                ItemCounter = id => Backpack != null ? Backpack.Count(id) : 0,
                FestivalDays = () => StoryCalendar.DaysUntilFestival(Story?.Events, Clock.Now),
                BirthdayDays = id => Npcs?.Get(id) is NpcDefinition npc ? StoryCalendar.DaysUntil(Clock.Now, npc.BirthdaySeason, npc.BirthdayDay) : -1,
                CropCounter = CountCrops,
                HeardLookup = id => memory.Value.HeardAnywhere(id),
                DeedLookup = kind => DeedLog.IsRecent(DeedState.Load(this), kind, Clock.Now.TotalDays),
                MoodLookup = id => MoodModel.Of(this, id).ToString().ToLowerInvariant(),
            };
        }

        int CountCrops()
        {
            var count = 0;
            foreach (var grid in _grids.Values)
                foreach (var tile in grid.Tiles)
                    if (tile.Crop != null) count++;
            return count;
        }

        // The player's luck, -1..+1 (0 neutral), after every module's modifiers. Roll-based systems should use it.
        public float Luck => InGame ? Hooks.ComputeLuck(State) : 0f;

        // Fatigue: what is carried from a collapse, and the rating right now (0 = rested, 1 = a whole night awake).
        public FatigueState Fatigue => new FatigueState(InGame ? State.FatigueCarried : 0f);
        public float FatigueRating => InGame ? Fatigue.LuckRating(Clock.Now.MinuteOfDay) : 0f;

        public int HorrorLevel => CurrentHorrorLevel;

        // The intensity setting (0 off, 1 mild, 2 full), read live. Tests can force a level with HorrorLevelOverride.
        public static int? HorrorLevelOverride;
        public static int CurrentHorrorLevel =>
            HorrorLevelOverride ?? (ServiceLocator.TryGet<SettingsStore>(out var s) ? s.Current.HorrorLevel : 2);


        public void Init(EventBus bus, GameDatabase db, SaveService saves)
        {
            _bus = bus;
            _db = db;
            _saves = saves;
            Hooks.AddLuckModifier(new FatigueLuckModifier(() => Clock != null ? Clock.Now.MinuteOfDay : GameDateTime.DayStartMinute));
            Hooks.AddWorldObjectSource(new StoredItemsSource(this, WorldObjectKinds.CraftedItem));
            Hooks.AddWorldObjectSource(new StoredItemsSource(this, WorldObjectKinds.PlantProduct));
            Hooks.AddWorldObjectSource(new StandingCropsSource(this));
            Hooks.AddWorldObjectSource(new AnimalSource(this));
            _bus.Subscribe<MinuteChanged>(OnMinuteChanged);
            _bus.Subscribe<VarChanged>(OnStoryStateChanged);
            _bus.Subscribe<FlagChanged>(OnStoryStateChanged);
            _bus.Subscribe<StatsChanged>(OnStoryStateChanged);
            _bus.Subscribe<DayCycleFinished>(e => Achievements.Check(this));
            _bus.Subscribe<FlagChanged>(e => Achievements.Check(this));
            _bus.Subscribe<FlagChanged>(e => { if (e.Value) Reactions.Fire(this, "flag:" + e.Flag); });
            _bus.Subscribe<QuestStarted>(e => Reactions.Fire(this, "quest.start:" + e.QuestId));
            _bus.Subscribe<QuestCompleted>(e => { Reactions.Fire(this, "quest.done:" + e.QuestId); DeedLog.Record(this, DeedLog.Quest); });
            _bus.Subscribe<SkillLevelUp>(e => { Reactions.Fire(this, "skill.up:" + e.Skill); AudioService.PlayIfAvailable(Sfx.LevelUp); DeedLog.Record(this, DeedLog.Skill); });
            _bus.Subscribe<EventFinished>(e => { Reactions.Fire(this, "event:" + e.EventId); DeedLog.Record(this, DeedLog.Scene); });
            _bus.Subscribe<SeasonChanged>(e => Reactions.Fire(this, "season:" + e.Season.ToString().ToLowerInvariant()));
            _bus.Subscribe<NpcGifted>(e => { Reactions.Fire(this, "gift:" + e.NpcId); DeedLog.Record(this, DeedLog.Gift); });
            _bus.Subscribe<RandomEventHappened>(e => Reactions.Fire(this, "random:" + e.EventId));
            _bus.Subscribe<DayStarted>(e => Reactions.NewDay(this));
        }

        void OnDestroy()
        {
            _bus?.Unsubscribe<MinuteChanged>(OnMinuteChanged);
            _bus?.Unsubscribe<VarChanged>(OnStoryStateChanged);
            _bus?.Unsubscribe<FlagChanged>(OnStoryStateChanged);
            _bus?.Unsubscribe<StatsChanged>(OnStoryStateChanged);
        }

        // Quests that start or finish by themselves react to anything that changes the game.
        void OnStoryStateChanged<T>(T _)
        {
            if (!InGame || _sleeping || _endingDay) return;
            QuestLog.Tick(this);
        }

        // The first time the clock passes 22:00 in a save, tell the player what staying up costs (once).
        void OnMinuteChanged(MinuteChanged e)
        {
            if (!InGame || _sleeping) return;
            if (!FatigueModel.NeedsWarning(e.Now.MinuteOfDay, HasFlag(FatigueModel.WarnedFlag))) return;
            SetFlag(FatigueModel.WarnedFlag);
            UiAccess.Run(ui => ui.ShowMessage("late_night.warning"));
        }

        // The game stops while its window is not in focus (the Steam overlay, alt-tab): the clock must not run away.
        bool _focusPaused;

        void OnApplicationFocus(bool focused)
        {
            if (Clock == null || !InGame) return;
            if (!focused && !_focusPaused) { _focusPaused = true; Clock.Pause(); }
            else if (focused && _focusPaused) { _focusPaused = false; Clock.Resume(); }
        }

        void Update()
        {
            if (InGame) Clock.Tick(Time.deltaTime);
        }

        // ---- lifecycle -----------------------------------------------------------------------------------------

        public void BeginNewGame(string playerName, string farmName, int slot, AvatarData avatar = null)
        {
            // Defence in depth: the new-game screen already refuses blocked names (T-145).
            Begin(GameState.NewGame(NameFilter.Sanitize(playerName, "Farmer"), NameFilter.Sanitize(farmName, "Meadow"), _db.MaxStack, avatar: avatar), slot);
            StoryDay.NewGame(this);
            Save();
        }

        // For starting a map scene directly in the Editor: a throwaway game that is never saved (slot -1).
        public void BeginDevGame() => Begin(GameState.NewGame("Dev", "Dev Farm", _db.MaxStack), -1);

        // Starts play from an already loaded state without a save slot (tests and tools).
        public void BeginState(GameState state) => Begin(state, -1);

        public bool BeginLoad(int slot, out string error)
        {
            if (!_saves.TryLoad(slot, out var state, out error)) return false;
            Begin(state, slot);
            return true;
        }

        public void EndGame()
        {
            State = null;
            Clock = null;
            Backpack = null;
            ActiveSlot = -1;
            _grids.Clear();
            _objectGrids.Clear();
        }

        void Begin(GameState state, int slot)
        {
            if (_endingDay) _endingDay = false;
            State = state;
            ActiveSlot = slot;
            // Saves from before the forecast existed (and brand new games) get one for tomorrow.
            if (string.IsNullOrEmpty(state.ForecastWeather))
                state.ForecastWeather = WeatherRoller.Roll(state.GetDate().StartOfNextDay(), Weather, state.WorldSeed, Hooks, state);
            Clock = new GameClock(state.GetDate(), _bus);
            ApplySettings();
            Achievements.SyncToPlatform(this);
            Backpack = Inventory.FromData(state.Backpack, _db.MaxStack);
            Backpack.Changed += OnBackpackChanged;
            _grids.Clear();
            _nodeGrids.Clear();
            _objectGrids.Clear();
            foreach (var kv in state.Maps)
            {
                _grids[kv.Key] = FarmGrid.FromTiles(kv.Value.Tiles);
                _grids[kv.Key].AllSeasons = kv.Key == MapIds.Greenhouse;
                _grids[kv.Key].RainsToday = RainsOn(kv.Key);
                _nodeGrids[kv.Key] = NodeGrid.FromNodes(kv.Value.Nodes);
                _objectGrids[kv.Key] = ObjectGrid.FromList(kv.Value.Objects, _db.MaxStack);
            }
            // Saves from before the builder's mallet existed get one, once (what does not fit waits in the mailbox).
            if (!state.Flags.Contains(GameState.HammerGivenFlag) && _db.TryGetItem(ItemIds.Hammer, out _))
            {
                state.Flags.Add(GameState.HammerGivenFlag);
                GiveItem(ItemIds.Hammer);
            }
            _bus.Publish(new StatsChanged());
        }

        void OnBackpackChanged()
        {
            if (InGame && Backpack != null)
                for (var i = 0; i < Backpack.Capacity; i++)
                {
                    var stack = Backpack.Get(i);
                    if (stack != null) State.Collected.Add(stack.ItemId);
                }
            _bus.Publish(new StatsChanged());
        }

        public IEnumerable<KeyValuePair<string, ObjectGrid>> AllObjectGrids => _objectGrids;

        public ObjectGrid GetObjects(string mapId)
        {
            if (!_objectGrids.TryGetValue(mapId, out var grid))
            {
                grid = ObjectGrid.FromList(null, _db.MaxStack);
                _objectGrids[mapId] = grid;
            }
            return grid;
        }

        public NodeGrid GetNodes(string mapId)
        {
            if (!_nodeGrids.TryGetValue(mapId, out var grid))
            {
                grid = new NodeGrid();
                _nodeGrids[mapId] = grid;
            }
            return grid;
        }

        // Scatters the starting clutter on a map the first time it is visited (the cells are chosen by the scene).
        public void EnsureClutter(string mapId, IEnumerable<(int x, int y)> candidates, float density)
        {
            var map = State.GetMap(mapId);
            if (map.ClutterSeeded) return;
            map.ClutterSeeded = true;
            NodeSpawner.Generate(GetNodes(mapId), candidates, Nodes.All, State.WorldSeed, mapId, density);
        }

        // Is the day's weather one that waters crops here? (The greenhouse stays dry.)
        bool RainsOn(string mapId) => mapId != MapIds.Greenhouse && State != null && !string.IsNullOrEmpty(State.Weather) && Weather.Get(State.Weather).WateringCrops;

        public FarmGrid GetGrid(string mapId)
        {
            if (!_grids.TryGetValue(mapId, out var grid))
            {
                grid = new FarmGrid { AllSeasons = mapId == MapIds.Greenhouse, RainsToday = RainsOn(mapId) };
                _grids[mapId] = grid;
            }
            return grid;
        }

        // ---- saving --------------------------------------------------------------------------------------------

        public void SyncToState()
        {
            State.SetDate(Clock.Now);
            State.Backpack = Backpack.ToData();
            foreach (var kv in _grids) State.GetMap(kv.Key).Tiles = kv.Value.ToList();
            foreach (var kv in _nodeGrids) State.GetMap(kv.Key).Nodes = kv.Value.ToList();
            foreach (var kv in _objectGrids) State.GetMap(kv.Key).Objects = kv.Value.ToList();
        }

        public bool Save()
        {
            if (!InGame || ActiveSlot < 0) return false;
            SyncToState();
            _saves.Save(ActiveSlot, State);
            return true;
        }

        // The player's quality-of-life settings (day length, energy cost) read live from the settings store.
        public static SettingsData Settings => ServiceLocator.TryGet<SettingsStore>(out var s) ? s.Current : null;

        public void ApplySettings()
        {
            if (Clock != null && Settings != null) Clock.SecondsPerStep = Settings.SecondsPerStep;
        }



        // ---- day cycle -----------------------------------------------------------------------------------------

        // Runs the overnight logic, autosaves, and tells the UI to show the summary. Re-entrancy safe.
        public DaySummary EndDay(bool passedOut)
        {
            if (!InGame || _endingDay) return null;
            _endingDay = true;

            foreach (var nodes in _nodeGrids.Values) nodes.Grow(Nodes.Get);                // saplings grow a day
            SyncToState();
            var summary = DayCycle.EndDay(State, Clock, _grids,
                id => _db.TryGetItem(id, out var i) ? i : null,
                id => _db.TryGetCrop(id, out var c) ? c : null,
                passedOut,
                Hooks,
                Weather,
                Placeables);
            try { StoryDay.Dawn(this, summary); }
            catch (Exception e) { Log.Error($"Story dawn failed: {e}"); }
            Save();

            _endingDay = false;
            if (summary != null && summary.Earnings >= DeedLog.BigSaleGold)
            {
                DeedLog.Record(this, DeedLog.BigSale);
                Reactions.Fire(this, "random:big_sale");
            }
            _bus.Publish(new StatsChanged());
            _bus.Publish(new DayCycleFinished(summary));
            return summary;
        }

        // ---- sleeping ------------------------------------------------------------------------------------------

        // Fade out, run the overnight logic, show the summary, then wake up in the farmhouse.
        public void StartSleep(bool passedOut)
        {
            if (!InGame || _sleeping) return;
            StartCoroutine(SleepRoutine(passedOut));
        }

        IEnumerator SleepRoutine(bool passedOut)
        {
            _sleeping = true;
            AudioService.PlayIfAvailable(Sfx.Sleep);
            ServiceLocator.TryGet<InputService>(out var input);
            input?.BlockGameplay();
            Clock.Pause();

            var loader = ServiceLocator.Get<SceneLoader>();
            yield return loader.FadeTo(1f, 0.6f);

            var summary = EndDay(passedOut);
            if (summary != null && ServiceLocator.TryGet<IUiService>(out var ui))
            {
                var done = false;
                ui.ShowDaySummary(summary, () => done = true);
                while (!done) yield return null;
            }

            Clock.Resume();
            input?.UnblockGameplay();
            _sleeping = false;
            AudioService.PlayIfAvailable(Sfx.Rooster);
            // The screen is already black; this fades back in once loaded. Day-cycle hooks may have moved the player.
            loader.Load(State.CurrentMap, 0.4f);
        }
    }
}
