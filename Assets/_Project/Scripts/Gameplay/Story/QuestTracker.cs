using System.Text;
using Farm.Core;

namespace Farm.Gameplay
{
    // The short list of active quests and what is still to do for each, shown on the screen while playing (playtest chat 2026-10-09: "I cannot tell what my
    // current quest needs without opening the journal"). Pure text, so it can be tested; the HUD only shows it.
    public static class QuestTracker
    {
        public const int MaxQuests = 3;

        // "have/need" for an objective that counts something, else null.
        public static string Count(GameSession s, QuestDefinition def, QuestObjective o)
        {
            if (!string.IsNullOrEmpty(o.GiveItem)) return QuestLog.Given(s, def, o) + "/" + o.GiveCount;
            if (!string.IsNullOrEmpty(o.TakeItem)) return System.Math.Min(s.Backpack.Count(o.TakeItem), o.TakeCount) + "/" + o.TakeCount;
            return TryHasCondition(s, o.Condition);
        }

        // A condition "has:item>=n" counts the item in the backpack.
        static string TryHasCondition(GameSession s, string condition)
        {
            if (string.IsNullOrEmpty(condition) || !condition.StartsWith("has:")) return null;
            var rest = condition.Substring(4);
            var at = rest.IndexOf(">=", System.StringComparison.Ordinal);
            if (at <= 0 || !int.TryParse(rest.Substring(at + 2), out var need)) return null;
            return System.Math.Min(s.Backpack.Count(rest.Substring(0, at)), need) + "/" + need;
        }

        // Up to MaxQuests active quests: the title, then each objective still unmet with its count. Empty when there is nothing active.
        public static string Text(GameSession s)
        {
            var sb = new StringBuilder();
            var shown = 0;
            foreach (var q in QuestLog.Active(s))
            {
                if (shown == MaxQuests) break;
                if (shown > 0) sb.Append('\n');
                sb.Append(L.Get(q.TitleKey));
                foreach (var o in q.Objectives)
                {
                    if (QuestLog.ObjectiveMet(s, q, o)) continue;
                    sb.Append('\n').Append("  - ").Append(L.Get(o.Text));
                    var count = Count(s, q, o);
                    if (count != null) sb.Append(" (").Append(count).Append(')');
                }
                shown++;
            }
            return sb.ToString();
        }
    }
}
