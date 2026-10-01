using System;

namespace Farm.Core
{
    public enum Season { Spring = 0, Summer = 1, Fall = 2, Winter = 3 }

    // One lunar cycle per season (28 days): every season starts on a new moon, and the moon is full on days 15-18.
    public enum MoonPhase
    {
        New = 0, WaxingCrescent, FirstQuarter, WaxingGibbous, Full, WaningGibbous, LastQuarter, WaningCrescent,
    }

    // Calendar position. Days run 06:00 to 26:00 (02:00 next morning): MinuteOfDay is 360..1559.
    // Year is 1-based, Day is 1..28.
    public readonly struct GameDateTime : IEquatable<GameDateTime>
    {
        public const int DaysPerSeason = 28;
        public const int SeasonsPerYear = 4;
        public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;
        public const int DayStartMinute = 6 * 60;
        public const int DayEndMinute = 26 * 60;

        public readonly int Year;
        public readonly Season Season;
        public readonly int Day;
        public readonly int MinuteOfDay;

        public GameDateTime(int year, Season season, int day, int minuteOfDay = DayStartMinute)
        {
            if (year < 1) throw new ArgumentOutOfRangeException(nameof(year));
            if (day < 1 || day > DaysPerSeason) throw new ArgumentOutOfRangeException(nameof(day));
            if (minuteOfDay < DayStartMinute || minuteOfDay > DayEndMinute) throw new ArgumentOutOfRangeException(nameof(minuteOfDay));
            Year = year;
            Season = season;
            Day = day;
            MinuteOfDay = minuteOfDay;
        }

        public static GameDateTime NewGame => new GameDateTime(1, Season.Spring, 1);

        public int Hour => MinuteOfDay / 60;
        public int Minute => MinuteOfDay % 60;

        // 0 = first day of Year 1 Spring.
        public int TotalDays => (Year - 1) * DaysPerYear + (int)Season * DaysPerSeason + (Day - 1);

        // 0 = Monday ... 6 = Sunday. Day 1 of every season is a Monday.
        public int DayOfWeek => (Day - 1) % 7;

        public bool IsDayOver => MinuteOfDay >= DayEndMinute;

        public MoonPhase MoonPhase => (MoonPhase)((Day - 1) * 8 / DaysPerSeason);

        public GameDateTime StartOfNextDay()
        {
            var day = Day + 1;
            var season = Season;
            var year = Year;
            if (day > DaysPerSeason)
            {
                day = 1;
                if (season == Season.Winter) { season = Season.Spring; year++; }
                else season = (Season)((int)season + 1);
            }
            return new GameDateTime(year, season, day);
        }

        // Clamps at the end of the day; the clock decides what happens then.
        public GameDateTime WithMinuteOfDay(int minuteOfDay) =>
            new GameDateTime(Year, Season, Day, Math.Min(Math.Max(minuteOfDay, DayStartMinute), DayEndMinute));

        public string ClockString()
        {
            var h = Hour % 24;
            var suffix = h >= 12 ? "PM" : "AM";
            var h12 = h % 12 == 0 ? 12 : h % 12;
            return $"{h12}:{Minute:00} {suffix}";
        }

        public bool Equals(GameDateTime other) =>
            Year == other.Year && Season == other.Season && Day == other.Day && MinuteOfDay == other.MinuteOfDay;

        public override bool Equals(object obj) => obj is GameDateTime o && Equals(o);
        public override int GetHashCode() => (TotalDays * 397) ^ MinuteOfDay;
        public override string ToString() => $"Y{Year} {Season} {Day}, {ClockString()}";
    }
}
