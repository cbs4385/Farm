using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The farmer creator in the real game (playtest request, 2026-10-05): the chosen look reaches the player in the world, the creator's controls
    // change the right things, and a new game started from the new-game screen keeps what was chosen.
    public class AvatarFlowTests : PlayModeFixture
    {

        [SetUp]
        public void SetUpMore()
        {
            AvatarSprites.ClearCache();
        }

        static Button ButtonIn(GameObject root, string name)
        {
            var b = root.GetComponentsInChildren<Button>(true).FirstOrDefault(x => x.name == name);
            Assert.IsNotNull(b, name + " is on the screen");
            return b;
        }

        static void Click(GameObject root, string name)
        {
            var b = ButtonIn(root, name);
            Assert.IsTrue(b.interactable, name + " can be used");
            b.onClick.Invoke();
        }

        static IEnumerator WaitForPlayer()
        {
            var end = Time.realtimeSinceStartup + 40f;
            while (UnityEngine.Object.FindAnyObjectByType<PlayerController>() == null && Time.realtimeSinceStartup < end) yield return null;
            for (var i = 0; i < 15; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheFarmer_WearsTheChosenLook_InTheWorld()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            var look = AvatarOptions.Presets[5].look;           // hearth: curly hair, vest, glasses
            session.BeginNewGame("Tester", "Test Farm", 0, look);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            yield return WaitForPlayer();

            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var sprite = player.GetComponentInChildren<SpriteRenderer>().sprite;
            Assert.IsTrue(sprite.name.StartsWith("avatar_"), "the player is drawn from the avatar layers, not the placeholder: " + sprite.name);
            // The pixels on screen are the ones the composer makes for that look.
            var expected = AvatarComposer.Compose(look, AvatarComposer.DownFacing);
            Assert.IsTrue(CharacterFrames.TryGetBase(sprite, out var plain), "what is shown is a frame of the farmer's own picture: " + sprite.name);
            var actual = plain.texture.GetPixels32();
            for (var y = 0; y < 32; y++)
                for (var x = 0; x < 16; x++)
                    Assert.AreEqual(expected[y * 16 + x], actual[(31 - y) * 16 + x], $"pixel {x},{y}");
            Assert.AreEqual(session.State.Avatar.Key, look.Key, "and the game state keeps it");

            // Turning around shows the other sprites of the same look.
            player.Face(Vector2Int.up);
            Assert.AreEqual("avatar_up", player.GetComponentInChildren<SpriteRenderer>().sprite.name);
            player.Face(Vector2Int.right);
            Assert.AreEqual("avatar_right", player.GetComponentInChildren<SpriteRenderer>().sprite.name);
        }

        [UnityTest]
        public IEnumerator TheCreator_ChangesStylesColoursAndPresets_AndReturnsTheLook()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var ui = ServiceLocator.Get<UiService>();
            var creator = new AvatarScreen(ui);
            AvatarData result = null;
            creator.OpenWith(AvatarOptions.Default(), look => result = look);
            yield return null;
            var root = creator.Root;
            Assert.IsTrue(creator.IsOpen);

            Click(root, "Preset_orchard");
            Assert.AreEqual("masculine", creator.Look.Build);
            Assert.AreEqual("overalls", creator.Look.Shirt);

            Click(root, "Build_feminine");
            Assert.AreEqual("feminine", creator.Look.Build);
            var hair = creator.Look.Hair;
            Click(root, "hair_next");
            Assert.AreNotEqual(hair, creator.Look.Hair, "the next hair style");
            Click(root, "hair_prev");
            Assert.AreEqual(hair, creator.Look.Hair, "and back again");
            for (var i = 0; i < AvatarOptions.Shirts.Length; i++) Click(root, "shirt_next");
            Assert.AreEqual("overalls", creator.Look.Shirt, "stepping past the last style wraps around");
            Click(root, "pants_next");
            Click(root, "accessory_next");

            Click(root, "Skin_8d5524"); Assert.AreEqual("#8d5524", creator.Look.Skin);
            Click(root, "hair_" + AvatarOptions.HairColors[10].Substring(1)); Assert.AreEqual(AvatarOptions.HairColors[10], creator.Look.HairColor);
            Click(root, "shirt_" + AvatarOptions.ShirtColors[1].Substring(1)); Assert.AreEqual(AvatarOptions.ShirtColors[1], creator.Look.ShirtColor);
            Click(root, "pants_" + AvatarOptions.PantsColors[2].Substring(1)); Assert.AreEqual(AvatarOptions.PantsColors[2], creator.Look.PantsColor);

            // An accessory that is not worn has no colour to choose.
            while (creator.Look.Accessory != "none") Click(root, "accessory_next");
            Assert.IsFalse(ButtonIn(root, "accessory_" + AvatarOptions.AccessoryColors[0].Substring(1)).interactable, "no colour for no accessory");
            Click(root, "accessory_next");
            Assert.IsTrue(ButtonIn(root, "accessory_" + AvatarOptions.AccessoryColors[0].Substring(1)).interactable);

            var turned = creator.Facing;
            Click(root, "Turn");
            Assert.AreNotEqual(turned, creator.Facing);
            var before = creator.Look.Key;
            Click(root, "Randomize");
            Assert.AreNotEqual(before, creator.Look.Key, "a surprise look");

            var chosen = creator.Look.Key;
            Click(root, "Done");
            Assert.IsFalse(creator.IsOpen);
            Assert.IsNotNull(result);
            Assert.AreEqual(chosen, result.Key, "Done hands back the look on screen");
            Assert.IsTrue(result.SameAs(AvatarOptions.Sanitize(result)), "and it is a valid look");
        }

        [UnityTest]
        public IEnumerator EachTimeTheNewGameScreenOpens_TheFarmerIsRandomised()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var newGame = new NewGameScreen(ServiceLocator.Get<UiService>(), null);
            var keys = new System.Collections.Generic.HashSet<string>();
            for (var i = 0; i < 6; i++)
            {
                newGame.Open();
                yield return null;
                keys.Add(newGame.Avatar.Key);
                Assert.IsFalse(newGame.Avatar.SameAs(AvatarOptions.Default()) && i > 0 && keys.Count == 1, "not stuck on the starting look");
            }
            Assert.Greater(keys.Count, 3, "a different farmer most times it opens");
        }

        [UnityTest]
        public IEnumerator FromTheNewGameScreen_ACustomisedFarmerStartsTheGame()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var ui = ServiceLocator.Get<UiService>();
            var newGame = new NewGameScreen(ui, null);
            newGame.Open();
            yield return null;
            Assert.IsTrue(newGame.Avatar.SameAs(AvatarOptions.Sanitize(newGame.Avatar)), "a random valid look is already chosen");

            Click(newGame.Root, "Customize");
            yield return null;
            var creatorRoot = newGame.Root.transform.parent.GetComponentsInChildren<Transform>(true).First(t => t.name == "Avatar").gameObject;
            Assert.IsTrue(creatorRoot.activeInHierarchy, "the creator opens on top");
            Click(creatorRoot, "Preset_lantern");
            Click(creatorRoot, "Skin_4a2a14");
            Click(creatorRoot, "Done");
            yield return null;
            Assert.AreEqual("lantern", AvatarOptions.Presets.First(p => p.look.Build == newGame.Avatar.Build && p.look.Hair == newGame.Avatar.Hair).id);
            Assert.AreEqual("#4a2a14", newGame.Avatar.Skin);
            var chosen = newGame.Avatar.Key;

            // The first slot starts the game (the names are already filled in).
            var slots = newGame.Root.GetComponentsInChildren<Transform>(true).First(t => t.name == "Slots");
            slots.GetComponentInChildren<Button>().onClick.Invoke();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            Assert.IsTrue(session.InGame, "the game began");
            Assert.AreEqual(chosen, session.State.Avatar.Key, "with the farmer that was chosen");
            yield return WaitForPlayer();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            Assert.IsNotNull(player);
            Assert.IsTrue(player.GetComponentInChildren<SpriteRenderer>().sprite.name.StartsWith("avatar_"));
        }
    }
}
