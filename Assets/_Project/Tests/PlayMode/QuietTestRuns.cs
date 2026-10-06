using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Every PlayMode test runs with all the audio options at 1%: the sound effects and music of hundreds of tests in a row are aggravating.
    // (The tests that check what plays look at the audio objects, not at what can be heard.)
    [SetUpFixture]
    public class QuietTestRuns
    {
        public const float Volume = 0.01f;

        [OneTimeSetUp]
        public void Quiet()
        {
            GameServices.AudioVolumeOverride = Volume;
            AudioListener.volume = Volume;
        }

        [OneTimeTearDown]
        public void Loud()
        {
            GameServices.AudioVolumeOverride = null;
            AudioListener.volume = 1f;
        }
    }
}
