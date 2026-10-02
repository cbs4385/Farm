using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The monsters of the loaded mine floor: spawns them, answers the player's swings, drops loot when one dies.
    public sealed class EnemyManager : MonoBehaviour
    {
        FarmMap _map;
        GameSession _session;
        MineController _mine;
        PlayerController _player;
        readonly List<EnemyActor> _enemies = new List<EnemyActor>();

        public static EnemyManager Current { get; private set; }
        public IReadOnlyList<EnemyActor> Enemies => _enemies;
        public bool Active => _session != null && _session.InGame && !_session.Clock.IsPaused;
        public Vector2 PlayerPosition => _player != null ? (Vector2)_player.transform.position : Vector2.zero;

        public void Init(FarmMap map, GameSession session, MineController mine, IEnumerable<MineEnemySpawn> spawns)
        {
            _map = map; _session = session; _mine = mine;
            _player = FindFirstObjectByType<PlayerController>();
            Current = this;
            var i = 0;
            foreach (var spawn in spawns)
            {
                var row = EnemyDefaults.Row(spawn.EnemyId);
                var go = new GameObject("enemy", typeof(SpriteRenderer));
                go.transform.position = _map.CellCenter(new Vector3Int(spawn.X, spawn.Y, 0));
                var actor = go.AddComponent<EnemyActor>();
                actor.Setup(row, session.State.WorldSeed + spawn.X * 31 + spawn.Y * 17 + i++, this);
                _enemies.Add(actor);
            }
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public bool Walkable(Vector2 world)
        {
            var cell = _map.WorldToCell(world);
            return !_mine.IsWall(cell.x, cell.y);
        }

        public void StrikePlayer(int damage)
        {
            var combat = _player != null ? _player.GetComponent<PlayerCombat>() : null;
            combat?.TakeDamage(damage);
        }

        // A swing: everything within `reach` in front of `origin` is hit. Returns how many enemies were hit.
        public int HitArea(Vector2 origin, Vector2 facing, float reach, int damage)
        {
            var hits = 0;
            foreach (var enemy in _enemies.ToArray())
            {
                if (enemy == null || enemy.IsDead) continue;
                var to = (Vector2)enemy.transform.position - origin;
                if (to.magnitude > reach + (enemy.Brain.Row.Boss ? 1f : 0f)) continue;
                if (Vector2.Dot(to.normalized, facing) < -0.2f && to.magnitude > 0.6f) continue;   // behind the player
                hits++;
                if (enemy.Hurt(damage, facing * 0.5f)) Kill(enemy);
            }
            return hits;
        }

        void Kill(EnemyActor enemy)
        {
            var row = enemy.Brain.Row;
            var ms = _session.State.Mine;
            ms.Kills++;
            var seed = ms.Kills * 53 + _session.Clock.Now.TotalDays;
            var loot = CombatModel.Loot(row, _session.Luck, i => WeatherRoller.Unit(seed * 7 + i, _session.State.WorldSeed ^ 0x6A09E667));
            foreach (var (item, count) in loot) _session.GiveItem(item, count);
            _session.AddSkillXp(SkillIds.Combat, row.Xp);
            _session.AddVar("stat.kills", 1);
            if (row.Boss)
            {
                ms.BossesDefeated++;
                _session.SetFlag("mine.boss_defeated");
                _session.Toast(L.Get("mine.boss_down"));
            }
            _enemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }
}
