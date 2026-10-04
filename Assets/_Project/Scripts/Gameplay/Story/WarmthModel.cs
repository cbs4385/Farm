namespace Farm.Gameplay
{
    // T-108, neglect and warmth: a villager the player has not spoken to for a week greets them with a "missed you" line the next time
    // they talk, and the return is worth a little friendship. The slow loss of points for neglect stays in NpcInteractions.ApplyDecay.
    // Pure rules; the lines are the dialogues npc.<villager>.missed (docs/narrative/STYLE.md).
    public static class WarmthModel
    {
        public const int MissedAfterDays = 7;
        public const int ReturnBonusPoints = 15;
        public const int MinimumHearts = 2;      // a villager the player barely knows does not miss them

        public static string DialogueId(string npcId) => $"npc.{npcId}.missed";

        // `daysSinceContact` is the days since the last talk or gift (-1 or less when there never was one).
        public static bool Missed(bool met, int daysSinceContact, int hearts) =>
            met && daysSinceContact >= MissedAfterDays && hearts >= MinimumHearts;
    }
}
