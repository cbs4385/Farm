using System.IO;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // A look at the frames: writes Builds/anim/<name>.png with, for each facing, the standing picture, the four walk frames and the four frames of a swing.
    // Run on purpose (it is Explicit): `-testFilter CharacterSheetDumpTests`.
    public class CharacterSheetDumpTests
    {
        const int Scale = 4;

        static PixelGrid Load(string name)
        {
            var path = Path.Combine(Application.dataPath, "_Project", "Art", "Placeholders", name + ".png");
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(path));
            var grid = PixelGrid.FromTexture(tex.GetPixels32(), tex.width, tex.height);
            Object.DestroyImmediate(tex);
            return grid;
        }

        static void Blit(Texture2D sheet, PixelGrid g, int ox, int oy)
        {
            for (var y = 0; y < g.H; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    for (var sy = 0; sy < Scale; sy++)
                        for (var sx = 0; sx < Scale; sx++)
                            sheet.SetPixel(ox + x * Scale + sx, sheet.height - 1 - (oy + y * Scale + sy), c);
                }
        }

        [Test, Explicit("writes pictures for a person to look at")]
        public void Dump()
        {
            var dir = Path.Combine(Application.dataPath, "..", "Builds", "anim");
            Directory.CreateDirectory(dir);
            foreach (var (prefix, tool) in new[] { ("player", ToolType.Hoe), ("npc_bram", ToolType.Hammer), ("npc_tilda", ToolType.Axe), ("npc_juno", ToolType.Hammer), ("npc_felix", ToolType.Rod), ("npc_elara", ToolType.Scythe), ("npc_wren", ToolType.WateringCan) })
            {
                var facings = new[] { "down", "up", "left", "right" };
                var cellW = CharacterRig.OutWidth(16) * Scale; var cellH = CharacterRig.OutHeight(32) * Scale;
                var sheet = new Texture2D(cellW * 9, cellH * facings.Length, TextureFormat.RGBA32, false);
                var bg = new Color32[sheet.width * sheet.height];
                for (var i = 0; i < bg.Length; i++) bg[i] = new Color32(94, 140, 80, 255);
                sheet.SetPixels32(bg);
                for (var row = 0; row < facings.Length; row++)
                {
                    var f = facings[row];
                    var g = Load($"{prefix}_idle_{f}");
                    var frames = new System.Collections.Generic.List<PixelGrid> { CharacterRig.Idle(g, f, false) };
                    for (var p = 0; p < 4; p++) frames.Add(CharacterRig.Walk(g, f, p));
                    for (var s = 0; s < 4; s++) frames.Add(CharacterRig.Strike(g, f, tool, s));
                    for (var i = 0; i < frames.Count; i++) Blit(sheet, CharacterLook.Apply(frames[i]), i * cellW, row * cellH);
                }
                sheet.Apply();
                File.WriteAllBytes(Path.Combine(dir, prefix + ".png"), sheet.EncodeToPNG());
                Object.DestroyImmediate(sheet);
            }
            var looks = new[]
            {
                ("avatar_f", new AvatarData()),
                ("avatar_m", new AvatarData { Build = "masculine", Hair = "short", Shirt = "overalls", Pants = "trousers", Accessory = "cap", ShirtColor = "#c0453f" }),
                ("avatar_d", new AvatarData { Build = "feminine", Hair = "bun", Shirt = "dress", Pants = "skirt", Accessory = "flower", ShirtColor = "#8a5aa8", Skin = "#8d5524" }),
            };
            foreach (var (name, look) in looks)
            {
                var facings = new[] { "down", "up", "left", "right" };
                var cellW = CharacterRig.OutWidth(16) * Scale; var cellH = CharacterRig.OutHeight(32) * Scale;
                var sheet = new Texture2D(cellW * 9, cellH * facings.Length, TextureFormat.RGBA32, false);
                var bg = new Color32[sheet.width * sheet.height];
                for (var i = 0; i < bg.Length; i++) bg[i] = new Color32(94, 140, 80, 255);
                sheet.SetPixels32(bg);
                for (var row = 0; row < facings.Length; row++)
                {
                    var f = facings[row];
                    var g = new PixelGrid(16, 32);
                    System.Array.Copy(AvatarComposer.Compose(look, f), g.P, g.P.Length);
                    var frames = new System.Collections.Generic.List<PixelGrid> { CharacterRig.Idle(g, f, false) };
                    for (var p = 0; p < 4; p++) frames.Add(CharacterRig.Walk(g, f, p));
                    for (var s = 0; s < 4; s++) frames.Add(CharacterRig.Strike(g, f, ToolType.Axe, s));
                    for (var i = 0; i < frames.Count; i++) Blit(sheet, CharacterLook.Apply(frames[i]), i * cellW, row * cellH);
                }
                sheet.Apply();
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), sheet.EncodeToPNG());
                Object.DestroyImmediate(sheet);
            }
            // the strike frame aimed at each of the eight tiles around the farmer (the tile's middle is marked)
            {
                var look = new AvatarData { Build = "masculine", Hair = "short", Shirt = "overalls", Pants = "trousers", Accessory = "cap", ShirtColor = "#c0453f" };
                var targets = new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1) };
                var facings = new[] { "down", "left" };
                var cellW = CharacterRig.OutWidth(16) * Scale; var cellH = CharacterRig.OutHeight(32) * Scale;
                var sheet = new Texture2D(cellW * targets.Length, cellH * facings.Length, TextureFormat.RGBA32, false);
                var bg = new Color32[sheet.width * sheet.height];
                for (var i = 0; i < bg.Length; i++) bg[i] = new Color32(94, 140, 80, 255);
                sheet.SetPixels32(bg);
                for (var row = 0; row < facings.Length; row++)
                {
                    var f = facings[row];
                    var g = new PixelGrid(16, 32);
                    System.Array.Copy(AvatarComposer.Compose(look, f), g.P, g.P.Length);
                    for (var i = 0; i < targets.Length; i++)
                    {
                        var frame = CharacterLook.Apply(CharacterRig.Strike(g, f, ToolType.Hoe, 2, targets[i]));
                        Blit(sheet, frame, i * cellW, row * cellH);
                        var (_, aimed, _) = CharacterRig.StrikeGeometry(g, f, ToolType.Hoe, targets[i]);
                        for (var d = -2; d <= 2; d++)
                        {
                            sheet.SetPixel(i * cellW + Mathf.RoundToInt(aimed.x) * Scale + d, sheet.height - 1 - (row * cellH + Mathf.RoundToInt(aimed.y) * Scale), Color.red);
                            sheet.SetPixel(i * cellW + Mathf.RoundToInt(aimed.x) * Scale, sheet.height - 1 - (row * cellH + Mathf.RoundToInt(aimed.y) * Scale + d), Color.red);
                        }
                    }
                }
                sheet.Apply();
                File.WriteAllBytes(Path.Combine(dir, "targets.png"), sheet.EncodeToPNG());
                Object.DestroyImmediate(sheet);
            }
            // watering: the four frames at a tile in front, a diagonal one and one beside
            {
                var look = new AvatarData { Build = "masculine", Hair = "short", Shirt = "overalls", Pants = "trousers", Accessory = "cap", ShirtColor = "#c0453f" };
                var rows = new[] { ("down", new Vector2Int(0, -1)), ("down", new Vector2Int(1, -1)), ("left", new Vector2Int(-1, 0)), ("up", new Vector2Int(0, 1)) };
                var cellW = CharacterRig.OutWidth(16) * Scale; var cellH = CharacterRig.OutHeight(32) * Scale;
                var sheet = new Texture2D(cellW * 4, cellH * rows.Length, TextureFormat.RGBA32, false);
                var bg = new Color32[sheet.width * sheet.height];
                for (var i = 0; i < bg.Length; i++) bg[i] = new Color32(94, 140, 80, 255);
                sheet.SetPixels32(bg);
                for (var row = 0; row < rows.Length; row++)
                {
                    var (f, t) = rows[row];
                    var g = new PixelGrid(16, 32);
                    System.Array.Copy(AvatarComposer.Compose(look, f), g.P, g.P.Length);
                    for (var frame = 0; frame < 4; frame++) Blit(sheet, CharacterLook.Apply(CharacterRig.Strike(g, f, ToolType.WateringCan, frame, t)), frame * cellW, row * cellH);
                }
                sheet.Apply();
                File.WriteAllBytes(Path.Combine(dir, "pour.png"), sheet.EncodeToPNG());
                Object.DestroyImmediate(sheet);
            }
            Assert.Pass("pictures are in Builds/anim");
        }
    }
}
