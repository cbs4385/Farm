using System;
using System.Collections;
using System.IO;
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
    // A gift that does not fit the backpack waits in the mailbox instead of being lost; villagers' seats restore energy once a day.
    public class ParcelsAndSeatsFlowTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-parcel-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator AGiftThatDoesNotFit_WaitsInTheMailbox_AndIsHandedOverWhenThereIsRoom()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);

            while (session.Backpack.Add("resource.wood", 999) == 0) { }       // fill every slot
            session.GiveItem("prop.sock");
            Assert.AreEqual(0, session.Backpack.Count("prop.sock"));
            Assert.AreEqual(1, Parcels.Count(session), "the sock waits in the mailbox");

            Assert.AreEqual(0, Parcels.Claim(session).Count, "still no room");
            Assert.AreEqual(1, Parcels.Count(session));

            session.Backpack.Remove("resource.wood", session.Backpack.Count("resource.wood"));
            CollectionAssert.AreEqual(new[] { "prop.sock" }, Parcels.Claim(session));
            Assert.AreEqual(1, session.Backpack.Count("prop.sock"));
            Assert.AreEqual(0, Parcels.Count(session));
        }

        [UnityTest]
        public IEnumerator TheLibrarySeat_RestoresEnergyOncePerDay()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 12 * 60));
            session.State.CurrentMap = MapIds.Library;
            var op = SceneManager.LoadSceneAsync(MapIds.Library);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var seat = UnityEngine.Object.FindAnyObjectByType<SeatSpot>();
            Assert.IsNotNull(seat, "the library has Ione's seat");
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            session.SetEnergy(20);
            seat.Interact(player);
            Assert.AreEqual(20 + SeatSpot.Rest, session.State.Energy);
            seat.Interact(player);
            Assert.AreEqual(20 + SeatSpot.Rest, session.State.Energy, "only once a day");
        }
    }
}
