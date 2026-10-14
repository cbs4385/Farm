using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The three sealing relics lie in an old chest on the mine's 30th floor (they used to lie in Harrow Wood, where trees could wall them in). Where on the floor the
    // chest stands is chosen from the world seed, so it is a different place in every game; the floor itself is rebuilt each day, so on another day it is another cell.
    public static class MythosRelics
    {
        public const int Floor = 30;

        public static string FlagOf(int index) => $"mythos.relic.found.{index + 1}";

        public static bool AnyLeft(GameSession s)
        {
            for (var i = 0; i < MythosData.Relics.Length; i++) if (!s.HasFlag(FlagOf(i))) return true;
            return false;
        }

        // Pure: the cell of the floor that holds the chest. A cave cell that can be walked to, well away from the arrival, with no ore or monster on it, and that
        // does not cut the floor in two (a chest is solid). Null when the floor has none.
        public static (int x, int y)? ChestCell(MineFloor f, int worldSeed)
        {
            var reach = Reachable(f, null);
            var taken = new HashSet<(int, int)> { f.Spawn, f.Up, f.Elevator };
            if (f.Ladder.HasValue) taken.Add(f.Ladder.Value);
            foreach (var n in f.Nodes) taken.Add((n.X, n.Y));
            foreach (var e in f.Enemies) taken.Add((e.X, e.Y));
            var far = new List<(int x, int y)>();
            for (var y = 1; y < f.Height - 1; y++)
                for (var x = 1; x < f.Width - 1; x++)
                    if (reach.Contains((x, y)) && !taken.Contains((x, y)) && System.Math.Abs(x - f.Spawn.x) + System.Math.Abs(y - f.Spawn.y) >= 10) far.Add((x, y));
            if (far.Count == 0) return null;
            var start = (int)(WeatherRoller.Unit(Floor, worldSeed ^ 0x51D3A7) * far.Count) % far.Count;
            for (var i = 0; i < far.Count; i++)
            {
                var cell = far[(start + i) % far.Count];
                if (Reachable(f, cell).Count == reach.Count - 1) return cell;
            }
            return null;
        }

        // The open cells that can be walked to from the arrival (the stairs and the lift are solid), with one more cell optionally blocked.
        static HashSet<(int, int)> Reachable(MineFloor f, (int x, int y)? blocked)
        {
            var seen = new HashSet<(int, int)> { f.Spawn };
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue(f.Spawn);
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                foreach (var n in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) })
                    if (!f.IsWall(n.Item1, n.Item2) && n != f.Up && n != f.Elevator && (!blocked.HasValue || n != blocked.Value) && seen.Add(n)) queue.Enqueue(n);
            }
            return seen;
        }

        // The map hook: on the floor, while there are relics left to find, put the chest in.
        public static void OnMapLoaded(MapLoadedContext ctx)
        {
            var s = ctx.Session;
            if (ctx.MapId != MapIds.Mine || !MythosLevel.On(s) || s.State.Mine.Floor != Floor || !AnyLeft(s)) return;
            var mine = Object.FindAnyObjectByType<MineController>();
            if (mine == null || mine.Floor == null) return;
            var cell = ChestCell(mine.Floor, s.State.WorldSeed);
            if (!cell.HasValue) return;
            new GameObject("RelicChest").AddComponent<RelicChest>().Init(s, ctx.Map, new Vector3Int(cell.Value.x, cell.Value.y, 0));
        }
    }
}
