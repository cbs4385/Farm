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

        [Test]
        public void ABreath_RisesForItsSecondHalf_AndNeighboursAreOutOfStep()
        {
            Assert.IsFalse(WalkBob.BreathUp(0.1f, 0f));
            Assert.IsTrue(WalkBob.BreathUp(WalkBob.BreathSeconds * 0.75f, 0f));
            Assert.IsFalse(WalkBob.BreathUp(WalkBob.BreathSeconds * 1.1f, 0f), "and it repeats");
            Assert.AreNotEqual(WalkBob.BreathUp(0.1f, 0f), WalkBob.BreathUp(0.1f, WalkBob.BreathSeconds * 0.5f));
        }

        [Test]
        public void ThePuffSpecks_ArcUpThenFall_FadeOut_AndFanAcross()
        {
            var (start, a0) = ActionPuff.BitAt(0f, 0, 5);
            Assert.AreEqual(Vector2.zero, start);
            Assert.AreEqual(1f, a0, 0.001f);
            Assert.Greater(ActionPuff.BitAt(0.3f, 2, 5).offset.y, 0f, "rising early on");
            Assert.Less(ActionPuff.BitAt(1f, 2, 5).offset.y, ActionPuff.BitAt(0.5f, 2, 5).offset.y, "falling by the end");
            Assert.AreEqual(0f, ActionPuff.BitAt(1f, 2, 5).alpha, 0.001f);
            Assert.Less(ActionPuff.BitAt(0.5f, 0, 5).offset.x, 0f);
            Assert.Greater(ActionPuff.BitAt(0.5f, 4, 5).offset.x, 0f);
            Assert.DoesNotThrow(() => ActionPuff.BitAt(0.5f, 0, 1));
        }

        [Test]
        public void TheWind_LeansPlantsInWholePixels_InAWaveAcrossTheMap()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var t = 0f; t < Sway.Period; t += 0.05f) seen.Add(Sway.LeanPixels(t, 5, 5));
            CollectionAssert.AreEquivalent(new[] { -1, 0, 1 }, seen, "a breath goes left, upright, right");
            var differs = false;
            for (var x = 0; x < 8; x++) if (Sway.LeanPixels(0.3f, x, 0) != Sway.LeanPixels(0.3f, 0, 0)) differs = true;
            Assert.IsTrue(differs, "neighbours are out of step");
            Assert.AreEqual(0, Sway.LeanPixels(0.3f, 4, 4, 0f), "dead calm");
        }

        [Test]
        public void ALeanShear_MovesTheTopButNotTheBase()
        {
            var m = Sway.Shear(1);
            var bottom = m.MultiplyPoint3x4(new Vector3(0f, -0.5f, 0f));
            var top = m.MultiplyPoint3x4(new Vector3(0f, 0.5f, 0f));
            Assert.AreEqual(0f, bottom.x, 0.0001f, "the base stays");
            Assert.AreEqual(1f / 16f, top.x, 0.0001f, "the top leans a pixel");
            Assert.AreEqual(Matrix4x4.identity, Sway.Shear(0));
            Assert.AreEqual(0f, Sway.AngleFor(0));
            Assert.AreEqual(-Sway.AngleFor(1), Sway.AngleFor(-1), 0.0001f);
        }

        [Test]
        public void TheWind_BlowsHarderInStormsAndSlantingRain_ThanOnAClearDay()
        {
            var clear = Sway.StrengthFor(0f, false);
            Assert.AreEqual(0.8f, clear, 0.001f);
            Assert.Greater(Sway.StrengthFor(0.5f, false), clear);
            Assert.Greater(Sway.StrengthFor(0.5f, true), Sway.StrengthFor(0.5f, false));
            Assert.LessOrEqual(Sway.StrengthFor(1f, true), 1.5f, "capped");
            Assert.AreEqual(Sway.StrengthFor(-0.4f, false), Sway.StrengthFor(0.4f, false), 0.001f, "either direction");
            Assert.AreEqual(1f, Sway.Current(), 0.001f, "a breeze when no game is running");
        }

        [Test]
        public void APleasedAnimalOrVillager_GetsPinkSpecksAndAHop()
        {
            var go = new GameObject("pleased");
            go.AddComponent<SpriteRenderer>();
            var bob = go.AddComponent<WalkBob>();
            var before = Object.FindObjectsByType<ActionPuff>(FindObjectsSortMode.None).Length;
            ActionPuff.Hearts(Vector3.zero, bob);
            var puffs = Object.FindObjectsByType<ActionPuff>(FindObjectsSortMode.None);
            Assert.AreEqual(before + 1, puffs.Length);
            Assert.IsTrue(bob.IsLunging, "it hops");
            foreach (var p in puffs) Object.DestroyImmediate(p.gameObject);
            Object.DestroyImmediate(go);
        }
    }
}
