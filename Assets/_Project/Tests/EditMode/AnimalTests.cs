using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-053: the daily loop of looking after animals, saving, and the `animal` world-object source.
    public class AnimalTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        static Inventory Feed(int n)
        {
            var inv = new Inventory(12, id => 999);
            inv.Add(AnimalRules.Feed, n);
            return inv;
        }

        [Test]
        public void TheTable_HasSixAnimals_WithItemsAndProducts()
        {
            Assert.AreEqual(6, AnimalDefaults.Rows.Length);
            var db = RealDb();
            foreach (var r in AnimalDefaults.Rows)
            {
                Assert.IsTrue(db.TryGetItem(r.ItemId, out var item), r.Id);
                Assert.AreEqual(ItemCategory.Animal, item.Category);
                Assert.AreEqual(r.Id, item.PlaceableId);
                Assert.AreEqual(r.Price, item.BuyPrice);
                Assert.IsTrue(item.SoldIn.Contains("carpenter"));
                Assert.IsTrue(db.TryGetItem(r.ProductItemId, out var product), r.ProductItemId);
                Assert.IsNotNull(product.Icon);
            }
            Assert.AreEqual(3, AnimalDefaults.Rows.Count(r => r.Building == MapIds.Coop));
            Assert.AreEqual(3, AnimalDefaults.Rows.Count(r => r.Building == MapIds.Barn));
        }

        [Test]
        public void ABuilding_HoldsFourAnimals()
        {
            var state = new GameState();
            for (var i = 0; i < 4; i++) Assert.IsNotNull(AnimalRules.Add(state, "chicken", "c" + i, "Hen"));
            Assert.IsNull(AnimalRules.Add(state, "chicken", "c5", "Hen"), "full");
            Assert.IsNotNull(AnimalRules.Add(state, "cow", "k1", "Cow"), "the barn is separate");
            Assert.IsNull(AnimalRules.Add(state, "dragon", "x", "x"));
        }

        [Test]
        public void FedAnimals_MakeProductOnSchedule_AndHungryOnesDoNot()
        {
            var state = new GameState();
            var hen = AnimalRules.Add(state, "chicken", "h", "Hen");
            var duck = AnimalRules.Add(state, "duck", "d", "Duck");          // every two days
            for (var day = 1; day <= 2; day++)
            {
                AnimalRules.FeedAll(state, MapIds.Coop, Feed(5));
                AnimalRules.NewDay(state);
                if (day == 1) { Assert.IsTrue(hen.ProductReady); Assert.IsFalse(duck.ProductReady); }
            }
            Assert.IsTrue(duck.ProductReady);

            var hungry = AnimalRules.Add(state, "chicken", "h2", "Hen2");
            var before = hungry.Happiness;
            AnimalRules.NewDay(state);
            Assert.IsFalse(hungry.ProductReady);
            Assert.Less(hungry.Happiness, before);
        }

        [Test]
        public void Feeding_UsesOneFeedPerAnimal_AndStopsWhenOut()
        {
            var state = new GameState();
            for (var i = 0; i < 3; i++) AnimalRules.Add(state, "chicken", "c" + i, "Hen");
            var pack = Feed(2);
            Assert.AreEqual(2, AnimalRules.FeedAll(state, MapIds.Coop, pack));
            Assert.AreEqual(0, pack.Count(AnimalRules.Feed));
            Assert.AreEqual(2, state.Animals.Count(a => a.FedToday));
            Assert.AreEqual(0, AnimalRules.FeedAll(state, MapIds.Coop, Feed(0)));
        }

        [Test]
        public void Petting_OnceADay_RaisesHappiness_AndQualityFollowsIt()
        {
            var a = new AnimalState { Type = "cow", Happiness = 50 };
            Assert.IsTrue(AnimalRules.Pet(a));
            Assert.IsFalse(AnimalRules.Pet(a));
            Assert.AreEqual(58, a.Happiness);
            Assert.AreEqual(0, AnimalRules.Quality(new AnimalState { Happiness = 59 }));
            Assert.AreEqual(1, AnimalRules.Quality(new AnimalState { Happiness = 60 }));
            Assert.AreEqual(2, AnimalRules.Quality(new AnimalState { Happiness = 95 }));
        }

        [Test]
        public void Collecting_GivesTheProduct_ResetsTheClock_AndNeedsRoom()
        {
            var a = new AnimalState { Type = "chicken", Happiness = 95, ProductReady = true, DaysSinceProduct = 1 };
            var full = new Inventory(1, id => 1);
            full.Add("filler", 1);
            Assert.IsFalse(AnimalRules.Collect(a, full, out _));
            Assert.IsTrue(a.ProductReady);
            var pack = new Inventory(4, id => 999);
            Assert.IsTrue(AnimalRules.Collect(a, pack, out var item));
            Assert.AreEqual("product.egg", item);
            Assert.AreEqual(1, pack.Count("product.egg"));
            Assert.AreEqual(2, pack.Get(0).Quality);
            Assert.IsFalse(a.ProductReady);
            Assert.IsFalse(AnimalRules.Collect(a, pack, out _));
        }

        [Test]
        public void TheDailyLoop_RunsThroughTheNightlyCycle_AndSaves()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                AnimalRules.Add(s.State, "chicken", "h", "Hen");
                s.Backpack.Add(AnimalRules.Feed, 3);
                AnimalRules.FeedAll(s.State, MapIds.Coop, s.Backpack);
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 1000));
                s.EndDay(false);
                Assert.IsTrue(s.State.Animals[0].ProductReady);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(s.State);
                var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
                Assert.AreEqual("h", back.Animals[0].Id);
                Assert.IsTrue(back.Animals[0].ProductReady);
            }
        }

        [Test]
        public void Animals_AreAWorldObjectSource_AndConsumingOneRemovesItQuietly()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                AnimalRules.Add(s.State, "chicken", "h", "Hen");
                AnimalRules.Add(s.State, "cow", "k", "Cow");
                var refs = s.Hooks.EnumerateWorldObjects(WorldObjectKinds.Animal);
                CollectionAssert.AreEquivalent(new[] { "chicken", "cow" }, refs.Select(r => r.ItemId).ToArray());
                var hen = refs.First(r => r.ItemId == "chicken");
                Assert.IsTrue(s.Hooks.WorldObjectExists(hen));
                Assert.IsTrue(s.Hooks.ConsumeWorldObject(hen));
                Assert.IsFalse(s.Hooks.WorldObjectExists(hen));
                Assert.IsFalse(s.Hooks.ConsumeWorldObject(hen));
                Assert.AreEqual(1, s.State.Animals.Count);
            }
        }

        [Test]
        public void TheCoopAndBarn_AreUnlockedByTheCarpenter()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.State.Gold = 50000;
                s.Backpack.Add(ItemIds.Wood, 900);
                Assert.IsTrue(s.BuyUpgrade(s.UpgradeTable.Get("coop")));
                Assert.IsTrue(s.HasFlag(AnimalRules.BuildingFlag(MapIds.Coop)));
                Assert.IsFalse(s.HasFlag(AnimalRules.BuildingFlag(MapIds.Barn)));
                Assert.IsTrue(s.BuyUpgrade(s.UpgradeTable.Get("barn")));
                Assert.IsTrue(s.HasFlag(AnimalRules.BuildingFlag(MapIds.Barn)));
            }
        }
    }
}
