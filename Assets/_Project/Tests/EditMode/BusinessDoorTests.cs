using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-031: what a closed door or counter tells the player.
    public class BusinessDoorTests
    {
        [SetUp]
        public void SetUp()
        {
            L.SetLanguage("en");
            BusinessHoursRegistry.Clear();
            BusinessHoursRegistry.RegisterDefaults();
        }

        [TearDown]
        public void TearDown() => BusinessHoursRegistry.Clear();

        [Test]
        public void HoursAreDescribedWithTheDayOff()
        {
            BusinessHoursRegistry.TryGet("general", out var general);
            Assert.AreEqual("9:00 AM - 5:00 PM, closed Sun", general.Describe());

            BusinessHoursRegistry.TryGet("saloon", out var saloon);
            Assert.AreEqual("12:00 PM - 2:00 AM, closed Tue", saloon.Describe());

            BusinessHoursRegistry.TryGet("merchant", out var merchant);
            Assert.AreEqual("9:00 AM - 9:00 PM", merchant.Describe(), "no day off to mention");
        }

        [Test]
        public void AClosedDoor_NamesTheBusinessAndItsHours()
        {
            Assert.AreEqual("The General Store is closed. Hours: 9:00 AM - 5:00 PM, closed Sun.",
                BusinessHoursRegistry.ClosedMessage("general"));
            Assert.AreEqual("The Fish Stall is closed. Hours: 6:00 AM - 2:00 PM, closed Thu.",
                BusinessHoursRegistry.ClosedMessage("fish"));
        }

        [Test]
        public void ABusinessWithNoRegisteredHours_JustSaysItIsClosed()
        {
            BusinessHoursRegistry.Clear();
            Assert.AreEqual("The General Store is closed.", BusinessHoursRegistry.ClosedMessage("general"));
        }

        [Test]
        public void EveryDefaultBusiness_HasAName()
        {
            foreach (var id in new[] { "general", "blacksmith", "carpenter", "fish", "clinic", "library", "saloon", "merchant" })
                Assert.AreNotEqual("business." + id, L.Get("business." + id), id);
        }

        [Test]
        public void TheLibraryKeepsTheClinicsHours()
        {
            BusinessHoursRegistry.TryGet("clinic", out var clinic);
            BusinessHoursRegistry.TryGet("library", out var library);
            Assert.AreEqual(clinic.OpenMinute, library.OpenMinute);
            Assert.AreEqual(clinic.CloseMinute, library.CloseMinute);
            Assert.AreEqual(clinic.DayOff, library.DayOff);
        }
    }
}
