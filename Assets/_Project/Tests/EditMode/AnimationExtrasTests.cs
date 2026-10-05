using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The rest of the animation set: a walk cycle for the farmer and villagers, animal idles, glints on the water and a door that swings open.
    public class AnimationExtrasTests
    {
        // A 6 x 8 figure: opaque from the second row up, with two legs (columns 1 and 4) going down to the floor.
        static Color32[] Figure()
        {
            const int w = 6, h = 8;
            var px = new Color32[w * h];
            var white = new Color32(255, 255, 255, 255);
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var body = y >= 3 && y <= 6 && x >= 1 && x <= 4;
                    var leg = y >= 0 && y <= 2 && (x == 1 || x == 4);
                    if (body || leg) px[y * w + x] = white;
                }
            return px;
        }

        [Test]
        public void LiftingAFoot_ShortensOneLeg_AndLeavesTheOtherAndTheBodyAlone()
        {
            const int w = 6, h = 8;
            var figure = Figure();
            var left = StepFrames.Lift(figure, w, h, true);
            Assert.AreEqual(0, left[0 * w + 1].a, "the left foot's bottom pixel is gone");
            Assert.AreEqual(255, left[1 * w + 1].a, "the leg is still there above it");
            Assert.AreEqual(255, left[0 * w + 4].a, "the right foot stays on the floor");
            for (var y = 3; y <= 6; y++) Assert.AreEqual(255, left[y * w + 2].a, "the body is untouched");

            var right = StepFrames.Lift(figure, w, h, false);
            Assert.AreEqual(255, right[0 * w + 1].a, "now the left foot is down");
            Assert.AreEqual(0, right[0 * w + 4].a, "and the right foot is up");
            CollectionAssert.AreNotEqual(left, right);
            CollectionAssert.AreNotEqual(figure, left);
        }

        [Test]
        public void LiftingAnEmptyPicture_ChangesNothing()
        {
            var empty = new Color32[4 * 4];
            CollectionAssert.AreEqual(empty, StepFrames.Lift(empty, 4, 4, true));
        }

        [Test]
        public void AnyReadablePicture_GetsTwoStepFrames_ThatAreBuiltOnceAndKnowTheirOriginal()
        {
            var tex = new Texture2D(6, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(Figure());
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 6, 8), new Vector2(0.5f, 0f), 16f);
            sprite.name = "figure";
            StepFrames.ClearCache();

            var frames = StepFrames.For(sprite);
            Assert.IsNotNull(frames);
            Assert.AreEqual("figure_stepL", frames[0].name);
            Assert.AreEqual("figure_stepR", frames[1].name);
            Assert.AreSame(frames, StepFrames.For(sprite), "built once");
            Assert.AreEqual(sprite.pivot, frames[0].pivot, "they stand where the original stands");
            Assert.IsTrue(StepFrames.TryGetOriginal(frames[1], out var original));
            Assert.AreSame(sprite, original);
            Assert.AreSame(frames, StepFrames.For(frames[0]), "asked about a frame, it answers for the original");
            Assert.IsNull(StepFrames.For(null));
            Object.DestroyImmediate(tex);
        }

        [Test]
        public void TheWalkCycle_StepsLeft_Passes_StepsRight_Passes()
        {
            // The hop (WalkBob.UpAt) is up on the odd steps, so the foot frames fall on the even ones.
            Assert.IsFalse(WalkBob.UpAt(0f));
            Assert.IsTrue(WalkBob.UpAt(WalkBob.StepSeconds * 1.1f));
            Assert.IsFalse(WalkBob.UpAt(WalkBob.StepSeconds * 2.1f));
            Assert.IsTrue(WalkBob.UpAt(WalkBob.StepSeconds * 3.1f));
        }

        [Test]
        public void AnAnimalThatStandsStill_GrazesOrFidgets_ButOneThatWalksOrSleepsDoesNot()
        {
            Assert.AreEqual(("eat0", false), AnimalSprites.Pick(Vector2Int.down, false, false, false, 0f, AnimalSprites.Graze));
            Assert.AreEqual(("idle0", false), AnimalSprites.Pick(Vector2Int.down, false, false, false, 0f, AnimalSprites.Fidget));
            Assert.AreEqual(("idle1", false), AnimalSprites.Pick(Vector2Int.down, false, false, false, AnimalSprites.FidgetFrameSeconds * 1.1f, AnimalSprites.Fidget));
            Assert.AreEqual(("down0", false), AnimalSprites.Pick(Vector2Int.down, false, false, false, 0f, AnimalSprites.NoIdle));
            Assert.AreEqual(("left0", true), AnimalSprites.Pick(Vector2Int.right, true, false, false, 0f, AnimalSprites.Fidget), "walking wins over a fidget");
            Assert.AreEqual(("sleep", false), AnimalSprites.Pick(Vector2Int.left, false, false, true, 0f, AnimalSprites.Graze), "asleep wins over everything");
        }

        [Test]
        public void EveryAnimal_HasTwoIdleFrames_ThatDiffer()
        {
            foreach (var row in AnimalDefaults.Rows)
            {
                var grids = AnimalSpriteData.Grids[row.Id];
                Assert.IsTrue(grids.ContainsKey("idle0") && grids.ContainsKey("idle1"), row.Id);
                CollectionAssert.AreNotEqual(grids["idle0"], grids["idle1"], row.Id + " idle frames");
                CollectionAssert.AreNotEqual(grids["down0"], grids["idle0"], row.Id + " idle and standing");
            }
        }

        [Test]
        public void AGlintOnTheWater_MostlyDark_WithAShortFlashEachBeat()
        {
            var dark = 0; var bright = 0;
            for (var t = 0f; t < 6.3f; t += 0.05f)
            {
                var v = WaterSparkle.Twinkle(t, 0f, 1f);
                Assert.That(v, Is.InRange(0f, 1f));
                if (v < 0.05f) dark++;
                if (v > 0.5f) bright++;
            }
            Assert.Greater(dark, bright * 2, "dark far more than bright");
            Assert.Greater(bright, 0, "and it does flash");
            Assert.AreNotEqual(WaterSparkle.Twinkle(1f, 0f, 1.3f), WaterSparkle.Twinkle(1f, 2f, 1.3f), "glints are out of step");
        }

        [Test]
        public void TheDoor_StaysOpenAMoment_ThenFades_AndIsGone()
        {
            Assert.AreEqual(1f, DoorFlash.AlphaAt(0f));
            Assert.AreEqual(1f, DoorFlash.AlphaAt(DoorFlash.Life * 0.4f));
            Assert.That(DoorFlash.AlphaAt(DoorFlash.Life * 0.75f), Is.InRange(0.1f, 0.9f));
            Assert.AreEqual(0f, DoorFlash.AlphaAt(DoorFlash.Life));
            var sprite = DoorFlash.OpenDoorway();
            Assert.AreEqual(16, (int)sprite.rect.width);
            Assert.AreSame(sprite, DoorFlash.OpenDoorway());
        }
    }
}
