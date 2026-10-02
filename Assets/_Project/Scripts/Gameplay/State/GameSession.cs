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
        public WeatherCatalog Weather => _weatherCatalog ?? (_weatherCatalog = WeatherCatalog.From(_db));
        public IDictionary<string, FarmGrid> Grids => _grids;

        // Extension points for optional layers; see GameHooks.
        public GameHooks Hooks { get; } = new GameHooks();

        // What conditions (Conditions.Evaluate) see. Live: always reflects the current game.
        public IWorldQuery World => new StateWorldQuery(State, Clock);

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
            _bus.Subscribe<MinuteChanged>(OnMinuteChanged);
        }

        void OnDestroy() => _bus?.Unsubscribe<MinuteChanged>(OnMinuteChanged);

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
            foreach (var kv in state.Maps) _grids[kv.Key] = FarmGrid.FromTiles(kv.Value.Tiles);
            _bus.Publish(new StatsChanged());
        }

        void OnBackpackChanged() => _bus.Publish(new StatsChanged());

        public FarmGrid GetGrid(string mapId)
        {
            if (!_grids.TryGetValue(mapId, out var grid))
            {
                grid = new FarmGrid();
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

        public bool TrySpendGold(int amount)
        {
            if (State.Gold < amount) return false;
            State.Gold -= amount;
            _bus.Publish(new StatsChanged());
            return true;
        }

        public void AddSkillXp(string skill, int xp)
        {
            State.SkillXp.TryGetValue(skill, out var current);
            State.SkillXp[skill] = current + xp;
        }

        public void Toast(string message) => _bus.Publish(new ToastRequested(message));

        // ---- shipping ------------------------------------------------------------------------------------------

        // Moves the whole stack in a backpack slot into the shipping bin. Returns false if it cannot be sold.
        public bool ShipSlot(int slot)
        {
            var stack = Backpack.Get(slot);
            if (stack == null || !_db.TryGetItem(stack.ItemId, out var item) || item.IsTool || item.SellPrice <= 0) return false;

            var removed = Backpack.RemoveFromSlot(slot, stack.Count);
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
                Weather);
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
