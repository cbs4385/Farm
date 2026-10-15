using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest request (2026-10-15): "anyway we could press and hold instead of mult clicking on the axe and pickaxe?" Holding the tool button (mouse, X or a pad button)
    // swings again when the last swing is over, and lets go at once on release, a spent node, no energy or a menu. A click is still one swing.
    public class HoldToSwingFlowTests : InputTestFixture
    {
        const int NodeX = 31, NodeY = 4;
        string _dataRoot;
        Keyboard _keyboard;
        Mouse _mouse;
        Gamepad _pad;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hold-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
            _pad = InputSystem.AddDevice<Gamepad>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static FarmMapView View => UnityEngine.Object.FindAnyObjectByType<FarmMapView>();

        // A new game on the farm with a node in front of the player, the tool in hand and full energy.
        IEnumerator Enter(string tool, string nodeType, int hp)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            for (var i = 0; i < _session.Backpack.Capacity; i++)
                if (_session.Backpack.Get(i)?.ItemId == tool) _session.State.SelectedHotbar = i;
            var nodes = _session.GetNodes(MapIds.Farm);
            nodes.Add(NodeX, NodeY, _session.Nodes.Get(nodeType));
            nodes.TryGet(NodeX, NodeY, out var node);
            node.Hp = hp;
            View.RefreshNode(new Vector3Int(NodeX, NodeY, 0));
            Player.transform.position = new Vector3(NodeX - 1 + 0.5f, NodeY + 0.5f, 0f);
            Player.Face(Vector2Int.right);
            for (var i = 0; i < 4; i++) yield return null;
            _session.State.Energy = _session.State.MaxEnergy;
        }

        int Hp { get { _session.GetNodes(MapIds.Farm).TryGet(NodeX, NodeY, out var n); return n?.Hp ?? 0; } }

        IEnumerator Wait(float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        [UnityTest]
        public IEnumerator HoldingX_SwingsTheAxeAgainAndAgain_UntilReleased()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            var start = Hp;
            var damage = ToolModel.Damage(_session.ToolTier(ToolModel.ItemId(ToolType.Axe)));
            Press(_keyboard.xKey);
            yield return Wait(1.3f);
            Release(_keyboard.xKey);
            yield return null;
            var swings = (start - Hp) / damage;
            Assert.GreaterOrEqual(swings, 3, "one at the press and one each 0.4 seconds after");
            Assert.LessOrEqual(swings, 4, "holding never swings faster than the swing takes");
            var after = Hp;
            yield return Wait(1f);
            Assert.AreEqual(after, Hp, "no swing after the button is let go");
        }

        [UnityTest]
        public IEnumerator Releasing_StopsTheSequence_MidWay()
        {
            yield return Enter(ItemIds.Pickaxe, "rock", 40);
            var start = Hp;
            Press(_keyboard.xKey);
            yield return Wait(0.6f);
            Release(_keyboard.xKey);
            yield return null;
            var stopped = Hp;
            Assert.Less(stopped, start, "it swung");
            yield return Wait(1.2f);
            Assert.AreEqual(stopped, Hp, "released: it stays where it was");
        }

        [UnityTest]
        public IEnumerator AClick_IsOneSwing_EachTime()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            var damage = ToolModel.Damage(_session.ToolTier(ToolModel.ItemId(ToolType.Axe)));
            var start = Hp;
            Press(_keyboard.xKey);
            yield return null;
            Release(_keyboard.xKey);
            yield return Wait(1.2f);
            Assert.AreEqual(damage, start - Hp, "one click, one swing");
            Press(_keyboard.xKey);
            yield return null;
            Release(_keyboard.xKey);
            yield return Wait(1.2f);
            Assert.AreEqual(2 * damage, start - Hp, "the next click is one more");
        }

        [UnityTest]
        public IEnumerator HoldingTheMouseButton_RepeatsToo()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            var start = Hp;
            Press(_mouse.leftButton);
            yield return Wait(1.0f);
            Release(_mouse.leftButton);
            yield return null;
            Assert.Less(Hp, start - ToolModel.Damage(_session.ToolTier(ToolModel.ItemId(ToolType.Axe))), "more than the first swing");
        }

        [UnityTest]
        public IEnumerator HoldingAPadButton_RepeatsToo()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            var start = Hp;
            Press(_pad.buttonWest);
            yield return Wait(1.0f);
            Release(_pad.buttonWest);
            yield return null;
            Assert.Less(Hp, start - ToolModel.Damage(_session.ToolTier(ToolModel.ItemId(ToolType.Axe))), "more than the first swing");
        }

        [UnityTest]
        public IEnumerator NoSwingAtAClearedNode_AndNothingMoreIsSpent()
        {
            yield return Enter(ItemIds.Pickaxe, "rock", 1);
            var energy = _session.State.Energy;
            Press(_keyboard.xKey);
            yield return Wait(1.3f);
            Release(_keyboard.xKey);
            yield return null;
            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(NodeX, NodeY), "the rock broke");
            var cost = ToolModel.EnergyCost(PlayerActions.PickaxeEnergy, _session.ToolTier(ToolModel.ItemId(ToolType.Pickaxe)));
            Assert.AreEqual(cost, energy - _session.State.Energy, "one swing paid for, none at the empty cell");
        }

        [UnityTest]
        public IEnumerator RunningOutOfEnergy_StopsTheRepeat()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            var tier = _session.ToolTier(ToolModel.ItemId(ToolType.Axe));
            var cost = ToolModel.EnergyCost(PlayerActions.AxeEnergy, tier);
            _session.State.Energy = cost * 2;
            var start = Hp;
            Press(_keyboard.xKey);
            yield return Wait(2.0f);
            Release(_keyboard.xKey);
            yield return null;
            Assert.AreEqual(2 * ToolModel.Damage(tier), start - Hp, "two swings were paid for, and no third");
        }

        [UnityTest]
        public IEnumerator OpeningAMenu_StopsTheRepeat_ForGood()
        {
            yield return Enter(ItemIds.Axe, "tree", 40);
            Press(_keyboard.xKey);
            yield return Wait(0.6f);
            GameContext.Current.Ui.ToggleInventory();
            yield return null;
            var stopped = Hp;
            yield return Wait(1.0f);
            Assert.AreEqual(stopped, Hp, "no swings while the menu is open");
            GameContext.Current.Ui.ToggleInventory();
            yield return Wait(1.0f);
            Assert.AreEqual(stopped, Hp, "and none after it closes with the key still down: the swing has to be started again");
            Release(_keyboard.xKey);
        }

    }
}
