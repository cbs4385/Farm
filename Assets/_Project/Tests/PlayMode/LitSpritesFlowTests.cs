using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The outside world follows the light (owner, 2026-10-12): a picture made while the game runs got the engine's plain unlit material in a built player (the Editor gave it
    // the lit one, so the sprite's own shader cannot show the fault) and glowed in the dark. What the world makes must be given the shared lit material.
    public class LitSpritesFlowTests : PlayModeFixture
    {
        IEnumerator Load(string map, int mineFloor = 0)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = GameContext.Current.Session;
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            s.State.CurrentMap = map;
            s.State.SpawnPoint = "default";
            if (map == MapIds.Mine) s.State.Mine.Floor = mineFloor;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [Test]
        public void TheSharedMaterial_IsALitSpriteMaterial()
        {
            Assert.IsNotNull(LitSprites.Material);
            Assert.AreEqual(LitSprites.ShaderName, LitSprites.Material.shader.name);
            Assert.AreSame(LitSprites.Material, LitSprites.Material, "one shared material");
        }

        [UnityTest]
        public IEnumerator InTheMine_TheMonstersAndTheStairs_AreGivenTheLitMaterial()
        {
            yield return Load(MapIds.Mine, 1);
            var enemies = Object.FindObjectsByType<EnemyActor>(FindObjectsSortMode.None);
            Assert.Greater(enemies.Length, 0, "there are monsters");
            foreach (var enemy in enemies) Assert.AreSame(LitSprites.Material, enemy.GetComponent<SpriteRenderer>().sharedMaterial, enemy.name);
            var stairs = Object.FindObjectsByType<MineStairs>(FindObjectsSortMode.None);
            Assert.Greater(stairs.Length, 0);
            foreach (var stair in stairs) Assert.AreSame(LitSprites.Material, stair.GetComponent<SpriteRenderer>().sharedMaterial, stair.name);
        }

        [UnityTest]
        public IEnumerator OnTheFarm_TheBuildingsAndTheGrassDecoration_AreGivenTheLitMaterial()
        {
            yield return Load(MapIds.Farm);
            var view = Object.FindAnyObjectByType<FarmBuildingsView>();
            foreach (var picture in view.GetComponentsInChildren<SpriteRenderer>())
                Assert.AreSame(LitSprites.Material, picture.sharedMaterial, picture.name);
            var decor = GameObject.Find("Decor");
            Assert.IsNotNull(decor, "the grass has its tufts and flowers");
            Assert.AreSame(LitSprites.Material, decor.GetComponent<TilemapRenderer>().sharedMaterial);
        }

        [UnityTest]
        public IEnumerator InTheVillage_TheVillagers_AreGivenTheLitMaterial()
        {
            yield return Load(MapIds.Village);
            var npcs = GameContext.Current.Map.Npcs;
            var bram = npcs.Take("bram", new Vector3Int(30, 20, 0));            // the village is empty at six in the morning
            Assert.IsNotNull(bram);
            foreach (var actor in npcs.Actors.Values) Assert.AreSame(LitSprites.Material, actor.GetComponent<SpriteRenderer>().sharedMaterial, actor.name);
        }
    }
}
