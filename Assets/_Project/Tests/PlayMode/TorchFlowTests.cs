using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The mine is dark and blue; a torch in the picked slot lights a bubble round the player (owner, 2026-10-11).
    public class TorchFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-torch-" + Guid.NewGuid().ToString("N"));
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
        public IEnumerator InTheMine_TheLightIsDimAndBlue_AndTheTorchLightsOnlyWhilePicked()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.Mine.Floor = 1;
            session.Backpack.Add(ItemIds.Torch, 3);
            var slot = Enumerable.Range(0, session.Backpack.Capacity).First(i => session.Backpack.Get(i)?.ItemId == ItemIds.Torch);
            var other = Enumerable.Range(0, session.Backpack.Capacity).First(i => i != slot);
            session.State.SelectedHotbar = other;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var global = UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None).First(l => l.lightType == Light2D.LightType.Global);
            var cave = DayNightLighting.AsLight(DayNightLighting.Cave);          // a tenth as the eye sees it, given in linear terms
            Assert.AreEqual(cave.r, global.color.r, 0.002f);
            Assert.AreEqual(cave.b, global.color.b, 0.002f, "dim and blue");
            Assert.Less(global.color.b, 0.03f, "dark enough that the torch is needed (a tenth as the eye sees it)");

            var torch = UnityEngine.Object.FindAnyObjectByType<TorchLight>();
            Assert.IsNotNull(torch);
            Assert.IsFalse(torch.IsLit, "nothing in hand");
            session.State.SelectedHotbar = slot;
            for (var i = 0; i < 5; i++) yield return null;
            Assert.IsTrue(torch.IsLit, "the torch is picked");
            var light = torch.GetComponentInChildren<Light2D>();
            Assert.AreEqual(5f, light.pointLightOuterRadius, "five tiles");
            Assert.GreaterOrEqual(torch.CurrentColor.r, 0.9f, "a warm colour");
            session.State.SelectedHotbar = other;
            for (var i = 0; i < 5; i++) yield return null;
            Assert.IsFalse(torch.IsLit, "and out again when something else is picked");
        }
    }
}
