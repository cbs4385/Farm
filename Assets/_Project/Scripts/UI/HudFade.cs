using UnityEngine;

namespace Farm.UI
{
    // The HUD panels get out of the player's way (playtest report, 2026-10-04: the player vanished in the upper right of the screen, because the
    // camera stops at the map edge and the opaque clock panel sits there). A panel that overlaps the player fades to a see-through state and
    // comes back when they move clear. Pure, so it can be tested.
    public static class HudFade
    {
        public const float FadedAlpha = 0.22f;
        public const float FadePerSecond = 5f;
        public const float Margin = 10f;          // pixels around the player that also count as "behind the panel"

        public static bool Overlaps(Rect panel, Rect player) =>
            panel.Overlaps(Rect.MinMaxRect(player.xMin - Margin, player.yMin - Margin, player.xMax + Margin, player.yMax + Margin));

        public static float TargetAlpha(Rect panel, Rect player) => Overlaps(panel, player) ? FadedAlpha : 1f;

        public static float Step(float current, float target, float deltaTime) => Mathf.MoveTowards(current, target, FadePerSecond * deltaTime);
    }
}
