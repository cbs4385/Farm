using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The tools a character swings, drawn pixel by pixel at any angle (a hand-drawn picture rotated by an arbitrary angle turns to mush at this size, and a line of
    // pixels does not). Angles are in degrees on a grid whose rows grow downward: 0 points right, 90 points down, 270 points up. Each glyph is a list of pixels
    // relative to the hand.
    public static class ToolGlyphs
    {
        public readonly struct Dot
        {
            public readonly int X, Y;
            public readonly Color32 Color;
            public Dot(int x, int y, Color32 color) { X = x; Y = y; Color = color; }
        }

        static readonly Color32 Wood = new Color32(0x86, 0x5c, 0x34, 255), WoodDark = new Color32(0x5a, 0x3b, 0x20, 255);
        static readonly Color32 Metal = new Color32(0xcf, 0xd6, 0xdc, 255), MetalDark = new Color32(0x84, 0x90, 0x9b, 255), MetalDeep = new Color32(0x5d, 0x68, 0x73, 255);
        static readonly Color32 Blue = new Color32(0x5b, 0x93, 0xcf, 255), BlueDark = new Color32(0x3a, 0x63, 0x98, 255);
        static readonly Color32 Thread = new Color32(0xe8, 0xe4, 0xd8, 255);

        // How far the working end of a tool is from the hand, in pixels (the glyphs below stop here).
        public static int Length(ToolType tool)
        {
            switch (tool)
            {
                case ToolType.Rod: return 13;
                case ToolType.Sword: return 13;
                case ToolType.Scythe: return 10;
                case ToolType.Hammer: return 9;
                case ToolType.WateringCan: return 8;
                default: return 9;
            }
        }

        // How far the end of the tool is from the hand when its handle is drawn `stretch` times as long (the head keeps its size).
        public static float StretchedLength(ToolType tool, float stretch)
        {
            var handle = Length(tool) - HeadDepth;
            return handle * stretch + HeadDepth;
        }

        const int HeadDepth = 2;

        // The tool in the hand at the given angle. Empty for a tool with no glyph. A `stretch` above one lengthens the handle (a swing that has to reach a long way).
        public static List<Dot> For(ToolType tool, float angleDegrees, float stretch = 1f)
        {
            var rad = angleDegrees * Mathf.Deg2Rad;
            var d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            var p = new Vector2(-d.y, d.x);
            var dots = new List<Dot>();

            void Put(Vector2 at, Color32 c) => dots.Add(new Dot(Mathf.RoundToInt(at.x), Mathf.RoundToInt(at.y), c));
            void Line(Vector2 a, Vector2 b, Color32 c)
            {
                var steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) * 1.5f));
                for (var i = 0; i <= steps; i++) Put(Vector2.Lerp(a, b, i / (float)steps), c);
            }
            var handleEnd = Length(tool) - HeadDepth;
            Vector2 At(float along, float across = 0f)
            {
                var a = along <= handleEnd ? along * stretch : handleEnd * stretch + (along - handleEnd);
                return d * a + p * across;
            }

            switch (tool)
            {
                case ToolType.Hoe:
                    Line(At(-1), At(9), Wood);
                    Line(At(9, -1), At(9, 3), Metal);
                    Put(At(8, 3), MetalDark); Put(At(10, 3), MetalDark);
                    break;
                case ToolType.Axe:
                    Line(At(-1), At(9), Wood);
                    for (var a = 0; a <= 3; a++) { Put(At(8, a), Metal); Put(At(9, a), a == 3 ? MetalDark : Metal); Put(At(7, a), MetalDark); }
                    break;
                case ToolType.Pickaxe:
                    Line(At(-1), At(9), Wood);
                    Line(At(9, -4), At(9, 4), Metal);
                    Put(At(8, -4), MetalDark); Put(At(8, 4), MetalDark);
                    break;
                case ToolType.Hammer:
                    Line(At(-1), At(8), Wood);
                    for (var a = -2; a <= 2; a++) for (var b = 7; b <= 10; b++) Put(At(b, a), b == 7 || a == 2 ? MetalDeep : MetalDark);
                    break;
                case ToolType.Scythe:
                    Line(At(-1), At(10), Wood);
                    for (var i = 0; i <= 5; i++) Put(At(10 - i * i / 8f, i), i < 2 ? Metal : MetalDark);
                    Put(At(10, 1), Metal);
                    break;
                case ToolType.Rod:
                    Line(At(-1), At(13), WoodDark);
                    Line(At(13), At(13) + new Vector2(0f, 6f), Thread);
                    Put(At(13) + new Vector2(0f, 7f), MetalDark);
                    break;
                case ToolType.Sword:
                    Line(At(-1), At(2), Wood);
                    Line(At(3, -2), At(3, 2), MetalDeep);
                    Line(At(4), At(12), Metal);
                    Put(At(13), MetalDark);
                    break;
                case ToolType.WateringCan:
                    for (var a = -1; a <= 1; a++) for (var b = 1; b <= 5; b++) Put(At(b, a), b == 5 || a == 1 ? BlueDark : Blue);
                    Line(At(6, 0), At(9, -1), BlueDark);
                    Put(At(0, -2), BlueDark); Put(At(-1, -1), BlueDark);
                    break;
            }
            return dots;
        }
    }
}
