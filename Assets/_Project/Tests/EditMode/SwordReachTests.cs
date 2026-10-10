using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-10 (the mines): "using the sword was not consistent: I was hit by adjacent slimes but could not hit the slime while targeting its tile".
    // Player and enemies stand at cell centres; the target tile is one of the eight around the player.
    public class SwordReachTests
    {
        static readonly Vector2 Player = new Vector2(10.5f, 10.5f);

        static bool Hits(Vector2 target, Vector2 enemy, bool boss = false) =>
            CombatModel.SwingHits(Player, target - Player, target, enemy, boss);

        [Test]
        public void An_enemy_in_the_tile_aimed_at_is_hit_for_all_eight_tiles()
        {
            for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var tile = Player + new Vector2(dx, dy);
                    Assert.IsTrue(Hits(tile, tile), $"the tile {dx},{dy}");
                    Assert.IsTrue(Hits(tile, tile + new Vector2(0.4f, -0.4f)), $"the tile {dx},{dy}, the enemy off its middle");
                    Assert.IsTrue(Hits(tile, tile + new Vector2(-0.45f, 0.45f)), $"the tile {dx},{dy}, the enemy off its middle the other way");
                }
        }

        [Test]
        public void Whatever_can_hit_the_player_can_be_hit_whichever_way_the_player_faces()
        {
            foreach (var angle in new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f })
            {
                var enemy = Player + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * EnemyBrain.AttackRange;
                Assert.IsTrue(Hits(Player + Vector2.up, enemy), $"an enemy {angle} degrees round, in attack range, with the player aiming up");
            }
        }

        [Test]
        public void An_enemy_far_behind_or_far_off_is_not_hit()
        {
            Assert.IsFalse(Hits(Player + Vector2.up, Player + new Vector2(0f, -1.5f)), "a cell and a half behind");
            Assert.IsFalse(Hits(Player + Vector2.up, Player + new Vector2(0f, 3f)), "three cells ahead");
            Assert.IsFalse(Hits(Player + Vector2.right, Player + new Vector2(-2f, 0.5f)), "two cells behind");
        }

        [Test]
        public void The_far_corner_of_the_tile_in_front_is_within_reach_and_a_boss_reaches_further()
        {
            Assert.GreaterOrEqual(CombatModel.SwingReach, 1.5f);
            Assert.IsTrue(Hits(Player + Vector2.up, Player + new Vector2(0.5f, 1.5f)), "the corner of the tile in front");
            Assert.IsFalse(Hits(Player + Vector2.up, Player + new Vector2(0f, 2.4f)));
            Assert.IsTrue(Hits(Player + Vector2.up, Player + new Vector2(0f, 2.4f), boss: true));
        }
    }
}
