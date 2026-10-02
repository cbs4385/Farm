using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // One tab of the game menu. Built once, refreshed every time the tab is shown.
    public abstract class MenuPage
    {
        public abstract string Id { get; }
        public RectTransform Root { get; private set; }

        public void BuildInto(UiService ui, RectTransform parent)
        {
            Root = UiKit.Rect(Id, parent);
            UiKit.Stretch(Root);
            Build(ui, Root);
            Root.gameObject.SetActive(false);
        }

        protected abstract void Build(UiService ui, RectTransform content);
        public abstract void Refresh(UiService ui);

        // The menu was opened (not just switched to this tab): reset any browsing state.
        public virtual void OnMenuOpened() { }
    }

    // The game menu (T-036): skills, social, calendar, map, journal and crafting as tabs. Q/E (or the shoulder buttons)
    // switch tabs; every control is a Selectable, so keyboard and gamepad navigation work.
    public sealed class GameMenuScreen : UiScreen
    {
        readonly List<MenuPage> _pages = new List<MenuPage>();
        readonly List<Button> _tabs = new List<Button>();
        readonly TextMeshProUGUI _gold;
        int _current;

        public MenuPage Current => _pages[_current];
        public IReadOnlyList<MenuPage> Pages => _pages;

        public GameMenuScreen(UiService ui, IEnumerable<MenuPage> pages) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "GameMenu", new Vector2(900f, 500f), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 12);
            UiKit.Stretch((RectTransform)stack.transform);

            var header = UiKit.HStack(stack.transform, "Header", 8f);
            UiKit.Size(header.gameObject, -1f, 40f);
            var tabs = UiKit.HStack(header.transform, "Tabs", 4f);
            UiKit.Size(tabs.gameObject, -1f, 38f, 1f);
            _gold = UiKit.Label(header.transform, "", 20f, TextAlignmentOptions.Right, UiKit.Accent);
            UiKit.Size(_gold.gameObject, 120f, 38f);

            var content = UiKit.Rect("Content", stack.transform);
            UiKit.Size(content.gameObject, -1f, -1f, 1f, 1f);

            var footer = UiKit.HStack(stack.transform, "Footer", 8f, TextAnchor.MiddleCenter);
            UiKit.Size(footer.gameObject, -1f, 34f);
            var hint = UiKit.Label(footer.transform, L.Get("menu.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 30f, 1f);
            UiKit.MakeButton(footer.transform, L.Get("ui.close"), Close, 140f, 32f).name = "Close";

            foreach (var page in pages)
            {
                page.BuildInto(ui, content);
                var index = _pages.Count;
                _pages.Add(page);
                var tab = UiKit.MakeButton(tabs.transform, L.Get("menu.tab." + page.Id), () => Show(index), 118f, 34f);
                tab.name = "Tab_" + page.Id;
                _tabs.Add(tab);
            }
            root.SetActive(false);
        }

        public void OpenTab(string tabId)
        {
            foreach (var page in _pages) page.OnMenuOpened();
            var index = _pages.FindIndex(p => p.Id == tabId);
            _current = index >= 0 ? index : _current;
            Open();
            Show(_current);
        }

        void Show(int index)
        {
            _current = Mathf.Clamp(index, 0, _pages.Count - 1);
            for (var i = 0; i < _pages.Count; i++) _pages[i].Root.gameObject.SetActive(i == _current);
            for (var i = 0; i < _tabs.Count; i++)
            {
                var image = _tabs[i].targetGraphic as Image;
                if (image != null) image.color = i == _current ? new Color(1f, 0.85f, 0.45f) : Color.white;
            }
            _gold.text = L.Get("hud.gold", Ui.Session.State.Gold);
            _pages[_current].Refresh(Ui);
        }

        public override void Tick()
        {
            if (Time.frameCount == OpenedFrame) return;
            var input = Ui.Input.Ui;
            if (input[InputNames.TabNext].WasPressedThisFrame()) Show((_current + 1) % _pages.Count);
            else if (input[InputNames.TabPrev].WasPressedThisFrame()) Show((_current + _pages.Count - 1) % _pages.Count);
        }
    }
}
