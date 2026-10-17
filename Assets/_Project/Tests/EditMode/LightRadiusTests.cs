using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Owner, 2026-10-16: "double the radius of the torch light and lamp effects".
    public class LightRadiusTests
    {
        [Test]
        public void TheTorchLightsTenTiles_AndAStreetLampNine_TwiceWhatTheyDid()
        {
            Assert.AreEqual(2f * 5f, TorchLight.Radius);
            Assert.AreEqual(2f * 4.5f, StreetLamp.Radius);
        }
    }
}
