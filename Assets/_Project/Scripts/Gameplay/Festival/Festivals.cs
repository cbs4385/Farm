using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public enum FestivalKind { Dance, FishingContest, CropShow, Lanterns }

    // One festival of the village calendar. The dialogue that opens the day is in the story data (`festival_<id>` in Resources/Story/hall_and_festivals.json); this is what
    // the village does all day (playtest 2026-10-13: "there are no activities for the player or npc to engage in, such as dancing"). Ids are saved (flags are keyed by them).
    public sealed class FestivalRow
    {
        public readonly string Id;
        public readonly Season Season;
        public readonly int Day;
        public readonly FestivalKind Kind;

        public FestivalRow(string id, Season season, int day, FestivalKind kind) { Id = id; Season = season; Day = day; Kind = kind; }

        public string NameKey => "calendar.festival." + Id;
        public string Condition => $"season:{Season.ToString().ToLowerInvariant()} && day=={Day}";          // the whole day: the props stand from dawn
    }

    // The rules of the four festivals, pure: when they are on, who takes part and what is won. State is kept in flags and variables named by year, so a festival can be
    // taken part in again the next year and no new field is needed in the save.
    public static class Festivals
    {
        public const int OpenMinute = 10 * 60;           // the villagers gather at ten and the activities open
        public const int ResultsMinute = 17 * 60;        // the contest and the show are judged from five
        public const int CloseMinute = 21 * 60;          // the village goes home at nine

        public static readonly FestivalRow[] Rows =
        {
            new FestivalRow("spring", Season.Spring, 13, FestivalKind.Dance),
            new FestivalRow("summer", Season.Summer, 11, FestivalKind.FishingContest),
            new FestivalRow("fall", Season.Fall, 16, FestivalKind.CropShow),
            new FestivalRow("winter", Season.Winter, 25, FestivalKind.Lanterns),
        };

        public static FestivalRow Get(string id) => Rows.FirstOrDefault(r => r.Id == id);

        // The festival of this day, or null.
        public static FestivalRow Today(GameDateTime now) => Rows.FirstOrDefault(r => r.Season == now.Season && r.Day == now.Day);

        // On, and between opening and closing.
        public static bool IsOpen(GameDateTime now) => Today(now) != null && now.MinuteOfDay >= OpenMinute && now.MinuteOfDay < CloseMinute;

        public static string Key(FestivalRow row, int year, string what) => $"festival.{row.Id}.y{year}.{what}";

        // ---- who is there ----

        // Where the villagers stand in the village, in the order of NpcRoster.Ids (cells of the Village map), for each festival. All are open ground in front of or around the
        // festival's own props; a test opens the village scene and checks every one.
        public static readonly Dictionary<string, (int x, int y, string facing)[]> Gathering = new Dictionary<string, (int, int, string)[]>
        {
            // round the maypole on the dance floor east of the fountain
            { "spring", new[] { (28, 16, "right"), (30, 16, "left"), (28, 18, "right"), (30, 18, "left"), (29, 16, "up"), (29, 18, "down"), (28, 17, "right"), (30, 17, "left"),
                                (27, 15, "right"), (31, 16, "left"), (31, 18, "left"), (29, 15, "up") } },
            // along the shore of the village pond, facing the water
            { "summer", new[] { (16, 13, "down"), (17, 13, "down"), (18, 13, "down"), (19, 13, "down"), (20, 13, "down"), (21, 13, "down"), (15, 13, "down"), (22, 13, "down"),
                                (24, 14, "left"), (21, 14, "down"), (17, 14, "down"), (19, 14, "down") } },
            // in front of the stalls and the judging table in the square
            { "fall", new[] { (29, 21, "up"), (30, 21, "up"), (31, 21, "up"), (34, 21, "up"), (35, 21, "up"), (12, 21, "up"), (13, 21, "up"), (14, 21, "up"),
                              (15, 21, "up"), (36, 22, "left"), (31, 20, "up"), (11, 21, "up") } },
            // round the long table along the south edge of the square, above and below it
            { "winter", new[] { (21, 15, "down"), (23, 15, "down"), (24, 15, "down"), (26, 15, "down"), (27, 15, "down"), (29, 15, "down"), (21, 13, "up"), (23, 13, "up"),
                                (27, 13, "up"), (29, 13, "up"), (22, 13, "up"), (28, 13, "up") } },
        };

        public static int IndexOf(string npcId) => System.Array.IndexOf(NpcIds.All, npcId);

        // The gathering cell of the n-th villager of the roster for a festival (wraps round when there are more villagers than cells).
        public static (int x, int y, string facing) CellFor(FestivalRow row, int index)
        {
            var cells = Gathering[row.Id];
            return cells[((index % cells.Length) + cells.Length) % cells.Length];
        }

        // ---- the contest of the Tidewatch ----

        public const int ContestUntilMinute = ResultsMinute;

        // Points for a fish: what it would sell for, a third more for a silver one, two thirds more for a gold one.
        public static int CatchPoints(int sellPrice, int quality) => sellPrice * (3 + System.Math.Max(0, System.Math.Min(2, quality))) / 3;

        // The four villagers who enter (Felix, who runs it, is the one to beat) and the scores they catch, the same for the same game and year.
        public static readonly string[] ContestEntrants = { NpcRoster.Felix, NpcRoster.Hazel, NpcRoster.Dorian, NpcRoster.Wren };

        public static int EntrantScore(string npcId, int worldSeed, int year, bool show)
        {
            var salt = StableHash(npcId) + year * 31 + (show ? 977 : 0);
            var unit = WeatherRoller.Unit(salt, worldSeed ^ 0x2545F491);
            switch (npcId)
            {
                case NpcRoster.Felix: return show ? 120 + (int)(unit * 90f) : 520 + (int)(unit * 280f);
                case NpcRoster.Wren: return show ? 150 + (int)(unit * 200f) : 180 + (int)(unit * 260f);
                default: return show ? 90 + (int)(unit * 220f) : 150 + (int)(unit * 330f);
            }
        }

        static int StableHash(string s)
        {
            unchecked
            {
                var h = 17;
                foreach (var ch in s ?? string.Empty) h = h * 31 + ch;
                return System.Math.Abs(h);
            }
        }

        // The crop show's entrants: the ones who bring their harvest or their preserves.
        public static readonly string[] ShowEntrants = { NpcIds.Tilda, NpcIds.Bram, NpcIds.Ione, NpcRoster.Wren };

        public readonly struct Standing
        {
            public readonly string Name;       // an NpcId, or "" for the player
            public readonly int Score;
            public Standing(string name, int score) { Name = name; Score = score; }
        }

        // The table, best first. The player wins a tie.
        public static List<Standing> Table(string[] entrants, int worldSeed, int year, bool show, int playerScore)
        {
            var list = entrants.Select(e => new Standing(e, EntrantScore(e, worldSeed, year, show))).ToList();
            list.Add(new Standing(string.Empty, playerScore));
            return list.OrderByDescending(s => s.Score).ThenBy(s => s.Name.Length > 0).ToList();
        }

        // 1 for the best score ... entrants + 1 for the worst.
        public static int Place(List<Standing> table) => table.FindIndex(s => s.Name.Length == 0) + 1;

        // What each place wins: gold, and for the first place something to keep.
        public static int Prize(int place) => place == 1 ? 600 : place == 2 ? 300 : place == 3 ? 150 : 50;

        public const string ContestTrophy = "prop.trophy";
        public const string ShowRibbon = "prop.ribbon";

        // ---- the dance ----

        public const int DanceFriendship = 40;           // with the villager who dances with the player
        public const int DanceEnergy = 25;
        public const float DanceSeconds = 8f;

        // ---- the feast and the lanterns ----

        public const int FeastFriendship = 20;
        public const int LanternFriendship = 30;
        public const int WishCount = 6;

        // Which wish a lantern carries this year (the key `festival.wish.<n>`).
        public static int WishFor(int worldSeed, int year) => (int)(WeatherRoller.Unit(year * 7 + 3, worldSeed ^ 0x51ED270B) * WishCount) % WishCount;
    }
}
