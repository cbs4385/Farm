using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // Sell to the general store's keeper, from the conversation menu: the backpack on screen, click a stack to pick it, then Sell 1 or Sell all; a right-click sells one
    // at once. Paid in gold on the spot.
    public sealed class SellScreen : UiScreen
    {
        const int Columns = 12;
        const float SlotSize = 46f;

        readonly RectTransform _grid;
        readonly TextMeshProUGUI _focus;
        readonly Button _sellOne;
        readonly Button _sellAll;
        int _selected = -1;
        System.Action _onClose;

        public int Selected => _selected;

        public SellScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Sell", new Vector2(740f, 380f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 6f, 14);
            UiKit.Stretch((RectTransform)stack.transform);

            UiKit.Label(stack.transform, L.Get("sell.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Label(stack.transform, L.Get("sell.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);

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

            var buttons = UiKit.HStack(stack.transform, "Buttons", 10f);
            UiKit.Size(buttons.gameObject, -1f, 34f);
            _sellOne = UiKit.MakeButton(buttons.transform, L.Get("sell.one"), () => SellSelected(1), 160f, 34f);
            _sellOne.name = "SellOne";
            _sellAll = UiKit.MakeButton(buttons.transform, L.Get("sell.all"), () => SellSelected(int.MaxValue), 160f, 34f);
            _sellAll.name = "SellAll";
            UiKit.MakeButton(buttons.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenSell(System.Action onClose)
        {
            _onClose = onClose;
            _selected = -1;
            Rebuild();
            Open();
        }

        public override void Close()
        {
            var callback = _onClose;
            _onClose = null;
            base.Close();
            callback?.Invoke();
        }

        // Sells from the picked stack. Returns the gold paid.
        public int SellSelected(int count) => _selected < 0 ? 0 : SellSlot(_selected, count);

        public int SellSlot(int slot, int count)
        {
            var s = Ui.Session;
            var stack = s.Backpack.Get(slot);
            if (stack == null) return 0;
            s.Db.TryGetItem(stack.ItemId, out var item);
            var before = stack.Count;
            var gold = SellLogic.Sell(s, slot, count);
            if (gold > 0)
            {
                var sold = before - (s.Backpack.Get(slot)?.Count ?? 0);
                s.Toast(L.Get("sell.sold", sold + " " + (item != null ? L.Get(item.NameKey) : stack.ItemId), gold));
                AudioService.PlayIfAvailable(Sfx.Coin);
            }
            if (s.Backpack.Get(slot) == null) _selected = -1;
            Rebuild();
            return gold;
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
                var can = SellLogic.CanSell(s, i);
                var button = UiKit.MakeButton(_grid, $"Slot{i}", () => { _selected = index; Rebuild(); }, SlotSize, SlotSize);
                button.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
                button.interactable = can;
                button.gameObject.AddComponent<SlotRightClick>().OnRightClick = () => { if (can) { _selected = index; SellSlot(index, 1); } };
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
                    SlotBadges.Quality(button.transform, stack.Quality);
                    if (stack.Count > 1)
                    {
                        var count = UiKit.Label(button.transform, stack.Count.ToString(), 12f, TextAlignmentOptions.BottomRight);
                        UiKit.Stretch(count.rectTransform, 3f);
                    }
                }
            }

            var focusStack = _selected >= 0 ? inv.Get(_selected) : null;
            if (focusStack != null && s.Db.TryGetItem(focusStack.ItemId, out var focusItem))
                _focus.text = L.Get("sell.focus", L.Get(focusItem.NameKey), focusStack.Count, SellLogic.ValueOf(s, _selected, 1));
            else _focus.text = SellLogic.AnythingToSell(s) ? L.Get("sell.pick") : L.Get("sell.nothing");

            _sellOne.interactable = _sellAll.interactable = focusStack != null;
            if (IsOpen && EventSystem.current != null && _selected >= 0 && _selected < _grid.childCount)
                EventSystem.current.SetSelectedGameObject(_grid.GetChild(_selected).gameObject);
        }
    }
}
