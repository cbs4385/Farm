using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest comment (2026-10-06): on the first talk Ione says she has put something on the desk for the player, but the library desk could
    // not be used. The desk now hands over the book she set aside (once).
    public class LibraryDeskFlowTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-desk-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator TheLibraryDesk_CanBeUsed_AndHandsOverTheBookOnce()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 12 * 60));      // the library is open
            session.State.CurrentMap = MapIds.Library;
            var op = SceneManager.LoadSceneAsync(MapIds.Library);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var desk = GameObject.Find("Desk");
            Assert.IsNotNull(desk, "the library has a desk");
            var hit = Physics2D.OverlapPointAll(desk.transform.position).Select(c => c.GetComponentInParent<IInteractable>()).FirstOrDefault(i => i != null);
            Assert.IsNotNull(hit, "the player can interact with the desk, as with any counter");

            var player = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            Assert.AreEqual(0, session.Backpack.Count(LibraryDesk.BookItemId));
            hit.Interact(player);
            Assert.AreEqual(1, session.Backpack.Count(LibraryDesk.BookItemId), "the book Ione set aside is handed over");
            Assert.IsTrue(session.HasFlag(LibraryDesk.TakenFlag));

            hit.Interact(player);
            Assert.AreEqual(1, session.Backpack.Count(LibraryDesk.BookItemId), "only once");
        }
    }
}
