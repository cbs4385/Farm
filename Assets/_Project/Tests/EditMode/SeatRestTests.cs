using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class SeatRestTests
    {
        [Test]
        public void An_hour_of_sitting_gives_about_24_energy()
        {
            var carry = 0f;
            Assert.AreEqual(24, SeatRest.Gain(60, ref carry));
        }

        [Test]
        public void The_fraction_is_carried_to_the_next_minute()
        {
            var carry = 0f;
            var total = 0;
            for (var i = 0; i < 5; i++) total += SeatRest.Gain(1, ref carry);
            Assert.AreEqual(2, total);
            Assert.AreEqual(0f, carry, 0.001f);
        }

        [Test]
        public void Negative_time_gives_nothing()
        {
            var carry = 0f;
            Assert.AreEqual(0, SeatRest.Gain(-10, ref carry));
        }
    }
}
