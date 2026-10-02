using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // Looks recipes up by id. Built from the game database (core assets plus module packs); when the database has
    // none, the built-in defaults are used so tests and bare projects still work.
    public sealed class RecipeCatalog
    {
        static RecipeCatalog _builtIn;
        readonly Dictionary<string, RecipeDefinition> _byId = new Dictionary<string, RecipeDefinition>();

        public static RecipeCatalog BuiltIn => _builtIn ?? (_builtIn = new RecipeCatalog(BuiltInAssets.Keep(CraftingDefaults.CreateRecipes())));

        public RecipeCatalog(IEnumerable<RecipeDefinition> definitions)
        {
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id)) _byId[d.Id] = d;
        }

        public static RecipeCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllRecipes.ToList() : new List<RecipeDefinition>();
            return all.Count == 0 ? BuiltIn : new RecipeCatalog(all);
        }

        public IEnumerable<RecipeDefinition> All => _byId.Values.OrderBy(r => r.Id, StringComparer.Ordinal);
        public RecipeDefinition Get(string id) => id != null && _byId.TryGetValue(id, out var d) ? d : null;
        public IEnumerable<RecipeDefinition> ForStation(string station) => All.Where(r => r.Station == station);
    }
}
