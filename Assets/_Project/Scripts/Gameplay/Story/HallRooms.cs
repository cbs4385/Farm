using System.Collections.Generic;
using System.Linq;
using Farm.Core;

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

        // Hands over what the backpack holds of one thing a room needs (up to what is still needed); the room is restored as soon as everything has been given.
        // Returns how many were given.
        public static int Donate(GameSession s, string roomQuestId, QuestObjective objective)
        {
            var def = s.Story.Quest(roomQuestId);
            if (def == null) return 0;
            var given = QuestLog.Give(s, def, objective);
            if (given > 0)
            {
                s.Toast(L.Get("hall.given", given));
                if (QuestLog.ObjectivesMet(s, def)) QuestLog.TryComplete(s, def);
            }
            return given;
        }

        // Gives everything the backpack has for every room that is still open. Returns how many items were given.
        public static int DonateAll(GameSession s)
        {
            var total = 0;
            foreach (var room in Rooms)
            {
                var def = s.Story.Quest(room);
                if (def == null || IsRestored(s.State, room)) continue;
                foreach (var o in def.Objectives) total += Donate(s, room, o);
            }
            return total;
        }
    }
}
