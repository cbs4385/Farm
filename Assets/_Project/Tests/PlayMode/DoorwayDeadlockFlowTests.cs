using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-10: "I'm stuck in a building's doorway, it's closed for the day so I can't go in, but I can't leave". A door is a nook one cell wide: the player
    // stood in it, a villager outside wanted the door cell and waited for the player, and the villager's body blocked the only way out.
    public class DoorwayDeadlockFlowTests : InputTestFixture
    {
        string _root;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-doorway-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        public override void TearDown()
        {
            Time.timeScale = 1f;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator APlayerInAShopDoorway_CanLeave_WhileAVillagerWaitsOutsideForTheDoor()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = MapIds.Village;
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            // Some villager's morning or afternoon in which they walk to a shop's door.
            string who = null; var minute = -1; var door = default(Vector3Int);
            var doors = VillageShops.All.Select(s => new Vector3Int(s.DoorX, s.DoorY, 0)).ToList();
            foreach (var npc in session.Npcs.All)
            {
                var plan = NpcSchedule.PlanFor(npc, session.World);
                for (var m = 360; m < 1300 && minute < 0; m++)
                {
                    var place = NpcSchedule.Where(npc, plan, m);
                    var target = new Vector3Int(place.ToX, place.ToY, 0);
                    if (place.Walking && place.Map == MapIds.Village && doors.Contains(target)) { who = npc.Id; minute = m; door = target; }
                }
                if (minute >= 0) break;
            }
            Assert.GreaterOrEqual(minute, 0, "a villager walks to a shop door");

            var map = Object.FindAnyObjectByType<FarmMap>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            foreach (var warp in Object.FindObjectsByType<Warp>(FindObjectsSortMode.None)) if (map.WorldToCell(warp.transform.position) == door) warp.GetComponent<Collider2D>().enabled = false;          // the shop is closed: its door does not take the player in
            player.Teleport(map.CellCenter(door));
            player.Stop();
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 1, minute - 3));
            Time.timeScale = 3f;
            yield return null;

            // Wait until the villager is next to the doorway and has been held up by the player; then walk out.
            NpcActor actor = null;
            var end = Time.realtimeSinceStartup + 120f;
            var trail = "";
            var frames = 0;
            while (Time.realtimeSinceStartup < end)
            {
                if (player == null) Assert.Fail($"the player was destroyed (map change?) after {frames} frames: {who} walking to {door} from minute {minute}; now minute {session.Clock.Now.MinuteOfDay} on {session.State.CurrentMap}; {trail}");
                if (actor == null) NpcManager.Current.Actors.TryGetValue(who, out actor);
                if (actor != null && frames++ % 60 == 0) trail += $"[{actor.transform.position.x:0.0},{actor.transform.position.y:0.0} w={actor.Waiting}] ";
                if (actor != null && actor.Waiting && (actor.transform.position - player.transform.position).sqrMagnitude < 4f) break;
                yield return null;
            }
            Assert.IsNotNull(actor);
            Assert.IsTrue(actor.Waiting, "the villager is waiting for the player to leave the door cell");

            var pad = InputSystem.AddDevice<Gamepad>();
            Set(pad.leftStick, new Vector2(0f, -1f));
            var left = false;
            end = Time.realtimeSinceStartup + 20f;
            while (!left && Time.realtimeSinceStartup < end)
            {
                left = player.transform.position.y < door.y - 0.6f;
                yield return null;
            }
            Set(pad.leftStick, Vector2.zero);
            Time.timeScale = 1f;
            Assert.IsTrue(left, "the player walked out of the doorway past the waiting villager (at " + player.transform.position + ")");
        }
    }
}
