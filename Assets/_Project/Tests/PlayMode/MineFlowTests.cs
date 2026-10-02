using System;
using System.Collections;
using System.IO;
using System.Linq;
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
    // T-051/T-052 in the real game: the mine scene builds a floor, swords kill monsters (loot, XP), the ladder goes down, and
    // being knocked out sends the player to the clinic.
    public class MineFlowTests : InputTestFixture
    {
        string _root;
        Keyboard _kb;
        GameSession _s;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-minetests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            _kb = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator StartMine(int floor)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.State.WorldSeed = 4242;
            _s.State.CurrentMap = MapIds.Mine;
            _s.State.SpawnPoint = "default";
            _s.State.Mine.Floor = floor;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
        }

        IEnumerator Tap(Key k) { Press(_kb[k]); yield return null; Release(_kb[k]); yield return null; }
        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        void Select(string itemId) =>
            _s.State.SelectedHotbar = Enumerable.Range(0, _s.Backpack.Capacity).First(i => _s.Backpack.Get(i)?.ItemId == itemId);

        [UnityTest]
        public IEnumerator TheMineScene_BuildsAFloor_WithAnArrivalLadderAndMonsters()
        {
            yield return StartMine(1);
            var mine = MineController.Current;
            Assert.IsNotNull(mine);
            Assert.AreEqual(1, mine.Floor.Floor);
            Assert.AreEqual(mine.Floor.Enemies.Count, EnemyManager.Current.Enemies.Count);
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<MineStairs>().Any(x => x.Kind == MineStairs.StairKind.Down));
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<MineStairs>().Any(x => x.Kind == MineStairs.StairKind.Up));
            var spawn = mine.Floor.Spawn;
            Assert.Less(Vector2.Distance(Player.transform.position, new Vector2(spawn.x + 0.5f, spawn.y + 0.5f)), 1f, "the player arrives at the floor's spawn");
        }

        [UnityTest]
        public IEnumerator ASword_KillsAMonster_ForLootAndCombatXp()
        {
            yield return StartMine(1);
            var enemy = EnemyManager.Current.Enemies.First();
            var target = enemy.transform.position;
            Player.transform.position = target + new Vector3(-0.8f, 0f, 0f);
            Player.Face(Vector2Int.right);
            Select(ItemIds.Sword);
            var xp = _s.GetSkillXp(SkillIds.Combat);
            for (var swing = 0; swing < 8 && enemy != null; swing++)
            {
                enemy.transform.position = target;                      // hold still for the test
                Player.transform.position = target + new Vector3(-0.8f, 0f, 0f);
                yield return Tap(Key.C);
                var wait = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - wait < CombatModel.SwingCooldown + 0.05f) yield return null;
            }
            Assert.IsTrue(enemy == null, "the slime is dead");
            Assert.Greater(_s.GetSkillXp(SkillIds.Combat), xp);
            Assert.AreEqual(1, _s.State.Mine.Kills);
        }

        [UnityTest]
        public IEnumerator ThePickaxe_MinesOre_OnTheFloor()
        {
            yield return StartMine(1);
            var floor = MineController.Current.Floor;
            var node = floor.Nodes.First(n => n.NodeId == MineGenerator.Copper || n.NodeId == MineGenerator.Coal || n.NodeId == NodeDefaults.Rock);
            _s.GetNodes(MapIds.Mine).TryGet(node.X, node.Y, out var inst);
            Assert.IsNotNull(inst, "the node stands on the floor");
            Select(ItemIds.Pickaxe);
            // Stand on a free neighbour cell facing the node.
            var cells = new[] { (-1, 0, Vector2Int.right), (1, 0, Vector2Int.left), (0, -1, Vector2Int.up), (0, 1, Vector2Int.down) };
            var free = cells.First(c => !floor.IsWall(node.X + c.Item1, node.Y + c.Item2));
            Player.transform.position = new Vector3(node.X + free.Item1 + 0.5f, node.Y + free.Item2 + 0.5f, 0f);
            Player.Face(free.Item3);
            for (var i = 0; i < 4; i++) yield return null;
            for (var i = 0; i < 12 && _s.GetNodes(MapIds.Mine).Has(node.X, node.Y); i++) yield return Tap(Key.C);
            Assert.IsFalse(_s.GetNodes(MapIds.Mine).Has(node.X, node.Y), "the node was broken");
        }

        [UnityTest]
        public IEnumerator TheLadder_GoesDownAFloor_AndRemembersTheDeepest()
        {
            yield return StartMine(4);
            var ladder = MineController.Current.Floor.Ladder.Value;
            var down = UnityEngine.Object.FindObjectsByType<MineStairs>().First(x => x.Kind == MineStairs.StairKind.Down);
            down.Interact(Player.GetComponent<PlayerActions>());
            var start = Time.realtimeSinceStartup;
            while (MineController.Current == null || MineController.Current.Floor.Floor != 5 || Time.realtimeSinceStartup - start < 0.5f)
            {
                if (Time.realtimeSinceStartup - start > 10f) break;
                yield return null;
            }
            Assert.AreEqual(5, _s.State.Mine.Floor);
            Assert.AreEqual(5, _s.State.Mine.Deepest);
            Assert.That(ladder.x, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator BeingKnockedOut_LosesGold_AndWakesAtTheClinic()
        {
            yield return StartMine(2);
            _s.State.Gold = 1000;
            Player.GetComponent<PlayerCombat>().TakeDamage(500);
            var start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.Clinic && Time.realtimeSinceStartup - start < 10f) yield return null;
            Assert.AreEqual(MapIds.Clinic, SceneManager.GetActiveScene().name);
            Assert.AreEqual(900, _s.State.Gold, "a tenth of the gold is lost");
            Assert.AreEqual(50, _s.State.Health);
            Assert.AreEqual(0, _s.State.Mine.Floor);
        }
    }
}
