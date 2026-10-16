using System.Collections;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-15: "reloading the cloud save did not remember where the player was; it loaded at the initial entry point of the map".
    public class ResumePositionFlowTests : PlayModeFixture
    {
        GameSession _s;

        IEnumerator StartFarm()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.State.CurrentMap = MapIds.Farm;
            _s.State.SpawnPoint = "default";
            yield return LoadMap(MapIds.Farm);
        }

        IEnumerator LoadMap(string map)
        {
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static Vector3 PlayerAt() => GameContext.Current.Map.Player.transform.position;

        [UnityTest]
        public IEnumerator ALoadedGame_StartsWhereThePlayerWasSaved_NotAtTheDoor()
        {
            yield return StartFarm();
            var door = PlayerAt();
            var map = GameContext.Current.Map.Map;
            var cell = map.WorldToCell(door) + new Vector3Int(3, 0, 0);
            var spot = map.CellCenter(cell);
            GameContext.Current.Map.Player.transform.position = spot;
            yield return null;

            Assert.IsTrue(_s.Save());
            Assert.IsNull(_s.State.ResumeMap, "the spot is written to the file only, never left in the running game");

            Assert.IsTrue(_s.BeginLoad(0, out var error), error);
            Assert.AreEqual(MapIds.Farm, _s.State.ResumeMap, "the file had it");
            yield return LoadMap(_s.State.CurrentMap);

            Assert.AreEqual(spot.x, PlayerAt().x, 0.05f, "x: not at the door");
            Assert.AreEqual(spot.y, PlayerAt().y, 0.05f, "y: not at the door");
            Assert.AreNotEqual(door.x, PlayerAt().x, "it really is a different place from the door");
            Assert.IsNull(_s.State.ResumeMap, "taken once");
        }

        [UnityTest]
        public IEnumerator AGameLoadedFromASaveWithoutASpot_StillStartsAtTheDoor()
        {
            yield return StartFarm();
            var door = PlayerAt();
            Assert.IsTrue(_s.Save());
            Assert.IsTrue(_s.BeginLoad(0, out var error), error);
            ResumePosition.Clear(_s.State);                                           // as a save made before this feature
            yield return LoadMap(_s.State.CurrentMap);
            Assert.AreEqual(door.x, PlayerAt().x, 0.05f);
            Assert.AreEqual(door.y, PlayerAt().y, 0.05f);
        }
    }
}
