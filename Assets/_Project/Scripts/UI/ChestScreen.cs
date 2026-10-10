using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // A chest next to the backpack (T-037). A click moves a whole stack to the other side, a right-click one item, Shift-click half; a stack can be dragged onto
    // any slot to put it exactly there (playtests 2026-10-09). On a pad, A moves the stack, X one item and Y half. Every slot is a Button, so mouse, keyboard and
    // gamepad all work.
    public sealed class ChestScreen : UiScreen
    {
        const int Columns = 12;
        const float SlotSize = 46f;

        readonly RectTransform _chestGrid;
        readonly RectTransform _packGrid;
        readonly TextMeshProUGUI _info;
        Inventory _chest;
        Image _dragIcon;
        bool _dragChestSide;
        int _dragSlot = -1;
        string _focusName;

        public ChestScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Chest", new Vector2(740f, 470f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 6f, 14);
            UiKit.Stretch((RectTransform)stack.transform);

            UiKit.Label(stack.transform, L.Get("chest.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _chestGrid = Grid(stack.transform, "ChestGrid", 3);
            UiKit.Label(stack.transform, L.Get("inventory.title"), 20f, TextAlignmentOptions.Left, UiKit.Accent);
            _packGrid = Grid(stack.transform, "PackGrid", 3);
            _info = UiKit.Label(stack.transform, L.Get("chest.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            PromptText.Attach(_info, "chest.hint");
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 32f).name = "Close";
            root.SetActive(false);
        }

        static RectTransform Grid(Transform parent, string name, int rows)
        {
            return SlotUi.Grid(parent, name, Columns, rows, SlotSize);
        }

        public void OpenChest(string objectId)
        {
            var session = Ui.Session;
            var grid = session.GetObjects(session.State.CurrentMap);
            var obj = grid.ById(objectId);
            if (obj == null) return;
            _chest = grid.ChestOf(obj);
            AudioService.PlayIfAvailable(Sfx.ChestOpen);
            Rebuild();
            Open();
        }

        Inventory SideOf(bool chestSide) => chestSide ? _chest : Ui.Session.Backpack;

        static string SlotName(bool chestSide, int slot) => (chestSide ? "ChestSlot" : "PackSlot") + slot;

        void Rebuild()
        {
            Ui.HideHover();                                  // the slots are made again: the pointer is on new ones
            BuildGrid(_chestGrid, true);
            BuildGrid(_packGrid, false);
            if (_focusName != null && IsOpen && EventSystem.current != null)
            {
                var again = Root.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == _focusName);
                if (again != null) EventSystem.current.SetSelectedGameObject(again.gameObject);
            }
            _focusName = null;
        }

        void BuildGrid(RectTransform grid, bool chestSide)
        {
            var source = SideOf(chestSide);
            UiKit.ClearChildren(grid);
            for (var i = 0; i < source.Capacity; i++)
            {
                var slot = i;
                var stack = source.Get(i);
                var button = UiKit.MakeButton(grid, string.Empty, () => Click(chestSide, slot, PointerEventData.InputButton.Left), SlotSize, SlotSize);
                button.name = SlotName(chestSide, i);
                button.gameObject.AddComponent<ChestSlotInput>().Bind(this, chestSide, slot);
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (stack == null) { label.text = string.Empty; continue; }

                SlotUi.Icon(button.transform, Ui.Session.Db, stack, 5f);
                label.transform.SetAsLastSibling();
                label.alignment = TextAlignmentOptions.BottomRight;
                label.fontSize = 14f;
                label.text = stack.Count > 1 ? stack.Count.ToString() : string.Empty;
            }
        }

        // The tooltip of the item in a slot, next to the pointer (or the slot, for a pad).
        public void ShowTip(bool chestSide, int slot, Vector2 screenPosition)
        {
            if (_dragSlot >= 0) return;
            var text = ItemTip.Text(Ui.Session, SideOf(chestSide).Get(slot));
            if (text == null) { Ui.HideHover(); return; }
            Ui.ShowHover(text, screenPosition);
        }

        public void HideTip() => Ui.HideHover();

        // How many of a stack a click moves: all of it, one with a right-click (or Ctrl), or half with Shift.
        public static int CountFor(int stackCount, PointerEventData.InputButton button, bool shift, bool control)
        {
            if (button == PointerEventData.InputButton.Right || control) return 1;
            return shift ? ChestTransfer.Half(stackCount) : stackCount;
        }

        public void Click(bool chestSide, int slot, PointerEventData.InputButton button)
        {
            var stack = SideOf(chestSide).Get(slot);
            if (stack == null) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var shift = keyboard != null && keyboard.shiftKey.isPressed;
            var control = keyboard != null && keyboard.ctrlKey.isPressed;
            MoveAcross(chestSide, slot, CountFor(stack.Count, button, shift, control));
        }

        // Moves `count` of a slot's stack to the other side (a click, or the pad's buttons).
        public void MoveAcross(bool chestSide, int slot, int count)
        {
            var source = SideOf(chestSide);
            var stack = source.Get(slot);
            if (stack == null) return;
            if (count < stack.Count) _focusName = SlotName(chestSide, slot);             // part of it stays: keep the cursor on it
            if (ChestTransfer.Move(source, slot, SideOf(!chestSide), intoChest: !chestSide, count: count) > 0)
                Ui.Session.Toast(L.Get("toast.inventory_full"));
            Rebuild();
        }

        // The pad: X moves one item and Y half of the stack under the cursor (A, the confirm button, moves it all).
        public override void Tick()
        {
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad == null || Time.frameCount == OpenedFrame) return;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            var input = selected != null ? selected.GetComponent<ChestSlotInput>() : null;
            if (input == null) return;
            var stack = SideOf(input.ChestSide).Get(input.Slot);
            if (stack == null) return;
            if (pad.buttonWest.wasPressedThisFrame) MoveAcross(input.ChestSide, input.Slot, 1);
            else if (pad.buttonNorth.wasPressedThisFrame) MoveAcross(input.ChestSide, input.Slot, ChestTransfer.Half(stack.Count));
        }

        // ---- dragging a stack onto a slot ---------------------------------------------------------------------------------

        public void BeginDrag(bool chestSide, int slot, PointerEventData eventData)
        {
            var stack = SideOf(chestSide).Get(slot);
            if (stack == null || !Ui.Session.Db.TryGetItem(stack.ItemId, out var item)) return;
            _dragChestSide = chestSide;
            _dragSlot = slot;
            if (_dragIcon == null)
            {
                _dragIcon = UiKit.Panel(Root.transform, "DragIcon", Color.white);
                _dragIcon.raycastTarget = false;
                _dragIcon.preserveAspect = true;
                _dragIcon.rectTransform.sizeDelta = new Vector2(SlotSize - 8f, SlotSize - 8f);
            }
            _dragIcon.sprite = item.Icon;
            _dragIcon.gameObject.SetActive(true);
            _dragIcon.transform.SetAsLastSibling();
            UpdateDrag(eventData);
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (_dragIcon == null || _dragSlot < 0) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Root.transform, eventData.position, eventData.pressEventCamera, out var local))
                _dragIcon.rectTransform.localPosition = local;
        }

        public void DropOn(bool chestSide, int slot)
        {
            if (_dragSlot < 0) return;
            var from = _dragSlot;
            var fromChest = _dragChestSide;
            _dragSlot = -1;
            if (ChestTransfer.Place(SideOf(fromChest), from, SideOf(chestSide), slot, intoChest: chestSide && !fromChest))
                _focusName = SlotName(chestSide, slot);
        }

        public void EndDrag()
        {
            _dragSlot = -1;
            if (_dragIcon != null) _dragIcon.gameObject.SetActive(false);
            if (IsOpen) Rebuild();
        }
    }
}
