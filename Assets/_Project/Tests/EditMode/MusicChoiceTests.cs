using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The music: which of the six tracks plays where and when, and that every track is really in the game.
    public class MusicChoiceTests
    {
        [Test]
        public void TheMenuAndAnyScreenWithoutAGame_PlayTheTitleTheme()
        {
            Assert.AreEqual(MusicChoice.Title, MusicChoice.For(SceneNames.MainMenu, false, Season.Spring, 12));
            Assert.AreEqual(MusicChoice.Title, MusicChoice.For(SceneNames.MainMenu, true, Season.Spring, 12));
            Assert.AreEqual(MusicChoice.Title, MusicChoice.For(MapIds.Farm, false, Season.Fall, 12), "no game running: the menu music");
        }

        [TestCase(Season.Spring, "farm_spring")]
        [TestCase(Season.Summer, "farm_summer")]
        [TestCase(Season.Fall, "farm_fall")]
        [TestCase(Season.Winter, "farm_winter")]
        public void ByDay_TheFarmThemeOfTheSeasonPlays_OnTheFarmAndInTown(Season season, string cue)
        {
            foreach (var map in new[] { MapIds.Farm, MapIds.FarmHouse, MapIds.Village, MapIds.Library, MapIds.Beach })
                Assert.AreEqual(cue, MusicChoice.For(map, true, season, 12), map);
        }

        [TestCase(19, false)]
        [TestCase(20, true)]
        [TestCase(23, true)]
        [TestCase(26, true)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        public void TheNightTheme_PlaysFromTwentyHundredUntilSix(int hour, bool night) =>
            Assert.AreEqual(night ? MusicChoice.FarmNight : "farm_summer", MusicChoice.For(MapIds.Farm, true, Season.Summer, hour));

        [Test]
        public void TheCavesAndTheWood_AreLeftToTheirAmbience()
        {
            Assert.IsNull(MusicChoice.For(MapIds.Mine, true, Season.Spring, 12));
            Assert.IsNull(MusicChoice.For(MapIds.Woods, true, Season.Spring, 23));
        }

        [TestCase("title")]
        [TestCase("farm_spring")]
        [TestCase("farm_summer")]
        [TestCase("farm_fall")]
        [TestCase("farm_winter")]
        [TestCase("farm_night")]
        public void EveryTrack_IsInTheGame_AsALongPieceOfMusic(string cue)
        {
            var clip = Resources.Load<AudioClip>(MusicDirector.ResourceFolder + "/" + cue);
            Assert.IsNotNull(clip, cue + " is under Resources/Music");
            Assert.Greater(clip.length, 60f, cue + " is a whole piece, not a sting");
            CollectionAssert.Contains(MusicChoice.AllCues, cue);
        }

        [Test]
        public void EveryCueTheChoiceCanReturn_HasATrack()
        {
            foreach (Season season in System.Enum.GetValues(typeof(Season)))
                foreach (var hour in new[] { 3, 12, 22 })
                {
                    var cue = MusicChoice.For(MapIds.Farm, true, season, hour);
                    Assert.IsNotNull(Resources.Load<AudioClip>(MusicDirector.ResourceFolder + "/" + cue), cue);
                }
        }
    }
}
