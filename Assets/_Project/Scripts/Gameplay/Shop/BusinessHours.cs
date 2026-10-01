using System.Collections.Generic;
using Farm.Core;

namespace Farm.Gameplay
{
    // When a business is open (owner decision, GDD section 9, AC): each business keeps hours that suit its type, and
    // shopkeepers have a regular weekly day off. The standard hours are 09:00-17:00.
    // Days of the week: 0 = Monday ... 6 = Sunday (GameDateTime.DayOfWeek). A negative day off means open every day.
    public sealed class BusinessHours
    {
        public const int StandardOpenMinute = 9 * 60;
        public const int StandardCloseMinute = 17 * 60;

        public readonly int OpenMinute;      // minute of day the doors open
        public readonly int CloseMinute;     // minute of day they close (may be after midnight for a late place)
        public readonly int DayOff;          // weekly day off, or -1

        public BusinessHours(int openMinute, int closeMinute, int dayOff = -1)
        {
            OpenMinute = openMinute;
            CloseMinute = closeMinute;
            DayOff = dayOff;
        }

        public static BusinessHours Standard(int dayOff = -1) => new BusinessHours(StandardOpenMinute, StandardCloseMinute, dayOff);

        public bool IsOpen(GameDateTime now) =>
            now.DayOfWeek != DayOff && now.MinuteOfDay >= OpenMinute && now.MinuteOfDay < CloseMinute;
    }

    // Shop id -> hours. Data registers its businesses here (the village maps, T-031); the condition atom
    // `open:<shopId>` lets doors, shop screens, NPC schedules and events ask whether a business is open.
    // A shop with no registered hours is treated as always open (so unfinished content never locks the player out).
    public static class BusinessHoursRegistry
    {
        static readonly Dictionary<string, BusinessHours> Hours = new Dictionary<string, BusinessHours>();

        public static void Register(string shopId, BusinessHours hours) => Hours[shopId] = hours;

        public static bool TryGet(string shopId, out BusinessHours hours) => Hours.TryGetValue(shopId, out hours);

        public static bool IsOpen(string shopId, GameDateTime now) =>
            !Hours.TryGetValue(shopId, out var h) || h.IsOpen(now);

        public static void Clear() => Hours.Clear();

        // Makes `open:<shopId>` available in condition expressions. Safe to call more than once.
        public static void RegisterConditionAtom() =>
            Conditions.Register("open", (shopId, world) => IsOpen(shopId, world.Now));
    }
}
