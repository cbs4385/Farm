using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Created by the map scene controller on every map: keeps a PlacedObjectActor for each object placed on this map.
    public sealed class PlacedObjectsView : MonoBehaviour
    {
        FarmMap _map;
        GameSession _session;
        readonly Dictionary<string, PlacedObjectActor> _actors = new Dictionary<string, PlacedObjectActor>();
        int _lastMinute = -1;

        public static PlacedObjectsView Current { get; private set; }

        public IReadOnlyDictionary<string, PlacedObjectActor> Actors => _actors;

        public void Init(FarmMap map, GameSession session)
        {
            _map = map;
            _session = session;
            Current = this;
            foreach (var obj in session.GetObjects(map.MapId).All) Spawn(obj);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public PlacedObjectActor Spawn(PlacedObject obj)
        {
            var def = _session.Placeables.Get(obj.TypeId);
            if (def == null)
            {
                Farm.Core.Log.Warn($"Placed object '{obj.Id}' has unknown type '{obj.TypeId}'.");
                return null;
            }
            var go = new GameObject("placed", typeof(SpriteRenderer));
            var actor = go.AddComponent<PlacedObjectActor>();
            actor.Setup(obj, def, _map.CellCenter(new Vector3Int(obj.X, obj.Y, 0)));
            actor.RefreshLook(ObjectGrid.Minute(_session.Clock.Now));
            _actors[obj.Id] = actor;
            return actor;
        }

        public void Despawn(string objectId)
        {
            if (_actors.TryGetValue(objectId, out var actor))
            {
                Destroy(actor.gameObject);
                _actors.Remove(objectId);
            }
        }

        public PlacedObjectActor At(Vector3Int cell)
        {
            foreach (var actor in _actors.Values)
                if (actor.Object.X == cell.x && actor.Object.Y == cell.y) return actor;
            return null;
        }

        // Machines change colour when their work is done.
        void Update()
        {
            if (_session == null || !_session.InGame) return;
            var minute = ObjectGrid.Minute(_session.Clock.Now);
            if (minute == _lastMinute) return;
            _lastMinute = minute;
            foreach (var actor in _actors.Values) actor.RefreshLook(minute);
        }
    }
}
