using System;

namespace Farm.Mythos
{
    // The Elder God's wakefulness, from the owner's design (GDD section 9, answer A and C). Pure functions only: the
    // module does not drive the game with them yet (task X-001).
    //
    // Stored as an integer in permille (0..1000 = 0%..100%) so it fits a story variable exactly.
    //   - Left alone it rises 25% per season, so it reaches full wakefulness in one game year.
    //   - Each ritual that succeeds lowers it by 30%-40% (never below 0), so several successful rituals can undo one
    //     that failed.
    //   - The horror layer's visual changes advance in steps of 5%.
    public static class WakefulnessModel
    {
        public const int Max = 1000;
        public const int StepSize = 50;                  // 5%
        public const int StepCount = Max / StepSize;     // 20 steps from asleep (0) to awake (20)
        public const int RisePerSeason = 250;            // 25%
        public const int DaysPerSeason = 28;
        public const int RitualReductionMin = 300;       // 30%
        public const int RitualReductionMax = 400;       // 40%

        // Increase for one day (dayOfSeason 1..28). Integer arithmetic spreads 250 exactly over the 28 days, so a
        // season always adds exactly 25% and a year exactly 100%.
        public static int RiseForDay(int dayOfSeason) =>
            RisePerSeason * dayOfSeason / DaysPerSeason - RisePerSeason * (dayOfSeason - 1) / DaysPerSeason;

        public static int AfterDay(int wakefulness, int dayOfSeason) =>
            Math.Min(Max, wakefulness + RiseForDay(dayOfSeason));

        // roll is a random number in [0, 1); the reduction lies in [30%, 40%].
        public static int RitualReduction(double roll)
        {
            var clamped = Math.Max(0.0, Math.Min(0.999999, roll));
            return RitualReductionMin + (int)(clamped * (RitualReductionMax - RitualReductionMin + 1));
        }

        public static int AfterSuccessfulRitual(int wakefulness, double roll) =>
            Math.Max(0, wakefulness - RitualReduction(roll));

        // 0..20: how many 5% thresholds have been crossed. A change in step triggers a visible change in the world.
        public static int Step(int wakefulness) => Math.Max(0, Math.Min(StepCount, wakefulness / StepSize));

        public static bool IsFullyAwake(int wakefulness) => wakefulness >= Max;

        public static float Percent(int wakefulness) => wakefulness / 10f;
    }
}
