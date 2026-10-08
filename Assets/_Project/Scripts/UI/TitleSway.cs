using UnityEngine;

namespace Farm.UI
{
    // The breeze over the title picture, as pure functions. Leaves, branches, crops and grass move; the sky, the buildings and the water of the
    // far hills hold still. The picture is drawn as a grid of points (TitleBackdrop) and each point is pushed a tiny way by Offset, scaled by how
    // much of a plant it sits on (Weight, worked out once from the picture's own colours).
    public static class TitleSway
    {
        public const float AmplitudeX = 0.0016f;        // of the picture's width: about three pixels at full HD
        public const float AmplitudeY = 0.0009f;        // of its height
        public const float GustPeriod = 9f;             // seconds between the stronger puffs

        // How far the point at (u, v) (0..1 across and up the picture) is moved at `time` seconds, in fractions of the picture's width and height.
        // `weight` is 0 for what must stay still and 1 for the most swaying foliage. Always small, smooth in time, and zero for a weight of zero.
        public static Vector2 Offset(float u, float v, float time, float weight)
        {
            if (weight <= 0f) return Vector2.zero;
            var gust = 0.65f + 0.35f * Mathf.Sin(time * (2f * Mathf.PI / GustPeriod));
            var wave = 0.6f * Mathf.Sin(time * 1.9f - u * 9f - v * 3f) + 0.4f * Mathf.Sin(time * 3.1f - u * 17f + v * 5f + 1.3f);
            var lift = Mathf.Sin(time * 2.3f - u * 11f + v * 2f);
            return new Vector2(wave * gust * AmplitudeX * weight, lift * gust * AmplitudeY * weight);
        }

        // How much a spot of the picture sways, from its colour (a small average of the pixels there) and its place (u across, vTop down from the top):
        // 1 for leaves and grass (greens, yellows, oranges, dark pine), 0 for sky, water, stone, wood and lit windows.
        public static float Weight(Color colour, float u, float vTop)
        {
            Color.RGBToHSV(colour, out var h, out var s, out var value);
            if (s < 0.28f || value < 0.14f) return 0f;                          // grey stone, shadow, bare ground
            var warm = h >= 0.045f && h <= 0.20f;                              // oranges and yellows (autumn leaves, crops)
            var green = h > 0.20f && h <= 0.52f;                               // greens and the dark teal of the pines
            if (!warm && !green) return 0f;                                    // reds (the barn), blues, purples (sky and water)
            var amount = green ? 1f : 0.85f;
            if (warm && value > 0.80f && vTop < 0.45f) return 0f;               // the bright orange glow of the sunset sky
            return amount * Edge(0.10f, 0.28f, vTop);               // the top of the picture is sky: fade out above the tree line
        }

        // 0 below `from`, 1 above `to`, smooth in between.
        static float Edge(float from, float to, float x)
        {
            var t = Mathf.Clamp01((x - from) / (to - from));
            return t * t * (3f - 2f * t);
        }

        // The weights of a (columns + 1) x (rows + 1) grid of points over `pixels` (row 0 = the bottom, like Texture2D.GetPixels32), smoothed so that
        // neighbouring points move together. Returned row by row from the bottom.
        public static float[] Weights(Color32[] pixels, int width, int height, int columns, int rows)
        {
            var w = new float[(columns + 1) * (rows + 1)];
            for (var j = 0; j <= rows; j++)
                for (var i = 0; i <= columns; i++)
                {
                    var u = i / (float)columns; var v = j / (float)rows;
                    var cx = Mathf.Clamp(Mathf.RoundToInt(u * (width - 1)), 0, width - 1);
                    var cy = Mathf.Clamp(Mathf.RoundToInt(v * (height - 1)), 0, height - 1);
                    float r = 0f, g = 0f, b = 0f; var n = 0;
                    for (var dy = -4; dy <= 4; dy += 2)
                        for (var dx = -4; dx <= 4; dx += 2)
                        {
                            var p = pixels[Mathf.Clamp(cy + dy, 0, height - 1) * width + Mathf.Clamp(cx + dx, 0, width - 1)];
                            r += p.r; g += p.g; b += p.b; n++;
                        }
                    w[j * (columns + 1) + i] = Weight(new Color(r / n / 255f, g / n / 255f, b / n / 255f), u, 1f - v);
                }
            Blur(w, columns + 1, rows + 1);
            Blur(w, columns + 1, rows + 1);
            // The edge of the picture never moves, so no gap opens at the screen's edge.
            for (var j = 0; j <= rows; j++)
                for (var i = 0; i <= columns; i++)
                {
                    var edge = Mathf.Min(Mathf.Min(i, columns - i), Mathf.Min(j, rows - j));
                    if (edge < 2) w[j * (columns + 1) + i] *= edge / 2f;
                }
            return w;
        }

        static void Blur(float[] w, int cols, int rows)
        {
            var copy = (float[])w.Clone();
            for (var j = 0; j < rows; j++)
                for (var i = 0; i < cols; i++)
                {
                    float sum = 0f; var n = 0;
                    for (var dj = -1; dj <= 1; dj++)
                        for (var di = -1; di <= 1; di++)
                        {
                            var x = i + di; var y = j + dj;
                            if (x < 0 || y < 0 || x >= cols || y >= rows) continue;
                            sum += copy[y * cols + x]; n++;
                        }
                    w[j * cols + i] = sum / n;
                }
        }
    }
}
