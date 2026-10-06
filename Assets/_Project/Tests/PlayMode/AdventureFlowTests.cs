using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Milestone 3 in the real game, with simulated keys: casting a line, looking after animals, the Community Hall board.
    public class AdventureFlowTests : InputTestFixture
    {
        string _root;
        Keyboard _kb;
        GameSession _s;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-adventure-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            _kb = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator Start(string map, int hour = 10)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, hour * 60));
            _s.State.SetDate(_s.Clock.Now);
            _s.State.GetMap(map).LastSpawnDay = _s.Clock.Now.TotalDays;
            _s.State.CurrentMap = map;
            _s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        IEnumerator Tap(Key k) { Press(_kb[k]); yield return null; Release(_kb[k]); yield return null; }
        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static UiService Ui => ServiceLocator.Get<UiService>();

        void Select(string itemId) =>
            _s.State.SelectedHotbar = Enumerable.Range(0, _s.Backpack.Capacity).First(i => _s.Backpack.Get(i)?.ItemId == itemId);

        [UnityTest]
        public IEnumerator CastingAtTheSea_OpensTheMiniGame_CostsEnergy_AndEscapeReelsIn()
        {
            yield return Start(MapIds.Beach);
            _s.Backpack.Add(FishDefaults.Rod, 1);
            Select(FishDefaults.Rod);
            Player.transform.position = new Vector3(17.5f, 5.5f, 0f);          // on the sand, facing the water at y=4
            Player.Face(Vector2Int.down);
            for (var i = 0; i < 4; i++) yield return null;
            var energy = _s.State.Energy;
            yield return Tap(Key.C);
            Assert.IsTrue(Ui.AnyModalOpen, "the fishing screen opens");
            Assert.AreEqual(energy - PlayerActions.FishingEnergy, _s.State.Energy);
            yield return Tap(Key.Escape);
            Assert.IsFalse(Ui.AnyModalOpen, "Escape pulls the line in");
        }

        [UnityTest]
        public IEnumerator CastingOnLand_DoesNothing()
        {
            yield return Start(MapIds.Beach);
            _s.Backpack.Add(FishDefaults.Rod, 1);
            Select(FishDefaults.Rod);
            Player.transform.position = new Vector3(17.5f, 12.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            var energy = _s.State.Energy;
            yield return Tap(Key.C);
            Assert.IsFalse(Ui.AnyModalOpen);
            Assert.AreEqual(energy, _s.State.Energy);
        }

        [UnityTest]
        public IEnumerator InTheCoop_TheAnimalsSleepAtNight_AndShowTheViewTheyAreWalking()
        {
            yield return Start(MapIds.Coop);
            _s.SetFlag(AnimalRules.BuildingFlag(MapIds.Coop));
            _s.Backpack.Add("animal.chicken", 1);
            Select("animal.chicken");
            Player.transform.position = new Vector3(5.5f, 3.5f, 0f);
            Player.Face(Vector2Int.right);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.C);
            var actor = UnityEngine.Object.FindAnyObjectByType<AnimalActor>();
            Assert.IsNotNull(actor);

            // By day it wanders, and what it shows matches the way it last stepped.
            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 12 * 60));
            var views = new System.Collections.Generic.HashSet<string>();
            var end = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < end && views.Count < 2)
            {
                yield return null;
                if (actor.ShownPicture != null) views.Add(actor.ShownPicture.Substring(0, actor.ShownPicture.Length - 1));
            }
            Assert.GreaterOrEqual(views.Count, 2, "it shows more than one view as it wanders: " + string.Join(",", views));

            // At night it lies down and stays put.
            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 23 * 60));
            var settle = Time.realtimeSinceStartup + 4f;            // a step in progress is finished first
            var last = actor.transform.position;
            var still = 0;
            while (Time.realtimeSinceStartup < settle && still < 20)
            {
                yield return null;
                still = actor.transform.position == last ? still + 1 : 0;
                last = actor.transform.position;
            }
            Assert.IsTrue(actor.IsAsleep);
            Assert.AreEqual("sleep", actor.ShownPicture);
            var at = actor.transform.position;
            for (var i = 0; i < 60; i++) yield return null;
            Assert.AreEqual(at, actor.transform.position, "asleep, it does not wander");

            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 4, 9 * 60));
            for (var i = 0; i < 5; i++) yield return null;
            Assert.IsFalse(actor.IsAsleep);
            Assert.AreNotEqual("sleep", actor.ShownPicture);
        }

        [UnityTest]
        public IEnumerator InTheCoop_AnAnimalIsPlaced_Fed_AndGivesAnEggTheNextMorning()
        {
            yield return Start(MapIds.Coop);
            _s.SetFlag(AnimalRules.BuildingFlag(MapIds.Coop));
            _s.Backpack.Add("animal.chicken", 1);
            _s.Backpack.Add(AnimalRules.Feed, 3);
            Select("animal.chicken");
            Player.transform.position = new Vector3(5.5f, 3.5f, 0f);
            Player.Face(Vector2Int.right);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.C);
            Assert.AreEqual(1, _s.State.Animals.Count, "the chicken moved in");
            var actor = UnityEngine.Object.FindAnyObjectByType<AnimalActor>();
            StringAssert.StartsWith("animal_chicken_", actor.GetComponent<SpriteRenderer>().sprite.name, "it is drawn with the chicken's own art, not a square");
            Assert.AreEqual(0, _s.Backpack.Count("animal.chicken"));

            // The trough is at (10, 6): stand below it.
            Player.transform.position = new Vector3(10.5f, 5.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.E);
            Assert.IsTrue(_s.State.Animals[0].FedToday, "the trough fed it");
            Assert.IsTrue(actor.IsEating, "it munches after being fed");
            for (var i = 0; i < 3; i++) yield return null;
            StringAssert.StartsWith("eat", actor.ShownPicture, "and shows the munching picture");
            Assert.AreEqual(2, _s.Backpack.Count(AnimalRules.Feed));

            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 22 * 60));
            _s.EndDay(false);
            Assert.IsTrue(_s.State.Animals[0].ProductReady, "an egg is waiting");
            Assert.IsTrue(AnimalRules.Collect(_s.State.Animals[0], _s.Backpack, out var item));
            Assert.AreEqual("product.egg", item);
        }

        [UnityTest]
        public IEnumerator TheHallBoard_ListsSixRooms_AndDonatingARoomPays()
        {
            yield return Start(MapIds.CommunityHall);
            _s.Story = StoryContent.LoadFromResources();
            _s.Backpack.Remove(Farm.Data.ItemIds.Hammer, 1);                       // the starting backpack is nearly full: make room for four kinds of crop
            _s.Backpack.Remove(Farm.Data.ItemIds.Machine("chest"), 1);
            foreach (var id in new[] { "crop.parsnip", "crop.potato", "crop.kale", "crop.cauliflower" }) _s.Backpack.Add(id, 3);
            Player.transform.position = new Vector3(7.5f, 6.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the board opens");
            var donate = UnityEngine.Object.FindObjectsByType<Button>()
                .First(b => b.gameObject.activeInHierarchy && b.name == "Donate" && b.transform.parent.name == "hall_pantry");
            Assert.IsTrue(donate.interactable);
            var gold = _s.State.Gold;
            donate.onClick.Invoke();
            Assert.AreEqual(gold + 500, _s.State.Gold);
            Assert.IsTrue(HallRooms.IsRestored(_s.State, "hall_pantry"));
        }
    }
}
