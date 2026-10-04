using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // On the player in every map: swinging a weapon, taking damage, and being knocked out. Health lives in GameState.
    public sealed class PlayerCombat : MonoBehaviour
    {
        float _cooldown, _invulnerable;
        GameSession _session;
        PlayerController _player;

        public bool Invulnerable => _invulnerable > 0f;

        void Start()
        {
            _session = ServiceLocator.Get<GameSession>();
            _player = GetComponent<PlayerController>();
        }

        void Update()
        {
            _cooldown = Mathf.Max(0f, _cooldown - Time.deltaTime);
            _invulnerable = Mathf.Max(0f, _invulnerable - Time.deltaTime);
        }

        // Returns true if the swing happened (it costs a little energy).
        public bool Swing(string weaponItemId)
        {
            if (_cooldown > 0f || _session == null || !_session.TrySpendEnergy(CombatModel.SwingEnergy)) return false;
            _cooldown = CombatModel.SwingCooldown;
            AudioService.PlayIfAvailable(Sfx.SwordSwing);
            var enemies = EnemyManager.Current;
            if (enemies == null) return true;
            var damage = Mathf.RoundToInt(CombatModel.SwingDamage(weaponItemId, _session.GetSkillLevel(SkillIds.Combat)) * Professions.DamageMultiplier(_session.State));
            enemies.HitArea(transform.position, new Vector2(_player.Facing.x, _player.Facing.y), CombatModel.SwingReach, damage);
            return true;
        }

        public void TakeDamage(int damage)
        {
            if (_session == null || !_session.InGame || Invulnerable) return;
            _invulnerable = CombatModel.InvulnerableSeconds;
            AudioService.PlayIfAvailable(Sfx.Hit);
            if (Combat.Hurt(_session, damage)) Combat.KnockOut(_session);
        }
    }

    public static class Combat
    {
        // Applies damage to the player. Returns true when it brought health to zero.
        public static bool Hurt(GameSession s, int damage)
        {
            s.State.Health = Mathf.Max(0, s.State.Health - Mathf.Max(0, damage));
            s.NotifyChanged();
            return s.State.Health <= 0;
        }

        // Knocked out: lose some gold, wake at the clinic with half health.
        public static void KnockOut(GameSession s)
        {
            var loss = CombatModel.KnockOutGoldLoss(s.State.Gold);
            s.ChangeGold(-loss);
            s.State.Health = CombatModel.KnockOutHealth(s.State.MaxHealth);
            s.State.Mine.Floor = 0;
            s.Toast(L.Get("combat.knocked_out", loss));
            MapTravel.GoTo(MapIds.Clinic, "default");
        }

        public static void Heal(GameSession s, int amount)
        {
            s.State.Health = Mathf.Min(s.State.MaxHealth, s.State.Health + amount);
            s.NotifyChanged();
        }
    }
}
