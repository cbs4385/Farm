using Farm.Core;

namespace Farm.Gameplay
{
    // When the late-night "stay awake" quick-time check is due (owner design, GDD section 9, R and V).
    // From 20:00 onward the player must pass a quick-time event to stay awake, at most once per 20 game minutes.
    // Energy-spending actions do NOT trigger checks (the owner removed that rule); only the clock does.
    // Pure logic: it decides *when*; the look, difficulty and consequences are StayAwakeChallenge and StayAwakePenalty.
    public sealed class StayAwakeScheduler
    {
        public const int StartHour = 20;                                  // the first check is due at 20:00
        public const int StartMinuteOfDay = StartHour * 60;
        public const int IntervalMinutes = 20;

        // The day ends at 06:00 (decision U). Checks are due at 20:00, 20:20 ... 05:40: thirty in a night.
        public const int NightEndMinuteOfDay = 30 * 60;
        public const int ChecksPerNight = (NightEndMinuteOfDay - StartMinuteOfDay) / IntervalMinutes;

        int _day = -1;
        int _lastCheckMinute = -1;

        public int ChecksToday { get; private set; }

        // True from 20:00 on, including the small hours (hour 24 and later).
        public static bool IsActive(GameDateTime now) => now.MinuteOfDay >= StartMinuteOfDay;

        // Is a check due now? Does not change state: call MarkChecked once the check has been presented.
        public bool ShouldCheck(GameDateTime now)
        {
            if (!IsActive(now)) return false;
            return now.MinuteOfDay >= NextCheckMinute(now);
        }

        // The minute of day at which the next check is due: 20:00 first, then every 20 minutes after the last one.
        public int NextCheckMinute(GameDateTime now) =>
            _day == now.TotalDays && _lastCheckMinute >= 0 ? _lastCheckMinute + IntervalMinutes : StartMinuteOfDay;

        // Record that a check was presented at `now`; the 20-minute timer restarts from here.
        public void MarkChecked(GameDateTime now)
        {
            if (_day != now.TotalDays) { _day = now.TotalDays; ChecksToday = 0; }
            _lastCheckMinute = now.MinuteOfDay;
            ChecksToday++;
        }

        // Start the timer from `now` without counting a check (loading a game at night should not ambush the player).
        // `checksAlreadyDone` restores the count when a saved game is loaded mid-night.
        public void StartTimerAt(GameDateTime now, int checksAlreadyDone = 0)
        {
            _day = now.TotalDays;
            _lastCheckMinute = now.MinuteOfDay;
            ChecksToday = checksAlreadyDone;
        }

        public void Reset()
        {
            _day = -1;
            _lastCheckMinute = -1;
            ChecksToday = 0;
        }
    }
}
