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
        public static readonly string[] Topics = { "help.eat", "help.chest", "help.quality", "help.trees", "help.forage", "help.tracker", "help.sleep", "help.shops", "help.cat", "help.health", "help.rows" };
        public static readonly string[] Controls = { "help.control.move", "help.control.use", "help.control.interact", "help.control.bar", "help.control.inventory", "help.control.crafting", "help.control.menu" };

        RectTransform _list;
        const float TextWidth = 640f;               // a little under the width the text really has, so that the measured height is never too small

        // The open branches of the tree. Tools are open to begin with (it is what is looked up most); the rest is closed until it is asked for.
        readonly System.Collections.Generic.HashSet<string> _open = new System.Collections.Generic.HashSet<string> { "section.tools" };

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

        UiService Ui;

        protected override void Build(UiService ui, RectTransform content)
        {
            Ui = ui;
            var stack = UiKit.VStack(content, "Help", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("help.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var hint = UiKit.Label(stack.transform, L.Get("help.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 22f);
            var tour = UiKit.MakeButton(stack.transform, L.Get("tour.start"), () => ui.ShowMenuTour(), 240f, 32f);          // the guided tour, for anyone who wants it
            tour.name = "StartTour";
            var scroll = UiKit.Scroll(stack.transform, "List", out _list);
            UiKit.Size(scroll.gameObject, -1f, -1f, 1f, 1f);
        }

        // A branch of the tree: a title and what it says when it is opened.
        readonly struct Entry
        {
            public readonly string Id, Title, Body;
            public readonly ToolType? Tool;
            public Entry(string id, string title, string body, ToolType? tool = null) { Id = id; Title = title; Body = body; Tool = tool; }
        }

        public static string TitleKeyOf(string key) => key + ".title";

        // Where a tool is found (playtest 2026-10-13: "I want a fishing rod. I remember someone said where to get it, but not where").
        public static string WhereKey(ToolType tool) => "tool.where." + tool.ToString().ToLowerInvariant();

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_list);

            var tools = new System.Collections.Generic.List<Entry>();
            foreach (var tool in ToolOrder)
            {
                var item = ui.Session.Db.AllItems.FirstOrDefault(i => i.IsTool && i.ToolType == tool);
                tools.Add(new Entry("tool." + tool, item != null ? L.Get(item.NameKey) : tool.ToString(), L.Get(HotbarTooltip.UseKey(tool)) + (L.Has(WhereKey(tool)) ? "\n" + L.Get(WhereKey(tool)) : string.Empty), tool));
            }
            Section("tools", L.Get("help.tools"), tools);
            Section("everyday", L.Get("help.everyday"), Topics.Select(k => new Entry(k, L.Get(TitleKeyOf(k)), L.Get(k))).ToList());
            Section("controls", L.Get("help.controls"), Controls.Select(k => new Entry(k, L.Get(TitleKeyOf(k)), L.Get(k))).ToList());
            Section("screen", L.Get("help.screen"), MenuTourScreen.HudSteps.Where(id => L.Has("tour.hud." + id))
                .Select(id => new Entry("hud." + id, L.Get("tour.hud." + id + ".title"), L.Get("tour.hud." + id))).ToList());
            Section("menu", L.Get("help.menu"), MenuTourScreen.Order.Where(id => L.Has("tour." + id))
                .Select(id => new Entry("tab." + id, L.Get("menu.tab." + id), L.Get("tour." + id))).ToList());
        }

        // A branch with its leaves. Opening or closing a branch keeps the place in the list.
        void Section(string id, string title, System.Collections.Generic.List<Entry> entries)
        {
            var key = "section." + id;
            var open = _open.Contains(key);
            Toggle(key, (open ? "v  " : ">  ") + title, 22f, 36f, "Section_" + id);
            if (!open) return;
            foreach (var entry in entries)
            {
                var leaf = "leaf." + entry.Id;
                var leafOpen = _open.Contains(leaf);
                var row = Toggle(leaf, "      " + (leafOpen ? "v  " : ">  ") + entry.Title, 18f, 32f, entry.Tool.HasValue ? "Tool_" + entry.Tool.Value : "Leaf_" + entry.Id);
                if (!leafOpen) continue;
                var text = UiKit.Label(_list, entry.Body, 16f, TextAlignmentOptions.TopLeft, UiKit.DimText);
                text.textWrappingMode = TextWrappingModes.Normal;
                text.margin = new Vector4(56f, 0f, 8f, 6f);
                text.name = "Body_" + entry.Id;
                UiKit.Size(text.gameObject, -1f, Mathf.Max(28f, MeasureHeight(entry.Body, 16f, TextWidth - 40f) + 10f));
            }
        }

        // One line of the tree that opens or closes a branch when it is clicked (or chosen with the pad).
        Button Toggle(string key, string text, float size, float height, string objectName)
        {
            var button = UiKit.MakeButton(_list, text, () =>
            {
                if (!_open.Remove(key)) _open.Add(key);
                if (Ui != null) Refresh(Ui);
            }, 100f, height);
            button.name = objectName;
            UiKit.Size(button.gameObject, -1f, height);
            return button;
        }
    }
}
