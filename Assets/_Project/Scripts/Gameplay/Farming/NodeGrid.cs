using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    [Serializable]
    public sealed class NodeInstance
    {
        public int X;
        public int Y;
        public string TypeId;
        public int Hp;     // damage still needed
    }

    // Looks resource node definitions up by id (the game database's, or the built-in defaults when it has none).
    public sealed class NodeCatalog
    {
        static NodeCatalog _builtIn;
        readonly Dictionary<string, ResourceNodeDefinition> _byId = new Dictionary<string, ResourceNodeDefinition>();

        public static NodeCatalog BuiltIn => _builtIn ?? (_builtIn = new NodeCatalog(BuiltInAssets.Keep(NodeDefaults.CreateAll().Concat(MineGenerator.CreateNodes()).ToArray())));

        public NodeCatalog(IEnumerable<ResourceNodeDefinition> definitions)
        {
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id)) _byId[d.Id] = d;
        }

        public static NodeCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllNodes.ToList() : new List<ResourceNodeDefinition>();
            return all.Count == 0 ? BuiltIn : new NodeCatalog(all);
        }

        public IEnumerable<ResourceNodeDefinition> All => _byId.Values;
        public ResourceNodeDefinition Get(string id) => id != null && _byId.TryGetValue(id, out var d) ? d : null;
    }

    public enum NodeHit { None, WrongTool, TooWeak, Damaged, Cleared }

    public readonly struct NodeHitResult
    {
        public readonly NodeHit Outcome;
        public readonly string DropItemId;
        public readonly int DropCount;
        public readonly string Skill;
        public readonly int Xp;
        public readonly string LeftBehind;
        public NodeHitResult(NodeHit outcome, string dropItemId = null, int dropCount = 0, string skill = null, int xp = 0, string leftBehind = null)
        { Outcome = outcome; DropItemId = dropItemId; DropCount = dropCount; Skill = skill; Xp = xp; LeftBehind = leftBehind; }
    }

    // Trees, stumps, rocks and weeds standing on one map. Pure C# so the rules are testable; rendering is the view's job.
    public sealed class NodeGrid
    {
        readonly Dictionary<(int, int), NodeInstance> _nodes = new Dictionary<(int, int), NodeInstance>();

        public int Count => _nodes.Count;
        public IEnumerable<NodeInstance> Nodes => _nodes.Values;

        public static NodeGrid FromNodes(IEnumerable<NodeInstance> nodes)
        {
            var grid = new NodeGrid();
            if (nodes != null)
                foreach (var n in nodes) grid._nodes[(n.X, n.Y)] = n;
            return grid;
        }

        public List<NodeInstance> ToList() => new List<NodeInstance>(_nodes.Values);

        public bool Has(int x, int y) => _nodes.ContainsKey((x, y));
        public bool TryGet(int x, int y, out NodeInstance node) => _nodes.TryGetValue((x, y), out node);

        public bool Add(int x, int y, ResourceNodeDefinition def)
        {
            if (_nodes.ContainsKey((x, y))) return false;
            _nodes[(x, y)] = new NodeInstance { X = x, Y = y, TypeId = def.Id, Hp = def.HitPoints };
            return true;
        }

        public bool Remove(int x, int y) => _nodes.Remove((x, y));

        public void Clear() => _nodes.Clear();

        // Picking something up by hand (forage): no tool involved. Returns the drop, or None when there is nothing to pick.
        public NodeHitResult Gather(int x, int y, Func<string, ResourceNodeDefinition> lookup, float dropRoll)
        {
            if (!_nodes.TryGetValue((x, y), out var node)) return new NodeHitResult(NodeHit.None);
            var def = lookup(node.TypeId);
            if (def == null || def.Tool != ToolType.None) return new NodeHitResult(NodeHit.None);
            _nodes.Remove((x, y));
            var count = def.DropMin + (int)Math.Min(def.DropMax - def.DropMin, Math.Floor(dropRoll * (def.DropMax - def.DropMin + 1)));
            return new NodeHitResult(NodeHit.Cleared, def.DropItemId, Math.Max(0, count), def.Skill, def.Xp);
        }

        // One swing of `tool` (of the given tier) at a cell. `dropRoll` (0..1) picks the drop count in the node's range.
        public NodeHitResult Hit(int x, int y, ToolType tool, int tier, Func<string, ResourceNodeDefinition> lookup, float dropRoll)
        {
            if (!_nodes.TryGetValue((x, y), out var node)) return new NodeHitResult(NodeHit.None);
            var def = lookup(node.TypeId);
            if (def == null) return new NodeHitResult(NodeHit.None);
            if (def.Tool != tool) return new NodeHitResult(NodeHit.WrongTool);
            if (tier < def.MinToolTier) return new NodeHitResult(NodeHit.TooWeak);

            node.Hp -= 1 + Math.Max(0, tier);
            if (node.Hp > 0) return new NodeHitResult(NodeHit.Damaged);

            _nodes.Remove((x, y));
            var count = def.DropMin + (int)Math.Min(def.DropMax - def.DropMin, Math.Floor(dropRoll * (def.DropMax - def.DropMin + 1)));
            if (!string.IsNullOrEmpty(def.LeavesNodeId))
            {
                var leaves = lookup(def.LeavesNodeId);
                if (leaves != null) Add(x, y, leaves);
            }
            return new NodeHitResult(NodeHit.Cleared, def.DropItemId, Math.Max(0, count), def.Skill, def.Xp, def.LeavesNodeId);
        }
    }

    // Scatters starting clutter over the free cells of a map, the same way for the same seed.
    public static class NodeSpawner
    {
        // `density` is the share of candidate cells that get a node. Returns how many were placed.
        public static int Generate(NodeGrid grid, IEnumerable<(int x, int y)> candidates, IEnumerable<ResourceNodeDefinition> defs,
            int seed, string mapKey, float density)
        {
            var table = defs.Where(d => d != null && d.SpawnWeight > 0f).OrderBy(d => d.Id, StringComparer.Ordinal).ToList();
            var total = table.Sum(d => d.SpawnWeight);
            if (table.Count == 0 || total <= 0f) return 0;
            var salt = StableHash(mapKey);
            var placed = 0;
            foreach (var (x, y) in candidates.OrderBy(c => c.y).ThenBy(c => c.x))
            {
                if (grid.Has(x, y)) continue;
                var roll = WeatherRoller.Unit(x * 7919 + y * 104729 + salt, seed);
                if (roll >= density) continue;
                var pick = WeatherRoller.Unit(x * 31337 + y * 8191 + salt, seed ^ 0x5bd1e995) * total;
                foreach (var d in table)
                {
                    if (pick < d.SpawnWeight) { if (grid.Add(x, y, d)) placed++; break; }
                    pick -= d.SpawnWeight;
                }
            }
            return placed;
        }

        static int StableHash(string s)
        {
            unchecked
            {
                var h = 17;
                foreach (var c in s ?? string.Empty) h = h * 31 + c;
                return h;
            }
        }
    }
}
