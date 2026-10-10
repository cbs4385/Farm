using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // A simple rig for any standing character picture (the farmer in any outfit, every villager) that has a head, a torso with arms and legs, as the pictures here all
    // do: the picture is cut into three bands (head, torso, legs) and each frame of a walk or a tool swing moves the bands and the arms a pixel or two, with the feet
    // staying on the ground. Tools are drawn pixel by pixel into the hand (ToolGlyphs). Pure: pixels in, pixels out, so the frames can be tested without a scene.
    // Frames are drawn on a bigger canvas than the picture (PadX each side, PadTop above, PadBottom below) so that raised tools and outlines have room; the picture
    // stays where it was on it (OriginX, OriginY).
    public static class CharacterRig
    {
        public const int PadX = 8, PadTop = 10, PadBottom = 1;
        public const int WalkPhases = 4, StrikeFrames = 4;

        public static int OutWidth(int w) => w + 2 * PadX;
        public static int OutHeight(int h) => h + PadTop + PadBottom;

        public struct Bands
        {
            public int Top;          // first row with anything
            public int HeadEnd;      // first row of the torso
            public int TorsoEnd;     // first row of the legs
            public int Bottom;       // last row (the feet)
            public int Left, Right;  // the columns the torso covers
        }

        public static readonly string[] Facings = { "down", "up", "left", "right" };

        // "down", "up", "left" or "right" when a sprite's name ends in one (idle pictures, the avatar's pictures), else null (a pose, a prop).
        public static string FacingOf(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            foreach (var f in Facings) if (spriteName.EndsWith("_" + f, System.StringComparison.Ordinal)) return f;
            return null;
        }

        public static bool TryBands(PixelGrid g, out Bands b)
        {
            b = default;
            var (top, bottom) = g.Rows();
            if (top < 0 || bottom - top < 12) return false;
            var height = bottom - top + 1;
            b.Top = top;
            b.Bottom = bottom;
            b.HeadEnd = top + Mathf.RoundToInt(height * 0.40f);
            b.TorsoEnd = top + Mathf.RoundToInt(height * 0.72f);
            var (l, r) = g.Columns(b.HeadEnd, b.TorsoEnd - 1);
            b.Left = l; b.Right = r;
            return l >= 0;
        }

        // ---- what a frame changes ----

        struct Pose
        {
            public int BodyDx, BodyDy;          // head and torso together
            public int HeadDx, HeadDy;          // the head on top of that
            public int LegLiftLeft, LegLiftRight;          // a foot raised by a pixel (front and back views): the left or the right half of the legs
            public int LegFrontDx, LegBackDx;   // the two legs in a side view: how far each is moved along the row
            public int ArmLeftDy, ArmRightDy;   // the arms in a front or back view
            public bool NoArms;                 // the arms are drawn by hand (a swing)
        }

        // ---- the frames ----

        // Standing: the picture on its bigger canvas, or breathing in (the body a pixel taller, the feet where they were).
        public static PixelGrid Idle(PixelGrid g, string facing, bool breath)
        {
            if (!TryBands(g, out var b)) return Plain(g);
            return Build(g, b, new Pose { BodyDy = breath ? -1 : 0 }, facing);
        }

        // A walk of four phases: 0 and 2 a foot forward and an arm swung, 1 and 3 passing with the body a pixel up.
        public static PixelGrid Walk(PixelGrid g, string facing, int phase)
        {
            if (!TryBands(g, out var b)) return Plain(g);
            phase = ((phase % WalkPhases) + WalkPhases) % WalkPhases;
            var pose = new Pose();
            var passing = phase == 1 || phase == 3;
            if (passing) pose.BodyDy = -1;
            var side = facing == "left" || facing == "right";
            if (side)
            {
                var sign = phase == 0 ? 1 : phase == 2 ? -1 : 0;
                pose.LegFrontDx = sign;          // positive: the front leg is ahead
                pose.LegBackDx = -sign;
                pose.HeadDy = 0;
            }
            else
            {
                pose.LegLiftLeft = phase == 0 ? 1 : 0;
                pose.LegLiftRight = phase == 2 ? 1 : 0;
                pose.ArmLeftDy = phase == 0 ? -1 : phase == 2 ? 1 : 0;
                pose.ArmRightDy = phase == 0 ? 1 : phase == 2 ? -1 : 0;
            }
            return Build(g, b, pose, facing);
        }

        // A swing of a tool in four frames: wind up, swing, strike, recover. The arm and the tool are drawn into the hand.
        public static PixelGrid Strike(PixelGrid g, string facing, ToolType tool, int frame)
        {
            if (!TryBands(g, out var b)) return Plain(g);
            frame = Mathf.Clamp(frame, 0, StrikeFrames - 1);
            var side = facing == "left" || facing == "right";
            var forward = facing == "left" ? -1 : 1;
            var pose = new Pose { NoArms = true };
            if (side)
            {
                pose.BodyDx = forward * new[] { -1, 0, 2, 1 }[frame];
                pose.BodyDy = frame == 0 ? -1 : 0;
            }
            else
            {
                pose.BodyDy = frame == 0 ? -1 : 0;
                pose.HeadDy = frame == 2 ? 1 : 0;
            }
            var body = Build(g, b, pose, facing);
            DrawArmAndTool(body, g, b, facing, tool, frame, pose);
            return body;
        }

        // ---- building a frame ----

        static PixelGrid Plain(PixelGrid g)
        {
            var o = new PixelGrid(OutWidth(g.W), OutHeight(g.H));
            for (var y = 0; y < g.H; y++)
                for (var x = 0; x < g.W; x++) o.Set(x + PadX, y + PadTop, g.Get(x, y));
            return o;
        }

        static PixelGrid Build(PixelGrid g, Bands b, Pose pose, string facing)
        {
            var o = new PixelGrid(OutWidth(g.W), OutHeight(g.H));
            var legsStart = b.TorsoEnd;
            var side = facing == "left" || facing == "right";
            var armColumnsLeft = (b.Left, b.Left + 1);
            var armColumnsRight = (b.Right - 1, b.Right);
            var armsMove = !side && !pose.NoArms && (pose.ArmLeftDy != 0 || pose.ArmRightDy != 0);

            // legs
            var (legL, legR) = g.Columns(legsStart, b.Bottom);
            var split = legL >= 0 ? (legL + legR + 1) / 2 : g.W / 2;
            for (var y = legsStart; y <= b.Bottom; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    var dx = 0; var dy = 0;
                    if (side)
                    {
                        // the leg nearer the side the character faces is the front one
                        var frontHalf = facing == "left" ? x < split : x >= split;
                        var move = frontHalf ? pose.LegFrontDx : pose.LegBackDx;
                        dx = facing == "left" ? -move : move;
                    }
                    else
                    {
                        var lift = x < split ? pose.LegLiftLeft : pose.LegLiftRight;
                        dy = -lift;
                        if (lift > 0 && y == b.Bottom) continue;           // the raised foot is a pixel shorter
                    }
                    o.Set(x + PadX + dx, y + PadTop + dy, c);
                }

            // torso (the arms are carried separately when they swing)
            for (var y = b.HeadEnd; y < b.TorsoEnd; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    if (armsMove && y >= b.HeadEnd + 2 && ((x >= armColumnsLeft.Item1 && x <= armColumnsLeft.Item2) || (x >= armColumnsRight.Item1 && x <= armColumnsRight.Item2))) continue;
                    o.Set(x + PadX + pose.BodyDx, y + PadTop + pose.BodyDy, c);
                }
            if (armsMove)
            {
                for (var y = b.HeadEnd + 2; y < b.TorsoEnd; y++)
                    for (var x = 0; x < g.W; x++)
                    {
                        var left = x >= armColumnsLeft.Item1 && x <= armColumnsLeft.Item2;
                        var right = x >= armColumnsRight.Item1 && x <= armColumnsRight.Item2;
                        if (!left && !right) continue;
                        var c = g.Get(x, y);
                        if (c.a == 0) continue;
                        var dy = left ? pose.ArmLeftDy : pose.ArmRightDy;
                        o.Set(x + PadX + pose.BodyDx, y + PadTop + pose.BodyDy + dy, c);
                    }
                // a swung arm leaves a cell behind: it takes the colour of the torso beside it
                for (var y = b.HeadEnd + 2; y < b.TorsoEnd; y++)
                    foreach (var (x0, inner) in new[] { (armColumnsLeft.Item1, 1), (armColumnsRight.Item2, -1) })
                    {
                        var ox = x0 + PadX + pose.BodyDx;
                        var oy = y + PadTop + pose.BodyDy;
                        if (!o.Opaque(ox, oy) && g.Opaque(x0, y)) o.Set(ox, oy, o.Get(ox + inner * 2, oy));
                    }
            }

            // head
            for (var y = b.Top; y < b.HeadEnd; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    o.Set(x + PadX + pose.BodyDx + pose.HeadDx, y + PadTop + pose.BodyDy + pose.HeadDy, c);
                }

            FillGaps(o, g, b);
            return o;
        }

        // A row left empty between two full ones (the body rose a pixel off the legs) takes the row above it.
        static void FillGaps(PixelGrid o, PixelGrid g, Bands b)
        {
            for (var y = PadTop + b.Top + 1; y < PadTop + b.Bottom; y++)
            {
                if (RowHas(o, y) || !RowHas(o, y - 1) || !RowHas(o, y + 1)) continue;
                for (var x = 0; x < o.W; x++) if (o.Opaque(x, y - 1)) o.Set(x, y, o.Get(x, y - 1));
            }
        }

        static bool RowHas(PixelGrid o, int y)
        {
            for (var x = 0; x < o.W; x++) if (o.Opaque(x, y)) return true;
            return false;
        }

        // ---- the arm and the tool of a swing ----

        // Hand position relative to the shoulder (x to the right, y down) and the angle of the tool, per facing and frame. Right-facing swings mirror the left ones.
        static readonly Dictionary<string, (int x, int y, int angle)[]> Hands = new Dictionary<string, (int, int, int)[]>
        {
            { "down", new[] { (1, -10, 285), (2, -6, 305), (0, 7, 85), (1, 3, 70) } },
            { "up", new[] { (1, -9, 280), (1, -12, 270), (0, -12, 262), (1, -4, 275) } },
            { "left", new[] { (3, -9, 300), (-2, -10, 250), (-5, 2, 125), (-3, 0, 105) } },
        };

        static void DrawArmAndTool(PixelGrid o, PixelGrid g, Bands b, string facing, ToolType tool, int frame, Pose pose)
        {
            var mirror = facing == "right";
            var key = mirror ? "left" : facing;
            if (!Hands.TryGetValue(key, out var table)) return;
            var (hx, hy, angle) = table[frame];
            var cx = PadX + (b.Left + b.Right) / 2 + pose.BodyDx;
            var shoulder = key == "left"
                ? new Vector2Int(cx + (mirror ? 1 : -1), PadTop + b.HeadEnd + 2 + pose.BodyDy)
                : new Vector2Int(cx + (key == "down" ? 4 : 4), PadTop + b.HeadEnd + 1 + pose.BodyDy);
            if (mirror) { hx = -hx; angle = 180 - angle; }
            var hand = new Vector2Int(shoulder.x + hx, shoulder.y + hy);
            var sleeve = SleeveColor(g, b);
            var skin = SkinColor(g, b, sleeve);

            // the arm: a line of the sleeve's colour from the shoulder, the last pixels skin
            var steps = Mathf.Max(Mathf.Abs(hand.x - shoulder.x), Mathf.Abs(hand.y - shoulder.y));
            for (var i = 0; i <= steps; i++)
            {
                var t = steps == 0 ? 1f : i / (float)steps;
                var at = Vector2.Lerp(shoulder, hand, t);
                var x = Mathf.RoundToInt(at.x); var y = Mathf.RoundToInt(at.y);
                var c = i >= steps - 1 && steps > 2 ? skin : sleeve;
                o.Set(x, y, c);
                o.Set(x + (key == "left" ? 0 : 1), y + (key == "left" ? 1 : 0), c);
            }
            o.Set(hand.x, hand.y, skin);
            foreach (var dot in ToolGlyphs.For(tool, angle)) o.Set(hand.x + dot.X, hand.y + dot.Y, dot.Color);
            o.Set(hand.x, hand.y, skin);                   // the hand over the handle
        }

        // The colour at the side of the torso (the sleeve, or the coat when there is no sleeve).
        static Color32 SleeveColor(PixelGrid g, Bands b)
        {
            for (var y = b.HeadEnd + 3; y < b.TorsoEnd; y++)
            {
                var c = g.Get(b.Left, y);
                if (c.a > 0) return c;
            }
            return new Color32(0x6b, 0x7f, 0x91, 255);
        }

        // The most common warm mid-tone of the lower half of the head (the face), else a plain skin colour.
        static Color32 SkinColor(PixelGrid g, Bands b, Color32 fallback)
        {
            var counts = new Dictionary<int, int>();
            var best = 0; var found = false; Color32 skin = default;
            for (var y = b.Top + (b.HeadEnd - b.Top) / 2; y < b.HeadEnd; y++)
                for (var x = g.W / 4; x < g.W - g.W / 4; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0 || !(c.r > 150 && c.r >= c.g && c.g >= c.b - 12 && c.r - c.b > 25)) continue;
                    var key = (c.r << 16) | (c.g << 8) | c.b;
                    counts.TryGetValue(key, out var n);
                    counts[key] = ++n;
                    if (n > best) { best = n; skin = c; found = true; }
                }
            return found ? skin : new Color32(0xe0, 0xb0, 0x88, 255);
        }
    }
}
