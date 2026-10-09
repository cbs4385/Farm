using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The walkthrough of the screen and the menu, shown once when the opening story is closed (owner, 2026-10-09): a frame points at each part of the HUD in turn with a
    // caption that says what it is and calls out the commands that go with it, then the controls together, then the menu opens and each of its tabs is explained. Next,
    // Back or Skip, with the keyboard, mouse or pad. The caption box sits over the menu without dimming it, and moves to the top of the screen when the thing it points
    // at is in the lower half.
    public sealed class MenuTourScreen : UiScreen
    {
        public const string DoneFlag = "tour.menu.done";

        // The parts of the screen, then the controls (no frame), then the menu's tabs.
        public static readonly string[] HudSteps = { "statusbar", "gold", "energy", "hotbar", "tracker", "controls" };

        // The tabs: the Journal first (it holds the quests), then the rest in the order of the tabs.
        public static readonly string[] Order =
        {
            MenuTabs.Journal, MenuTabs.Skills, MenuTabs.Social, MenuTabs.Calendar, MenuTabs.Map, MenuTabs.Collections, MenuTabs.Memories, MenuTabs.Gossip, MenuTabs.Gazette, MenuTabs.Crafting, MenuTabs.Help,
        };

        readonly struct Step
        {
            public readonly string Id;
            public readonly bool IsTab;
            public Step(string id, bool isTab) { Id = id; IsTab = isTab; }
        }

        readonly TextMeshProUGUI _step;
        readonly TextMeshProUGUI _title;
        readonly TextMeshProUGUI _body;
        readonly Button _back;
        readonly Button _next;
        readonly RectTransform _box;
        readonly RectTransform _highlight;
        readonly Image[] _bars = new Image[4];
        readonly List<Step> _steps = new List<Step>();
        bool _menuOpen;
        int _index;

        // Automated tests redirect the data folder and must not be interrupted by a tour; the tour's own tests switch it on.
        public static bool ForceInTests;
        public static bool Suppressed => GameServices.DataRootOverride != null && !ForceInTests;

        public int Index => _index;
        public int Count => _steps.Count;
        public string CurrentStepId => _index >= 0 && _index < _steps.Count ? _steps[_index].Id : null;
        public IReadOnlyList<string> StepIds { get { var ids = new List<string>(); foreach (var s in _steps) ids.Add(s.Id); return ids; } }
        public bool HighlightVisible => _highlight.gameObject.activeSelf;
        public Rect HighlightRect { get; private set; }          // in screen pixels
        public bool CaptionOnTop { get; private set; }
        public Vector2 CaptionSize => _box.rect.size;                    // in canvas units

        public MenuTourScreen(UiService ui) : base(ui)
        {
            var scrim = UiKit.Panel(ui.ScreenCanvas.transform, "MenuTour", Color.clear);          // no dimming: what is being shown stays in view
            UiKit.Stretch(scrim.rectTransform);
            scrim.raycastTarget = false;
            Root = scrim.gameObject;

            _highlight = UiKit.Rect("TourHighlight", scrim.transform);
            UiKit.Stretch(_highlight);
            for (var i = 0; i < _bars.Length; i++)
            {
                _bars[i] = UiKit.Panel(_highlight, "Bar" + i, UiKit.Accent);
                _bars[i].raycastTarget = false;
                _bars[i].rectTransform.anchorMin = _bars[i].rectTransform.anchorMax = _bars[i].rectTransform.pivot = Vector2.zero;
            }
            _highlight.gameObject.SetActive(false);

            var box = UiKit.Panel(scrim.transform, "Box", UiKit.PanelLight);            // lighter than the menu behind it, so that it stands out and hides the menu's footer
            _box = box.rectTransform;
            _box.sizeDelta = new Vector2(900f, 190f);                                        // before FitToCanvas, which keeps the size it finds when it is added
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
            _next = UiKit.MakeButton(buttons.transform, L.Get("tour.next"), () => { if (_index >= _steps.Count - 1) Finish(); else Go(_index + 1); }, 150f, 32f);
            _next.name = "TourNext";
            Root.SetActive(false);
        }

        // Starts the walkthrough. Remembers that it was shown, so it never comes back by itself.
        public void Begin()
        {
            if (IsOpen) return;
            Ui.Session.SetFlag(DoneFlag);
            var menu = Ui.EnsureGameMenu();
            _steps.Clear();
            foreach (var id in HudSteps)
                if (L.Has("tour.hud." + id) && (id == "controls" || Ui.Hud != null && Ui.Hud.TryGetTourRect(id, out _))) _steps.Add(new Step(id, false));
            foreach (var id in Order)
                if (menu.HasTab(id) && L.Has("tour." + id)) _steps.Add(new Step(id, true));
            _index = 0;
            _menuOpen = false;
            Open();
            Go(0);
        }

        void Go(int index)
        {
            _index = Mathf.Clamp(index, 0, _steps.Count - 1);
            var step = _steps[_index];
            var last = _index == _steps.Count - 1;

            if (step.IsTab)
            {
                if (!_menuOpen) { Ui.ShowGameMenu(step.Id); Ui.BringToFront(this); _menuOpen = true; }
                else Ui.GameMenu.SelectTab(step.Id);
                _title.text = L.Get("menu.tab." + step.Id);
                _body.text = L.Get("tour." + step.Id);
                ShowHighlight(null);
                SetCaption(onTop: false);
            }
            else
            {
                if (_menuOpen) { Ui.GameMenu.Close(); _menuOpen = false; }
                _title.text = L.Get("tour.hud." + step.Id + ".title");
                _body.text = L.Get("tour.hud." + step.Id);
                var pointed = step.Id != "controls" && Ui.Hud != null && Ui.Hud.TryGetTourRect(step.Id, out var rect) ? (Rect?)rect : null;
                ShowHighlight(pointed);
                SetCaption(onTop: pointed.HasValue && pointed.Value.center.y < Screen.height * 0.5f);
            }
            if (last) _body.text += "\n\n" + L.Get("tour.end");
            _step.text = L.Get("tour.step", _index + 1, _steps.Count);
            _back.interactable = _index > 0;
            UiKit.SetButtonText(_next, L.Get(last ? "tour.done" : "tour.next"));
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(_next.gameObject);
        }

        // The caption box along the bottom of the screen, or along the top when what it points at is down there.
        void SetCaption(bool onTop)
        {
            CaptionOnTop = onTop;
            var anchor = new Vector2(0.5f, onTop ? 1f : 0f);
            _box.anchorMin = _box.anchorMax = _box.pivot = anchor;
            _box.anchoredPosition = new Vector2(0f, onTop ? -(20f + 34f) : 20f);               // below the status bar when on top
        }

        // A frame round a rectangle of the screen (pixels), or none.
        void ShowHighlight(Rect? screen)
        {
            _highlight.gameObject.SetActive(screen.HasValue);
            if (!screen.HasValue) { HighlightRect = default; return; }
            const float pad = 5f, thick = 3f;
            var k = Mathf.Max(0.01f, Ui.ScreenCanvas.scaleFactor);
            var r = screen.Value;
            HighlightRect = r;
            var x0 = r.xMin / k - pad; var y0 = r.yMin / k - pad;
            var w = r.width / k + 2f * pad; var h = r.height / k + 2f * pad;
            Bar(0, x0, y0, w, thick);                         // bottom
            Bar(1, x0, y0 + h - thick, w, thick);             // top
            Bar(2, x0, y0, thick, h);                         // left
            Bar(3, x0 + w - thick, y0, thick, h);             // right
        }

        void Bar(int i, float x, float y, float w, float h)
        {
            _bars[i].rectTransform.anchoredPosition = new Vector2(x, y);
            _bars[i].rectTransform.sizeDelta = new Vector2(w, h);
        }

        // The frame breathes a little, so that the eye finds it.
        public override void Tick()
        {
            if (!_highlight.gameObject.activeSelf) return;
            var a = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 5f);
            var c = UiKit.Accent;
            foreach (var bar in _bars) bar.color = new Color(c.r, c.g, c.b, a);
        }

        public override void OnCancel() => Finish();

        void Finish()
        {
            Close();
            if (_menuOpen) { Ui.GameMenu?.Close(); _menuOpen = false; }
        }
    }
}
