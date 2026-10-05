using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest feedback: items could not be thrown away, and no chest was to be found. A stack can be discarded (not a tool), and a new game
    // starts with a chest in the pack.
    public class InventoryDiscardTests
    {
        static ItemDefinition Def(string id, ItemCategory category, ToolType tool = ToolType.None) =>
            ItemDefinition.Create(id, category, toolType: tool);

        [Test]
        public void ATool_CannotBeDiscarded_ButOrdinaryItemsCan()
        {
            Assert.IsFalse(InventoryRules.CanDiscard(Def("tool.hoe", ItemCategory.Tool, ToolType.Hoe)));
            Assert.IsTrue(InventoryRules.CanDiscard(Def("resource.stone", ItemCategory.Resource)));
            Assert.IsFalse(InventoryRules.CanDiscard(null));
        }

        [Test]
        public void ANewGame_StartsWithAChest_InThePack()
        {
            var state = GameState.NewGame("T", "F", id => 999, 1);
            var pack = Inventory.FromData(state.Backpack, id => 999);
            Assert.AreEqual(1, pack.Count(ItemIds.Machine("chest")));
        }

        [Test]
        public void TheChestRecipe_IsCheaperThanItWas()
        {
            var recipe = new RecipeCatalog(CraftingDefaults.CreateRecipes()).Get("chest");
            Assert.IsNotNull(recipe);
            foreach (var need in recipe.Ingredients) if (need.ItemId == ItemIds.Wood) Assert.AreEqual(25, need.Count);
        }
    }
}
