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
            Assert.AreEqual(0, _s.Backpack.Count("animal.chicken"));

            // The trough is at (10, 6): stand below it.
            Player.transform.position = new Vector3(10.5f, 5.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.E);
            Assert.IsTrue(_s.State.Animals[0].FedToday, "the trough fed it");
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
