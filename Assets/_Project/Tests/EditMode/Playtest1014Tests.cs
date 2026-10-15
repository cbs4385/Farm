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

        // ---- the second list: tips, help, fishing, monsters --------------------------------------------------------------

        [Test]
        public void Tips_ComeOnceEach_AndNotWhenTurnedOff()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                Assert.IsNull(ContextTips.Next(s, false), "nothing to say on a quiet farm");
                s.State.Health = 10;
                Assert.AreEqual(ContextTips.LowHealth, ContextTips.Tick(s, false));
                Assert.IsNull(ContextTips.Tick(s, false), "once");
                s.State.Health = s.State.MaxHealth;

                s.State.CurrentMap = MapIds.Mine;
                Assert.AreEqual(ContextTips.Mine, ContextTips.Tick(s, false));
                Assert.AreEqual(ContextTips.Bats, ContextTips.Tick(s, true));
                Assert.IsNull(ContextTips.Tick(s, true));
            }
        }

        [Test]
        public void TheFullPackTip_NeedsNoEmptySlot()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var items = Enumerable.Range(0, 40).Select(i => ItemDefinition.Create("test.item" + i, ItemCategory.Resource, 999)).ToList();
            using (var f = new TestSessionFixture(RealDb().AllItems.Concat(items)))
            {
                var s = f.Session;
                f.EmptyBackpack();
                Assert.IsTrue(s.Backpack.HasEmptySlot());
                for (var i = 0; i < s.Backpack.Capacity; i++) s.Backpack.Add("test.item" + i, 1);
                Assert.IsFalse(s.Backpack.HasEmptySlot());
                Assert.AreEqual(ContextTips.PackFull, ContextTips.Tick(s, false));
                Assert.IsNull(ContextTips.Tick(s, false));
                s.Backpack.Remove("test.item0", 1);
                Assert.IsTrue(s.Backpack.HasEmptySlot());
            }
        }

        [Test]
        public void EveryTip_HasItsWords()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            foreach (var tip in new[] { ContextTips.LowHealth, ContextTips.PackFull, ContextTips.Mine, ContextTips.Bats })
                Assert.IsTrue(L.Has("tip." + tip), tip);
        }

        [Test]
        public void AMonster_ShowsWhenToHitIt_PaleBlueAfterAStrike_YellowWhenAboutToStrike()
        {
            var row = EnemyDefaults.Row("cave_bat");
            var brain = new EnemyBrain(row, 1);
            Assert.AreEqual(EnemyCue.None, brain.Cue, "far away and idle");
            var step = brain.Tick(0.1f, Vector2.zero, new Vector2(0.5f, 0f));
            Assert.IsTrue(step.Strike, "next to the player it strikes");
            Assert.AreEqual(EnemyCue.Recovering, brain.Cue, "it has just struck: the time to hit it");
            for (var i = 0; i < 8; i++) brain.Tick(0.1f, Vector2.zero, new Vector2(0.5f, 0f));
            Assert.AreEqual(EnemyCue.Warning, brain.Cue, "close and ready to strike again");
            Assert.AreNotEqual(EnemyActor.TintOf(EnemyCue.None), EnemyActor.TintOf(EnemyCue.Recovering));
            Assert.AreNotEqual(EnemyActor.TintOf(EnemyCue.Recovering), EnemyActor.TintOf(EnemyCue.Warning));
        }

        [Test]
        public void TheHelp_SearchFindsTitlesAndWords_InAnyCase()
        {
            Assert.IsTrue(HelpPage.Matches("", "Eating", "x"));
            Assert.IsTrue(HelpPage.Matches("health", "The health bar", "x"));
            Assert.IsTrue(HelpPage.Matches("HEALTH", "Eating", "gives back a little health"));
            Assert.IsFalse(HelpPage.Matches("fish", "Eating", "gives back energy"));
        }

        [Test]
        public void TheHelp_HasAnAnswerForHealthEnergyCookingCapacityMiningFishingAndMonsters()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            foreach (var key in new[] { "help.energy", "help.health", "help.capacity", "help.cooking", "help.mining", "help.fishing", "help.combat" })
            {
                CollectionAssert.Contains(HelpPage.Topics, key);
                Assert.IsTrue(L.Has(key) && L.Has(key + ".title"), key);
            }
            StringAssert.Contains("heal", L.Get("help.health").ToLowerInvariant());
        }

        [Test]
        public void LongNotices_StayLongerOnScreen()
        {
            Assert.AreEqual(2.2f, HudView.ToastSeconds("Short."), 0.001f);
            Assert.Greater(HudView.ToastSeconds(new string('x', 200)), 6f);
            Assert.LessOrEqual(HudView.ToastSeconds(new string('x', 5000)), 9f);
        }

        [Test]
        public void TheCamera_MayShowBlackBelowTheMap_SoTheItemBarNeverCoversTheFarmer()
        {
            Assert.AreEqual(0f, CameraFollow.BottomMargin(0f));
            var halfHeight = 8.4375f;                                      // a 1080 line window at 8 pixels to the tile... any size: the margin is a share of the view
            var margin = CameraFollow.BottomMargin(halfHeight);
            Assert.AreEqual(halfHeight * 2f * CameraFollow.BottomMarginShare, margin, 0.0001f);
            Assert.Greater(CameraFollow.BottomMarginShare, 0.11f, "more than the item bar's own height (about a tenth of the screen)");
            Assert.Less(CameraFollow.BottomMarginShare, 0.25f, "and not a quarter of the screen thrown away");
        }

        [Test]
        public void TheToolRackPicture_IsFinalArt_SoRegeneratingPlaceholdersLeavesItAlone()
        {
            var listed = File.ReadAllText("Assets/_Project/Art/Placeholders/final_art.txt").Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);          // on any white space
            CollectionAssert.Contains(listed, "obj_toolrack");
            CollectionAssert.Contains(listed, "item_machine_toolrack");
        }
    }
}
