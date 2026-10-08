using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The breeze over the title picture: small, smooth, and only on foliage.
    public class TitleSwayTests
    {
        [Test]
        public void TheOffset_IsZeroForNoWeight_AndAlwaysSmall()
        {
            Assert.AreEqual(Vector2.zero, TitleSway.Offset(0.4f, 0.5f, 3f, 0f));
            var maxX = 0f; var maxY = 0f; var moved = false;
            for (var t = 0f; t < 60f; t += 0.37f)
                for (var u = 0f; u <= 1f; u += 0.1f)
                    for (var v = 0f; v <= 1f; v += 0.1f)
                    {
                        var o = TitleSway.Offset(u, v, t, 1f);
                        maxX = Mathf.Max(maxX, Mathf.Abs(o.x)); maxY = Mathf.Max(maxY, Mathf.Abs(o.y));
                        if (o.sqrMagnitude > 1e-9f) moved = true;
                    }
            Assert.IsTrue(moved, "the breeze moves things");
            Assert.LessOrEqual(maxX, TitleSway.AmplitudeX * 1.001f);
            Assert.LessOrEqual(maxY, TitleSway.AmplitudeY * 1.001f);
            Assert.Less(maxX, 0.003f, "a slight breeze: under a third of a percent of the picture's width");
        }

        [Test]
        public void TheOffset_ChangesSmoothlyWithTime()
        {
            var a = TitleSway.Offset(0.3f, 0.4f, 10f, 1f);
            var b = TitleSway.Offset(0.3f, 0.4f, 10.016f, 1f);          // one frame later
            Assert.Less((a - b).magnitude, TitleSway.AmplitudeX * 0.2f, "no jumps between frames");
        }

        [Test]
        public void Foliage_Sways_ButSkyWaterStoneAndTheBarnDoNot()
        {
            Assert.Greater(TitleSway.Weight(new Color(0.30f, 0.50f, 0.20f), 0.5f, 0.7f), 0.8f, "leaf green");
            Assert.Greater(TitleSway.Weight(new Color(0.85f, 0.45f, 0.12f), 0.05f, 0.5f), 0.7f, "an orange autumn tree");
            Assert.Greater(TitleSway.Weight(new Color(0.12f, 0.30f, 0.28f), 0.8f, 0.3f), 0.2f, "dark pine");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.35f, 0.25f, 0.60f), 0.5f, 0.1f), "the purple sky");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.95f, 0.65f, 0.25f), 0.3f, 0.3f), "the orange glow of the sunset");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.40f, 0.40f, 0.62f), 0.4f, 0.5f), "the water");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.50f, 0.50f, 0.50f), 0.4f, 0.6f), "grey stone");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.55f, 0.15f, 0.12f), 0.4f, 0.6f), "the red barn");
            Assert.AreEqual(0f, TitleSway.Weight(new Color(0.30f, 0.50f, 0.20f), 0.5f, 0.02f), "nothing sways at the very top edge");
        }

        [Test]
        public void ThePicture_IsInTheGame_Readable_AndMostlyStill_WithTheFoliageSwaying()
        {
            var picture = Resources.Load<Texture2D>(MainMenuScreen.PicturePath);
            Assert.IsNotNull(picture, "the title picture is under Resources/Title");
            Assert.IsTrue(picture.isReadable, "readable, so that the foliage can be found");
            Assert.AreEqual(1671, picture.width); Assert.AreEqual(941, picture.height);

            var w = TitleSway.Weights(picture.GetPixels32(), picture.width, picture.height, TitleBackdrop.Columns, TitleBackdrop.Rows);
            var stride = TitleBackdrop.Columns + 1;
            float At(float u, float vTop) => w[Mathf.RoundToInt((1f - vTop) * TitleBackdrop.Rows) * stride + Mathf.RoundToInt(u * TitleBackdrop.Columns)];
            var swaying = 0;
            foreach (var x in w) if (x > 0.2f) swaying++;
            var share = swaying / (float)w.Length;
            Assert.That(share, Is.InRange(0.12f, 0.65f), "some of the picture sways, not all of it (" + share + ")");
            Assert.AreEqual(0f, At(0.25f, 0.05f), "the sky at the top");
            Assert.AreEqual(0f, At(0f, 0.5f), "the left edge");
            Assert.AreEqual(0f, At(1f, 0.5f), "the right edge");
            Assert.Greater(At(0.10f, 0.92f), 0.2f, "the crops at the bottom left");
            Assert.Greater(At(0.04f, 0.45f), 0.2f, "the big autumn tree at the left");
        }
    }
}
