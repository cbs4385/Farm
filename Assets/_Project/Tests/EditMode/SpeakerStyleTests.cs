using System.Linq;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class SpeakerStyleTests
    {
        static readonly string[] All = { "wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione" };

        static float Lum(Color c)
        {
            float F(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * F(c.r) + 0.7152f * F(c.g) + 0.0722f * F(c.b);
        }

        [Test]
        public void EveryVillager_HasAnAccent_ReadableOnThePanel()
        {
            var panel = Lum(UiKit.PanelColor);
            foreach (var id in All)
            {
                Assert.IsTrue(SpeakerStyle.Styled.Contains(id), id);
                var ratio = (Lum(SpeakerStyle.AccentFor(id)) + 0.05f) / (panel + 0.05f);
                Assert.GreaterOrEqual(ratio, 4.5f, id);
            }
        }

        [Test]
        public void Accents_AreDistinct_AndUnknownSpeakersFallBack()
        {
            var cols = All.Select(SpeakerStyle.AccentFor).ToList();
            for (var i = 0; i < cols.Count; i++)
                for (var j = i + 1; j < cols.Count; j++)
                    Assert.Greater(Mathf.Abs(cols[i].r - cols[j].r) + Mathf.Abs(cols[i].g - cols[j].g) + Mathf.Abs(cols[i].b - cols[j].b), 0.12f, All[i] + "/" + All[j]);
            Assert.AreEqual(UiKit.Accent, SpeakerStyle.AccentFor("nobody"));
            Assert.AreEqual(UiKit.Accent, SpeakerStyle.AccentFor(null));
        }
    }
}
