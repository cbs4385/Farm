using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-11: "deep mine levels are too hard to reach within the time limit; the player needs a way to fast travel out". Once floor 5 has been reached
    // the elevator stands at the start of every floor, and its list ends with a way out of the mine.
    public class MineElevatorFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-elevator-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [UnityTest]
        public IEnumerator OnADeepFloor_TheElevatorIsThere_AndLeaveTheMineTakesYouOut()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.Mine.Floor = 13;
            session.State.Mine.Deepest = 13;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<MineStairs>(FindObjectsSortMode.None).Any(s => s.Kind == MineStairs.StairKind.Elevator), "an elevator on floor 13");
            ServiceLocator.Get<IUiService>().ShowElevator();
            yield return null;
            var leave = Resources.FindObjectsOfTypeAll<Button>().First(b => b.name == "Leave" && b.gameObject.scene.IsValid());
            Assert.IsNotNull(Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(b => b.name == "Floor10" && b.gameObject.scene.IsValid()), "and it stops at floor 10");
            leave.onClick.Invoke();
            Assert.AreEqual(0, session.State.Mine.Floor, "out of the mine");
        }
    }
}
