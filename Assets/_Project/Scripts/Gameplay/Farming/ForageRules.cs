using System.Linq;

namespace Farm.Gameplay
{
    // Wild plants spread from the ones left standing (playtest 2026-10-09: "the flora does not respawn fast enough, or the player should be told not to pick every
    // one so that more can grow"). The spawn tables favour a kind that is still on the map and rarely bring back one that was picked clean; these helpers let the
    // game say so before and after the last one is picked.
    public static class ForageRules
    {
        // How many of this kind of node stand on the map.
        public static int Standing(NodeGrid grid, string typeId) => grid.Nodes.Count(n => n.TypeId == typeId);

        public static bool IsLastOfItsKind(NodeGrid grid, string typeId) => Standing(grid, typeId) <= 1;

        // Shells and clams on the beach do not grow back from the ones left: the reminder there is to leave some for others.
        public static bool IsShoreFind(string itemId) => itemId == "forage.seashell" || itemId == "forage.clam" || itemId == "forage.pearl";

        // The string key of a reminder for what was picked: the beach's own wording for its finds.
        public static string ReminderKey(string baseKey, string itemId) => IsShoreFind(itemId) ? baseKey + ".shore" : baseKey;

        // A table that spreads: a kind with plants still standing is this many times as likely to appear as the table's weight says, and a kind picked clean
        // this fraction as likely.
        public const float StandingBoost = 3f;
        public const float PickedCleanFactor = 0.25f;
    }
}
