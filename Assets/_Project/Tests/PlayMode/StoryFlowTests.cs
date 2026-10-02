using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
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
    // T-039/T-041 in the real game: the mailbox, the help-wanted board and the journal, with a simulated keyboard.
    public class StoryFlowTests : InputTestFixture
    {
        string _root;
        Keyboard _kb;
        GameSession _s;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-storytests-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Start(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _s.State.GetMap(map).LastSpawnDay = _s.Clock.Now.TotalDays;     // no random forage on the cells the test uses
            _s.State.CurrentMap = map;
            _s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        IEnumerator Tap(Key k) { Press(_kb[k]); yield return null; Release(_kb[k]); yield return null; }
        static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        static UiService Ui => ServiceLocator.Get<UiService>();

        [UnityTest]
        public IEnumerator TheMailbox_HoldsTheWelcomeLetter_ReadingItPaysAndKeepsIt()
        {
            yield return Start(MapIds.Farm);
            CollectionAssert.Contains(_s.State.Mailbox, "welcome");
            var gold = _s.State.Gold;
            Player.transform.position = new Vector3(10.5f, 18.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the letter opens");
            yield return Tap(Key.Escape);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.AreEqual(gold + 100, _s.State.Gold, "the letter's gift");
            CollectionAssert.Contains(_s.State.MailKept, "welcome");
            Assert.IsEmpty(_s.State.Mailbox);
        }

        [UnityTest]
        public IEnumerator TheBoard_ListsAJob_AndDeliveringPays()
        {
            yield return Start(MapIds.Village);
            _s.State.Board.Add(new BoardJob { Id = "j", ItemId = "crop.parsnip", Count = 2, Reward = 90, ExpiresDay = 99 });
            _s.Backpack.Add("crop.parsnip", 2);
            Player.transform.position = new Vector3(22.5f, 18.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 4; i++) yield return null;
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the board opens; player at " + Player.transform.position + " map " + SceneManager.GetActiveScene().name + " toasts");
            var deliver = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.gameObject.activeInHierarchy && b.name == "Deliver");
            var gold = _s.State.Gold;
            deliver.onClick.Invoke();
            Assert.AreEqual(gold + 90, _s.State.Gold);
            Assert.IsEmpty(_s.State.Board);
        }

        [UnityTest]
        public IEnumerator TheJournal_ShowsTheTutorialQuest()
        {
            yield return Start(MapIds.Farm);
            yield return Tap(Key.J);
            Assert.IsTrue(Ui.AnyModalOpen);
            Assert.AreEqual(MenuTabs.Journal, Ui.GameMenu.Current.Id);
            var text = string.Join("\n", Ui.GameMenu.Current.Root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).Select(t => t.text));
            StringAssert.Contains("First Furrows", text);
        }
    }
}
