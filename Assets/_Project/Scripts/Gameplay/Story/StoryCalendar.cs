using System.Collections.Generic;
using Farm.Core;

namespace Farm.Gameplay
{
    // Pure calendar arithmetic for story conditions (T-092): how many days until a festival or a birthday.
    public static class StoryCalendar
    {
        static int DayOfYear(Season season, int day) => (int)season * GameDateTime.DaysPerSeason + (day - 1);

        // Days from `now` until the next occurrence of (season, day); 0 when it is today.
        public static int DaysUntil(GameDateTime now, Season season, int day)
        {
            var delta = DayOfYear(season, day) - DayOfYear(now.Season, now.Day);
            return delta >= 0 ? delta : delta + GameDateTime.DaysPerYear;
        }

        // The nearest festival (an event with a calendar entry), or -1 when none is defined.
        public static int DaysUntilFestival(IEnumerable<EventDefinition> events, GameDateTime now)
        {
            if (events == null) return -1;
            var best = -1;
            foreach (var e in events)
            {
                if (e == null || string.IsNullOrEmpty(e.Calendar) || e.CalendarSeason < 0 || e.CalendarSeason > 3 || e.CalendarDay < 1 || e.CalendarDay > GameDateTime.DaysPerSeason) continue;
                var days = DaysUntil(now, (Season)e.CalendarSeason, e.CalendarDay);
                if (best < 0 || days < best) best = days;
            }
            return best;
        }
    }
}
