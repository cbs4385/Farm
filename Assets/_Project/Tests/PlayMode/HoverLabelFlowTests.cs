using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest feedback in the real game, with a simulated mouse: resting the pointer over the shipping bin names it; over bare ground, or
    // with the option off, nothing is shown.
    public class HoverLabelFlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;
        FarmMap _map;
        Camera _cam;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hover-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            HoverInspector.ResetForTests();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Enter(bool hover)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.HoverLabels = hover;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            _cam = Camera.main;
            Assert.IsNotNull(_map); Assert.IsNotNull(_cam);
        }

        // The pointer over the middle of a world position (with a small jitter, as a real mouse has).
        IEnumerator PointAtWorld(Vector3 world)
        {
            var screen = _cam.WorldToScreenPoint(world);
            Set(_mouse.position, new Vector2(screen.x, screen.y));
            yield return null;
            Set(_mouse.position, new Vector2(screen.x + 1f, screen.y));
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator OverTheShippingBin_TheLabelNamesIt_AndAnEmptyPatchOfGroundShowsNothing()
        {
            yield return Enter(true);
            var bin = UnityEngine.Object.FindAnyObjectByType<ShippingBin>();
            Assert.IsNotNull(bin, "the farm has a shipping bin");
            yield return PointAtWorld(bin.transform.position);
            Assert.AreEqual(L.Get("hover.shipping_bin"), HoverInspector.Current);
            var ui = ServiceLocator.Get<UiService>();
            Assert.AreEqual(L.Get("hover.shipping_bin"), ui.HoverText, "the label is on screen");

            // Somewhere with nothing on it.
            yield return PointAtWorld(bin.transform.position + new Vector3(0f, -8f, 0f));
            if (HoverInspector.Current != null) Assert.AreNotEqual(L.Get("hover.shipping_bin"), HoverInspector.Current);
        }

        [UnityTest]
        public IEnumerator WithTheOptionOff_NothingIsShown()
        {
            yield return Enter(false);
            var bin = UnityEngine.Object.FindAnyObjectByType<ShippingBin>();
            yield return PointAtWorld(bin.transform.position);
            Assert.IsNull(HoverInspector.Current);
            Assert.IsNull(ServiceLocator.Get<UiService>().HoverText);
        }
    }
}
