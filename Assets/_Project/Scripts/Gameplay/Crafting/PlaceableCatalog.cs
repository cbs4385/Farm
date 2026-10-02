using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // Looks placeable object types up by id. Built from the game database; the built-in defaults are used when it has none.
    public sealed class PlaceableCatalog
    {
        static PlaceableCatalog _builtIn;
        readonly Dictionary<string, PlaceableDefinition> _byId = new Dictionary<string, PlaceableDefinition>();

        public static PlaceableCatalog BuiltIn => _builtIn ?? (_builtIn = new PlaceableCatalog(BuiltInAssets.Keep(CraftingDefaults.CreatePlaceables())));

        public PlaceableCatalog(IEnumerable<PlaceableDefinition> definitions)
        {
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id)) _byId[d.Id] = d;
        }

        public static PlaceableCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllPlaceables.ToList() : new List<PlaceableDefinition>();
            return all.Count == 0 ? BuiltIn : new PlaceableCatalog(all);
        }

        public IEnumerable<PlaceableDefinition> All => _byId.Values.OrderBy(p => p.Id, StringComparer.Ordinal);
        public PlaceableDefinition Get(string id) => id != null && _byId.TryGetValue(id, out var d) ? d : null;
    }
}
