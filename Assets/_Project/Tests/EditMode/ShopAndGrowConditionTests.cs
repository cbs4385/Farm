using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class ShopCatalogTests
    {
        sealed class World : IWorldQuery
        {
            public HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string f) => Flags.Contains(f);
            public int GetVar(string n) => 0;
            public GameDateTime Date = GameDateTime.NewGame;
            public GameDateTime Now => Date;
            public string Weather => "sunny";
            public string MapId => "Farm";
        }

        [Test]
        public void GeneralStore_OffersOnlyItemsThatOptIn_SoPackSeedsNeverAppearByAccident()
        {
            var core = ItemDefinition.Create("seed.parsnip", ItemCategory.Seed, buyPrice: 20, soldIn: new[] { "general" });
            var db = GameDatabase.Create(new[] { core }, new CropDefinition[0]);

            // A priced item from an optional pack that does not list any shop.
            var strange = ItemDefinition.Create("seed.nightbloom", ItemCategory.Seed, buyPrice: 5);
            db.Merge(ContentPack.Create("pack", new[] { strange }, null));

            var stock = ShopCatalog.For(db, "general", new World());

            CollectionAssert.AreEqual(new[] { "seed.parsnip" }, stock.Select(i => i.Id).ToArray());
        }

        [Test]
        public void ItemsCanBeSoldInOtherShops_AndSaleConditionsGateStock()
        {
            var hidden = ItemDefinition.Create("seed.hidden", ItemCategory.Seed, buyPrice: 50,
                soldIn: new[] { "peddler" }, saleCondition: "flag:peddler.trusted");
            var plain = ItemDefinition.Create("seed.plain", ItemCategory.Seed, buyPrice: 10, soldIn: new[] { "peddler", "general" });
            var db = GameDatabase.Create(new[] { hidden, plain }, new CropDefinition[0]);
            var world = new World();

            Assert.AreEqual(new[] { "seed.plain" }, ShopCatalog.For(db, "peddler", world).Select(i => i.Id).ToArray());
            Assert.AreEqual(new[] { "seed.plain" }, ShopCatalog.For(db, "general", world).Select(i => i.Id).ToArray());

            world.Flags.Add("peddler.trusted");
            Assert.AreEqual(new[] { "seed.plain", "seed.hidden" }, ShopCatalog.For(db, "peddler", world).Select(i => i.Id).ToArray());
        }

        [Test]
        public void ZeroPriceOrBadCondition_IsNeverOffered()
        {
            var free = ItemDefinition.Create("x.free", ItemCategory.Misc, buyPrice: 0, soldIn: new[] { "general" });
            var broken = ItemDefinition.Create("x.broken", ItemCategory.Misc, buyPrice: 5, soldIn: new[] { "general" }, saleCondition: "nonsense:1");
            var db = GameDatabase.Create(new[] { free, broken }, new CropDefinition[0]);
            Assert.IsEmpty(ShopCatalog.For(db, "general", new World()));
        }

        [Test]
        public void GeneratedCoreSeeds_AreAllInTheGeneralStore()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var seeds = db.AllItems.Where(i => i.Category == ItemCategory.Seed).ToList();
            Assert.IsNotEmpty(seeds);
            // Seeds are on sale only while they can be planted; saplings any time.
            foreach (Season season in System.Enum.GetValues(typeof(Season)))
            {
                var stock = ShopCatalog.For(db, "general", new World { Date = new GameDateTime(1, season, 5) }).Select(i => i.Id).ToList();
                foreach (var seed in seeds)
                {
                    db.TryGetCrop(seed.CropId, out var crop);
                    var expected = crop.IsTree || crop.Seasons.Includes(season);
                    Assert.AreEqual(expected, stock.Contains(seed.Id), $"{seed.Id} in {season}");
                }
            }
        }

        [Test]
        public void EveryCrop_CanBeBoughtInItsSeason()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            foreach (var crop in db.Crops)
            {
                var season = crop.IsTree ? Season.Spring : System.Enum.GetValues(typeof(Season)).Cast<Season>().First(s => crop.Seasons.Includes(s));
                var stock = ShopCatalog.For(db, "general", new World { Date = new GameDateTime(1, season, 5) }).Select(i => i.Id);
                CollectionAssert.Contains(stock, crop.SeedItemId, crop.Id);
            }
        }
    }

    public class GrowConditionTests
    {
        sealed class World : IWorldQuery
        {
            public int Dread;
            public bool HasFlag(string f) => false;
            public int GetVar(string n) => n == "dread" ? Dread : 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Farm";
        }

        CropDefinition _normal, _dreadCrop;
        Dictionary<string, CropDefinition> _crops;

        [SetUp]
        public void SetUp()
        {
            _normal = CropDefinition.Create("parsnip", new[] { 1, 1 }, SeasonMask.All);
            _dreadCrop = CropDefinition.Create("nightbloom", new[] { 1, 1 }, SeasonMask.All);
            _dreadCrop.SetGrowCondition("var:dread>=20");
            _crops = new Dictionary<string, CropDefinition> { { "parsnip", _normal }, { "nightbloom", _dreadCrop } };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_normal);
            Object.DestroyImmediate(_dreadCrop);
        }

        CropDefinition Lookup(string id) => _crops[id];

        static CropInstance Crop(FarmGrid g, int x) { g.TryGetTile(x, 0, out var t); return t.Crop; }

        [Test]
        public void ConditionedCrop_IsDormantUntilTheConditionHolds_ThenGrows()
        {
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Till(1, 0);
            grid.Plant(0, 0, _normal, Season.Spring);
            grid.Plant(1, 0, _dreadCrop, Season.Spring);
            var world = new World { Dread = 5 };

            for (var i = 0; i < 3; i++)
            {
                grid.Water(0, 0); grid.Water(1, 0);
                grid.AdvanceDay(Season.Spring, false, Lookup, world);
            }
            Assert.AreEqual(2, Crop(grid, 0).Stage, "the normal crop matured");
            Assert.AreEqual(0, Crop(grid, 1).Stage, "the dread crop stayed dormant");
            Assert.IsNotNull(Crop(grid, 1), "dormant crops do not die");

            world.Dread = 25;
            for (var i = 0; i < 2; i++)
            {
                grid.Water(1, 0);
                grid.AdvanceDay(Season.Spring, false, Lookup, world);
            }
            Assert.AreEqual(2, Crop(grid, 1).Stage, "it grows once dread is high enough");
        }

        [Test]
        public void WithoutAWorld_ConditionsAreIgnored_AndCropsGrowNormally()
        {
            var grid = new FarmGrid();
            grid.Till(1, 0);
            grid.Plant(1, 0, _dreadCrop, Season.Spring);
            grid.Water(1, 0);
            grid.AdvanceDay(Season.Spring, false, Lookup);
            Assert.AreEqual(1, Crop(grid, 1).Stage);
        }

        [Test]
        public void ADayCycleHook_CanTurnNormalCropsIntoMutatedOnes()
        {
            // The hook API is enough for "normal plants change as dread increases": swap the crop id in place.
            var grid = new FarmGrid();
            grid.Till(0, 0);
            grid.Plant(0, 0, _normal, Season.Spring);
            var state = GameState.NewGame("T", "F", _ => 999);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 3, 1000));
            state.SetDate(clock.Now);
            state.Vars["dread"] = 30;
            var hooks = new GameHooks();
            hooks.AddDayCycleHook(new MutateHook());

            DayCycle.EndDay(state, clock, new Dictionary<string, FarmGrid> { { MapIds.Farm, grid } },
                id => null, Lookup, false, hooks);

            Assert.AreEqual("nightbloom", Crop(grid, 0).CropId);
        }

        sealed class MutateHook : DayCycleHook
        {
            public override void OnNightFalls(DayCycleContext c)
            {
                if (c.State.Vars.TryGetValue("dread", out var d) && d >= 20)
                    foreach (var grid in c.Grids.Values)
                        foreach (var tile in grid.Tiles)
                            if (tile.Crop != null && tile.Crop.CropId == "parsnip") tile.Crop.CropId = "nightbloom";
            }
        }
    }
}
