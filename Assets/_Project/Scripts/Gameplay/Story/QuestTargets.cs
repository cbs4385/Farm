using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // Who is waiting on something the player is carrying (playtest 2026-10-10: "how do you find them if they aren't in their house"): the giver of an active quest
    // that asks for an item to be handed over, once the backpack holds enough of it. The world marks them with a "!" and the map names the place. Pure given the session.
    public static class QuestTargets
    {
        public static IReadOnlyList<string> Wanting(GameSession session)
        {
            var result = new List<string>();
            if (session == null || !session.InGame || session.Story == null) return result;
            foreach (var quest in QuestLog.Active(session))
            {
                if (string.IsNullOrEmpty(quest.Giver) || result.Contains(quest.Giver)) continue;
                if (quest.Objectives.Any(o => !string.IsNullOrEmpty(o.TakeItem) && o.TakeCount > 0 && QuestLog.ObjectiveMet(session, quest, o)))
                    result.Add(quest.Giver);
            }
            return result;
        }

        public static bool Wants(GameSession session, string npcId) => Wanting(session).Contains(npcId);
    }
}
