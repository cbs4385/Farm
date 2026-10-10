using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // The shipping bin as a window: the backpack on screen, pick a stack, set how many go in the lot (-10, -1, +1, +10, all, none), see
    // what the lot is worth, and ship it all at once. Every control is a Button, so mouse, keyboard and gamepad all work.
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

            var holder = UiKit.Rect("Grid", stack.transform);
            var layout = holder.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(4f, 4f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            UiKit.Size(holder.gameObject, -1f, 3 * SlotSize + 8f);
            _grid = holder;

            _focus = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Left);
            UiKit.Size(_focus.gameObject, -1f, 26f);

            var row = UiKit.HStack(stack.transform, "Quantity", 6f);
            UiKit.Size(row.gameObject, -1f, 32f);
            Qty(row.transform, "-10", () => Change(-10));
            Qty(row.transform, "-1", () => Change(-1));
            Qty(row.transform, "+1", () => Change(1));
            Qty(row.transform, "+10", () => Change(10));
            Qty(row.transform, L.Get("shipping.all"), () => { if (_selected >= 0) _lot.All(Ui.Session, _selected); Rebuild(); });
            Qty(row.transform, L.Get("shipping.none"), () => { if (_selected >= 0) _lot.None(_selected); Rebuild(); });

            _summary = UiKit.Label(stack.transform, "", 19f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_summary.gameObject, -1f, 28f);

            var buttons = UiKit.HStack(stack.transform, "Buttons", 10f);
            UiKit.Size(buttons.gameObject, -1f, 34f);
            _ship = UiKit.MakeButton(buttons.transform, L.Get("shipping.ship"), ShipLot, 220f, 34f);
            _ship.name = "ShipLot";
            UiKit.MakeButton(buttons.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        static void Qty(Transform parent, string text, UnityEngine.Events.UnityAction action) =>
            UiKit.MakeButton(parent, text, action, 84f, 30f).name = "Qty_" + text;

        public void OpenShipping()
        {
            _lot = new ShippingLot();
            _selected = -1;
            Rebuild();
            Open();
        }

        void Change(int delta)
        {
            if (_selected >= 0) _lot.Add(Ui.Session, _selected, delta);
            Rebuild();
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
                var button = UiKit.MakeButton(_grid, $"Slot{i}", () => { _selected = index; if (_lot.Get(index) == 0) _lot.Add(s, index, 1); Rebuild(); }, SlotSize, SlotSize);
                button.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
                button.interactable = can;
                button.gameObject.AddComponent<SlotRightClick>().OnRightClick = () => { if (!can) return; _selected = index; _lot.Add(s, index, 1); Rebuild(); };          // right-click: one more into the lot
                if (_selected == i)
                {
                    var mark = UiKit.Panel(button.transform, "Selected", new Color(UiKit.Accent.r, UiKit.Accent.g, UiKit.Accent.b, 0.6f));
                    UiKit.Stretch(mark.rectTransform);
                    mark.raycastTarget = false;
                }
                if (stack != null && s.Db.TryGetItem(stack.ItemId, out var item))
                {
                    var icon = UiKit.Panel(button.transform, "Icon", can ? Color.white : new Color(1f, 1f, 1f, 0.35f));
                    UiKit.Stretch(icon.rectTransform, 7f);
                    icon.sprite = item.Icon;
                    icon.preserveAspect = true;
                    icon.raycastTarget = false;
                    var inLot = _lot.Get(i);
                    var text = inLot > 0 ? $"{inLot}/{stack.Count}" : stack.Count > 1 ? stack.Count.ToString() : string.Empty;
                    if (text.Length > 0)
                    {
                        var count = UiKit.Label(button.transform, text, 12f, TextAlignmentOptions.BottomRight, inLot > 0 ? UiKit.Accent : UiKit.TextColor);
                        UiKit.Stretch(count.rectTransform, 3f);
                    }
                }
            }

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
