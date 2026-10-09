using Farm.Core;

namespace Farm.Gameplay
{
    // Which music belongs where and when (pure, so it can be tested). Eighteen tracks: the title theme; the farm theme in each season, and the farm at night; a theme
    // each for the village, the forest, the beach, the saloon, the library, the other indoors places, the mine and the rain; and a festival theme for each season.
    // Priority: the menu; the places with a theme of their own (the mine, the saloon, the library, indoors); then outdoors: night, a festival in the village, rain,
    // the place's own daytime theme, and last the farm theme of the season. Harrow Wood is left to its ambience (it has no track).
    public static class MusicChoice
    {
        public const string Title = "title", FarmNight = "farm_night";
        public const string Village = "village", Forest = "forest", Beach = "beach", Saloon = "saloon", Library = "library", Indoors = "indoors", Mine = "mine", Rain = "rain";
        public const int NightFrom = 20, NightUntil = 6;           // hours: from 20:00 until 06:00

        public static string FarmDay(Season season) => "farm_" + season.ToString().ToLowerInvariant();
        public static string Festival(Season season) => "festival_" + season.ToString().ToLowerInvariant();

        public static readonly string[] AllCues =
        {
            Title, "farm_spring", "farm_summer", "farm_fall", "farm_winter", FarmNight,
            Village, Forest, Beach, Saloon, Library, Indoors, Mine, Rain,
            "festival_spring", "festival_summer", "festival_fall", "festival_winter",
        };

        // `hour` is the hour of the game day (06 to 29: after midnight counts on from 24). `rain` is a wet day (rain or storm); `festivalToday` is the day of a festival.
        public static string For(string sceneName, bool inGame, Season season, int hour, bool rain = false, bool festivalToday = false)
        {
            if (!inGame || sceneName == SceneNames.MainMenu || sceneName == SceneNames.Bootstrap) return Title;
            if (Silent(sceneName)) return null;
            if (sceneName == MapIds.Mine || System.Array.IndexOf(MapIds.Dungeons, sceneName) >= 0) return Mine;
            if (sceneName == MapIds.Saloon) return Saloon;
            if (sceneName == MapIds.Library) return Library;
            if (IsIndoors(sceneName)) return Indoors;

            var night = hour >= NightFrom || hour < NightUntil;
            if (night) return FarmNight;
            var outdoors = sceneName == MapIds.Farm || sceneName == MapIds.Village || sceneName == MapIds.Forest || sceneName == MapIds.Beach;
            if (festivalToday && sceneName == MapIds.Village) return Festival(season);
            if (rain && outdoors) return Rain;
            if (sceneName == MapIds.Village) return Village;
            if (sceneName == MapIds.Forest) return Forest;
            if (sceneName == MapIds.Beach) return Beach;
            return FarmDay(season);
        }

        // The shops, the clinic, the hall, the farmhouse and the villagers' homes (the saloon and the library have themes of their own).
        public static bool IsIndoors(string mapId) =>
            mapId == MapIds.FarmHouse || mapId == MapIds.GeneralStore || mapId == MapIds.Blacksmith || mapId == MapIds.Carpenter || mapId == MapIds.Clinic
            || mapId == MapIds.CommunityHall || MapIds.IsHome(mapId);

        // Places that keep to their own sounds: Harrow Wood has no music of its own (yet).
        public static bool Silent(string mapId) => mapId == MapIds.Woods;
    }
}
