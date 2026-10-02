using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-031 in the real scenes: walking between maps through the actual trigger volumes, business hours at the doors
    // and counters, and the closed gate in the forest. A simulated keyboard walks and interacts.
    public class VillageFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly System.Collections.Generic.List<string> _toasts = new System.Collections.Generic.List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-villagetests-" + Guid.NewGuid().ToString("N"));
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

        // Starts a game on `day` (1 = Monday) at `hour`, standing at a spawn of `map`.
        IEnumerator Start(string map, string spawn, int day, int hour)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;   // random clutter would make tile positions unpredictable
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, day, hour * 60));
            _session.State.SetDate(_session.Clock.Now);
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = spawn;
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();

        IEnumerator WaitForMap(string map)
        {
            var start = Time.realtimeSinceStartup;
            while (!(SceneManager.GetActiveScene().name == map && !ServiceLocator.Get<SceneLoader>().IsLoading)
                   && Time.realtimeSinceStartup - start < 10f) yield return null;
            for (var i = 0; i < 4; i++) yield return null;
        }

        static void StandOn(Warp warp) => Player.transform.position = warp.transform.position;

        static Warp WarpTo(string target) =>
            UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == target);

        // ---- travelling -------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheFarmRoadLeadsToTheVillage_AndBack()
        {
            yield return Start(MapIds.Farm, "default", 1, 10);
            StandOn(WarpTo(MapIds.Village));
            yield return WaitForMap(MapIds.Village);
            Assert.AreEqual(MapIds.Village, SceneManager.GetActiveScene().name);
            Assert.Less(Vector2.Distance(Player.transform.position, new Vector2(2.5f, 17.5f)), 1.5f, "arrives at the west end of the road");

            StandOn(WarpTo(MapIds.Farm));
            yield return WaitForMap(MapIds.Farm);
            Assert.AreEqual(MapIds.Farm, SceneManager.GetActiveScene().name);
            Assert.AreEqual(MapIds.Farm, _session.State.CurrentMap);
        }

        [UnityTest]
        public IEnumerator TheForestAndTheBeachAreOneStepFromTheVillage()
        {
            yield return Start(MapIds.Village, "fromFarm", 1, 10);
            StandOn(WarpTo(MapIds.Forest));
            yield return WaitForMap(MapIds.Forest);
            Assert.AreEqual(MapIds.Forest, SceneManager.GetActiveScene().name);

            StandOn(WarpTo(MapIds.Village));
            yield return WaitForMap(MapIds.Village);
            StandOn(WarpTo(MapIds.Beach));
            yield return WaitForMap(MapIds.Beach);
            Assert.AreEqual(MapIds.Beach, SceneManager.GetActiveScene().name);
        }

        // ---- business hours at the doors --------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheGeneralStoreDoor_OpensDuringTheDay()
        {
            yield return Start(MapIds.Village, "fromFarm", 1, 10);   // Monday 10:00
            StandOn(WarpTo(MapIds.GeneralStore));
            yield return WaitForMap(MapIds.GeneralStore);
            Assert.AreEqual(MapIds.GeneralStore, SceneManager.GetActiveScene().name);

            StandOn(WarpTo(MapIds.Village));
            yield return WaitForMap(MapIds.Village);
            Assert.Less(Vector2.Distance(Player.transform.position, new Vector2(8.5f, 22.5f)), 1.5f, "steps out in front of the shop door");
        }

        [UnityTest]
        public IEnumerator TheGeneralStoreDoor_IsLockedBeforeOpeningTime_AndExplainsItself()
        {
            yield return Start(MapIds.Village, "fromFarm", 1, 7);   // Monday 07:00
            StandOn(WarpTo(MapIds.GeneralStore));
            yield return new WaitForSeconds(0.4f);   // a few physics steps
            Assert.AreEqual(MapIds.Village, SceneManager.GetActiveScene().name, "still outside");
            CollectionAssert.Contains(_toasts, "The General Store is closed. Hours: 9:00 AM - 5:00 PM, closed Sun.");
        }

        [UnityTest]
        public IEnumerator TheGeneralStoreDoor_IsLockedOnItsDayOff()
        {
            yield return Start(MapIds.Village, "fromFarm", 7, 12);   // Sunday noon
            StandOn(WarpTo(MapIds.GeneralStore));
            yield return new WaitForSeconds(0.4f);   // a few physics steps
            Assert.AreEqual(MapIds.Village, SceneManager.GetActiveScene().name);
            Assert.IsTrue(_toasts.Any(t => t.Contains("closed")));
        }

        [UnityTest]
        public IEnumerator EachDoor_FollowsItsOwnHours()
        {
            // Monday 10:00: the blacksmith is closed on Mondays, the carpenter is open.
            yield return Start(MapIds.Village, "fromFarm", 1, 10);
            StandOn(WarpTo(MapIds.Blacksmith));
            yield return new WaitForSeconds(0.4f);   // a few physics steps
            Assert.AreEqual(MapIds.Village, SceneManager.GetActiveScene().name);
            Assert.IsTrue(_toasts.Any(t => t.StartsWith("The Blacksmith is closed")));

            StandOn(WarpTo(MapIds.Carpenter));
            yield return WaitForMap(MapIds.Carpenter);
            Assert.AreEqual(MapIds.Carpenter, SceneManager.GetActiveScene().name);
        }

        // ---- the counter ------------------------------------------------------------------------------------------

        IEnumerator UseCounter()
        {
            _session.State.SpawnPoint = "default";
            Player.transform.position = new Vector3(5.5f, 4.5f, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 5; i++) yield return null;
            Press(_keyboard.eKey);
            yield return null;
            Release(_keyboard.eKey);
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheCounter_OpensTheShopWhileTheStoreIsOpen()
        {
            yield return Start(MapIds.GeneralStore, "default", 2, 10);
            yield return UseCounter();
            Assert.IsTrue(ServiceLocator.Get<IUiService>().AnyModalOpen, "the shop screen is open");
        }

        [UnityTest]
        public IEnumerator TheCounter_RefusesAfterClosingTime()
        {
            yield return Start(MapIds.GeneralStore, "default", 2, 18);
            yield return UseCounter();
            Assert.IsFalse(ServiceLocator.Get<IUiService>().AnyModalOpen);
            Assert.IsTrue(_toasts.Any(t => t.StartsWith("The General Store is closed")));
        }

        // ---- the forest gate --------------------------------------------------------------------------------------

        IEnumerator WalkUp(float seconds)
        {
            Press(_keyboard.wKey);
            yield return new WaitForSeconds(seconds);
            Release(_keyboard.wKey);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheForestPath_EndsAtBrambles()
        {
            yield return Start(MapIds.Forest, "default", 1, 10);
            Player.transform.position = new Vector3(19.5f, 22.5f, 0f);
            yield return WalkUp(2.5f);

            Assert.AreEqual(MapIds.Forest, SceneManager.GetActiveScene().name);
            Assert.Less(Player.transform.position.y, 28f, "the brambles stop the player");
            CollectionAssert.Contains(_toasts, "Thick brambles block the path north.");
        }

        [UnityTest]
        public IEnumerator WithTheWoodsFlagOn_TheGateOpens_ButTheBaseGameHasNothingBehindIt()
        {
            yield return Start(MapIds.Forest, "default", 1, 10);
            _session.SetFlag(MapIds.WoodsOpenFlag);
            for (var i = 0; i < 3; i++) yield return null;
            var brambles = UnityEngine.Object.FindFirstObjectByType<ConditionalObject>();
            Assert.IsFalse(brambles.transform.GetChild(0).gameObject.activeSelf, "the brambles are gone");

            Player.transform.position = new Vector3(19.5f, 22.5f, 0f);
            yield return WalkUp(2.5f);
            Assert.AreEqual(MapIds.Forest, SceneManager.GetActiveScene().name, "no Woods scene ships in the base game");
            CollectionAssert.Contains(_toasts, "You cannot go that way.");
        }
    }
}
