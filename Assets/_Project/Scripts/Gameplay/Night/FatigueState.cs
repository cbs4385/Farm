using System;

namespace Farm.Gameplay
{
    // What the player carries between nights (owner decisions, GDD section 9, AA-d and AB).
    //   - Energy recovery depends only on how fatigued the player is at the moment they fall asleep.
    //   - The fatigue penalty on luck (and the meter) clears only after a sleep period in a bed. If the player
    //     collapses at 06:00 instead (falling asleep where they are), the fatigue they had is carried into the next day
    //     and keeps their luck reduced until they next sleep in a bed.
    // Pure logic; it is saved as one number (T-046).
    public sealed class FatigueState
    {
        public float Carried;   // 0..1: fatigue carried over from a sleep that was not in a bed

        public FatigueState() { }
        public FatigueState(float carried) { Carried = Math.Max(0f, Math.Min(1f, carried)); }

        // The rating that reduces luck and decides whether the meter is shown: the larger of what was carried over
        // and what the current time of night adds.
        public float LuckRating(int minuteOfDay) => Math.Max(Carried, FatigueModel.Fatigue(minuteOfDay));

        // The rating that reduces how much energy sleep restores: time awake tonight only. A carried penalty does not
        // cost a second night's recovery.
        public float RecoveryRating(int minuteOfDay) => FatigueModel.Fatigue(minuteOfDay);

        public bool MeterVisible(int minuteOfDay) => LuckRating(minuteOfDay) > 0f;

        public float Luck(float baseLuck, int minuteOfDay) => FatigueModel.ApplyToLuck(baseLuck, LuckRating(minuteOfDay));

        public int EnergyAfterSleep(int current, int target, int minuteOfDay) =>
            FatigueModel.EnergyAfterSleep(current, target, RecoveryRating(minuteOfDay));

        // The player slept at `minuteOfDay`. In a bed everything clears; anywhere else (collapsing at 06:00) the
        // fatigue they had is carried into the next day.
        public void AfterSleep(bool inBed, int minuteOfDay)
        {
            Carried = inBed ? 0f : Math.Max(Carried, FatigueModel.Fatigue(minuteOfDay));
        }
    }
}
