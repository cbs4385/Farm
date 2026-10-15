using System.Collections.Generic;

namespace Farm.Gameplay
{
    // The farmhouse's own furniture that the mallet takes into the backpack like furniture one has bought (playtest 2026-10-15: carrying pieces one at a time took forever,
    // and a piece one dislikes could not be sold or stored). Keyed by the fixture id the scene builder gives (furn_<name>); the value is a decoration id (CraftingDefaults.Decor).
    // The couch, the fireplace, the large rugs and the jar shelf are wider than one cell, so they are still carried and set down by the mallet.
    public static class FixtureItems
    {
        static readonly Dictionary<string, string> ByFixture = new Dictionary<string, string>
        {
            { "furn_lamp", "lamp" }, { "furn_bedlamp", "lamp" },
            { "furn_painting", "painting" }, { "furn_bedpainting", "painting" },
            { "furn_clock", "clock" },
            { "furn_bookshelf", "bookshelf" }, { "furn_bookshelfeast", "bookshelf" },
            { "furn_armchair", "armchair" },
            { "furn_plantwest", "plant" }, { "furn_planteast", "plant" }, { "furn_plantsouth", "plant" }, { "furn_plantkitchen", "plant" }, { "furn_plantbedroom", "plant" },
            { "furn_diningtable", "dining_table" }, { "furn_kitchentable", "dining_table" },
            { "furn_chairwest", "chair" }, { "furn_chaireast", "chair" }, { "furn_kitchenchairwest", "chair" }, { "furn_kitchenchaireast", "chair" },
            { "furn_bench", "bench" },
            { "furn_sacks", "sacks" }, { "furn_barrels", "barrels" },
            { "furn_wardrobe", "wardrobe" }, { "furn_vase", "vase" },
        };

        // The decoration a house fixture becomes in the backpack, or null when it can only be carried.
        public static string For(string fixtureId) => fixtureId != null && ByFixture.TryGetValue(fixtureId, out var id) ? id : null;
    }
}
