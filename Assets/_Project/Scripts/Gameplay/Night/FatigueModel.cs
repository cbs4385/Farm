using System;

namespace Farm.Gameplay
{
    // Late-night fatigue (owner design, GDD section 9, decisions R-X as revised). The player may stay up as late as they
    // like, up to the 06:00 end of the day, but past 22:00 they grow more tired the longer they stay awake. Fatigue is a
    // rating from 0 (rested) to 1 (the whole night awake) that grows with time after 22:00:
    //   - energy recovery from sleeping is reduced by up to 100% (nothing recovered after an all-nighter);
    //   - luck is reduced by up to 50% (half effectiveness).
    // A fatigue meter next to the energy bar shows it, only while the rating is above zero, and a one-time message
    // warns the player the first time the clock reaches 22:00.
    // Assumptions awaiting the owner's confirmation (GDD questions AA/AB): growth is linear from 22:00 to 06:00, and the
    // full-night values are the 100% / 50% from decision X. Pure arithmetic; wiring is T-046.
    public static class FatigueModel
    {
        public const int StartMinuteOfDay = 22 * 60;        // the first warning and the start of fatigue
        public const int EndMinuteOfDay = 30 * 60;          // 06:00, the end of the day (decision U)
        public const float FullEnergyRecoveryPenalty = 1.0f;
        public const float FullLuckPenalty = 0.5f;

        // 0 until 22:00, then growing linearly to 1 at 06:00. Minutes beyond the end stay at 1.
        public static float Fatigue(int minuteOfDay)
        {
            if (minuteOfDay <= StartMinuteOfDay) return 0f;
            var fraction = (minuteOfDay - StartMinuteOfDay) / (float)(EndMinuteOfDay - StartMinuteOfDay);
            return Math.Min(1f, fraction);
        }

        // The fatigue meter is shown only while the rating is positive.
        public static bool MeterVisible(int minuteOfDay) => Fatigue(minuteOfDay) > 0f;

        public static float EnergyRecoveryPenalty(float fatigue) => Clamp01(fatigue) * FullEnergyRecoveryPenalty;

        public static float LuckPenalty(float fatigue) => Clamp01(fatigue) * FullLuckPenalty;

        // 1 minus the penalty: 1 is fully effective, 0.5 is half effective.
        public static float LuckEffectiveness(float fatigue) => 1f - LuckPenalty(fatigue);

        // Luck as the player experiences it. Fatigue scales good luck and bonus chances by the luck effectiveness and
        // leaves bad luck unchanged (decision AA-c). At neutral luck (0) there is nothing to scale.
        public static float ApplyToLuck(float luck, float fatigue) =>
            luck > 0f ? luck * LuckEffectiveness(fatigue) : luck;

        // Sleeping restores energy towards `target` (full when going to bed, less when passing out). Fatigue scales how
        // much of the missing energy comes back: none at full fatigue. Never lowers energy.
        public static int EnergyAfterSleep(int current, int target, float fatigue)
        {
            if (target <= current) return current;
            var recovered = (target - current) * (1f - EnergyRecoveryPenalty(fatigue));
            return current + (int)Math.Round(recovered);
        }

        // True on the tick where the clock first reaches 22:00 and the player has not yet been warned.
        public static bool ShouldWarn(int previousMinuteOfDay, int currentMinuteOfDay, bool alreadyWarned) =>
            !alreadyWarned && previousMinuteOfDay < StartMinuteOfDay && currentMinuteOfDay >= StartMinuteOfDay;

        // True while the warning is still owed: from 22:00 until the player has seen it (also covers loading a save
        // that is already past 22:00 without the flag).
        public static bool NeedsWarning(int minuteOfDay, bool alreadyWarned) =>
            !alreadyWarned && minuteOfDay >= StartMinuteOfDay && minuteOfDay < EndMinuteOfDay;

        // Story flag that records the one-time warning in the save.
        public const string WarnedFlag = "tutorial.late_night_warned";

        static float Clamp01(float v) => Math.Max(0f, Math.Min(1f, v));
    }
}
