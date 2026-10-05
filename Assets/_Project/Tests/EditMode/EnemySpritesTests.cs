using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest report (2026-10-05): "the art for what lives in the mines is not done."
    public class EnemySpritesTests
    {
        [Test]
        public void EveryMonster_HasTwoFramesOfArt_ThatFitItsCanvas()
        {
            foreach (var row in EnemyDefaults.Rows)
            {
                Assert.IsTrue(EnemySprites.All.TryGetValue(row.Id, out var art), row.Id + " has art");
                Assert.AreEqual(row.Boss ? 20 : 16, art.Size, row.Id + " canvas");
                Assert.AreEqual(2, art.Frames.Length, row.Id + " frames");
                foreach (var frame in art.Frames)
                {
                    Assert.LessOrEqual(frame.Length, art.Size, row.Id + " is not taller than its canvas");
                    foreach (var line in frame)
                    {
                        Assert.AreEqual(art.Size / 2, line.Length, row.Id + " row is half the canvas wide: " + line);
                        foreach (var c in line) Assert.IsTrue(c == '.' || art.Palette.ContainsKey(c), row.Id + " uses an unknown colour " + c);
                    }
                }
            }
        }

        [Test]
        public void TheFramesDiffer_TheyAreMirrorImages_AndTheFlashIsAWhiteSilhouette()
        {
            foreach (var art in EnemySprites.All.Values)
            {
                var a = EnemySprites.Pixels(art, 0, false);
                var b = EnemySprites.Pixels(art, 1, false);
                Assert.IsTrue(a.Where((p, i) => !p.Equals(b[i])).Any(), "the two frames are not the same picture");
                var size = art.Size;
                for (var y = 0; y < size; y++)
                    for (var x = 0; x < size / 2; x++)
                        Assert.AreEqual(a[y * size + x], a[y * size + size - 1 - x], "left and right halves mirror");
                var flash = EnemySprites.Pixels(art, 0, true);
                for (var i = 0; i < a.Length; i++)
                {
                    Assert.AreEqual(a[i].a, flash[i].a, "the silhouette covers the same pixels");
                    if (flash[i].a > 0) Assert.AreEqual(new Color32(255, 255, 255, 255), flash[i]);
                }
            }
        }

        [Test]
        public void TheMonsterSitsOnTheBottomEdge_AndTheSpritesAreBuilt()
        {
            foreach (var row in EnemyDefaults.Rows)
            {
                var frames = EnemySprites.Frames(row.Id);
                Assert.AreEqual(3, frames.Length, row.Id);
                Assert.AreEqual(row.Boss ? 20 : 16, (int)frames[0].rect.width);
                Assert.AreSame(frames, EnemySprites.Frames(row.Id), "built once");
            }
            Assert.IsNull(EnemySprites.Frames("no_such_monster"));
            Assert.AreEqual(0, EnemySprites.FrameAt(0f, 0f));
            Assert.AreEqual(1, EnemySprites.FrameAt(EnemySprites.FrameSeconds * 1.5f, 0f));
            Assert.AreEqual(1, EnemySprites.FrameAt(0f, EnemySprites.FrameSeconds), "an offset puts a crowd out of step");
        }
    }
}
