using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The parts every window with a grid of item slots (backpack, chest, shipping bin, sale) is made of, built one way: the grid, a slot's picture and its count.
    public static class SlotUi
    {
        public const float Gap = 4f;

        // A grid of `columns` slots across; `rows` fixes the height the layout asks for (a window stacking several grids needs it).
        public static RectTransform Grid(Transform parent, string name, int columns, int rows, float slotSize)
        {
            var holder = UiKit.Rect(name, parent);
            var layout = holder.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(slotSize, slotSize);
            layout.spacing = new Vector2(Gap, Gap);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = columns;
            UiKit.Size(holder.gameObject, -1f, rows * slotSize + (rows - 1) * Gap);
            return holder;
        }

        // The item's picture and quality diamond on a slot. Returns the picture, or null when the stack is empty or unknown.
        public static Image Icon(Transform slot, GameDatabase db, ItemStack stack, float inset, bool dim = false)
        {
            if (stack == null || !db.TryGetItem(stack.ItemId, out var item)) return null;
            var icon = UiKit.Panel(slot, "Icon", dim ? new Color(1f, 1f, 1f, 0.35f) : Color.white);
            UiKit.Stretch(icon.rectTransform, inset);
            icon.sprite = item.Icon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            SlotBadges.Quality(slot, stack.Quality);
            var quality = stack.Quality;
            UiTip.Add(slot.gameObject, () => SlotBadges.NameWithQuality(L.Get(item.NameKey), quality));      // a screen with a richer tooltip of its own marks the slot UiTip.HasOwn afterwards
            return icon;
        }

        // A small number in the bottom right corner of a slot.
        public static TextMeshProUGUI Count(Transform slot, string text, float fontSize, Color? color = null)
        {
            var count = UiKit.Label(slot, text, fontSize, TextAlignmentOptions.BottomRight, color ?? UiKit.TextColor);
            UiKit.Stretch(count.rectTransform, 3f);
            return count;
        }

        // The highlight over a slot that is picked.
        public static void Mark(Transform slot, string name = "Selected")
        {
            var mark = UiKit.Panel(slot, name, new Color(UiKit.Accent.r, UiKit.Accent.g, UiKit.Accent.b, 0.6f));
            UiKit.Stretch(mark.rectTransform);
            mark.raycastTarget = false;
        }

        // A solid contour round a slot: used to show every stack that is in a lot at once, not only the one being edited (playtest 2026-10-13).
        public static void Outline(Transform slot, Color color, float thickness = 3f, string name = "InLot")
        {
            var holder = new GameObject(name, typeof(RectTransform));
            holder.transform.SetParent(slot, false);
            UiKit.Stretch((RectTransform)holder.transform);
            Edge(holder.transform, color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness));      // top
            Edge(holder.transform, color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness));      // bottom
            Edge(holder.transform, color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f));      // left
            Edge(holder.transform, color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f));      // right
        }

        static void Edge(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var bar = UiKit.Panel(parent, "Edge", color);
            bar.raycastTarget = false;
            var rt = bar.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
