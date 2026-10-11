using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The village green can be chopped, broken and dug, and the villagers do not like it (playtest 2026-10-11: "you should be able to use your stuff anywhere, but the townies get
    // upset if they see you doing it in the village"). The trees are pictures in the village scene ("Tree_x_y"); the first time the village is loaded they become resource
    // nodes, a few rocks are scattered on open grass, and on every load these are the village's original state: when the player has felled, broken or dug anything, the
    // villagers have put it back by the next day.
    public static class VillageGreen
    {
        public const string SeededFlag = "village.trees.seeded";
        public const string RocksSeededFlag = "village.rocks.seeded";
        public const string DamagedVar = "village.damaged";          // the day (counted from 1) the player last did damage on the green; 0 = none
        static readonly Regex TreeName = new Regex(@"^Tree_(-?\d+)_(-?\d+)$", RegexOptions.Compiled);

        // Pure: has a day passed since the damage was done?
        public static bool ShouldRestore(int damagedVar, int today) => damagedVar > 0 && today + 1 > damagedVar;

        public static bool IsRestorable(NodeInstance node) => node == null || node.TypeId == NodeDefaults.Stump;

        // Pure: the rock (or boulder) that belongs on a cell of open grass, or null. Never beside anything that is not grass, so that no rock narrows a way.
        public const float RockChance = 0.009f, BoulderShare = 0.12f;
        public static string RockAt(int x, int y, Func<int, int, bool> grassy)
        {
            if (WoodLayout.Random01(x, y, 91) >= RockChance || !grassy(x, y)) return null;
            for (var dy = -1; dy <= 1; dy++)
                for (var dx = -1; dx <= 1; dx++)
                    if (!grassy(x + dx, y + dy)) return null;
            return WoodLayout.Random01(x, y, 92) < BoulderShare ? NodeDefaults.Boulder : NodeDefaults.Rock;
        }

        // The rocks are remembered in the save, one flag each ("village.rock:x,y,type"), so that "the way it was" never depends on what happens to be standing on the
        // green when the village loads (a villager, a placed chest, a sapling).
        public const string RockFlagPrefix = "village.rock:";
        public static string RockFlag(int x, int y, string type) => $"{RockFlagPrefix}{x},{y},{type}";

        public static bool TryParseRockFlag(string flag, out int x, out int y, out string type)
        {
            x = y = 0; type = null;
            if (flag == null || !flag.StartsWith(RockFlagPrefix, StringComparison.Ordinal)) return false;
            var parts = flag.Substring(RockFlagPrefix.Length).Split(',');
            if (parts.Length != 3 || !int.TryParse(parts[0], out x) || !int.TryParse(parts[1], out y)) return false;
            type = parts[2];
            return type.Length > 0;
        }

        static List<(Vector3Int cell, string type)> StoredRocks(GameSession session)
        {
            var list = new List<(Vector3Int, string)>();
            foreach (var flag in session.State.Flags)
                if (TryParseRockFlag(flag, out var x, out var y, out var type)) list.Add((new Vector3Int(x, y, 0), type));
            return list;
        }

        static List<(Vector3Int cell, string type)> Rocks(FarmMap map)
        {
            var list = new List<(Vector3Int, string)>();
            map.Ground.CompressBounds();
            var memo = new Dictionary<(int, int), bool>();
            bool Grassy(int x, int y)
            {
                if (memo.TryGetValue((x, y), out var known)) return known;
                return memo[(x, y)] = map.HasTillableGround(new Vector3Int(x, y, 0));
            }
            foreach (var cell in map.Ground.cellBounds.allPositionsWithin)
            {
                var type = RockAt(cell.x, cell.y, Grassy);
                if (type != null) list.Add((cell, type));
            }
            return list;
        }

        // Called when the village map starts, before its nodes are drawn. Returns how many things the villagers have put back (0 on a first visit and on a day with no damage).
        public static int Prepare(GameSession session, FarmMap map)
        {
            var tree = session.Nodes.Get(NodeDefaults.Tree);
            if (tree == null) return 0;
            var cells = new List<Vector3Int>();
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!TreeName.IsMatch(t.name)) continue;
                cells.Add(map.WorldToCell(t.position));
                UnityEngine.Object.Destroy(t.gameObject);                      // the node is drawn in its place
            }
            var nodes = session.GetNodes(MapIds.Village);
            var rocks = StoredRocks(session);
            var firstTrees = !session.HasFlag(SeededFlag);
            var firstRocks = !session.HasFlag(RocksSeededFlag);
            if (firstTrees) { session.SetFlag(SeededFlag); foreach (var c in cells) nodes.Add(c.x, c.y, tree); }
            if (firstRocks || rocks.Count == 0)                                  // a new game, or a save from before the rocks were remembered: choose them now
            {
                session.SetFlag(RocksSeededFlag);
                rocks = Rocks(map);
                foreach (var (cell, type) in rocks) session.SetFlag(RockFlag(cell.x, cell.y, type));
                if (firstRocks) AddRocks(session, rocks);
            }
            if (firstTrees || firstRocks) return 0;

            if (!ShouldRestore(session.GetVar(DamagedVar), session.Clock.Now.TotalDays)) return 0;
            var restored = session.GetGrid(MapIds.Village).Clear();              // dug soil is levelled
            restored += AddRocks(session, rocks);                                // broken rocks are back
            return restored + Restore(session, cells, tree);                     // felled trees are replanted (and the damage forgiven)
        }

        static int AddRocks(GameSession session, IEnumerable<(Vector3Int cell, string type)> rocks)
        {
            var nodes = session.GetNodes(MapIds.Village);
            var added = 0;
            foreach (var (cell, type) in rocks)
            {
                var def = session.Nodes.Get(type);
                if (def != null && nodes.Add(cell.x, cell.y, def)) added++;
            }
            return added;
        }

        public static int Restore(GameSession session, IList<Vector3Int> cells, ResourceNodeDefinition tree)
        {
            var nodes = session.GetNodes(MapIds.Village);
            var restored = 0;
            foreach (var c in cells)
            {
                if (nodes.TryGet(c.x, c.y, out var node))
                {
                    if (!IsRestorable(node)) continue;
                    nodes.Remove(c.x, c.y);
                }
                if (nodes.Add(c.x, c.y, tree)) restored++;
            }
            session.SetVar(DamagedVar, 0);
            return restored;
        }
    }
}
