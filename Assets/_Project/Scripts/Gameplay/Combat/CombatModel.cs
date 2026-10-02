using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public readonly struct WeaponRow
    {
        public readonly string ItemId; public readonly int Damage, Price; public readonly Color Color;
        public WeaponRow(string itemId, int damage, int price, Color color) { ItemId = itemId; Damage = damage; Price = price; Color = color; }
    }

    // Combat arithmetic and the enemy brain (T-052), pure and tested. Balance numbers live in the tables (EnemyDefaults,
    // Weapons).
    public static class CombatModel
    {
        public const float SwingCooldown = 0.45f;
        public const float SwingReach = 1.4f;           // cells in front of the player the blade covers
        public const float InvulnerableSeconds = 1f;    // after being hit
        public const int SwingEnergy = 1;

        public static readonly WeaponRow[] Weapons =
        {
            new WeaponRow(ItemIds.Sword, 10, 150, new Color(0.8f, 0.8f, 0.9f)),
            new WeaponRow("tool.sword_steel", 18, 1200, new Color(0.7f, 0.75f, 0.9f)),
            new WeaponRow("tool.sword_gold", 30, 5000, new Color(0.95f, 0.8f, 0.3f)),
        };

        public static int WeaponDamage(string itemId) => Weapons.Where(w => w.ItemId == itemId).Select(w => w.Damage).DefaultIfEmpty(5).First();

        // What a swing does: the weapon plus a point per Combat level.
        public static int SwingDamage(string weaponItemId, int combatLevel) => WeaponDamage(weaponItemId) + Math.Max(0, combatLevel - 1);

        // Gold lost when knocked out (a tenth, at most 1000), and the health the player wakes with.
        public static int KnockOutGoldLoss(int gold) => Math.Min(gold / 10, 1000);
        public static int KnockOutHealth(int maxHealth) => maxHealth / 2;

        // What an enemy drops: each row rolls its chance (luck raises it a little); `roll(i)` gives numbers in [0, 1).
        public static List<(string item, int count)> Loot(EnemyRow enemy, float luck, Func<int, float> roll)
        {
            var result = new List<(string, int)>();
            for (var i = 0; i < enemy.Drops.Length; i++)
            {
                var d = enemy.Drops[i];
                if (roll(i * 2) >= Math.Min(1f, d.Chance * (1f + 0.5f * Math.Max(0f, luck)))) continue;
                var count = d.Min + (int)(roll(i * 2 + 1) * (d.Max - d.Min + 1));
                result.Add((d.ItemId, Math.Min(d.Max, count)));
            }
            return result;
        }
    }

    public enum EnemyMode { Idle, Chase, Attack, Hurt, Dead }

    // The enemy's mind: a small state machine. Each frame it is told where it is and where the player is and answers with
    // a movement direction and whether it hits this frame.
    public sealed class EnemyBrain
    {
        public const float AttackRange = 0.9f;
        public const float AttackCooldown = 1.0f;
        public const float HurtSeconds = 0.25f;

        readonly EnemyRow _row;
        float _cooldown, _hurt, _wanderTimer;
        Vector2 _wander;
        readonly System.Random _rng;

        public int Health { get; private set; }
        public EnemyMode Mode { get; private set; } = EnemyMode.Idle;
        public EnemyRow Row => _row;

        public EnemyBrain(EnemyRow row, int seed)
        {
            _row = row;
            Health = row.Hp;
            _rng = new System.Random(seed);
        }

        // Returns true when this hit killed it.
        public bool Hurt(int damage)
        {
            if (Mode == EnemyMode.Dead) return false;
            Health -= damage;
            if (Health <= 0) { Mode = EnemyMode.Dead; return true; }
            Mode = EnemyMode.Hurt;
            _hurt = HurtSeconds;
            return false;
        }

        public struct Step { public Vector2 Move; public bool Strike; }

        public Step Tick(float dt, Vector2 self, Vector2 player)
        {
            var step = new Step();
            if (Mode == EnemyMode.Dead) return step;
            _cooldown = Mathf.Max(0f, _cooldown - dt);
            var toPlayer = player - self;
            var distance = toPlayer.magnitude;

            if (Mode == EnemyMode.Hurt)
            {
                _hurt -= dt;
                if (_hurt <= 0f) Mode = EnemyMode.Idle;
                return step;
            }

            if (distance <= _row.Aggro) Mode = distance <= AttackRange ? EnemyMode.Attack : EnemyMode.Chase;
            else Mode = EnemyMode.Idle;

            switch (Mode)
            {
                case EnemyMode.Chase:
                    step.Move = toPlayer.normalized * _row.Speed;
                    break;
                case EnemyMode.Attack:
                    if (_cooldown <= 0f) { step.Strike = true; _cooldown = AttackCooldown; }
                    break;
                default:
                    _wanderTimer -= dt;
                    if (_wanderTimer <= 0f)
                    {
                        _wanderTimer = 1f + (float)_rng.NextDouble() * 2f;
                        _wander = _rng.NextDouble() < 0.4 ? Vector2.zero : new Vector2((float)_rng.NextDouble() * 2f - 1f, (float)_rng.NextDouble() * 2f - 1f).normalized * _row.Speed * 0.4f;
                    }
                    step.Move = _wander;
                    break;
            }
            return step;
        }
    }
}
