using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The monsters' pictures, drawn from small pixel grids like the village cat's: two frames each (they alternate while it moves), and a white
    // silhouette that flashes when it is hit. Each grid row is the LEFT HALF of the picture; the right half is its mirror. The picture is bottom
    // aligned. Letters are colours from the monster's own palette, '.' is nothing. Project-made.
    public static class EnemySprites
    {
        public const float FrameSeconds = 0.28f;

        public sealed class Art
        {
            public int Size;
            public Dictionary<char, Color32> Palette;
            public string[][] Frames;
        }

        static Color32 C(int r, int g, int b) => new Color32((byte)r, (byte)g, (byte)b, 255);

        public static readonly Dictionary<string, Art> All = new Dictionary<string, Art>
        {
            ["slime"] = new Art
            {
                Size = 16,
                Palette = new Dictionary<char, Color32> { ['d'] = C(40, 110, 50), ['g'] = C(90, 200, 100), ['G'] = C(170, 240, 170), ['m'] = C(60, 150, 70), ['w'] = C(255, 255, 255), ['k'] = C(20, 30, 20) },
                Frames = new[]
                {
                    new[] { "......dd", "....ddgg", "...dgGGg", "..dgGggg", "..dgwkgg", ".dgggggg", ".dgggggg", ".dmmmmmm", "..dddddd" },
                    new[] { "........", "........", "..dddddd", ".dgGGggg", ".dgwkggg", "dggggggg", "dggggggg", "dmmmmmmm", "dddddddd" },
                },
            },
            ["cave_bat"] = new Art
            {
                Size = 16,
                Palette = new Dictionary<char, Color32> { ['d'] = C(35, 25, 50), ['p'] = C(120, 95, 145), ['P'] = C(170, 140, 195), ['e'] = C(250, 225, 80), ['f'] = C(255, 255, 255) },
                Frames = new[]
                {
                    new[] { "dd....d.", "dPd..dpd", "dPPddppp", "dPPPdpep", ".dPPdppp", "..dddppf", "....dppp", ".....dpp", "......dd" },
                    new[] { "........", "......d.", ".....dpd", "...ddppp", "..dPdpep", ".dPPdppp", "dPPPdppf", "dPPd.dpp", "ddd...dd" },
                },
            },
            ["rock_crab"] = new Art
            {
                Size = 16,
                Palette = new Dictionary<char, Color32> { ['d'] = C(70, 55, 45), ['c'] = C(160, 135, 115), ['C'] = C(205, 180, 155), ['s'] = C(115, 95, 85), ['r'] = C(210, 105, 80), ['R'] = C(240, 150, 110), ['e'] = C(15, 15, 20), ['w'] = C(255, 255, 255) },
                Frames = new[]
                {
                    new[] { "....we..", "rr..de..", "rRr.dddd", "rRdddccC", ".drdccCC", "..dccccc", "..dcsccs", "..dcccss", "...dcccc", "..d.dd.d", "........" },
                    new[] { "........", "....we..", "rr..de..", "rRr.dddd", "rRdddccC", ".drdccCC", "..dccccc", "..dcsccs", "...dcccs", "..d.dd.d", "........" },
                },
            },
            ["bone_walker"] = new Art
            {
                Size = 16,
                Palette = new Dictionary<char, Color32> { ['d'] = C(60, 55, 50), ['b'] = C(238, 232, 212), ['s'] = C(172, 166, 150), ['k'] = C(25, 25, 30), ['r'] = C(215, 70, 50) },
                Frames = new[]
                {
                    new[] { "....dddd", "...dbbbb", "...dbkkb", "...dbbsb", "....dbbb", "....dbsb", "...dddbb", "..dbbdbb", "..dbbdbs", "..dbbdbb", "...dbddb", "...db.db", "..ddb.dd" },
                    new[] { "....dddd", "...dbbbb", "...dbkkb", "...dbbsb", "....dbbb", "....dbsb", "...dddbb", "..dbbdbb", "..dbbdbs", "..dbbdbb", "....dddb", "...dbd.b", "...ddb.d" },
                },
            },
            ["ember_imp"] = new Art
            {
                Size = 16,
                Palette = new Dictionary<char, Color32> { ['d'] = C(105, 30, 15), ['o'] = C(238, 105, 40), ['O'] = C(255, 165, 55), ['y'] = C(255, 238, 110), ['h'] = C(75, 25, 20), ['f'] = C(255, 215, 70), ['k'] = C(40, 10, 10) },
                Frames = new[]
                {
                    new[] { "..h.....", "..hd....", "..hdddd.", "..dooooo", ".doyyooo", ".doyOooo", ".dooooOO", "..dddooo", "...dooOO", "..dodooO", "..dod.oo", "...dd.do", "..dd..dd" },
                    new[] { "..h.....", "..hd....", "..hdddd.", "..dooooo", ".doyyooo", ".doyOooo", ".dooooOO", "..dddooo", "...dooOO", ".dodoooO", ".dod..oo", "...dd.do", "..dd...d" },
                },
            },
            ["cavern_warden"] = new Art
            {
                Size = 20,
                Palette = new Dictionary<char, Color32> { ['d'] = C(40, 20, 60), ['p'] = C(125, 50, 150), ['P'] = C(180, 100, 205), ['s'] = C(110, 105, 120), ['S'] = C(150, 145, 160), ['g'] = C(245, 205, 60), ['k'] = C(20, 10, 30) },
                Frames = new[]
                {
                    new[] { "...dd.....", "..dsSd....", "..dsSddddd", "...dSSSSSS", "...dSggSSS", "...dSggkSS", "...dSSSSSS", "....dssSSS", ".dddddsSSS", "dpPPddsSss", "dpPPppdddd", "dpPPpppPPp", ".dpppppPPp", "..dddppppp", "...dpppppp", "...dpppddd", "...dppd...", "...dsSd...", "..dsSSd...", "..dddddd.." },
                    new[] { "...dd.....", "..dsSd....", "..dsSddddd", "...dSSSSSS", "...dSggSSS", "...dSggkSS", "...dSSSSSS", "....dssSSS", ".dddddsSSS", "dpPPddsSss", "dpPPppdddd", "dpPPpppPPp", ".dpppppPPp", "..dddppppp", "...dpppppp", "...dpppddd", "....dpd...", "...dsSd...", "..dsSSd...", "..dddddd.." },
                },
            },
        };

        static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        static Sprite _pixel;

        public static bool Has(string enemyId) => All.ContainsKey(enemyId);

        public static void ClearCache() { Cache.Clear(); _pixel = null; }

        // Frames 0 and 1, then the white silhouette, for a monster; null when there is no art for it.
        public static Sprite[] Frames(string enemyId)
        {
            if (!All.TryGetValue(enemyId, out var art)) return null;
            if (Cache.TryGetValue(enemyId, out var cached) && cached[0] != null) return cached;
            var sprites = new[] { Make(art, 0, false, enemyId), Make(art, 1, false, enemyId), Make(art, 0, true, enemyId) };
            Cache[enemyId] = sprites;
            return sprites;
        }

        // Which of the two frames is showing `seconds` into the game (pure). `offset` keeps a crowd out of step.
        public static int FrameAt(float seconds, float offset) => (int)((seconds + offset) / FrameSeconds) % 2;

        // The picture as pixels, top row first (pure, so it can be tested). `silhouette` makes every drawn pixel white.
        public static Color32[] Pixels(Art art, int frame, bool silhouette)
        {
            var size = art.Size; var half = size / 2;
            var rows = art.Frames[frame];
            var pixels = new Color32[size * size];
            var top = size - rows.Length - (size == 16 ? 1 : 0);          // sits on the bottom edge (a row clear below the small ones)
            for (var r = 0; r < rows.Length; r++)
                for (var x = 0; x < size; x++)
                {
                    var c = x < half ? rows[r][x] : rows[r][size - 1 - x];
                    if (c == '.') continue;
                    pixels[(top + r) * size + x] = silhouette ? new Color32(255, 255, 255, 255) : art.Palette[c];
                }
            return pixels;
        }

        static Sprite Make(Art art, int frame, bool silhouette, string name)
        {
            var size = art.Size;
            var pixels = Pixels(art, frame, silhouette);
            var flipped = new Color32[size * size];
            for (var y = 0; y < size; y++)                                 // a texture starts at the bottom row
                for (var x = 0; x < size; x++) flipped[(size - 1 - y) * size + x] = pixels[y * size + x];
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "enemy_" + name };
            texture.SetPixels32(flipped);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = "enemy_" + name + (silhouette ? "_flash" : "_" + frame);
            return sprite;
        }

        // A single white pixel, stretched for health bars.
        public static Sprite Pixel()
        {
            if (_pixel != null) return _pixel;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "enemy_pixel" };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
            return _pixel;
        }
    }
}
