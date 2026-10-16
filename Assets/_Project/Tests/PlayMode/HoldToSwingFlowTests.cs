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


        // ---- every tool, until the button is let go (owner, 2026-10-15: "any tool should continue to be used until the action button is released") ----

        void AimAt(Vector3Int cell)
        {
            var cam = Camera.main;
            var p = cam.WorldToScreenPoint(GameContext.Current.Map.Map.CellCenter(cell));
            Set(_mouse.position, new Vector2(p.x, p.y));
        }

        // The open cells around the player that can be tilled and are not (yet): where a hoe sweep can go.
        System.Collections.Generic.List<Vector3Int> FreshCellsAround()
        {
            var map = GameContext.Current.Map.Map;
            var grid = _session.GetGrid(MapIds.Farm);
            var nodes = _session.GetNodes(MapIds.Farm);
            var here = map.WorldToCell(Player.CellSamplePoint);
            var found = new System.Collections.Generic.List<Vector3Int>();
            foreach (var d in new[] { new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0) })
            {
                var c = here + d;
                if (map.IsTillable(c) && !grid.IsTilled(c.x, c.y) && !nodes.Has(c.x, c.y)) found.Add(c);
            }
            return found;
        }

        [UnityTest]
        public IEnumerator TheHoe_KeepsTillingFreshTiles_AsTheAimMovesWithTheButtonHeld()
        {
            yield return Enter(ItemIds.Hoe, "tree", 40);
            _session.GetNodes(MapIds.Farm).Remove(NodeX, NodeY);
            View.RefreshNode(new Vector3Int(NodeX, NodeY, 0));
            yield return null;
            var cells = FreshCellsAround();
            Assert.GreaterOrEqual(cells.Count, 2, "open ground to sweep over");
            var grid = _session.GetGrid(MapIds.Farm);
            var energy = _session.State.Energy;

            Set(_mouse.position, new Vector2(0, 0));
            yield return null;
            AimAt(cells[0]);
            yield return null;
            Press(_mouse.leftButton);
            yield return Wait(0.1f);
            Assert.IsTrue(grid.IsTilled(cells[0].x, cells[0].y), "the first press tills the first tile");
            for (var i = 1; i < cells.Count; i++)
            {
                AimAt(cells[i]);                                                  // the aim moves to a fresh tile with the button still down
                yield return Wait(0.9f);
                Assert.IsTrue(grid.IsTilled(cells[i].x, cells[i].y), "tile " + i + " is tilled without another click");
            }
            Release(_mouse.leftButton);
            yield return null;
            var spent = energy - _session.State.Energy;
            var cost = ToolModel.EnergyCost(PlayerActions.EnergyCost(ToolType.Hoe), _session.ToolTier(ToolModel.ItemId(ToolType.Hoe)));
            Assert.AreEqual(cost * cells.Count, spent, "each tile paid for once");
        }

        [UnityTest]
        public IEnumerator SweepingOverATilledTile_DoesNothingAndSaysNothing()
        {
            yield return Enter(ItemIds.Hoe, "tree", 40);
            _session.GetNodes(MapIds.Farm).Remove(NodeX, NodeY);
            View.RefreshNode(new Vector3Int(NodeX, NodeY, 0));
            yield return null;
            var cells = FreshCellsAround();
            Assert.GreaterOrEqual(cells.Count, 2);
            var grid = _session.GetGrid(MapIds.Farm);
            grid.Till(cells[0].x, cells[0].y);                                   // already tilled
            var energy = _session.State.Energy;
            Set(_mouse.position, new Vector2(0, 0));
            yield return null;
            AimAt(cells[1]);
            yield return null;
            Press(_mouse.leftButton);
            yield return Wait(0.1f);
            AimAt(cells[0]);                                                      // over the tilled one, held
            yield return Wait(1.0f);
            Release(_mouse.leftButton);
            yield return null;
            var cost = ToolModel.EnergyCost(PlayerActions.EnergyCost(ToolType.Hoe), _session.ToolTier(ToolModel.ItemId(ToolType.Hoe)));
            Assert.AreEqual(cost, energy - _session.State.Energy, "only the one fresh tile was paid for");
        }

        [UnityTest]
        public IEnumerator AfterANodeBreaks_HoldingOnSwingsAtTheNextOneTheAimMovesTo()
        {
            yield return Enter(ItemIds.Pickaxe, "rock", 1);
            var map = GameContext.Current.Map.Map;
            var here = map.WorldToCell(Player.CellSamplePoint);
            var second = here + new Vector3Int(0, 1, 0);
            var nodes = _session.GetNodes(MapIds.Farm);
            nodes.Add(second.x, second.y, _session.Nodes.Get("rock"));
            nodes.TryGet(second.x, second.y, out var other);
            other.Hp = 1;
            View.RefreshNode(second);
            yield return null;
            Set(_mouse.position, new Vector2(0, 0));
            yield return null;
            AimAt(new Vector3Int(NodeX, NodeY, 0));
            yield return null;
            Press(_mouse.leftButton);
            yield return Wait(0.2f);
            Assert.IsFalse(nodes.Has(NodeX, NodeY), "the first rock broke with the first swing");
            AimAt(second);                                                        // the button is still down
            yield return Wait(1.0f);
            Release(_mouse.leftButton);
            yield return null;
            Assert.IsFalse(nodes.Has(second.x, second.y), "the second rock broke without another click");
        }

        [UnityTest]
        public IEnumerator InTheMine_HoldingThePickaxe_KeepsSwinging()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.SetFlag(Farm.UI.HudView.HealthBarFlag);                   // the one-time explanation of the red bar stops the game in a new mine
            _session.State.CurrentMap = MapIds.Mine;
            _session.State.Mine.Floor = 1;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            var until = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < until && UnityEngine.Object.FindAnyObjectByType<MineController>()?.Floor == null) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            for (var i = 0; i < _session.Backpack.Capacity; i++)
                if (_session.Backpack.Get(i)?.ItemId == ItemIds.Pickaxe) _session.State.SelectedHotbar = i;
            var mine = UnityEngine.Object.FindAnyObjectByType<MineController>();
            var map = GameContext.Current.Map.Map;
            var here = map.WorldToCell(Player.CellSamplePoint);
            Vector3Int rockCell = default; var dir = Vector2Int.zero;
            foreach (var d in new[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down })
                if (!mine.IsWall(here.x + d.x, here.y + d.y)) { rockCell = new Vector3Int(here.x + d.x, here.y + d.y, 0); dir = d; break; }
            Assert.AreNotEqual(Vector2Int.zero, dir, "an open cell next to the arrival cell");
            var nodes = _session.GetNodes(MapIds.Mine);
            nodes.Remove(rockCell.x, rockCell.y);
            nodes.Add(rockCell.x, rockCell.y, _session.Nodes.Get("rock"));
            nodes.TryGet(rockCell.x, rockCell.y, out var rock);
            rock.Hp = 40;
            Player.Face(dir);
            for (var i = 0; i < 4; i++) yield return null;
            _session.State.Energy = _session.State.MaxEnergy;
            var start = rock.Hp;
            var damage = ToolModel.Damage(_session.ToolTier(ToolModel.ItemId(ToolType.Pickaxe)));

            Press(_keyboard.xKey);
            yield return Wait(1.3f);
            Release(_keyboard.xKey);
            yield return null;

            nodes.TryGet(rockCell.x, rockCell.y, out rock);
            Assert.GreaterOrEqual((start - rock.Hp) / damage, 3, "in the mine too: one swing at the press and one each 0.4 seconds");
        }
    }
}
