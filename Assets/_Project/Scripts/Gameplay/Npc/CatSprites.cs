using Farm.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The black cat's pictures, drawn from small pixel grids: two walking frames facing down, up and left (right is the left mirrored).
    // b body, h highlight, e eye, n nose, '.' nothing. Project-made.
    public static class CatSprites
    {
        public const string Down = "down", Up = "up", Left = "left", Right = "right";

        static readonly string[] SideA =
        {
            "................", "................", "................", "................", "................", "................",
            "..............b.", "..b.b.........b.", ".bbbbb.......bb.", ".ebbbb.bhhhhbb..", ".nbbbbbbbbbbbb..", "..bbbbbbbbbbbb..",
            "...bbbbbbbbbb...", "...b.b....b.b...", "...b.b....b.b...", "................",
        };
        static readonly string[] SideB =
        {
            "................", "................", "................", "................", "................", "................",
            "...............b", "..b.b.........b.", ".bbbbb.......bb.", ".ebbbb.bhhhhbb..", ".nbbbbbbbbbbbb..", "..bbbbbbbbbbbb..",
            "...bbbbbbbbbb...", "....b..bb..b....", "....b..bb..b....", "................",
        };
        static readonly string[] FrontA =
        {
            "................", "................", "................", "................", "................",
            "....b......b....", "....bb....bb....", "....bbbbbbbb....", "....bebbbbeb....", "....bbbnnbbb....", ".....bbbbbbb....",
            "....bbbbbbbbb...", "....bbbbbbbbb...", "....bhbbbbhb....", "....bb.bb.bb....", "................",
        };
        static readonly string[] FrontB =
        {
            "................", "................", "................", "................", "................",
            "....b......b....", "....bb....bb....", "....bbbbbbbb....", "....bebbbbeb....", "....bbbnnbbb....", ".....bbbbbbb....",
            "....bbbbbbbbb...", "....bbbbbbbbb...", "....bhbbbbhb....", "....b.bbbb.b....", "................",
        };
        static readonly string[] BackA =
        {
            "................", "................", "................", "................", "................",
            "....b......b....", "....bb....bb....", "....bbbbbbbb....", "....bbbbbbbb....", "....bbbbbbbbb...", ".....bbbbbbb.b..",
            "....bbbbbbbbb.b.", "....bbhhhhbbb.b.", "....bbbbbbbbb.b.", "....bb.bb.bb....", "................",
        };
        static readonly string[] BackB =
        {
            "................", "................", "................", "................", "................",
            "....b......b....", "....bb....bb....", "....bbbbbbbb....", "....bbbbbbbb....", "....bbbbbbbbb...", ".....bbbbbbb.b..",
            "....bbbbbbbbb.b.", "....bbhhhhbbb.b.", "....bbbbbbbbb.b.", "....b.bbbb.b....", "................",
        };

        static readonly Color32 Body = new Color32(0x1e, 0x1e, 0x26, 255), Highlight = new Color32(0x3c, 0x3c, 0x4a, 255),
            Eye = new Color32(0xf0, 0xcf, 0x3a, 255), Nose = new Color32(0xd9, 0x8a, 0xa8, 255);

        static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

        static CatSprites() => TestResets.Add(ClearCache);
        public static void ClearCache() => Cache.Clear();

        // The two walking frames for a facing ("down", "up", "left", "right").
        public static Sprite[] Frames(string facing)
        {
            if (Cache.TryGetValue(facing, out var cached) && cached[0] != null) return cached;
            string[] a, b; var mirror = false;
            switch (facing)
            {
                case Up: a = BackA; b = BackB; break;
                case Left: a = SideA; b = SideB; break;
                case Right: a = SideA; b = SideB; mirror = true; break;
                default: a = FrontA; b = FrontB; break;
            }
            var frames = new[] { Make(a, mirror, facing + "0"), Make(b, mirror, facing + "1") };
            Cache[facing] = frames;
            return frames;
        }

        // Top row of the grid is the top of the picture; the texture is bottom-up.
        public static Color32[] Pixels(string[] grid, bool mirror)
        {
            var pixels = new Color32[16 * 16];
            for (var row = 0; row < 16; row++)
                for (var col = 0; col < 16; col++)
                {
                    var c = grid[row][mirror ? 15 - col : col];
                    pixels[(15 - row) * 16 + col] = c == 'b' ? Body : c == 'h' ? Highlight : c == 'e' ? Eye : c == 'n' ? Nose : new Color32(0, 0, 0, 0);
                }
            return pixels;
        }

        static Sprite Make(string[] grid, bool mirror, string name)
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "cat_" + name };
            tex.SetPixels32(Pixels(grid, mirror));
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = "cat_" + name;
            return sprite;
        }

        public static IEnumerable<string[]> AllGrids() { yield return SideA; yield return SideB; yield return FrontA; yield return FrontB; yield return BackA; yield return BackB; }
    }
}
