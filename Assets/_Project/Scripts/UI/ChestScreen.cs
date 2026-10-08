using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // A chest next to the backpack (T-037). Select a stack to move all of it to the other side. Every slot is a Button, so
    // mouse, keyboard and gamepad all work.
    public sealed class ChestScreen : UiScreen
    {
        const int Columns = 12;
        const float SlotSize = 46f;

        readonly RectTransform _chestGrid;
        readonly RectTransform _packGrid;
        readonly TextMeshProUGUI _info;
        Inventory _chest;

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
            var holder = UiKit.Rect(name, parent);
            var layout = holder.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(4f, 4f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            UiKit.Size(holder.gameObject, -1f, rows * SlotSize + (rows - 1) * 4f);
            return holder;
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

        void Rebuild()
        {
            var session = Ui.Session;
            BuildGrid(_chestGrid, _chest, session.Backpack, "Chest");
            BuildGrid(_packGrid, session.Backpack, _chest, "Pack");
        }

        void BuildGrid(RectTransform grid, Inventory source, Inventory target, string prefix)
        {
            UiKit.ClearChildren(grid);
            for (var i = 0; i < source.Capacity; i++)
            {
                var slot = i;
                var stack = source.Get(i);
                var button = UiKit.MakeButton(grid, string.Empty, () => Move(source, slot, target), SlotSize, SlotSize);
                button.name = $"{prefix}Slot{i}";
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (stack == null) { label.text = string.Empty; continue; }

                var icon = UiKit.Panel(button.transform, "Icon", Color.white);
                UiKit.Stretch(icon.rectTransform, 5f);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                icon.sprite = Ui.Session.Db.TryGetItem(stack.ItemId, out var item) ? item.Icon : null;
                label.transform.SetAsLastSibling();
                label.alignment = TextAlignmentOptions.BottomRight;
                label.fontSize = 14f;
                label.text = stack.Count > 1 ? stack.Count.ToString() : string.Empty;
            }
        }

        void Move(Inventory source, int slot, Inventory target)
        {
            if (source.Get(slot) == null) return;
            if (ChestTransfer.Move(source, slot, target, intoChest: source == Ui.Session.Backpack) > 0)
                Ui.Session.Toast(L.Get("toast.inventory_full"));
            Rebuild();
        }
    }
}
