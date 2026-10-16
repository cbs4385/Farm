using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest request (2026-10-15): "my pickaxe is far from my sword", "I had to go into the inventory menu to move things around", and a pad player who could not
    // select an item and then a new place on the main screen. The item bar is rearranged on the main screen with the mouse (a drag), the keyboard (V, then a place) and a
    // pad (a quick press of the left stick, then a place), nothing is used while an item is picked up, and the pad walks the backpack grid slot by slot.
    public class HotbarRearrangeFlowTests : InputTestFixture
    {
        const int NodeX = 31, NodeY = 4;
        string _dataRoot;
        Keyboard _keyboard;
        Mouse _mouse;
        Gamepad _pad;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-rearrange-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
            _pad = InputSystem.AddDevice<Gamepad>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static FarmMapView View => UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
        static UiService Ui => (UiService)GameContext.Current.Ui;
        static GameObject Selected => EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        // A new game on the farm with a tree in front of the player, and a backpack holding only the axe (slot 0), the pickaxe (slot 1) and the sword (slot 5).
        IEnumerator Enter(int capacity = 12)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 12 * 60));          // daylight: no torch is needed (an emptied backpack would raise the torch warning window)
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            var pack = _session.Backpack;
            if (pack.Capacity < capacity) pack.Resize(capacity);
            for (var i = 0; i < pack.Capacity; i++) pack.Take(i);
            pack.Put(0, new ItemStack(ItemIds.Axe, 1));
            pack.Put(1, new ItemStack(ItemIds.Pickaxe, 1));
            pack.Put(5, new ItemStack(ItemIds.Sword, 1));
            _session.State.SelectedHotbar = 0;
            var nodes = _session.GetNodes(MapIds.Farm);
            nodes.Add(NodeX, NodeY, _session.Nodes.Get("tree"));
            nodes.TryGet(NodeX, NodeY, out var node);
            node.Hp = 400;
            View.RefreshNode(new Vector3Int(NodeX, NodeY, 0));
            Player.transform.position = new Vector3(NodeX - 1 + 0.5f, NodeY + 0.5f, 0f);
            Player.Face(Vector2Int.right);
            for (var i = 0; i < 4; i++) yield return null;
            _session.State.Energy = _session.State.MaxEnergy;
        }

        int TreeHp { get { _session.GetNodes(MapIds.Farm).TryGet(NodeX, NodeY, out var n); return n?.Hp ?? 0; } }
        string At(int slot) => _session.Backpack.Get(slot)?.ItemId;

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            yield return null;
            Release(button);
            yield return null;
        }

        IEnumerator Wait(float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        // Playtest 2026-10-16: "the buttons to scroll the hot bar for larger backpacks obscure and overlap the last item slot".
        [UnityTest]
        public IEnumerator TheRowButtons_StandPastTheEndOfTheBar_AndNeverOverTheLastSlot()
        {
            yield return Enter(24);
            for (var i = 0; i < 8; i++) yield return null;
            var holder = Resources.FindObjectsOfTypeAll<RectTransform>().First(r => r.name == "RowButtons" && r.gameObject.scene.IsValid());
            Assert.IsTrue(holder.gameObject.activeInHierarchy, "a backpack of two rows shows the buttons");
            var last = holder.parent.Find("Slot" + (InputNames.HotbarSlots - 1)) as RectTransform;
            Assert.IsNotNull(last, "the last slot of the bar");
            var buttons = new Vector3[4];
            var slot = new Vector3[4];
            holder.GetWorldCorners(buttons);
            last.GetWorldCorners(slot);
            Assert.GreaterOrEqual(buttons[0].x, slot[2].x, "the buttons start where the last slot ends");
            foreach (var b in holder.GetComponentsInChildren<Button>(true))
            {
                var corners = new Vector3[4];
                ((RectTransform)b.transform).GetWorldCorners(corners);
                Assert.GreaterOrEqual(corners[0].x, slot[2].x, b.name + " is clear of the last slot");
            }
        }

        // ---- the keyboard ------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Keyboard_PicksUpTheSelectedItem_AndPutsItDownOnTheNextSlot()
        {
            yield return Enter();
            _session.State.SelectedHotbar = 1;
            yield return Tap(_keyboard.vKey);
            Assert.IsTrue(_session.Rearranging, "V picks up the pickaxe");
            yield return Tap(_keyboard.periodKey);
            yield return Tap(_keyboard.periodKey);
            Assert.AreEqual(3, _session.State.SelectedHotbar, "the choosing moves along the bar");
            yield return Tap(_keyboard.eKey);
            Assert.IsFalse(_session.Rearranging);
            Assert.IsNull(At(1));
            Assert.AreEqual(ItemIds.Pickaxe, At(3), "put down on slot 4");
            Assert.AreEqual(3, _session.State.SelectedHotbar, "the pickaxe is still the one in hand");
        }

        [UnityTest]
        public IEnumerator Keyboard_ANumberKey_PutsTheItemDownThere_AndSwapsWithWhatWasThere()
        {
            yield return Enter();
            _session.State.SelectedHotbar = 1;
            yield return Tap(_keyboard.vKey);
            yield return Tap(_keyboard.digit6Key);
            Assert.IsFalse(_session.Rearranging);
            Assert.AreEqual(ItemIds.Pickaxe, At(5), "the number key is the place");
            Assert.AreEqual(ItemIds.Sword, At(1), "the sword took its place");
        }

        [UnityTest]
        public IEnumerator WhileAnItemIsPickedUp_NothingIsUsed_AndEscapeOnlyCancels()
        {
            yield return Enter();
            var hp = TreeHp;
            yield return Tap(_keyboard.vKey);
            Press(_keyboard.xKey);
            yield return null;
            yield return Wait(0.6f);
            Release(_keyboard.xKey);
            yield return Tap(_keyboard.eKey);          // puts it down where it is
            Assert.AreEqual(hp, TreeHp, "no swing while the axe was picked up");
            yield return Tap(_keyboard.vKey);
            yield return Tap(_keyboard.periodKey);
            yield return Tap(_keyboard.escapeKey);
            Assert.IsFalse(_session.Rearranging);
            Assert.IsFalse(Ui.AnyModalOpen, "Escape cancelled the move instead of opening the pause menu");
            Assert.AreEqual(ItemIds.Axe, At(0), "nothing moved");
            Assert.AreEqual(0, _session.State.SelectedHotbar, "the choice is back on the axe");
        }

        [UnityTest]
        public IEnumerator AnEmptySlot_CannotBePickedUp()
        {
            yield return Enter();
            _session.State.SelectedHotbar = 8;
            yield return Tap(_keyboard.vKey);
            Assert.IsFalse(_session.Rearranging);
        }

        // ---- a pad -------------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Pad_AQuickPressOfTheLeftStick_PicksUp_TheShouldersChoose_AndAPutsDown()
        {
            yield return Enter();
            _session.State.SelectedHotbar = 1;
            Press(_pad.leftStickButton);
            yield return null;
            Release(_pad.leftStickButton);
            yield return null;
            yield return null;
            Assert.IsTrue(_session.Rearranging, "a quick press picks up the pickaxe");
            Assert.IsFalse(PhotoMode.Active, "and is not photo mode");
            yield return Tap(_pad.rightShoulder);
            yield return Tap(_pad.rightShoulder);
            yield return Tap(_pad.rightShoulder);
            yield return Tap(_pad.buttonSouth);
            Assert.IsFalse(_session.Rearranging);
            Assert.AreEqual(ItemIds.Pickaxe, At(4));
            Assert.IsNull(At(1));
            Assert.AreEqual(4, _session.State.SelectedHotbar);
        }

        [UnityTest]
        public IEnumerator Pad_B_Cancels_AndNoToolIsUsedWithX()
        {
            yield return Enter();
            var hp = TreeHp;
            yield return Tap(_pad.leftStickButton);
            Assert.IsTrue(_session.Rearranging);
            yield return Tap(_pad.buttonWest);          // X
            yield return Tap(_pad.leftShoulder);
            yield return Tap(_pad.buttonEast);          // B
            Assert.IsFalse(_session.Rearranging);
            Assert.AreEqual(ItemIds.Axe, At(0));
            Assert.AreEqual(0, _session.State.SelectedHotbar);
            yield return Wait(0.3f);
            Assert.AreEqual(hp, TreeHp, "X did not swing the axe");
        }

        [UnityTest]
        public IEnumerator Pad_HoldingTheLeftStick_IsStillPhotoMode_NotARearrange()
        {
            yield return Enter();
            Press(_pad.leftStickButton);
            yield return Wait(0.8f);
            Release(_pad.leftStickButton);
            yield return null;
            Assert.IsFalse(_session.Rearranging);
            Assert.IsTrue(PhotoMode.Active, "held in, the stick takes a photo as before");
        }

        // ---- the mouse ---------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Mouse_DraggingASlotOntoAnother_MovesIt_AndSwapsWithAFullOne()
        {
            yield return Enter();
            yield return null;
            var hud = Ui.Hud;
            Set(_mouse.position, hud.BarSlotCenter(1));
            yield return null;
            Press(_mouse.leftButton);
            yield return null;
            Set(_mouse.position, hud.BarSlotCenter(3));
            yield return null;
            Set(_mouse.position, hud.BarSlotCenter(8));
            yield return null;
            Release(_mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(ItemIds.Pickaxe, At(8), "dragged to slot 9");
            Assert.IsNull(At(1));
            Assert.AreEqual(8, _session.State.SelectedHotbar, "the dragged item is the one in hand");

            Set(_mouse.position, hud.BarSlotCenter(5));
            yield return null;
            Press(_mouse.leftButton);
            yield return null;
            Set(_mouse.position, hud.BarSlotCenter(0));
            yield return null;
            Release(_mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(ItemIds.Sword, At(0), "dropped on the axe: they swap");
            Assert.AreEqual(ItemIds.Axe, At(5));
        }

        [UnityTest]
        public IEnumerator Mouse_AClickOnASlot_StillOnlySelectsIt_AndADragThatEndsOffTheBarChangesNothing()
        {
            yield return Enter();
            var hud = Ui.Hud;
            Set(_mouse.position, hud.BarSlotCenter(5));
            yield return null;
            Press(_mouse.leftButton);
            yield return null;
            Release(_mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(5, _session.State.SelectedHotbar);
            Assert.AreEqual(ItemIds.Sword, At(5));

            Press(_mouse.leftButton);
            yield return null;
            Set(_mouse.position, new Vector2(Screen.width * 0.5f, Screen.height * 0.6f));
            yield return null;
            Release(_mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(ItemIds.Sword, At(5), "released away from the bar: nothing moved");
            Assert.AreEqual(ItemIds.Pickaxe, At(1));
            Assert.AreEqual(ItemIds.Axe, At(0));
        }

        // ---- the pad in the backpack -------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Pad_WalksTheBackpackGrid_SlotBySlot_AndNeverLandsOnAnythingElse()
        {
            yield return Enter(24);
            Ui.ToggleInventory();
            yield return null;
            yield return null;
            Assert.AreEqual("Slot0", Selected?.name, "the window opens on the first slot");
            for (var row = 0; row < 2; row++)
            {
                for (var column = 1; column < 12; column++)
                {
                    yield return Tap(_pad.dpad.right);
                    Assert.AreEqual("Slot" + (row * 12 + column), Selected?.name, $"right along row {row}");
                }
                yield return Tap(_pad.dpad.right);
                Assert.AreEqual("Slot" + (row * 12 + 11), Selected?.name, "the end of a row: the highlight stays");
                if (row == 0)
                {
                    yield return Tap(_pad.dpad.down);
                    Assert.AreEqual("Slot23", Selected?.name, "down is the slot under it");
                    for (var column = 10; column >= 0; column--)
                    {
                        yield return Tap(_pad.dpad.left);
                        Assert.AreEqual("Slot" + (12 + column), Selected?.name, "left along the lower row");
                    }
                }
            }
            yield return Tap(_pad.dpad.up);
            Assert.AreEqual("Slot11", Selected?.name, "up is the slot above it");
            for (var column = 10; column >= 0; column--) yield return Tap(_pad.dpad.left);
            Assert.AreEqual("Slot0", Selected?.name);
            yield return Tap(_pad.dpad.up);
            Assert.AreEqual("Slot0", Selected?.name, "the top: it stays");
            yield return Tap(_pad.dpad.left);
            Assert.AreEqual("Slot0", Selected?.name, "no wrapping round to the other end");
            for (var i = 0; i < 4; i++)
            {
                yield return Tap(_pad.dpad.down);
                Assert.IsNotNull(Selected);
                Assert.IsTrue(Selected.name.StartsWith("Slot") || Selected.name == L.Get("inventory.discard"), "down never leaves the window for the bar's row buttons: " + Selected.name);
            }
            Assert.IsFalse(Selected.name == "RowNext" || Selected.name == "RowPrev");
        }
    }
}
