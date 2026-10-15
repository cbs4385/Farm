using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Softens the outline of a trail through the forest. A trail painted in whole tiles has straight edges that follow the grid, which read as a road (playtest 2026-10-15).
    // Where a path (or dirt) cell meets forest floor, a layer of small pictures is laid over both: made when the map starts from the two tiles' own pixels, with the line
    // between them wandering across the cell borders (smooth noise for the broad bends, fine noise for a ragged edge) and a worn rim of slightly darker trail along it.
    // The tiles underneath stay as they are, so walking, footsteps and farming rules do not change.
    public sealed class TrailEdges : MonoBehaviour
    {
        public const float Threshold = 0.5f;
        public const float RimWidth = 0.08f;
        const string WoodTile = "tile_forest";
        const float BroadNoise = 0.5f, FineNoise = 0.3f;         // peak-to-peak: with the two together the line moves by at most 0.4 of a cell, so only cells next to the trail are drawn

        readonly List<UnityEngine.Object> _made = new List<UnityEngine.Object>();

        public Tilemap Layer { get; private set; }

        public static bool IsTrail(string tile) => tile == "tile_path" || tile == "tile_dirt";
        public static bool IsWood(string tile) => tile == WoodTile;

        // How much of a point is trail, 0..1, 0.5 being the edge. `trail` says whether a cell is trail; the point is in cell units. Pure, so that it can be tested.
        public static float Coverage(Func<int, int, bool> trail, float x, float y, int seed)
        {
            var u = x - 0.5f;
            var v = y - 0.5f;
            var i = Mathf.FloorToInt(u);
            var j = Mathf.FloorToInt(v);
            var fx = u - i;
            var fy = v - j;
            float T(int cx, int cy) => trail(cx, cy) ? 1f : 0f;
            var bottom = Mathf.Lerp(T(i, j), T(i + 1, j), fx);
            var top = Mathf.Lerp(T(i, j + 1), T(i + 1, j + 1), fx);
            var field = Mathf.Lerp(bottom, top, fy);
            return field + (WoodLayout.Noise(x, y, 0.8f, seed) - 0.5f) * BroadNoise + (WoodLayout.Noise(x, y, 0.12f, seed + 1) - 0.5f) * FineNoise;
        }

        // Lays the edge pictures over the map's ground; returns how many cells carry one.
        public int Build(FarmMap map, int seed)
        {
            if (map.Ground == null) return 0;
            var ground = map.Ground;
            var cells = new List<(Vector3Int cell, bool trail, string other)>();
            var names = new Dictionary<Vector3Int, string>();
            foreach (var cell in ground.cellBounds.allPositionsWithin)
            {
                var t = ground.GetTile(cell);
                if (t != null) names[cell] = t.name;
            }
            bool Trail(int x, int y) => names.TryGetValue(new Vector3Int(x, y, 0), out var n) && IsTrail(n);
            bool Wood(int x, int y) => names.TryGetValue(new Vector3Int(x, y, 0), out var n) && IsWood(n);

            foreach (var kv in names)
            {
                var isTrail = IsTrail(kv.Value);
                if (!isTrail && !IsWood(kv.Value)) continue;
                string other = null;
                for (var dx = -1; dx <= 1 && other == null; dx++)
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var nx = kv.Key.x + dx;
                        var ny = kv.Key.y + dy;
                        if (isTrail ? Wood(nx, ny) : Trail(nx, ny)) { other = names[new Vector3Int(nx, ny, 0)]; break; }
                    }
                if (other != null) cells.Add((kv.Key, isTrail, other));
            }
            if (cells.Count == 0) return 0;

            var patches = new Dictionary<string, (Color32[] pixels, int w, int h)>();
            bool Patch(string tileName, Vector3Int at, out (Color32[] pixels, int w, int h) patch)
            {
                if (patches.TryGetValue(tileName, out patch)) return patch.pixels != null;
                var sprite = ground.GetSprite(at);
                patch = sprite != null ? ReadPixels(sprite) : default;
                patches[tileName] = patch;
                return patch.pixels != null;
            }

            if (Layer != null) Destroy(Layer.gameObject);
            var go = new GameObject("TrailEdges");
            go.transform.SetParent(ground.transform.parent, false);
            Layer = go.AddComponent<Tilemap>();
            Layer.color = ground.color;
            var renderer = go.AddComponent<TilemapRenderer>();
            LitSprites.Apply(renderer);

            var placed = 0;
            foreach (var (cell, isTrail, other) in cells)
            {
                var ownName = names[cell];
                var otherAt = FindCell(names, cell, other);
                if (!Patch(ownName, cell, out var own) || !Patch(other, otherAt, out var far)) continue;
                var trailPatch = isTrail ? own : far;
                var woodPatch = isTrail ? far : own;
                if (trailPatch.w != woodPatch.w || trailPatch.h != woodPatch.h) continue;
                var w = own.w;
                var h = own.h;
                var pixels = new Color32[w * h];
                var any = false;
                for (var py = 0; py < h; py++)
                    for (var px = 0; px < w; px++)
                    {
                        var cov = Coverage(Trail, cell.x + (px + 0.5f) / w, cell.y + (py + 0.5f) / h, seed);
                        var i = py * w + px;
                        if (cov >= Threshold)
                        {
                            if (isTrail && cov >= Threshold + RimWidth) continue;               // trail on trail: the tile shows
                            var c = trailPatch.pixels[i];
                            if (cov < Threshold + RimWidth) c = Worn(c);
                            pixels[i] = c;
                        }
                        else
                        {
                            if (!isTrail) continue;                                              // forest on forest
                            pixels[i] = woodPatch.pixels[i];
                        }
                        any = true;
                    }
                if (!any) continue;
                var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "trail_edge" };
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                var sprite = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.name = "trail_edge";
                _made.Add(texture); _made.Add(sprite); _made.Add(tile);
                Layer.SetTile(cell, tile);
                placed++;
            }
            if (placed == 0) { Destroy(go); Layer = null; return 0; }

            renderer.sortingOrder = -1;                                        // over the ground, under the tufts and flowers (order 0) and the soil
            ground.GetComponent<TilemapRenderer>().sortingOrder = -2;
            return placed;
        }

        void OnDestroy()
        {
            foreach (var o in _made) if (o != null) Destroy(o);
            _made.Clear();
        }

        static Vector3Int FindCell(Dictionary<Vector3Int, string> names, Vector3Int around, string name)
        {
            for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++)
                {
                    var c = new Vector3Int(around.x + dx, around.y + dy, 0);
                    if (names.TryGetValue(c, out var n) && n == name) return c;
                }
            return around;
        }

        // A slightly darker, browner version of a trail pixel: the worn rim.
        static Color32 Worn(Color32 c) => new Color32((byte)(c.r * 0.86f), (byte)(c.g * 0.84f), (byte)(c.b * 0.80f), c.a);

        // The pixels of a sprite, as they are in its picture (the texture may not be readable, so it is drawn into a temporary one).
        static (Color32[] pixels, int w, int h) ReadPixels(Sprite sprite)
        {
            var source = sprite.texture;
            var rect = sprite.textureRect;
            var w = Mathf.RoundToInt(rect.width);
            var h = Mathf.RoundToInt(rect.height);
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;
                var copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(rect.x, rect.y, w, h), 0, 0);
                copy.Apply(false);
                var pixels = copy.GetPixels32();
                Destroy(copy);
                return (pixels, w, h);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
