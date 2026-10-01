using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-008: generates simple placeholder sprites so agents can build features without final art.
    // Naming: <category>_<name>[_<frame>] (e.g. crop_parsnip_2). Final art should reuse the same names.
    // One PNG per sprite. Import settings come from TextureImportPostprocessor.
    public static class PlaceholderArtGenerator
    {
        const string OutDir = "Assets/_Project/Art/Placeholders";

        public readonly struct CropArt
        {
            public readonly string Name; public readonly int Stages; public readonly Color Color;
            public CropArt(string name, int stages, Color color) { Name = name; Stages = stages; Color = color; }
        }

        // Stage counts must match the crop growth tables in ContentGenerator.
        public static readonly CropArt[] Crops =
        {
            new CropArt("parsnip", 4, new Color(0.93f, 0.85f, 0.65f)),
            new CropArt("potato", 5, new Color(0.72f, 0.55f, 0.32f)),
            new CropArt("cauliflower", 4, new Color(0.94f, 0.94f, 0.88f)),
            new CropArt("greenbean", 5, new Color(0.35f, 0.75f, 0.30f)),
            new CropArt("strawberry", 5, new Color(0.90f, 0.22f, 0.28f)),
            new CropArt("kale", 4, new Color(0.20f, 0.50f, 0.30f)),
        };

        static readonly Color Clear = new Color(0, 0, 0, 0);
        static readonly Color Outline = new Color(0.1f, 0.1f, 0.12f, 1f);

        [MenuItem("Farm/Generate Placeholder Art")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutDir);
            var written = new List<string>();

            // Tiles (16x16)
            Tile(written, "tile_grass", new Color(0.36f, 0.62f, 0.28f), new Color(0.31f, 0.55f, 0.24f));
            Tile(written, "tile_dirt", new Color(0.55f, 0.40f, 0.26f), new Color(0.50f, 0.36f, 0.23f));
            Tile(written, "tile_tilled", new Color(0.40f, 0.27f, 0.17f), new Color(0.33f, 0.22f, 0.14f), furrows: true);
            Tile(written, "tile_tilled_watered", new Color(0.28f, 0.19f, 0.12f), new Color(0.22f, 0.15f, 0.10f), furrows: true);
            Tile(written, "tile_path", new Color(0.72f, 0.66f, 0.52f), new Color(0.66f, 0.60f, 0.47f));
            Tile(written, "tile_water", new Color(0.25f, 0.48f, 0.80f), new Color(0.30f, 0.54f, 0.86f));
            Tile(written, "tile_wall", new Color(0.45f, 0.40f, 0.42f), new Color(0.38f, 0.34f, 0.36f), border: true);
            Tile(written, "tile_floor_wood", new Color(0.62f, 0.45f, 0.28f), new Color(0.56f, 0.40f, 0.25f), border: true);

            // Characters (16x32)
            foreach (var dir in new[] { "down", "up", "left", "right" })
            {
                Character(written, $"player_idle_{dir}", new Color(0.25f, 0.45f, 0.85f), dir);
                Character(written, $"npc_generic_idle_{dir}", new Color(0.85f, 0.45f, 0.25f), dir);
            }

            // Crops: <stages + 1> growth sprites each, plus seed and harvest item icons (16x16)
            foreach (var crop in Crops)
            {
                for (int stage = 0; stage <= crop.Stages; stage++)
                    Crop(written, $"crop_{crop.Name}_{stage}", stage, crop.Stages, crop.Color);
                Item(written, $"item_seed_{crop.Name}", Color.Lerp(crop.Color, new Color(0.85f, 0.78f, 0.45f), 0.6f));
                Item(written, $"item_crop_{crop.Name}", crop.Color);
            }

            // Items (16x16)
            Item(written, "item_resource_fiber", new Color(0.55f, 0.70f, 0.30f));
            Item(written, "item_seed_generic", new Color(0.85f, 0.78f, 0.45f));
            Item(written, "item_crop_generic", new Color(0.55f, 0.80f, 0.30f));
            Item(written, "item_tool_hoe", new Color(0.65f, 0.65f, 0.70f));
            Item(written, "item_tool_wateringcan", new Color(0.35f, 0.55f, 0.80f));
            Item(written, "item_tool_axe", new Color(0.70f, 0.50f, 0.30f));
            Item(written, "item_tool_pickaxe", new Color(0.55f, 0.55f, 0.60f));
            Item(written, "item_tool_scythe", new Color(0.80f, 0.80f, 0.85f));
            Item(written, "item_resource_wood", new Color(0.55f, 0.38f, 0.22f));
            Item(written, "item_resource_stone", new Color(0.60f, 0.60f, 0.62f));

            // World objects (16x16)
            WorldObject(written, "obj_bin", new Color(0.55f, 0.36f, 0.20f), new Color(0.35f, 0.22f, 0.12f));
            WorldObject(written, "obj_bed", new Color(0.85f, 0.45f, 0.45f), new Color(0.95f, 0.90f, 0.85f));
            WorldObject(written, "obj_shop", new Color(0.30f, 0.55f, 0.75f), new Color(0.95f, 0.85f, 0.40f));
            Cursor(written, "ui_cursor");

            // UI (small 9-slice-friendly frames)
            UiFrame(written, "ui_frame_panel", 24, new Color(0.93f, 0.85f, 0.65f), new Color(0.45f, 0.30f, 0.15f));
            UiFrame(written, "ui_slot", 18, new Color(0.80f, 0.70f, 0.50f), new Color(0.45f, 0.30f, 0.15f));
            UiFrame(written, "ui_slot_selected", 18, new Color(0.95f, 0.88f, 0.55f), new Color(0.85f, 0.30f, 0.20f));

            AssetDatabase.Refresh();
            Debug.Log($"[PlaceholderArt] Wrote {written.Count} sprites to {OutDir}.");
        }

        public static void GenerateAndExit()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        static void Save(List<string> written, string name, Texture2D tex)
        {
            var path = $"{OutDir}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            written.Add(path);
        }

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = Clear;
            t.SetPixels(px);
            return t;
        }

        static void Rect(Texture2D t, int x, int y, int w, int h, Color c)
        {
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                    if (i >= 0 && j >= 0 && i < t.width && j < t.height) t.SetPixel(i, j, c);
        }

        static void Tile(List<string> written, string name, Color a, Color b, bool furrows = false, bool border = false)
        {
            var t = NewTex(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    // Deterministic speckle so tiles are visibly tile-aligned (helps spot shimmer/seams).
                    var c = ((x * 7 + y * 13) % 5 == 0) ? b : a;
                    if (furrows && y % 4 == 1) c = b;
                    if (border && (x == 0 || y == 0)) c = b;
                    t.SetPixel(x, y, c);
                }
            Save(written, name, t);
        }

        static void Character(List<string> written, string name, Color body, string dir)
        {
            var t = NewTex(16, 32);
            var skin = new Color(0.95f, 0.78f, 0.62f);
            Rect(t, 4, 0, 8, 12, body);              // legs/body
            Rect(t, 3, 12, 10, 8, body);             // torso
            Rect(t, 4, 20, 8, 8, skin);              // head
            Rect(t, 4, 26, 8, 3, new Color(0.30f, 0.20f, 0.12f)); // hair
            // facing marker: eyes/back/side
            switch (dir)
            {
                case "down": Rect(t, 5, 22, 2, 2, Outline); Rect(t, 9, 22, 2, 2, Outline); break;
                case "left": Rect(t, 4, 22, 2, 2, Outline); break;
                case "right": Rect(t, 10, 22, 2, 2, Outline); break;
                case "up": Rect(t, 4, 20, 8, 3, new Color(0.30f, 0.20f, 0.12f)); break;
            }
            Save(written, name, t);
        }

        static void Crop(List<string> written, string name, int stage, int maxStage, Color fruit)
        {
            var t = NewTex(16, 16);
            var green = new Color(0.30f, 0.70f, 0.25f);
            if (stage == 0)
            {
                // Freshly planted: a visible mound with a small sprout (must read clearly at 1x).
                var dirt = new Color(0.62f, 0.46f, 0.30f);
                Rect(t, 4, 0, 8, 4, dirt);
                Rect(t, 5, 4, 6, 1, dirt);
                Rect(t, 7, 4, 2, 4, green);
                Rect(t, 5, 6, 2, 2, green);
                Rect(t, 9, 7, 2, 2, green);
            }
            else
            {
                int height = 5 + stage * 8 / Mathf.Max(1, maxStage);
                Rect(t, 7, 0, 2, height, green);
                Rect(t, 4, height / 2, 3, 2, green);
                Rect(t, 9, height / 2 + 1, 3, 2, green);
                if (stage == maxStage) Rect(t, 5, Mathf.Min(height, 12), 6, 4, fruit);
            }
            Save(written, name, t);
        }

        static void Item(List<string> written, string name, Color c)
        {
            var t = NewTex(16, 16);
            Rect(t, 3, 3, 10, 10, Outline);
            Rect(t, 4, 4, 8, 8, c);
            Save(written, name, t);
        }

        static void WorldObject(List<string> written, string name, Color body, Color trim)
        {
            var t = NewTex(16, 16);
            Rect(t, 0, 0, 16, 16, Outline);
            Rect(t, 1, 1, 14, 14, body);
            Rect(t, 1, 10, 14, 5, trim);
            Save(written, name, t);
        }

        static void Cursor(List<string> written, string name)
        {
            var t = NewTex(16, 16);
            var c = new Color(1f, 0.9f, 0.3f, 0.95f);
            Rect(t, 0, 0, 16, 1, c); Rect(t, 0, 15, 16, 1, c);
            Rect(t, 0, 0, 1, 16, c); Rect(t, 15, 0, 1, 16, c);
            Save(written, name, t);
        }

        static void UiFrame(List<string> written, string name, int size, Color fill, Color border)
        {
            var t = NewTex(size, size);
            Rect(t, 0, 0, size, size, border);
            Rect(t, 2, 2, size - 4, size - 4, fill);
            Save(written, name, t);
        }
    }
}
