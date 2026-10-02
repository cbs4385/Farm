using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Flat-colour pixel sprites made at run time, for things that are placeholders without art assets yet (stairs, monsters).
    // Final art replaces them by naming real sprites on the definitions.
    public static class RuntimeSprites
    {
        static readonly Dictionary<Color, Sprite> Cache = new Dictionary<Color, Sprite>();

        public static Sprite Square(Color color, int size = 16, bool character = false)
        {
            var key = new Color(color.r, color.g, color.b, character ? 0.5f : 1f);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var outline = new Color(0.08f, 0.08f, 0.1f, 1f);
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var edge = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                    var inside = !character || (x > 2 && x < size - 3 && y > 1 && y < size - 2);
                    tex.SetPixel(x, y, !inside ? Color.clear : edge ? outline : color);
                }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            Cache[key] = sprite;
            return sprite;
        }
    }
}
