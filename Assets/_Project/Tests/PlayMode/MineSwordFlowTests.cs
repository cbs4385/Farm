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
    // Playtest report (2026-10-05): "in the mines, using the sword on the orange squares that attack does nothing."
    public class MineSwordFlowTests : InputTestFixture
    {
        string _root;
        Keyboard _kb;
        GameSession _s;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-minesword-" + Guid.NewGuid().ToString("N"));
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

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        IEnumerator Tap(Key k) { Press(_kb[k]); yield return null; Release(_kb[k]); yield return null; }

        [UnityTest]
        public IEnumerator EverySortOfMonster_IsHurtByASwordSwingFromTheKeyboard([ValueSource(nameof(Ids))] string enemyId)
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
            _s.State.Mine.Floor = 1;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            _s.State.SelectedHotbar = Enumerable.Range(0, _s.Backpack.Capacity).First(i => _s.Backpack.Get(i)?.ItemId == ItemIds.Sword);
            var enemy = EnemyManager.Current.Enemies.First();
            enemy.Setup(EnemyDefaults.Row(enemyId), 1, EnemyManager.Current);
            foreach (var other in EnemyManager.Current.Enemies.Where(e => e != enemy)) other.gameObject.SetActive(false);

            var spawn = MineController.Current.Floor.Spawn;
            var at = new Vector3(spawn.x + 0.5f, spawn.y + 0.5f, 0f);
            Player.transform.position = at;
            Player.Face(Vector2Int.right);
            var before = enemy.Brain.Health;
            var hits = 0;
            var feedback = false;
            var sparks = false;
            for (var swing = 0; swing < 3 && enemy != null; swing++)
            {
                enemy.transform.position = at + Vector3.right * 1.0f;          // held still, one cell in front
                Player.transform.position = at;
                var health = enemy.Brain.Health;
                yield return Tap(Key.C);
                sparks |= UnityEngine.Object.FindObjectsByType<ActionPuff>(FindObjectsSortMode.None).Length > 0;
                if (enemy == null) { hits++; feedback = true; break; }         // it died: that is a hit too
                if (enemy.Brain.Health < health) { hits++; feedback |= enemy.HealthBarShown; }
                var wait = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - wait < CombatModel.SwingCooldown + 0.05f) yield return null;
            }
            Assert.Greater(hits, 0, enemyId + " lost health to the sword (from " + before + ")");
            Assert.IsTrue(feedback, enemyId + " shows that it was hit (a health bar over it, or it is gone)");
            Assert.IsTrue(sparks, "sparks fly from the hit");
        }

        static string[] Ids() => EnemyDefaults.Rows.Select(r => r.Id).ToArray();
    }
}
