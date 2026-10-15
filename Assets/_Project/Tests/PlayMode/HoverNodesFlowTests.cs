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
    // Playtest 2026-10-15: bushes and logs on the farm showed nothing when the mouse rested on them. Every clearable thing names itself, with a simulated mouse in the real game.
    public class HoverNodesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;
        FarmMap _map;
        Camera _cam;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hovernodes-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Enter()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            GameContext.Current.Settings.Current.HoverLabels = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            _cam = Camera.main;
        }

        IEnumerator PointAt(Vector3Int cell)
        {
            var screen = _cam.WorldToScreenPoint(_map.CellCenter(cell));
            Set(_mouse.position, new Vector2(screen.x, screen.y));
            yield return null;
            Set(_mouse.position, new Vector2(screen.x + 1f, screen.y));
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator EveryThingAToolClears_NamesItselfWhenTheMouseRestsOnIt()
        {
            yield return Enter();
            var session = GameContext.Current.Session;
            var nodes = session.GetNodes(MapIds.Farm);
            var x = 28;
            foreach (var id in new[] { "bush", "log", "stump", "rock", "boulder", "weed", "tree", "sapling", "copper_node" })
            {
                var def = session.Nodes.Get(id);
                Assert.IsNotNull(def, id);
                Assert.IsTrue(nodes.Add(x, 8, def), id + " fits on the empty cell");
                yield return PointAt(new Vector3Int(x, 8, 0));
                Assert.AreEqual(L.Get("node." + id), HoverInspector.Current, id);
                Assert.AreEqual(L.Get("node." + id), ((UiService)GameContext.Current.Ui).HoverText, id + ": the label is on screen");
                x++;
            }
        }
    }
}
