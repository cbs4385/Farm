using Farm.Data;

namespace Farm.Gameplay
{
    // What eating a dish gives back: its energy, and half as much health (the rule `ItemActions.Eat` has always used). One place, so that the recipe lists can show the same numbers the
    // farmer gets (playtest 2026-10-16: "show how much health and energy each dish restores so that players can plan"). Pure.
    public static class FoodEffects
    {
        public static bool Restores(ItemDefinition item) => item != null && item.EnergyRestore > 0;

        public static int Energy(ItemDefinition item) => item == null ? 0 : item.EnergyRestore;

        public static int Health(ItemDefinition item) => item == null ? 0 : item.EnergyRestore / 2;
    }
}
