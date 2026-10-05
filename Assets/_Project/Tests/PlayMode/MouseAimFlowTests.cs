using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest feedback in the real game, with a simulated mouse and keyboard: moving the pointer over a cell next to the player puts the
    // tool square there (diagonals too), walking hands the square back to the facing, and the option turns the whole thing off.
    public class MouseAimFlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;
        Keyboard _keyboard;
        PlayerActions _actions;
        PlayerController _player;
        FarmMap _map;
        Camera _cam;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-aim-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Enter(bool mouseAim)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.MouseAim = mouseAim;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            _cam = Camera.main;
            Assert.IsNotNull(_actions); Assert.IsNotNull(_player); Assert.IsNotNull(_map); Assert.IsNotNull(_cam);
        }

        Vector3Int PlayerCell => _map.WorldToCell(_player.CellSamplePoint);

        // Puts the pointer over the middle of a cell and lets a couple of frames pass.
        IEnumerator PointAt(Vector3Int cell)
        {
            var screen = _cam.WorldToScreenPoint(_map.CellCenter(cell));
            Set(_mouse.position, new Vector2(screen.x, screen.y));
            yield return null;
            Set(_mouse.position, new Vector2(screen.x + 2f, screen.y));      // a real mouse jitters; the square must still land on the cell
            yield return null;
            Set(_mouse.position, new Vector2(screen.x, screen.y));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ThePointer_SteersTheSquare_ToEveryNeighbour()
        {
            yield return Enter(true);
            var me = PlayerCell;
            foreach (var d in new[] { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, 1, 0), new Vector3Int(-1, -1, 0) })
            {
                yield return PointAt(me + d);
                Assert.IsTrue(_actions.MouseAiming, "the mouse is steering");
                Assert.AreEqual(me + d, _actions.Target, "square on " + d);
            }
            yield return PointAt(me + new Vector3Int(7, 5, 0));
            Assert.AreEqual(me + new Vector3Int(1, 1, 0), _actions.Target, "a far pointer aims in its direction, one cell out");
        }

        [UnityTest]
        public IEnumerator Walking_GivesTheSquareBackToTheFacing()
        {
            yield return Enter(true);
            yield return PointAt(PlayerCell + new Vector3Int(1, 1, 0));
            Assert.IsTrue(_actions.MouseAiming);
            Press(_keyboard[Key.A]);
            for (var i = 0; i < 6; i++) yield return null;
            Release(_keyboard[Key.A]);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(_actions.MouseAiming, "walking ends mouse aiming");
        }

        [UnityTest]
        public IEnumerator WithTheOptionOff_TheMouseDoesNothing()
        {
            yield return Enter(false);
            yield return PointAt(PlayerCell + new Vector3Int(1, 1, 0));
            Assert.IsFalse(_actions.MouseAiming);
            Assert.AreEqual(PlayerCell + new Vector3Int(_player.Facing.x, _player.Facing.y, 0), _actions.Target, "facing aims, as before");
        }
    }
}
