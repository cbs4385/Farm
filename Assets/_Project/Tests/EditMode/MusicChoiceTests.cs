using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The music: which of the eighteen tracks plays where and when, and that every track is really in the game.
    public class MusicChoiceTests
    {
        static readonly string[] Everywhere =
        {
            MapIds.Farm, MapIds.FarmHouse, MapIds.Village, MapIds.Forest, MapIds.Beach, MapIds.GeneralStore, MapIds.Blacksmith, MapIds.Carpenter, MapIds.Saloon, MapIds.Clinic,
            MapIds.Library, MapIds.Greenhouse, MapIds.Coop, MapIds.Barn, MapIds.CommunityHall, MapIds.Mine, MapIds.Woods, MapIds.HomeTilda, MapIds.HomeBram,
        };

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
        public void ByDay_TheFarmThemeOfTheSeasonPlays_OnTheFarmAndItsBuildings(Season season, string cue)
        {
            foreach (var map in new[] { MapIds.Farm, MapIds.Greenhouse, MapIds.Coop, MapIds.Barn })
                Assert.AreEqual(cue, MusicChoice.For(map, true, season, 12), map);
        }

        [Test]
        public void ByDay_TheVillageTheForestAndTheBeach_HaveTheirOwnThemes()
        {
            Assert.AreEqual("village", MusicChoice.For(MapIds.Village, true, Season.Summer, 12));
            Assert.AreEqual("forest", MusicChoice.For(MapIds.Forest, true, Season.Winter, 12));
            Assert.AreEqual("beach", MusicChoice.For(MapIds.Beach, true, Season.Fall, 12));
        }

        [TestCase(19, false)]
        [TestCase(20, true)]
        [TestCase(23, true)]
        [TestCase(26, true)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        public void TheNightTheme_PlaysOutdoorsFromTwentyHundredUntilSix(int hour, bool night)
        {
            Assert.AreEqual(night ? MusicChoice.FarmNight : "farm_summer", MusicChoice.For(MapIds.Farm, true, Season.Summer, hour));
            Assert.AreEqual(night ? MusicChoice.FarmNight : "village", MusicChoice.For(MapIds.Village, true, Season.Summer, hour));
            Assert.AreEqual(night ? MusicChoice.FarmNight : "forest", MusicChoice.For(MapIds.Forest, true, Season.Summer, hour));
            Assert.AreEqual(night ? MusicChoice.FarmNight : "beach", MusicChoice.For(MapIds.Beach, true, Season.Summer, hour));
        }

        [Test]
        public void Indoors_TheSaloonTheLibraryAndTheRest_HaveTheirOwnThemes_AtAnyHour()
        {
            foreach (var hour in new[] { 7, 12, 21, 25 })
            {
                Assert.AreEqual("saloon", MusicChoice.For(MapIds.Saloon, true, Season.Fall, hour));
                Assert.AreEqual("library", MusicChoice.For(MapIds.Library, true, Season.Fall, hour));
                foreach (var map in new[] { MapIds.FarmHouse, MapIds.GeneralStore, MapIds.Blacksmith, MapIds.Carpenter, MapIds.Clinic, MapIds.CommunityHall, MapIds.HomeTilda, MapIds.HomeBram })
                    Assert.AreEqual("indoors", MusicChoice.For(map, true, Season.Fall, hour), map);
            }
        }

        [Test]
        public void TheMineHasItsTheme_AndHarrowWoodIsLeftToItsAmbience()
        {
            Assert.AreEqual("mine", MusicChoice.For(MapIds.Mine, true, Season.Spring, 12));
            Assert.AreEqual("mine", MusicChoice.For(MapIds.Mine, true, Season.Spring, 23), "underground it is always the same");
            Assert.IsNull(MusicChoice.For(MapIds.Woods, true, Season.Spring, 23));
        }

        [Test]
        public void InTheRain_TheRainThemePlaysOutdoorsByDay_AndOnlyThere()
        {
            foreach (var map in new[] { MapIds.Farm, MapIds.Village, MapIds.Forest, MapIds.Beach })
            {
                Assert.AreEqual("rain", MusicChoice.For(map, true, Season.Spring, 12, rain: true), map);
                Assert.AreEqual(MusicChoice.FarmNight, MusicChoice.For(map, true, Season.Spring, 22, rain: true), map + " at night");
            }
            Assert.AreEqual("indoors", MusicChoice.For(MapIds.GeneralStore, true, Season.Spring, 12, rain: true), "indoors the rain is outside");
            Assert.AreEqual("saloon", MusicChoice.For(MapIds.Saloon, true, Season.Spring, 12, rain: true));
            Assert.AreEqual("farm_spring", MusicChoice.For(MapIds.Greenhouse, true, Season.Spring, 12, rain: true), "the greenhouse keeps the farm theme");
        }

        [TestCase(Season.Spring, "festival_spring")]
        [TestCase(Season.Summer, "festival_summer")]
        [TestCase(Season.Fall, "festival_fall")]
        [TestCase(Season.Winter, "festival_winter")]
        public void OnFestivalDay_TheVillagePlaysTheFestivalTheme_ByDayOnly(Season season, string cue)
        {
            Assert.AreEqual(cue, MusicChoice.For(MapIds.Village, true, season, 12, festivalToday: true));
            Assert.AreEqual(cue, MusicChoice.For(MapIds.Village, true, season, 12, rain: true, festivalToday: true), "a festival is not rained off");
            Assert.AreEqual(MusicChoice.FarmNight, MusicChoice.For(MapIds.Village, true, season, 22, festivalToday: true));
            Assert.AreNotEqual(cue, MusicChoice.For(MapIds.Farm, true, season, 12, festivalToday: true), "the farm is not at the festival");
            Assert.AreEqual("saloon", MusicChoice.For(MapIds.Saloon, true, season, 12, festivalToday: true));
        }

        [Test]
        public void EveryTrack_IsInTheGame_AsALongPieceOfMusic()
        {
            Assert.AreEqual(18, MusicChoice.AllCues.Length);
            foreach (var cue in MusicChoice.AllCues)
            {
                var clip = Resources.Load<AudioClip>(MusicDirector.ResourceFolder + "/" + cue);
                Assert.IsNotNull(clip, cue + " is under Resources/Music");
                Assert.Greater(clip.length, 30f, cue + " is a whole piece, not a sting");
            }
        }

        [Test]
        public void EveryCueTheChoiceCanReturn_IsListed_AndHasATrack()
        {
            foreach (var map in Everywhere)
                foreach (Season season in System.Enum.GetValues(typeof(Season)))
                    foreach (var hour in new[] { 3, 12, 22 })
                        foreach (var rain in new[] { false, true })
                            foreach (var festival in new[] { false, true })
                            {
                                var cue = MusicChoice.For(map, true, season, hour, rain, festival);
                                if (cue == null) { Assert.AreEqual(MapIds.Woods, map); continue; }
                                CollectionAssert.Contains(MusicChoice.AllCues, cue, map);
                                Assert.IsNotNull(Resources.Load<AudioClip>(MusicDirector.ResourceFolder + "/" + cue), cue);
                            }
        }

        [Test]
        public void EveryTrackIsUsedSomewhere()
        {
            var used = new System.Collections.Generic.HashSet<string> { MusicChoice.Title };
            foreach (var map in Everywhere)
                foreach (Season season in System.Enum.GetValues(typeof(Season)))
                    foreach (var hour in new[] { 3, 12, 22 })
                        foreach (var rain in new[] { false, true })
                            foreach (var festival in new[] { false, true })
                            {
                                var cue = MusicChoice.For(map, true, season, hour, rain, festival);
                                if (cue != null) used.Add(cue);
                            }
            CollectionAssert.AreEquivalent(MusicChoice.AllCues, used.ToList(), "no track is left out and none is missing from the list");
        }
    }
}
