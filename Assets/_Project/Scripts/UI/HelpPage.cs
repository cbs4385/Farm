using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The Help tab: how to use every tool, and the everyday things a new player has to find out (eating, chests, planting trees, leaving wild plants, the controls).
    // Picking a tool shows its help once in a box over the item bar; everything is kept here to look up again (owner, 2026-10-09).
    public sealed class HelpPage : MenuPage
    {
        // The order the tools are listed in, and the strings for the everyday topics.
        public static readonly ToolType[] ToolOrder = { ToolType.Hoe, ToolType.WateringCan, ToolType.Axe, ToolType.Pickaxe, ToolType.Scythe, ToolType.Hammer, ToolType.Rod, ToolType.Sword };
        public static readonly string[] Topics = { "help.eat", "help.chest", "help.quality", "help.trees", "help.forage", "help.tracker", "help.sleep" };
        public static readonly string[] Controls = { "help.control.move", "help.control.use", "help.control.interact", "help.control.bar", "help.control.inventory", "help.control.menu" };

        RectTransform _list;
        const float TextWidth = 640f;               // a little under the width the text really has, so that the measured height is never too small

        // How tall a text is at a font size when it wraps at a width (measured by a throwaway label: the real height, not a guess from the number of characters).
        static float MeasureHeight(string text, float size, float width)
        {
            var go = new GameObject("Measure", typeof(RectTransform), typeof(TextMeshProUGUI));
            var label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = size * UiKit.TextScale;
            label.textWrappingMode = TextWrappingModes.Normal;
            var height = label.GetPreferredValues(text, width, 0f).y;
            Object.Destroy(go);
            return height;
        }

        public override string Id => MenuTabs.Help;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Help", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("help.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var hint = UiKit.Label(stack.transform, L.Get("help.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 22f);
            var scroll = UiKit.Scroll(stack.transform, "List", out _list);
            UiKit.Size(scroll.gameObject, -1f, -1f, 1f, 1f);
        }

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_list);
            Heading(L.Get("help.tools"));
            foreach (var tool in ToolOrder)
            {
                var item = ui.Session.Db.AllItems.FirstOrDefault(i => i.IsTool && i.ToolType == tool);
                var instructions = L.Get(HotbarTooltip.UseKey(tool));
                var rowHeight = Mathf.Max(56f, 30f + MeasureHeight(instructions, 15f, TextWidth));          // as tall as the words need
                var row = UiKit.HStack(_list, "Tool_" + tool, 10f);
                UiKit.Size(row.gameObject, -1f, rowHeight);
                var slot = UiKit.Rect("IconSlot", row.transform);                 // a fixed-size slot, so that every name starts at the same place whatever the icon is
                UiKit.Size(slot.gameObject, 48f, rowHeight);
                var icon = UiKit.Panel(slot, "Icon", Color.white);
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.rectTransform.sizeDelta = new Vector2(40f, 40f);
                icon.preserveAspect = true;
                icon.sprite = item != null ? item.Icon : null;
                icon.enabled = icon.sprite != null;
                var column = UiKit.VStack(row.transform, "Text", 0f);
                UiKit.Size(column.gameObject, -1f, rowHeight, 1f);
                UiKit.Label(column.transform, item != null ? L.Get(item.NameKey) : tool.ToString(), 18f, TextAlignmentOptions.Left, UiKit.Accent);
                UiKit.Label(column.transform, instructions, 15f, TextAlignmentOptions.TopLeft);
            }

            Heading(L.Get("help.everyday"));
            foreach (var key in Topics) Line(L.Get(key));

            Heading(L.Get("help.controls"));
            foreach (var key in Controls) Line(L.Get(key));
        }

        void Heading(string text)
        {
            var label = UiKit.Label(_list, text, 20f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(label.gameObject, -1f, 30f);
        }

        void Line(string text)
        {
            var label = UiKit.Label(_list, text, 16f, TextAlignmentOptions.TopLeft);
            label.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Size(label.gameObject, -1f, Mathf.Max(24f, MeasureHeight(text, 16f, TextWidth + 50f) + 4f));
        }
    }
}
