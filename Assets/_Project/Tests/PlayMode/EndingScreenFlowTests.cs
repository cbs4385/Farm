using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Reaching an ending shows its illustration full screen; closing it goes on.
    public class EndingScreenFlowTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-ending-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [UnityTest]
        public IEnumerator TheSealedEnding_ShowsItsPicture_ThenCloses()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            GameSession.HorrorLevelOverride = 2;
            yield return null;

            var ui = ServiceLocator.Get<IUiService>();
            Assert.IsFalse(ui.AnyModalOpen);
            MythosEnding.Finish(session, MythosEnding.Sealed);
            yield return null;

            Assert.IsTrue(ui.AnyModalOpen, "the illustration is up");
            var screen = GameObject.Find("Illustration");
            Assert.IsNotNull(screen);
            var image = screen.GetComponentInChildren<UnityEngine.UI.RawImage>();
            Assert.IsNotNull(image.texture, "with its picture");
            Assert.AreEqual(FilterMode.Point, image.texture.filterMode);
            Assert.IsTrue(session.HasFlag("ending.sealed"));

            screen.GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            Assert.IsFalse(ui.AnyModalOpen);
        }
    }
}
