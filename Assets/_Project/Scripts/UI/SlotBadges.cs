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
        static readonly Color[] Tints =
        {
            Color.clear,
            new Color(0.82f, 0.86f, 0.92f),     // silver
            new Color(0.97f, 0.78f, 0.22f),     // gold
            new Color(0.78f, 0.45f, 0.95f),     // iridium
        };

        public static Color TintOf(int quality) => quality > 0 && quality < Tints.Length ? Tints[quality] : Color.clear;

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
