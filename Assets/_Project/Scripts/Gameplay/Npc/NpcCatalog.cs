using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // Looks villagers up by id. Built from the game database (core assets plus module packs); when the database has
    // none, the built-in defaults are used so tests and bare projects still work.
    public sealed class NpcCatalog
    {
        static NpcCatalog _builtIn;
        readonly Dictionary<string, NpcDefinition> _byId = new Dictionary<string, NpcDefinition>();

        public static NpcCatalog BuiltIn => _builtIn ?? (_builtIn = new NpcCatalog(BuiltInAssets.Keep(NpcDefaults.CreateAll())));

        public NpcCatalog(IEnumerable<NpcDefinition> definitions)
        {
            foreach (var d in definitions)
                if (d != null && !string.IsNullOrEmpty(d.Id)) _byId[d.Id] = d;
        }

        public static NpcCatalog From(GameDatabase db)
        {
            var all = db != null ? db.AllNpcs.ToList() : new List<NpcDefinition>();
            return all.Count == 0 ? BuiltIn : new NpcCatalog(all);
        }

        public IEnumerable<NpcDefinition> All => _byId.Values.OrderBy(n => n.Id, StringComparer.Ordinal);
        public NpcDefinition Get(string id) => id != null && _byId.TryGetValue(id, out var d) ? d : null;
        public bool Contains(string id) => id != null && _byId.ContainsKey(id);
    }
}
