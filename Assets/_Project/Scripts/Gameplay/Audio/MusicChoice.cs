using Farm.Core;

namespace Farm.Gameplay
{
    // Which music belongs where and when (pure, so it can be tested). Today there are six tracks: the title theme, the farm theme in each
    // season, and the farm at night. Every outdoor and indoor place of the village uses them until it gets a theme of its own; the caves and the
    // wood are left to their ambience.
    public static class MusicChoice
    {
        public const string Title = "title", FarmNight = "farm_night";
        public const int NightFrom = 20, NightUntil = 6;           // hours: from 20:00 until 06:00

        public static string FarmDay(Season season) => "farm_" + season.ToString().ToLowerInvariant();

        public static readonly string[] AllCues = { Title, "farm_spring", "farm_summer", "farm_fall", "farm_winter", FarmNight };

        // `hour` is the hour of the game day (06 to 29: after midnight counts on from 24).
        public static string For(string sceneName, bool inGame, Season season, int hour)
        {
            if (!inGame || sceneName == SceneNames.MainMenu || sceneName == SceneNames.Bootstrap) return Title;
            if (Silent(sceneName)) return null;
            var night = hour >= NightFrom || hour < NightUntil;
            return night ? FarmNight : FarmDay(season);
        }

        // Places that keep to their own sounds: the mine, the dungeons and Harrow Wood.
        public static bool Silent(string mapId) => mapId == MapIds.Mine || mapId == MapIds.Woods || System.Array.IndexOf(MapIds.Dungeons, mapId) >= 0;
    }
}
