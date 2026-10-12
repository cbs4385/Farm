using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // How a farm building looks before the carpenter has built it: faded and greyed, with boards nailed across its door, so that it reads as needing repair
    // (playtest 2026-10-12: the barn, coop and greenhouse looked finished from the first day).
    public static class DerelictLook
    {
        public static readonly Color Tint = new Color(0.64f, 0.60f, 0.55f, 1f);

        static Sprite _boards;

        static DerelictLook() => TestResets.Add(() => _boards = null);

        // Is this building still waiting for the carpenter? (pure)
        public static bool IsDerelict(FarmBuildingType type, bool built) => type != null && !string.IsNullOrEmpty(type.UnlockFlag) && !built;

        // Boards across a door: one cell wide and two tall, its foot at the bottom centre.
        public static Sprite Boards()
        {
            if (_boards != null) return _boards;
            const int w = 16, h = 32;
            var pixels = new Color32[w * h];
            var dark = new Color32(70, 46, 28, 255);
            var light = new Color32(118, 84, 52, 255);
            var nail = new Color32(190, 190, 185, 255);

            void Plank(float ax, float ay, float bx, float by, float half)
            {
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                    {
                        var d = DistanceToSegment(x + 0.5f, y + 0.5f, ax, ay, bx, by);
                        if (d > half) continue;
                        pixels[y * w + x] = d > half - 1f ? dark : light;
                    }
            }

            Plank(1f, 3f, 15f, 27f, 1.9f);
            Plank(15f, 3f, 1f, 27f, 1.9f);
            Plank(0f, 15f, 16f, 15f, 1.9f);
            pixels[15 * w + 2] = nail;
            pixels[15 * w + 8] = nail;
            pixels[15 * w + 13] = nail;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "derelict_boards" };
            texture.SetPixels32(pixels);
            texture.Apply();
            _boards = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
            return _boards;
        }

        static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var t = Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy));
            var cx = ax + t * dx - px;
            var cy = ay + t * dy - py;
            return Mathf.Sqrt(cx * cx + cy * cy);
        }
    }
}
