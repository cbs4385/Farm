using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-030 in the real scenes: the weather effects follow the day's weather outdoors and stay out of buildings.
    public class WeatherFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-weathertests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static IEnumerator Start(string weather, string map, Action<GameSession> ready)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;   // random clutter would make tile positions unpredictable
            session.State.Weather = weather;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 6; i++) yield return null;
            ready(session);
        }

        [UnityTest]
        public IEnumerator Outdoors_RainShowsParticles_AndClearingTheWeatherRemovesThem()
        {
            GameSession session = null;
            yield return Start("rain", MapIds.Farm, s => session = s);

            var effects = UnityEngine.Object.FindFirstObjectByType<WeatherEffects>();
            Assert.IsNotNull(effects, "outdoor maps get weather effects");
            Assert.AreEqual("rain", effects.Shown.Id);
            Assert.Greater(effects.ActiveParticles, 50);

            session.State.Weather = "sunny";
            yield return null;
            yield return null;
            Assert.AreEqual("sunny", effects.Shown.Id);
            Assert.AreEqual(0, effects.ActiveParticles);

            session.State.Weather = "storm";
            yield return null;
            yield return null;
            Assert.Greater(effects.ActiveParticles, 200, "storms are heavier than rain");
        }

        [UnityTest]
        public IEnumerator ParticlesStayInsideTheView_AndMove()
        {
            yield return Start("snow", MapIds.Farm, _ => { });
            var effects = UnityEngine.Object.FindFirstObjectByType<WeatherEffects>();
            var cam = Camera.main;
            var renderers = effects.GetComponentsInChildren<SpriteRenderer>();

            SpriteRenderer first = null;
            foreach (var r in renderers) if (r.enabled && r.gameObject.name.StartsWith("p")) { first = r; break; }
            Assert.IsNotNull(first);
            var before = first.transform.position;
            yield return new WaitForSeconds(0.3f);
            Assert.AreNotEqual(before, first.transform.position, "snow falls");

            var half = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
            var center = (Vector2)cam.transform.position;
            foreach (var r in renderers)
            {
                if (!r.enabled || !r.gameObject.name.StartsWith("p")) continue;
                var p = (Vector2)r.transform.position - center;
                Assert.LessOrEqual(Mathf.Abs(p.x), half.x + 0.2f);
                Assert.LessOrEqual(Mathf.Abs(p.y), half.y + 0.2f);
            }
        }

        [UnityTest]
        public IEnumerator Indoors_HasNoWeatherEffects()
        {
            yield return Start("rain", MapIds.FarmHouse, _ => { });
            Assert.IsNull(UnityEngine.Object.FindFirstObjectByType<WeatherEffects>());
        }

        [UnityTest]
        public IEnumerator ANewGame_HasAForecast_ThatBecomesTomorrowsWeather()
        {
            GameSession session = null;
            yield return Start("sunny", MapIds.Farm, s => session = s);
            var forecast = session.State.ForecastWeather;
            Assert.IsNotEmpty(forecast);

            var summary = session.EndDay(false);
            Assert.AreEqual(forecast, summary.NewWeather);
            Assert.AreEqual(forecast, session.State.Weather);
        }
    }
}
