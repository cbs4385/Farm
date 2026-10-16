using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // One per Mine scene: builds the current floor (walls, ore, ladder, monsters) from MineGenerator when the map loads. The
    // scene itself is just a floor with a wall border; the floor number lives in GameState.Mine.
    public sealed class MineController : MonoBehaviour
    {
        MineFloor _floor;
        MineFloorMemory _memory;
        GameSession _session;
        FarmMap _map;

        public MineFloor Floor => _floor;
        public bool IsWall(int x, int y) => _floor == null || _floor.IsWall(x, y);


        public void Build(GameSession session, FarmMap map)
        {
            _session = session;
            _map = map;
            var daylight = FindAnyObjectByType<DayNightLighting>();
            if (daylight != null) daylight.SetCave();                    // dark and blue down here: the torch is what lights it
            var ms = session.State.Mine;
            if (ms.Floor <= 0) ms.Floor = 1;
            ms.Deepest = Mathf.Max(ms.Deepest, ms.Floor);
            var day = session.Clock.Now.TotalDays;
            // Cozy (horror off) has no slimes: the bats are the only monsters there (playtest 2026-10-13).
            var slimes = (session.Settings?.HorrorLevel ?? 2) > 0;
            _floor = MineGenerator.Generate(MineGenerator.FloorSeed(session.State.WorldSeed, day, ms.Floor), ms.Floor, slimes);

            // Tiles: copy the wall and floor tiles the scene was built with.
            var wallTile = map.Walls.GetTile(new Vector3Int(0, 0, 0));
            var floorTile = map.Ground.GetTile(new Vector3Int(1, 1, 0));
            for (var y = 0; y < _floor.Height; y++)
                for (var x = 0; x < _floor.Width; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    map.Ground.SetTile(cell, floorTile);
                    map.Walls.SetTile(cell, _floor.IsWall(x, y) ? wallTile : null);
                }

            // Ore and rocks: as the player left them when they have been on this floor during this trip, fresh otherwise.
            var nodes = session.GetNodes(MapIds.Mine);
            nodes.Clear();
            _memory = ms.MemoryOf(ms.Floor);
            if (_memory == null)
            {
                _memory = new MineFloorMemory { Floor = ms.Floor };
                ms.Memory.Add(_memory);
                foreach (var spawn in _floor.Nodes)
                    if (session.Nodes.Get(spawn.NodeId) is ResourceNodeDefinition def) nodes.Add(spawn.X, spawn.Y, def);
            }
            else
            {
                foreach (var left in _memory.Nodes)
                    if (session.Nodes.Get(left.TypeId) is ResourceNodeDefinition def && nodes.Add(left.X, left.Y, def) && nodes.TryGet(left.X, left.Y, out var restored))
                    {
                        restored.Hp = left.Hp;
                        restored.Age = left.Age;
                    }
            }

            // The arrival cell, and the stairs.
            foreach (var sp in FindObjectsByType<SpawnPoint>())
                if (sp.Id == "default") sp.transform.position = map.CellCenter(new Vector3Int(_floor.Spawn.x, _floor.Spawn.y, 0));

            var below = new GameObject("Spawn_" + MineTravel.FromBelowSpawn).AddComponent<SpawnPoint>();
            below.Id = MineTravel.FromBelowSpawn;
            var arrival = BesideLadder();
            below.transform.position = map.CellCenter(new Vector3Int(arrival.x, arrival.y, 0));

            AddStairs(MineStairs.StairKind.Up, _floor.Up.x, _floor.Up.y);
            if (_floor.Ladder.HasValue) AddStairs(MineStairs.StairKind.Down, _floor.Ladder.Value.x, _floor.Ladder.Value.y);
            if (ms.Deepest >= 5) AddStairs(MineStairs.StairKind.Elevator, _floor.Elevator.x, _floor.Elevator.y);

            var manager = new GameObject("Enemies").AddComponent<EnemyManager>();
            manager.Init(map, session, this, _floor.Enemies, _memory.Dead);
            if (session.Game?.Map != null) session.Game.Map.Enemies = manager;
        }

        // A monster of this floor was killed: it stays dead for the rest of the trip.
        public void RecordDead(int spawnIndex)
        {
            if (_memory != null && !_memory.Dead.Contains(spawnIndex)) _memory.Dead.Add(spawnIndex);
        }

        // The ore and rocks that are left are kept when the floor is left (by stairs, elevator, the forest or the end of the game), as long as the trip goes on.
        void OnDestroy() => KeepOre();

        // Also whenever the game is saved: a save made while standing in the mine must hold the ore as it is now, or a reloaded game finds the floor as it was on arrival
        // (playtest 2026-10-15: "the mine does not survive game reloads"). The trip's floors, the monsters killed and the floor itself are kept already.
        public void KeepOre()
        {
            if (_session == null || _memory == null || !_session.InGame || !StillRemembered(_session.State.Mine, _memory)) return;
            _memory.Nodes = _session.GetNodes(MapIds.Mine).ToList();
        }

        // The floor number in the state is already the next floor when this scene is destroyed (stairs and lift set it before loading), so the floor cannot be compared:
        // the floor is kept as long as the trip still holds its memory (leaving the mine, a knock-out or the end of the day clears the trip first).
        public static bool StillRemembered(MineState ms, MineFloorMemory memory) => ms != null && memory != null && ms.Memory.Contains(memory);

        // The open cell next to the ladder down (the arrival cell when climbing up from the floor below); the usual arrival cell on the last floor, which has no ladder.
        (int x, int y) BesideLadder()
        {
            if (!_floor.Ladder.HasValue) return _floor.Spawn;
            var l = _floor.Ladder.Value;
            foreach (var (dx, dy) in new[] { (-1, 0), (0, -1), (1, 0), (0, 1) })
                if (!_floor.IsWall(l.x + dx, l.y + dy)) return (l.x + dx, l.y + dy);
            return _floor.Spawn;
        }

        void AddStairs(MineStairs.StairKind kind, int x, int y)
        {
            var go = new GameObject("Stairs_" + kind, typeof(SpriteRenderer));
            LitSprites.Apply(go);
            go.transform.position = _map.CellCenter(new Vector3Int(x, y, 0));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            sr.sprite = StairSprite(kind);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            go.AddComponent<MineStairs>().Kind = kind;

            // The marker that says what it is: a lantern post by the way out, a crystal cluster over the ladder down, a barrel at the lift (Caves and Mines Props).
            var prop = Resources.Load<Sprite>(PropOf(kind, _floor.Floor));
            if (prop == null) return;
            var marker = new GameObject("Marker", typeof(SpriteRenderer));
            LitSprites.Apply(marker);
            marker.transform.SetParent(go.transform, false);
            marker.transform.localPosition = new Vector3(0f, -0.5f, 0f);          // the foot of the picture at the foot of the cell
            marker.transform.localScale = Vector3.one * PropScale;
            var markerRenderer = marker.GetComponent<SpriteRenderer>();
            markerRenderer.sprite = prop;
            markerRenderer.sortingOrder = 4;
        }

        public const float PropScale = 1f;          // the pictures are the mine entrance of the world map, two cells wide, at its own size

        // The way up from the first floor leaves the mine: it is drawn as the entrance in daylight.
        public static string PropOf(MineStairs.StairKind kind, int floor) => kind == MineStairs.StairKind.Up && floor <= 1 ? "Mine/prop_mine_exit" : PropOf(kind);

        public static string PropOf(MineStairs.StairKind kind) => kind == MineStairs.StairKind.Down ? "Mine/prop_mine_down" : kind == MineStairs.StairKind.Up ? "Mine/prop_mine_up" : "Mine/prop_mine_lift";

        static Sprite StairSprite(MineStairs.StairKind kind) =>
            RuntimeSprites.Square(kind == MineStairs.StairKind.Down ? new Color(0.95f, 0.8f, 0.3f)
                : kind == MineStairs.StairKind.Up ? new Color(0.4f, 0.6f, 0.95f) : new Color(0.7f, 0.7f, 0.75f));
    }
}
