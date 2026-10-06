using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Carry and fishing poses: the pure parts (what is carried, where, and where the rod points).
    public class CarryPoseTests
    {
        [Test]
        public void BulkyThings_AreCarried_ButToolsSeedsAndFoodAreNot()
        {
            foreach (var c in new[] { ItemCategory.Crop, ItemCategory.Forage, ItemCategory.Fish, ItemCategory.Artisan, ItemCategory.Resource, ItemCategory.Furniture, ItemCategory.Machine, ItemCategory.Animal })
                Assert.IsTrue(HeldItem.Carries(c), c.ToString());
            foreach (var c in new[] { ItemCategory.Tool, ItemCategory.Seed, ItemCategory.Food, ItemCategory.Fertilizer, ItemCategory.Misc })
                Assert.IsFalse(HeldItem.Carries(c), c.ToString());
        }

        [Test]
        public void TheCarriedThing_SitsInFront_AtTheSideInASideView_AndBehindWhenFacingAway()
        {
            Assert.AreEqual(0f, HeldItem.CarryOffset(Vector2Int.down).x);
            Assert.Less(HeldItem.CarryOffset(Vector2Int.left).x, 0f);
            Assert.Greater(HeldItem.CarryOffset(Vector2Int.right).x, 0f);
            Assert.IsTrue(HeldItem.BehindBody(Vector2Int.up));
            Assert.IsFalse(HeldItem.BehindBody(Vector2Int.down));
            Assert.IsFalse(HeldItem.BehindBody(Vector2Int.left));
        }

        [Test]
        public void AHoistedFind_RisesAboveTheHead_ThenHangsThere()
        {
            Assert.Greater(HeldItem.HoistHeight(0f), 1.5f, "starts at the head");
            Assert.Greater(HeldItem.HoistHeight(0.25f), HeldItem.HoistHeight(0f), "rises");
            Assert.AreEqual(HeldItem.HoistHeight(0.5f), HeldItem.HoistHeight(1f), 0.0001f, "then stays");
        }

        [Test]
        public void TheRod_PointsTowardTheWayTheFarmerFaces_AndTheBobberRidesAPixel()
        {
            var (handL, tipL) = FishingPose.Rod(Vector2Int.left);
            var (handR, tipR) = FishingPose.Rod(Vector2Int.right);
            Assert.Less(tipL.x, handL.x);
            Assert.Greater(tipR.x, handR.x);
            Assert.Greater(FishingPose.Rod(Vector2Int.up).tip.y, FishingPose.Rod(Vector2Int.up).hand.y);
            for (var t = 0f; t < 6f; t += 0.1f) Assert.That(Mathf.Abs(FishingPose.Bob(t)), Is.LessThanOrEqualTo(1f / 16f + 0.0001f));
        }
    }
}
