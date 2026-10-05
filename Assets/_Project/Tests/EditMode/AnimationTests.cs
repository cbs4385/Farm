using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest comment (2026-10-05): "there are no animations". Walkers hop a pixel in step, flags and vanes flip, chimneys smoke.
    public class AnimationTests
    {
        static Sprite Make(Vector2 pivot)
        {
            var tex = new Texture2D(16, 32) { filterMode = FilterMode.Point };
            return Sprite.Create(tex, new Rect(0, 0, 16, 32), pivot, 16f);
        }

        [Test]
        public void AWalk_AlternatesUpAndDown_AtASteadyBeat()
        {
            Assert.IsFalse(WalkBob.UpAt(0f));
            Assert.IsTrue(WalkBob.UpAt(WalkBob.StepSeconds * 1.2f));
            Assert.IsFalse(WalkBob.UpAt(WalkBob.StepSeconds * 2.2f));
            Assert.IsTrue(WalkBob.UpAt(WalkBob.StepSeconds * 3.2f));
        }

        [Test]
        public void TheRaisedPicture_IsTheSameTextureOnePixelHigher_AndIsNotRaisedAgain()
        {
            var sprite = Make(new Vector2(0.5f, 0f));
            var up = WalkBob.Raised(sprite);
            Assert.AreSame(sprite.texture, up.texture);
            Assert.AreEqual(sprite.rect, up.rect);
            Assert.AreEqual(sprite.pivot.y - 1f, up.pivot.y, 0.001f, "the pivot is a pixel lower, so the picture sits a pixel higher");
            Assert.AreEqual(sprite.pivot.x, up.pivot.x, 0.001f);
            Assert.AreSame(up, WalkBob.Raised(sprite), "cached");
            Assert.AreSame(up, WalkBob.Raised(up), "a raised picture stays as it is");
            Assert.IsNull(WalkBob.Raised(null));
        }

        [Test]
        public void TheFrameClock_CyclesThroughTheFrames()
        {
            Assert.AreEqual(0, FrameAnimator.FrameAt(0f, 0.5f, 0f, 2));
            Assert.AreEqual(1, FrameAnimator.FrameAt(0.6f, 0.5f, 0f, 2));
            Assert.AreEqual(0, FrameAnimator.FrameAt(1.1f, 0.5f, 0f, 2));
            Assert.AreEqual(1, FrameAnimator.FrameAt(0f, 0.5f, 0.5f, 2), "an offset starts it out of step with its neighbours");
            Assert.AreEqual(0, FrameAnimator.FrameAt(5f, 0.5f, 0f, 0));
        }

        [Test]
        public void ThePuffsOfSmoke_AreSpreadAlongTheirRise_AndKeepRepeating()
        {
            var a = RoofSmoke.Progress(1f, 0); var b = RoofSmoke.Progress(1f, 1);
            Assert.AreNotEqual(a, b);
            for (var t = 0f; t < 20f; t += 0.37f)
                for (var i = 0; i < 4; i++) Assert.That(RoofSmoke.Progress(t, i), Is.InRange(0f, 1f));
        }

        [Test]
        public void ASwing_PushesTheFarmerOutAndBack_AndEndsAtRest()
        {
            Assert.AreEqual(0, WalkBob.LungeOffset(0f));
            Assert.AreEqual(2, WalkBob.LungeOffset(0.2f));
            Assert.AreEqual(1, WalkBob.LungeOffset(0.7f));
            Assert.AreEqual(0, WalkBob.LungeOffset(1f));
        }

        [Test]
        public void AShiftedPicture_MovesSidewaysAndUp_AndIsRecognisedAsShifted()
        {
            var sprite = Make(new Vector2(0.5f, 0f));
            var right = WalkBob.Shifted(sprite, 2, 0);
            Assert.AreEqual(sprite.pivot.x - 2f, right.pivot.x, 0.001f);
            Assert.AreSame(right, WalkBob.Shifted(sprite, 2, 0), "cached");
            Assert.AreSame(sprite, WalkBob.Shifted(sprite, 0, 0));
            Assert.AreSame(right, WalkBob.Shifted(right, 1, 1), "a shifted picture is never shifted again");
        }
    }
}
