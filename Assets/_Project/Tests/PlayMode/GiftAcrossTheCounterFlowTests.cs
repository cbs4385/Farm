using System.Collections;
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
    // Playtest 2026-10-12: a gift for Tilda could not be given while she worked behind her counter.
    public class GiftAcrossTheCounterFlowTests : PlayModeFixture
    {
        [UnityTest]
        public IEnumerator AGift_ReachesAVillagerBehindAShopCounter_WithTheKeyboardOrPad()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = GameContext.Current.Session;
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            s.State.CurrentMap = MapIds.GeneralStore;
            s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.GeneralStore);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var npcs = GameContext.Current.Map.Npcs;
            var villager = npcs.Take("tilda", new Vector3Int(4, 6, 0));          // behind the counter (the counter stands on row 5)
            Assert.IsNotNull(villager);
            var player = Object.FindAnyObjectByType<PlayerController>();
            var actions = Object.FindAnyObjectByType<PlayerActions>();
            player.Teleport(new Vector3(4.5f, 4.5f, 0f));
            player.Face(Vector2Int.up);
            yield return null;

            s.Backpack.Add(ItemIds.Wood, 1);
            var slot = Enumerable.Range(0, s.Backpack.Capacity).First(i => s.Backpack.Get(i)?.ItemId == ItemIds.Wood);
            s.State.SelectedHotbar = slot;
            var before = s.Backpack.Count(ItemIds.Wood);
            actions.UseSelected();
            Assert.AreEqual(before - 1, s.Backpack.Count(ItemIds.Wood), "the gift was given to her across the counter");
            Assert.AreEqual(1, NpcInteractions.StateOf(s.State, "tilda").GiftsToday);
        }
    }
}
