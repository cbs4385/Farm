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
        public const int PadX = 16, PadTop = 10, PadBottom = 20;          // room for a tool that reaches the tile beside or in front of the feet (a cell is 16 pixels)
        public const int CellPixels = 16;
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
            public int ShiftX, ShiftY;          // the whole figure, feet and all (a lunge toward a target)
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
        // `target` is the tile the swing is aimed at, in cells from the character's feet (x to the right, y up: any of the eight around them, or two cells away): on
        // the strike frame the tool's working end lands on the middle of that tile (the body leans and the arm stretches to get it there).
        public static PixelGrid Strike(PixelGrid g, string facing, ToolType tool, int frame, Vector2Int target)
        {
            if (!TryBands(g, out var b)) return Plain(g);
            frame = Mathf.Clamp(frame, 0, StrikeFrames - 1);
            target = new Vector2Int(Mathf.Clamp(target.x, -2, 2), Mathf.Clamp(target.y, -2, 2));
            if (target == Vector2Int.zero) target = FacingVector(facing);
            var pose = StrikePose(frame, target);
            var body = Build(g, b, pose, facing);
            DrawArmAndTool(body, g, b, facing, tool, frame, pose, target);
            return body;
        }

        public static PixelGrid Strike(PixelGrid g, string facing, ToolType tool, int frame) => Strike(g, facing, tool, frame, FacingVector(facing));

        public static Vector2Int FacingVector(string facing) => facing == "up" ? Vector2Int.up : facing == "left" ? Vector2Int.left : facing == "right" ? Vector2Int.right : Vector2Int.down;

        // How the body leans over a swing: back and up to wind up, then toward the target (a crouch for a tile below, a step for a tile to the side) on the strike.
        static Pose StrikePose(int frame, Vector2Int target)
        {
            var pose = new Pose { NoArms = true };
            if (frame == 0) { pose.BodyDy = -1; pose.BodyDx = -Mathf.Clamp(target.x, -1, 1); return pose; }
            var amount = frame == 2 ? 1f : frame == 3 ? 0.5f : 0f;
            pose.BodyDx = Mathf.RoundToInt(Mathf.Clamp(target.x, -2, 2) * 1.5f * amount);
            pose.BodyDy = target.y < 0 ? Mathf.RoundToInt(Mathf.Min(-target.y, 2) * 2f * amount) : 0;
            // a step toward the target: the whole figure, feet included, moves over (its shadow stays where the farmer is)
            pose.ShiftX = Mathf.RoundToInt(Mathf.Clamp(target.x, -1, 1) * 4f * amount);
            pose.ShiftY = target.y < 0 ? Mathf.RoundToInt(4f * amount) : 0;
            if (frame == 2 && target.y < 0) pose.HeadDy = 1;
            return pose;
        }

        // Where the shoulder and the middle of the tile aimed at are on the frame canvas (rows grow downward), and the tip of the tool when the strike lands. For tests.
        public static (Vector2 shoulder, Vector2 targetPixel, Vector2 tip) StrikeGeometry(PixelGrid g, string facing, ToolType tool, Vector2Int target)
        {
            if (!TryBands(g, out var b)) return default;
            target = new Vector2Int(Mathf.Clamp(target.x, -2, 2), Mathf.Clamp(target.y, -2, 2));
            if (target == Vector2Int.zero) target = FacingVector(facing);
            var pose = StrikePose(2, target);
            var shoulder = ShoulderOf(b, facing, pose);
            var targetPixel = TargetPixel(g, b, target);
            var (hand, angle, stretch) = HitHand(shoulder, targetPixel, FeetPixel(g, b), tool);
            var rad = angle * Mathf.Deg2Rad;
            return (shoulder, targetPixel, hand + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * ToolGlyphs.StretchedLength(tool, stretch));
        }

        // The middle of a tile, `target` cells from the feet, on the bigger canvas. The feet are on the bottom edge of the picture, in the middle of a cell.
        static Vector2 TargetPixel(PixelGrid g, Bands b, Vector2Int target) =>
            new Vector2(PadX + g.W / 2f + target.x * CellPixels, PadTop + b.Bottom + 1 - target.y * CellPixels);

        // The middle of the feet (where the tile the farmer stands on has its middle) on the bigger canvas.
        static Vector2 FeetPixel(PixelGrid g, Bands b) => new Vector2(PadX + g.W / 2f, PadTop + b.Bottom + 1);

        static Vector2 ShoulderOf(Bands b, string facing, Pose pose)
        {
            var cx = PadX + (b.Left + b.Right + 1) / 2f + pose.BodyDx + pose.ShiftX;
            var side = facing == "left" || facing == "right";
            var x = side ? cx + (facing == "right" ? 1 : -1) : cx + 4;
            return new Vector2(x, PadTop + b.HeadEnd + (side ? 2 : 1) + pose.BodyDy + pose.ShiftY);
        }

        // Where the hand and the tool go for the strike: along the line from the shoulder to the target, the arm stretching (to ten pixels) so that the tool's end lands on it.
        static (Vector2 hand, float angle, float stretch) HitHand(Vector2 shoulder, Vector2 targetPixel, Vector2 feet, ToolType tool)
        {
            var v = targetPixel - shoulder;
            var length = ToolGlyphs.Length(tool);
            if (v.magnitude < length + 3f)
            {
                // a tile up beside the shoulder: the tool points from the hip toward it (as seen from the feet) and ends on it
                var toward = (targetPixel - feet).sqrMagnitude < 0.01f ? Vector2.down : (targetPixel - feet).normalized;
                var held = targetPixel - toward * length;
                var offset = held - shoulder;
                if (offset.magnitude > 10f) held = shoulder + offset.normalized * 10f;
                return (held, Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg, 1f);
            }
            var arm = Mathf.Clamp(v.magnitude - length, 3f, 10f);
            var dir = v.sqrMagnitude < 0.01f ? Vector2.down : v.normalized;
            // past what the arm can stretch the handle is drawn longer, up to twice as long
            var missing = Mathf.Max(0f, v.magnitude - arm - length);
            var stretch = Mathf.Clamp(1f + missing / Mathf.Max(1f, length - 2f), 1f, 2f);
            return (shoulder + dir * arm, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, stretch);
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
                    o.Set(x + PadX + dx + pose.ShiftX, y + PadTop + dy + pose.ShiftY, c);
                }

            // torso (the arms are carried separately when they swing)
            for (var y = b.HeadEnd; y < b.TorsoEnd; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    if (armsMove && y >= b.HeadEnd + 2 && ((x >= armColumnsLeft.Item1 && x <= armColumnsLeft.Item2) || (x >= armColumnsRight.Item1 && x <= armColumnsRight.Item2))) continue;
                    o.Set(x + PadX + pose.BodyDx + pose.ShiftX, y + PadTop + pose.BodyDy + pose.ShiftY, c);
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
                        o.Set(x + PadX + pose.BodyDx + pose.ShiftX, y + PadTop + pose.BodyDy + pose.ShiftY + dy, c);
                    }
                // a swung arm leaves a cell behind: it takes the colour of the torso beside it
                for (var y = b.HeadEnd + 2; y < b.TorsoEnd; y++)
                    foreach (var (x0, inner) in new[] { (armColumnsLeft.Item1, 1), (armColumnsRight.Item2, -1) })
                    {
                        var ox = x0 + PadX + pose.BodyDx + pose.ShiftX;
                        var oy = y + PadTop + pose.BodyDy + pose.ShiftY;
                        if (!o.Opaque(ox, oy) && g.Opaque(x0, y)) o.Set(ox, oy, o.Get(ox + inner * 2, oy));
                    }
            }

            // head
            for (var y = b.Top; y < b.HeadEnd; y++)
                for (var x = 0; x < g.W; x++)
                {
                    var c = g.Get(x, y);
                    if (c.a == 0) continue;
                    o.Set(x + PadX + pose.BodyDx + pose.HeadDx + pose.ShiftX, y + PadTop + pose.BodyDy + pose.HeadDy + pose.ShiftY, c);
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

        static void DrawArmAndTool(PixelGrid o, PixelGrid g, Bands b, string facing, ToolType tool, int frame, Pose pose, Vector2Int target)
        {
            var mirror = facing == "right";
            var key = mirror ? "left" : facing;
            if (!Hands.TryGetValue(key, out var table)) return;
            var (hx, hy, angle) = table[frame];
            var shoulderF = ShoulderOf(b, facing, pose);
            var shoulder = new Vector2Int(Mathf.RoundToInt(shoulderF.x), Mathf.RoundToInt(shoulderF.y));
            if (mirror) { hx = -hx; angle = 180 - angle; }
            var hand = new Vector2Int(shoulder.x + hx, shoulder.y + hy);
            var stretch = 1f;
            if (frame >= 2)
            {
                // the strike: the tool's end on the middle of the tile aimed at; recovering, drawn back along the same line
                var (hitHand, hitAngle, hitStretch) = HitHand(shoulderF, TargetPixel(g, b, target), FeetPixel(g, b), tool);
                stretch = frame == 2 ? hitStretch : Mathf.Lerp(1f, hitStretch, 0.55f);
                var back = frame == 2 ? 1f : 0.55f;
                hand = new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(shoulderF.x, hitHand.x, back)), Mathf.RoundToInt(Mathf.Lerp(shoulderF.y, hitHand.y, back)));
                angle = Mathf.RoundToInt(frame == 2 ? hitAngle : hitAngle - 25f);
            }
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
            foreach (var dot in ToolGlyphs.For(tool, angle, stretch)) o.Set(hand.x + dot.X, hand.y + dot.Y, dot.Color);
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
