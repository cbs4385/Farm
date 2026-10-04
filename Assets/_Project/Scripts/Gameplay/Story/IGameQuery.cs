using Farm.Core;

namespace Farm.Gameplay
{
    // What story conditions can additionally ask about the game. `StateWorldQuery` implements it; the plain
    // IWorldQuery (and its test fakes) stays small. Atoms that need it simply evaluate to false/0 without it.
    public interface IGameQuery
    {
        int Hearts(string npcId);
        int ItemCount(string itemId);
        string QuestState(string questId);       // "active", "done", "failed" or "new"
        bool KnowsRecipe(string recipeId);
        bool MerchantHere() => false;           // the traveling merchant's stall is up today
        int FestivalDaysAway() => -1;           // days until the next festival (0 = today), -1 when there is none
        int BirthdayDaysAway(string npcId) => -1;   // days until a villager's birthday (0 = today), -1 when unknown
        int CropCount() => 0;                   // crops planted on every map
        int AnimalCount() => 0;                 // animals on the farm
        bool HeardLine(string dialogueId) => false; // a villager has said this dialogue before
        string FarmName() => string.Empty;
        string PlayerName() => string.Empty;
        string MoodOf(string npcId) => "content";   // today's mood of a villager (see MoodModel)
    }

    // Condition atoms used by story data (dialogue, schedules, quests, events). Safe to call more than once.
    //   hearts:<npc>>=4   has:<item>>=5   quest:<id>=active|done|failed|new   knows:<recipe>   weekday:<mon..sun>
    public static class StoryConditions
    {
        public static void Register()
        {
            Conditions.Register("hearts", (arg, w) =>
                Split(arg, out var id, out var op, out var n) && Compare(w is IGameQuery q ? q.Hearts(id) : 0, op, n));
            Conditions.Register("has", (arg, w) =>
                Split(arg, out var id, out var op, out var n) && Compare(w is IGameQuery q ? q.ItemCount(id) : 0, op, n));
            Conditions.Register("quest", (arg, w) =>
            {
                var eq = arg.IndexOf('=');
                if (eq <= 0) return false;
                var state = w is IGameQuery q ? q.QuestState(arg.Substring(0, eq)) : "new";
                return state == arg.Substring(eq + 1);
            });
            Conditions.Register("knows", (arg, w) => w is IGameQuery q && q.KnowsRecipe(arg));
            // Narrative atoms (T-092). festival.in:<=3 (days until the next festival, 0 = today), birthday.in:wren<=3,
            // farm:crops>=5, farm:animals>=1, heard:<dialogueId>, choice:<flag> (flag "choice.<flag>"),
            // storyline:<id> (flag "storyline.<id>"), farmname:<name> and playername:<name> (case-insensitive; an underscore stands for a space).
            Conditions.Register("festival.in", (arg, w) =>
                SplitNumber(arg, out var op, out var n) && w is IGameQuery q && q.FestivalDaysAway() >= 0 && Compare(q.FestivalDaysAway(), op, n));
            Conditions.Register("birthday.in", (arg, w) =>
                Split(arg, out var id, out var op, out var n) && w is IGameQuery q && q.BirthdayDaysAway(id) >= 0 && Compare(q.BirthdayDaysAway(id), op, n));
            Conditions.Register("farm", (arg, w) =>
            {
                if (!Split(arg, out var what, out var op, out var n) || !(w is IGameQuery q)) return false;
                if (what == "crops") return Compare(q.CropCount(), op, n);
                if (what == "animals") return Compare(q.AnimalCount(), op, n);
                return false;
            });
            // mood:<npc>=<content|tired|worried|delighted|lonely|mischievous>
            Conditions.Register("mood", (arg, w) =>
            {
                var eq = arg.IndexOf('=');
                if (eq <= 0 || eq == arg.Length - 1) throw new ConditionException($"mood needs <npc>=<state>, got '{arg}'");
                var state = arg.Substring(eq + 1);
                if (!MoodModel.TryParse(state, out _)) throw new ConditionException($"unknown mood '{state}'");
                return w is IGameQuery q && string.Equals(q.MoodOf(arg.Substring(0, eq)), state, System.StringComparison.OrdinalIgnoreCase);
            });
            Conditions.Register("heard", (arg, w) => w is IGameQuery q && q.HeardLine(arg));
            Conditions.Register("choice", (arg, w) => w.HasFlag("choice." + arg));
            Conditions.Register("storyline", (arg, w) => w.HasFlag("storyline." + arg));
            Conditions.Register("farmname", (arg, w) => w is IGameQuery q && string.Equals(q.FarmName().Replace(' ', '_'), arg, System.StringComparison.OrdinalIgnoreCase));
            Conditions.Register("playername", (arg, w) => w is IGameQuery q && string.Equals(q.PlayerName().Replace(' ', '_'), arg, System.StringComparison.OrdinalIgnoreCase));
            Conditions.Register("unseen", (arg, w) => w.GetVar(arg) < w.Now.Year);
            Merchant.RegisterConditions();
        }

        // "tilda>=4" -> id, op, number. The operator is required.
        public static bool Split(string arg, out string id, out string op, out int number)
        {
            id = null; op = null; number = 0;
            var idx = arg.IndexOfAny(new[] { '>', '<', '=', '!' });
            if (idx <= 0) return false;
            id = arg.Substring(0, idx);
            foreach (var candidate in new[] { ">=", "<=", "==", "!=", ">", "<" })
            {
                if (string.CompareOrdinal(arg, idx, candidate, 0, candidate.Length) != 0) continue;
                op = candidate;
                return int.TryParse(arg.Substring(idx + candidate.Length), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out number);
            }
            return false;
        }

        // "<=3" -> op, number (no id).
        static bool SplitNumber(string arg, out string op, out int number)
        {
            op = null; number = 0;
            foreach (var candidate in new[] { ">=", "<=", "==", "!=", ">", "<" })
            {
                if (!arg.StartsWith(candidate, System.StringComparison.Ordinal)) continue;
                op = candidate;
                return int.TryParse(arg.Substring(candidate.Length), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out number);
            }
            return false;
        }

        static bool Compare(int left, string op, int right)
        {
            switch (op)
            {
                case ">=": return left >= right;
                case "<=": return left <= right;
                case ">": return left > right;
                case "<": return left < right;
                case "==": return left == right;
                default: return left != right;
            }
        }
    }
}
