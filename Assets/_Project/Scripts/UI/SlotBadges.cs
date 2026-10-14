using Farm.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The small marks on an item slot. Stacks of different quality never merge, so the quality has to be visible or two stacks of the same picture look like a
    // stacking bug (playtest 2026-10-09: "items not all stacking in inventory").
    public static class SlotBadges
    {
        static readonly Color Outline = new Color(0.14f, 0.09f, 0.06f, 0.95f);
        public static Color TintOf(int quality) => Farm.Gameplay.QualityMarks.TintOf(quality);

        // A small diamond in the top left corner of the slot, tinted by quality. Nothing for ordinary quality.
        public static void Quality(Transform slot, int quality)
        {
            if (quality <= 0) return;
            var tint = TintOf(quality);
            var back = UiKit.Panel(slot, "Quality", Outline);
            Place(back.rectTransform, 15f);
            back.raycastTarget = false;
            var front = UiKit.Panel(back.transform, "Fill", tint);
            front.rectTransform.anchorMin = Vector2.zero;
            front.rectTransform.anchorMax = Vector2.one;
            front.rectTransform.offsetMin = new Vector2(2f, 2f);
            front.rectTransform.offsetMax = new Vector2(-2f, -2f);
            front.raycastTarget = false;
        }

        // The same for a picture that follows the mouse: the old diamond goes, the stack's own comes.
        public static void Replace(Transform holder, int quality)
        {
            var old = holder.Find("Quality");
            if (old != null) { old.name = "Old"; Object.Destroy(old.gameObject); }
            Quality(holder, quality);
        }

        static void Place(RectTransform rect, float size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(10f, -10f);
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }

        // "Silver Parsnip" for a stack of better than ordinary quality, else just the name.
        public static string NameWithQuality(string name, int quality) =>
            quality > 0 && quality <= 3 ? L.Get("quality.name", L.Get("quality." + quality), name) : name;
    }
}
