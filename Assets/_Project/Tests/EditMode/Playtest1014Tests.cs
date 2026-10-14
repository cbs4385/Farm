using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest chat 2026-10-14: the pad and the wheel in the menus, ripe crops that twinkle, crafting from what is near, counters that give everything a business offers,
    // quality marks wherever an item's picture is.
    public class Playtest1014Tests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
        static RecipeCatalog Recipes() => new RecipeCatalog(CraftingDefaults.CreateRecipes());

        // ---- crafting from what is near ----------------------------------------------------------------------------------

        [Test]
        public void Crafting_UsesTheBackpackFirst_ThenChests_ThenFinishedMachineGoods()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                f.EmptyBackpack();
                var map = s.State.CurrentMap;
                var grid = s.GetObjects(map);
                var chestDef = s.Placeables.Get(CraftingDefaults.Chest);
                var near = grid.Place(chestDef, 10, 10, "near");
                var far = grid.Place(chestDef, 30, 30, "far");
                grid.ChestOf(near).Add(ItemIds.Wood, 20);
                grid.ChestOf(far).Add(ItemIds.Wood, 100);
                s.Backpack.Add(ItemIds.Wood, 10);
                s.PlayerCellOverride = () => (12, 10);

                var chest = Recipes().Get("chest");                                          // costs 25 wood
                Assert.AreEqual(CraftResult.Ok, s.Craft(chest, Stations.Hand));
                Assert.AreEqual(0, s.Backpack.Count(ItemIds.Wood), "the backpack's wood went first");
                Assert.AreEqual(5, grid.ChestOf(near).Count(ItemIds.Wood), "then the chest in reach");
                Assert.AreEqual(100, grid.ChestOf(far).Count(ItemIds.Wood), "the far chest is out of reach");
            }
        }

        [Test]
        public void Crafting_DoesNotReachBeyondFiveTiles_AndWithoutAMapOnlyTheBackpackCounts()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                f.EmptyBackpack();
                var grid = s.GetObjects(s.State.CurrentMap);
                var obj = grid.Place(s.Placeables.Get(CraftingDefaults.Chest), 10, 10, "c");
                grid.ChestOf(obj).Add(ItemIds.Wood, 60);
                var chest = Recipes().Get("chest");

                s.PlayerCellOverride = () => (16, 10);                                       // six tiles away
                Assert.AreEqual(CraftResult.MissingIngredients, s.Craft(chest, Stations.Hand));
                s.PlayerCellOverride = () => (15, 10);                                       // five away
                Assert.AreEqual(CraftResult.Ok, s.Craft(chest, Stations.Hand));
                s.PlayerCellOverride = () => (13, 14);                                       // 3 and 4: five in a straight line, within the circle
                Assert.AreEqual(CraftResult.Ok, s.Craft(chest, Stations.Hand));
                s.PlayerCellOverride = null;                                                 // no map loaded
                Assert.AreEqual(CraftResult.MissingIngredients, s.Craft(chest, Stations.Hand));
            }
        }

        [Test]
        public void Crafting_TakesFinishedMachineGoods_AndLeavesTheMachineIdle()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                f.EmptyBackpack();
                var grid = s.GetObjects(s.State.CurrentMap);
                var kegDef = s.Placeables.Get(CraftingDefaults.Furnace);
                var furnace = grid.Place(kegDef, 5, 5, "f");
                furnace.RecipeId = "x"; furnace.OutputItemId = ItemIds.Wood; furnace.OutputCount = 30; furnace.ReadyAt = 0;
                s.PlayerCellOverride = () => (5, 6);
                Assert.AreEqual(CraftResult.Ok, s.Craft(Recipes().Get("chest"), Stations.Hand));
                Assert.AreEqual(5, furnace.OutputCount);
                var unfinished = grid.Place(kegDef, 6, 5, "g");
                unfinished.RecipeId = "x"; unfinished.OutputItemId = ItemIds.Wood; unfinished.OutputCount = 99; unfinished.ReadyAt = int.MaxValue;
                furnace.OutputCount = 0; furnace.RecipeId = null; furnace.OutputItemId = null;
                Assert.AreEqual(CraftResult.MissingIngredients, s.Craft(Recipes().Get("chest"), Stations.Hand), "goods still cooking are not loose");
            }
        }

        [Test]
        public void CraftingPlan_FillsEachIngredientFromTheEarlierSourceFirst()
        {
            var pack = new Inventory(8, id => 999);
            var chest = new Inventory(8, id => 999);
            pack.Add(ItemIds.Wood, 5);
            chest.Add(ItemIds.Wood, 50);
            var plan = CraftingRules.Plan(Recipes().Get("chest"), new IIngredientSource[] { new InventorySource(pack), new InventorySource(chest) });
            CollectionAssert.AreEqual(new[] { (0, ItemIds.Wood, 5), (1, ItemIds.Wood, 20) }, plan);
        }

        // ---- the pad scrolls a page ----------------------------------------------------------------------------------------

        [Test]
        public void PadScroll_StickUpMovesTheViewUp_AndStopsAtTheEnds()
        {
            Assert.Greater(PadScroll.Step(0.5f, 1f, 400f, 0.1f), 0.5f);
            Assert.Less(PadScroll.Step(0.5f, -1f, 400f, 0.1f), 0.5f);
            Assert.AreEqual(1f, PadScroll.Step(0.99f, 1f, 400f, 1f), 0.0001f);
            Assert.AreEqual(0f, PadScroll.Step(0.01f, -1f, 400f, 1f), 0.0001f);
            Assert.AreEqual(0.4f, PadScroll.Step(0.4f, 1f, 0f, 1f), 0.0001f, "nothing to scroll");
        }

        [Test]
        public void TextPagesOfTheMenu_HaveAScrollingBody()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var canvas = new GameObject("c", typeof(RectTransform));
            try
            {
                var scroll = UiKit.Scroll(canvas.transform, "Body", out var content);
                Assert.IsNotNull(scroll.GetComponent<ScrollRect>());
                Assert.IsNotNull(scroll.GetComponent<ScrollFollowSelection>());
                Assert.AreSame(content, scroll.GetComponent<ScrollRect>().content);
            }
            finally { Object.DestroyImmediate(canvas); }
        }

        // ---- ripe crops twinkle ------------------------------------------------------------------------------------------

        [Test]
        public void Twinkle_RisesAndFalls_AndEachPlantIsAtItsOwnMoment()
        {
            Assert.AreEqual(0f, RipeSparkle.Twinkle(0f, 0f), 0.0001f);
            Assert.AreEqual(1f, RipeSparkle.Twinkle(0.8f, 0f), 0.0001f);
            for (var t = 0f; t < 3f; t += 0.05f) Assert.That(RipeSparkle.Twinkle(t, 0.3f), Is.InRange(0f, 1f));
            Assert.AreNotEqual(RipeSparkle.Twinkle(0.3f, 0f), RipeSparkle.Twinkle(0.3f, 0.2f));
        }

        [Test]
        public void MatureCrops_AreCollectedForSparkles_ButGrowingAndWitheredOnesAreNot()
        {
            var crop = CropDefinition.Create("test_crop", new[] { 1, 1 }, SeasonMask.Spring);
            var grid = new FarmGrid();
            for (var x = 1; x <= 3; x++) { grid.Till(x, 1); grid.Plant(x, 1, crop, Season.Spring); }
            grid.TryGetTile(1, 1, out var ripe); ripe.Crop.Stage = crop.MatureStage;
            grid.TryGetTile(3, 1, out var dead); dead.Crop.Stage = crop.MatureStage; dead.Crop.Withered = true;
            CropDefinition Lookup(string id) => id == crop.Id ? crop : null;
            Assert.IsTrue(grid.IsMature(1, 1, Lookup));
            Assert.IsFalse(grid.IsMature(2, 1, Lookup), "still growing");
            Assert.IsFalse(grid.IsMature(3, 1, Lookup), "withered");
        }

        // ---- counters ----------------------------------------------------------------------------------------------------

        [Test]
        public void EveryBusinessWithAShopOrUpgrades_OpensWhatItHas_FromAnyCounter()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                foreach (var business in new[] { "general", "blacksmith", "carpenter", "saloon", "clinic" })
                {
                    var shop = CounterServices.HasShop(s, business);
                    var upgrades = CounterServices.HasUpgrades(s, business);
                    Assert.IsTrue(shop || upgrades || business == "saloon", business + " offers something");
                    Assert.AreEqual(shop || !upgrades, CounterServices.OpensShop(s, business), business);
                }
                Assert.IsTrue(CounterServices.HasUpgrades(s, "general"), "the general store sells the bigger backpack");
                Assert.IsTrue(CounterServices.HasShop(s, "general"));
            }
        }

        // ---- quality marks -----------------------------------------------------------------------------------------------

        [Test]
        public void QualityMarks_TintSilverGoldIridium_AndNothingForOrdinary()
        {
            Assert.AreEqual(0f, QualityMarks.TintOf(0).a);
            Assert.Greater(QualityMarks.TintOf(1).a, 0f);
            Assert.AreNotEqual(QualityMarks.TintOf(1), QualityMarks.TintOf(2));
            Assert.AreNotEqual(QualityMarks.TintOf(2), QualityMarks.TintOf(3));
            Assert.AreEqual(0f, QualityMarks.TintOf(9).a);
            Assert.AreEqual(QualityMarks.TintOf(2), SlotBadges.TintOf(2), "the slots and the world use the same colours");
        }

        [Test]
        public void QualityMark_ShowsOnlyForBetterThanOrdinary()
        {
            var go = new GameObject("holder");
            try
            {
                var mark = QualityMarks.Attach(go.transform, Vector3.zero, 1f);
                Assert.IsFalse(mark.enabled);
                QualityMarks.Set(mark, 2, 5);
                Assert.IsTrue(mark.enabled);
                Assert.AreEqual(6, mark.sortingOrder);
                QualityMarks.Set(mark, 0, 5);
                Assert.IsFalse(mark.enabled);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void AnimalProductQuality_FollowsHappiness_SoTheBubbleCanShowIt()
        {
            Assert.AreEqual(0, AnimalRules.Quality(new AnimalState { Happiness = 30 }));
            Assert.AreEqual(1, AnimalRules.Quality(new AnimalState { Happiness = 70 }));
            Assert.AreEqual(2, AnimalRules.Quality(new AnimalState { Happiness = 95 }));
        }
    }
}
