using System.Collections.Generic;
using UnityEngine;

namespace Farm.Mythos
{
    // The pictures of "Crawling Depths" (Resources/Depths, copied unchanged by tools/art/import_pack_depths.py). The animation sheets are one file each; a frame is cut
    // from its sheet here, once, and cached. A missing sheet gives null and the caller leaves the piece out, so the layer never breaks the game.
    public static class DepthsArt
    {
        public const string Folder = "Depths/";
        public const float PixelsPerUnit = 16f;

        // The sheets and their frame sizes (pixels).
        public const int TentacleW = 48, TentacleH = 64, EyeSize = 32, LittleSize = 16, VeinSize = 16, TileSize = 16, TileColumns = 5, TileRows = 4;

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> Sheets = new Dictionary<string, Texture2D>();

        static DepthsArt() => Farm.Core.TestResets.Add(() => { Cache.Clear(); Sheets.Clear(); });

        public static Texture2D Sheet(string name)
        {
            if (Sheets.TryGetValue(name, out var cached) && cached != null) return cached;
            var tex = Resources.Load<Texture2D>(Folder + name);
            if (tex != null) Sheets[name] = tex;
            return tex;
        }

        // How many frames of `frameWidth` a sheet holds (0 when it is missing).
        public static int Frames(string sheet, int frameWidth)
        {
            var tex = Sheet(sheet);
            return tex == null ? 0 : tex.width / frameWidth;
        }

        // Frame `index` of a sheet of frames side by side, its foot (bottom centre) as the pivot. `rows` > 0 gives only the TOP `rows` pixel rows of the frame: a tentacle
        // rising out of the ground shows its tip first, then more of itself.
        public static Sprite Frame(string sheet, int frameWidth, int frameHeight, int index, int rows = 0)
        {
            var tex = Sheet(sheet);
            if (tex == null) return null;
            var frames = tex.width / frameWidth;
            if (frames <= 0) return null;
            index = ((index % frames) + frames) % frames;
            rows = rows <= 0 || rows > frameHeight ? frameHeight : rows;
            var key = $"{sheet}:{frameWidth}x{frameHeight}:{index}:{rows}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var rect = new Rect(index * frameWidth, frameHeight - rows, frameWidth, rows);
            return Cache[key] = Make(tex, rect, new Vector2(0.5f, 0f), key);
        }

        // A whole picture (a statue, an urn), its foot as the pivot.
        public static Sprite Whole(string sheet)
        {
            var key = sheet + ":whole";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = Sheet(sheet);
            return tex == null ? null : Cache[key] = Make(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), key);
        }

        // One of the 5 x 4 cracked floor tiles (index counted along the rows from the top left), centred.
        public static Sprite Tile(int index)
        {
            index = ((index % (TileColumns * TileRows)) + TileColumns * TileRows) % (TileColumns * TileRows);
            var key = "floor_tiles:" + index;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = Sheet("floor_tiles");
            if (tex == null) return null;
            var column = index % TileColumns; var row = index / TileColumns;
            return Cache[key] = Make(tex, new Rect(column * TileSize, (TileRows - 1 - row) * TileSize, TileSize, TileSize), new Vector2(0.5f, 0.5f), key);
        }

        static Sprite Make(Texture2D tex, Rect rect, Vector2 pivot, string name)
        {
            var sprite = Sprite.Create(tex, rect, pivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }
    }
}
