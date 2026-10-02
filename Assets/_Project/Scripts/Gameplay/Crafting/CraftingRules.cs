using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public enum CraftResult { Ok, NotKnown, MissingIngredients, NoRoom, WrongStation }
    public enum LoadResult { Loaded, Busy, NoRecipe, MissingIngredients }

    // The rules of crafting, cooking and machines. Pure logic over a backpack and the saved state.
    public static class CraftingRules
    {
        // ---- what a recipe needs --------------------------------------------------------------------------------------

        public static int Available(RecipeIngredient ingredient, Inventory backpack)
        {
            if (ingredient.AnyOf != null && ingredient.AnyOf.Length > 0) return ingredient.AnyOf.Sum(backpack.Count);
            return backpack.Count(ingredient.ItemId);
        }

        public static bool HasIngredients(RecipeDefinition recipe, Inventory backpack) =>
            recipe.Ingredients.All(i => Available(i, backpack) >= i.Count);

        // Removes the ingredients (from the accepted items in list order). Call only after HasIngredients.
        public static void TakeIngredients(RecipeDefinition recipe, Inventory backpack)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                var left = ingredient.Count;
                var ids = ingredient.AnyOf != null && ingredient.AnyOf.Length > 0 ? ingredient.AnyOf : new[] { ingredient.ItemId };
                foreach (var id in ids)
                {
                    if (left <= 0) break;
                    left -= backpack.Remove(id, left);
                }
            }
        }

        // ---- knowing recipes ------------------------------------------------------------------------------------------

        // A recipe is known from the start (no skill requirement), once learned (a letter, a villager), or once the player's
        // skill reaches its level.
        public static bool Knows(RecipeDefinition recipe, GameState state, Func<string, int> skillLevel)
        {
            if (!string.IsNullOrEmpty(recipe.Condition) && state != null) { /* offered conditions are checked by callers with a world */ }
            if (recipe.SkillLevel <= 0 || string.IsNullOrEmpty(recipe.SkillId)) return true;
            return state.Recipes.Contains(recipe.Id) || skillLevel(recipe.SkillId) >= recipe.SkillLevel;
        }

        // Recipes a skill has just unlocked by going from `fromLevel` to `toLevel`.
        public static List<RecipeDefinition> NewlyUnlocked(RecipeCatalog recipes, string skill, int fromLevel, int toLevel) =>
            recipes.All.Where(r => r.SkillId == skill && r.SkillLevel > fromLevel && r.SkillLevel <= toLevel).ToList();

        // ---- crafting by hand or at a kitchen -------------------------------------------------------------------------

        public static CraftResult TryCraft(RecipeDefinition recipe, string station, GameState state, Inventory backpack, Func<string, int> skillLevel)
        {
            if (recipe.Station != station) return CraftResult.WrongStation;
            if (!Knows(recipe, state, skillLevel)) return CraftResult.NotKnown;
            if (!HasIngredients(recipe, backpack)) return CraftResult.MissingIngredients;

            // Check the room with the ingredients taken out, on a copy, so a full pack never loses materials.
            var probe = Inventory.FromData(backpack.ToData(), id => 999);
            TakeIngredients(recipe, probe);
            if (!probe.CanAdd(recipe.OutputItemId, recipe.OutputCount)) return CraftResult.NoRoom;

            TakeIngredients(recipe, backpack);
            backpack.Add(recipe.OutputItemId, recipe.OutputCount);
            return CraftResult.Ok;
        }

        // ---- machines -------------------------------------------------------------------------------------------------

        public static bool IsBusy(PlacedObject machine) => !string.IsNullOrEmpty(machine.RecipeId);
        public static bool IsReady(PlacedObject machine, int nowMinute) => IsBusy(machine) && nowMinute >= machine.ReadyAt;
        public static int MinutesLeft(PlacedObject machine, int nowMinute) => Math.Max(0, machine.ReadyAt - nowMinute);

        // The recipe a machine would start with the held item, or null.
        public static RecipeDefinition RecipeFor(PlaceableDefinition machine, string heldItemId, RecipeCatalog recipes, Inventory backpack) =>
            recipes.ForStation(machine.Station).FirstOrDefault(r => r.Ingredients.Any(i => i.Accepts(heldItemId)) && HasIngredients(r, backpack));

        // Loads a machine with the ingredients for the recipe that matches the held item.
        public static LoadResult TryLoad(PlacedObject machine, PlaceableDefinition def, string heldItemId, RecipeCatalog recipes,
            Inventory backpack, int nowMinute, out RecipeDefinition started)
        {
            started = null;
            if (IsBusy(machine)) return LoadResult.Busy;
            var candidates = recipes.ForStation(def.Station).Where(r => r.Ingredients.Any(i => i.Accepts(heldItemId))).ToList();
            if (candidates.Count == 0) return LoadResult.NoRecipe;
            var recipe = candidates.FirstOrDefault(r => HasIngredients(r, backpack));
            if (recipe == null) return LoadResult.MissingIngredients;

            TakeIngredients(recipe, backpack);
            machine.RecipeId = recipe.Id;
            machine.OutputItemId = recipe.OutputItemId;
            machine.OutputCount = recipe.OutputCount;
            machine.ReadyAt = nowMinute + Math.Max(recipe.Minutes, 10);
            started = recipe;
            return LoadResult.Loaded;
        }

        // Takes the finished output. Returns false when it is not ready or the backpack has no room (nothing changes).
        public static bool TryCollect(PlacedObject machine, Inventory backpack, int nowMinute)
        {
            if (!IsReady(machine, nowMinute)) return false;
            if (!backpack.CanAdd(machine.OutputItemId, machine.OutputCount)) return false;
            backpack.Add(machine.OutputItemId, machine.OutputCount);
            machine.RecipeId = null;
            machine.OutputItemId = null;
            machine.OutputCount = 0;
            machine.ReadyAt = 0;
            return true;
        }
    }
}
