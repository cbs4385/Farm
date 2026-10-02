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
    public sealed class GameSession : MonoBehaviour
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

        public UpgradeCatalog UpgradeTable => _upgradeCatalog ?? (_upgradeCatalog = UpgradeCatalog.From(_db));

        // Buys an upgrade at a counter (gold, materials, effect). Explains a refusal with a toast.
        public bool BuyUpgrade(UpgradeDefinition def)
        {
            var result = Upgrades.Buy(UpgradeTable, def, State, Backpack, Clock.Now);
            switch (result)
            {
                case UpgradeCheck.NoGold: Toast(L.Get("toast.not_enough_gold")); break;
                case UpgradeCheck.NoMaterials: Toast(L.Get("toast.upgrade_materials")); break;
                case UpgradeCheck.NotOffered: break;
                default:
                    if (def.Kind == UpgradeKind.Unlock && !string.IsNullOrEmpty(def.FlagId)) SetFlag(def.FlagId);
                    _bus.Publish(new StatsChanged());
                    Toast(L.Get(def.Kind == UpgradeKind.Tool ? "toast.upgrade_started" : "toast.upgrade_done"));
                    break;
            }
            return result == UpgradeCheck.Ok;
        }

        // Takes back every finished tool the backpack has room for.
        public int CollectUpgrades()
        {
            var collected = Upgrades.Collect(State, Backpack, Clock.Now);
            foreach (var p in collected) Toast(L.Get("toast.upgrade_collected", ToolTitle(p.ToolItemId, p.Tier)));
            if (collected.Count == 0 && Upgrades.Ready(State, Clock.Now).Count > 0) Toast(L.Get("toast.inventory_full"));
            if (collected.Count > 0) _bus.Publish(new StatsChanged());
            return collected.Count;
        }

        // "Copper Axe" for a tool item at a tier ("Axe" for basic).
        public string ToolTitle(string toolItemId, int tier)
        {
            var name = _db.TryGetItem(toolItemId, out var item) ? L.Get(item.NameKey) : toolItemId;
            return tier <= 0 ? name : L.Get("upgrade.tool", L.Get(ToolModel.TierKey(tier)), name);
        }

        // Events waiting to play (the `event:` effect); the event director on the loaded map plays them in order. Not saved.
        public List<string> PendingEvents { get; } = new List<string>();

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
        public IWorldQuery World => new StateWorldQuery(State, Clock) { ItemCounter = id => Backpack != null ? Backpack.Count(id) : 0 };

        // The player's luck, -1..+1 (0 neutral), after every module's modifiers. Roll-based systems should use it.
        public float Luck => InGame ? Hooks.ComputeLuck(State) : 0f;

        // Fatigue: what is carried from a collapse, and the rating right now (0 = rested, 1 = a whole night awake).
        public FatigueState Fatigue => new FatigueState(InGame ? State.FatigueCarried : 0f);
        public float FatigueRating => InGame ? Fatigue.LuckRating(Clock.Now.MinuteOfDay) : 0f;

        public int HorrorLevel => ServiceLocator.TryGet<SettingsStore>(out var s) ? s.Current.HorrorLevel : 2;

        // ---- story flags and variables -------------------------------------------------------------------------

        public bool HasFlag(string flag) => InGame && State.Flags.Contains(flag);

        public void SetFlag(string flag, bool on = true)
        {
            var changed = on ? State.Flags.Add(flag) : State.Flags.Remove(flag);
            if (changed) _bus.Publish(new FlagChanged(flag, on));
        }

        public int GetVar(string name) => InGame && State.Vars.TryGetValue(name, out var v) ? v : 0;

        public void SetVar(string name, int value)
        {
            var old = GetVar(name);
            if (old == value) return;
            State.Vars[name] = value;
            _bus.Publish(new VarChanged(name, old, value));
        }

        // Adds `delta` and clamps to [min, max]. Returns the new value.
        public int AddVar(string name, int delta, int min = int.MinValue, int max = int.MaxValue)
        {
            var value = Mathf.Clamp(GetVar(name) + delta, min, max);
            SetVar(name, value);
            return value;
        }

        // ---- module-owned persistent data ----------------------------------------------------------------------

        // Each module keeps its own serializable object in the save under its id. Returns a fresh T when none exists.
        public T GetModuleData<T>(string moduleId) where T : class, new()
        {
            if (InGame && State.ModuleData.TryGetValue(moduleId, out var json))
            {
                try { return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json) ?? new T(); }
                catch (System.Exception e) { Log.Error($"Module data '{moduleId}' unreadable: {e.Message}"); }
            }
            return new T();
        }

        public void SetModuleData<T>(string moduleId, T data) where T : class
        {
            if (!InGame) return;
            State.ModuleData[moduleId] = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public void Init(EventBus bus, GameDatabase db, SaveService saves)
        {
            _bus = bus;
            _db = db;
            _saves = saves;
            Hooks.AddLuckModifier(new FatigueLuckModifier(() => Clock != null ? Clock.Now.MinuteOfDay : GameDateTime.DayStartMinute));
            Hooks.AddWorldObjectSource(new StoredItemsSource(this, WorldObjectKinds.CraftedItem));
            Hooks.AddWorldObjectSource(new StoredItemsSource(this, WorldObjectKinds.PlantProduct));
            Hooks.AddWorldObjectSource(new StandingCropsSource(this));
            _bus.Subscribe<MinuteChanged>(OnMinuteChanged);
            _bus.Subscribe<VarChanged>(OnStoryStateChanged);
            _bus.Subscribe<FlagChanged>(OnStoryStateChanged);
            _bus.Subscribe<StatsChanged>(OnStoryStateChanged);
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
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowMessage("late_night.warning");
        }

        void Update()
        {
            if (InGame) Clock.Tick(Time.deltaTime);
        }

        // ---- lifecycle -----------------------------------------------------------------------------------------

        public void BeginNewGame(string playerName, string farmName, int slot)
        {
            Begin(GameState.NewGame(playerName, farmName, _db.MaxStack), slot);
            StoryDay.NewGame(this);
            Save();
        }

        // For starting a map scene directly in the Editor: a throwaway game that is never saved (slot -1).
        public void BeginDevGame() => Begin(GameState.NewGame("Dev", "Dev Farm", _db.MaxStack), -1);

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
            Backpack = Inventory.FromData(state.Backpack, _db.MaxStack);
            Backpack.Changed += OnBackpackChanged;
            _grids.Clear();
            _nodeGrids.Clear();
            _objectGrids.Clear();
            foreach (var kv in state.Maps)
            {
                _grids[kv.Key] = FarmGrid.FromTiles(kv.Value.Tiles);
                _grids[kv.Key].AllSeasons = kv.Key == MapIds.Greenhouse;
                _nodeGrids[kv.Key] = NodeGrid.FromNodes(kv.Value.Nodes);
                _objectGrids[kv.Key] = ObjectGrid.FromList(kv.Value.Objects, _db.MaxStack);
            }
            _bus.Publish(new StatsChanged());
        }

        void OnBackpackChanged() => _bus.Publish(new StatsChanged());

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

        public FarmGrid GetGrid(string mapId)
        {
            if (!_grids.TryGetValue(mapId, out var grid))
            {
                grid = new FarmGrid { AllSeasons = mapId == MapIds.Greenhouse };
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

        // ---- player stats --------------------------------------------------------------------------------------

        public bool TrySpendEnergy(int amount)
        {
            if (State.Energy < amount) return false;
            State.Energy -= amount;
            _bus.Publish(new StatsChanged());
            return true;
        }

        public void SetEnergy(int value)
        {
            State.Energy = Mathf.Clamp(value, 0, State.MaxEnergy);
            _bus.Publish(new StatsChanged());
        }

        // Tells the HUD to redraw (used by tools that change the clock or other state directly).
        public void NotifyChanged() => _bus.Publish(new StatsChanged());

        public void RestoreEnergy(int amount)
        {
            State.Energy = Mathf.Min(State.MaxEnergy, State.Energy + amount);
            _bus.Publish(new StatsChanged());
        }

        public void AddGold(int amount)
        {
            State.Gold += amount;
            _bus.Publish(new StatsChanged());
        }

        // Text from the string table with the player's and farm's names filled in ("[player]", "[farm]").
        public string StoryText(string key, object[] args)
        {
            var text = L.Get(key, args);
            return InGame ? text.Replace("[player]", State.PlayerName).Replace("[farm]", State.FarmName) : text;
        }

        // Starts a conversation (modal; the clock is paused while it is open). Returns false when there is no such
        // dialogue or no UI to show it.
        public bool BeginDialogue(string dialogueId, Action onFinished = null)
        {
            var graph = Story.Dialogue(dialogueId);
            if (graph == null) { Log.Warn($"Unknown dialogue '{dialogueId}'."); return false; }
            if (!ServiceLocator.TryGet<IUiService>(out var ui)) return false;
            var runner = new DialogueRunner(graph, World, StoryText, effect => Effects.Run(this, effect));
            ui.ShowDialogue(runner, onFinished);
            return true;
        }

        // Teaches a recipe (a letter, a villager). Returns false when it is already known or does not exist.
        public bool LearnRecipe(string recipeId)
        {
            var recipe = Recipes.Get(recipeId);
            if (recipe == null || State.Recipes.Contains(recipeId)) return false;
            State.Recipes.Add(recipeId);
            if (_db.TryGetItem(recipe.OutputItemId, out var made)) Toast(L.Get("toast.recipe_learned", L.Get(made.NameKey)));
            return true;
        }

        // Crafts or cooks a recipe at a station; counts it for quests.
        public CraftResult Craft(RecipeDefinition recipe, string station)
        {
            var result = CraftingRules.TryCraft(recipe, station, State, Backpack, GetSkillLevel);
            if (result == CraftResult.Ok) AddVar(QuestLog.Stats.Crafted, 1);
            return result;
        }

        public bool KnowsRecipe(RecipeDefinition recipe) => CraftingRules.Knows(recipe, State, GetSkillLevel);

        // Adds or removes gold (a story effect); never goes below zero.
        public void ChangeGold(int delta)
        {
            State.Gold = System.Math.Max(0, State.Gold + delta);
            _bus.Publish(new StatsChanged());
        }

        // Puts items in the backpack (a reward, a gift). What does not fit is lost, with a toast saying so.
        public void GiveItem(string itemId, int count = 1, int quality = 0)
        {
            if (!InGame || count <= 0) return;
            if (!_db.TryGetItem(itemId, out _)) { Log.Error($"give: unknown item '{itemId}'"); return; }
            if (Backpack.Add(itemId, count, quality) > 0) Toast(L.Get("toast.inventory_full"));
        }

        public bool TrySpendGold(int amount)
        {
            if (State.Gold < amount) return false;
            State.Gold -= amount;
            _bus.Publish(new StatsChanged());
            return true;
        }

        public int GetSkillXp(string skill) => InGame && State.SkillXp.TryGetValue(skill, out var xp) ? xp : 0;
        public int GetSkillLevel(string skill) => SkillModel.LevelForXp(GetSkillXp(skill));

        // Adds XP; announces each level gained (event and a toast).
        public void AddSkillXp(string skill, int xp)
        {
            if (!InGame || xp <= 0 || string.IsNullOrEmpty(skill)) return;
            var before = SkillModel.LevelForXp(GetSkillXp(skill));
            State.SkillXp[skill] = GetSkillXp(skill) + xp;
            var after = SkillModel.LevelForXp(State.SkillXp[skill]);
            for (var level = before + 1; level <= after; level++)
            {
                _bus.Publish(new SkillLevelUp(skill, level));
                Toast(L.Get("toast.skill_level", L.Get("skill." + skill), level));
            }
            // Levels unlock recipes: say what can be made now.
            foreach (var recipe in CraftingRules.NewlyUnlocked(Recipes, skill, before, after))
                if (_db.TryGetItem(recipe.OutputItemId, out var made)) Toast(L.Get("toast.recipe_unlocked", L.Get(made.NameKey)));
        }

        // The upgrade tier of a tool item the player owns (0 = basic).
        public int ToolTier(string itemId) => InGame && itemId != null && State.ToolTiers.TryGetValue(itemId, out var t) ? t : 0;

        // Publishes an event on the game's bus (for systems that only hold the session).
        public void Publish<T>(T evt) => _bus.Publish(evt);

        public void Toast(string message) => _bus.Publish(new ToastRequested(message));

        // ---- shipping ------------------------------------------------------------------------------------------

        // Moves the whole stack in a backpack slot into the shipping bin. Returns false if it cannot be sold.
        public bool ShipSlot(int slot)
        {
            var stack = Backpack.Get(slot);
            if (stack == null || !_db.TryGetItem(stack.ItemId, out var item) || item.IsTool || item.SellPrice <= 0) return false;

            var removed = Backpack.RemoveFromSlot(slot, stack.Count);
            AddVar(QuestLog.Stats.Shipped, removed.Count);
            foreach (var existing in State.ShippingBin)
            {
                if (existing.ItemId == removed.ItemId && existing.Quality == removed.Quality && existing.Mark == removed.Mark)
                {
                    existing.Count += removed.Count;
                    _bus.Publish(new StatsChanged());
                    return true;
                }
            }
            State.ShippingBin.Add(removed);
            _bus.Publish(new StatsChanged());
            return true;
        }

        // ---- day cycle -----------------------------------------------------------------------------------------

        // Runs the overnight logic, autosaves, and tells the UI to show the summary. Re-entrancy safe.
        public DaySummary EndDay(bool passedOut)
        {
            if (!InGame || _endingDay) return null;
            _endingDay = true;

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
            // The screen is already black; this fades back in once loaded. Day-cycle hooks may have moved the player.
            loader.Load(State.CurrentMap, 0.4f);
        }
    }
}
