using UnityEngine;

namespace Farm.Gameplay
{
    // What makes a flat character picture read as a body in a world (playtest 2026-10-10: "the avatars still look flat"): a dark outline round the whole figure, a lit
    // edge on the sides facing the light (top and left) and a shaded edge on the sides facing away. Pure: pixels in, pixels out. Applied to every frame of a character.
    public static class CharacterLook
    {
        static readonly Color32 OutlineInk = new Color32(0x2b, 0x1d, 0x16, 255);

        public static float Luma(Color32 c) => (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;

        static Color32 Mix(Color32 a, Color32 b, float t) => new Color32(
            (byte)Mathf.RoundToInt(Mathf.Lerp(a.r, b.r, t)), (byte)Mathf.RoundToInt(Mathf.Lerp(a.g, b.g, t)), (byte)Mathf.RoundToInt(Mathf.Lerp(a.b, b.b, t)), 255);

        public static PixelGrid Apply(PixelGrid src)
        {
            var o = src.Clone();
            // the lowest third of the figure sits a little in its own shade, like something standing on the ground
            var (top, bottom) = src.Rows();
            var shadeFrom = top < 0 ? int.MaxValue : bottom - (bottom - top) / 3;
            for (var y = Mathf.Max(0, shadeFrom); y <= bottom; y++)
                for (var x = 0; x < src.W; x++)
                {
                    var c = src.Get(x, y);
                    if (c.a > 0 && Luma(c) >= 0.18f) o.Set(x, y, Mix(c, new Color32(20, 12, 28, 255), 0.07f + 0.05f * (y - shadeFrom) / Mathf.Max(1, bottom - shadeFrom)));
                }
            for (var y = 0; y < src.H; y++)
                for (var x = 0; x < src.W; x++)
                {
                    var c = src.Get(x, y);
                    if (c.a == 0) continue;
                    // light from the top left: the lit edge is brighter, the far edge darker (never on the darkest inks, which are outlines already)
                    if (Luma(c) < 0.18f) continue;
                    var lit = !src.Opaque(x, y - 1) || !src.Opaque(x - 1, y);
                    var shade = !src.Opaque(x, y + 1) || !src.Opaque(x + 1, y);
                    if (lit && !shade) o.Set(x, y, Mix(c, new Color32(255, 250, 235, 255), 0.16f));
                    else if (shade && !lit) o.Set(x, y, Mix(c, new Color32(20, 12, 20, 255), 0.18f));
                }
            for (var y = 0; y < src.H; y++)
                for (var x = 0; x < src.W; x++)
                {
                    if (src.Opaque(x, y)) continue;
                    Color32 edge = default; var touches = false;
                    foreach (var (dx, dy) in new[] { (0, -1), (-1, 0), (1, 0), (0, 1) })
                        if (src.Opaque(x + dx, y + dy)) { edge = src.Get(x + dx, y + dy); touches = true; break; }
                    if (touches) o.Set(x, y, Mix(Mix(edge, OutlineInk, 0.75f), OutlineInk, 0.5f));
                }
            return o;
        }

        // A soft oval of shade for under a character's feet: `width` pixels wide, a third as tall.
        public static PixelGrid Shadow(int width)
        {
            var height = Mathf.Max(3, width / 3);
            var g = new PixelGrid(width, height);
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var dx = (x + 0.5f - width / 2f) / (width / 2f);
                    var dy = (y + 0.5f - height / 2f) / (height / 2f);
                    var d = dx * dx + dy * dy;
                    if (d <= 1f) g.Set(x, y, new Color32(20, 14, 30, (byte)(d < 0.55f ? 130 : 84)));
                }
            return g;
        }
    }
}
