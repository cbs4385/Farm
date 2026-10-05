using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The four facing sprites of one look, built from the layers and kept: the same look is only drawn once.
    public sealed class AvatarSpriteSet
    {
        public Sprite Down, Up, Left, Right;
        public Sprite For(string facing) => facing == AvatarComposer.UpFacing ? Up : facing == AvatarComposer.LeftFacing ? Left : facing == AvatarComposer.RightFacing ? Right : Down;
    }

    public static class AvatarSprites
    {
        const float PixelsPerUnit = 16f;       // like every other character sprite
        static readonly Dictionary<string, AvatarSpriteSet> Cache = new Dictionary<string, AvatarSpriteSet>();

        public static AvatarSpriteSet For(AvatarData look)
        {
            var a = AvatarOptions.Sanitize(look);
            if (Cache.TryGetValue(a.Key, out var set) && set.Down != null) return set;
            set = new AvatarSpriteSet
            {
                Down = Make(a, AvatarComposer.DownFacing), Up = Make(a, AvatarComposer.UpFacing),
                Left = Make(a, AvatarComposer.LeftFacing), Right = Make(a, AvatarComposer.RightFacing),
            };
            Cache[a.Key] = set;
            return set;
        }

        // One sprite, feet on the bottom edge of the pivot like the other characters.
        static Sprite Make(AvatarData look, string facing)
        {
            var w = AvatarLayers.Width; var h = AvatarLayers.Height;
            var pixels = AvatarComposer.Compose(look, facing);
            var flipped = new Color32[w * h];
            for (var y = 0; y < h; y++)                         // a texture starts at the bottom row
                for (var x = 0; x < w; x++) flipped[(h - 1 - y) * w + x] = pixels[y * w + x];
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = $"avatar_{facing}" };
            texture.SetPixels32(flipped);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), PixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
        }

        public static void ClearCache() => Cache.Clear();
    }
}
