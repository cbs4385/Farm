using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class CropArtTests
    {
        const string Dir = "Assets/_Project/Art/Placeholders";

        [Test]
        public void EveryCropStage_IsClearlyVisible()
        {
            // A freshly planted crop must be visible: guard against near-empty stage sprites.
            var files = Directory.GetFiles(Dir, "crop_*.png");
            Assert.IsNotEmpty(files);
            foreach (var path in files)
            {
                var tex = new Texture2D(2, 2);
                Assert.IsTrue(ImageConversion.LoadImage(tex, File.ReadAllBytes(path)), path);
                var opaque = tex.GetPixels32().Count(c => c.a > 0);
                Object.DestroyImmediate(tex);
                Assert.GreaterOrEqual(opaque, 20, $"{path} has only {opaque} visible pixels");
            }
        }
    }
}
