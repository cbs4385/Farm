using System.Collections.Generic;
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
        GameSession _session;
        FarmMap _map;

        public static MineController Current { get; private set; }
        public MineFloor Floor => _floor;
        public bool IsWall(int x, int y) => _floor == null || _floor.IsWall(x, y);

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public void Build(GameSession session, FarmMap map)
        {
            _session = session;
            _map = map;
            Current = this;
            var ms = session.State.Mine;
            if (ms.Floor <= 0) ms.Floor = 1;
            ms.Deepest = Mathf.Max(ms.Deepest, ms.Floor);
            var day = session.Clock.Now.TotalDays;
            _floor = MineGenerator.Generate(MineGenerator.FloorSeed(session.State.WorldSeed, day, ms.Floor), ms.Floor);

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

            // Ore and rocks: kept as they were if the player already visited this floor today, fresh otherwise.
            var nodes = session.GetNodes(MapIds.Mine);
            if (ms.GenDay != day || ms.GenFloor != ms.Floor)
            {
                nodes.Clear();
                foreach (var spawn in _floor.Nodes)
                    if (session.Nodes.Get(spawn.NodeId) is ResourceNodeDefinition def) nodes.Add(spawn.X, spawn.Y, def);
                ms.GenDay = day;
                ms.GenFloor = ms.Floor;
            }

            // The arrival cell, and the stairs.
            foreach (var sp in FindObjectsByType<SpawnPoint>())
                if (sp.Id == "default") sp.transform.position = map.CellCenter(new Vector3Int(_floor.Spawn.x, _floor.Spawn.y, 0));

            AddStairs(MineStairs.StairKind.Up, _floor.Spawn.x - 1, _floor.Spawn.y);
            if (_floor.Ladder.HasValue) AddStairs(MineStairs.StairKind.Down, _floor.Ladder.Value.x, _floor.Ladder.Value.y);
            if (ms.Floor == 1 && ms.Deepest >= 5) AddStairs(MineStairs.StairKind.Elevator, _floor.Spawn.x, _floor.Spawn.y + 1);

            var manager = new GameObject("Enemies").AddComponent<EnemyManager>();
            manager.Init(map, session, this, _floor.Enemies);
        }

        void AddStairs(MineStairs.StairKind kind, int x, int y)
        {
            var go = new GameObject("Stairs_" + kind, typeof(SpriteRenderer));
            go.transform.position = _map.CellCenter(new Vector3Int(x, y, 0));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            sr.sprite = StairSprite(kind);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            go.AddComponent<MineStairs>().Kind = kind;
        }

        static Sprite StairSprite(MineStairs.StairKind kind) =>
            RuntimeSprites.Square(kind == MineStairs.StairKind.Down ? new Color(0.95f, 0.8f, 0.3f)
                : kind == MineStairs.StairKind.Up ? new Color(0.4f, 0.6f, 0.95f) : new Color(0.7f, 0.7f, 0.75f));
    }
}
