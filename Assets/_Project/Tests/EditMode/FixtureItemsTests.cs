using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class FixtureItemsTests
    {
        [TestCase("furn_chairwest", "chair")]
        [TestCase("furn_kitchentable", "dining_table")]
        [TestCase("furn_bench", "bench")]
        [TestCase("furn_barrels", "barrels")]
        public void HouseFurnitureBecomesADecoration(string fixtureId, string decor)
        {
            Assert.AreEqual(decor, FixtureItems.For(fixtureId));
            Assert.IsTrue(CraftingDefaults.Decor.Any(d => d.id == decor) || CraftingDefaults.FarmProps.Contains(decor));
        }

        [TestCase("furn_couch")]
        [TestCase("furn_fireplace")]
        [TestCase("furn_ruglarge")]
        [TestCase(null)]
        public void WideFurnitureIsStillCarried(string fixtureId) => Assert.IsNull(FixtureItems.For(fixtureId));

        [Test]
        public void ARoomFixtureWithoutAnItemUsesTheTable()
        {
            var go = new UnityEngine.GameObject("f");
            var fixture = go.AddComponent<MovableFixture>();
            fixture.Id = "furn_vase";
            Assert.AreEqual("vase", fixture.ItemId);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
