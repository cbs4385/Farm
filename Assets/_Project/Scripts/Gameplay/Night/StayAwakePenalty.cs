using System;

namespace Farm.Gameplay
{
    // The cost of staying up (owner design, GDD section 9, X). Each stay-awake check passed adds a penalty so that a
    // player who stays up until 06:00 (all thirty checks) has effectively no energy recovery from sleeping (100%
    // penalty) and their luck is at half effectiveness (50% penalty).
    // Pure arithmetic; wiring it into sleeping and luck is T-046. How the luck penalty applies is still open.
    public static class StayAwakePenalty
    {
        public const float FullEnergyRecoveryPenalty = 1.0f;
        public const float FullLuckPenalty = 0.5f;

        public static float EnergyRecoveryPenalty(int checksPassed) => Fraction(checksPassed) * FullEnergyRecoveryPenalty;

        public static float LuckPenalty(int checksPassed) => Fraction(checksPassed) * FullLuckPenalty;

        // 1 - penalty: 1 means luck is fully effective, 0.5 means half effective.
        public static float LuckEffectiveness(int checksPassed) => 1f - LuckPenalty(checksPassed);

        // Sleeping restores energy towards `target` (full when sleeping, less when passing out). The penalty scales
        // how much of the missing energy comes back: none at a 100% penalty.
        public static int EnergyAfterSleep(int current, int target, int checksPassed)
        {
            if (target <= current) return current;
            var recovered = (target - current) * (1f - EnergyRecoveryPenalty(checksPassed));
            return current + (int)Math.Round(recovered);
        }

        static float Fraction(int checksPassed) =>
            Math.Max(0f, Math.Min(1f, checksPassed / (float)StayAwakeScheduler.ChecksPerNight));
    }
}
