using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-034/T-035 in the real game: villagers appear where their schedule says, walk between places without
    // teleporting, and can be talked to and given gifts with a simulated keyboard.
    public class NpcFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npctests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _toasts.Clear();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        // Starts on `map` on day `day` of spring (1 = Monday) at the given time.
        IEnumerator Start(string map, int day, int hour, int minute = 0, string weather = "sunny")
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, day, hour * 60 + minute));
            _session.State.SetDate(_session.Clock.Now);
            _session.State.Weather = weather;
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = "default";
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static IUiService Ui => ServiceLocator.Get<IUiService>();
        static NpcActor Actor(string id) => UnityEngine.Object.FindObjectsByType<NpcActor>().FirstOrDefault(a => a.Definition.Id == id);

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        IEnumerator StandBelow(float x, float y, Vector2Int facing)
        {
            Player.transform.position = new Vector3(x, y, 0f);
            Player.Face(facing);
            for (var i = 0; i < 4; i++) yield return null;
        }

        // Presses Enter until the conversation is over (taking the first option at each choice).
        IEnumerator FinishConversation(int maxPresses = 80)
        {
            for (var i = 0; i < maxPresses && Ui.AnyModalOpen; i++) yield return Tap(Key.Enter);
        }

        // ---- where they are -----------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheShopkeeper_IsAtHerPost_WhileTheShopIsOpen()
        {
            yield return Start(MapIds.GeneralStore, 3, 10);     // Wednesday 10:00
            var tilda = Actor("tilda");
            Assert.IsNotNull(tilda, "Tilda is in her shop");
            Assert.AreEqual(new Vector3Int(7, 5, 0), tilda.Cell);
            Assert.IsFalse(tilda.IsWalking);
        }

        [UnityTest]
        public IEnumerator TheShopkeeper_IsNotInTheShop_OnHerDayOff_AndAtNight()
        {
            yield return Start(MapIds.GeneralStore, 7, 12);     // Sunday: the shop is closed, Tilda is out
            Assert.IsNull(Actor("tilda"));
        }

        [UnityTest]
        public IEnumerator TheVillageHasVillagersOnTheirRoutes()
        {
            yield return Start(MapIds.Village, 3, 18);          // Wednesday 18:00: Tilda has left the shop and strolls the lane
            var tilda = Actor("tilda");
            Assert.IsNotNull(tilda, "Tilda is out in the village after work");
            Assert.AreEqual(new Vector3Int(25, 20, 0), tilda.Cell);
        }

        [UnityTest]
        public IEnumerator AVillager_WalksSmoothlyFromTheShopDoor_NeverJumping()
        {
            // Tilda leaves the shop at 17:30 on a workday. Watch her walk from the door into the village.
            yield return Start(MapIds.Village, 3, 17, 30);
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 17 * 60 + 38));
            for (var i = 0; i < 3; i++) yield return null;

            var tilda = Actor("tilda");
            Assert.IsNotNull(tilda, "she has stepped out of the door");
            var last = tilda.transform.position;
            var travelled = 0f;
            for (var step = 0; step < 14; step++)
            {
                _session.Clock.AdvanceMinutes(1);
                yield return null;
                Assert.IsNotNull(tilda, "she stays in view while walking along the village");
                var now = tilda.transform.position;
                var jump = Vector3.Distance(last, now);
                Assert.LessOrEqual(jump, MapRoutes.CellsPerMinute * 1.3f, $"no teleporting (step {step}, jumped {jump:0.00})");
                travelled += jump;
                last = now;
            }
            Assert.Greater(travelled, 8f, "she actually walked");
        }

        [UnityTest]
        public IEnumerator AVillager_ChangesMap_OnlyThroughADoor()
        {
            // At 17:20 Tilda is still in the shop; by 17:45 she has left: the shop scene no longer has her.
            yield return Start(MapIds.GeneralStore, 3, 17, 20);
            Assert.IsNotNull(Actor("tilda"));
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 17 * 60 + 50));
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsNull(Actor("tilda"), "she walked out of the door and is gone from the shop");
        }

        // ---- talking ----------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Talking_ToAVillager_PlaysTheFirstMeeting_WithRealKeys()
        {
            yield return Start(MapIds.GeneralStore, 3, 10);
            yield return StandBelow(7.5f, 4.5f, Vector2Int.up);
            Assert.IsFalse(_session.HasFlag("met.tilda"));

            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the dialogue box opens");
            Assert.IsTrue(_session.HasFlag("met.tilda"), "the first node's effect ran");

            yield return FinishConversation();
            Assert.IsFalse(Ui.AnyModalOpen, "the conversation ends");
            var state = _session.State.Npcs["tilda"];
            Assert.IsTrue(state.Met);
            Assert.AreEqual(FriendshipModel.TalkPoints + 10, state.Points, "the first choice is the friendly one (+10), plus the daily talk");
        }

        [UnityTest]
        public IEnumerator Escape_SkipsAheadInADialogue()
        {
            yield return Start(MapIds.GeneralStore, 3, 10);
            _session.SetFlag("met.tilda");                                  // skip the first meeting: an ordinary chat
            yield return StandBelow(7.5f, 4.5f, Vector2Int.up);
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen);
            yield return Tap(Key.Escape);                                   // finishes the typing
            yield return Tap(Key.Escape);                                   // skips to the end
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator TalkingTwice_OnTheSameDay_GivesFriendshipOnlyOnce()
        {
            yield return Start(MapIds.GeneralStore, 3, 10);
            _session.SetFlag("met.tilda");
            yield return StandBelow(7.5f, 4.5f, Vector2Int.up);
            for (var round = 0; round < 2; round++)
            {
                yield return Tap(Key.E);
                for (var i = 0; i < 3; i++) yield return null;
                yield return FinishConversation();
            }
            Assert.AreEqual(FriendshipModel.TalkPoints, _session.State.Npcs["tilda"].Points);
        }

        // ---- gifts ------------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator UsingAnItemOnAVillager_IsAGift()
        {
            yield return Start(MapIds.GeneralStore, 3, 10);
            _session.SetFlag("met.tilda");
            _session.Backpack.Add("crop.strawberry", 3);
            var slot = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == "crop.strawberry");
            _session.State.SelectedHotbar = slot;
            yield return StandBelow(7.5f, 4.5f, Vector2Int.up);

            yield return Tap(Key.C);                                        // the use-tool key
            for (var i = 0; i < 3; i++) yield return null;
            Assert.AreEqual(2, _session.Backpack.Count("crop.strawberry"), "one strawberry was given");
            Assert.AreEqual(80, _session.State.Npcs["tilda"].Points, "she loves strawberries");
            Assert.IsTrue(Ui.AnyModalOpen, "she reacts");
            yield return FinishConversation();

            // Only one gift a day.
            yield return Tap(Key.C);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.AreEqual(2, _session.Backpack.Count("crop.strawberry"));
            Assert.That(_toasts, Has.Some.Contains("gift"), "the player is told why");
        }
    }
}
