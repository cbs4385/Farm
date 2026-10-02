using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // A* over a map's walkable cells (four directions). Pure C#: the scene decides which cells are walkable.
    public sealed class WalkGrid
    {
        readonly int _minX, _minY, _width, _height;
        readonly bool[] _walkable;

        public WalkGrid(int minX, int minY, int width, int height, Func<int, int, bool> isWalkable)
        {
            _minX = minX; _minY = minY; _width = width; _height = height;
            _walkable = new bool[width * height];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    _walkable[y * width + x] = isWalkable(minX + x, minY + y);
        }

        public bool Contains(int x, int y) => x >= _minX && y >= _minY && x < _minX + _width && y < _minY + _height;

        public bool IsWalkable(int x, int y) => Contains(x, y) && _walkable[(y - _minY) * _width + (x - _minX)];

        // Cells from start to goal inclusive, or null when there is no way. A blocked start or goal is allowed (an NPC
        // standing "on" furniture still has to be able to leave); only the cells between must be walkable.
        public List<(int x, int y)> FindPath(int sx, int sy, int gx, int gy)
        {
            if (!Contains(sx, sy) || !Contains(gx, gy)) return null;
            if (sx == gx && sy == gy) return new List<(int, int)> { (sx, sy) };

            var count = _width * _height;
            var cost = new int[count];
            var parent = new int[count];
            var closed = new bool[count];
            for (var i = 0; i < count; i++) { cost[i] = int.MaxValue; parent[i] = -1; }

            int Index(int x, int y) => (y - _minY) * _width + (x - _minX);
            int Heuristic(int x, int y) => Math.Abs(x - gx) + Math.Abs(y - gy);

            var open = new List<(int f, int x, int y)>();
            var start = Index(sx, sy);
            cost[start] = 0;
            open.Add((Heuristic(sx, sy), sx, sy));
            var goal = Index(gx, gy);
            var dx = new[] { 1, -1, 0, 0 };
            var dy = new[] { 0, 0, 1, -1 };

            while (open.Count > 0)
            {
                var best = 0;
                for (var i = 1; i < open.Count; i++) if (open[i].f < open[best].f) best = i;
                var (_, cx, cy) = open[best];
                open.RemoveAt(best);
                var ci = Index(cx, cy);
                if (closed[ci]) continue;
                closed[ci] = true;
                if (ci == goal) break;

                for (var d = 0; d < 4; d++)
                {
                    var nx = cx + dx[d]; var ny = cy + dy[d];
                    if (!Contains(nx, ny)) continue;
                    var ni = Index(nx, ny);
                    if (closed[ni]) continue;
                    if (ni != goal && !_walkable[ni]) continue;
                    var next = cost[ci] + 1;
                    if (next >= cost[ni]) continue;
                    cost[ni] = next;
                    parent[ni] = ci;
                    open.Add((next + Heuristic(nx, ny), nx, ny));
                }
            }

            if (parent[goal] < 0) return null;
            var path = new List<(int, int)>();
            for (var at = goal; at >= 0; at = parent[at])
            {
                path.Add((_minX + at % _width, _minY + at / _width));
                if (at == start) break;
            }
            path.Reverse();
            return path;
        }
    }
}
