using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Data
{
    // One thing a recipe uses up: a specific item, or any one of a list (all vegetables, say), and how many.
    [Serializable]
    public sealed class RecipeIngredient
    {
        public string ItemId;
        public string[] AnyOf;     // when set, any of these items counts (ItemId is then only a display hint)
        public int Count = 1;

        public bool Accepts(string itemId)
        {
            if (AnyOf != null && AnyOf.Length > 0) return Array.IndexOf(AnyOf, itemId) >= 0;
            return ItemId == itemId;
        }
    }

    public static class Stations
    {
        public const string Hand = "hand";          // crafted from the menu, anywhere
        public const string Kitchen = "kitchen";    // cooked at a kitchen
        public const string Furnace = "furnace";    // machines: loaded with ingredients, collected after `Minutes`
        public const string Keg = "keg";
        public const string Jar = "jar";
        public static readonly string[] Machines = { Furnace, Keg, Jar };
    }

    // A crafting, cooking or machine recipe. Ids are stable (saves remember learned recipes).
    [CreateAssetMenu(menuName = "Farm/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string _station = Stations.Hand;
        [SerializeField] RecipeIngredient[] _ingredients = new RecipeIngredient[0];
        [SerializeField] string _outputItemId;
        [SerializeField] int _outputCount = 1;
        [SerializeField] int _minutes;                 // machine processing time in game minutes (0 = instant)
        [SerializeField] string _skillId;              // learned automatically at this skill level ...
        [SerializeField] int _skillLevel;              // ... (0 = known from the start)
        [SerializeField] string _condition;            // optional: only offered while it holds

        public string Id => _id;
        public string Station => _station;
        public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients;
        public string OutputItemId => _outputItemId;
        public int OutputCount => _outputCount;
        public int Minutes => _minutes;
        public string SkillId => _skillId;
        public int SkillLevel => _skillLevel;
        public string Condition => _condition;
        public bool IsMachineRecipe => Array.IndexOf(Stations.Machines, _station) >= 0;
        public string NameKey => $"recipe.{_id}.name";

        public static RecipeDefinition Create(string id, string station, string outputItemId, int outputCount,
            RecipeIngredient[] ingredients, int minutes = 0, string skillId = null, int skillLevel = 0, string condition = null)
        {
            var r = CreateInstance<RecipeDefinition>();
            r._id = id;
            r.name = id;
            r._station = station;
            r._outputItemId = outputItemId;
            r._outputCount = outputCount;
            r._ingredients = ingredients;
            r._minutes = minutes;
            r._skillId = skillId;
            r._skillLevel = skillLevel;
            r._condition = condition;
            return r;
        }
    }
}
