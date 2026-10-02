using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // ------------------------------------------------------------------------------------------------------------
    // Extension points for optional layers (see docs/adr/0002-mythos-extension-points.md). Modules register hooks
    // here from IGameModule.Initialize; core gameplay code calls them but knows nothing about what they do.
    // A hook that throws is logged and skipped, so a faulty module can never break sleeping or loading a map.
    // ------------------------------------------------------------------------------------------------------------

    public sealed class SummaryNote
    {
        public readonly string Key;
        public readonly object[] Args;
        public SummaryNote(string key, object[] args) { Key = key; Args = args; }
    }

    // Mutable view of the overnight transition handed to day-cycle hooks.
    public sealed class DayCycleContext
    {
        public GameState State;
        public GameClock Clock;
        public IDictionary<string, FarmGrid> Grids;
        public Func<string, ItemDefinition> Items;
        public Func<string, CropDefinition> Crops;
        public DaySummary Summary;
        public bool PassedOut;
        public IWorldQuery World;

        // Where the player wakes up. Hooks may change these (sleepwalking, being found elsewhere...).
        public string WakeMap;
        public string WakeSpawn;

        // Adds a localized line to the day summary screen (key into the string table).
        public void Note(string key, params object[] args) => Summary.Notes.Add(new SummaryNote(key, args));
    }

    public interface IDayCycleHook
    {
        int Order { get; }                         // lower runs first
        // After the shipping bin has paid out, before crops grow. Dreams, blight, offerings, gold changes.
        void OnNightFalls(DayCycleContext context);
        // After the new date and weather are set and rest is applied; before the player is placed in bed.
        void OnDawn(DayCycleContext context);
    }

    public abstract class DayCycleHook : IDayCycleHook
    {
        public virtual int Order => 0;
        public virtual void OnNightFalls(DayCycleContext context) { }
        public virtual void OnDawn(DayCycleContext context) { }
    }

    public interface IWeatherModifier
    {
        int Order { get; }
        // Receives the weather rolled so far and returns the weather to use ("fog", "bloodmoon"...).
        // New weather ids need a "weather.<id>" string (L.AddTable).
        string Modify(GameDateTime date, string weather, GameState state);
    }

    // Reshapes the odds of tomorrow's weather before it is rolled (more storms as dread rises, a module's own
    // weather added with Add). The roll stays deterministic for a given result of the modifiers.
    public interface IWeatherWeightModifier
    {
        int Order { get; }
        void Adjust(GameDateTime date, WeatherWeights weights, GameState state);
    }

    // Adjusts the player's luck, a value from -1 (very unlucky) to +1 (very lucky), 0 being neutral. Systems that roll
    // for good or bad outcomes (forage quality, drops, random events, fishing) read GameSession.Luck.
    public interface ILuckModifier
    {
        int Order { get; }
        float Modify(float luck, GameState state);
    }

    // Adjusts how fast an NPC's attitude toward the player decays when the player ignores them: the base rate (points
    // per day) goes in, the rate to use comes out. Dread can speed it up.
    public interface IFriendshipDecayModifier
    {
        int Order { get; }
        float Modify(string npcId, float rate, GameState state);
    }

    // Reshapes the odds of a seasonal random event: its weight goes in, the weight to use comes out (0 removes it).
    // Dread shifts weight toward unfavourable events (see RandomEventDefinition.Mood).
    public interface IEventWeightModifier
    {
        int Order { get; }
        float Modify(RandomEventDefinition def, float weight, GameState state);
    }

    public sealed class MapLoadedContext
    {
        public string MapId;
        public GameSession Session;
        public FarmMap Map;
        public FarmMapView View;
        public PlayerController Player;
        public CameraFollow Camera;
    }

    // A small HUD element supplied by a module (a sanity meter, a moon icon...). The UI layer hosts it.
    public interface IHudWidget
    {
        void Build(Transform parent);
        void Refresh(GameSession session);
    }

    public sealed class GameHooks
    {
        readonly List<IDayCycleHook> _dayCycle = new List<IDayCycleHook>();
        readonly List<IWeatherModifier> _weather = new List<IWeatherModifier>();
        readonly List<IWeatherWeightModifier> _weatherWeights = new List<IWeatherWeightModifier>();
        readonly List<Func<IHudWidget>> _hudWidgets = new List<Func<IHudWidget>>();
        readonly List<ILuckModifier> _luck = new List<ILuckModifier>();
        readonly List<IFriendshipDecayModifier> _decay = new List<IFriendshipDecayModifier>();
        readonly List<IEventWeightModifier> _eventWeights = new List<IEventWeightModifier>();

        public IReadOnlyList<IDayCycleHook> DayCycleHooks => _dayCycle;
        public IReadOnlyList<ILuckModifier> LuckModifiers => _luck;
        public IReadOnlyList<Func<IHudWidget>> HudWidgets => _hudWidgets;

        // Raised by the map scene controller once a map is set up (player placed, farm drawn).
        public event Action<MapLoadedContext> MapLoaded;

        public void AddDayCycleHook(IDayCycleHook hook)
        {
            if (hook == null || _dayCycle.Contains(hook)) return;
            _dayCycle.Add(hook);
            _dayCycle.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void AddWeatherModifier(IWeatherModifier modifier)
        {
            if (modifier == null || _weather.Contains(modifier)) return;
            _weather.Add(modifier);
            _weather.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void AddWeatherWeightModifier(IWeatherWeightModifier modifier)
        {
            if (modifier == null || _weatherWeights.Contains(modifier)) return;
            _weatherWeights.Add(modifier);
            _weatherWeights.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void AddHudWidget(Func<IHudWidget> factory)
        {
            if (factory != null) _hudWidgets.Add(factory);
        }

        // ---- world objects (animals, produce, crafted items) ---------------------------------------------------

        readonly List<IWorldObjectSource> _worldSources = new List<IWorldObjectSource>();

        public IReadOnlyList<IWorldObjectSource> WorldObjectSources => _worldSources;

        public void AddWorldObjectSource(IWorldObjectSource source)
        {
            if (source != null && !_worldSources.Contains(source)) _worldSources.Add(source);
        }

        // Every object of the given kinds (all kinds when none are given). A failing source is logged and skipped.
        public List<WorldObjectRef> EnumerateWorldObjects(params string[] kinds)
        {
            var result = new List<WorldObjectRef>();
            foreach (var source in _worldSources)
            {
                if (kinds != null && kinds.Length > 0 && Array.IndexOf(kinds, source.Kind) < 0) continue;
                try { result.AddRange(source.Enumerate()); }
                catch (Exception e) { Log.Error($"World object source {source.GetType().Name} failed: {e}"); }
            }
            return result;
        }

        public bool WorldObjectExists(WorldObjectRef reference)
        {
            foreach (var source in _worldSources)
            {
                if (source.Kind != reference.Kind) continue;
                try { if (source.Exists(reference)) return true; }
                catch (Exception e) { Log.Error($"World object source {source.GetType().Name} failed: {e}"); }
            }
            return false;
        }

        public bool ConsumeWorldObject(WorldObjectRef reference)
        {
            foreach (var source in _worldSources)
            {
                if (source.Kind != reference.Kind) continue;
                try { if (source.TryConsume(reference)) return true; }
                catch (Exception e) { Log.Error($"World object source {source.GetType().Name} failed: {e}"); }
            }
            return false;
        }

        public void AddLuckModifier(ILuckModifier modifier)
        {
            if (modifier == null || _luck.Contains(modifier)) return;
            _luck.Add(modifier);
            _luck.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        // Neutral luck (0) passed through every modifier in order, clamped to [-1, 1].
        public float ComputeLuck(GameState state, float baseLuck = 0f)
        {
            var luck = Mathf.Clamp(baseLuck, -1f, 1f);
            foreach (var m in _luck)
            {
                try { luck = Mathf.Clamp(m.Modify(luck, state), -1f, 1f); }
                catch (Exception e) { Log.Error($"Luck modifier {m.GetType().Name} failed: {e}"); }
            }
            return luck;
        }

        public void AddFriendshipDecayModifier(IFriendshipDecayModifier modifier)
        {
            if (modifier == null || _decay.Contains(modifier)) return;
            _decay.Add(modifier);
            _decay.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        // The daily friendship loss for an ignored NPC after every modifier (never negative).
        public float ComputeFriendshipDecay(string npcId, float baseRate, GameState state)
        {
            var rate = baseRate;
            foreach (var m in _decay)
            {
                try { rate = Mathf.Max(0f, m.Modify(npcId, rate, state)); }
                catch (Exception e) { Log.Error($"Friendship decay modifier {m.GetType().Name} failed: {e}"); }
            }
            return rate;
        }

        public void AddEventWeightModifier(IEventWeightModifier modifier)
        {
            if (modifier == null || _eventWeights.Contains(modifier)) return;
            _eventWeights.Add(modifier);
            _eventWeights.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public float ComputeEventWeight(RandomEventDefinition def, float weight, GameState state)
        {
            foreach (var m in _eventWeights)
            {
                try { weight = Mathf.Max(0f, m.Modify(def, weight, state)); }
                catch (Exception e) { Log.Error($"Event weight modifier {m.GetType().Name} failed: {e}"); }
            }
            return weight;
        }

        public void Clear()
        {
            _decay.Clear();
            _eventWeights.Clear();
            _dayCycle.Clear();
            _weather.Clear();
            _weatherWeights.Clear();
            _luck.Clear();
            _worldSources.Clear();
            _hudWidgets.Clear();
            MapLoaded = null;
        }

        public void AdjustWeatherWeights(GameDateTime date, WeatherWeights weights, GameState state)
        {
            foreach (var m in _weatherWeights)
            {
                try { m.Adjust(date, weights, state); }
                catch (Exception e) { Log.Error($"Weather weight modifier {m.GetType().Name} failed: {e}"); }
            }
        }

        public string ApplyWeather(GameDateTime date, string weather, GameState state)
        {
            foreach (var m in _weather)
            {
                try
                {
                    var result = m.Modify(date, weather, state);
                    if (!string.IsNullOrEmpty(result)) weather = result;
                }
                catch (Exception e) { Log.Error($"Weather modifier {m.GetType().Name} failed: {e}"); }
            }
            return weather;
        }

        public void RunNightFalls(DayCycleContext context)
        {
            foreach (var h in _dayCycle)
            {
                try { h.OnNightFalls(context); }
                catch (Exception e) { Log.Error($"Day hook {h.GetType().Name}.OnNightFalls failed: {e}"); }
            }
        }

        public void RunDawn(DayCycleContext context)
        {
            foreach (var h in _dayCycle)
            {
                try { h.OnDawn(context); }
                catch (Exception e) { Log.Error($"Day hook {h.GetType().Name}.OnDawn failed: {e}"); }
            }
        }

        public void RaiseMapLoaded(MapLoadedContext context)
        {
            var handlers = MapLoaded;
            if (handlers == null) return;
            foreach (var d in handlers.GetInvocationList())
            {
                try { ((Action<MapLoadedContext>)d)(context); }
                catch (Exception e) { Log.Error($"MapLoaded handler failed: {e}"); }
            }
        }
    }
}
