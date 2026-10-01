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
        readonly List<Func<IHudWidget>> _hudWidgets = new List<Func<IHudWidget>>();

        public IReadOnlyList<IDayCycleHook> DayCycleHooks => _dayCycle;
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

        public void AddHudWidget(Func<IHudWidget> factory)
        {
            if (factory != null) _hudWidgets.Add(factory);
        }

        public void Clear()
        {
            _dayCycle.Clear();
            _weather.Clear();
            _hudWidgets.Clear();
            MapLoaded = null;
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
