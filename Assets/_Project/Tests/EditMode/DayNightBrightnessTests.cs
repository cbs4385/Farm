using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Owner, 2026-10-09: the outdoor light follows the day, about half as bright at night as at noon.
    public class DayNightBrightnessTests
    {
        static float Level(int hour, int minute = 0) => DayNightLighting.Brightness(hour * 60 + minute);

        [Test]
        public void Noon_IsFull_AndNight_IsAboutHalf()
        {
            Assert.AreEqual(1f, Level(12), 0.001f);
            Assert.AreEqual(0.5f, Level(23), 0.001f);
            Assert.AreEqual(0.5f, Level(26), 0.001f);
            Assert.AreEqual(0.5f, Level(6), 0.001f, "the day starts in the half light");
        }

        [Test]
        public void TheLight_RisesToNoon_AndFallsToNight_WithoutJumps()
        {
            var last = Level(6);
            for (var m = 6 * 60; m <= 12 * 60; m += 10)
            {
                var now = DayNightLighting.Brightness(m);
                Assert.GreaterOrEqual(now, last - 0.0001f, "never darker on the way to noon (" + m + ")");
                Assert.LessOrEqual(now - last, 0.03f, "no jump");
                last = now;
            }
            for (var m = 12 * 60; m <= 22 * 60; m += 10)
            {
                var now = DayNightLighting.Brightness(m);
                Assert.LessOrEqual(now, last + 0.0001f, "never lighter on the way to night (" + m + ")");
                Assert.LessOrEqual(last - now, 0.03f, "no jump");
                last = now;
            }
        }

        [Test]
        public void TheColour_IsTheTintAtThatBrightness()
        {
            var night = DayNightLighting.ColorAt(24 * 60);
            var noon = DayNightLighting.ColorAt(12 * 60);
            Assert.AreEqual(Color.white, noon);
            Assert.AreEqual(0.5f, Mathf.Max(night.r, night.g, night.b), 0.001f, "the strongest channel at night is half");
            Assert.Greater(night.b, night.r, "a cool night");
            for (var m = 6 * 60; m < 30 * 60; m += 15)
            {
                var c = DayNightLighting.ColorAt(m);
                Assert.GreaterOrEqual(Mathf.Max(c.r, c.g, c.b), 0.45f, "never darker than about half at " + m);
            }
        }
    }
}
