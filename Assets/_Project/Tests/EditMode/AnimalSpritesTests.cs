using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Request (2026-10-05): the farm animals need art for every orientation and every animation they need.
    public class AnimalSpritesTests
    {
        static readonly string[] Views = { "down", "up", "left" };

        [Test]
        public void EveryAnimal_HasEveryView_EveryWalkFrame_AMunch_AndASleepingPose()
        {
            foreach (var row in AnimalDefaults.Rows)
            {
                Assert.IsTrue(AnimalSprites.Has(row.Id), row.Id + " has art");
                var grids = AnimalSpriteData.Grids[row.Id];
                var palette = AnimalSpriteData.Palettes[row.Id];
                foreach (var key in AnimalSpriteData.Keys)
                {
                    Assert.IsTrue(grids.ContainsKey(key), row.Id + " " + key);
                    Assert.AreEqual(16, grids[key].Length, row.Id + " " + key + " rows");
                    foreach (var line in grids[key])
                    {
                        Assert.AreEqual(16, line.Length, row.Id + " " + key + " row width");
                        foreach (var c in line) Assert.IsTrue(c == '.' || palette.ContainsKey(c), row.Id + " " + key + " uses an unknown colour " + c);
                    }
                    Assert.IsTrue(AnimalSprites.Pixels(row.Id, key).Any(p => p.a > 0), row.Id + " " + key + " is not empty");
                }
                foreach (var v in Views) foreach (var f in new[] { 0, 1 }) Assert.IsTrue(grids.ContainsKey(v + f), row.Id + " " + v + f);
                Assert.IsTrue(grids.ContainsKey("eat0") && grids.ContainsKey("eat1") && grids.ContainsKey("sleep"));
            }
        }

        [Test]
        public void TheFramesOfAnAnimation_AndTheViews_AreDifferentPictures()
        {
            foreach (var row in AnimalDefaults.Rows)
            {
                string[] G(string key) => AnimalSpriteData.Grids[row.Id][key];
                foreach (var v in Views) CollectionAssert.AreNotEqual(G(v + "0"), G(v + "1"), row.Id + " " + v + " walk frames");
                CollectionAssert.AreNotEqual(G("eat0"), G("eat1"), row.Id + " munch");
                CollectionAssert.AreNotEqual(G("down0"), G("up0"), row.Id + " front and back");
                CollectionAssert.AreNotEqual(G("down0"), G("left0"), row.Id + " front and side");
                CollectionAssert.AreNotEqual(G("down0"), G("eat0"), row.Id + " standing and eating");
                CollectionAssert.AreNotEqual(G("left0"), G("sleep"), row.Id + " standing and sleeping");
            }
        }

        [Test]
        public void ThePictureShown_FollowsTheWayItFaces_WhetherItMoves_EatsOrSleeps()
        {
            Assert.AreEqual(("down0", false), AnimalSprites.Pick(Vector2Int.down, false, false, false, 5f), "standing, facing the front");
            Assert.AreEqual(("up0", false), AnimalSprites.Pick(Vector2Int.up, false, false, false, 5f));
            Assert.AreEqual(("left0", false), AnimalSprites.Pick(Vector2Int.left, false, false, false, 5f));
            Assert.AreEqual(("left0", true), AnimalSprites.Pick(Vector2Int.right, false, false, false, 5f), "looking right is the side view mirrored");
            Assert.AreEqual(("left1", true), AnimalSprites.Pick(Vector2Int.right, true, false, false, AnimalSprites.WalkFrameSeconds * 1.5f), "walking steps through the frames");
            Assert.AreEqual(("down0", false), AnimalSprites.Pick(Vector2Int.down, true, false, false, 0f));
            Assert.AreEqual(("down1", false), AnimalSprites.Pick(Vector2Int.down, true, false, false, AnimalSprites.WalkFrameSeconds * 1.1f));
            Assert.AreEqual(("eat0", false), AnimalSprites.Pick(Vector2Int.right, false, true, false, 0f), "eating faces the front");
            Assert.AreEqual(("eat1", false), AnimalSprites.Pick(Vector2Int.right, false, true, false, AnimalSprites.MunchFrameSeconds * 1.1f));
            Assert.AreEqual(("sleep", true), AnimalSprites.Pick(Vector2Int.right, true, true, true, 0f), "asleep beats everything else");
        }

        [Test]
        public void TheAnimals_SleepAtNight()
        {
            Assert.IsTrue(AnimalSprites.IsNight(22));
            Assert.IsTrue(AnimalSprites.IsNight(25), "after midnight");
            Assert.IsTrue(AnimalSprites.IsNight(2));
            Assert.IsFalse(AnimalSprites.IsNight(6));
            Assert.IsFalse(AnimalSprites.IsNight(14));
        }

        [Test]
        public void TheSpritesAreBuiltOnce_AndNamed()
        {
            AnimalSprites.ClearCache();
            var a = AnimalSprites.Get("cow", "left1");
            Assert.AreEqual("animal_cow_left1", a.name);
            Assert.AreEqual(16, (int)a.rect.width);
            Assert.AreSame(a, AnimalSprites.Get("cow", "left1"));
            Assert.IsNull(AnimalSprites.Get("cow", "no_such_picture"));
            Assert.IsNull(AnimalSprites.Get("dragon", "down0"));
        }
    }
}
