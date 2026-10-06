using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Draws the farm's greenhouse, coop and barn from the saved game state and gives each its door: a warp to the building's map, with the
    // spawn point where one comes back out. Everything is rebuilt from the state, so a building can be moved and the farm redrawn.
    public sealed class FarmBuildingsView : MonoBehaviour
    {
        [SerializeField] FarmMap _map;
        [SerializeField] TileBase _wall;
        [SerializeField] TileBase _roof;
        [SerializeField] TileBase _door;

        readonly List<GameObject> _made = new List<GameObject>();
        readonly List<Vector3Int> _wallCells = new List<Vector3Int>();
        readonly List<Vector3Int> _doorCells = new List<Vector3Int>();
        readonly Dictionary<Vector3Int, TileBase> _groundBefore = new Dictionary<Vector3Int, TileBase>();

        public void Configure(FarmMap map, TileBase wall, TileBase roof, TileBase door)
        {
            _map = map; _wall = wall; _roof = roof; _door = door;
        }

        // Clears what was drawn and draws every building where the state says it stands.
        public void Rebuild(GameSession session)
        {
            if (_map == null || session == null) return;
            FarmBuildings.EnsureDefaults(session.State);
            Build(session.State.FarmBuildings);
        }

        // Draws these buildings (also used by the tests, which have no game).
        public void Build(IReadOnlyList<FarmBuildingState> buildings)
        {
            if (_map == null) return;
            Clear();
            foreach (var at in buildings)
            {
                var type = FarmBuildings.Type(at.TypeId);
                if (type == null) continue;
                var door = FarmBuildings.DoorCell(type, at);
                for (var y = at.Y; y < at.Y + type.H; y++)
                    for (var x = at.X; x < at.X + type.W; x++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        if (x == door.x && y == door.y)
                        {
                            _groundBefore[cell] = _map.Ground.GetTile(cell);
                            _map.Ground.SetTile(cell, _door);
                            _doorCells.Add(cell);
                            continue;
                        }
                        _map.Walls.SetTile(cell, FarmBuildings.IsRoof(type, at, y) ? _roof : _wall);
                        _wallCells.Add(cell);
                    }
                MakeDoor(type, door);
            }
        }

        void MakeDoor(FarmBuildingType type, (int x, int y) door)
        {
            var go = new GameObject("Door_" + type.Id);
            go.transform.SetParent(transform, false);
            go.transform.position = _map.CellCenter(new Vector3Int(door.x, door.y, 0));
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            var warp = go.AddComponent<Warp>();
            warp.TargetMap = type.InteriorMap;
            warp.TargetSpawn = "default";
            warp.Condition = "flag:" + type.UnlockFlag;
            warp.BlockedMessageKey = type.LockedKey;
            _made.Add(go);

            var spawn = new GameObject("Spawn_" + type.ReturnSpawn);
            spawn.transform.SetParent(transform, false);
            spawn.transform.position = _map.CellCenter(new Vector3Int(door.x, door.y - 2, 0));
            spawn.AddComponent<SpawnPoint>().Id = type.ReturnSpawn;
            _made.Add(spawn);
        }

        void Clear()
        {
            foreach (var cell in _wallCells) _map.Walls.SetTile(cell, null);
            foreach (var cell in _doorCells) _map.Ground.SetTile(cell, _groundBefore.TryGetValue(cell, out var before) && before != null ? before : null);
            _wallCells.Clear();
            _doorCells.Clear();
            _groundBefore.Clear();
            foreach (var go in _made)
                if (go != null) { go.SetActive(false); Destroy(go); }          // gone at once for anyone looking, freed at the end of the frame
            _made.Clear();
        }
    }
}
