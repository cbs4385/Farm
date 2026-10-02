using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // Backpack grid. Select a slot to pick up its stack, select another slot to swap/merge. Works with mouse,
    // keyboard and gamepad (every slot is a Button). The tooltip shows the item under the cursor/selection.
    public sealed class InventoryScreen : UiScreen
    {
        const int Columns = 12;
        const float SlotSize = 52f;

        readonly RectTransform _grid;
        readonly TextMeshProUGUI _title;
        readonly TextMeshProUGUI _tooltipName;
        readonly TextMeshProUGUI _tooltipBody;
        readonly TextMeshProUGUI _hint;
        int _picked = -1;
        int _builtCapacity;
        int _dragFrom = -1;
        Image _dragIcon;

        public InventoryScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Inventory", new Vector2(740, 330), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, L.Get("inventory.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);

            var gridHolder = UiKit.Rect("Grid", stack.transform);
            var layout = gridHolder.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(4f, 4f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            UiKit.Size(gridHolder.gameObject, -1f, 3 * SlotSize + 8f);
            _grid = gridHolder;

            var tip = UiKit.Panel(stack.transform, "Tooltip", UiKit.PanelLight);
            UiKit.Size(tip.gameObject, -1f, 78f);
            var tipStack = UiKit.VStack(tip.transform, "TipStack", 2f, 8);
            UiKit.Stretch((RectTransform)tipStack.transform);
            _tooltipName = UiKit.Label(tipStack.transform, "", 19f, TextAlignmentOptions.Left, UiKit.Accent);
            _tooltipBody = UiKit.Label(tipStack.transform, "", 15f, TextAlignmentOptions.Left);
            _hint = UiKit.Label(stack.transform, L.Get("inventory.hint"), 14f, TextAlignmentOptions.Left, UiKit.DimText);

            root.SetActive(false);
        }

        public override void Open()
        {
            _picked = -1;
            Rebuild();
            Ui.Input.Inventory.Enable();   // so the same key can close it while gameplay input is blocked
            base.Open();
        }

        public override void Close()
        {
            _picked = -1;
            EndDrag();
            base.Close();
        }

        // ---- mouse drag and drop (keyboard and gamepad use click-to-pick) ------------------------------------------

        public bool IsDragging => _dragFrom >= 0;

        public void BeginDrag(int index, PointerEventData eventData)
        {
            var stack = Ui.Session.Backpack.Get(index);
            if (stack == null || !Ui.Session.Db.TryGetItem(stack.ItemId, out var item)) return;

            _dragFrom = index;
            _picked = -1;
            if (_dragIcon == null)
            {
                _dragIcon = UiKit.Panel(Ui.ScreenCanvas.transform, "DragIcon", Color.white);
                _dragIcon.raycastTarget = false;      // so the slot underneath still receives the drop
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
            if (_dragIcon == null || !IsDragging) return;
            var canvas = (RectTransform)Ui.ScreenCanvas.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, eventData.position, null, out var local))
                _dragIcon.rectTransform.localPosition = local;
        }

        // The slot under the pointer calls this when the stack is released over it.
        public void DropOn(int targetIndex)
        {
            if (!IsDragging) return;
            var from = _dragFrom;
            _dragFrom = -1;
            if (from != targetIndex) Ui.Session.Backpack.Move(from, targetIndex);
        }

        // Always called when the drag ends (dropped on a slot or released elsewhere).
        public void EndDrag()
        {
            _dragFrom = -1;
            if (_dragIcon != null) _dragIcon.gameObject.SetActive(false);
            if (IsOpen) Rebuild();
        }

        public override void Tick()
        {
            if (Ui.Input.Inventory.WasPressedThisFrame() && Time.frameCount != OpenedFrame) Close();
        }

        public override void OnCancel()
        {
            if (_picked >= 0) { _picked = -1; Rebuild(); return; }
            Close();
        }

        void Rebuild()
        {
            var inv = Ui.Session.Backpack;
            UiKit.ClearChildren(_grid);
            _builtCapacity = inv.Capacity;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            var selectedIndex = selected != null && selected.transform.parent == _grid ? selected.transform.GetSiblingIndex() : 0;

            for (var i = 0; i < inv.Capacity; i++)
            {
                var index = i;
                var button = UiKit.MakeButton(_grid, $"Slot{i}", () => OnSlot(index), SlotSize, SlotSize);
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                label.text = string.Empty;

                var stack = inv.Get(i);
                if (_picked == i)
                {
                    var mark = UiKit.Panel(button.transform, "Picked", new Color(UiKit.Accent.r, UiKit.Accent.g, UiKit.Accent.b, 0.6f));
                    UiKit.Stretch(mark.rectTransform);
                    mark.raycastTarget = false;
                }

                if (stack != null && Ui.Session.Db.TryGetItem(stack.ItemId, out var item))
                {
                    var icon = UiKit.Panel(button.transform, "Icon", Color.white);
                    UiKit.Stretch(icon.rectTransform, 7f);
                    icon.sprite = item.Icon;
                    icon.preserveAspect = true;
                    icon.raycastTarget = false;
                    if (stack.Count > 1)
                    {
                        var count = UiKit.Label(button.transform, stack.Count.ToString(), 14f, TextAlignmentOptions.BottomRight);
                        UiKit.Stretch(count.rectTransform, 3f);
                    }
                }

                button.gameObject.AddComponent<InventorySlotDrag>().Bind(this, index);

                var trigger = button.gameObject.AddComponent<EventTrigger>();
                var enter = new EventTrigger.Entry { eventID = EventTriggerType.Select };
                enter.callback.AddListener(_ => ShowTooltip(index));
                trigger.triggers.Add(enter);
                var hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                hover.callback.AddListener(_ => ShowTooltip(index));
                trigger.triggers.Add(hover);
            }

            _hint.text = _picked >= 0 ? L.Get("inventory.hint_place") : L.Get("inventory.hint");
            ShowTooltip(-1);
            // While closed, Open() -> PushModal focuses the first slot once the screen is active (so Select fires).
            if (IsOpen && _grid.childCount > 0)
                EventSystem.current?.SetSelectedGameObject(_grid.GetChild(Mathf.Clamp(selectedIndex, 0, _grid.childCount - 1)).gameObject);
        }

        void OnSlot(int index)
        {
            var inv = Ui.Session.Backpack;
            if (_picked < 0)
            {
                if (inv.Get(index) != null) _picked = index;
            }
            else
            {
                inv.Move(_picked, index);
                _picked = -1;
            }
            Rebuild();
        }

        void ShowTooltip(int index)
        {
            var inv = Ui.Session.Backpack;
            var stack = index >= 0 && index < inv.Capacity ? inv.Get(index) : null;
            if (stack == null || !Ui.Session.Db.TryGetItem(stack.ItemId, out var item))
            {
                _tooltipName.text = string.Empty;
                _tooltipBody.text = string.Empty;
                return;
            }
            _tooltipName.text = item.IsTool ? Ui.Session.ToolTitle(item.Id, Ui.Session.ToolTier(item.Id)) : L.Get(item.NameKey);
            var body = L.Get(item.DescriptionKey);
            if (item.SellPrice > 0) body += "\n" + L.Get("inventory.sell_value", item.SellPrice);
            _tooltipBody.text = body;
        }
    }
}
