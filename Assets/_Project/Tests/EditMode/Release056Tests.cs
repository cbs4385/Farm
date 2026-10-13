using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest notes of 2026-10-13: the library shelves, jam and pickles to eat, the sword and the weather.
    public class Release056Tests
    {
        TestSessionFixture _f;

        [SetUp]
        public void SetUp() => _f = new TestSessionFixture();

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            L.SetTable(new Dictionary<string, string>());
            _f.Dispose();
        }

        // ---- the shelves ------------------------------------------------------------------------------------------

        [Test]
        public void AShelf_ListsSixDifferentTitles_TheSameWayAllDay_AndOtherTitlesTomorrow()
        {
            var today = Bookshelf.PickIndices("Shelf1", 40);
            Assert.AreEqual(Bookshelf.Listed, today.Length);
            Assert.AreEqual(today.Length, today.Distinct().Count());
            CollectionAssert.AreEqual(today, Bookshelf.PickIndices("Shelf1", 40));
            CollectionAssert.AreNotEqual(today, Bookshelf.PickIndices("Shelf1", 41));
            CollectionAssert.AreNotEqual(today, Bookshelf.PickIndices("Shelf2", 40));
        }

        [Test]
        public void EveryTitle_Exists()
        {
            var keys = Enumerable.Range(0, Bookshelf.Pool).Select(i => "bookshelf.title." + i)
                .Concat(Enumerable.Range(0, MythosBookshelf.Grades).SelectMany(g => Enumerable.Range(0, MythosBookshelf.TitlesPerGrade).Select(i => $"mythos.book.g{g}.{i}")));
            var json = System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json");
            foreach (var key in keys) StringAssert.Contains("\"" + key + "\"", json);
        }

        [Test]
        public void TheDisturbingTitles_GrowWithWakefulness_AndMildIsGentler_AndOffChangesNothing()
        {
            Assert.AreEqual((0, 0), MythosBookshelf.Plan(0, 2, 6), "a sleeping god leaves the shelves alone");
            Assert.AreEqual((0, 0), MythosBookshelf.Plan(20, 0, 6), "off");
            var last = 0;
            for (var step = 1; step <= 20; step++)
            {
                var (swapped, grade) = MythosBookshelf.Plan(step, 2, 6);
                Assert.GreaterOrEqual(swapped, last, "never fewer as it wakes");
                Assert.LessOrEqual(grade, MythosBookshelf.Grades - 1);
                last = swapped;
            }
            Assert.AreEqual(6, MythosBookshelf.Plan(20, 2, 6).swapped);
            Assert.AreEqual(3, MythosBookshelf.Plan(20, 2, 6).grade);
            Assert.LessOrEqual(MythosBookshelf.Plan(20, 1, 6).swapped, 2);
            Assert.LessOrEqual(MythosBookshelf.Plan(20, 1, 6).grade, 1);
        }

        [Test]
        public void TheFilter_SwapsTitles_OnlyWhenTheGodStirs()
        {
            var table = new Dictionary<string, string>();
            for (var g = 0; g < MythosBookshelf.Grades; g++)
                for (var i = 0; i < MythosBookshelf.TitlesPerGrade; i++) table[$"mythos.book.g{g}.{i}"] = $"strange {g}.{i}";
            L.SetTable(table);
            var plain = Enumerable.Range(0, 6).Select(i => "plain " + i).ToList();
            var filter = new MythosBookshelf(_f.Session);

            GameSession.HorrorLevelOverride = 2;
            _f.Session.SetVar(MythosIds.Vars.Wakefulness, 0);
            CollectionAssert.AreEqual(plain, filter.Filter(plain, 7).ToList());

            _f.Session.SetVar(MythosIds.Vars.Wakefulness, 1000);
            var late = filter.Filter(plain, 7).ToList();
            Assert.AreEqual(6, late.Count(t => t.StartsWith("strange")), "deep in its sleep's end every title is strange");
            CollectionAssert.AreEqual(late, filter.Filter(plain, 7).ToList(), "the same all day");

            GameSession.HorrorLevelOverride = 0;
            CollectionAssert.AreEqual(plain, filter.Filter(plain, 7).ToList(), "at off the shelves are ordinary");
        }

        [Test]
        public void ABrokenShelfFilter_DoesNotBreakTheShelf()
        {
            _f.Session.Hooks.AddBookshelfFilter(new ThrowingFilter());
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("Bookshelf filter ThrowingFilter failed"));
            var titles = new List<string> { "a", "b" };
            CollectionAssert.AreEqual(titles, _f.Session.Hooks.FilterBookshelf(titles, 1).ToList());
        }

        sealed class ThrowingFilter : IBookshelfFilter
        {
            public IList<string> Filter(IList<string> titles, int seed) => throw new System.InvalidOperationException("test");
        }

        // ---- food, the sword, the seasons --------------------------------------------------------------------------

        [Test]
        public void JamAndPickles_RestoreEnergy_JuiceAndWineDoNot()
        {
            var rows = CraftingDefaults.CreateItems().ToDictionary(r => r.Id);
            Assert.Greater(rows["artisan.jam"].Energy, 0);
            Assert.Greater(rows["artisan.pickles"].Energy, 0);
            Assert.AreEqual(0, rows["artisan.juice"].Energy);
        }

        [Test]
        public void EverySeed_IsSoldAllYear()
        {
            var db = UnityEngine.Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var seeds = db.AllItems.Where(i => i.Category == ItemCategory.Seed && i.BuyPrice > 0).ToList();
            Assert.Greater(seeds.Count, 10);
            foreach (var seed in seeds) Assert.IsTrue(string.IsNullOrEmpty(seed.SaleCondition), seed.Id + " is for sale in every season");
        }

        [Test]
        public void PlantingOutOfSeason_IsAllowed_TheCropIsDeadByMorning_AndNewSeedComesBack()
        {
            var db = UnityEngine.Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            db.TryGetCrop("tomato", out var tomato);              // summer only
            var grid = new FarmGrid();
            grid.Till(1, 1);
            Assert.IsFalse(grid.Plantable(tomato, Season.Winter));
            Assert.IsTrue(grid.Plant(1, 1, tomato, Season.Winter), "it can be planted, with a warning");
            Assert.IsTrue(grid.TryUproot(1, 1, out var id) && id == tomato.Id, "pulled up the same day it is a seed again");

            grid.Plant(1, 1, tomato, Season.Winter);
            grid.Water(1, 1);
            grid.AdvanceDay(Season.Winter, false, x => tomato);
            Assert.IsTrue(grid.IsWithered(1, 1), "dead overnight");
            Assert.IsFalse(grid.IsMature(1, 1, x => tomato));
            Assert.IsTrue(grid.Untill(1, 1) == false, "a dead crop has to be cleared first");
            grid.ClearCrop(1, 1);
            Assert.IsTrue(grid.Untill(1, 1), "bare soil goes back to grass");
            Assert.IsFalse(grid.IsTilled(1, 1));
        }

        [Test]
        public void ANewGame_CanLeaveTheToolsOnTheRack()
        {
            var plain = GameState.NewGame("A", "B", id => 99);
            Assert.IsTrue(Inventory.FromData(plain.Backpack, id => 99).Has(ItemIds.Hoe), "the default start is unchanged");

            var racked = GameState.NewGame("A", "B", id => 99, toolsOnRack: true);
            var pack = Inventory.FromData(racked.Backpack, id => 99);
            foreach (var tool in ToolRack.Tools) Assert.IsFalse(pack.Has(tool), tool + " hangs on the rack");
            Assert.IsTrue(pack.Has(ItemIds.Seed("parsnip")));
            Assert.IsTrue(pack.Has(ItemIds.Torch));
            Assert.IsTrue(racked.Flags.Contains(ToolRack.StockedFlag));

            _f.Session.State.Flags.Add(ToolRack.StockedFlag);
            Assert.AreEqual(ToolRack.Tools.Length, ToolRack.Remaining(_f.Session).Count);
        }

        [Test]
        public void ARainyDay_SendsTheVillageHomeAfterWork_NotToTheSquare()
        {
            var jobs = NpcRoster.CreateAll().Where(n => n.Schedule.Any(e => e.Id == "workday")).ToList();
            Assert.Greater(jobs.Count, 4);
            foreach (var npc in jobs.Where(n => n.Schedule.First(e => e.Id == "workday").Stops.Last().Minute < 1260 && n.Business != "saloon"))
            {
                var wet = npc.Schedule.FirstOrDefault(e => e.Id == "wet_workday");
                Assert.IsNotNull(wet, npc.Id + " has a plan for wet days");
                Assert.AreEqual(npc.HomeMap, wet.Stops.Last().Map, npc.Id + " ends the wet day at home");
            }
        }

        [Test]
        public void ASprinkler_WatersEveryTileNextToIt_TheDiagonalsToo_AndTheSoilIsWetAtDawn()
        {
            var crop = CropDefinition.Create("x", new[] { 1, 1 }, SeasonMask.All);
            var state = GameState.NewGame("a", "b", id => 999, 1);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 1000));
            state.SetDate(clock.Now);
            var grid = new FarmGrid();
            var around = new[] { (4, 4), (5, 4), (6, 4), (4, 5), (6, 5), (4, 6), (5, 6), (6, 6) };
            foreach (var (x, y) in around) { grid.Till(x, y); grid.Plant(x, y, crop, Season.Spring); }
            state.GetMap(MapIds.Farm).Objects.Add(new PlacedObject { Id = "s", TypeId = "sprinkler1", X = 5, Y = 5 });

            DayCycle.EndDay(state, clock, new Dictionary<string, FarmGrid> { { MapIds.Farm, grid } }, id => null, id => id == "x" ? crop : null, false);

            foreach (var (x, y) in around)
            {
                grid.TryGetTile(x, y, out var tile);
                Assert.AreEqual(1, tile.Crop.Stage, $"({x},{y}) grew without being watered by hand");
                Assert.IsTrue(tile.Watered, $"({x},{y}) is wet in the morning");
            }
            UnityEngine.Object.DestroyImmediate(crop);
        }

        [Test]
        public void TheSwordSwing_IsNeverRefusedForWantOfEnergy()
        {
            Assert.AreEqual(1, CombatModel.SwingEnergy);
            var src = System.IO.File.ReadAllText("Assets/_Project/Scripts/Gameplay/Combat/PlayerCombat.cs");
            StringAssert.DoesNotContain("!_session.TrySpendEnergy(CombatModel.SwingEnergy)", src, "the swing's energy is spent when there is any, and the swing happens anyway");
        }
    }
}
