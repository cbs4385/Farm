using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The rig that animates every character picture (walk and tool swings), the look pass (outline, light) and the shadow. Playtest 2026-10-10: "the avatars look flat".
    public class CharacterRigTests
    {
        static readonly Color32 Skin = new Color32(0xe0, 0xb0, 0x88, 255), Shirt = new Color32(0x3a, 0x70, 0x90, 255), Pants = new Color32(0x6a, 0x40, 0x28, 255), Hair = new Color32(0x50, 0x30, 0x20, 255);

        // A 16 x 32 figure: hair and a face, a shirt with arms at the sides, two legs.
        static PixelGrid Figure()
        {
            var g = new PixelGrid(16, 32);
            for (var y = 2; y < 13; y++) for (var x = 3; x < 13; x++) g.Set(x, y, y < 5 ? Hair : Skin);
            for (var y = 13; y < 23; y++) for (var x = 3; x < 13; x++) g.Set(x, y, Shirt);
            for (var y = 23; y < 31; y++) { for (var x = 4; x < 7; x++) g.Set(x, y, Pants); for (var x = 9; x < 12; x++) g.Set(x, y, Pants); }
            return g;
        }

        static string Same(PixelGrid a, PixelGrid b) => a.P.SequenceEqual(b.P, new ColorEquality()) ? "same" : "different";

        sealed class ColorEquality : System.Collections.Generic.IEqualityComparer<Color32>
        {
            public bool Equals(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            public int GetHashCode(Color32 c) => c.r | c.g << 8 | c.b << 16 | c.a << 24;
        }

        [TestCase("npc_bram_idle_down", "down")]
        [TestCase("avatar_left", "left")]
        [TestCase("player_idle_up", "up")]
        [TestCase("npc_juno_idle_right", "right")]
        [TestCase("npc_juno_pose_wave", null)]
        public void The_facing_comes_from_the_end_of_the_name(string name, string facing) => Assert.AreEqual(facing, CharacterRig.FacingOf(name));

        [Test]
        public void Standing_is_the_picture_on_a_bigger_canvas_with_nothing_lost()
        {
            var g = Figure();
            var idle = CharacterRig.Idle(g, "down", false);
            Assert.AreEqual(CharacterRig.OutWidth(16), idle.W);
            Assert.AreEqual(CharacterRig.OutHeight(32), idle.H);
            Assert.AreEqual(g.Count(), idle.Count());
            Assert.AreEqual(g.Get(5, 25).r, idle.Get(5 + CharacterRig.PadX, 25 + CharacterRig.PadTop).r, "every pixel is where it was, on the bigger canvas");
        }

        [TestCase("down")]
        [TestCase("up")]
        [TestCase("left")]
        [TestCase("right")]
        public void A_walk_is_four_different_frames_with_the_feet_on_the_ground(string facing)
        {
            var g = Figure();
            var frames = Enumerable.Range(0, 4).Select(p => CharacterRig.Walk(g, facing, p)).ToList();
            Assert.AreEqual("different", Same(frames[0], frames[1]), "a step differs from passing");
            Assert.AreEqual("different", Same(frames[2], frames[3]), "and so does the other");
            Assert.AreEqual("different", Same(frames[0], frames[2]), "left and right steps differ");
            var (_, floor) = CharacterRig.Idle(g, facing, false).Rows();
            foreach (var f in frames)
            {
                var (top, bottom) = f.Rows();
                Assert.LessOrEqual(bottom, floor, "a foot never goes below the floor");
                Assert.GreaterOrEqual(bottom, floor - 1, "and the feet stay on it (at most a raised foot)");
            }
            Assert.Less(frames[1].Rows().top, frames[0].Rows().top, "passing, the body is a pixel higher");
        }

        [Test]
        public void Breathing_raises_the_body_and_leaves_the_feet()
        {
            var g = Figure();
            var calm = CharacterRig.Idle(g, "down", false);
            var breath = CharacterRig.Idle(g, "down", true);
            Assert.Less(breath.Rows().top, calm.Rows().top);
            Assert.AreEqual(calm.Rows().bottom, breath.Rows().bottom);
        }

        [Test]
        public void Every_tool_has_a_picture_to_swing()
        {
            foreach (var tool in new[] { ToolType.Hoe, ToolType.WateringCan, ToolType.Axe, ToolType.Pickaxe, ToolType.Scythe, ToolType.Rod, ToolType.Sword, ToolType.Hammer })
                foreach (var angle in new[] { 0f, 90f, 180f, 270f, 305f })
                    Assert.Greater(ToolGlyphs.For(tool, angle).Count, 6, $"{tool} at {angle}");
        }

        [TestCase("down")]
        [TestCase("up")]
        [TestCase("left")]
        [TestCase("right")]
        public void A_swing_is_four_frames_that_show_the_tool_and_change_with_the_tool(string facing)
        {
            var g = Figure();
            var idle = CharacterRig.Idle(g, facing, false);
            var hoe = Enumerable.Range(0, 4).Select(f => CharacterRig.Strike(g, facing, ToolType.Hoe, f)).ToList();
            var hammer = Enumerable.Range(0, 4).Select(f => CharacterRig.Strike(g, facing, ToolType.Hammer, f)).ToList();
            for (var f = 0; f < 4; f++)
            {
                Assert.Greater(hoe[f].Count(), idle.Count() - 40, $"frame {f} still has a body");
                Assert.AreEqual("different", Same(hoe[f], idle), $"frame {f} is not the standing picture");
                Assert.AreEqual("different", Same(hoe[f], hammer[f]), $"frame {f} shows the tool that was asked for");
            }
            Assert.AreEqual("different", Same(hoe[0], hoe[2]), "wind-up and strike differ");
        }

        [Test]
        public void The_look_puts_an_outline_round_the_figure_and_lights_one_side()
        {
            var plain = CharacterRig.Idle(Figure(), "down", false);
            var looked = CharacterLook.Apply(plain);
            Assert.Greater(looked.Count(), plain.Count(), "the outline adds pixels");
            Assert.IsFalse(plain.Opaque(CharacterRig.PadX + 2, CharacterRig.PadTop + 8));
            Assert.IsTrue(looked.Opaque(CharacterRig.PadX + 2, CharacterRig.PadTop + 8), "outside the left edge of the face is now ink");
            Assert.Greater(CharacterLook.Luma(looked.Get(CharacterRig.PadX + 5, CharacterRig.PadTop + 2)), CharacterLook.Luma(plain.Get(CharacterRig.PadX + 5, CharacterRig.PadTop + 2)), "the top edge of the hair is lit");
        }

        [Test]
        public void A_shadow_is_a_soft_oval()
        {
            var shadow = CharacterLook.Shadow(12);
            Assert.AreEqual(12, shadow.W);
            Assert.AreEqual(4, shadow.H);
            Assert.IsTrue(shadow.Opaque(6, 2));
            Assert.IsFalse(shadow.Opaque(0, 0), "the corners are clear");
            Assert.AreEqual(6, ActorShadow.WidthFor(8), "never narrower than 6");
            Assert.AreEqual(12, ActorShadow.WidthFor(16));
        }

        [Test]
        public void Every_villager_and_the_farmer_can_walk_and_swing_in_every_direction()
        {
            var dir = Path.Combine(Application.dataPath, "_Project", "Art", "Placeholders");
            var names = Directory.GetFiles(dir, "*_idle_*.png").Select(Path.GetFileNameWithoutExtension).Where(n => CharacterRig.FacingOf(n) != null).ToList();
            Assert.GreaterOrEqual(names.Count, 13 * 4, "twelve villagers and the farmer, four facings each");
            foreach (var name in names)
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(File.ReadAllBytes(Path.Combine(dir, name + ".png")));
                var g = PixelGrid.FromTexture(tex.GetPixels32(), tex.width, tex.height);
                Object.DestroyImmediate(tex);
                var facing = CharacterRig.FacingOf(name);
                Assert.IsTrue(CharacterRig.TryBands(g, out _), name + " has a head, a body and legs the rig can find");
                for (var p = 0; p < 4; p++) Assert.Greater(CharacterRig.Walk(g, facing, p).Count(), g.Count() - 24, $"{name} walk {p}");
                for (var f = 0; f < 4; f++) Assert.Greater(CharacterRig.Strike(g, facing, ToolType.Axe, f).Count(), g.Count() - 40, $"{name} swing {f}");
            }
        }

        [Test]
        public void Villagers_with_a_trade_work_with_its_tool_and_the_rest_have_none()
        {
            Assert.AreEqual(ToolType.Hammer, NpcActor.WorkToolOf(NpcRoster.Juno));
            Assert.AreEqual(ToolType.Hammer, NpcActor.WorkToolOf(NpcRoster.Marcus));
            Assert.AreEqual(ToolType.Rod, NpcActor.WorkToolOf(NpcRoster.Felix));
            Assert.AreEqual(ToolType.None, NpcActor.WorkToolOf(NpcRoster.Wren));
        }

        // Playtest 2026-10-10: "the tool animations do not reach the tile being acted upon".
        [Test]
        public void On_the_strike_the_tool_lands_on_the_middle_of_the_tile_aimed_at_for_all_eight_neighbours()
        {
            var g = Figure();
            var worst = 0f; var worstAt = "";
            foreach (var facing in new[] { "down", "up", "left", "right" })
                foreach (var tool in new[] { ToolType.Hoe, ToolType.Axe, ToolType.Pickaxe, ToolType.Scythe, ToolType.Hammer, ToolType.Sword })
                    foreach (var target in new[] { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) })
                    {
                        var (_, aimed, tip) = CharacterRig.StrikeGeometry(g, facing, tool, target);
                        var gap = Vector2.Distance(aimed, tip);
                        if (gap > worst) { worst = gap; worstAt = $"{facing} {tool} {target}"; }
                    }
            Assert.LessOrEqual(worst, 5.5f, "the tool's end is within a third of a tile of the aimed tile's middle (worst: " + worstAt + ")");
        }

        [Test]
        public void A_swing_at_a_diagonal_tile_differs_from_one_straight_ahead()
        {
            var g = Figure();
            var straight = CharacterRig.Strike(g, "down", ToolType.Hoe, 2, new Vector2Int(0, -1));
            var diagonal = CharacterRig.Strike(g, "down", ToolType.Hoe, 2, new Vector2Int(1, -1));
            Assert.AreEqual("different", Same(straight, diagonal));
        }

        [Test]
        public void The_frame_canvas_has_room_for_the_tool_below_and_beside_the_feet()
        {
            Assert.GreaterOrEqual(CharacterRig.PadBottom, CharacterRig.CellPixels + 4);
            Assert.GreaterOrEqual(CharacterRig.PadX, CharacterRig.CellPixels);
        }
    }
}
