using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // Items that could not be handed over because the backpack was full, or that were left for the player "by the door" (a neighbour's
    // gift, a package of roots). They wait in the farm's mailbox, kept as story variables (`parcel.<itemId>` = count), so nothing a letter,
    // an event or a quest hands over is ever lost. The mailbox hands them over when there is room.
    public static class Parcels
    {
        public const string Prefix = "parcel.";

        public static string VarFor(string itemId) => Prefix + itemId;

        public static void Send(GameSession s, string itemId, int count)
        {
            if (!s.InGame || count <= 0 || string.IsNullOrEmpty(itemId)) return;
            s.AddVar(VarFor(itemId), count);
        }

        public static IEnumerable<KeyValuePair<string, int>> Waiting(GameSession s) =>
            s.InGame ? s.State.Vars.Where(v => v.Key.StartsWith(Prefix) && v.Value > 0).Select(v => new KeyValuePair<string, int>(v.Key.Substring(Prefix.Length), v.Value)).ToList()
                     : Enumerable.Empty<KeyValuePair<string, int>>();

        public static int Count(GameSession s) => Waiting(s).Sum(p => p.Value);

        // Moves what fits into the backpack. Returns the item ids handed over (one entry per kind) and leaves the rest waiting.
        public static List<string> Claim(GameSession s)
        {
            var handed = new List<string>();
            foreach (var parcel in Waiting(s).OrderBy(p => p.Key, System.StringComparer.Ordinal))
            {
                var left = s.Backpack.Add(parcel.Key, parcel.Value);
                var took = parcel.Value - left;
                if (took <= 0) continue;
                s.AddVar(VarFor(parcel.Key), -took);
                handed.Add(parcel.Key);
            }
            return handed;
        }
    }
}
