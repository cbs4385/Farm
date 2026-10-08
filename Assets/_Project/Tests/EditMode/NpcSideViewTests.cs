using System.IO;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-08: "several NPCs do not face the correct direction when moving". Their walking code was right; the pictures were not: for six villagers the
    // "left" picture showed them from the front, so walking sideways showed no direction. Every villager's left picture must be a side view (not a copy of the front
    // view, and not left-right symmetric like a front view is), and the right picture the mirror image of it.
    public class NpcSideViewTests
    {
        const string Folder = "Assets/_Project/Art/Placeholders/";

        static Color32[] Load(string id, string direction, out int width, out int height)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Assert.IsTrue(ImageConversion.LoadImage(texture, File.ReadAllBytes($"{Folder}npc_{id}_idle_{direction}.png")), id + " " + direction);
            width = texture.width; height = texture.height;
            return texture.GetPixels32();
        }

        // The mean difference between a picture and the same picture turned over, across the columns the character covers: near 0 for a front view.
        static float Asymmetry(Color32[] pixels, int width, int height)
        {
            int first = width, last = -1;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    if (pixels[y * width + x].a > 0) { first = Mathf.Min(first, x); last = Mathf.Max(last, x); }
            double total = 0; var count = 0;
            for (var y = 0; y < height; y++)
                for (var x = first; x <= last; x++)
                {
                    var a = pixels[y * width + x]; var b = pixels[y * width + (first + last - x)];
                    total += (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a)) / 4.0; count++;
                }
            return count == 0 ? 0f : (float)(total / count);
        }

        static float Difference(Color32[] a, Color32[] b)
        {
            double total = 0;
            for (var i = 0; i < a.Length; i++) total += (Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b) + Mathf.Abs(a[i].a - b[i].a)) / 4.0;
            return (float)(total / a.Length);
        }

        [Test]
        public void EveryVillagersLeftPicture_IsASideView_AndTheRightPictureItsMirror()
        {
            foreach (var id in NpcIds.All)
            {
                var down = Load(id, "down", out var w, out var h);
                var left = Load(id, "left", out var lw, out var lh);
                var right = Load(id, "right", out var rw, out var rh);
                Assert.AreEqual((w, h), (lw, lh)); Assert.AreEqual((w, h), (rw, rh));

                Assert.GreaterOrEqual(Difference(left, down), 12f, id + "'s left picture is a copy of the front view");
                Assert.GreaterOrEqual(Asymmetry(left, w, h), 10f, id + "'s left picture is as symmetric as a front view: it is not a side view");

                var mirrored = new Color32[left.Length];
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++) mirrored[y * w + x] = left[y * w + (w - 1 - x)];
                Assert.AreEqual(0f, Difference(right, mirrored), 0.001f, id + "'s right picture is the left one turned over");
            }
        }
    }
}
