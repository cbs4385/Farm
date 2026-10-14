using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // The player's progress underground. Saved in GameState.Mine (additive).
    [Serializable]
    public sealed class MineState
    {
        public int Floor;             // the floor the player is on (0 = not in the mine)
        public int Deepest;           // the deepest floor reached (elevator stops every 5th floor up to here)
        public int GenDay = -1;       // the day and floor whose nodes are stored in MapState("Mine")
        public int GenFloor = -1;
        public int BossesDefeated;
        public int Kills;

        // The floors of this trip underground, as the player left them (playtest 2026-10-13: "coming out of the mine you have to fight your way back out"). A floor is
        // generated once per trip: going up the stairs finds it as it was, with the monsters that were killed still dead and the ore that was broken still gone. The
        // trip ends when the player leaves the mine (or is knocked out, or the day ends): the next trip generates every floor afresh.
        public List<MineFloorMemory> Memory = new List<MineFloorMemory>();
        public MineFloorMemory MemoryOf(int floor) => Memory.Find(m => m.Floor == floor);
        public void ForgetTrip() => Memory.Clear();
    }

    [Serializable]
    public sealed class MineFloorMemory
    {
        public int Floor;
        public List<NodeInstance> Nodes = new List<NodeInstance>();        // the ore and rocks that are left
        public List<int> Dead = new List<int>();                           // indices of the monsters that were killed (their place in the generated floor's list)
    }

    public readonly struct MineNodeSpawn
    {
        public readonly int X, Y; public readonly string NodeId;
        public MineNodeSpawn(int x, int y, string nodeId) { X = x; Y = y; NodeId = nodeId; }
    }

    public readonly struct MineEnemySpawn
    {
        public readonly int X, Y; public readonly string EnemyId;
        public MineEnemySpawn(int x, int y, string enemyId) { X = x; Y = y; EnemyId = enemyId; }
    }

    // One generated floor: walls, where the player arrives, the ladder down (none on the last floor), ore and rocks, monsters.
    public sealed class MineFloor
    {
        public int Floor, Width, Height;
        public bool[] Wall;
        public (int x, int y) Spawn;
        public (int x, int y)? Ladder;
        public (int x, int y) Up, Elevator;           // the way up beside the arrival cell and the lift two rows below it (their pictures are two cells wide and tall and must not overlap); both are solid, so the floor is walked without them
        public List<MineNodeSpawn> Nodes = new List<MineNodeSpawn>();
        public List<MineEnemySpawn> Enemies = new List<MineEnemySpawn>();
        public bool IsBoss;

        public bool IsWall(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height || Wall[y * Width + x];
        public int OpenCells => Wall.Count(w => !w);
    }

    // Deterministic splitmix64 so floors are identical on every platform and across versions of the runtime.
    public struct DetRandom
    {
        ulong _s;
        public DetRandom(long seed) { _s = (ulong)seed * 0x9E3779B97F4A7C15UL + 0x1234567UL; }
        public ulong Next()
        {
            _s += 0x9E3779B97F4A7C15UL;
            var z = _s;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
        public int Range(int minInclusive, int maxExclusive) => minInclusive + (int)(Next() % (ulong)Math.Max(1, maxExclusive - minInclusive));
        public float Unit() => (Next() >> 40) / (float)(1 << 24);
    }

    // T-051: the mine. `Generate(seed, floor)` always gives the same floor, with the ladder reachable from the spawn.
    public static class MineGenerator
    {
        public const int Width = 32, Height = 24, Floors = 40;
        public const string Copper = "copper_node", Iron = "iron_node", Gold = "gold_node", Coal = "coal_node";

        public static int FloorSeed(int worldSeed, int day, int floor) => unchecked(worldSeed * 31 + day * 7919 + floor * 104729);

        // `slimes` false leaves the slimes out (the cozy setting: only the bats are left).
        public static MineFloor Generate(int seed, int floor, bool slimes = true)
        {
            floor = Math.Max(1, Math.Min(Floors, floor));
            var rng = new DetRandom(seed * 31 + floor);
            var f = new MineFloor { Floor = floor, Width = Width, Height = Height, Wall = Enumerable.Repeat(true, Width * Height).ToArray(), IsBoss = floor == Floors };
            f.Spawn = (3, Height / 2);
            f.Up = (f.Spawn.x - 1, f.Spawn.y);
            f.Elevator = (f.Spawn.x, f.Spawn.y - 2);       // the pictures are two cells wide and tall: two rows below the arrival, so the lift does not stand over the way up (playtest 2026-10-14)

            if (f.IsBoss) CarveArena(f); else CarveCaves(f, rng);

            // The stairs up and the lift stand in cells that cannot be walked through, so every distance is measured around them: the ladder down, the ore and
            // the monsters are all reachable from the arrival cell without passing them (playtest 2026-10-13: the ways in and out were not guaranteed reachable).
            var dist = Distances(f);
            if (!f.IsBoss) f.Ladder = Farthest(f, dist);

            PlaceNodes(f, rng, dist);
            KeepPathClear(f, dist);
            PlaceEnemies(f, rng, dist, slimes);
            return f;
        }

        static void Open(MineFloor f, int x, int y)
        {
            if (x > 0 && y > 0 && x < f.Width - 1 && y < f.Height - 1) f.Wall[y * f.Width + x] = false;
        }

        static void CarveArena(MineFloor f)
        {
            for (var y = 2; y < f.Height - 2; y++)
                for (var x = 2; x < f.Width - 2; x++) Open(f, x, y);
        }

        // A drunkard's walk from the spawn until about 45% of the floor is open: always one connected cave.
        static void CarveCaves(MineFloor f, DetRandom rng)
        {
            var target = (int)(f.Width * f.Height * 0.45f);
            int x = f.Spawn.x, y = f.Spawn.y, open = 0;
            for (var i = 0; i < 20000 && open < target; i++)
            {
                if (f.IsWall(x, y) && x > 0 && y > 0 && x < f.Width - 1 && y < f.Height - 1) open++;
                Open(f, x, y);
                // thick tunnels: open a neighbour too
                if (rng.Range(0, 3) == 0) Open(f, x + 1, y);
                switch (rng.Range(0, 4))
                {
                    case 0: x++; break;
                    case 1: x--; break;
                    case 2: y++; break;
                    default: y--; break;
                }
                x = Math.Max(1, Math.Min(f.Width - 2, x));
                y = Math.Max(1, Math.Min(f.Height - 2, y));
            }
            f.Spawn = (f.Spawn.x, f.Spawn.y);
            Open(f, f.Spawn.x, f.Spawn.y);
            Open(f, f.Spawn.x + 1, f.Spawn.y);
            Open(f, f.Spawn.x - 1, f.Spawn.y);
            Open(f, f.Spawn.x, f.Spawn.y + 1);
            Open(f, f.Spawn.x, f.Spawn.y - 1);
            Open(f, f.Elevator.x, f.Elevator.y);
        }

        // Steps from the spawn to every open cell (-1 = unreachable), four directions.
        public static int[] Distances(MineFloor f)
        {
            var dist = Enumerable.Repeat(-1, f.Width * f.Height).ToArray();
            var q = new Queue<(int, int)>();
            var blocked = new HashSet<(int, int)> { f.Up, f.Elevator };
            dist[f.Spawn.y * f.Width + f.Spawn.x] = 0;
            q.Enqueue(f.Spawn);
            var dx = new[] { 1, -1, 0, 0 }; var dy = new[] { 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                for (var d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (f.IsWall(nx, ny) || dist[ny * f.Width + nx] >= 0 || blocked.Contains((nx, ny))) continue;
                    dist[ny * f.Width + nx] = dist[y * f.Width + x] + 1;
                    q.Enqueue((nx, ny));
                }
            }
            return dist;
        }

        static (int, int) Farthest(MineFloor f, int[] dist)
        {
            var best = f.Spawn; var bestD = -1;
            for (var y = 0; y < f.Height; y++)
                for (var x = 0; x < f.Width; x++)
                    if (dist[y * f.Width + x] > bestD) { bestD = dist[y * f.Width + x]; best = (x, y); }
            return best;
        }

        static string OreFor(int floor, DetRandom rng)
        {
            var r = rng.Unit();
            if (r < 0.10f) return Coal;
            if (floor >= 25 && r < 0.45f) return Gold;
            if (floor >= 10 && r < 0.65f) return Iron;
            if (floor <= 15 && r < 0.80f) return Copper;
            if (floor > 15 && r < 0.30f) return Iron;
            return r < 0.9f ? NodeDefaults.Rock : NodeDefaults.Boulder;
        }

        static void PlaceNodes(MineFloor f, DetRandom rng, int[] dist)
        {
            if (f.IsBoss) return;
            var cells = new List<(int x, int y)>();
            for (var y = 1; y < f.Height - 1; y++)
                for (var x = 1; x < f.Width - 1; x++)
                    if (dist[y * f.Width + x] > 3 && !NextToLadder(f, x, y)) cells.Add((x, y));
            foreach (var c in cells)
            {
                if (rng.Unit() >= 0.12f) continue;
                var gem = f.Floor >= GemDefaults.FirstFloor && rng.Unit() < GemDefaults.ChanceOnFloor(f.Floor) ? GemDefaults.NodeFor(f.Floor, rng.Unit()) : null;          // deeper floors: some of the ore spots are gem clusters
                f.Nodes.Add(new MineNodeSpawn(c.x, c.y, gem ?? OreFor(f.Floor, rng)));
            }
        }

        // One walkable route from the arrival cell to the ladder is kept free of ore and rocks (a boulder in a one-cell tunnel would shut the way down for anyone
        // who cannot break it yet). The route is the shortest one, followed back from the ladder.
        static void KeepPathClear(MineFloor f, int[] dist)
        {
            if (!f.Ladder.HasValue) return;
            var (x, y) = f.Ladder.Value;
            var dx = new[] { 1, -1, 0, 0 }; var dy = new[] { 0, 0, 1, -1 };
            while (dist[y * f.Width + x] > 0)
            {
                f.Nodes.RemoveAll(n => n.X == x && n.Y == y);
                var here = dist[y * f.Width + x];
                for (var d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (nx < 0 || ny < 0 || nx >= f.Width || ny >= f.Height || dist[ny * f.Width + nx] != here - 1) continue;
                    x = nx; y = ny;
                    break;
                }
            }
        }

        // The ladder and the cells around it stay clear: ore there could shut the way to it, and a monster there would wait for whoever arrives.
        static bool NextToLadder(MineFloor f, int x, int y) => f.Ladder.HasValue && Math.Abs(f.Ladder.Value.x - x) + Math.Abs(f.Ladder.Value.y - y) <= 1;

        static void PlaceEnemies(MineFloor f, DetRandom rng, int[] dist, bool slimes)
        {
            if (f.IsBoss) { f.Enemies.Add(new MineEnemySpawn(f.Width / 2 + 4, f.Height / 2, EnemyDefaults.Boss)); return; }
            var kinds = EnemyDefaults.Rows.Where(r => !r.Boss && r.MinFloor <= f.Floor && r.MaxFloor >= f.Floor && r.Weight > 0 && (slimes || r.Id != "slime")).ToList();
            if (kinds.Count == 0) return;
            var count = Math.Min(10, 3 + f.Floor / 6);
            var cells = new List<(int x, int y)>();
            for (var y = 1; y < f.Height - 1; y++)
                for (var x = 1; x < f.Width - 1; x++)
                    if (dist[y * f.Width + x] >= 8 && !NextToLadder(f, x, y) && !f.Nodes.Any(n => n.X == x && n.Y == y)) cells.Add((x, y));
            if (cells.Count == 0) return;
            var total = kinds.Sum(k => k.Weight);
            for (var i = 0; i < count; i++)
            {
                var cell = cells[rng.Range(0, cells.Count)];
                var at = rng.Unit() * total;
                var kind = kinds[kinds.Count - 1];
                foreach (var k in kinds) { if (at < k.Weight) { kind = k; break; } at -= k.Weight; }
                f.Enemies.Add(new MineEnemySpawn(cell.x, cell.y, kind.Id));
            }
        }

        // The elevator stops at every fifth floor the player has reached (and floor 1).
        public static List<int> ElevatorFloors(int deepest)
        {
            var list = new List<int> { 1 };
            for (var f = 5; f <= Math.Min(deepest, Floors); f += 5) list.Add(f);
            return list;
        }

        // The ore nodes the mine uses (the content tools write them as assets; sprites obj_<id>). Never part of clutter.
        public static ResourceNodeDefinition[] CreateNodes() => GemDefaults.CreateNodes().Concat(new[]
        {
            ResourceNodeDefinition.Create(Copper, ToolType.Pickaxe, 3, 0, ItemIds.CopperOre, 1, 3, "mining", 6, null, true, 0f),
            ResourceNodeDefinition.Create(Iron, ToolType.Pickaxe, 4, 1, ItemIds.IronOre, 1, 3, "mining", 10, null, true, 0f),
            ResourceNodeDefinition.Create(Gold, ToolType.Pickaxe, 5, 2, ItemIds.GoldOre, 1, 2, "mining", 16, null, true, 0f),
            ResourceNodeDefinition.Create(Coal, ToolType.Pickaxe, 3, 0, ItemIds.Coal, 1, 2, "mining", 8, null, true, 0f),
        }).ToArray();
    }
}
