using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Farm.Data
{
    // One thing a spawn table can put on the ground.
    [Serializable]
    public sealed class SpawnEntry
    {
        public string NodeId;
        public float Weight = 1f;
        public SeasonMask Seasons = SeasonMask.All;
        public string Condition;          // optional (see Conditions): the entry only spawns while it holds
        public bool LuckSensitive;        // rare finds: the weight is scaled by 1 + luck, so lucky players see more of them
    }

    // What appears on a map over time: each morning (when the map is entered) it makes a few attempts to put one of
    // its entries on a free cell of the allowed ground, until the map holds `MaxNodes` nodes of the table's kinds.
    // Used for forage and for the slow regrowth of farm clutter. Modules add tables through content packs.
    [CreateAssetMenu(menuName = "Farm/Spawn Table")]
    public sealed class SpawnTableDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string _mapId;
        [SerializeField] string[] _groundTiles;     // names of the ground tiles it may spawn on
        [SerializeField] int _dailyAttempts = 3;
        [SerializeField] int _maxNodes = 20;
        [SerializeField] bool _spreads;             // kinds still standing on the map come back more readily than kinds picked clean (forage)
        [SerializeField] List<SpawnEntry> _entries = new List<SpawnEntry>();

        public string Id => _id;
        public string MapId => _mapId;
        public IReadOnlyList<string> GroundTiles => _groundTiles;
        public int DailyAttempts => _dailyAttempts;
        public int MaxNodes => _maxNodes;
        public bool Spreads => _spreads;
        public IReadOnlyList<SpawnEntry> Entries => _entries;

        public static SpawnTableDefinition Create(string id, string mapId, string[] groundTiles, int dailyAttempts, int maxNodes,
            IEnumerable<SpawnEntry> entries, bool spreads = false)
        {
            var t = CreateInstance<SpawnTableDefinition>();
            t._id = id;
            t.name = id;
            t._mapId = mapId;
            t._groundTiles = groundTiles;
            t._dailyAttempts = dailyAttempts;
            t._maxNodes = maxNodes;
            t._spreads = spreads;
            t._entries = entries.ToList();
            return t;
        }
    }

    // The wild things the player can pick up by hand, and where and when they grow.
    public static class ForageDefaults
    {
        public enum Place { Meadow, Wood, Shore }

        public readonly struct Row
        {
            public readonly string Id;           // item "forage.<id>" and node "<id>"
            public readonly Place Where;
            public readonly SeasonMask Seasons;
            public readonly int Price;
            public readonly float Weight;
            public readonly bool Rare;
            public readonly Color Color;
            public Row(string id, Place where, SeasonMask seasons, int price, float weight, Color color, bool rare = false)
            { Id = id; Where = where; Seasons = seasons; Price = price; Weight = weight; Color = color; Rare = rare; }
            public string ItemId => "forage." + Id;
        }

        public static readonly Row[] Rows =
        {
            new Row("dandelion", Place.Meadow, SeasonMask.Spring, 40, 10f, new Color(0.95f, 0.85f, 0.25f)),
            new Row("wildgarlic", Place.Wood, SeasonMask.Spring, 60, 8f, new Color(0.80f, 0.90f, 0.70f)),
            new Row("raspberry", Place.Wood, SeasonMask.Summer, 50, 8f, new Color(0.85f, 0.20f, 0.35f)),
            new Row("elderflower", Place.Meadow, SeasonMask.Summer, 70, 8f, new Color(0.95f, 0.93f, 0.85f)),
            new Row("hazelnut", Place.Wood, SeasonMask.Fall, 40, 10f, new Color(0.65f, 0.45f, 0.25f)),
            new Row("mushroom", Place.Wood, SeasonMask.Fall, 90, 6f, new Color(0.70f, 0.55f, 0.45f)),
            new Row("blackberry", Place.Meadow, SeasonMask.Fall, 25, 10f, new Color(0.30f, 0.15f, 0.40f)),
            new Row("truffle", Place.Wood, SeasonMask.Fall, 400, 1f, new Color(0.25f, 0.20f, 0.18f), rare: true),
            new Row("snowdrop", Place.Meadow, SeasonMask.Winter, 80, 8f, new Color(0.90f, 0.95f, 1.00f)),
            new Row("winterroot", Place.Wood, SeasonMask.Winter, 70, 8f, new Color(0.65f, 0.50f, 0.60f)),
            new Row("seashell", Place.Shore, SeasonMask.All, 60, 10f, new Color(0.95f, 0.75f, 0.70f)),
            new Row("clam", Place.Shore, SeasonMask.All, 50, 8f, new Color(0.75f, 0.75f, 0.80f)),
            new Row("pearl", Place.Shore, SeasonMask.All, 500, 1f, new Color(0.97f, 0.97f, 0.97f), rare: true),
        };

        // Forage as nodes the hand can pick (no tool): one pick, forage XP, never part of the starting clutter.
        public static ResourceNodeDefinition[] CreateNodes() => Rows.Select(r =>
            ResourceNodeDefinition.Create(r.Id, ToolType.None, 1, 0, r.ItemId, 1, 1, "foraging", 7, null, false, 0f)).ToArray();

        static SpawnEntry Entry(string node, float weight, SeasonMask seasons, bool luck = false) =>
            new SpawnEntry { NodeId = node, Weight = weight, Seasons = seasons, LuckSensitive = luck };

        public static SpawnTableDefinition[] CreateTables()
        {
            SpawnTableDefinition Forage(string id, string map, Place place, string tile, int attempts, int max) =>
                SpawnTableDefinition.Create(id, map, new[] { tile }, attempts, max,
                    Rows.Where(r => r.Where == place).Select(r => Entry(r.Id, r.Weight, r.Seasons, r.Rare)), spreads: true);

            return new[]
            {
                // The farm slowly grows over again.
                SpawnTableDefinition.Create("farm.clutter", MapIdFarm, new[] { "tile_grass", "tile_dirt" }, 8, 160, new[]
                {
                    Entry(NodeDefaults.Weed, 60f, SeasonMask.Spring | SeasonMask.Summer | SeasonMask.Fall),
                    Entry(NodeDefaults.Rock, 8f, SeasonMask.All),
                    Entry(NodeDefaults.Tree, 12f, SeasonMask.All),          // playtest 2026-10-09: not enough trees (was 4 in 94 on 5 tries a day)
                    Entry(NodeDefaults.Stump, 2f, SeasonMask.All),
                    Entry(NodeDefaults.Sunpatch, 10f, SeasonMask.Summer | SeasonMask.Fall),
                }),
                Forage("village.forage", "Village", Place.Meadow, "tile_grass", 5, 26),
                Forage("forest.forage", "Forest", Place.Wood, "tile_forest", 7, 42),
                Forage("beach.forage", "Beach", Place.Shore, "tile_sand", 5, 22),
            };
        }

        const string MapIdFarm = "Farm";
    }
}
