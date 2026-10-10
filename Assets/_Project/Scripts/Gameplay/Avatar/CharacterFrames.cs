using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The animation frames of a character picture as sprites, made from the picture itself by the rig and kept (each frame is made once). A character picture is one
    // whose name starts with npc_, avatar_ or player_; one whose name ends in a facing (idle pictures, the farmer's four) can walk and swing tools, any other (a pose) only
    // gets the look (outline and shading) so that it matches the rest.
    public static class CharacterFrames
    {
        static readonly Dictionary<(Sprite, string), Sprite> Cache = new Dictionary<(Sprite, string), Sprite>();
        static readonly Dictionary<Sprite, Sprite> BaseOf = new Dictionary<Sprite, Sprite>();

        static CharacterFrames() => TestResets.Add(ClearCache);
        public static void ClearCache() { Cache.Clear(); BaseOf.Clear(); }

        public static bool IsCharacter(Sprite sprite) =>
            sprite != null && (sprite.name.StartsWith("npc_") || sprite.name.StartsWith("avatar_") || sprite.name.StartsWith("player_")) && !sprite.name.EndsWith("_asleep");        // the sleeping picture is a cut of one and is placed by its own size

        // Can this picture walk and swing (it faces a way)?
        public static bool Animates(Sprite sprite) => IsCharacter(sprite) && CharacterRig.FacingOf(sprite.name) != null;

        // The picture a frame was made from.
        public static bool TryGetBase(Sprite frame, out Sprite original) => BaseOf.TryGetValue(frame, out original);

        public static Sprite Idle(Sprite sprite, bool breath = false) =>
            Frame(sprite, breath ? "idle1" : "idle0", (g, facing) => facing != null ? CharacterRig.Idle(g, facing, breath) : null);

        public static Sprite Walk(Sprite sprite, int phase) =>
            Frame(sprite, "walk" + (((phase % CharacterRig.WalkPhases) + CharacterRig.WalkPhases) % CharacterRig.WalkPhases), (g, facing) => facing != null ? CharacterRig.Walk(g, facing, phase) : null);

        // `target`: the tile aimed at, in cells from the feet (x right, y up); the tool lands on it. Without one, the tile in front.
        public static Sprite Strike(Sprite sprite, ToolType tool, int frame) => Strike(sprite, tool, frame, Vector2Int.zero);

        public static Sprite Strike(Sprite sprite, ToolType tool, int frame, Vector2Int target) =>
            Frame(sprite, $"strike_{tool}_{frame}_{target.x}_{target.y}", (g, facing) => facing != null ? CharacterRig.Strike(g, facing, tool, frame, target) : null);

        // The picture with the look only (a pose).
        public static Sprite Looked(Sprite sprite) => Frame(sprite, "look", (g, facing) => null);

        static Sprite Frame(Sprite sprite, string kind, System.Func<PixelGrid, string, PixelGrid> make)
        {
            if (sprite == null) return null;
            if (BaseOf.ContainsKey(sprite)) return sprite;                       // already a frame
            if (!IsCharacter(sprite)) return sprite;
            var key = (sprite, kind);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            if (!StepFrames.Read(sprite, out var pixels, out var w, out var h)) return sprite;
            var grid = PixelGrid.FromTexture(pixels, w, h);
            var facing = CharacterRig.FacingOf(sprite.name);
            var framed = make(grid, facing);
            if (framed == null) framed = Padded(grid);
            var looked = CharacterLook.Apply(framed);
            var made = ToSprite(sprite, looked, kind);
            Cache[key] = made;
            BaseOf[made] = sprite;
            return made;
        }

        static PixelGrid Padded(PixelGrid g)
        {
            var o = new PixelGrid(CharacterRig.OutWidth(g.W), CharacterRig.OutHeight(g.H));
            for (var y = 0; y < g.H; y++)
                for (var x = 0; x < g.W; x++) o.Set(x + CharacterRig.PadX, y + CharacterRig.PadTop, g.Get(x, y));
            return o;
        }

        static Sprite ToSprite(Sprite source, PixelGrid grid, string kind)
        {
            var texture = new Texture2D(grid.W, grid.H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = source.name + "_" + kind };
            texture.SetPixels32(grid.ToTexture());
            texture.Apply(false, false);
            var pivot = new Vector2((source.pivot.x + CharacterRig.PadX) / grid.W, (source.pivot.y + CharacterRig.PadBottom) / grid.H);
            var sprite = Sprite.Create(texture, new Rect(0, 0, grid.W, grid.H), pivot, source.pixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
        }
    }
}
