using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // One-time tips that come when the player first meets the thing (playtest 2026-10-14: nothing told a player what the health bar was or what to do when it ran low).
    // Each shows once, as a notice, and is remembered by a flag; the "tutorial tips" setting turns them all off. Pure decisions, so they can be tested.
    public static class ContextTips
    {
        public const string Prefix = "tip.shown.";
        public const string LowHealth = "low_health", PackFull = "pack_full", Mine = "mine", Bats = "bats";
        public const float LowHealthShare = 0.35f;
        public const float BatSight = 9f;          // how near a bat must be before the tip about bats comes

        // The tip to show now, or null.
        public static string Next(GameSession s, bool batNear)
        {
            if (s == null || !s.InGame) return null;
            if (s.Settings != null && !s.Settings.TutorialTips) return null;
            var state = s.State;
            if (state.Health > 0 && state.Health <= state.MaxHealth * LowHealthShare && !Shown(s, LowHealth)) return LowHealth;
            if (state.CurrentMap == MapIds.Mine && !Shown(s, Mine)) return Mine;
            if (batNear && !Shown(s, Bats)) return Bats;
            if (!s.Backpack.HasEmptySlot() && !Shown(s, PackFull)) return PackFull;
            return null;
        }

        public static bool Shown(GameSession s, string tip) => s.HasFlag(Prefix + tip);

        // Shows the tip that is due, once.
        public static string Tick(GameSession s, bool batNear)
        {
            var tip = Next(s, batNear);
            if (tip == null) return null;
            s.SetFlag(Prefix + tip);
            s.Toast(L.Get("tip." + tip));
            return tip;
        }

        // Whether a living cave bat is within sight of the player on this floor.
        public static bool BatNear(GameSession s)
        {
            var enemies = s?.Game?.Map?.Enemies;
            if (enemies == null) return false;
            var player = enemies.PlayerPosition;
            foreach (var actor in enemies.Enemies)
                if (actor != null && !actor.IsDead && actor.Brain.Row.Flying && ((Vector2)actor.transform.position - player).sqrMagnitude <= BatSight * BatSight) return true;
            return false;
        }
    }
}
