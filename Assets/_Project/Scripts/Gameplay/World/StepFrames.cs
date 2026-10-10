using Farm.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // A walk cycle for any standing picture, made from the picture itself: two copies in which one foot (then the other) is lifted a pixel.
    // Played in turn with the body's own hop (WalkBob) a walker steps left, passes, steps right, passes. No extra art is needed, so it works for
    // the farmer in any outfit and for every villager, in every direction (in a side view the two halves are the near and far foot).
    public static class StepFrames
    {
        public const int FootRows = 3;                      // how many rows at the bottom of the picture are the foot

        static readonly Dictionary<Sprite, Sprite[]> Cache = new Dictionary<Sprite, Sprite[]>();
        static readonly Dictionary<Sprite, Sprite> OriginalOfFrame = new Dictionary<Sprite, Sprite>();

        static StepFrames() => TestResets.Add(ClearCache);
        public static void ClearCache() { Cache.Clear(); OriginalOfFrame.Clear(); }

        // Pixels in texture order (row 0 is the bottom). Lifts the foot under one half of the picture by a pixel: that half's bottom rows move
        // up and the lowest row is cleared, so the leg is a pixel shorter. (pure)
        public static Color32[] Lift(Color32[] pixels, int width, int height, bool leftHalf)
        {
            var result = (Color32[])pixels.Clone();
            var floor = -1;
            for (var y = 0; y < height && floor < 0; y++)
                for (var x = 0; x < width; x++)
                    if (pixels[y * width + x].a > 0) { floor = y; break; }
            if (floor < 0) return result;

            var from = leftHalf ? 0 : width / 2;
            var to = leftHalf ? width / 2 : width;
            for (var x = from; x < to; x++)
            {
                for (var y = floor; y < floor + FootRows && y + 1 < height; y++)
                {
                    var p = pixels[y * width + x];
                    if (p.a > 0) result[(y + 1) * width + x] = p;
                }
                if (pixels[floor * width + x].a > 0) result[floor * width + x] = new Color32(0, 0, 0, 0);
            }
            return result;
        }

        // [left foot lifted, right foot lifted], or null when the picture cannot be read back.
        public static Sprite[] For(Sprite sprite)
        {
            if (sprite == null) return null;
            if (OriginalOfFrame.ContainsKey(sprite)) return For(OriginalOfFrame[sprite]);
            if (Cache.TryGetValue(sprite, out var cached)) return cached != null && cached[0] != null ? cached : null;

            Sprite[] frames = null;
            if (Read(sprite, out var pixels, out var w, out var h))
            {
                frames = new[] { Make(sprite, Lift(pixels, w, h, true), w, h, "_stepL"), Make(sprite, Lift(pixels, w, h, false), w, h, "_stepR") };
                foreach (var f in frames) OriginalOfFrame[f] = sprite;
            }
            Cache[sprite] = frames;
            return frames;
        }

        // The picture a step frame was made from (so WalkBob can tell a frame it set from one the game set).
        public static bool TryGetOriginal(Sprite frame, out Sprite original) => OriginalOfFrame.TryGetValue(frame, out original);

        static Sprite Make(Sprite source, Color32[] pixels, int w, int h, string suffix)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = source.name + suffix };
            texture.SetPixels32(pixels);
            texture.Apply();
            var pivot = new Vector2(source.pivot.x / w, source.pivot.y / h);
            var sprite = Sprite.Create(texture, new Rect(0, 0, w, h), pivot, source.pixelsPerUnit);
            sprite.name = source.name + suffix;
            return sprite;
        }

        // The sprite's pixels, even when it is packed in an atlas that cannot be read on the CPU: the part of the texture is drawn into a
        // render texture of its own size and read back.
        static bool Read(Sprite sprite, out Color32[] pixels, out int w, out int h)
        {
            pixels = null;
            var rect = sprite.textureRect;
            w = (int)rect.width; h = (int)rect.height;
            var tex = sprite.texture;
            if (tex == null || w <= 0 || h <= 0) return false;
            try
            {
                if (tex.isReadable)
                {
                    var block = tex.GetPixels((int)rect.x, (int)rect.y, w, h);
                    pixels = new Color32[block.Length];
                    for (var i = 0; i < block.Length; i++) pixels[i] = block[i];
                    return true;
                }
                var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                var previous = RenderTexture.active;
                var scale = new Vector2(w / (float)tex.width, h / (float)tex.height);
                var offset = new Vector2(rect.x / tex.width, rect.y / tex.height);
                Graphics.Blit(tex, rt, scale, offset);
                RenderTexture.active = rt;
                var copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                pixels = copy.GetPixels32();
                Object.Destroy(copy);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[StepFrames] could not read {sprite.name}: {e.Message}");
                return false;
            }
        }
    }
}
