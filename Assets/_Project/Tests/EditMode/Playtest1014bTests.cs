using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-14 (second list): books go back to the library desk, milk and eggs make food and cheese, animals show when their product is due.
    public class Playtest1014bTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void ABorrowedBook_IsTakenBackAtTheDesk_ForALittleFriendship_OnceEach()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                var slim = LibraryDesk.Books[0];
                Assert.IsFalse(LibraryDesk.TryReturn(s), "nothing to hand back yet");
                s.SetFlag(slim.Flag);
                s.Backpack.Add(slim.ItemId, 1);
                var before = NpcInteractions.StateOf(s.State, NpcIds.Ione).Points;
                Assert.IsTrue(LibraryDesk.TryReturn(s));
                Assert.AreEqual(0, s.Backpack.Count(slim.ItemId), "the book stays at the library");
                Assert.AreEqual(before + LibraryDesk.ReturnPoints, NpcInteractions.StateOf(s.State, NpcIds.Ione).Points);
                s.Backpack.Add(slim.ItemId, 1);
                Assert.IsFalse(LibraryDesk.TryReturn(s), "a book already returned earns nothing twice");
            }
        }

        [Test]
        public void MilkAndEggs_MakeFood_AndMilkMakesCheeseInAJar()
        {
            var db = RealDb();
            var recipes = new RecipeCatalog(CraftingDefaults.CreateRecipes());
            foreach (var id in new[] { "food.omelette", "food.custard", "food.cheesy_potato", "food.goat_cheese_salad", "artisan.cheese", "artisan.goat_cheese" })
                Assert.IsTrue(db.TryGetItem(id, out _), id + " is in the item database (run Generate Content)");
            Assert.AreEqual("artisan.cheese", recipes.Get("jar_cheese").OutputItemId);
            Assert.AreEqual(Stations.Jar, recipes.Get("jar_goat_cheese").Station);
            foreach (var id in new[] { "cook_omelette", "cook_custard" })
            {
                var cook = recipes.Get(id);
                Assert.AreEqual(Stations.Kitchen, cook.Station);
                Assert.AreEqual(0, cook.SkillLevel, "known from the start");
                Assert.IsTrue(cook.Ingredients.Any(i => i.ItemId == "product.egg"));
            }
            Assert.IsTrue(recipes.Get("cook_custard").Ingredients.Any(i => i.ItemId == "product.milk"));
        }

        [Test]
        public void AnAnimal_SaysHowManyNightsRemain_UntilItsProductIsReady()
        {
            var state = new GameState();
            var goat = AnimalRules.Add(state, "goat", "g", "Goat");           // every two days
            Assert.AreEqual(2, AnimalRules.NightsToProduct(goat));
            goat.FedToday = true;
            AnimalRules.NewDay(state);
            Assert.AreEqual(1, AnimalRules.NightsToProduct(goat));
            goat.FedToday = true;
            AnimalRules.NewDay(state);
            Assert.AreEqual(0, AnimalRules.NightsToProduct(goat));
            Assert.IsTrue(goat.ProductReady);
        }

        [Test]
        public void TheFarmhouse_HasAMainRoom_AKitchen_AndABedroom_JoinedByDoors_EachFurnished()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{MapIds.FarmHouse}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var map = roots.SelectMany(r => r.GetComponentsInChildren<FarmMap>(true)).Single();
            Assert.AreEqual(2, map.DoorTiles().Count(), "a door to the kitchen and a door to the bedroom");
            Vector3Int Cell(Component c) => map.Ground.WorldToCell(c.transform.position);
            var bed = Cell(roots.SelectMany(r => r.GetComponentsInChildren<Bed>(true)).Single());
            var range = Cell(roots.SelectMany(r => r.GetComponentsInChildren<Kitchen>(true)).Single());
            Assert.That(bed.x, Is.InRange(12, 20)); Assert.That(bed.y, Is.InRange(5, 7), "the bed is in the bedroom");
            Assert.That(range.x, Is.InRange(12, 20)); Assert.That(range.y, Is.InRange(1, 3), "the range is in the kitchen");
            var names = roots.SelectMany(r => r.GetComponentsInChildren<MovableFixture>(true)).Select(f => f.name).ToList();
            foreach (var n in new[] { "Wardrobe", "BedLamp", "KitchenTable", "Sacks", "Couch", "DiningTable" }) CollectionAssert.Contains(names, n);
        }

        [Test]
        public void Furniture_IsSoldAtTheGeneralStore_AndByTheTravelingMerchant()
        {
            var db = RealDb();
            foreach (var (id, _) in CraftingDefaults.Decor)
            {
                Assert.IsTrue(db.TryGetItem(ItemIds.Machine(id), out var item));
                CollectionAssert.IsSupersetOf(item.SoldIn, new[] { "general", "merchant", "carpenter" }, id);
            }
        }

        [Test]
        public void TitleAndTextColours_AreChosenSeparately_ExceptDarkTextAndWarnings_AndReturnToDefault()
        {
            var go = new GameObject("ui-root");
            try
            {
                var plain = Farm.UI.UiKit.Label(go.transform, "plain");
                var title = Farm.UI.UiKit.Label(go.transform, "title", color: Farm.UI.UiKit.Accent);
                var warning = Farm.UI.UiKit.Label(go.transform, "warning", color: Farm.UI.UiKit.Danger);
                var ink = Farm.UI.UiKit.Label(go.transform, "ink", color: Color.black);
                var settings = new SettingsData { BodyColor = "00FF00", TitleColor = "0000FF" };
                Farm.UI.UiTextStyle.Apply(settings, go.transform);
                Assert.AreEqual(1f, plain.color.g, 0.01f, "ordinary text takes the text colour");
                Assert.AreEqual(1f, title.color.b, 0.01f); Assert.AreEqual(0f, title.color.g, 0.01f);
                Assert.AreEqual(Farm.UI.UiKit.Danger.g, warning.color.g, 0.001f, "a warning keeps its colour");
                Assert.AreEqual(0f, ink.color.g, 0.001f, "black text stays black");
                settings.BodyColor = "FF0000";                               // from one chosen colour to another
                Farm.UI.UiTextStyle.Apply(settings, go.transform);
                Assert.AreEqual(1f, plain.color.r, 0.01f, "the new choice shows at once, not the one before");
                plain.color = Color.green;                                   // the game recolours a label
                Farm.UI.UiTextStyle.Refresh(go.transform);
                Assert.AreEqual(1f, plain.color.r, 0.01f);
                settings.TitleColor = "";                                    // only the title back to default
                Farm.UI.UiTextStyle.Apply(settings, go.transform);
                Assert.AreEqual(Farm.UI.UiKit.Accent.g, title.color.g, 0.001f);
                Assert.AreEqual(1f, plain.color.r, 0.01f);
                settings.BodyColor = "";
                Farm.UI.UiTextStyle.Apply(settings, go.transform);
                Assert.AreEqual(Color.green.g, plain.color.g, 0.001f, "back to what the game set");
            }
            finally
            {
                Farm.UI.UiTextStyle.Apply(new SettingsData());
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AColourSurvivesTheRoundTripThroughItsHex()
        {
            Assert.AreEqual("FFB347", Farm.UI.UiTextStyle.ToHex(Farm.UI.UiTextStyle.Parse("FFB347").Value));
            Assert.IsNull(Farm.UI.UiTextStyle.Parse(""));
            Assert.IsNull(Farm.UI.UiTextStyle.Parse("nonsense"));
        }

        [Test]
        public void TheFontChoices_StartWithTheGameFont_AndAnUnknownFontLeavesTheGameFontInPlace()
        {
            Assert.AreEqual(string.Empty, Farm.UI.UiTextStyle.FontChoices()[0]);
            Farm.UI.UiTextStyle.Apply(new SettingsData { FontName = "No Such Font 12345" });
            Assert.IsNull(Farm.UI.UiKit.Font);
            Farm.UI.UiTextStyle.Apply(new SettingsData());
        }
    }
}
