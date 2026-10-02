using System;
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

        static readonly string[] DayKeys = { "day.mon", "day.tue", "day.wed", "day.thu", "day.fri", "day.sat", "day.sun" };

        // "9:00 AM - 5:00 PM, closed Sun" for a door note.
        public string Describe()
        {
            string Clock(int minute) => new GameDateTime(1, Season.Spring, 1, Math.Max(GameDateTime.DayStartMinute, Math.Min(GameDateTime.DayEndMinute, minute))).ClockString();
            var text = $"{Clock(OpenMinute)} - {Clock(CloseMinute)}";
            return DayOff >= 0 && DayOff < DayKeys.Length ? L.Get("door.hours_with_day_off", text, L.Get(DayKeys[DayOff])) : text;
        }
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

        // What a locked door or counter tells the player: which business, and when it keeps hours.
        public static string ClosedMessage(string shopId)
        {
            var name = L.Get("business." + shopId);
            return TryGet(shopId, out var h) ? L.Get("door.closed", name, h.Describe()) : L.Get("door.closed_plain", name);
        }

        public static void Clear() => Hours.Clear();

        // The hours agreed with the owner (GDD decision AF). Days off are staggered so something is always open:
        // Monday blacksmith, Tuesday saloon, Wednesday carpenter, Thursday fish shop, Saturday clinic and library,
        // Sunday general store. Day numbers follow GameDateTime.DayOfWeek (0 = Monday).
        public static class Ids
        {
            public const string General = "general";
            public const string Blacksmith = "blacksmith";
            public const string Carpenter = "carpenter";
            public const string FishShop = "fish";
            public const string Clinic = "clinic";
            public const string Library = "library";
            public const string Saloon = "saloon";
            public const string TravelingMerchant = "merchant";
        }

        public static void RegisterDefaults()
        {
            Register(Ids.General, BusinessHours.Standard(dayOff: 6));
            Register(Ids.Blacksmith, BusinessHours.Standard(dayOff: 0));
            Register(Ids.Carpenter, BusinessHours.Standard(dayOff: 2));
            Register(Ids.FishShop, new BusinessHours(6 * 60, 14 * 60, dayOff: 3));       // early, for the morning catch
            Register(Ids.Clinic, BusinessHours.Standard(dayOff: 5));                      // clinic and library
            Register(Ids.Library, BusinessHours.Standard(dayOff: 5));
            Register(Ids.Saloon, new BusinessHours(12 * 60, 26 * 60, dayOff: 1));         // 12:00 to 02:00
            // The traveling merchant appears only on random days (T-057); these are the hours it keeps when it does.
            Register(Ids.TravelingMerchant, new BusinessHours(9 * 60, 21 * 60));
        }

        // Makes `open:<shopId>` available in condition expressions. Safe to call more than once.
        public static void RegisterConditionAtom() =>
            Conditions.Register("open", (shopId, world) => IsOpen(shopId, world.Now));
    }
}
