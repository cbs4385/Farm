namespace Farm.Gameplay
{
    // Trees come back: a felled tree may drop acorns, and an acorn planted on open ground grows into a tree (playtest 2026-10-09: "there are not enough
    // harvestable trees, the player should be able to plant them").
    public static class TreeRules
    {
        // How many acorns a felled tree drops for a roll in 0..1: one half of the time, two now and then.
        public static int AcornsFor(float roll) => roll < 0.15f ? 2 : roll < 0.65f ? 1 : 0;
    }
}
