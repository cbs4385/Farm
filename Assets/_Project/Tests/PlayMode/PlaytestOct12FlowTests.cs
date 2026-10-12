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
    // Playtest 2026-10-12 (version 0.5.0), in the real scenes: the pond over a tree, buildings over the player at their doors, farm buildings that looked finished.
    public class PlaytestOct12FlowTests : PlayModeFixture
    {
        GameSession _s;

        IEnumerator Start(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.State.CurrentMap = map;
            _s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator ThePondWater_IsOnTheGround_AndNotOnTheWallLayerThatIsDrawnOverTheTrees()
        {
            yield return Start(MapIds.Village);
            var map = Object.FindAnyObjectByType<FarmMap>();
            var water = 0;
            foreach (var cell in map.Ground.cellBounds.allPositionsWithin)
            {
                if (!map.IsWater(cell)) continue;
                water++;
                var wall = map.Walls.GetTile(cell);
                Assert.IsNotNull(wall, "still blocks the way: " + cell);
                Assert.IsFalse(WaterShore.IsWaterTile(wall.name), "but is not drawn as water over what stands south of it: " + cell);
            }
            Assert.Greater(water, 10, "the pond is there");
        }

        [UnityTest]
        public IEnumerator ABuilding_SlipsUnderThePlayerAtItsDoor_AndOverThemBehindIt()
        {
            yield return Start(MapIds.Village);
            var player = Object.FindAnyObjectByType<PlayerController>();
            var building = Object.FindObjectsByType<BuildingDepth>().First(d => d.TryGetComponent<SpriteRenderer>(out var r) && r.sprite != null && r.sprite.name.StartsWith("prop_bld_"));
            var x = building.transform.position.x;
            var playerOrder = player.GetComponent<SpriteRenderer>().sortingOrder;

            player.transform.position = new Vector3(x, building.LineY - 0.6f, 0f);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.Less(building.GetComponent<SpriteRenderer>().sortingOrder, playerOrder, "the player at the door is drawn over the building");

            player.transform.position = new Vector3(x, building.LineY + 2f, 0f);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.Greater(building.GetComponent<SpriteRenderer>().sortingOrder, playerOrder, "the building covers a player behind it");
        }

        [UnityTest]
        public IEnumerator TheFarmBuildings_LookDerelict_UntilTheCarpenterHasBuiltThem()
        {
            yield return Start(MapIds.Farm);
            var view = Object.FindAnyObjectByType<FarmBuildingsView>();
            var barn = view.transform.Find("Picture_barn");
            Assert.IsNotNull(barn);
            Assert.AreEqual(DerelictLook.Tint, barn.GetComponent<SpriteRenderer>().color, "faded");
            Assert.IsNotNull(barn.Find("Boards_barn"), "with boards across the door");
            Assert.AreEqual("hover.farm.barn.derelict", barn.GetComponent<HoverNote>().Key);

            _s.SetFlag(FarmBuildings.Barn.UnlockFlag);
            view.Rebuild(_s);
            yield return null;
            barn = view.transform.Find("Picture_barn");
            Assert.AreEqual(Color.white, barn.GetComponent<SpriteRenderer>().color, "built: as new");
            Assert.IsNull(barn.Find("Boards_barn"));
            Assert.AreEqual("hover.farm.barn", barn.GetComponent<HoverNote>().Key);
        }
    }
}
