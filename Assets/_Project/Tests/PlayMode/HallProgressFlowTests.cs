using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-13: "no change to the village or the community center after completing a quest". Each restored room of the hall shows in the hall and in the village.
    public class HallProgressFlowTests : PlayModeFixture
    {
        static readonly string[] Rooms = { "pantry", "crafts", "fishtank", "boiler", "bulletin", "vault" };

        IEnumerator Load(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = GameContext.Current.Session;
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            s.State.CurrentMap = map;
            s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static bool Shown(string holderName)
        {
            var holder = Object.FindObjectsByType<ConditionalObject>().FirstOrDefault(c => c.name == holderName);
            Assert.IsNotNull(holder, holderName + " is in the scene");
            Assert.Greater(holder.transform.childCount, 0, holderName + " holds something");
            return holder.transform.GetChild(0).gameObject.activeSelf;
        }

        [UnityTest]
        public IEnumerator InsideTheHall_EachRestoredRoomAddsItsOwnThings()
        {
            yield return Load(MapIds.CommunityHall);
            var s = GameContext.Current.Session;
            foreach (var room in Rooms) Assert.IsFalse(Shown("HallRestored_" + room), room + " is bare at first");
            foreach (var room in Rooms)
            {
                s.SetFlag("hall." + room);
                yield return null;
                Assert.IsTrue(Shown("HallRestored_" + room), room + " shows once restored");
            }
        }

        [UnityTest]
        public IEnumerator InTheVillage_EachRestoredRoomAddsSomethingToTheFrontOfTheHall()
        {
            yield return Load(MapIds.Village);
            var s = GameContext.Current.Session;
            foreach (var room in Rooms) Assert.IsFalse(Shown("VillageRestored_" + room), room + " has not changed the village yet");
            foreach (var room in Rooms)
            {
                s.SetFlag("hall." + room);
                yield return null;
                Assert.IsTrue(Shown("VillageRestored_" + room), room + " shows in the village once restored");
            }
        }
    }
}
