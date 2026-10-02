using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Saved per-NPC relationship state. Additive to GameState, so older saves load with defaults.
    [Serializable]
    public sealed class NpcState
    {
        public int Points;                 // friendship; 250 per heart, 10 hearts at most
        public bool Met;                   // the player has talked to them at least once
        public bool TalkedToday;
        public int GiftsToday;
        public int GiftsThisWeek;
        public int GiftWeek = -1;          // week index (TotalDays / 7) that GiftsThisWeek counts for
        public int LastContactDay = -1;    // last day (TotalDays) the player talked to or gave a gift to them
    }

    public static class QuestStatus
    {
        public const string Active = "active";
        public const string Done = "done";
        public const string Failed = "failed";
    }

    [Serializable]
    public sealed class QuestProgress
    {
        public string Status = QuestStatus.Active;
        public int StartedDay;
        public int EndedDay = -1;
        public int ExpiresDay = -1;        // help-wanted jobs vanish after this day; -1 = never
    }

    // A help-wanted board job generated for a few days: bring N of an item for gold.
    [Serializable]
    public sealed class BoardJob
    {
        public string Id;
        public string ItemId;
        public int Count;
        public int Reward;
        public int PostedDay;
        public int ExpiresDay;
        public bool Accepted;
    }
}
