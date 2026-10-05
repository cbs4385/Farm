using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The HUD gets out of the player's way (playtest report: the player vanished behind the clock panel in the upper right).
    public class HudFadeTests
    {
        static readonly Rect Panel = new Rect(1000, 600, 190, 136);

        [Test]
        public void APlayerBehindThePanel_FadesIt_AndOneClearOfItDoesNot()
        {
            Assert.AreEqual(HudFade.FadedAlpha, HudFade.TargetAlpha(Panel, new Rect(1050, 650, 32, 64)));
            Assert.AreEqual(1f, HudFade.TargetAlpha(Panel, new Rect(300, 300, 32, 64)));
        }

        [Test]
        public void TheMarginCounts_SoThePlayerDoesNotPokeOutFromUnderTheEdge()
        {
            var justOutside = new Rect(Panel.xMin - 32 - HudFade.Margin + 2f, 650, 32, 64);      // inside the margin
            Assert.AreEqual(HudFade.FadedAlpha, HudFade.TargetAlpha(Panel, justOutside));
            var wellOutside = new Rect(Panel.xMin - 32 - HudFade.Margin - 20f, 650, 32, 64);
            Assert.AreEqual(1f, HudFade.TargetAlpha(Panel, wellOutside));
        }

        [Test]
        public void TheFade_IsSmooth_AndReachesItsTarget()
        {
            var a = 1f;
            a = HudFade.Step(a, HudFade.FadedAlpha, 0.016f);
            Assert.Less(a, 1f);
            Assert.Greater(a, HudFade.FadedAlpha, "one frame does not snap");
            for (var i = 0; i < 120; i++) a = HudFade.Step(a, HudFade.FadedAlpha, 0.016f);
            Assert.AreEqual(HudFade.FadedAlpha, a, 0.0001f);
            for (var i = 0; i < 120; i++) a = HudFade.Step(a, 1f, 0.016f);
            Assert.AreEqual(1f, a, 0.0001f);
        }
    }
}
