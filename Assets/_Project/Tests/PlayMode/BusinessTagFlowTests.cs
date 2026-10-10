using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest comment (2026-10-05): "there is no indication that a village business is closed". Each business has a tag by its door that
    // turns red when it is closed, and its door says so when the mouse rests on it.
    public class BusinessTagFlowTests : PlayModeFixture
    {

        [UnityTest]
        public IEnumerator TheTagBesideEachDoor_FollowsTheBusinessHours()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 10 * 60));      // a Wednesday morning: the general store is open
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var tags = UnityEngine.Object.FindObjectsByType<BusinessStatusSign>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(tags.Length, 6, "a tag for each business with hours");
            var general = tags.First(t => t.name.StartsWith(MapIds.GeneralStore));
            var renderer = general.GetComponent<SpriteRenderer>();
            var end = Time.realtimeSinceStartup + 3f;
            while (!general.IsOpenNow && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(general.IsOpenNow);
            StringAssert.StartsWith("bld_tag_open", renderer.sprite.name);

            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 20 * 60));      // after hours
            end = Time.realtimeSinceStartup + 3f;
            while (general.IsOpenNow && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsFalse(general.IsOpenNow);
            StringAssert.StartsWith("bld_tag_closed", renderer.sprite.name);
            StringAssert.Contains("closed", general.HoverLabel.ToLowerInvariant().Replace("closes", "closed"));

            var door = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.HoverLabel != null && w.HoverLabel.Contains("(closed)"));
            Assert.IsNotNull(door, "a closed door says so under the mouse");
        }
    }
}
