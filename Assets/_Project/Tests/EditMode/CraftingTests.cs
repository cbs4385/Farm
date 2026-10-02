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
    // T-037: recipes, crafting, machines that work across days, placed objects (chests, sprinklers, scarecrows), and the
    // world-object sources that expose crafted items and produce.
    public class CraftingTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        static Inventory Pack(params (string id, int n)[] items)
        {
            var inv = new Inventory(24, id => 999);
            foreach (var (id, n) in items) inv.Add(id, n);
            return inv;
        }

        static RecipeCatalog Catalog() => new RecipeCatalog(CraftingDefaults.CreateRecipes());

        // ---- data -------------------------------------------------------------------------------------------------------

        [Test]
        public void EveryRecipe_UsesRealItems_AndHasAValidStation()
        {
            var db = RealDb();
            var ids = new HashSet<string>(db.AllItems.Select(i => i.Id));
            var seen = new HashSet<string>();
            foreach (var r in db.AllRecipes)
            {
                Assert.IsTrue(seen.Add(r.Id), "unique id " + r.Id);
                Assert.Contains(r.Station, new[] { Stations.Hand, Stations.Kitchen, Stations.Furnace, Stations.Keg, Stations.Jar }, r.Id);
                Assert.IsTrue(ids.Contains(r.OutputItemId), $"{r.Id}: output {r.OutputItemId}");
                Assert.Greater(r.OutputCount, 0, r.Id);
                Assert.IsNotEmpty(r.Ingredients, r.Id);
                foreach (var i in r.Ingredients)
                {
                    Assert.Greater(i.Count, 0, r.Id);
                    if (i.AnyOf != null && i.AnyOf.Length > 0) foreach (var any in i.AnyOf) Assert.IsTrue(ids.Contains(any), $"{r.Id}: {any}");
                    else Assert.IsTrue(ids.Contains(i.ItemId), $"{r.Id}: {i.ItemId}");
                }
                if (r.IsMachineRecipe) Assert.Greater(r.Minutes, 0, r.Id + " takes time");
                else Assert.AreEqual(0, r.Minutes, r.Id + " is instant");
                if (!string.IsNullOrEmpty(r.SkillId)) Assert.Contains(r.SkillId, SkillIds.All);
            }
            Assert.GreaterOrEqual(db.AllRecipes.Count(r => r.Station == Stations.Kitchen), 8, "cooking has a menu");
        }

        [Test]
        public void EveryPlaceable_HasAnItemASpriteAndAMatchingItemCategory()
        {
            var db = RealDb();
            foreach (var p in db.AllPlaceables)
            {
                Assert.IsNotNull(p.Sprite, p.Id);
                Assert.IsTrue(db.TryGetItem(p.ItemId, out var item), p.Id);
                Assert.AreEqual(ItemCategory.Machine, item.Category, p.Id);
                Assert.AreEqual(p.Id, item.PlaceableId);
                Assert.IsTrue(db.AllRecipes.Any(r => r.OutputItemId == p.ItemId), $"{p.Id} can be crafted");
                if (p.Kind == PlaceableKind.Machine) Assert.IsTrue(db.AllRecipes.Any(r => r.Station == p.Station), p.Id);
            }
        }

        [Test]
        public void ExtraItems_AreAllGenerated_WithIcons()
        {
            var db = RealDb();
            foreach (var row in CraftingDefaults.CreateItems())
            {
                Assert.IsTrue(db.TryGetItem(row.Id, out var item), row.Id);
                Assert.IsNotNull(item.Icon, row.Id);
            }
        }

        [Test]
        public void DishesRestoreEnergy_AndHaveAPrice()
        {
            var db = RealDb();
            foreach (var recipe in db.AllRecipes.Where(r => r.Station == Stations.Kitchen))
            {
                var dish = db.GetItem(recipe.OutputItemId);
                Assert.AreEqual(ItemCategory.Food, dish.Category);
                Assert.Greater(dish.EnergyRestore, 0, dish.Id);
                Assert.Greater(dish.SellPrice, 0, dish.Id);
            }
        }

        // ---- ingredients ------------------------------------------------------------------------------------------------

        [Test]
        public void AnyOfIngredients_CountAcrossItems_AndAreTakenInListOrder()
        {
            var recipe = RecipeDefinition.Create("x", Stations.Hand, "out", 1,
                new[] { new RecipeIngredient { ItemId = "any", AnyOf = new[] { "a", "b" }, Count = 3 } });
            var pack = Pack(("a", 1), ("b", 5));
            Assert.IsTrue(CraftingRules.HasIngredients(recipe, pack));
            CraftingRules.TakeIngredients(recipe, pack);
            Assert.AreEqual(0, pack.Count("a"));
            Assert.AreEqual(3, pack.Count("b"), "one from a, then two from b");
            Assert.IsFalse(CraftingRules.HasIngredients(recipe, Pack(("a", 2))));
        }

        // ---- crafting ---------------------------------------------------------------------------------------------------

        TestSessionFixture Fixture() => new TestSessionFixture(RealDb().AllItems);

        [Test]
        public void Crafting_ConsumesIngredients_AndGivesTheOutput()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.Backpack.Add(ItemIds.Wood, 60);
                var chest = s.Recipes.Get("chest") ?? Catalog().Get("chest");
                Assert.AreEqual(CraftResult.Ok, CraftingRules.TryCraft(chest, Stations.Hand, s.State, s.Backpack, s.GetSkillLevel));
                Assert.AreEqual(10, s.Backpack.Count(ItemIds.Wood));
                Assert.AreEqual(1, s.Backpack.Count("machine.chest"));
            }
        }

        [Test]
        public void Crafting_RefusesWhenMissingStuff_WrongStation_OrNotKnown()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var cat = Catalog();
                Assert.AreEqual(CraftResult.MissingIngredients, CraftingRules.TryCraft(cat.Get("chest"), Stations.Hand, s.State, s.Backpack, s.GetSkillLevel));
                s.Backpack.Add(ItemIds.Wood, 60);
                Assert.AreEqual(CraftResult.WrongStation, CraftingRules.TryCraft(cat.Get("chest"), Stations.Kitchen, s.State, s.Backpack, s.GetSkillLevel));

                s.Backpack.Add(ItemIds.CopperBar, 1);
                s.Backpack.Add(ItemIds.Stone, 10);
                var sprinkler = cat.Get("sprinkler1");                          // needs farming level 2
                Assert.AreEqual(CraftResult.NotKnown, CraftingRules.TryCraft(sprinkler, Stations.Hand, s.State, s.Backpack, s.GetSkillLevel));
                s.AddSkillXp(SkillIds.Farming, 100);
                Assert.AreEqual(CraftResult.Ok, CraftingRules.TryCraft(sprinkler, Stations.Hand, s.State, s.Backpack, s.GetSkillLevel));
            }
        }

        [Test]
        public void Crafting_WithAFullBackpack_LosesNothing()
        {
            var cat = Catalog();
            var state = GameState.NewGame("a", "b", id => 999, 1);
            var pack = new Inventory(2, id => 999);
            pack.Add(ItemIds.Wood, 100);                             // slot 1 (only half of it is used up)
            pack.Add("filler", 1);                                   // slot 2: the pack is full
            Assert.AreEqual(CraftResult.NoRoom, CraftingRules.TryCraft(cat.Get("chest"), Stations.Hand, state, pack, id => 1));
            Assert.AreEqual(100, pack.Count(ItemIds.Wood), "the wood is still there");
        }

        [Test]
        public void SkillLevels_UnlockRecipes_AndTellThePlayer()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var gratin = s.Recipes.Get("cook_gratin");
                Assert.IsFalse(s.KnowsRecipe(gratin));
                s.AddSkillXp(SkillIds.Farming, 460);                // level 4 at 450
                Assert.IsTrue(s.KnowsRecipe(gratin));
                Assert.That(f.Toasts, Has.Some.Contains("You can now make"), "level-ups announce new recipes");
            }
        }

        [Test]
        public void LearningARecipe_FromStory_AlsoUnlocksIt()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var pie = s.Recipes.Get("cook_pumpkin_pie");        // farming 5
                Assert.IsFalse(s.KnowsRecipe(pie));
                Effects.Run(s, "learn:cook_pumpkin_pie");
                Assert.IsTrue(s.KnowsRecipe(pie));
                Assert.IsFalse(s.LearnRecipe("cook_pumpkin_pie"), "already known");
                Assert.IsFalse(s.LearnRecipe("nope"));
            }
        }

        // ---- machines ---------------------------------------------------------------------------------------------------

        static PlacedObject Machine(string type) => new PlacedObject { Id = "m1", TypeId = type, X = 3, Y = 3 };

        [Test]
        public void AMachine_TakesIngredients_WorksForTheRecipeTime_ThenGivesTheOutput()
        {
            var cat = Catalog();
            var def = PlaceableCatalog.BuiltIn.Get("keg");
            var machine = Machine("keg");
            var pack = Pack(("crop.parsnip", 3));

            Assert.AreEqual(LoadResult.Loaded, CraftingRules.TryLoad(machine, def, "crop.parsnip", cat, pack, 1000, out var started));
            Assert.AreEqual("keg_juice", started.Id);
            Assert.AreEqual(2, pack.Count("crop.parsnip"));
            Assert.IsTrue(CraftingRules.IsBusy(machine));
            Assert.AreEqual(LoadResult.Busy, CraftingRules.TryLoad(machine, def, "crop.parsnip", cat, pack, 1000, out _));

            Assert.IsFalse(CraftingRules.IsReady(machine, 1000 + 2 * 1440 - 1));
            Assert.IsFalse(CraftingRules.TryCollect(machine, pack, 1000 + 100));
            Assert.AreEqual(2 * 1440, CraftingRules.MinutesLeft(machine, 1000));

            Assert.IsTrue(CraftingRules.TryCollect(machine, pack, 1000 + 2 * 1440));
            Assert.AreEqual(1, pack.Count("artisan.juice"));
            Assert.IsFalse(CraftingRules.IsBusy(machine), "the machine is free again");
        }

        [Test]
        public void AMachine_RefusesWrongOrMissingIngredients_AndCollectingWithoutRoomKeepsTheOutput()
        {
            var cat = Catalog();
            var furnace = PlaceableCatalog.BuiltIn.Get("furnace");
            var machine = Machine("furnace");
            var pack = Pack((ItemIds.CopperOre, 3), (ItemIds.Coal, 1), ("crop.parsnip", 1));
            Assert.AreEqual(LoadResult.NoRecipe, CraftingRules.TryLoad(machine, furnace, "crop.parsnip", cat, pack, 0, out _), "parsnips do not smelt");
            Assert.AreEqual(LoadResult.MissingIngredients, CraftingRules.TryLoad(machine, furnace, ItemIds.CopperOre, cat, pack, 0, out _), "needs five ore");

            pack.Add(ItemIds.CopperOre, 2);
            Assert.AreEqual(LoadResult.Loaded, CraftingRules.TryLoad(machine, furnace, ItemIds.CopperOre, cat, pack, 0, out _));
            var full = new Inventory(1, id => 1);
            full.Add("filler", 1);
            Assert.IsFalse(CraftingRules.TryCollect(machine, full, 100000), "no room");
            Assert.IsTrue(CraftingRules.IsReady(machine, 100000), "the bar waits in the furnace");
        }

        [Test]
        public void MachineTime_IsAbsolute_SoItKeepsWorkingAcrossTheNight()
        {
            var evening = new GameDateTime(1, Season.Spring, 3, 22 * 60);
            var morning = new GameDateTime(1, Season.Spring, 4, 6 * 60);
            Assert.AreEqual(8 * 60, ObjectGrid.Minute(morning) - ObjectGrid.Minute(evening), "22:00 to 06:00 is eight hours");
            var end = new GameDateTime(1, Season.Spring, 3, GameDateTime.DayEndMinute);
            Assert.AreEqual(ObjectGrid.Minute(morning), ObjectGrid.Minute(end), "30:00 and the next 06:00 are the same moment");
            Assert.Greater(ObjectGrid.Minute(new GameDateTime(2, Season.Spring, 1)), ObjectGrid.Minute(new GameDateTime(1, Season.Winter, 28, 1700)));
        }

        // ---- placed objects ---------------------------------------------------------------------------------------------

        [Test]
        public void ObjectGrid_PlacesOnePerCell_AndRemoves()
        {
            var grid = ObjectGrid.FromList(null);
            var chest = PlaceableCatalog.BuiltIn.Get("chest");
            var placed = grid.Place(chest, 2, 2, "a");
            Assert.IsNotNull(placed);
            Assert.IsNull(grid.Place(chest, 2, 2, "b"), "the cell is taken");
            Assert.AreSame(placed, grid.At(2, 2));
            Assert.IsTrue(grid.Has(2, 2));
            Assert.IsTrue(grid.Remove("a"));
            Assert.IsFalse(grid.Has(2, 2));
            Assert.IsFalse(grid.Remove("a"));
        }

        [Test]
        public void Chests_KeepTheirItems_ThroughSavingAndLoading()
        {
            var grid = ObjectGrid.FromList(null, id => 999);
            var obj = grid.Place(PlaceableCatalog.BuiltIn.Get("chest"), 1, 1, "c1");
            grid.ChestOf(obj).Add("crop.parsnip", 7, 1);
            Assert.IsFalse(grid.ChestIsEmpty(obj));

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grid.ToList());
            var back = ObjectGrid.FromList(Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlacedObject>>(json), id => 999);
            var chest = back.ChestOf(back.ById("c1"));
            Assert.AreEqual(7, chest.Count("crop.parsnip"));
            Assert.AreEqual(1, chest.Get(0).Quality);
            Assert.AreEqual(36, chest.Capacity);
        }

        [Test]
        public void Machines_KeepTheirProgress_ThroughSavingAndLoading()
        {
            var obj = Machine("jar");
            CraftingRules.TryLoad(obj, PlaceableCatalog.BuiltIn.Get("jar"), "crop.parsnip", Catalog(), Pack(("crop.parsnip", 1)), 500, out _);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<PlacedObject>(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
            Assert.AreEqual("jar_pickles", back.RecipeId);
            Assert.AreEqual(500 + 1440, back.ReadyAt);
            Assert.AreEqual("artisan.pickles", back.OutputItemId);
        }

        [Test]
        public void PickUp_NeedsAnEmptyChestOrIdleMachine_AndRoom()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var grid = s.GetObjects(MapIds.Farm);
                var chest = grid.Place(s.Placeables.Get("chest"), 5, 5, "c");
                grid.ChestOf(chest).Add("crop.parsnip", 1);
                Assert.AreEqual(PlacedInteractions.PickUpResult.NotEmpty, PlacedInteractions.PickUp(s, MapIds.Farm, 5, 5));
                grid.ChestOf(chest).Remove("crop.parsnip", 1);
                Assert.AreEqual(PlacedInteractions.PickUpResult.Ok, PlacedInteractions.PickUp(s, MapIds.Farm, 5, 5));
                Assert.AreEqual(1, s.Backpack.Count("machine.chest"));
                Assert.IsFalse(grid.Has(5, 5));
                Assert.AreEqual(PlacedInteractions.PickUpResult.NothingThere, PlacedInteractions.PickUp(s, MapIds.Farm, 5, 5));

                var keg = grid.Place(s.Placeables.Get("keg"), 6, 6, "k");
                keg.RecipeId = "keg_juice";
                Assert.AreEqual(PlacedInteractions.PickUpResult.NotEmpty, PlacedInteractions.PickUp(s, MapIds.Farm, 6, 6), "a working machine stays");
            }
        }

        [Test]
        public void PlacedObjects_AreSavedWithTheGame_AndComeBack()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var chest = s.GetObjects(MapIds.Farm).Place(s.Placeables.Get("chest"), 9, 9, "keepme");
                s.GetObjects(MapIds.Farm).ChestOf(chest).Add("crop.parsnip", 3);
                s.SyncToState();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(s.State);
                var loaded = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);
                var grid = ObjectGrid.FromList(loaded.GetMap(MapIds.Farm).Objects, id => 999);
                Assert.AreEqual(3, grid.ChestOf(grid.ById("keepme")).Count("crop.parsnip"));
            }
        }

        // ---- sprinklers and scarecrows ----------------------------------------------------------------------------------

        [Test]
        public void SprinklerRanges_CoverFourEightAndTwentyFourTiles()
        {
            Assert.AreEqual(4, Sprinklers.Covered(10, 10, 1).Count());
            Assert.AreEqual(8, Sprinklers.Covered(10, 10, 2).Count());
            Assert.AreEqual(24, Sprinklers.Covered(10, 10, 3).Count());
            Assert.IsFalse(Sprinklers.Covered(10, 10, 1).Contains((11, 11)), "no diagonals at range 1");
            Assert.IsTrue(Sprinklers.Covered(10, 10, 2).Contains((11, 11)));
            Assert.IsFalse(Sprinklers.Covered(10, 10, 2).Contains((12, 10)));
            Assert.IsTrue(Sprinklers.Covered(10, 10, 3).Contains((12, 12)));
        }

        [Test]
        public void ASprinkler_WatersTilledTilesAroundItEachNight()
        {
            var crop = CropDefinition.Create("x", new[] { 1, 1 }, SeasonMask.All);
            var state = GameState.NewGame("a", "b", id => 999, 1);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 1000));
            state.SetDate(clock.Now);
            var grid = new FarmGrid();
            grid.Till(5, 6); grid.Till(8, 8);
            grid.Plant(5, 6, crop, Season.Spring);
            grid.Plant(8, 8, crop, Season.Spring);
            state.GetMap(MapIds.Farm).Objects.Add(new PlacedObject { Id = "s", TypeId = "sprinkler1", X = 5, Y = 5 });

            DayCycle.EndDay(state, clock, new Dictionary<string, FarmGrid> { { MapIds.Farm, grid } }, id => null, id => id == "x" ? crop : null, false);

            grid.TryGetTile(5, 6, out var near);
            grid.TryGetTile(8, 8, out var far);
            Assert.AreEqual(1, near.Crop.Stage, "the sprinkled crop grew without being watered by hand");
            Assert.AreEqual(0, far.Crop.Stage, "the one out of range did not");
            UnityEngine.Object.DestroyImmediate(crop);
        }

        [Test]
        public void AScarecrow_ProtectsCropsWithinItsRange()
        {
            var objects = new[] { new PlacedObject { Id = "sc", TypeId = "scarecrow", X = 10, Y = 10 } };
            var cat = PlaceableCatalog.BuiltIn;
            Assert.IsTrue(Sprinklers.Protected(objects, cat, 10, 16));
            Assert.IsTrue(Sprinklers.Protected(objects, cat, 14, 14));
            Assert.IsFalse(Sprinklers.Protected(objects, cat, 10, 19));
            Assert.IsFalse(Sprinklers.Protected(new PlacedObject[0], cat, 10, 10));
        }

        // ---- world objects (ADR 0002) -----------------------------------------------------------------------------------

        [Test]
        public void ChestsMachinesAndTheBin_ExposeCraftedGoodsAndProduce()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var grid = s.GetObjects(MapIds.Farm);
                var chest = grid.Place(s.Placeables.Get("chest"), 4, 4, "ch");
                grid.ChestOf(chest).Add("artisan.jam", 2);
                grid.ChestOf(chest).Add("crop.parsnip", 5);
                grid.ChestOf(chest).Add(ItemIds.Wood, 9);                   // neither crafted nor produce
                var keg = grid.Place(s.Placeables.Get("keg"), 5, 4, "kg");
                keg.RecipeId = "keg_wine"; keg.OutputItemId = "artisan.wine"; keg.OutputCount = 1;
                s.State.ShippingBin.Add(new ItemStack("food.salad", 1));

                var crafted = s.Hooks.EnumerateWorldObjects(WorldObjectKinds.CraftedItem);
                CollectionAssert.AreEquivalent(new[] { "artisan.jam", "artisan.wine", "food.salad" }, crafted.Select(r => r.ItemId).ToArray());
                var plants = s.Hooks.EnumerateWorldObjects(WorldObjectKinds.PlantProduct);
                CollectionAssert.AreEquivalent(new[] { "crop.parsnip" }, plants.Select(r => r.ItemId).ToArray());

                var jam = crafted.First(r => r.ItemId == "artisan.jam");
                Assert.IsTrue(s.Hooks.WorldObjectExists(jam));
                Assert.IsTrue(s.Hooks.ConsumeWorldObject(jam));
                Assert.IsFalse(s.Hooks.WorldObjectExists(jam), "it is gone from the chest");
                Assert.AreEqual(0, grid.ChestOf(chest).Count("artisan.jam"));
                Assert.IsTrue(s.Hooks.ConsumeWorldObject(crafted.First(r => r.ItemId == "artisan.wine")));
                Assert.IsFalse(CraftingRules.IsBusy(keg), "the machine's output was consumed");
                Assert.IsTrue(s.Hooks.ConsumeWorldObject(crafted.First(r => r.ItemId == "food.salad")));
                Assert.IsEmpty(s.State.ShippingBin);
            }
        }

        [Test]
        public void StandingMatureCrops_AreAPlantSource()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var crop = CropDefinition.Create("parsnip", new[] { 1 }, SeasonMask.All);
                f.Session.Db.SetContents(f.Session.Db.AllItems, new[] { crop });
                var grid = s.GetGrid(MapIds.Farm);
                grid.Till(2, 2); grid.Plant(2, 2, crop, Season.Spring);
                Assert.IsEmpty(s.Hooks.EnumerateWorldObjects(WorldObjectKinds.PlantProduct).Where(r => r.Id.StartsWith("Farm/")), "still growing");
                grid.TryGetTile(2, 2, out var tile);
                tile.Crop.Stage = crop.MatureStage;
                var refs = s.Hooks.EnumerateWorldObjects(WorldObjectKinds.PlantProduct).Where(r => r.Id.StartsWith("Farm/")).ToList();
                Assert.AreEqual(1, refs.Count);
                Assert.IsTrue(s.Hooks.ConsumeWorldObject(refs[0]));
                Assert.IsFalse(grid.TryGetTile(2, 2, out var after) && after.Crop != null);
                UnityEngine.Object.DestroyImmediate(crop);
            }
        }

        // ---- greenhouse -------------------------------------------------------------------------------------------------

        [Test]
        public void TheGreenhouseUpgrade_SetsItsFlag_AndNothingElseUnlocksIt()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.State.Gold = 20000;
                s.Backpack.Add(ItemIds.Wood, 150);
                var greenhouse = s.UpgradeTable.Get("greenhouse");
                Assert.IsNotNull(greenhouse);
                Assert.IsFalse(s.HasFlag(MapIds.GreenhouseFlag));
                Assert.IsTrue(s.BuyUpgrade(greenhouse));
                Assert.IsTrue(s.HasFlag(MapIds.GreenhouseFlag));
                Assert.AreEqual(10000, s.State.Gold);
                Assert.AreEqual(0, s.Backpack.Count(ItemIds.Wood));
                CollectionAssert.DoesNotContain(Upgrades.Offered(s.UpgradeTable, "carpenter", s.State, s.Backpack).Select(u => u.Id), "greenhouse", "it is bought once");
            }
        }
    }
}
