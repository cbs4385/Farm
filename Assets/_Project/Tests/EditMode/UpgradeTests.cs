using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-032 part B: paying for better tools, a bigger backpack and a larger energy reserve.
    public class UpgradeTests
    {
        GameState _state;
        Inventory _pack;
        UpgradeCatalog _catalog;
        readonly GameDateTime _today = new GameDateTime(1, Season.Spring, 5, 600);

        [SetUp]
        public void SetUp()
        {
            L.SetLanguage("en");
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 1);
            _pack = Inventory.FromData(_state.Backpack, _ => 999);
            _catalog = UpgradeCatalog.BuiltIn;
            _state.Gold = 100000;
        }

        UpgradeDefinition Def(string id) => _catalog.Get(id);

        void GiveBars(string id, int n) => _pack.Add(id, n);

        // ---- the table --------------------------------------------------------------------------------------------

        [Test]
        public void TheTable_CoversEveryToolEveryTier_AndTheBackpackAndEnergy()
        {
            foreach (var tool in new[] { ItemIds.Hoe, ItemIds.WateringCan, ItemIds.Axe, ItemIds.Pickaxe, ItemIds.Scythe })
                for (var tier = 1; tier <= ToolModel.MaxTier; tier++)
                    Assert.IsTrue(_catalog.All.Any(u => u.Kind == UpgradeKind.Tool && u.ToolItemId == tool && u.Tier == tier && u.ShopId == "blacksmith"), $"{tool} tier {tier}");
            Assert.AreEqual(2, _catalog.All.Count(u => u.Kind == UpgradeKind.Backpack));
            Assert.AreEqual(3, _catalog.All.Count(u => u.Kind == UpgradeKind.Energy));
            CollectionAssert.AllItemsAreUnique(_catalog.All.Select(u => u.Id).ToList());
        }

        [Test]
        public void BetterTiersCostMore_AndBackpackReachesThirtySix()
        {
            Assert.Less(Def("tool.axe.copper").GoldCost, Def("tool.axe.iron").GoldCost);
            Assert.Less(Def("tool.axe.iron").GoldCost, Def("tool.axe.gold").GoldCost);
            Assert.AreEqual(24, Def("backpack.24").Value);
            Assert.AreEqual(36, Def("backpack.36").Value);
        }

        [Test]
        public void TheGeneratedAssets_ReferenceItemsThatExist()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var upgrades = db.AllUpgrades.ToList();
            Assert.AreEqual(UpgradeDefaults.CreateAll().Length, upgrades.Count, "regenerate content: Farm/Setup/Generate Content");
            foreach (var u in upgrades)
            {
                if (!string.IsNullOrEmpty(u.ToolItemId)) Assert.IsTrue(db.TryGetItem(u.ToolItemId, out _), $"{u.Id}: tool {u.ToolItemId}");
                if (!string.IsNullOrEmpty(u.MaterialItemId)) Assert.IsTrue(db.TryGetItem(u.MaterialItemId, out _), $"{u.Id}: material {u.MaterialItemId}");
                Assert.IsTrue(u.GoldCost > 0, u.Id);
            }
            foreach (var n in db.AllNodes)
                if (!string.IsNullOrEmpty(n.DropItemId)) Assert.IsTrue(db.TryGetItem(n.DropItemId, out _), $"{n.Id}: drop {n.DropItemId}");
        }

        // ---- what is offered ---------------------------------------------------------------------------------------

        [Test]
        public void TheBlacksmith_OffersTheNextTierOfEachToolThePlayerHolds()
        {
            var offers = Upgrades.Offered(_catalog, "blacksmith", _state, _pack);
            Assert.AreEqual(5, offers.Count, "copper for each of the five starting tools");
            Assert.IsTrue(offers.All(o => o.Tier == 1));

            _state.ToolTiers[ItemIds.Axe] = 1;
            offers = Upgrades.Offered(_catalog, "blacksmith", _state, _pack);
            Assert.IsTrue(offers.Any(o => o.Id == "tool.axe.iron"));
            Assert.IsFalse(offers.Any(o => o.Id == "tool.axe.copper"), "already copper");

            _state.ToolTiers[ItemIds.Axe] = 3;
            Assert.IsFalse(Upgrades.Offered(_catalog, "blacksmith", _state, _pack).Any(o => o.ToolItemId == ItemIds.Axe), "nothing beyond gold");
        }

        [Test]
        public void AToolThePlayerDoesNotHold_IsNotOffered()
        {
            _pack.Remove(ItemIds.Scythe, 1);
            Assert.IsFalse(Upgrades.Offered(_catalog, "blacksmith", _state, _pack).Any(o => o.ToolItemId == ItemIds.Scythe));
        }

        [Test]
        public void BackpackAndEnergySteps_AreOfferedInOrder()
        {
            Assert.AreEqual("backpack.24", Upgrades.Offered(_catalog, "general", _state, _pack).Single().Id);
            Assert.AreEqual("energy.1", Upgrades.Offered(_catalog, "clinic", _state, _pack).Single().Id);
            _state.UpgradesDone.Add("backpack.24");
            Assert.AreEqual("backpack.36", Upgrades.Offered(_catalog, "general", _state, _pack).Single().Id);
            _state.UpgradesDone.Add("backpack.36");
            Assert.IsEmpty(Upgrades.Offered(_catalog, "general", _state, _pack));
        }

        [Test]
        public void EachCounterOffersOnlyItsOwnWares()
        {
            Assert.IsTrue(Upgrades.Offered(_catalog, "general", _state, _pack).All(o => o.Kind == UpgradeKind.Backpack));
            Assert.IsTrue(Upgrades.Offered(_catalog, "clinic", _state, _pack).All(o => o.Kind == UpgradeKind.Energy));
            Assert.IsEmpty(Upgrades.Offered(_catalog, "saloon", _state, _pack));
        }

        // ---- buying ------------------------------------------------------------------------------------------------

        [Test]
        public void ATool_IsHandedIn_PaidFor_AndComesBackInTwoDays()
        {
            GiveBars(ItemIds.CopperBar, 5);
            var def = Def("tool.axe.copper");
            Assert.AreEqual(UpgradeCheck.Ok, Upgrades.Buy(_catalog, def, _state, _pack, _today));

            Assert.AreEqual(100000 - def.GoldCost, _state.Gold);
            Assert.AreEqual(0, _pack.Count(ItemIds.CopperBar), "materials are used up");
            Assert.IsFalse(_pack.Has(ItemIds.Axe), "the axe is with the blacksmith");
            var pending = _state.PendingUpgrades.Single();
            Assert.AreEqual(_today.TotalDays + 2, pending.ReadyDay);

            Assert.IsEmpty(Upgrades.Collect(_state, _pack, _today), "not ready yet");
            Assert.IsEmpty(Upgrades.Collect(_state, _pack, new GameDateTime(1, Season.Spring, 6)), "one day is not enough");
            Assert.IsFalse(Upgrades.Offered(_catalog, "blacksmith", _state, _pack).Any(o => o.ToolItemId == ItemIds.Axe), "no second order for it");

            var ready = new GameDateTime(1, Season.Spring, 7);
            var collected = Upgrades.Collect(_state, _pack, ready);
            Assert.AreEqual(1, collected.Count);
            Assert.IsTrue(_pack.Has(ItemIds.Axe));
            Assert.AreEqual(1, _state.ToolTiers[ItemIds.Axe]);
            Assert.IsEmpty(_state.PendingUpgrades);
        }

        [Test]
        public void NotEnoughGold_OrMaterials_ChangesNothing()
        {
            GiveBars(ItemIds.CopperBar, 4);
            Assert.AreEqual(UpgradeCheck.NoMaterials, Upgrades.Buy(_catalog, Def("tool.axe.copper"), _state, _pack, _today));
            GiveBars(ItemIds.CopperBar, 1);
            _state.Gold = 100;
            Assert.AreEqual(UpgradeCheck.NoGold, Upgrades.Buy(_catalog, Def("tool.axe.copper"), _state, _pack, _today));
            Assert.AreEqual(100, _state.Gold);
            Assert.AreEqual(5, _pack.Count(ItemIds.CopperBar));
            Assert.IsTrue(_pack.Has(ItemIds.Axe));
            Assert.IsEmpty(_state.PendingUpgrades);
        }

        [Test]
        public void AnUpgradeThatIsNotOfferedYet_CannotBeBought()
        {
            GiveBars(ItemIds.IronBar, 5);
            Assert.AreEqual(UpgradeCheck.NotOffered, Upgrades.Buy(_catalog, Def("tool.axe.iron"), _state, _pack, _today), "copper comes first");
            Assert.AreEqual(UpgradeCheck.NotOffered, Upgrades.Buy(_catalog, Def("backpack.36"), _state, _pack, _today));
        }

        [Test]
        public void ThePackGrowsToTwentyFourThenThirtySix_KeepingItsContents()
        {
            var before = _pack.Count(ItemIds.Seed("parsnip"));
            Assert.AreEqual(12, _pack.Capacity);
            Assert.AreEqual(UpgradeCheck.Ok, Upgrades.Buy(_catalog, Def("backpack.24"), _state, _pack, _today));
            Assert.AreEqual(24, _pack.Capacity);
            Assert.AreEqual(UpgradeCheck.Ok, Upgrades.Buy(_catalog, Def("backpack.36"), _state, _pack, _today));
            Assert.AreEqual(36, _pack.Capacity);
            Assert.AreEqual(before, _pack.Count(ItemIds.Seed("parsnip")));
            CollectionAssert.IsSupersetOf(_state.UpgradesDone, new[] { "backpack.24", "backpack.36" });
            Assert.AreEqual(100000 - 12000, _state.Gold);
        }

        [Test]
        public void EnergyUpgrades_RaiseTheReserve_AndAreBoughtOnce()
        {
            var max = _state.MaxEnergy;
            Assert.AreEqual(UpgradeCheck.Ok, Upgrades.Buy(_catalog, Def("energy.1"), _state, _pack, _today));
            Assert.AreEqual(max + 20, _state.MaxEnergy);
            Assert.AreEqual(UpgradeCheck.NotOffered, Upgrades.Buy(_catalog, Def("energy.1"), _state, _pack, _today), "not twice");
            Upgrades.Buy(_catalog, Def("energy.2"), _state, _pack, _today);
            Upgrades.Buy(_catalog, Def("energy.3"), _state, _pack, _today);
            Assert.AreEqual(max + 60, _state.MaxEnergy);
        }

        [Test]
        public void AFinishedTool_WaitsWhenThereIsNoRoom_ThenComes()
        {
            GiveBars(ItemIds.CopperBar, 5);
            Upgrades.Buy(_catalog, Def("tool.pickaxe.copper"), _state, _pack, _today);
            // Fill every free slot with something else.
            var fillers = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            foreach (var f in fillers) _pack.Add("filler." + f, 1);
            Assert.IsFalse(_pack.CanAdd(ItemIds.Pickaxe, 1));

            var ready = new GameDateTime(1, Season.Spring, 8);
            Assert.IsEmpty(Upgrades.Collect(_state, _pack, ready));
            Assert.AreEqual(1, _state.PendingUpgrades.Count, "still waiting at the counter");

            _pack.Remove("filler.a", 1);
            Assert.AreEqual(1, Upgrades.Collect(_state, _pack, ready).Count);
        }

        [Test]
        public void ToolsTakenInForUpgrade_CanBeUpgradedAgainAfterwards()
        {
            GiveBars(ItemIds.CopperBar, 5);
            Upgrades.Buy(_catalog, Def("tool.hoe.copper"), _state, _pack, _today);
            Upgrades.Collect(_state, _pack, new GameDateTime(1, Season.Spring, 7));
            GiveBars(ItemIds.IronBar, 5);
            Assert.AreEqual(UpgradeCheck.Ok, Upgrades.Buy(_catalog, Def("tool.hoe.iron"), _state, _pack, new GameDateTime(1, Season.Spring, 7)));
        }

        [Test]
        public void TheUpgradeState_IsSaved()
        {
            GiveBars(ItemIds.CopperBar, 5);
            Upgrades.Buy(_catalog, Def("tool.axe.copper"), _state, _pack, _today);
            Upgrades.Buy(_catalog, Def("backpack.24"), _state, _pack, _today);
            _state.ToolTiers[ItemIds.Hoe] = 2;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_state);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
            Assert.AreEqual(1, back.PendingUpgrades.Count);
            Assert.AreEqual(ItemIds.Axe, back.PendingUpgrades[0].ToolItemId);
            Assert.AreEqual(_today.TotalDays + 2, back.PendingUpgrades[0].ReadyDay);
            Assert.IsTrue(back.UpgradesDone.Contains("backpack.24"));
            Assert.AreEqual(2, back.ToolTiers[ItemIds.Hoe]);
        }

        // ---- the day summary ---------------------------------------------------------------------------------------

        [Test]
        public void TheMorningAToolIsReady_TheSummaryTellsThePlayer()
        {
            var axe = ItemDefinition.Create(ItemIds.Axe, ItemCategory.Tool, toolType: ToolType.Axe);
            try
            {
                _state.PendingUpgrades.Add(new PendingUpgrade { ToolItemId = ItemIds.Axe, Tier = 2, ReadyDay = new GameDateTime(1, Season.Spring, 6).TotalDays });
                var clock = new GameClock(new GameDateTime(1, Season.Spring, 5, 1000));
                _state.SetDate(clock.Now);
                var grids = new Dictionary<string, FarmGrid> { { MapIds.Farm, new FarmGrid() } };
                var summary = DayCycle.EndDay(_state, clock, grids, id => id == ItemIds.Axe ? axe : null, id => null, false);
                Assert.IsTrue(summary.Notes.Any(n => n.Key == "summary.upgrade_ready" && (string)n.Args[0] == "Iron Axe"));

                // The night before it is ready, there is no note.
                _state.PendingUpgrades[0].ReadyDay = clock.Now.TotalDays + 5;
                var quiet = DayCycle.EndDay(_state, clock, grids, id => id == ItemIds.Axe ? axe : null, id => null, false);
                Assert.IsFalse(quiet.Notes.Any(n => n.Key == "summary.upgrade_ready"));
            }
            finally { UnityEngine.Object.DestroyImmediate(axe); }
        }
    }
}
