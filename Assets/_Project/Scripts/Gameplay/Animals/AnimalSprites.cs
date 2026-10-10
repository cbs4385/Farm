using Farm.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The farm animals' sprites, built from AnimalSpriteData (drawn by tools/art/build_animal_sprites.py). Each animal has three views (front, back
    // and side; looking right is the side view mirrored), two walking frames per view, a two-frame munch (front view, head down) and a sleeping
    // pose (side view). Standing still shows the first walking frame.
    public static class AnimalSprites
    {
        public const float WalkFrameSeconds = 0.22f;
        public const float MunchFrameSeconds = 0.3f;
        public const float FidgetFrameSeconds = 0.25f;
        public const int NoIdle = 0, Graze = 1, Fidget = 2;      // what a standing animal does now and then
        public const int FirstSleepHour = 22, WakeHour = 6;

        static readonly Dictionary<string, Dictionary<string, Sprite>> Cache = new Dictionary<string, Dictionary<string, Sprite>>();

        public static bool Has(string type) => AnimalSpriteData.Grids.ContainsKey(type);

        static AnimalSprites() => TestResets.Add(ClearCache);
        public static void ClearCache() => Cache.Clear();

        public static bool IsNight(int hour) => hour >= FirstSleepHour || hour < WakeHour;

        // Which picture to show, and whether it is mirrored (pure, so it can be tested). `facing` is the direction it last moved in.
        public static (string key, bool flip) Pick(Vector2Int facing, bool moving, bool eating, bool asleep, float time, int idle = NoIdle)
        {
            var mirrored = facing.x > 0;
            if (asleep) return ("sleep", mirrored);
            if (eating || (idle == Graze && !moving)) return ("eat" + (int)(time / MunchFrameSeconds) % 2, false);
            if (idle == Fidget && !moving) return ("idle" + (int)(time / FidgetFrameSeconds) % 2, false);
            var frame = moving ? (int)(time / WalkFrameSeconds) % 2 : 0;
            if (facing.x != 0) return ("left" + frame, mirrored);
            return ((facing.y > 0 ? "up" : "down") + frame, false);
        }

        // The sprite for a picture key ("down0", "left1", "eat0", "sleep" ...); null when there is no art for the animal.
        public static Sprite Get(string type, string key)
        {
            if (!AnimalSpriteData.Grids.TryGetValue(type, out var grids) || !grids.TryGetValue(key, out var rows)) return null;
            if (!Cache.TryGetValue(type, out var byKey)) Cache[type] = byKey = new Dictionary<string, Sprite>();
            if (byKey.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = Make(type, key, rows);
            byKey[key] = sprite;
            return sprite;
        }

        // The picture as pixels, top row first (pure).
        public static Color32[] Pixels(string type, string key)
        {
            var rows = AnimalSpriteData.Grids[type][key];
            var palette = AnimalSpriteData.Palettes[type];
            var pixels = new Color32[16 * 16];
            for (var y = 0; y < 16; y++)
                for (var x = 0; x < 16; x++)
                    if (rows[y][x] != '.') pixels[y * 16 + x] = palette[rows[y][x]];
            return pixels;
        }

        static Sprite Make(string type, string key, string[] rows)
        {
            var pixels = Pixels(type, key);
            var flipped = new Color32[16 * 16];
            for (var y = 0; y < 16; y++)
                for (var x = 0; x < 16; x++) flipped[(15 - y) * 16 + x] = pixels[y * 16 + x];     // a texture starts at the bottom row
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "animal_" + type + "_" + key };
            texture.SetPixels32(flipped);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = "animal_" + type + "_" + key;
            return sprite;
        }
    }
}
