using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-038: the full crop set (24 crops and 4 fruit trees), harvest quality, fertilizer, trees, and the greenhouse.
    public class CropTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        static Func<string, CropDefinition> Lookup(GameDatabase db) => id => db.TryGetCrop(id, out var c) ? c : null;

        // Plants a crop on a fresh tilled tile, waters it every day, and returns the days until it can be harvested.
        static int DaysToMature(CropDefinition crop, Season season, string fertilizer = null, bool allSeasons = false)
        {
            var grid = new FarmGrid { AllSeasons = allSeasons };
            grid.Till(1, 1);
            if (fertilizer != null) grid.Fertilize(1, 1, fertilizer);
            Assert.IsTrue(grid.Plant(1, 1, crop, season), crop.Id);
            for (var day = 1; day <= 200; day++)
            {
                grid.Water(1, 1);
                grid.AdvanceDay(season, false, id => crop);
                if (grid.IsMature(1, 1, id => crop)) return day;
            }
            return -1;
        }

        // ---- the table ------------------------------------------------------------------------------------------------------

        [Test]
        public void TheBaseGame_HasTwentyFourCropsAndFourTrees()
        {
            Assert.AreEqual(24, CropDefaults.Rows.Count(r => !r.IsTree));
            Assert.AreEqual(4, CropDefaults.Rows.Count(r => r.IsTree));
            var db = RealDb();
            Assert.AreEqual(28, db.Crops.Count);
            CollectionAssert.AllItemsAreUnique(CropDefaults.Rows.Select(r => r.Id).ToList());
        }

        [Test]
        public void EveryCrop_IsCompleteData_WithSeedAndHarvestItemsAndSprites()
        {
            var db = RealDb();
            foreach (var row in CropDefaults.Rows)
            {
                Assert.IsTrue(db.TryGetCrop(row.Id, out var crop), row.Id);
                Assert.AreEqual(row.Stages + 1, crop.StageSprites.Length, row.Id);
                Assert.IsTrue(crop.StageSprites.All(s => s != null), row.Id);
                Assert.IsTrue(db.TryGetItem(crop.SeedItemId, out var seed), crop.SeedItemId);
                Assert.IsTrue(db.TryGetItem(crop.HarvestItemId, out var harvest), crop.HarvestItemId);
                Assert.AreEqual(ItemCategory.Seed, seed.Category);
                Assert.AreEqual(ItemCategory.Crop, harvest.Category);
                Assert.AreEqual(row.Id, seed.CropId);
                Assert.IsNotNull(seed.Icon, seed.Id);
                Assert.IsNotNull(harvest.Icon, harvest.Id);
                Assert.IsTrue(seed.SoldIn.Contains("general"), $"{seed.Id} is sold in the general store");
                Assert.AreEqual(row.IsTree, crop.IsTree);
                Assert.AreEqual(row.Kind, crop.Kind);
            }
        }

        [Test]
        public void EveryCropSeason_IsCoveredBySomeCrop_ExceptWinter()
        {
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall })
                Assert.GreaterOrEqual(CropDefaults.Rows.Count(r => !r.IsTree && r.Seasons.Includes(season)), 8, season.ToString());
            Assert.IsFalse(CropDefaults.Rows.Any(r => !r.IsTree && r.Seasons.Includes(Season.Winter)), "winter is for the greenhouse");
        }

        [Test]
        public void EveryOrdinaryCrop_GrowsToMaturity_InExactlyItsGrowthDays()
        {
            var db = RealDb();
            foreach (var row in CropDefaults.Rows.Where(r => !r.IsTree))
            {
                db.TryGetCrop(row.Id, out var crop);
                var season = Enum.GetValues(typeof(Season)).Cast<Season>().First(s => crop.Seasons.Includes(s));
                Assert.AreEqual(row.Days.Sum(), DaysToMature(crop, season), row.Id);
            }
        }

        [Test]
        public void RegrowingCrops_ProduceAgainAfterTheirRegrowDays()
        {
            var db = RealDb();
            foreach (var row in CropDefaults.Rows.Where(r => !r.IsTree && r.Regrow > 0))
            {
                db.TryGetCrop(row.Id, out var crop);
                var season = Enum.GetValues(typeof(Season)).Cast<Season>().First(s => crop.Seasons.Includes(s));
                var grid = new FarmGrid();
                grid.Till(1, 1);
                grid.Plant(1, 1, crop, season);
                for (var d = 0; d < row.Days.Sum(); d++) { grid.Water(1, 1); grid.AdvanceDay(season, false, id => crop); }
                Assert.IsTrue(grid.TryHarvest(1, 1, id => crop, out _), row.Id);
                Assert.IsFalse(grid.IsMature(1, 1, id => crop), row.Id + " is not ready the same day");
                for (var d = 0; d < row.Regrow; d++) { grid.Water(1, 1); grid.AdvanceDay(season, false, id => crop); }
                Assert.IsTrue(grid.IsMature(1, 1, id => crop), $"{row.Id} regrows after {row.Regrow} days");
            }
        }

        [Test]
        public void OutOfSeasonCrops_Die_AndTwoSeasonCropsSurviveTheirSecondSeason()
        {
            var db = RealDb();
            db.TryGetCrop("corn", out var corn);                      // summer and fall
            db.TryGetCrop("tomato", out var tomato);                  // summer only
            var grid = new FarmGrid();
            grid.Till(1, 1); grid.Till(2, 2);
            grid.Plant(1, 1, corn, Season.Summer);
            grid.Plant(2, 2, tomato, Season.Summer);
            var died = grid.AdvanceDay(Season.Fall, false, Lookup(db));
            Assert.AreEqual(1, died);
            Assert.IsTrue(grid.TryGetTile(1, 1, out var c) && c.Crop != null, "corn lives into fall");
            Assert.IsFalse(grid.TryGetTile(2, 2, out var t) && t.Crop != null, "tomatoes do not");
            Assert.AreEqual(1, grid.AdvanceDay(Season.Winter, false, Lookup(db)), "nothing here survives winter");
        }

        [Test]
        public void SeedsCanOnlyBePlanted_InTheirSeasons()
        {
            var db = RealDb();
            db.TryGetCrop("pumpkin", out var pumpkin);
            var grid = new FarmGrid();
            grid.Till(1, 1);
            Assert.IsFalse(grid.CanPlant(1, 1, pumpkin, Season.Spring));
            Assert.IsTrue(grid.CanPlant(1, 1, pumpkin, Season.Fall));
        }

        // ---- fruit trees -----------------------------------------------------------------------------------------------------

        [Test]
        public void ATree_CanBePlantedAnyTime_GrowsWithoutWatering_AndNeverDiesWithTheSeason()
        {
            var db = RealDb();
            db.TryGetCrop("tree_apple", out var apple);
            var grid = new FarmGrid();
            grid.Till(1, 1);
            Assert.IsTrue(grid.Plant(1, 1, apple, Season.Winter), "a sapling goes in any season");
            var died = 0;
            for (var day = 0; day < 28; day++) died += grid.AdvanceDay(day < 14 ? Season.Winter : Season.Spring, false, Lookup(db));
            Assert.AreEqual(0, died);
            grid.TryGetTile(1, 1, out var tile);
            Assert.AreEqual(apple.MatureStage, tile.Crop.Stage, "28 days to grow, no watering needed");
        }

        [Test]
        public void AMatureTree_BearsFruitDailyInItsSeason_AndKeepsStanding()
        {
            var db = RealDb();
            db.TryGetCrop("tree_apple", out var apple);                // fruits in fall
            var grid = new FarmGrid();
            grid.Till(1, 1);
            grid.Plant(1, 1, apple, Season.Fall);
            for (var day = 0; day < 28; day++) grid.AdvanceDay(Season.Fall, false, Lookup(db));
            Assert.IsTrue(grid.IsMature(1, 1, Lookup(db)), "fruit is ready");

            Assert.IsTrue(grid.TryHarvest(1, 1, Lookup(db), out var result));
            Assert.AreEqual("crop.tree_apple", result.ItemId);
            Assert.IsFalse(grid.IsMature(1, 1, Lookup(db)), "no more fruit today");
            Assert.IsFalse(grid.TryHarvest(1, 1, Lookup(db), out _));
            Assert.IsTrue(grid.TryGetTile(1, 1, out var tile) && tile.Crop != null, "the tree stays");

            grid.AdvanceDay(Season.Fall, false, Lookup(db));
            Assert.IsTrue(grid.IsMature(1, 1, Lookup(db)), "new fruit overnight");
            grid.AdvanceDay(Season.Winter, false, Lookup(db));
            Assert.IsFalse(grid.IsMature(1, 1, Lookup(db)), "no fruit out of season");
        }

        [Test]
        public void EveryTree_BearsFruitInExactlyItsSeason()
        {
            var db = RealDb();
            foreach (var row in CropDefaults.Rows.Where(r => r.IsTree))
            {
                db.TryGetCrop(row.Id, out var tree);
                foreach (Season season in Enum.GetValues(typeof(Season)))
                {
                    var grid = new FarmGrid();
                    grid.Till(1, 1);
                    grid.Plant(1, 1, tree, season);
                    for (var d = 0; d < 28; d++) grid.AdvanceDay(season, false, Lookup(db));
                    grid.AdvanceDay(season, false, Lookup(db));
                    Assert.AreEqual(row.Seasons.Includes(season), grid.IsMature(1, 1, Lookup(db)), $"{row.Id} in {season}");
                }
            }
        }

        // ---- greenhouse -------------------------------------------------------------------------------------------------------

        [Test]
        public void InAGreenhouse_AnyCropGrowsInAnySeason_AndNothingDiesWithTheSeason()
        {
            var db = RealDb();
            db.TryGetCrop("pumpkin", out var pumpkin);               // fall only
            var outside = new FarmGrid();
            var inside = new FarmGrid { AllSeasons = true };
            foreach (var g in new[] { outside, inside }) g.Till(1, 1);
            Assert.IsFalse(outside.CanPlant(1, 1, pumpkin, Season.Winter));
            Assert.IsTrue(inside.CanPlant(1, 1, pumpkin, Season.Winter));
            inside.Plant(1, 1, pumpkin, Season.Winter);
            Assert.AreEqual(0, inside.AdvanceDay(Season.Winter, false, Lookup(db)));
            Assert.AreEqual(pumpkin.GrowthDays.Sum(), DaysToMature(pumpkin, Season.Winter, allSeasons: true));
        }

        [Test]
        public void TheGreenhouseDoesNotGetRain()
        {
            var db = RealDb();
            db.TryGetCrop("pumpkin", out var pumpkin);
            var state = GameState.NewGame("a", "b", id => 999, 1);
            state.Weather = "rain";
            var clock = new GameClock(new GameDateTime(1, Season.Fall, 10, 1000));
            state.SetDate(clock.Now);
            var field = new FarmGrid(); var house = new FarmGrid { AllSeasons = true };
            field.Till(1, 1); house.Till(1, 1);
            field.Plant(1, 1, pumpkin, Season.Fall); house.Plant(1, 1, pumpkin, Season.Fall);
            DayCycle.EndDay(state, clock, new Dictionary<string, FarmGrid> { { MapIds.Farm, field }, { MapIds.Greenhouse, house } },
                id => null, Lookup(db), false);
            field.TryGetTile(1, 1, out var a); house.TryGetTile(1, 1, out var b);
            Assert.AreEqual(1, a.Crop.Stage, "the field grew in the rain");
            Assert.AreEqual(0, b.Crop.Stage, "the greenhouse stayed dry");
        }

        // ---- fertilizer -------------------------------------------------------------------------------------------------------

        [Test]
        public void SpeedGro_ShortensEveryLongStageByADay()
        {
            var db = RealDb();
            db.TryGetCrop("pumpkin", out var pumpkin);               // 1,2,3,4,3 = 13 days; four stages last 2+ days
            var plain = DaysToMature(pumpkin, Season.Fall);
            var fast = DaysToMature(pumpkin, Season.Fall, ItemIds.FertilizerSpeed);
            Assert.AreEqual(13, plain);
            Assert.AreEqual(plain - 4, fast);
            db.TryGetCrop("parsnip", out var parsnip);               // every stage is one day: nothing to save
            Assert.AreEqual(DaysToMature(parsnip, Season.Spring), DaysToMature(parsnip, Season.Spring, ItemIds.FertilizerSpeed));
        }

        [Test]
        public void Fertilizer_IsPerTile_ClearedWhenTheCropIsGone()
        {
            var db = RealDb();
            db.TryGetCrop("parsnip", out var parsnip);
            var grid = new FarmGrid();
            Assert.IsFalse(grid.Fertilize(1, 1, ItemIds.FertilizerQuality), "only tilled soil");
            grid.Till(1, 1);
            Assert.IsTrue(grid.Fertilize(1, 1, ItemIds.FertilizerQuality));
            Assert.AreEqual(ItemIds.FertilizerQuality, grid.FertilizerAt(1, 1));
            grid.Plant(1, 1, parsnip, Season.Spring);
            for (var d = 0; d < 4; d++) { grid.Water(1, 1); grid.AdvanceDay(Season.Spring, false, Lookup(db)); }
            grid.TryHarvest(1, 1, Lookup(db), out _);
            Assert.IsNull(grid.FertilizerAt(1, 1), "used up with the crop");
        }

        // ---- quality ----------------------------------------------------------------------------------------------------------

        static float Share(Func<float, int> quality, int expected, int samples = 4000)
        {
            var hits = 0;
            for (var i = 0; i < samples; i++) if (quality((i + 0.5f) / samples) == expected) hits++;
            return hits / (float)samples;
        }

        [Test]
        public void CropQuality_ImprovesWithLevelFertilizerAndLuck()
        {
            var basic = Share(r => CropQuality.Roll(1, null, 0f, r), CropQuality.Normal);
            Assert.AreEqual(1f, basic, 0.001f, "a level 1 farmer with no help always harvests normal crops");
            var skilled = Share(r => CropQuality.Roll(10, null, 0f, r), CropQuality.Normal);
            var fertilised = Share(r => CropQuality.Roll(1, ItemIds.FertilizerQuality, 0f, r), CropQuality.Normal);
            var lucky = Share(r => CropQuality.Roll(1, null, 1f, r), CropQuality.Normal);
            Assert.Less(skilled, basic);
            Assert.Less(fertilised, basic);
            Assert.Less(lucky, basic);
            Assert.Less(Share(r => CropQuality.Roll(10, ItemIds.FertilizerQuality, 1f, r), CropQuality.Normal), skilled, "they stack");
        }

        [Test]
        public void CropQuality_BadLuckNeverMakesItWorseThanNormal_AndNeutralIsUnchanged()
        {
            Assert.AreEqual(CropQuality.Roll(5, null, 0f, 0.5f), CropQuality.Roll(5, null, -1f, 0.5f), "bad luck equals neutral luck");
            Assert.AreEqual(CropQuality.Normal, CropQuality.Roll(1, null, -1f, 0.0f));
            Assert.AreEqual(CropQuality.Gold, CropQuality.Roll(10, ItemIds.FertilizerQuality, 1f, 0.0f));
        }

        [Test]
        public void HigherQualityCrops_SellForMore()
        {
            var db = RealDb();
            var item = db.GetItem("crop.pumpkin");
            Assert.Greater(DayCycle.SellValue(item, CropQuality.Silver, 1), DayCycle.SellValue(item, CropQuality.Normal, 1));
            Assert.Greater(DayCycle.SellValue(item, CropQuality.Gold, 1), DayCycle.SellValue(item, CropQuality.Silver, 1));
        }

        // ---- saving -----------------------------------------------------------------------------------------------------------

        [Test]
        public void TreesAndFertilizer_SurviveASaveRoundTrip()
        {
            var db = RealDb();
            db.TryGetCrop("tree_cherry", out var cherry);
            var grid = new FarmGrid();
            grid.Till(3, 3);
            grid.Fertilize(3, 3, ItemIds.FertilizerSpeed);
            grid.Plant(3, 3, cherry, Season.Spring);
            for (var d = 0; d < 28; d++) grid.AdvanceDay(Season.Spring, false, Lookup(db));
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grid.ToList());
            var back = FarmGrid.FromTiles(Newtonsoft.Json.JsonConvert.DeserializeObject<List<FarmTile>>(json));
            back.TryGetTile(3, 3, out var tile);
            Assert.AreEqual(ItemIds.FertilizerSpeed, tile.Fertilizer);
            Assert.IsTrue(tile.Crop.Fruit);
        }
    }
}
