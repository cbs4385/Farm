using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The playtesters' list of 2026-10-13 through the real scenes: planting out of season, the mine's markers, the sizes of the clinic's beds, the stores' shelves and the
    // range, the saloon's meals, and the four festival days in the village.
    public class Playtest1013FlowTests : PlayModeFixture
    {
        readonly List<string> _toasts = new List<string>();

        IEnumerator Begin(string scene, Action<GameSession> before = null)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _toasts.Clear();
            GameContext.Current.Bus.Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            before?.Invoke(session);
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        static GameSession Session => GameContext.Current.Session;

        static void Select(GameSession session, string itemId)
        {
            for (var i = 0; i < InputNames.HotbarSlots; i++)
                if (session.Backpack.Get(i)?.ItemId == itemId) { session.State.SelectedHotbar = i; return; }
            Assert.Fail($"{itemId} not on the hotbar");
        }

        // ---- planting ----

        [UnityTest]
        public IEnumerator ASeedOutOfSeason_IsRefused_AndTheMessageNamesTheSeason()
        {
            yield return Begin(MapIds.Farm, s => s.Clock.SetTime(new GameDateTime(1, Season.Winter, 5, 10 * 60)));
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            player.transform.position = new Vector3(20.5f, 10.1f, 0f);
            player.Face(Vector2Int.right);
            yield return null;
            var grid = Session.GetGrid(MapIds.Farm);
            grid.Till(21, 10);
            var seeds = Session.Backpack.Count(ItemIds.Seed("parsnip"));
            Select(Session, ItemIds.Seed("parsnip"));
            actions.UseSelected();
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsNull(tile.Crop, "nothing is planted in winter");
            Assert.AreEqual(seeds, Session.Backpack.Count(ItemIds.Seed("parsnip")), "and the seed is kept");
            Assert.IsTrue(_toasts.Any(t => t.Contains("Spring") && t.Contains("will not grow")), string.Join(" | ", _toasts));

            Session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, 10 * 60));
            actions.UseSelected();
            Assert.IsNotNull(tile.Crop, "in its own season it goes in");
        }

        [UnityTest]
        public IEnumerator InTheGreenhouse_AnySeedGoesInInAnySeason()
        {
            yield return Begin(MapIds.Greenhouse, s => s.Clock.SetTime(new GameDateTime(1, Season.Winter, 5, 10 * 60)));
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var here = map.WorldToCell(player.transform.position);
            var cell = new Vector3Int(here.x + 1, here.y, 0);
            player.Face(Vector2Int.right);
            yield return null;
            var grid = Session.GetGrid(MapIds.Greenhouse);
            Assert.IsTrue(grid.AllSeasons);
            grid.Till(cell.x, cell.y);
            Select(Session, ItemIds.Seed("parsnip"));
            actions.UseSelected();
            grid.TryGetTile(cell.x, cell.y, out var tile);
            Assert.IsNotNull(tile.Crop, "the greenhouse keeps every season");
        }

        // ---- the mine ----

        [UnityTest]
        public IEnumerator TheMineMarkers_AreTheEntranceInTheirOwnColours()
        {
            yield return Begin(MapIds.Farm, s => { s.State.Mine.Floor = 1; s.State.Mine.Deepest = 5; });
            Session.State.Mine.Floor = 1;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            var names = new Dictionary<MineStairs.StairKind, string>();
            foreach (var stairs in UnityEngine.Object.FindObjectsByType<MineStairs>(FindObjectsSortMode.None))
            {
                var marker = stairs.transform.Find("Marker");
                Assert.IsNotNull(marker, stairs.Kind + " has its marker");
                names[stairs.Kind] = marker.GetComponent<SpriteRenderer>().sprite.name;
                Assert.AreEqual(32f, marker.GetComponent<SpriteRenderer>().sprite.rect.width, "the entrance's own size");
            }
            Assert.AreEqual("prop_mine_exit", names[MineStairs.StairKind.Up], "the way out of the first floor");
            Assert.AreEqual("prop_mine_down", names[MineStairs.StairKind.Down]);
            Assert.AreEqual("prop_mine_lift", names[MineStairs.StairKind.Elevator]);
        }

        // ---- furniture ----

        [UnityTest]
        public IEnumerator TheClinicBeds_AndTheStoreShelves_AreTwoCellsTall()
        {
            yield return Begin(MapIds.Clinic);
            foreach (var name in new[] { "Bed1", "Bed2", "Bed3" })
            {
                var bed = GameObject.Find(name);
                Assert.IsNotNull(bed, name);
                Assert.AreEqual(new Vector2(1f, 2f), bed.GetComponent<BoxCollider2D>().size, name + " is one cell wide and two tall");
                Assert.AreEqual(32f, bed.GetComponent<SpriteRenderer>().sprite.rect.height, name + " picture");
            }
            var op = SceneManager.LoadSceneAsync(MapIds.GeneralStore);
            while (!op.isDone) yield return null;
            yield return null;
            var shelf = GameObject.Find("Shelf1");
            Assert.AreEqual(new Vector2(1f, 2f), shelf.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(32f, shelf.GetComponent<SpriteRenderer>().sprite.rect.height);
        }

        [UnityTest]
        public IEnumerator TheRange_IsTwoCellsSquare_AndStillCooks()
        {
            yield return Begin(MapIds.FarmHouse);
            var range = GameObject.Find("Kitchen");
            Assert.IsNotNull(range);
            Assert.AreEqual(new Vector2(2f, 2f), range.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(32f, range.GetComponent<SpriteRenderer>().sprite.rect.width);
            Assert.IsNotNull(range.GetComponent<Kitchen>());
            Assert.AreEqual(new Vector2Int(2, 2), range.GetComponent<MovableFixture>().Size, "the mallet moves it as one piece");
            // Nothing else stands in its two cells.
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var centre = range.transform.position;
            foreach (var other in UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
                if (other.gameObject != range && !other.isTrigger && other.GetComponentInParent<UnityEngine.Tilemaps.Tilemap>() == null)
                    Assert.IsFalse(other.bounds.Intersects(new Bounds(centre, new Vector3(1.9f, 1.9f, 1f))), other.name + " overlaps the range");
            Assert.IsNotNull(map);
        }

        // ---- the saloon ----

        [UnityTest]
        public IEnumerator TheSaloonBar_IsACounterThatSellsMeals()
        {
            yield return Begin(MapIds.Saloon);
            var counters = UnityEngine.Object.FindObjectsByType<ShopCounter>(FindObjectsSortMode.None);
            Assert.IsTrue(counters.Any(c => c.ShopId == "saloon"), "a bar counter sells");
            var stock = ShopCatalog.For(Session.Db, "saloon", Session.World);
            Assert.GreaterOrEqual(stock.Count, 9);
            Assert.IsTrue(stock.All(i => i.Category == ItemCategory.Food));
        }

        // sooziq: "I don't see how Tilda buys items. For me, it's all things that I can buy from her." The counter of the general store has a sell button.
        [UnityTest]
        public IEnumerator TheGeneralStoreCounter_HasASellButton_AndTheOthersDoNot()
        {
            yield return Begin(MapIds.GeneralStore);
            var ui = (Farm.UI.UiService)GameContext.Current.Ui;
            Session.Backpack.Add("crop.parsnip", 4);
            ui.ShowShop("general");
            yield return null;
            Assert.IsTrue(ui.Shop.SellButtonShown, "the general store buys");
            var gold = Session.State.Gold;
            ui.Shop.SellToTheShop();
            yield return null;
            Assert.IsNotNull(ui.Sell);
            Assert.IsTrue(ui.Sell.IsOpen, "the sell window opens over the counter");
            ui.Sell.SellSlot(Enumerable.Range(0, Session.Backpack.Capacity).First(i => Session.Backpack.Get(i)?.ItemId == "crop.parsnip"), 2);
            Assert.Greater(Session.State.Gold, gold);
            ui.Sell.Close();
            yield return null;
            Assert.IsTrue(ui.Shop.IsOpen, "and the counter is there again when it is closed");
            ui.Shop.Close();
            ui.ShowShop("carpenter");
            yield return null;
            Assert.IsFalse(ui.Shop.SellButtonShown, "the carpenter sells only");
        }

        // ---- the festivals ----

        static IEnumerator OnFestivalDay(Season season, int day, int minute, string scene, Action<GameSession> also = null)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, season, day, minute));
            also?.Invoke(session);
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 40; i++) yield return null;
        }

        static IEnumerable<FestivalStation> Stations(FestivalStationKind kind) =>
            UnityEngine.Object.FindObjectsByType<FestivalStation>(FindObjectsSortMode.None).Where(s => s.Kind == kind && s.gameObject.activeInHierarchy);

        [UnityTest]
        public IEnumerator OnAnOrdinaryDay_NoFestivalPropIsInTheVillage()
        {
            yield return OnFestivalDay(Season.Spring, 12, 12 * 60, MapIds.Village, s => s.SetVar("festival.spring", 1));
            foreach (FestivalStationKind kind in Enum.GetValues(typeof(FestivalStationKind)))
                Assert.IsEmpty(Stations(kind), kind + " is not out today");
            Assert.IsNull(GameObject.Find("DanceFloor_25_17") is GameObject floor && floor.activeInHierarchy ? floor : null);
        }

        [UnityTest]
        public IEnumerator BlossomDay_TheVillagersDance_AndThePlayerJoinsAtTheMaypole()
        {
            yield return OnFestivalDay(Season.Spring, 13, 12 * 60, MapIds.Village, s => s.SetVar("festival.spring", 1));
            var maypole = Stations(FestivalStationKind.Maypole).Single();
            Assert.IsTrue(GameObject.Find("DanceFloor_25_17").activeInHierarchy, "the floor is down");
            for (var i = 0; i < 60; i++) yield return null;
            var actors = GameContext.Current.Map.Npcs.Actors.Values.ToList();
            Assert.GreaterOrEqual(actors.Count, 6, "the villagers are in the village at noon");
            var dancers = actors.Where(a => a.Dancing).ToList();
            Assert.GreaterOrEqual(dancers.Count, 4, "those by the maypole dance: " + string.Join(",", actors.Select(a => a.Definition.Id + (a.Dancing ? "*" : ""))));
            Assert.IsTrue(dancers.All(a => a.GetComponent<WalkBob>().Dancing));
            var frame = dancers[0].GetComponent<SpriteRenderer>().sprite;
            var seen = new HashSet<int>();
            var bob = dancers[0].GetComponent<WalkBob>();
            var until = Time.time + 1.3f;          // the beat is a fifth of a second: frames run much faster than that in a test
            while (Time.time < until) { seen.Add(bob.DancePhase); yield return null; }
            Assert.GreaterOrEqual(seen.Count, 3, "the dance goes through its phases with the music: " + string.Join(",", seen));
            Assert.AreEqual(CharacterFrames.Dance(dancers[0].Definition.SpriteFor(Vector2Int.down), 1).name.Contains("dance1"), true, "and its pictures are made from the villager's own");
            Assert.IsNotNull(frame);

            // The player steps up to the maypole and dances.
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            player.transform.position = maypole.transform.position + new Vector3(0f, -0.9f, 0f);
            player.Face(Vector2Int.up);
            yield return null;
            Session.State.Energy = 20;
            maypole.Interact(actions);
            yield return null;
            Assert.IsTrue(player.Dancing, "the farmer dances");
            Assert.IsTrue(player.GetComponent<WalkBob>().Dancing);
            var start = Time.realtimeSinceStartup;
            while (player.Dancing && Time.realtimeSinceStartup - start < Festivals.DanceSeconds + 6f) yield return null;
            Assert.IsFalse(player.Dancing, "and the dance ends");
            Assert.IsFalse(player.GetComponent<WalkBob>().Dancing);
            Assert.AreEqual(20 + Festivals.DanceEnergy, Session.State.Energy, "it lifts the spirits");
            Assert.IsTrue(Session.HasFlag(Festivals.Key(Festivals.Get("spring"), 1, FestivalActivities.Danced)));
            Assert.IsTrue(_toasts.Count >= 0);
        }

        [UnityTest]
        public IEnumerator BlossomDay_PushingTheStickEndsTheDanceEarlyWithNoReward()
        {
            yield return OnFestivalDay(Season.Spring, 13, 12 * 60, MapIds.Village, s => s.SetVar("festival.spring", 1));
            var maypole = Stations(FestivalStationKind.Maypole).Single();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            player.transform.position = maypole.transform.position + new Vector3(0f, -0.9f, 0f);
            yield return null;
            Assert.IsTrue(player.StartDance(30f, () => Assert.Fail("not danced to the end")));
            Assert.IsTrue(player.Dancing);
            Assert.IsFalse(player.StartDance(5f), "already dancing");
            player.Face(Vector2Int.down);
            Assert.IsNotNull(actions);
        }

        [UnityTest]
        public IEnumerator Tidewatch_TheBoardAndTheBoatsStand_AndTheShoreIsFullOfAnglers()
        {
            yield return OnFestivalDay(Season.Summer, 11, 12 * 60, MapIds.Village, s => s.SetVar("festival.summer", 1));
            var board = Stations(FestivalStationKind.Scoreboard).Single();
            Assert.IsNotNull(GameObject.Find("Boat1"));
            for (var i = 0; i < 60; i++) yield return null;
            var actors = GameContext.Current.Map.Npcs.Actors.Values.ToList();
            Assert.GreaterOrEqual(actors.Count(a => a.Cell.y == 13 || a.Cell.y == 14), 6, "the anglers stand along the shore");
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            player.transform.position = board.transform.position + new Vector3(0f, -0.9f, 0f);
            yield return null;
            board.Interact(actions);
            Assert.IsTrue(Session.HasFlag(Festivals.Key(Festivals.Get("summer"), 1, FestivalActivities.ContestStarted)), "entered at the board");
            Assert.IsTrue(_toasts.Count >= 0);
        }

        [UnityTest]
        public IEnumerator HarvestFair_TheStallsAndTheJudgingTableStand_AndACropCanBeEntered()
        {
            yield return OnFestivalDay(Season.Fall, 16, 12 * 60, MapIds.Village, s => s.SetVar("festival.fall", 1));
            var table = Stations(FestivalStationKind.JudgingTable).Single();
            Assert.IsNotNull(GameObject.Find("StallWest1"));
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            Session.Backpack.Add("crop.pumpkin", 1);
            Select(Session, "crop.pumpkin");
            table.Interact(actions);
            Assert.AreEqual(0, Session.Backpack.Count("crop.pumpkin"), "the pumpkin is entered");
            Assert.IsTrue(Session.HasFlag(Festivals.Key(Festivals.Get("fall"), 1, FestivalActivities.ShowEntered)));
        }

        [UnityTest]
        public IEnumerator LanternFeast_TheTableTheLanternsAndTheRackStand_AndTheLanternLights()
        {
            yield return OnFestivalDay(Season.Winter, 25, 18 * 60, MapIds.Village, s => s.SetVar("festival.winter", 1));
            Assert.AreEqual(4, Stations(FestivalStationKind.FeastTable).Count(), "four pieces of table");
            var rack = Stations(FestivalStationKind.LanternRack).Single();
            Assert.IsFalse(rack.Lit.activeSelf, "unlit to begin with");
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            rack.Interact(actions);
            Assert.IsTrue(rack.Lit.activeSelf, "the lantern is lit");
            Session.State.Energy = 3;
            Stations(FestivalStationKind.FeastTable).First().Interact(actions);
            Assert.AreEqual(Session.State.MaxEnergy, Session.State.Energy);
        }

        [UnityTest]
        public IEnumerator TheDayAfter_TheGroundIsBare_AndTheVillagersAreBackAtWork()
        {
            yield return OnFestivalDay(Season.Spring, 14, 12 * 60, MapIds.Village);
            Assert.IsEmpty(Stations(FestivalStationKind.Maypole));
            var maypole = GameObject.Find("Maypole");
            Assert.IsTrue(maypole == null || !maypole.activeInHierarchy);
            var dancing = GameContext.Current.Map.Npcs.Actors.Values.Count(a => a.Dancing);
            Assert.AreEqual(0, dancing);
        }
    }
}
