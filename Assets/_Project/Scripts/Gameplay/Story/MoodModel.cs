using System;

namespace Farm.Gameplay
{
    public enum Mood { Content, Tired, Worried, Delighted, Lonely, Mischievous }

    public struct MoodInputs
    {
        public string NpcId;
        public int Today;                 // GameDateTime.TotalDays
        public string Weather;
        public int Hearts;
        public int DaysSinceContact;      // -1 when the player has never talked to them
        public bool GiftedToday;
        public bool BirthdayToday;
        public bool FestivalToday;
        public int Forced;                // var mood.<npc>.force: 0 = none, otherwise (Mood + 1); story arcs and layers use it
    }

    // A villager's mood today (T-093): a pure function of existing state, so nothing extra is saved and the answer is the
    // same all day. It drives greeting lines through the `mood:<npc>=<state>` condition (and, later, portrait expressions).
    // It never changes gameplay: no penalties, no locked options.
    public static class MoodModel
    {
        public const int LonelyAfterDays = 4;
        const double RainTiredChance = 0.35, MischiefChance = 0.12, WorriedChance = 0.04;

        public static Mood Compute(MoodInputs i)
        {
            if (i.Forced > 0 && i.Forced <= Enum.GetValues(typeof(Mood)).Length) return (Mood)(i.Forced - 1);
            if (i.BirthdayToday || i.GiftedToday || i.FestivalToday) return Mood.Delighted;
            if (i.DaysSinceContact >= LonelyAfterDays) return Mood.Lonely;
            var roll = DialogueSet.Unit(i.Today * 31 + NpcInteractions.StableHash(i.NpcId));
            if (i.Weather == "rain" || i.Weather == "storm") { if (roll < RainTiredChance) return Mood.Tired; }
            if (i.Hearts >= 3 && roll > 1.0 - MischiefChance) return Mood.Mischievous;
            if (roll > 1.0 - MischiefChance - WorriedChance && roll <= 1.0 - MischiefChance) return Mood.Worried;
            return Mood.Content;
        }

        public static bool TryParse(string name, out Mood mood) => Enum.TryParse(name, true, out mood) && Enum.IsDefined(typeof(Mood), mood);

        public static MoodInputs InputsFor(GameSession s, string npcId)
        {
            var now = s.Clock.Now;
            s.State.Npcs.TryGetValue(npcId, out var st);
            var npc = s.Npcs?.Get(npcId);
            return new MoodInputs
            {
                NpcId = npcId,
                Today = now.TotalDays,
                Weather = s.State.Weather,
                Hearts = st != null ? FriendshipModel.Hearts(st.Points) : 0,
                DaysSinceContact = st != null && st.LastContactDay >= 0 ? now.TotalDays - st.LastContactDay : -1,
                GiftedToday = st != null && st.GiftsToday > 0,
                BirthdayToday = npc != null && npc.IsBirthday(now),
                FestivalToday = StoryCalendar.DaysUntilFestival(s.Story?.Events, now) == 0,
                Forced = s.GetVar("mood." + npcId + ".force"),
            };
        }

        public static Mood Of(GameSession s, string npcId) => Compute(InputsFor(s, npcId));
    }
}
