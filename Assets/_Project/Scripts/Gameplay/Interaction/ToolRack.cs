using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // The rack at the front of the farmhouse where a new farmer's tools hang (playtest 2026-10-13). It is a chest like any other (a placed object of the type "toolrack"):
    // open it with Interact to take the tools you want and to put them back. A new game puts the tools in it; a game begun before it has no rack.
    public static class ToolRack
    {
        public const string StockedFlag = "rack.stocked";          // set by a new game that starts with its tools on the rack
        public const string ObjectId = "toolrack";
        public const int X = 83, Y = 19, Slots = 12;               // east of the farmhouse door
        const string MigratedFlag = "rack.migrated";
        public static readonly string[] Tools = { ItemIds.Hoe, ItemIds.WateringCan, ItemIds.Axe, ItemIds.Pickaxe, ItemIds.Scythe, ItemIds.Sword, ItemIds.Hammer };

        // A game begun while the rack was still a fixed prop of the farm (early 0.6.0) has the stocked flag but no rack chest: set one up once, holding the tools not yet taken
        // (the old rack recorded each taken tool as the flag "rack.taken.<id>").
        public static bool MigrateOldRack(GameState state, System.Func<string, int> maxStack)
        {
            if (!state.Flags.Contains(StockedFlag) || !state.Flags.Add(MigratedFlag)) return false;
            var farm = state.GetMap(MapIds.Farm);
            if (farm.Objects.Any(o => o.Id == ObjectId)) return false;
            var rack = new Inventory(Slots, maxStack);
            foreach (var tool in Tools)
                if (!state.Flags.Contains("rack.taken." + tool)) rack.Add(tool, 1);
            farm.Objects.Add(new PlacedObject { Id = ObjectId, TypeId = CraftingDefaults.ToolRackId, X = X, Y = Y, Items = rack.ToData() });
            return true;
        }
    }
}
