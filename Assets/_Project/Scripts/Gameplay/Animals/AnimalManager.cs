using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // On the coop and barn maps: one AnimalActor per animal in that building.
    public sealed class AnimalManager : MonoBehaviour
    {
        GameSession _session;
        readonly List<AnimalActor> _actors = new List<AnimalActor>();

        public static AnimalManager Current { get; private set; }
        public FarmMap Map { get; private set; }
        // The animals sleep at night (the picture changes and they stay put).
        public bool Night => _session != null && AnimalSprites.IsNight(_session.Clock.Now.MinuteOfDay / 60);
        public bool Running => _session != null && _session.InGame && !_session.Clock.IsPaused;

        public void Init(FarmMap map, GameSession session)
        {
            Map = map;
            _session = session;
            Current = this;
            var n = 0;
            foreach (var a in AnimalRules.In(session.State, map.MapId)) Spawn(a, 3 + (n++ % 4) * 2, 3 + n % 3);
        }

        float _nextCall = 20f;

        // Every half a minute or so one of the animals makes its call, so a barn or coop sounds lived in.
        void Update()
        {
            if (!Running || _actors.Count == 0 || Time.time < _nextCall) return;
            _nextCall = Time.time + Random.Range(25f, 60f);
            var actor = _actors[Random.Range(0, _actors.Count)];
            if (actor != null) actor.Call();
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public AnimalActor Spawn(AnimalState state, int x, int y)
        {
            var go = new GameObject("animal", typeof(SpriteRenderer));
            var start = new Vector3Int(x, y, 0);
            if ((!Map.CanPlaceAt(start) || CellOccupants.IsTaken(start)) && CellOccupants.TryFindFree(start, c => Map.CanPlaceAt(c), null, out var free, 6)) start = free;
            go.transform.position = Map.CellCenter(start);
            var actor = go.AddComponent<AnimalActor>();
            actor.Setup(state, this);
            _actors.Add(actor);
            return actor;
        }

        // The trough was filled: every animal in the building turns to the front and munches.
        public void Munch(float seconds = 2.5f)
        {
            foreach (var a in _actors) if (a != null) a.Eat(seconds);
        }

        public AnimalActor ActorAt(Vector3Int cell)
        {
            foreach (var a in _actors) if (a != null && a.Cell == cell) return a;
            return null;
        }

        // A cell an animal may step into: floor, no wall, nothing solid, no other animal.
        public bool Free(Vector3Int cell, AnimalActor self)
        {
            if (!Map.CanPlaceAt(cell)) return false;
            return !CellOccupants.IsTaken(cell, self);                    // no other animal, villager or cat
        }
    }
}
