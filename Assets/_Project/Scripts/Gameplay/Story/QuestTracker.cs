using System.Collections.Generic;
using System.Linq;
using System.Text;
using Farm.Core;

namespace Farm.Gameplay
{
    // The quests the screen lists (playtest 2026-10-14): every active quest, unless the player switched its Track button off in the Journal. The choice is a flag, so it is
    // saved with the game. Pure text, so it can be tested; the HUD only shows it.
    public static class QuestTracker
    {
        public const string HiddenPrefix = "quest.hidden.";          // set when the player switched a quest's tracking off

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

        // A quest is listed on screen unless the player turned it off (playtest 2026-10-14: Track is a switch, and any number of quests can be on). The flag is the
        // "off" side so that every new quest appears by itself.
        public static bool IsTracked(GameSession s, string questId) => !s.HasFlag(HiddenPrefix + questId);

        // The active quests that are listed, in the order of the journal.
        public static List<string> TrackedIds(GameSession s) => QuestLog.Active(s).Select(q => q.Id).Where(id => IsTracked(s, id)).ToList();

        public static void SetTracked(GameSession s, string questId, bool tracked) => s.SetFlag(HiddenPrefix + questId, !tracked);

        // The Track button: on becomes off and off becomes on. Returns whether the quest is listed now.
        public static bool Toggle(GameSession s, string questId)
        {
            var now = !IsTracked(s, questId);
            SetTracked(s, questId, now);
            return now;
        }

        // Lists this quest on screen.
        public static void Track(GameSession s, string questId) => SetTracked(s, questId, true);

        // Each listed quest: its title, then each objective still unmet with its count; quests are set apart by a blank line. Empty when none is listed.
        public static string Text(GameSession s)
        {
            var sb = new StringBuilder();
            foreach (var q in QuestLog.Active(s))
            {
                if (!IsTracked(s, q.Id)) continue;
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append(L.Get(q.TitleKey));
                foreach (var o in q.Objectives)
                {
                    if (QuestLog.ObjectiveMet(s, q, o)) continue;
                    sb.Append('\n').Append("  - ").Append(L.Get(o.Text));
                    var count = Count(s, q, o);
                    if (count != null) sb.Append(" (").Append(count).Append(')');
                    sb.Append(QuestLog.CarryingText(s, q, o));
                }
            }
            return sb.ToString();
        }
    }
}
