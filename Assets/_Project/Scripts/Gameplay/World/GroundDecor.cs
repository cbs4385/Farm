using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Scatters small things over the grass of an outdoor map: tufts and flowers in the colours of the season (green in spring and summer, orange in fall, pale blue in
    // winter). A layer of tiles between the ground and the soil, made when the map starts, from the pictures in Resources/Decor. Only where nothing stands: not on a
    // path, a wall or water; tilled soil, crops and everything else draw over it. Playtest 2026-10-09: "the world looks flat".
    public sealed class GroundDecor : MonoBehaviour
    {
        static readonly string[] GroundNames = { "tile_grass", "tile_forest", "tile_sand" };
        const string SandGround = "tile_sand";
        static Dictionary<string, Sprite[]> _sprites;

        public Tilemap Layer { get; private set; }

        public static string SetFor(Season season) => season == Season.Fall ? "fall" : season == Season.Winter ? "winter" : "spring";

        static Sprite[] Load(string set, string kind)
        {
            _sprites ??= Resources.LoadAll<Sprite>("Decor")
                .GroupBy(s => Regex.Replace(s.name, @"\d+$", string.Empty))
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray());
            return _sprites.TryGetValue("decor_" + set + "_" + kind, out var list) ? list : new Sprite[0];
        }

        static GroundDecor() => TestResets.Add(ResetForTests);
        public static void ResetForTests() => _sprites = null;

        // Lays the decoration for a season over the map's grass; returns how many cells carry something.
        public int Build(FarmMap map, int seed, Season season)
        {
            var set = SetFor(season);
            var tufts = Load(set, "t");
            var flowers = Load(set, "f");
            var sandItems = Load("sand", "t");                      // pebbles, driftwood and dune grass on the sand, in every season
            if (tufts.Length + flowers.Length == 0 || map.Ground == null) return 0;

            if (Layer != null) Destroy(Layer.gameObject);
            var go = new GameObject("Decor");
            go.transform.SetParent(map.Ground.transform.parent, false);
            Layer = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;
            map.Ground.GetComponent<TilemapRenderer>().sortingOrder = -1;          // the ground under the decoration, both under the soil

            var tiles = new Dictionary<Sprite, Tile>();
            Tile TileFor(Sprite s)
            {
                if (!tiles.TryGetValue(s, out var t)) { t = ScriptableObject.CreateInstance<Tile>(); t.sprite = s; t.name = s.name; tiles[s] = t; }
                return t;
            }

            var placed = 0;
            foreach (var cell in map.Ground.cellBounds.allPositionsWithin)
            {
                var ground = map.Ground.GetTile(cell);
                if (ground == null || System.Array.IndexOf(GroundNames, ground.name) < 0) continue;
                if (map.Walls != null && map.Walls.GetTile(cell) != null) continue;
                var onSand = ground.name == SandGround;
                var cellTufts = onSand ? sandItems : tufts;
                var cellFlowers = onSand ? new Sprite[0] : flowers;
                var pick = DecorPlanner.At(seed, map.MapId, cell.x, cell.y, cellTufts.Length, cellFlowers.Length, out var flower);
                if (pick < 0) continue;
                Layer.SetTile(cell, TileFor(flower ? cellFlowers[pick] : cellTufts[pick]));
                placed++;
            }
            return placed;
        }
    }
}
