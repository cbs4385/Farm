using System;
using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // What each dialogue set (one per villager) has already said, and when. Saved as module data, so no GameState field
    // changes and older saves load with an empty memory. Pure data: the variety rules live in DialogueSet.PickVaried.
    [Serializable]
    public sealed class LineMemory
    {
        public const string ModuleId = "dialogue.memory";
        public const int MaxPerScope = 160;

        [Serializable]
        public sealed class Last
        {
            public int Day = -1;
            public string Dialogue;
            public int Priority;
        }

        public Dictionary<string, Dictionary<string, int>> Heard = new Dictionary<string, Dictionary<string, int>>();   // scope -> dialogue -> last day
        public Dictionary<string, int> Count = new Dictionary<string, int>();                                          // scope|dialogue -> times
        public Dictionary<string, Last> LastPick = new Dictionary<string, Last>();

        public static LineMemory Load(GameSession s) => s.GetModuleData<LineMemory>(ModuleId);
        public static void Store(GameSession s, LineMemory m) => s.SetModuleData(ModuleId, m);

        // Day (TotalDays) the line was last said, or -1 when never.
        public int LastHeardDay(string scope, string dialogue) =>
            Heard.TryGetValue(scope, out var lines) && lines.TryGetValue(dialogue, out var day) ? day : -1;

        public bool WasHeard(string scope, string dialogue) => LastHeardDay(scope, dialogue) >= 0;

        public int TimesHeard(string scope, string dialogue) => Count.TryGetValue(scope + "|" + dialogue, out var n) ? n : 0;

        // True when any villager has said this dialogue (the `heard:` condition).
        public bool HeardAnywhere(string dialogue) => Heard.Values.Any(lines => lines.ContainsKey(dialogue));

        public Last LastOf(string scope) => LastPick.TryGetValue(scope, out var l) ? l : null;

        public void Record(string scope, string dialogue, int day, int priority)
        {
            if (!Heard.TryGetValue(scope, out var lines)) Heard[scope] = lines = new Dictionary<string, int>();
            lines[dialogue] = day;
            Count[scope + "|" + dialogue] = TimesHeard(scope, dialogue) + 1;
            LastPick[scope] = new Last { Day = day, Dialogue = dialogue, Priority = priority };
            if (lines.Count > MaxPerScope)
            {
                var oldest = lines.OrderBy(kv => kv.Value).First().Key;
                lines.Remove(oldest);
            }
        }
    }

    // Rarity tiers for dialogue set entries: how often an eligible line is chosen relative to a common one.
    public static class LineRarity
    {
        public const string Common = "common", Uncommon = "uncommon", Rare = "rare", Legendary = "legendary";
        public static readonly string[] All = { Common, Uncommon, Rare, Legendary };

        public static bool IsKnown(string rarity) => string.IsNullOrEmpty(rarity) || Array.IndexOf(All, rarity) >= 0;

        public static float Weight(string rarity)
        {
            switch (rarity)
            {
                case Uncommon: return 30f;
                case Rare: return 8f;
                case Legendary: return 1f;
                default: return 100f;
            }
        }
    }
}
