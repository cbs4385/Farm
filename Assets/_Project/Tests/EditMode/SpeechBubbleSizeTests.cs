using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-09: the text in villagers' speech bubbles was too small to read.
    public class SpeechBubbleSizeTests
    {
        [Test]
        public void TheBubbleText_IsLargerThanItWas_AndFollowsTheTextSizeOption()
        {
            Assert.GreaterOrEqual(SpeechBubbleFactory.SizeFactor(1f), 1.5f);
            Assert.Greater(SpeechBubbleFactory.SizeFactor(1.5f), SpeechBubbleFactory.SizeFactor(1f));
            Assert.Less(SpeechBubbleFactory.SizeFactor(0.75f), SpeechBubbleFactory.SizeFactor(1f));
            Assert.AreEqual(SpeechBubbleFactory.SizeFactor(1.5f), SpeechBubbleFactory.SizeFactor(9f), "capped at the largest option");
        }

        [Test]
        public void ABubble_HasTextAtThatSize_AndAPanelThatFitsIt()
        {
            var actor = new GameObject("Villager");
            try
            {
                var bubble = SpeechBubbleFactory.Create(actor.transform, "Good morning to you, neighbor");
                var mesh = bubble.GetComponentInChildren<TextMesh>();
                Assert.GreaterOrEqual(mesh.characterSize, SpeechBubbleFactory.BaseCharacterSize * 1.4f);
                var panel = bubble.transform.Find("Panel");
                Assert.Greater(panel.localScale.x, 0.17f * 10f, "the panel grew with the text");
            }
            finally { Object.DestroyImmediate(actor); }
        }
    }
}
