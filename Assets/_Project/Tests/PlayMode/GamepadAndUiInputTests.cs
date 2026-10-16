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
    // Drives the game with a simulated gamepad (and keyboard/mouse where noted), standing in for hardware. Covers the
    // T-010 acceptance criteria: gamepad and keyboard both drive every action, and rebinding persists.
    public class GamepadAndUiInputTests : InputTestFixture
    {
        string _dataRoot;
        Gamepad _pad;
        Keyboard _keyboard;
        Mouse _mouse;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-padtests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _pad = InputSystem.AddDevice<Gamepad>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
        }

        IEnumerator StartFarm(Action<GameSession> afterLoad = null)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;   // random clutter would make tile positions unpredictable
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            afterLoad?.Invoke(session);
        }

        // ---- gameplay with a gamepad ----------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator LeftStick_WalksThePlayer_AndFacesTheDirection()
        {
            yield return StartFarm();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.down);
            yield return null;
            var startX = player.transform.position.x;

            Set(_pad.leftStick, new Vector2(1f, 0f));
            yield return new WaitForSeconds(0.4f);
            Set(_pad.leftStick, Vector2.zero);
            yield return null;

            Assert.Greater(player.transform.position.x, startX + 0.8f, "stick right walks right");
            Assert.AreEqual(Vector2Int.right, player.Facing);

            Set(_pad.leftStick, new Vector2(0f, 1f));
            yield return new WaitForSeconds(0.2f);
            Set(_pad.leftStick, Vector2.zero);
            yield return null;
            Assert.AreEqual(Vector2Int.up, player.Facing);
        }

        [UnityTest]
        public IEnumerator DPad_AlsoWalks()
        {
            yield return StartFarm();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            yield return null;
            var startX = player.transform.position.x;
            Press(_pad.dpad.left);
            yield return new WaitForSeconds(0.3f);
            Release(_pad.dpad.left);
            yield return null;
            Assert.Less(player.transform.position.x, startX - 0.5f);
            Assert.AreEqual(Vector2Int.left, player.Facing);
        }

        [UnityTest]
        public IEnumerator Shoulders_CycleTheHotbar_BothWays()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            Assert.AreEqual(0, s.State.SelectedHotbar);
            yield return Tap(_pad.rightShoulder);
            Assert.AreEqual(1, s.State.SelectedHotbar);
            yield return Tap(_pad.rightShoulder);
            Assert.AreEqual(2, s.State.SelectedHotbar);
            yield return Tap(_pad.leftShoulder);
            Assert.AreEqual(1, s.State.SelectedHotbar);
            yield return Tap(_pad.leftShoulder);
            yield return Tap(_pad.leftShoulder);
            Assert.AreEqual(InputNames.HotbarSlots - 1, s.State.SelectedHotbar, "wraps backwards");
        }

        [UnityTest]
        public IEnumerator WestButtonUsesTheTool_AndSouthButtonPlantsSeeds()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.right);
            yield return null;
            var grid = s.GetGrid(MapIds.Farm);

            yield return Tap(_pad.buttonWest);       // hoe is selected: till the tile in front
            Assert.IsTrue(grid.IsTilled(21, 10));

            s.State.SelectedHotbar = 5;               // seeds
            var before = s.Backpack.Count(ItemIds.Seed("parsnip"));
            yield return Tap(_pad.buttonSouth);       // interact plants the selected seeds
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsNotNull(tile.Crop);
            Assert.AreEqual(before - 1, s.Backpack.Count(ItemIds.Seed("parsnip")));
        }

        [UnityTest]
        public IEnumerator NorthButtonOpensTheBackpack_EastClosesIt()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            yield return Tap(_pad.buttonNorth);
            Assert.IsTrue(ui.AnyModalOpen, "Y opens the backpack");
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(ui.AnyModalOpen, "B closes it");
        }

        [UnityTest]
        public IEnumerator StartOpensPause_AndTheDPadAndSouthButtonOperateTheMenu()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            var ui = ((UiService)GameContext.Current.Ui);
            var input = GameContext.Current.Input;

            yield return Tap(_pad.startButton);
            Assert.IsTrue(ui.AnyModalOpen, "Start opens the pause menu");
            Assert.IsTrue(input.GameplayBlocked);

            // Something is focused for gamepad navigation as soon as a menu opens.
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject, "menus must come up with a selection");
            var first = EventSystem.current.currentSelectedGameObject;

            Press(_pad.dpad.down);
            yield return null;
            yield return null;
            Release(_pad.dpad.down);
            yield return null;
            Assert.AreNotSame(first, EventSystem.current.currentSelectedGameObject, "D-pad down moves the selection");

            // Go back to the first entry (Resume) and press A.
            Press(_pad.dpad.up);
            yield return null;
            yield return null;
            Release(_pad.dpad.up);
            yield return null;
            Assert.AreSame(first, EventSystem.current.currentSelectedGameObject);
            yield return Tap(_pad.buttonSouth);
            Assert.IsFalse(ui.AnyModalOpen, "A on Resume closes the pause menu");
            Assert.IsFalse(input.GameplayBlocked);
        }

        [UnityTest]
        public IEnumerator EveryGameplayActionIsReachableFromBothDevices()
        {
            // Not a hardware test: asserts the keyboard AND gamepad each have a working binding for every action.
            yield return StartFarm();
            var input = GameContext.Current.Input;
            foreach (var action in input.Gameplay.actions)
            {
                var controls = action.controls.Select(c => c.device).Distinct().ToList();
                Assert.IsTrue(controls.OfType<Keyboard>().Any() || controls.OfType<Mouse>().Any(), $"{action.name}: keyboard/mouse");
                if (action.name.StartsWith(InputNames.HotbarPrefix) && action.name != InputNames.HotbarNext && action.name != InputNames.HotbarPrev) continue;
                if (action.name == InputNames.RowPrev || action.name == InputNames.Crafting) continue;                       // a pad has one row button: RT goes round; crafting opens from the menu tab (LT is Run)
                Assert.IsTrue(controls.OfType<Gamepad>().Any(), $"{action.name}: gamepad");
            }
        }

        // Playtest 2026-10-11: a tester's controller buttons were not the ones the game named. Every button can be moved from Options, on any pad.
        [UnityTest]
        public IEnumerator ABusyPadButton_CanBeMovedToTheRightTrigger_ThroughOptions_AndTheGameSaysSo()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            var input = GameContext.Current.Input;
            var settings = GameContext.Current.Settings;
            ui.ShowOptions();
            ui.Options.Expand("pad");
            yield return null;
            yield return null;

            var row = UnityEngine.Object.FindObjectsByType<RectTransform>().First(r => r.gameObject.activeInHierarchy && r.name == L.Get("options.pad_row", L.Get("action.UseTool")));
            var button = row.GetComponentInChildren<Button>();
            StringAssert.Contains("X", button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
            button.onClick.Invoke();
            yield return null;
            yield return null;
            StringAssert.Contains(L.Get("options.press_pad"), button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);

            Press(_pad.rightTrigger);
            yield return null;
            yield return null;
            Release(_pad.rightTrigger);
            yield return null;

            var bound = input.UseTool.bindings.First(b => b.path.StartsWith("<Gamepad>"));
            Assert.AreEqual("<Gamepad>/rightTrigger", bound.effectivePath);
            Assert.AreEqual("RT", button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
            StringAssert.Contains("rightTrigger", settings.Current.BindingOverridesJson, "it is saved with the settings");
            Assert.AreEqual("press RT", ControlPrompts.Expand("[[UseTool]]", InputKind.Gamepad));
        }

        [UnityTest]
        public IEnumerator TheKeyboard_CancelsAPadRebind_LeavingTheButtonAlone()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            var input = GameContext.Current.Input;
            ui.ShowOptions();
            ui.Options.Expand("pad");
            yield return null;
            yield return null;
            var row = UnityEngine.Object.FindObjectsByType<RectTransform>().First(r => r.gameObject.activeInHierarchy && r.name == L.Get("options.pad_row", L.Get("action.Rotate")));
            row.GetComponentInChildren<Button>().onClick.Invoke();
            yield return null;
            Press(_keyboard.escapeKey);
            yield return null;
            yield return null;
            Release(_keyboard.escapeKey);
            yield return null;
            Assert.AreEqual("<Gamepad>/buttonEast", input.Rotate.bindings.First(b => b.path.StartsWith("<Gamepad>")).effectivePath);
        }

        [UnityTest]
        public IEnumerator TheOptionsScreen_ListsEveryKey()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ShowOptions();
            ui.Options.Expand("keys");
            yield return null;
            yield return null;
            var names = UnityEngine.Object.FindObjectsByType<Button>().Where(b => b.gameObject.activeInHierarchy).Select(b => b.name).ToList();
            var keyActions = names.Where(n => n.StartsWith(OptionsScreen.KeyPrefix)).Select(n => n.Split('_')[1]).ToList();
            foreach (var action in InputNames.Rebindable) CollectionAssert.Contains(keyActions, action, action);
            for (var i = 1; i <= InputNames.HotbarSlots; i++) CollectionAssert.Contains(keyActions, InputNames.HotbarPrefix + i);
            Assert.AreEqual(4, names.Count(n => n.StartsWith(OptionsScreen.KeyPrefix + "Move_")), "the four walking keys");

            ui.Options.Expand("pad");
            yield return null;
            yield return null;
            names = UnityEngine.Object.FindObjectsByType<Button>().Where(b => b.gameObject.activeInHierarchy).Select(b => b.name).ToList();
            Assert.GreaterOrEqual(names.Count(n => n.StartsWith(OptionsScreen.PadPrefix)), 13, "and a controller button for each action that has one, on the controller page");
        }

        static System.Collections.Generic.List<Button> ActiveButtons() => UnityEngine.Object.FindObjectsByType<Button>().Where(b => b.gameObject.activeInHierarchy).ToList();

        [UnityTest]
        public IEnumerator TheOptionsScreen_ShowsOnePageAtATime_AStartsOnSound_AndACategoryChangesIt()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ShowOptions();
            yield return null;
            yield return null;
            var names = ActiveButtons().Select(b => b.name).ToList();
            foreach (var id in OptionsScreen.Groups) CollectionAssert.Contains(names, OptionsScreen.CategoryPrefix + id, id);
            CollectionAssert.DoesNotContain(names, "HorrorLevel", "the content page is not showing");
            Assert.IsEmpty(names.Where(n => n.StartsWith(OptionsScreen.KeyPrefix)), "and neither is the key table");
            Assert.AreEqual("sound", ui.Options.CurrentPage);
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<Slider>().Any(s => s.gameObject.activeInHierarchy), "the sound sliders are showing");

            ActiveButtons().First(b => b.name == OptionsScreen.CategoryPrefix + "content").onClick.Invoke();
            yield return null;
            yield return null;
            Assert.IsTrue(ActiveButtons().Any(b => b.name == "HorrorLevel"), "its category shows its page");
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<Slider>().Any(s => s.gameObject.activeInHierarchy), "and the sound page is gone");

            ui.CloseAllModals();
            yield return null;
            ui.ShowOptions();
            yield return null;
            yield return null;
            Assert.IsTrue(ActiveButtons().Any(b => b.name == "HorrorLevel"), "the page is still the content page when the screen comes back");
        }

        [UnityTest]
        public IEnumerator TheKeyTable_HasAnActionAPrimaryAndASecondaryCell_AndADashWhereThereIsNoSecondKey()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ShowOptions();
            ui.Options.Expand("keys");
            yield return null;
            yield return null;

            var rows = UnityEngine.Object.FindObjectsByType<RectTransform>().Where(r => r.gameObject.activeInHierarchy).ToList();
            var interact = rows.First(r => r.name == L.Get("action.Interact"));
            var cells = interact.GetComponentsInChildren<Button>().ToList();
            Assert.AreEqual(2, cells.Count, "a primary and a secondary cell");
            StringAssert.StartsWith(OptionsScreen.KeyPrefix, cells[0].name);
            StringAssert.StartsWith(OptionsScreen.AltPrefix, cells[1].name);
            Assert.Less(cells[0].transform.position.x, cells[1].transform.position.x, "the primary is left of the secondary");

            var pause = rows.First(r => r.name == L.Get("action.Pause"));
            var pauseCells = pause.GetComponentsInChildren<Button>().ToList();
            Assert.AreEqual(1, pauseCells.Count, "Escape has no second key");
            StringAssert.Contains(L.Get("options.none_key"), pause.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Last().text);
            Assert.AreEqual(cells[0].transform.position.x, pauseCells[0].transform.position.x, 0.5f, "the columns line up from row to row");
        }

        [UnityTest]
        public IEnumerator ASecondaryKey_CanBeRebound_ThroughTheTable_AndSticks()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            var input = GameContext.Current.Input;
            ui.ShowOptions();
            ui.Options.Expand("keys");
            yield return null;
            yield return null;
            var row = UnityEngine.Object.FindObjectsByType<RectTransform>().First(r => r.gameObject.activeInHierarchy && r.name == L.Get("action.Inventory"));
            var secondary = row.GetComponentsInChildren<Button>()[1];
            secondary.onClick.Invoke();
            yield return null;
            yield return null;
            Press(_keyboard.gKey);
            yield return null;
            currentTime += 1.0;
            yield return null;
            yield return null;
            Release(_keyboard.gKey);
            yield return null;
            yield return null;
            Assert.AreEqual("<Keyboard>/tab", input.Inventory.bindings.First(b => b.path == "<Keyboard>/tab").effectivePath, "the primary key is untouched");
            Assert.AreEqual("<Keyboard>/g", input.Inventory.bindings.First(b => b.path == "<Keyboard>/i").effectivePath, "the secondary key is the new one");
        }

        [UnityTest]
        public IEnumerator TheCKey_OpensCrafting()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            Press(_keyboard.cKey);
            yield return null;
            yield return null;
            Release(_keyboard.cKey);
            yield return null;
            Assert.IsTrue(ui.AnyModalOpen, "C opens the crafting page of the menu");
        }

        [UnityTest]
        public IEnumerator WithAPad_TheMouseIsIgnored_AndItsPointerIsBack_WhenTheMouseMoves()
        {
            yield return StartFarm();
            var input = GameContext.Current.Input;
            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            yield return null;
            Assert.IsTrue(input.Ui[InputNames.Point].enabled, "mouse in use: the pointer works");
            Assert.IsTrue(Cursor.visible);

            ControlPrompts.SetKind(InputKind.Gamepad);
            yield return null;
            Assert.IsFalse(input.Ui[InputNames.Point].enabled, "pad in use: nothing under the mouse is hovered");
            Assert.IsFalse(input.Ui[InputNames.Click].enabled);
            Assert.IsFalse(Cursor.visible, "and the pointer is hidden");

            Press(_keyboard.spaceKey);
            yield return null;
            yield return null;
            Release(_keyboard.spaceKey);
            Assert.AreEqual(InputKind.KeyboardMouse, ControlPrompts.Kind, "a key takes over again (and so does a moving mouse)");
            Assert.IsTrue(input.Ui[InputNames.Point].enabled);
            Assert.IsTrue(Cursor.visible);
        }

        // Playtest 2026-10-15: no pad button skipped the tips, and the pointer cannot reach the Skip button.
        [UnityTest]
        public IEnumerator WithAPad_BSkipsTheTips_AndTheButtonSaysSo()
        {
            yield return StartFarm(started => started.SetFlag(FatigueModel.WarnedFlag));
            ControlPrompts.SetKind(InputKind.Gamepad);
            for (var i = 0; i < 30; i++) yield return null;
            var s = GameContext.Current.Session;
            var card = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Image>().First(i => i.name == "TutorialCoach" && i.gameObject.scene.IsValid());
            Assert.IsTrue(card.gameObject.activeSelf, "the first tip is up");
            var label = card.transform.Find("CoachSkip").GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
            StringAssert.Contains("B", label, "the button names the pad button");
            StringAssert.DoesNotContain("[[", label);

            yield return Tap(_pad.buttonEast);
            for (var i = 0; i < 10; i++) yield return null;
            Assert.IsTrue(s.HasFlag(TutorialCoach.SkipFlag), "B skipped the tips");
            Assert.IsFalse(card.gameObject.activeSelf);
        }

        // Playtest 2026-10-11: the "It is getting late" window had to be closed with A, while B closed the shop.
        [UnityTest]
        public IEnumerator TheLateNightWarning_ClosesWithB_AsWellAsWithA()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ShowMessage("late_night.warning");
            yield return null;
            yield return null;
            Assert.IsTrue(ui.AnyModalOpen, "the warning is up");
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(ui.AnyModalOpen, "B closes it");

            ui.ShowMessage("late_night.warning");
            yield return null;
            yield return null;
            yield return Tap(_pad.buttonSouth);
            Assert.IsFalse(ui.AnyModalOpen, "and so does A");
        }

        // ---- rebinding persists through the real Options screen -------------------------------------------------

        [UnityTest]
        public IEnumerator RebindingAKey_ThroughOptions_PersistsAcrossARestart()
        {
            yield return StartFarm();
            var ui = ((UiService)GameContext.Current.Ui);
            var input = GameContext.Current.Input;
            Assert.AreEqual("<Keyboard>/e", input.Interact.bindings.First(b => b.path.StartsWith("<Keyboard>")).effectivePath);

            ui.ShowOptions();
            ui.Options.Expand("keys");
            yield return null;
            yield return null;

            // The Interact row: a label plus a button showing the current key.
            var row = UnityEngine.Object.FindObjectsByType<RectTransform>()
                .First(r => r.gameObject.activeInHierarchy && r.name == L.Get("action.Interact"));
            var button = row.GetComponentInChildren<Button>();
            Assert.IsNotNull(button);
            button.onClick.Invoke();
            yield return null;
            yield return null;

            Press(_keyboard.gKey);
            yield return null;
            // The rebinding operation completes a short while after the match, measured on the input clock, which a
            // test fixture freezes. Move it on.
            currentTime += 1.0;
            yield return null;
            yield return null;
            Release(_keyboard.gKey);
            yield return null;
            yield return null;

            Assert.AreEqual("<Keyboard>/g", input.Interact.bindings.First(b => b.path.StartsWith("<Keyboard>")).effectivePath,
                "the new key is bound in the running game");

            // Close Options (saves settings), then "restart": rebuild every service from the files on disk.
            ui.CloseAllModals();
            yield return null;
            var settingsFile = Path.Combine(_dataRoot, "settings.json");
            Assert.IsTrue(File.Exists(settingsFile), "settings were saved");
            StringAssert.Contains("<Keyboard>/g", File.ReadAllText(settingsFile));

            Bootstrapper.ResetForTests();
            Bootstrapper.InitializeServices();
            yield return null;
            var restarted = GameContext.Current.Input;
            Assert.AreEqual("<Keyboard>/g", restarted.Interact.bindings.First(b => b.path.StartsWith("<Keyboard>")).effectivePath,
                "the rebinding survived a restart");
        }

        // ---- backpack drag and drop -----------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator DraggingAStack_ToAnotherSlot_MovesIt()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ToggleInventory();
            yield return null;
            yield return null;

            var slots = UnityEngine.Object.FindObjectsByType<InventorySlotDrag>()
                .OrderBy(d => d.transform.GetSiblingIndex()).ToList();
            Assert.GreaterOrEqual(slots.Count, 8);

            var hoe = s.Backpack.Get(0).ItemId;
            Assert.IsNull(s.Backpack.Get(11));

            var data = new PointerEventData(EventSystem.current) { position = new Vector2(100, 100), button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(slots[0].gameObject, data, ExecuteEvents.beginDragHandler);
            yield return null;
            var screen = FindScreen(ui);
            Assert.IsTrue(screen.IsDragging, "a drag is in progress");
            Assert.IsNotNull(GameObject.Find("DragIcon"), "an icon follows the pointer");
            ExecuteEvents.Execute(slots[11].gameObject, data, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(slots[0].gameObject, data, ExecuteEvents.endDragHandler);
            yield return null;

            Assert.IsNull(s.Backpack.Get(0), "the stack left its slot");
            Assert.AreEqual(hoe, s.Backpack.Get(11).ItemId, "and arrived in the target slot");
            Assert.IsFalse(screen.IsDragging);
        }

        [UnityTest]
        public IEnumerator ReleasingADragOutsideAnySlot_ChangesNothing()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ToggleInventory();
            yield return null;
            yield return null;
            var slots = UnityEngine.Object.FindObjectsByType<InventorySlotDrag>()
                .OrderBy(d => d.transform.GetSiblingIndex()).ToList();
            var first = s.Backpack.Get(0).ItemId;

            var data = new PointerEventData(EventSystem.current) { position = new Vector2(5, 5), button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(slots[0].gameObject, data, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(slots[0].gameObject, data, ExecuteEvents.endDragHandler);
            yield return null;

            Assert.AreEqual(first, s.Backpack.Get(0).ItemId);
            Assert.IsFalse(FindScreen(ui).IsDragging);
        }

        [UnityTest]
        public IEnumerator DraggingAnEmptySlot_DoesNothing()
        {
            GameSession s = null;
            yield return StartFarm(session => s = session);
            var ui = ((UiService)GameContext.Current.Ui);
            ui.ToggleInventory();
            yield return null;
            yield return null;
            var slots = UnityEngine.Object.FindObjectsByType<InventorySlotDrag>()
                .OrderBy(d => d.transform.GetSiblingIndex()).ToList();
            var data = new PointerEventData(EventSystem.current) { position = new Vector2(5, 5) };
            ExecuteEvents.Execute(slots[10].gameObject, data, ExecuteEvents.beginDragHandler);   // slot 10 is empty
            Assert.IsFalse(FindScreen(ui).IsDragging);
        }

        // The inventory screen is private to UiService; find it through the open modal's drag components.
        static InventoryScreen FindScreen(UiService ui)
        {
            var field = typeof(UiService).GetField("_inventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (InventoryScreen)field.GetValue(ui);
        }
    }
}
