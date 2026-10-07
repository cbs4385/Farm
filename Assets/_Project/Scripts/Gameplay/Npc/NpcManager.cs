using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Created by the map scene controller on every map: keeps an NpcActor for each villager whose schedule currently
    // puts them on this map, and removes it when they leave. Positions come from NpcSchedule.Where each frame.
    public sealed class NpcManager : MonoBehaviour
    {
        FarmMap _map;
        GameSession _session;
        WalkGrid _grid;
        readonly Dictionary<string, NpcActor> _actors = new Dictionary<string, NpcActor>();
        readonly HashSet<string> _suspended = new HashSet<string>();
        readonly Dictionary<string, (int day, NpcScheduleEntry plan)> _plans = new Dictionary<string, (int, NpcScheduleEntry)>();

        public IReadOnlyDictionary<string, NpcActor> Actors => _actors;

        // The manager of the map that is loaded (there is one at a time). Used by the player to find who is on a cell.
        public static NpcManager Current { get; private set; }

        public NpcActor ActorAt(Vector3Int cell)
        {
            foreach (var actor in _actors.Values)
                if (actor.Cell == cell) return actor;
            return null;
        }

        // The villager standing on the cell, or one walking across it or just leaving it (a click lands on the picture, not on the rounded cell).
        public NpcActor ActorNear(Vector3Int cell, float reach = NearReach)
        {
            var exact = ActorAt(cell);
            if (exact != null) return exact;
            var centre = _map != null ? _map.CellCenter(cell) : Vector3.zero;
            NpcActor best = null;
            var bestDistance = reach * reach;
            foreach (var actor in _actors.Values)
            {
                var d = (actor.transform.position - centre).sqrMagnitude;
                if (d < bestDistance) { best = actor; bestDistance = d; }
            }
            return best;
        }

        public const float NearReach = 0.75f;            // tiles
        public const float HoldSeconds = 12f;            // how long a villager stands still after being spoken to (the clock of the hold only runs with no dialogue open)

        // A villager the player has clicked stops where they are (and is late for whatever came next by exactly that long, then hurries to catch
        // up), so that the player can talk to them without chasing them.
        public void Hold(string npcId, float seconds = HoldSeconds)
        {
            if (!string.IsNullOrEmpty(npcId)) _holdLeft[npcId] = seconds;
        }

        public bool IsHeld(string npcId) => _holdLeft.TryGetValue(npcId, out var left) && left > 0f;
        public float LagMinutes(string npcId) => _lag.TryGetValue(npcId, out var lag) ? lag : 0f;

        // Pure: the new lag after `dtMinutes` of clock time, held or not. While held the villager falls behind the schedule at the clock rate;
        // afterwards they run at twice the rate until they have caught up.
        public static float NextLag(float lag, float dtMinutes, bool held) =>
            held ? lag + dtMinutes : Mathf.Max(0f, lag - dtMinutes * CatchUp);

        public const float CatchUp = 1f;                 // extra schedule minutes per clock minute while catching up (2x speed)
        const float MaxLag = 90f;
        readonly Dictionary<string, float> _holdLeft = new Dictionary<string, float>();
        readonly Dictionary<string, float> _lag = new Dictionary<string, float>();
        float _lastMinute = -1f;

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        // Cutscenes take over a villager: their schedule is ignored until ReleaseAll. Returns the actor (appearing on `cell` if
        // they are not on this map and a cell is given), or null.
        public NpcActor Take(string npcId, Vector3Int? cell)
        {
            _suspended.Add(npcId);
            if (_actors.TryGetValue(npcId, out var actor)) return actor;
            var npc = _session.Npcs.Get(npcId);
            if (npc == null || cell == null) return null;
            var go = new GameObject("Npc_" + npcId, typeof(SpriteRenderer));
            actor = go.AddComponent<NpcActor>();
            actor.Setup(npc);
            actor.SetCell(_map, cell.Value);
            _actors[npcId] = actor;
            return actor;
        }

        public void ReleaseAll() => _suspended.Clear();

        public void Init(FarmMap map, GameSession session)
        {
            _map = map;
            _session = session;
            Current = this;
            AnnounceBirthdays();
        }

        // Once per day, on the first map loaded, a toast says whose birthday it is.
        void AnnounceBirthdays()
        {
            var today = _session.Clock.Now.TotalDays;
            if (_session.GetVar("npc.birthday_shown") == today + 1) return;
            _session.SetVar("npc.birthday_shown", today + 1);
            foreach (var npc in _session.Npcs.All)
                if (npc.IsBirthday(_session.Clock.Now)) _session.Toast(L.Get("npc.birthday_today", L.Get(npc.NameKey)));
        }

        NpcScheduleEntry PlanFor(NpcDefinition npc, int day)
        {
            if (_plans.TryGetValue(npc.Id, out var cached) && cached.day == day) return cached.plan;
            var plan = NpcSchedule.PlanFor(npc, _session.World, _session.Hooks.ScheduleEntriesFor(npc));
            _plans[npc.Id] = (day, plan);
            return plan;
        }

        void Update()
        {
            if (_session == null || !_session.InGame || _map == null) return;
            var minute = _session.Clock.PreciseMinuteOfDay;
            var day = _session.Clock.Now.TotalDays;
            var dMinute = _lastMinute < 0f ? 0f : minute - _lastMinute;
            if (dMinute < 0f || dMinute > 30f) { _lag.Clear(); dMinute = 0f; }          // a new day or a jump of the clock: everybody is on time
            _lastMinute = minute;
            var modal = ServiceLocator.TryGet<IUiService>(out var uiService) && uiService.AnyModalOpen;

            foreach (var npc in _session.Npcs.All)
            {
                if (_suspended.Contains(npc.Id)) continue;
                var held = _holdLeft.TryGetValue(npc.Id, out var left) && left > 0f;
                if (held && !modal) _holdLeft[npc.Id] = left - Time.unscaledDeltaTime;
                _lag.TryGetValue(npc.Id, out var lag);
                var waiting = _actors.TryGetValue(npc.Id, out var current) && current != null && current.Waiting;      // held up by something in the way: the schedule waits too
                lag = Mathf.Min(MaxLag, NextLag(lag, dMinute, held || waiting));
                _lag[npc.Id] = lag;
                var place = NpcSchedule.Where(npc, PlanFor(npc, day), Mathf.Max(0f, minute - lag));
                if (place.Map != _map.MapId)
                {
                    if (_actors.TryGetValue(npc.Id, out var gone)) { Destroy(gone.gameObject); _actors.Remove(npc.Id); }
                    continue;
                }
                if (!_actors.TryGetValue(npc.Id, out var actor))
                {
                    var go = new GameObject("Npc_" + npc.Id, typeof(SpriteRenderer));
                    actor = go.AddComponent<NpcActor>();
                    actor.Setup(npc);
                    _actors[npc.Id] = actor;
                }
                actor.Apply(place, _map, Grid());
            }
        }

        // The walkable cells of this map: ground with no wall tile and nothing solid on it. Built on first use (a frame
        // after the scene starts, so colliders exist).
        public WalkGrid Grid()
        {
            if (_grid != null) return _grid;
            Physics2D.SyncTransforms();
            _map.Ground.CompressBounds();
            var bounds = _map.Ground.cellBounds;
            _grid = new WalkGrid(bounds.xMin, bounds.yMin, bounds.size.x, bounds.size.y, (x, y) =>
            {
                var cell = new Vector3Int(x, y, 0);
                if (_map.Ground.GetTile(cell) == null) return false;
                if (_map.Walls != null && _map.Walls.GetTile(cell) != null) return false;
                foreach (var hit in Physics2D.OverlapPointAll(_map.CellCenter(cell)))
                    if (!hit.isTrigger && !hit.CompareTag("Player") && hit.GetComponentInParent<ICellOccupant>() == null) return false;      // people and animals move; they are not walls
                return true;
            });
            return _grid;
        }
    }
}
