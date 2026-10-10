using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Ambience in the real game: a sunny morning on the farm has birds, rain has rain, and a house interior is quiet.
    public class AmbienceFlowTests : PlayModeFixture
    {
        GameSession _session;

        [SetUp]
        public void SetUpMore()
        {
            AmbienceDirector.ResetForTests();
        }

        IEnumerator Enter(string map, string weather, int hour)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.State.Weather = weather;
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, hour * 60));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator ASunnyMorningOutside_HasBirds_AndTheServiceFadesThemIn()
        {
            yield return Enter(MapIds.Farm, WeatherIds.Sunny, 9);
            Assert.AreEqual(AmbienceKind.Birds, AmbienceDirector.Last);
            var audio = ServiceLocator.Get<AudioService>();
            var end = Time.realtimeSinceStartup + 10f;
            while (audio.AmbiencePlaying != AmbienceKind.Birds && Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(AmbienceKind.Birds, audio.AmbiencePlaying);
        }

        [UnityTest]
        public IEnumerator Rain_HasRain()
        {
            yield return Enter(MapIds.Farm, WeatherIds.Rain, 12);
            Assert.AreEqual(AmbienceKind.Rain, AmbienceDirector.Last);
            Assert.AreEqual(AmbienceKind.Rain, ServiceLocator.Get<AudioService>().AmbienceWanted);
        }

        [UnityTest]
        public IEnumerator AnInterior_IsQuiet()
        {
            yield return Enter(MapIds.Saloon, WeatherIds.Rain, 14);
            Assert.AreEqual(AmbienceKind.None, AmbienceDirector.Last);
        }
    }
}
