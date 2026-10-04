using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-146: the weekly Village Gazette.
    public class GazetteTests
    {
        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        static StateWorldQuery World(Season season, int day, string weather = "sunny", int year = 1)
        {
            var state = new GameState { Weather = weather };
            state.SetDate(new GameDateTime(year, season, day, 8 * 60));
            return new StateWorldQuery(state, null);
        }

        [Test]
        public void AnIssue_HasAHeadline_ThreeDistinctNewsItems_AndThreeCorners()
        {
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter })
                for (var day = 1; day <= 28; day += 7)
                {
                    var issue = Gazette.Build(_story, World(season, day), 1234);
                    Assert.IsNotNull(issue.Headline, season + " " + day);
                    Assert.AreEqual(Gazette.NewsItems, issue.News.Count, season + " " + day);
                    Assert.AreEqual(issue.News.Count, issue.News.Distinct().Count());
                    Assert.IsNotNull(issue.Farm);
                    Assert.IsNotNull(issue.Weather);
                    Assert.IsNotNull(issue.Classified);
                    foreach (var id in new[] { issue.Headline, issue.Farm, issue.Weather, issue.Classified }.Concat(issue.News))
                        Assert.IsNotNull(_story.Dialogue(id), id);
                }
        }

        [Test]
        public void TheSameWeek_PrintsTheSameIssue_AndTheWeeksDiffer()
        {
            var a = Gazette.Build(_story, World(Season.Spring, 3), 99);
            var b = Gazette.Build(_story, World(Season.Spring, 5), 99);   // same week (days 1-7)
            Assert.AreEqual(a.Headline, b.Headline);
            CollectionAssert.AreEqual(a.News, b.News);
            var headlines = Enumerable.Range(0, 16).Select(w => Gazette.Build(_story, World(Season.Fall, 1 + (w % 4) * 7, year: 1 + w / 4), 99).Headline).Distinct().Count();
            Assert.GreaterOrEqual(headlines, 5, "different weeks, different front pages");
        }

        [Test]
        public void TheNews_FollowsTheSeasonAndTheWeather()
        {
            var winter = Enumerable.Range(0, 30).SelectMany(s => Gazette.PickMany(_story, Gazette.News, World(Season.Winter, 3), s, 1, 3)).Distinct().ToList();
            Assert.IsFalse(winter.Contains("gazette.news.blossom"), "no spring garlands in winter");
            Assert.IsTrue(winter.Contains("gazette.news.lantern"));
            var rain = Gazette.Build(_story, World(Season.Spring, 3, "rain"), 5);
            Assert.AreEqual("gazette.weather.rain", Enumerable.Range(0, 20).Select(s => Gazette.Build(_story, World(Season.Spring, 3, "rain"), s).Weather).First(w => w != "gazette.weather.spring"));
        }

        [Test]
        public void EveryItem_HasText_AndKeepsToTheHouseStyle()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var id in new[] { Gazette.Headline, Gazette.News, Gazette.Farm, Gazette.Weather, Gazette.Classified })
                foreach (var e in _story.Set(id).Entries)
                {
                    var key = _story.Dialogue(e.Dialogue).Nodes[0].Text;
                    Assert.IsTrue(en.ContainsKey(key), e.Dialogue);
                    Assert.LessOrEqual(en[key].Length, 140, e.Dialogue + " fits a column");
                    Assert.IsFalse(en[key].Contains("!"), e.Dialogue + ": a newspaper does not shout");
                }
            foreach (var key in new[] { "menu.tab.gazette", "gazette.masthead", "gazette.issue", "gazette.corner.farm", "gazette.corner.weather", "gazette.corner.classified" })
                Assert.IsTrue(en.ContainsKey(key), key);
        }

        [Test]
        public void FarmCornerItems_NeverNameAVillagerWhoseHeartsTheyDoNotCheck()
        {
            // Corners that mention a person by name must be conditioned on knowing them (hearts), so a new player never reads a stranger's name.
            foreach (var e in _story.Set(Gazette.News).Entries.Where(e => new[] { "Wren", "Hazel", "Bram", "Tilda", "Juno", "Piper" }.Any(n => _story.Dialogue(e.Dialogue) != null && e.Dialogue.Contains(n.ToLowerInvariant()))))
                StringAssert.Contains("hearts:", e.Condition ?? string.Empty, e.Dialogue);
        }
    }
}
