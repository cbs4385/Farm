using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // A tiny log of the player's recent deeds, so the Gazette and other writing can say what happened this week (T-146). It stores only the
    // last day each kind of deed happened (saved as module data, no GameState change), read by the `recent:<kind>` condition.
    [Serializable]
    public sealed class DeedState
    {
        public const string ModuleId = "story.deeds";
        public Dictionary<string, int> Last = new Dictionary<string, int>();

        public static DeedState Load(GameSession s) => s.GetModuleData<DeedState>(ModuleId);
        public static void Store(GameSession s, DeedState d) => s.SetModuleData(ModuleId, d);
    }

    public static class DeedLog
    {
        public const int RecentDays = 7;

        // Deed kinds the game records.
        public const string Quest = "quest", Gift = "gift", Skill = "skill", Scene = "scene", BigSale = "bigsale";
        public const int BigSaleGold = 300;

        public static void Record(GameSession s, string kind)
        {
            if (s == null || !s.InGame) return;
            var state = DeedState.Load(s);
            state.Last[kind] = s.Clock.Now.TotalDays;
            DeedState.Store(s, state);
        }

        // True when the deed happened within the last RecentDays days (today counts).
        public static bool IsRecent(DeedState state, string kind, int today) =>
            state != null && state.Last.TryGetValue(kind, out var day) && today - day >= 0 && today - day < RecentDays;
    }
}
