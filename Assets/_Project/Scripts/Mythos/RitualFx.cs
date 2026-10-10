using System.Collections.Generic;
using UnityEngine;

namespace Farm.Mythos
{
    // The little pictures of a ritual night, made at run time: candle, flame, glow, ember. Pixel shapes, point filtered, like the rest of the art.
    public static class RitualFx
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        static RitualFx() => Farm.Core.TestResets.Add(() => Cache.Clear());

        static Sprite Make(string key, int w, int h, System.Func<int, int, Color> pixel, Vector2 pivot)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = key };
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++) tex.SetPixel(x, y, pixel(x, y));
            tex.Apply(false, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, 16f);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        // A stub of pale wax, 3 x 4 pixels, its foot on the pivot.
        public static Sprite Candle() => Make("ritual_candle", 3, 5, (x, y) => y < 5 ? (x == 2 ? new Color(0.75f, 0.7f, 0.6f) : new Color(0.93f, 0.9f, 0.8f)) : Color.clear, new Vector2(0.5f, 0f));

        // Three flames, a little different from one another, 3 x 4 pixels.
        public static Sprite Flame(int frame)
        {
            frame = ((frame % 3) + 3) % 3;
            var rows = new[]
            {
                new[] { ".y.", "yoy", "ooy", ".y." },
                new[] { ".y.", ".oy", "yoo", ".y." },
                new[] { "..y", ".yo", "yoy", ".y." },
            }[frame];
            return Make("ritual_flame_" + frame, 3, 4, (x, y) =>
            {
                var c = rows[3 - y][x];
                return c == 'y' ? new Color(1f, 0.92f, 0.5f) : c == 'o' ? new Color(1f, 0.62f, 0.2f) : Color.clear;
            }, new Vector2(0.5f, 0f));
        }

        // A soft round glow, 24 x 24 pixels, white (tinted by the renderer).
        public static Sprite Glow() => Make("ritual_glow", 24, 24, (x, y) =>
        {
            var dx = (x + 0.5f - 12f) / 12f; var dy = (y + 0.5f - 12f) / 12f;
            var d = Mathf.Sqrt(dx * dx + dy * dy);
            return d >= 1f ? Color.clear : new Color(1f, 1f, 1f, Mathf.Pow(1f - d, 2f) * 0.55f);
        }, new Vector2(0.5f, 0.5f));

        // The ring of runes on the ground round the altar: 64 x 40 pixels, white (tinted by the renderer). An oval of tick marks and two thin rings.
        public static Sprite Sigil() => Make("ritual_sigil", 64, 40, (x, y) =>
        {
            var dx = (x + 0.5f - 32f) / 31f; var dy = (y + 0.5f - 20f) / 19f;
            var d = Mathf.Sqrt(dx * dx + dy * dy);
            var angle = Mathf.Atan2(dy, dx);
            var ring = Mathf.Abs(d - 1f) < 0.045f || Mathf.Abs(d - 0.72f) < 0.04f;
            var tick = d > 0.78f && d < 0.94f && Mathf.Abs(Mathf.Repeat(angle / (Mathf.PI * 2f) * 16f, 1f) - 0.5f) < 0.07f;
            return ring || tick ? new Color(1f, 1f, 1f, 0.9f) : Color.clear;
        }, new Vector2(0.5f, 0.5f));

        // A single bright pixel.
        public static Sprite Mote() => Make("ritual_mote", 1, 1, (x, y) => Color.white, new Vector2(0.5f, 0.5f));
    }
}
