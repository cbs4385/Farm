using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-050: which fish can bite, the timing mini-game, quality and luck.
    public class FishingTests
    {
        sealed class World : IWorldQuery
        {
            public GameDateTime Date = new GameDateTime(1, Season.Spring, 5, 10 * 60);
            public string W = "sunny";
            public bool HasFlag(string f) => false;
            public int GetVar(string n) => 0;
            public GameDateTime Now => Date;
            public string Weather => W;
            public string MapId => "Beach";
        }

        [SetUp] public void SetUp() { Conditions.ClearCustomForTests(); }

        [Test]
        public void TheTable_HasTwentyFish_AllWithItemsAndPrices()
        {
            Assert.AreEqual(20, FishDefaults.Rows.Length);
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            foreach (var f in FishDefaults.Rows)
            {
                Assert.IsTrue(db.TryGetItem(f.ItemId, out var item), f.Id);
                Assert.AreEqual(ItemCategory.Fish, item.Category);
                Assert.IsNotNull(item.Icon);
                Assert.Greater(item.SellPrice, 0);
                Assert.IsTrue(Conditions.Validate(f.Condition, out _), f.Id);
            }
            Assert.IsTrue(db.TryGetItem(FishDefaults.Rod, out var rod));
            Assert.AreEqual(ToolType.Rod, rod.ToolType);
            Assert.IsTrue(rod.SoldIn.Contains("fish"));
        }

        [Test]
        public void EveryOceanAndPondSeason_HasSomethingToCatchByDay()
        {
            foreach (var spot in new[] { FishSpots.Ocean, FishSpots.Pond })
                foreach (Season season in System.Enum.GetValues(typeof(Season)))
                    Assert.IsNotEmpty(FishingModel.Eligible(FishDefaults.Rows, spot, new World { Date = new GameDateTime(1, season, 5, 10 * 60) }), $"{spot} {season}");
        }

        [Test]
        public void Eligibility_FollowsSeasonWeatherTimeAndMoon()
        {
            var w = new World();
            bool Has(string id) => FishingModel.Eligible(FishDefaults.Rows, FishSpots.Ocean, w).Any(f => f.Id == id);
            Assert.IsFalse(Has("tuna"), "tuna are a summer fish");
            Assert.IsTrue(Has("sardine"));
            Assert.IsFalse(Has("eel"), "eels only bite in the rain");
            w.W = "rain";
            Assert.IsTrue(Has("eel"));
            w.Date = new GameDateTime(1, Season.Summer, 5, 22 * 60);
            Assert.IsTrue(Has("octopus"), "octopus at night");
            Assert.IsTrue(Has("tuna"));
            w.Date = new GameDateTime(1, Season.Summer, 5, 10 * 60);
            Assert.IsFalse(Has("octopus"));
            var pond = new World { Date = new GameDateTime(1, Season.Spring, 16, 22 * 60) };
            Assert.IsTrue(FishingModel.Eligible(FishDefaults.Rows, FishSpots.Pond, pond).Any(f => f.Id == "moonfish"), "full moon, late");
            pond.Date = new GameDateTime(1, Season.Spring, 3, 22 * 60);
            Assert.IsFalse(FishingModel.Eligible(FishDefaults.Rows, FishSpots.Pond, pond).Any(f => f.Id == "moonfish"));
        }

        [Test]
        public void Luck_MakesRareFishLikelier()
        {
            var list = FishingModel.Eligible(FishDefaults.Rows, FishSpots.Ocean, new World { Date = new GameDateTime(1, Season.Summer, 5, 22 * 60) });
            float Rare(float luck) { var c = 0; for (var i = 0; i < 3000; i++) if (FishingModel.Pick(list, luck, (i + .5f) / 3000f).Value.Weight <= 3) c++; return c; }
            Assert.Greater(Rare(0.9f), Rare(0f));
            Assert.Less(Rare(-0.9f), Rare(0f));
            Assert.IsFalse(FishingModel.Pick(new List<FishRow>(), 0f, 0.5f).HasValue);
        }

        [Test]
        public void ZonesShrinkWithDifficulty_AndGrowWithLevel()
        {
            Assert.Greater(FishingModel.ZoneWidth(1, 0.1f), FishingModel.ZoneWidth(1, 0.8f));
            Assert.Greater(FishingModel.ZoneWidth(10, 0.5f), FishingModel.ZoneWidth(1, 0.5f));
            Assert.GreaterOrEqual(FishingModel.ZoneWidth(1, 1f), 0.12f);
            Assert.LessOrEqual(FishingModel.ZoneWidth(10, 0f), 0.6f);
            Assert.Less(FishingModel.BiteDelay(1, true, 0.5f), FishingModel.BiteDelay(1, false, 0.5f));
            Assert.Less(FishingModel.BiteDelay(10, false, 0.5f), FishingModel.BiteDelay(1, false, 0.5f));
        }

        [Test]
        public void Judge_AcceptsTheZone_AndRewardsTheMiddle()
        {
            Assert.IsTrue(FishingModel.Judge(0.5f, 0.5f, 0.3f, out var perfect));
            Assert.IsTrue(perfect);
            Assert.IsTrue(FishingModel.Judge(0.62f, 0.5f, 0.3f, out perfect));
            Assert.IsFalse(perfect);
            Assert.IsFalse(FishingModel.Judge(0.8f, 0.5f, 0.3f, out _));
        }

        [Test]
        public void Quality_ImprovesWithLevelPerfectAndLuck()
        {
            float Normal(int level, bool perfect, float luck) { var c = 0; for (var i = 0; i < 2000; i++) if (FishingModel.Quality(level, luck, perfect, (i + .5f) / 2000f) == 0) c++; return c; }
            Assert.AreEqual(2000, Normal(1, false, 0f));
            Assert.Less(Normal(10, false, 0f), Normal(1, false, 0f));
            Assert.Less(Normal(1, true, 0f), Normal(1, false, 0f));
            Assert.Less(Normal(1, false, 1f), Normal(1, false, 0f));
        }

        static FishingSession Session(float zoneRoll = 0.5f)
        {
            var list = FishingModel.Eligible(FishDefaults.Rows, FishSpots.Ocean, new World());
            return new FishingSession(list, 1, 0f, false, 0f, 0f, zoneRoll, 0.99f);
        }

        [Test]
        public void ACast_WaitsForABite_ThenTheBar_AndCatchesInsideTheZone()
        {
            var s = Session();
            Assert.AreEqual(FishingState.Waiting, s.State);
            s.Press();
            Assert.AreEqual(FishingState.Waiting, s.State, "pressing early does nothing");
            for (var i = 0; i < 100 && s.State == FishingState.Waiting; i++) s.Tick(0.1f);
            Assert.AreEqual(FishingState.Bite, s.State);
            s.Press();
            Assert.AreEqual(FishingState.Bar, s.State);
            while (Mathf.Abs(s.Marker - s.ZoneCenter) > 0.02f) s.Tick(0.01f);
            s.Press();
            Assert.AreEqual(FishingState.Done, s.State);
            Assert.IsTrue(s.Caught);
            Assert.IsTrue(s.Fish.HasValue);
        }

        [Test]
        public void MissingTheBite_LosesTheFish()
        {
            var late = Session();
            for (var i = 0; i < 200 && late.State != FishingState.Done; i++) late.Tick(0.1f);
            Assert.IsFalse(late.Caught, "too slow");
        }

        [Test]
        public void MissingTheZone_LosesTheFish()
        {
            var miss = Session(0.9f);
            for (var i = 0; i < 100 && miss.State == FishingState.Waiting; i++) miss.Tick(0.1f);
            miss.Press();
            miss.Tick(0.05f);
            Assert.Greater(Mathf.Abs(miss.Marker - miss.ZoneCenter), miss.ZoneWidth / 2f, "the marker has barely moved");
            miss.Press();
            Assert.IsFalse(miss.Caught);
        }

        [Test]
        public void NothingEligible_NeverBites_AndCancelEndsTheCast()
        {
            var s = new FishingSession(new List<FishRow>(), 1, 0f, false, 0.5f, 0.5f, 0.5f, 0.5f);
            Assert.IsTrue(s.NothingBites);
            for (var i = 0; i < 400 && s.State != FishingState.Done; i++) s.Tick(0.1f);
            Assert.AreEqual(FishingState.Done, s.State);
            Assert.IsFalse(s.Caught);
            var c = Session();
            c.Cancel();
            Assert.AreEqual(FishingState.Done, c.State);
        }

        [Test]
        public void TheFishStall_SellsARodAndBait()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var stock = ShopCatalog.For(db, "fish", new World()).Select(i => i.Id).ToList();
            CollectionAssert.IsSupersetOf(stock, new[] { FishDefaults.Rod, FishDefaults.Bait });
        }
    }
}
