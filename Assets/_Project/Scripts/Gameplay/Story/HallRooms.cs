using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // The Community Hall (T-055): six rooms, each restored by a "bundle" (an ordinary quest in the story data whose objectives
    // hand over items). Restoring all six sets the flag `hall.restored` and plays the ending event. The relation to the
    // mythos arc is deliberately only flags: the optional layer reads `hall.*` and the `quest:` states.
    public static class HallRooms
    {
        public const string RestoredFlag = "hall.restored";
        public const string CompleteQuest = "hall_complete";

        public static readonly string[] Rooms = { "hall_pantry", "hall_crafts", "hall_fishtank", "hall_boiler", "hall_bulletin", "hall_vault" };

        public static string FlagOf(string roomQuestId) => "hall." + roomQuestId.Substring("hall_".Length);

        public static bool IsRestored(GameState state, string roomQuestId) => QuestLog.StatusOf(state, roomQuestId) == QuestStatus.Done;

        public static int RestoredCount(GameState state) => Rooms.Count(r => IsRestored(state, r));

        // Starts every room's quest that has not been started (done on the first visit).
        public static void StartAll(GameSession s)
        {
            foreach (var id in Rooms)
            {
                var def = s.Story.Quest(id);
                if (def != null && QuestLog.StatusOf(s.State, id) == "new") QuestLog.Start(s, def);
            }
        }

        public static bool Donate(GameSession s, string roomQuestId)
        {
            var def = s.Story.Quest(roomQuestId);
            return def != null && QuestLog.TryComplete(s, def);
        }
    }
}
