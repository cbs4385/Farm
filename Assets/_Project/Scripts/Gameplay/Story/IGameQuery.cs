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
