using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // A short guided tour of the game menu, shown once early in a game: the menu opens on the Journal and a caption box walks through each tab in turn, saying what it
    // is for (owner, 2026-10-09). Next / Back / Skip, with the keyboard, mouse or pad. It sits over the menu without dimming it, so the tab being described shows.
    public sealed class MenuTourScreen : UiScreen
    {
        public const string DoneFlag = "tour.menu.done";

        // The Journal first (it holds the quests), then the rest in the order of the tabs.
        public static readonly string[] Order =
        {
            MenuTabs.Journal, MenuTabs.Skills, MenuTabs.Social, MenuTabs.Calendar, MenuTabs.Map, MenuTabs.Collections, MenuTabs.Memories, MenuTabs.Gossip, MenuTabs.Gazette, MenuTabs.Crafting, MenuTabs.Help,
        };

        readonly TextMeshProUGUI _step;
        readonly TextMeshProUGUI _title;
        readonly TextMeshProUGUI _body;
        readonly Button _back;
        readonly Button _next;
        readonly List<string> _tabs = new List<string>();
        int _index;

        // Automated tests redirect the data folder and must not be interrupted by a tour; the tour's own tests switch it on.
        public static bool ForceInTests;
        public static bool Suppressed => GameServices.DataRootOverride != null && !ForceInTests;

        public int Index => _index;
        public int Count => _tabs.Count;

        public MenuTourScreen(UiService ui) : base(ui)
        {
            var scrim = UiKit.Panel(ui.ScreenCanvas.transform, "MenuTour", Color.clear);          // no dimming: the menu behind stays in view
            UiKit.Stretch(scrim.rectTransform);
            scrim.raycastTarget = false;
            Root = scrim.gameObject;

            var box = UiKit.Panel(scrim.transform, "Box", UiKit.PanelLight);            // lighter than the menu behind it, so that it stands out and hides the menu's footer
            UiKit.Place(box.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 190f), new Vector2(0f, 20f));
            box.gameObject.AddComponent<FitToCanvas>();
            var stack = UiKit.VStack(box.transform, "Stack", 6f, 12);
            UiKit.Stretch((RectTransform)stack.transform);

            var head = UiKit.HStack(stack.transform, "Head", 8f);
            UiKit.Size(head.gameObject, -1f, 30f);
            _title = UiKit.Label(head.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_title.gameObject, -1f, 30f, 1f);
            _step = UiKit.Label(head.transform, "", 16f, TextAlignmentOptions.Right, UiKit.DimText);
            UiKit.Size(_step.gameObject, 110f, 30f);

            _body = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.TopLeft);
            UiKit.Size(_body.gameObject, -1f, -1f, 1f, 1f);

            var buttons = UiKit.HStack(stack.transform, "Buttons", 10f, TextAnchor.MiddleRight);
            UiKit.Size(buttons.gameObject, -1f, 34f);
            UiKit.MakeButton(buttons.transform, L.Get("tour.skip"), Finish, 150f, 32f).name = "TourSkip";
            _back = UiKit.MakeButton(buttons.transform, L.Get("tour.back"), () => Go(_index - 1), 130f, 32f);
            _back.name = "TourBack";
            _next = UiKit.MakeButton(buttons.transform, L.Get("tour.next"), () => { if (_index >= _tabs.Count - 1) Finish(); else Go(_index + 1); }, 150f, 32f);
            _next.name = "TourNext";
            Root.SetActive(false);
        }

        // Opens the menu on the Journal and starts the tour. Remembers that it was shown, so it never comes back by itself.
        public void Begin()
        {
            if (IsOpen) return;
            Ui.Session.SetFlag(DoneFlag);
            Ui.ShowGameMenu(MenuTabs.Journal);
            var menu = Ui.GameMenu;
            _tabs.Clear();
            foreach (var id in Order)
                if (menu.HasTab(id) && L.Has("tour." + id)) _tabs.Add(id);
            _index = 0;
            Open();
            Go(0);
        }

        void Go(int index)
        {
            _index = Mathf.Clamp(index, 0, _tabs.Count - 1);
            var id = _tabs[_index];
            Ui.GameMenu.SelectTab(id);
            _title.text = L.Get("menu.tab." + id);
            _body.text = L.Get("tour." + id) + (_index == _tabs.Count - 1 ? "\n\n" + L.Get("tour.end") : string.Empty);
            _step.text = L.Get("tour.step", _index + 1, _tabs.Count);
            _back.interactable = _index > 0;
            UiKit.SetButtonText(_next, L.Get(_index >= _tabs.Count - 1 ? "tour.done" : "tour.next"));
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(_next.gameObject);
        }

        public override void OnCancel() => Finish();

        void Finish()
        {
            Close();
            Ui.GameMenu?.Close();
        }
    }
}
