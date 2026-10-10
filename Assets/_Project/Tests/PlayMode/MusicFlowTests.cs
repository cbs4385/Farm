using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The music in the real game: the director follows the season, the time of day and the place, and asks for nothing it has no file for.
    public class MusicFlowTests : PlayModeFixture
    {
        GameSession _s;

        IEnumerator Enter(string map, Season season, int hour)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.Clock.SetTime(new GameDateTime(1, season, 3, hour * 60));
            _s.State.SetDate(_s.Clock.Now);
            _s.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
        }

        static IEnumerator WaitForMusic(string cue)
        {
            var end = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < end && !(MusicDirector.Current != null && MusicDirector.Current.Playing == cue)) yield return null;
        }

        [UnityTest]
        public IEnumerator OnTheFarmBySpringDay_TheSpringThemePlays_AndAtNightTheNightTheme()
        {
            yield return Enter(MapIds.Farm, Season.Spring, 12);
            Assert.IsNotNull(MusicDirector.Current, "the game has a music director");
            yield return WaitForMusic("farm_spring");
            Assert.AreEqual("farm_spring", MusicDirector.Current.Playing);
            var source = MusicDirector.Current.GetComponents<AudioSource>().First(a => a.clip != null);
            Assert.AreEqual("farm_spring", source.clip.name);
            Assert.IsTrue(source.loop, "it loops");

            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 22 * 60));
            yield return WaitForMusic("farm_night");
            Assert.AreEqual("farm_night", MusicDirector.Current.Playing);
        }

        [UnityTest]
        public IEnumerator InTheMine_TheMineThemePlays()
        {
            yield return Enter(MapIds.Farm, Season.Summer, 12);
            yield return WaitForMusic("farm_summer");
            _s.State.CurrentMap = MapIds.Mine;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            yield return WaitForMusic("mine");
            Assert.AreEqual("mine", MusicDirector.Current.Playing);
        }

        [UnityTest]
        public IEnumerator EachPlaceHasItsOwnTheme_TheVillage_TheSaloon_TheLibrary_TheShops()
        {
            yield return Enter(MapIds.Village, Season.Summer, 12);
            yield return WaitForMusic("village");
            Assert.AreEqual("village", MusicDirector.Current.Playing);
            foreach (var (map, cue) in new[] { (MapIds.Saloon, "saloon"), (MapIds.Library, "library"), (MapIds.GeneralStore, "indoors"), (MapIds.Forest, "forest"), (MapIds.Beach, "beach") })
            {
                _s.State.CurrentMap = map;
                var op = SceneManager.LoadSceneAsync(map);
                while (!op.isDone) yield return null;
                yield return WaitForMusic(cue);
                Assert.AreEqual(cue, MusicDirector.Current.Playing, map);
            }
        }

        [UnityTest]
        public IEnumerator InTheRainOnTheFarm_TheRainThemePlays_AndTheDryDayTheFarmTheme()
        {
            yield return Enter(MapIds.Farm, Season.Spring, 12);
            yield return WaitForMusic("farm_spring");
            _s.State.Weather = WeatherDefaults.Rain;
            yield return WaitForMusic("rain");
            Assert.AreEqual("rain", MusicDirector.Current.Playing);
            _s.State.Weather = WeatherDefaults.Sunny;
            yield return WaitForMusic("farm_spring");
            Assert.AreEqual("farm_spring", MusicDirector.Current.Playing);
        }

        [UnityTest]
        public IEnumerator ASceneAskingForATrackThatDoesNotExist_DoesNotSilenceTheGame()
        {
            yield return Enter(MapIds.Farm, Season.Fall, 12);
            yield return WaitForMusic("farm_fall");
            _s.Publish(new MusicCue("ending_sealed"));          // no such file yet
            yield return new WaitForSeconds(1f);
            Assert.AreEqual("farm_fall", MusicDirector.Current.Playing, "the usual music carries on");
        }
    }
}
