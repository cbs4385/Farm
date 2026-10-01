using Farm.Core;

namespace Farm.Gameplay
{
    // When the late-night "stay awake" quick-time check is due (owner design, GDD section 9, R). From 20:00 onward the
    // player must pass a quick-time event to stay awake. A check is due as soon as the player finishes an action that
    // spends energy, or when half an hour of game time has passed since the last check, whichever comes first.
    // Pure logic: it decides *when*; what the check looks like and its difficulty are separate (T-046).
    public sealed class StayAwakeScheduler
    {
        public const int StartHour = 20;                                  // 20:00
        public const int StartMinuteOfDay = StartHour * 60;
        public const int IntervalMinutes = 30;

        int _lastCheckDay = -1;
        int _lastCheckMinute = -1;

        // True from 20:00 on, including the small hours (hour 24-25) and anything after.
        public static bool IsActive(GameDateTime now) => now.MinuteOfDay >= StartMinuteOfDay;

        // Is a check due now? `energyActionFinished` is true on the tick where an energy-spending action just ended.
        // Does not change state: call MarkChecked once the check has been presented.
        public bool ShouldCheck(GameDateTime now, bool energyActionFinished)
        {
            if (!IsActive(now)) return false;
            if (energyActionFinished) return true;
            return now.MinuteOfDay >= NextTimedCheckMinute(now);
        }

        // The minute of day at which the half-hour timer fires next (20:30 for the first check of the night).
        public int NextTimedCheckMinute(GameDateTime now)
        {
            var sinceDay = _lastCheckDay == now.TotalDays ? _lastCheckMinute : StartMinuteOfDay;
            return sinceDay + IntervalMinutes;
        }

        // Record that a check was presented at `now`; the half-hour timer restarts from here.
        public void MarkChecked(GameDateTime now)
        {
            _lastCheckDay = now.TotalDays;
            _lastCheckMinute = now.MinuteOfDay;
        }

        // Start the half-hour timer from `now` without a check (loading a game at night should not ambush the player).
        public void StartTimerAt(GameDateTime now) => MarkChecked(now);

        public void Reset()
        {
            _lastCheckDay = -1;
            _lastCheckMinute = -1;
        }
    }
}
