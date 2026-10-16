using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The recipe lists show what a dish gives back (playtest 2026-10-16), using the same numbers eating gives.
    public class FoodEffectsTests
    {
        [Test]
        public void ADish_GivesItsEnergy_AndHalfThatInHealth()
        {
            var pie = ItemDefinition.Create("dish.pie", ItemCategory.Food, 99, sellPrice: 120, energyRestore: 90);
            Assert.IsTrue(FoodEffects.Restores(pie));
            Assert.AreEqual(90, FoodEffects.Energy(pie));
            Assert.AreEqual(45, FoodEffects.Health(pie));
        }

        [Test]
        public void ThingsThatAreNotFood_RestoreNothing()
        {
            var bar = ItemDefinition.Create("resource.copperbar", ItemCategory.Resource, 99, sellPrice: 60);
            Assert.IsFalse(FoodEffects.Restores(bar));
            Assert.AreEqual(0, FoodEffects.Energy(bar));
            Assert.AreEqual(0, FoodEffects.Health(bar));
            Assert.IsFalse(FoodEffects.Restores(null));
            Assert.AreEqual(0, FoodEffects.Health(null));
        }

        [Test]
        public void EveryCookedDish_WithAnEnergyValue_ShowsBothNumbers()
        {
            var db = UnityEngine.Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var story = RecipeCatalog.From(db);
            var any = false;
            foreach (var recipe in story.All)
                if (db.TryGetItem(recipe.OutputItemId, out var item) && FoodEffects.Restores(item))
                {
                    any = true;
                    Assert.Greater(FoodEffects.Energy(item), 0, recipe.Id);
                    Assert.GreaterOrEqual(FoodEffects.Health(item), 0, recipe.Id);
                }
            Assert.IsTrue(any, "some recipe makes food");
        }
    }
}
