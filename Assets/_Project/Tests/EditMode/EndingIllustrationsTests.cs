using Farm.Core;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // X-010: each ending has its own illustration and caption.
    public class EndingIllustrationsTests
    {
        static readonly string[] Endings = { MythosEnding.Awakened, MythosEnding.Sealed, MythosEnding.Joined, MythosEnding.Ignored };

        [TestCaseSource(nameof(Endings))]
        public void EveryEnding_HasAnIllustration_At320By180_AndACaption(string ending)
        {
            var texture = Resources.Load<Texture2D>(MythosEnding.IllustrationPath(ending));
            Assert.IsNotNull(texture, ending);
            Assert.AreEqual(320, texture.width, ending);
            Assert.AreEqual(180, texture.height, ending);
            Assert.IsTrue(L.Has("mythos.ending." + ending + ".caption"), ending);
        }

        [Test]
        public void TheIllustrations_AreFourDifferentPictures()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            foreach (var ending in Endings)
            {
                var t = new Texture2D(2, 2);
                t.LoadImage(System.IO.File.ReadAllBytes(System.IO.Path.Combine(Application.dataPath, "_Project/Resources/" + MythosEnding.IllustrationPath(ending) + ".png")));
                var pixels = t.GetPixels32();
                var hash = 17;
                for (var i = 0; i < pixels.Length; i += 37) hash = hash * 31 + pixels[i].r * 7 + pixels[i].g * 3 + pixels[i].b;
                Assert.IsTrue(seen.Add(hash), ending + " differs from the others");
            }
        }
    }
}
