using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Early data validation for the generated item/crop content (the full data validator lands in T-040).
    public class ContentTests
    {
        GameDatabase _db;

        [SetUp]
        public void SetUp()
        {
            _db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            Assert.IsNotNull(_db, "Resources/GameDatabase is missing; run Farm/Setup/Generate Content");
        }

        [Test]
        public void ItemIds_AreUniqueAndWellFormed()
        {
            var ids = _db.Items.Select(i => i.Id).ToList();
            CollectionAssert.AllItemsAreUnique(ids);
            foreach (var id in ids) StringAssert.IsMatch(@"^[a-z]+\.[a-z]+$", id);
        }

        [Test]
        public void EveryItem_HasAnIcon()
        {
            var missing = _db.Items.Where(i => i.Icon == null).Select(i => i.Id).ToList();
            CollectionAssert.IsEmpty(missing);
        }

        [Test]
        public void Tools_HaveToolTypes_AndStackToOne()
        {
            foreach (var item in _db.Items.Where(i => i.Category == ItemCategory.Tool))
            {
                Assert.AreNotEqual(ToolType.None, item.ToolType, item.Id);
                Assert.AreEqual(1, item.MaxStack, item.Id);
            }
        }

        [Test]
        public void Crops_AreConsistent()
        {
            Assert.GreaterOrEqual(_db.Crops.Count, 6);
            foreach (var crop in _db.Crops)
            {
                Assert.IsTrue(_db.TryGetItem(crop.SeedItemId, out var seed), $"{crop.Id}: seed item");
                Assert.AreEqual(ItemCategory.Seed, seed.Category);
                Assert.AreEqual(crop.Id, seed.CropId);
                Assert.Greater(seed.BuyPrice, 0, $"{crop.Id}: seed must be buyable");

                Assert.IsTrue(_db.TryGetItem(crop.HarvestItemId, out var harvest), $"{crop.Id}: harvest item");
                Assert.AreEqual(ItemCategory.Crop, harvest.Category);
                Assert.Greater(harvest.SellPrice, 0);

                Assert.IsNotEmpty(crop.GrowthDays);
                Assert.IsTrue(crop.GrowthDays.All(d => d >= 1), $"{crop.Id}: growth days");
                Assert.AreEqual(crop.GrowthDays.Length + 1, crop.StageSprites.Length, $"{crop.Id}: stage sprite count");
                Assert.IsTrue(crop.StageSprites.All(s => s != null), $"{crop.Id}: stage sprites");
                if (crop.RegrowDays > 0) Assert.LessOrEqual(crop.RegrowDays, crop.GrowthDays.Sum(), crop.Id);
                Assert.AreNotEqual(SeasonMask.None, crop.Seasons);
            }
        }

        [Test]
        public void EveryCrop_HarvestIsWorthAtLeastHalfItsSeedCost()
        {
            // Sanity bound on the economy: regrowing crops (greenbean) may sell below seed cost per harvest
            // because they pay back over several harvests; nothing may be worth less than half its seed.
            foreach (var crop in _db.Crops)
            {
                var seed = _db.GetItem(crop.SeedItemId);
                var harvest = _db.GetItem(crop.HarvestItemId);
                Assert.Greater(harvest.SellPrice, seed.BuyPrice * 0.5f, crop.Id);
            }
        }

        [Test]
        public void NewGameStartingItems_AllExist()
        {
            var state = GameState.NewGame("T", "F", _db.MaxStack);
            var inv = Inventory.FromData(state.Backpack, _db.MaxStack);
            for (var i = 0; i < inv.Capacity; i++)
            {
                var s = inv.Get(i);
                if (s != null) Assert.IsTrue(_db.TryGetItem(s.ItemId, out _), s.ItemId);
            }
            Assert.IsTrue(inv.Has(ItemIds.Hoe));
            Assert.IsTrue(inv.Has(ItemIds.WateringCan));
            Assert.IsTrue(inv.Has(ItemIds.Seed("parsnip"), 15));
        }

        [Test]
        public void ParsnipMatchesDesignNumbers()
        {
            Assert.IsTrue(_db.TryGetCrop("parsnip", out var parsnip));
            Assert.AreEqual(4, parsnip.GrowthDays.Sum());
            Assert.AreEqual(35, _db.GetItem("crop.parsnip").SellPrice);
            Assert.AreEqual(20, _db.GetItem("seed.parsnip").BuyPrice);
        }
    }
}
