namespace Farm.Gameplay
{
    // How well the village cat knows the player (playtest 2026-10-06: "enough interaction should lead to the cat moving into the player's home").
    // Each pet is +1, at most two count a day, and the bond stops at 30. A day with no petting costs 1. At 20 the cat adopts the player and
    // lives in the farmhouse; once adopted it leaves again if the bond falls to 10. Kept in vars and a flag, so the save needs nothing new.
    public static class CatBond
    {
        public const string BondVar = "cat.bond", PetDayVar = "cat.pet_day", PetsTodayVar = "cat.pets_today", AdoptedFlag = "cat.adopted";
        public const int Max = 30, PerDay = 2, AdoptAt = 20, LeaveAt = 10;

        public enum PetResult { Counted, DayLimit, Adopted }
        public enum DayResult { Nothing, Faded, Left }

        public static int Bond(GameState s) => s.Vars.TryGetValue(BondVar, out var v) ? v : 0;
        public static bool Adopted(GameState s) => s.Flags.Contains(AdoptedFlag);

        // The player pets the cat on day `today` (a running day number).
        public static PetResult Pet(GameState s, int today)
        {
            if (!s.Vars.TryGetValue(PetDayVar, out var day) || day != today)
            {
                s.Vars[PetDayVar] = today;
                s.Vars[PetsTodayVar] = 0;
            }
            var done = s.Vars.TryGetValue(PetsTodayVar, out var n) ? n : 0;
            if (done >= PerDay) return PetResult.DayLimit;
            s.Vars[PetsTodayVar] = done + 1;
            s.Vars[BondVar] = System.Math.Min(Max, Bond(s) + 1);
            if (!Adopted(s) && Bond(s) >= AdoptAt)
            {
                s.Flags.Add(AdoptedFlag);
                return PetResult.Adopted;
            }
            return PetResult.Counted;
        }

        // A new day begins (`today` is the new day): a day without a pet costs 1, and an adopted cat whose bond has worn down to 10 leaves.
        public static DayResult NewDay(GameState s, int today)
        {
            var bond = Bond(s);
            var result = DayResult.Nothing;
            var petYesterday = s.Vars.TryGetValue(PetDayVar, out var day) && day == today - 1;
            if (bond > 0 && !petYesterday)
            {
                s.Vars[BondVar] = bond - 1;
                result = DayResult.Faded;
            }
            if (Adopted(s) && Bond(s) <= LeaveAt)
            {
                s.Flags.Remove(AdoptedFlag);
                result = DayResult.Left;
            }
            return result;
        }
    }
}
