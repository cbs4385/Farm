using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The avatar's layers (Resources/Avatar/avatar_layers.txt, made by tools/art/build_avatar_layers.py) and the rules for stacking and colouring
    // them. Each layer is a 16 x 32 grid of cell characters per facing; right is the mirror of left. Cell characters: s skin, h hair, t shirt,
    // p pants, a accessory, w white cloth, k dark, b boots, . empty; an upper-case letter is the same colour in shade.
    public static class AvatarLayers
    {
        public const int Width = 16, Height = 32;

        static Dictionary<string, string[]> _grids;

        public static IReadOnlyDictionary<string, string[]> Grids => _grids ?? (_grids = Load());

        static Dictionary<string, string[]> Load()
        {
            var asset = Resources.Load<TextAsset>("Avatar/avatar_layers");
            if (asset == null) { Farm.Core.Log.Error("Avatar layers are missing (Resources/Avatar/avatar_layers)."); return new Dictionary<string, string[]>(); }
            return Parse(asset.text);
        }

        // "@name" then 32 rows of 16 characters; blank lines and # comments are ignored.
        public static Dictionary<string, string[]> Parse(string text)
        {
            var result = new Dictionary<string, string[]>();
            string name = null;
            var rows = new List<string>();
            void Flush()
            {
                if (name == null) return;
                if (rows.Count != Height) throw new FormatException($"avatar layer '{name}' has {rows.Count} rows, not {Height}");
                result[name] = rows.ToArray();
            }
            foreach (var raw in text.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0) continue;
                if (line[0] == '@') { Flush(); name = line.Substring(1).Trim(); rows.Clear(); continue; }
                if (line.Length != Width) throw new FormatException($"avatar layer '{name}': a row has {line.Length} cells, not {Width}");
                rows.Add(line);
            }
            Flush();
            return result;
        }

        public static void ResetForTests() => _grids = null;
    }

    public static class AvatarComposer
    {
        public const string DownFacing = "down", UpFacing = "up", LeftFacing = "left", RightFacing = "right";
        public static readonly string[] Facings = { DownFacing, UpFacing, LeftFacing, RightFacing };

        static readonly Color32 Outline = new Color32(0x2b, 0x1d, 0x16, 255);
        static readonly Color32 Dark = new Color32(0x2b, 0x1d, 0x16, 255);
        static readonly Color32 Boots = new Color32(0x5a, 0x3b, 0x28, 255);
        static readonly Color32 Cloth = new Color32(0xef, 0xe6, 0xd8, 255);
        const float ShadeFactor = 0.72f;

        public static Color32 ParseColor(string hex, Color32 fallback)
        {
            if (!AvatarOptions.IsHex(hex)) return fallback;
            return new Color32(Convert.ToByte(hex.Substring(1, 2), 16), Convert.ToByte(hex.Substring(3, 2), 16), Convert.ToByte(hex.Substring(5, 2), 16), 255);
        }

        static Color32 Shade(Color32 c) => new Color32((byte)(c.r * ShadeFactor), (byte)(c.g * ShadeFactor), (byte)(c.b * ShadeFactor), 255);

        // The layers that make up a look, back to front, for the grid facing ("right" uses the left grids, mirrored).
        public static IEnumerable<string> LayerNames(AvatarData a, string gridFacing)
        {
            yield return $"body.{a.Build}.{gridFacing}";
            yield return $"pants.{a.Pants}.{a.Build}.{gridFacing}";
            yield return $"shirt.{a.Shirt}.{a.Build}.{gridFacing}";
            yield return $"hair.{a.Hair}.{gridFacing}";
            yield return $"accessory.{a.Accessory}.{gridFacing}";
        }

        // 16 x 32 pixels, top row first, transparent where there is nothing. The silhouette gets a one-pixel dark outline.
        public static Color32[] Compose(AvatarData look, string facing, IReadOnlyDictionary<string, string[]> grids = null)
        {
            var a = AvatarOptions.Sanitize(look);
            grids = grids ?? AvatarLayers.Grids;
            var w = AvatarLayers.Width; var h = AvatarLayers.Height;
            var mirror = facing == RightFacing;
            var gridFacing = mirror ? LeftFacing : facing;

            var colours = new Dictionary<char, Color32>
            {
                ['s'] = ParseColor(a.Skin, Boots), ['h'] = ParseColor(a.HairColor, Boots), ['t'] = ParseColor(a.ShirtColor, Boots),
                ['p'] = ParseColor(a.PantsColor, Boots), ['a'] = ParseColor(a.AccessoryColor, Boots), ['w'] = Cloth, ['k'] = Dark, ['b'] = Boots,
            };
            var pixels = new Color32[w * h];
            var filled = new bool[w * h];
            foreach (var name in LayerNames(a, gridFacing))
            {
                if (!grids.TryGetValue(name, out var rows)) continue;
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                    {
                        var c = rows[y][mirror ? w - 1 - x : x];
                        if (c == '.') continue;
                        if (!colours.TryGetValue(char.ToLowerInvariant(c), out var colour)) continue;
                        pixels[y * w + x] = char.IsUpper(c) ? Shade(colour) : colour;
                        filled[y * w + x] = true;
                    }
            }

            // The outline: any empty pixel that touches the figure above, below or beside it.
            var result = (Color32[])pixels.Clone();
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    if (filled[y * w + x]) continue;
                    var touches = (x > 0 && filled[y * w + x - 1]) || (x < w - 1 && filled[y * w + x + 1]) || (y > 0 && filled[(y - 1) * w + x]) || (y < h - 1 && filled[(y + 1) * w + x]);
                    if (touches) result[y * w + x] = Outline;
                }
            return result;
        }

        public static int OpaqueCount(Color32[] pixels)
        {
            var n = 0;
            foreach (var p in pixels) if (p.a > 0) n++;
            return n;
        }
    }
}
