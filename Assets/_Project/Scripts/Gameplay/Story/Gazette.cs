using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // T-146: the weekly Village Gazette. One issue per in-game week, assembled from dialogue sets (gazette.headline, .news, .farm, .weather,
    // .classified) whose entries carry conditions, so a week's news follows the season, the weather, the festival, the player's farm and
    // deeds. The same week of the same game always prints the same issue (the seed is the world seed and the week number).
    public sealed class GazetteIssue
    {
        public int Number;
        public string Headline;                                  // dialogue ids; the page turns them into text
        public List<string> News = new List<string>();
        public string Farm, Weather, Classified;
    }

    public static class Gazette
    {
        public const int NewsItems = 3;
        public const string Headline = "gazette.headline", News = "gazette.news", Farm = "gazette.farm", Weather = "gazette.weather", Classified = "gazette.classified";

        public static int WeekOf(GameDateTime now) => now.TotalDays / 7;

        public static GazetteIssue Build(StoryContent story, IWorldQuery world, int worldSeed)
        {
            var week = WeekOf(world.Now);
            var issue = new GazetteIssue { Number = week + 1 };
            issue.Headline = PickOne(story, Headline, world, worldSeed, week, 0);
            issue.News = PickMany(story, News, world, worldSeed, week, NewsItems);
            issue.Farm = PickOne(story, Farm, world, worldSeed, week, 1);
            issue.Weather = PickOne(story, Weather, world, worldSeed, week, 2);
            issue.Classified = PickOne(story, Classified, world, worldSeed, week, 3);
            return issue;
        }

        static int Seed(int worldSeed, int week, int slot, int attempt) => unchecked(worldSeed * 7919 + week * 104729 + slot * 1299709 + attempt * 15485863);

        static string PickOne(StoryContent story, string setId, IWorldQuery world, int worldSeed, int week, int slot)
        {
            var set = story.Set(setId);
            return set == null ? null : set.Pick(world, Seed(worldSeed, week, slot, 0));
        }

        // Distinct items: the pick is repeated with fresh seeds until `count` different lines are found (or the pool runs out).
        public static List<string> PickMany(StoryContent story, string setId, IWorldQuery world, int worldSeed, int week, int count)
        {
            var result = new List<string>();
            var set = story.Set(setId);
            if (set == null) return result;
            for (var attempt = 0; attempt < 60 && result.Count < count; attempt++)
            {
                var id = set.Pick(world, Seed(worldSeed, week, 10, attempt));
                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
            return result;
        }
    }
}
