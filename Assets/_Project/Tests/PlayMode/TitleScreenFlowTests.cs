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
    // The main menu in the real game: the picture is behind it, the breeze moves it, and the menu still works over it.
    public class TitleScreenFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-title-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        [UnityTest]
        public IEnumerator TheMainMenu_ShowsThePicture_ItSways_AndTheButtonsStillWork()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var op = SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var backdrop = UnityEngine.Object.FindAnyObjectByType<TitleBackdrop>();
            Assert.IsNotNull(backdrop, "the picture is behind the menu");
            Assert.IsTrue(backdrop.Sways, "and it was read, so it can sway");
            Assert.AreEqual(1671f / 941f, backdrop.GetComponentInParent<AspectRatioFitter>().aspectRatio, 0.001f, "kept in proportion, never stretched");

            // The points of the picture really move: compare two moments (the mesh the canvas has).
            Vector3[] Snapshot() { Canvas.ForceUpdateCanvases(); return Enumerable.Range(0, backdrop.PointCount).Select(backdrop.PointAt).ToArray(); }      // a headless run draws no frames, so the canvas is updated by hand
            yield return null;
            var first = Snapshot();
            yield return new WaitForSeconds(0.6f);
            var second = Snapshot();
            Assert.IsTrue(backdrop.isActiveAndEnabled, "the picture is showing");
            Assert.AreNotEqual(Vector3.zero, first[first.Length / 2], $"the mesh was built (canvas {backdrop.canvas != null}, rect {backdrop.rectTransform.rect}, active {backdrop.IsActive()}, tex {backdrop.mainTexture}, cull {backdrop.canvasRenderer.cull}, canvasEnabled {(backdrop.canvas != null && backdrop.canvas.enabled)}, parentActive {backdrop.transform.parent.gameObject.activeInHierarchy})");
            var moved = 0;
            for (var i = 0; i < first.Length; i++) if ((first[i] - second[i]).sqrMagnitude > 1e-6f) moved++;
            Assert.Greater(moved, first.Length / 10, "the breeze moves a good part of the picture's points");
            Assert.Less(moved, first.Length * 9 / 10, "and leaves the rest still");

            var ambience = UnityEngine.Object.FindAnyObjectByType<TitleAmbience>();
            Assert.IsNotNull(ambience, "leaves, smoke and lights are over the picture");

            var options = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "ReportBug");
            options.onClick.Invoke();
            yield return null;
            Assert.IsTrue(ServiceLocator.Get<UiService>().BugReporter.IsOpen, "the buttons over the picture still work");
        }
    }
}
