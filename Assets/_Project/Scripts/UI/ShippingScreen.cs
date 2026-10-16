using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // The shipping bin as a window: the backpack on screen, pick a stack, set how many go in the lot in the "how many?" window that opens (a single item is put in the lot or
    // taken out again with a click), see what the lot is worth, and ship it all at once. Every control is a Button, so mouse, keyboard and gamepad all work.
    public sealed class ShippingScreen : UiScreen
    {
        const int Columns = 12;
        const float SlotSize = 46f;

        readonly RectTransform _grid;
        readonly TextMeshProUGUI _focus;
        readonly TextMeshProUGUI _summary;
        readonly Button _ship;
        ShippingLot _lot = new ShippingLot();
        int _selected = -1;

        public ShippingLot Lot => _lot;
        public int Selected => _selected;

        public ShippingScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Shipping", new Vector2(740f, 420f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 6f, 14);
            UiKit.Stretch((RectTransform)stack.transform);

            UiKit.Label(stack.transform, L.Get("shipping.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Label(stack.transform, L.Get("shipping.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);

            _grid = SlotUi.Grid(stack.transform, "Grid", Columns, 3, SlotSize);

            _focus = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Left);
            UiKit.Size(_focus.gameObject, -1f, 26f);

            _summary = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _summary.fontStyle = FontStyles.Bold;
            UiKit.Size(_summary.gameObject, -1f, 34f);

            var buttons = UiKit.HStack(stack.transform, "Buttons", 10f);
            UiKit.Size(buttons.gameObject, -1f, 34f);
            _ship = UiKit.MakeButton(buttons.transform, L.Get("shipping.ship"), ShipLot, 220f, 34f);
            _ship.name = "ShipLot";
            UiKit.MakeButton(buttons.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenShipping()
        {
            _lot = new ShippingLot();
            _selected = -1;
            Rebuild();
            Open();
            if (EventSystem.current != null && _grid.childCount > 0) EventSystem.current.SetSelectedGameObject(_grid.GetChild(0).gameObject);          // the pad cursor starts on the first slot, not nowhere
        }

        void AskHowMany(int slot)
        {
            var s = Ui.Session;
            var stack = s.Backpack.Get(slot);
            if (stack == null || !ShippingLot.CanShip(s, slot)) return;
            s.Db.TryGetItem(stack.ItemId, out var item);
            Ui.ShowQuantity(new QuantityRequest
            {
                Title = L.Get("quantity.ship_title"),
                ItemName = item != null ? L.Get(item.NameKey) : stack.ItemId,
                Icon = item?.Icon,
                Min = 0,
                Max = QuantityPicker.MaxToGive(s, slot),
                Initial = System.Math.Max(1, _lot.Get(slot)),
                UnitValue = ShippingLot.ValueOf(s, slot, 1),
                TotalKey = "quantity.total_worth",
            }, count => { _lot.Set(s, slot, count); Rebuild(); });          // Set keeps it between none and the whole stack
        }

        void ShipLot()
        {
            if (_lot.IsEmpty) return;
            var gold = _lot.Gold(Ui.Session);
            var items = _lot.Ship(Ui.Session);
            Ui.Session.Toast(L.Get("shipping.shipped", items, gold));
            AudioService.PlayIfAvailable(Sfx.Coin);
            Close();
        }

        // The cursor moves along the grid exactly as it is drawn: left and right step one slot (wrapping to the next row), up and down one row; down from the last row
        // reaches the Ship button.
        void WireGridNavigation(int count)
        {
            var below = _ship;
            for (var i = 0; i < count && i < _grid.childCount; i++)
            {
                var button = _grid.GetChild(i).GetComponent<Button>();
                if (button == null) continue;
                Selectable At(int j) => j >= 0 && j < count && j < _grid.childCount ? _grid.GetChild(j).GetComponent<Button>() : null;
                button.navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = At(i - 1),
                    selectOnRight = At(i + 1),
                    selectOnUp = At(i - Columns),
                    selectOnDown = At(i + Columns) ?? below,
                };
            }
        }

        void Rebuild()
        {
            var s = Ui.Session;
            var inv = s.Backpack;
            UiKit.ClearChildren(_grid);
            for (var i = 0; i < inv.Capacity; i++)
            {
                var index = i;
                var stack = inv.Get(i);
                var can = ShippingLot.CanShip(s, i);
                var button = UiKit.MakeButton(_grid, $"Slot{i}", () =>
                {
                    if (!can) return;
                    _selected = index;
                    var held = s.Backpack.Get(index);
                    if (held != null && held.Count > 1)
                    {
                        if (_lot.Get(index) == 0) _lot.Add(s, index, 1);
                        Rebuild();
                        AskHowMany(index);          // a stack opens the "how many?" window
                        return;
                    }
                    if (_lot.Get(index) == 0) _lot.Add(s, index, 1); else _lot.None(index);          // one item: a click puts it in the lot, another takes it out
                    Rebuild();
                }, SlotSize, SlotSize);
                button.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
                // A stack that cannot be shipped stays a stop for the pad cursor (dimmed, a click does nothing): if it dropped out of the navigation, the cursor jumped over the gaps
                // to whichever slot was nearest, and a player in a groove ended on the wrong stack (playtest 2026-10-16).
                button.interactable = true;
                button.gameObject.AddComponent<SlotRightClick>().OnRightClick = () => { if (!can) return; _selected = index; _lot.Add(s, index, 1); Rebuild(); };          // right-click: one more into the lot
                var inLot = _lot.Get(i);
                if (inLot > 0) SlotUi.Outline(button.transform, UiKit.Accent);          // every stack in the lot is outlined, so it is plain that several kinds go at once
                if (_selected == i) SlotUi.Mark(button.transform);
                if (SlotUi.Icon(button.transform, s.Db, stack, 7f, dim: !can) != null)
                {
                    var text = inLot > 0 ? $"{inLot}/{stack.Count}" : stack.Count > 1 ? stack.Count.ToString() : string.Empty;
                    if (text.Length > 0) {
                        var badge = SlotUi.Count(button.transform, text, inLot > 0 ? 14f : 12f, inLot > 0 ? UiKit.Accent : UiKit.TextColor);
                        if (inLot > 0) badge.fontStyle = FontStyles.Bold;
                    }
                }
            }

            WireGridNavigation(inv.Capacity);

            var focusStack = _selected >= 0 ? inv.Get(_selected) : null;
            if (focusStack != null && s.Db.TryGetItem(focusStack.ItemId, out var focusItem))
            {
                var unit = ShippingLot.ValueOf(s, _selected, 1);
                _focus.text = L.Get("shipping.focus", L.Get(focusItem.NameKey), focusStack.Count, unit, _lot.Get(_selected));
            }
            else _focus.text = L.Get("shipping.pick");

            _summary.text = _lot.IsEmpty ? L.Get("shipping.empty") : L.Get("shipping.summary", _lot.ItemCount, _lot.Gold(s));
            _ship.interactable = !_lot.IsEmpty;
            if (IsOpen && EventSystem.current != null && _selected >= 0 && _selected < _grid.childCount)
                EventSystem.current.SetSelectedGameObject(_grid.GetChild(_selected).gameObject);
        }
    }
}
