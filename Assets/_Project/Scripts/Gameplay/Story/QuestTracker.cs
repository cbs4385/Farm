using System.Linq;
using System.Text;
using Farm.Core;

namespace Farm.Gameplay
{
    // The quest the screen follows (playtest 2026-10-09): one quest at a time, chosen in the Journal. With only one active quest there is nothing to choose, so it is
    // followed by itself; when the followed quest is finished (or none was chosen) the first active quest is followed. The choice is a flag, so it is saved with the game.
    // Pure text, so it can be tested; the HUD only shows it.
    public static class QuestTracker
    {
        public const string TrackedPrefix = "quest.tracked.";

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

        // The id of the quest being followed, or null when no quest is active.
        public static string TrackedId(GameSession s)
        {
            var active = QuestLog.Active(s).Select(q => q.Id).ToList();
            if (active.Count == 0) return null;
            foreach (var flag in s.State.Flags)
                if (flag.StartsWith(TrackedPrefix, System.StringComparison.Ordinal) && active.Contains(flag.Substring(TrackedPrefix.Length)))
                    return flag.Substring(TrackedPrefix.Length);
            return active[0];
        }

        // Follows this quest from now on (and no other).
        public static void Track(GameSession s, string questId)
        {
            foreach (var old in s.State.Flags.Where(f => f.StartsWith(TrackedPrefix, System.StringComparison.Ordinal)).ToList()) s.SetFlag(old, false);
            s.SetFlag(TrackedPrefix + questId);
        }

        // The followed quest: its title, then each objective still unmet with its count. Empty when no quest is active.
        public static string Text(GameSession s)
        {
            var id = TrackedId(s);
            if (id == null) return string.Empty;
            var q = QuestLog.Active(s).FirstOrDefault(x => x.Id == id);
            if (q == null) return string.Empty;
            var sb = new StringBuilder(L.Get(q.TitleKey));
            foreach (var o in q.Objectives)
            {
                if (QuestLog.ObjectiveMet(s, q, o)) continue;
                sb.Append('\n').Append("  - ").Append(L.Get(o.Text));
                var count = Count(s, q, o);
                if (count != null) sb.Append(" (").Append(count).Append(')');
            }
            return sb.ToString();
        }
    }
}
