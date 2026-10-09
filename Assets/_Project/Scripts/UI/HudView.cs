using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farm.UI
{
    // Clock/date panel, gold, energy bar, hotbar, and toast messages. Redraws once per frame when dirty.
    public sealed class HudView
    {
        sealed class HotbarSlot
        {
            public Image Background;
            public Image Icon;
            public TextMeshProUGUI Count;
            public Image Quality;
        }

        static readonly string[] SeasonKeys = { "season.spring", "season.summer", "season.fall", "season.winter" };
        static readonly string[] DayKeys = { "day.mon", "day.tue", "day.wed", "day.thu", "day.fri", "day.sat", "day.sun" };

        readonly UiService _ui;
        readonly HotbarSlot[] _slots = new HotbarSlot[InputNames.HotbarSlots];
        readonly Queue<string> _toasts = new Queue<string>();
        readonly List<IHudWidget> _widgets = new List<IHudWidget>();

        // Panels that fade while the player stands behind them (HudFade).
        readonly List<(RectTransform rect, CanvasGroup group)> _fade = new List<(RectTransform, CanvasGroup)>();
        PlayerController _player;
        float _nextPlayerSearch;

        public int FadablePanels => _fade.Count;

        void Fadable(Image panel)
        {
            // (Not `GetComponent ?? AddComponent`: in the Editor a missing component comes back as a "fake null" that `??` does not see through.)
            if (!panel.gameObject.TryGetComponent<CanvasGroup>(out var group)) group = panel.gameObject.AddComponent<CanvasGroup>();
            _fade.Add((panel.rectTransform, group));
        }

        // Every frame: a panel the player is behind turns see-through; it comes back when they move clear.
        void UpdateFade()
        {
            var cam = Camera.main;
            if (_player == null && Time.unscaledTime >= _nextPlayerSearch)
            {
                _nextPlayerSearch = Time.unscaledTime + 0.5f;
                _player = Object.FindAnyObjectByType<PlayerController>();
            }
            var haveRect = false;
            var playerRect = default(Rect);
            if (_player != null && cam != null)
            {
                var sprite = _player.GetComponentInChildren<SpriteRenderer>();
                if (sprite != null)
                {
                    var b = sprite.bounds;
                    var min = cam.WorldToScreenPoint(b.min);
                    var max = cam.WorldToScreenPoint(b.max);
                    playerRect = Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
                    haveRect = true;
                }
            }
            var corners = new Vector3[4];
            foreach (var (rect, group) in _fade)
            {
                if (rect == null || group == null) continue;
                var target = 1f;
                if (haveRect && rect.gameObject.activeInHierarchy)
                {
                    rect.GetWorldCorners(corners);          // an overlay canvas: world corners are screen pixels
                    target = HudFade.TargetAlpha(Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y), playerRect);
                }
                group.alpha = HudFade.Step(group.alpha, target, Time.unscaledDeltaTime);
            }
        }

        GameObject _healthFrame;
        Image _healthFill;
        TextMeshProUGUI _healthLabel;
        GameObject _fatigueFrame;
        Image _fatigueFill;
        TextMeshProUGUI _fatigueLabel;
        TextMeshProUGUI _date, _time, _weather, _forecast, _gold, _energyLabel, _toast, _mineFloor;
        Image _clockPanel;
        float _transparency = -1f;
        Image _energyFill;
        Image _dateIcon, _timeIcon, _weatherIcon, _goldIcon, _healthIcon;
        static readonly string[] SeasonArt = { "spring", "summer", "fall", "winter" };
        bool _dirty = true;
        bool _tooltipShown;
        float _toastTimer;
        HudDriver _driver;

        public HudView(UiService ui, Transform canvas)
        {
            _ui = ui;
            BuildClockPanel(canvas);
            BuildEnergy(canvas);
            BuildHotbar(canvas);
            BuildToast(canvas);
            BuildExtensions(canvas);
            BuildQuestTracker(canvas);
            BuildSelectionHelp(canvas);

            ui.Bus.Subscribe<StatsChanged>(_ => _dirty = true);
            ui.Bus.Subscribe<MinuteChanged>(_ => _dirty = true);
            ui.Bus.Subscribe<DayStarted>(_ => _dirty = true);
            ui.Bus.Subscribe<ToastRequested>(e => _toasts.Enqueue(e.Message));
            ui.Bus.Subscribe<FlagChanged>(_ => _dirty = true);
            ui.Bus.Subscribe<VarChanged>(_ => _dirty = true);
            L.LanguageChanged += () => _dirty = true;

            _driver = canvas.gameObject.AddComponent<HudDriver>();
            _driver.Init(this);
        }

        // Plain MonoBehaviour that calls back each frame (the HUD itself is a plain class).
        sealed class HudDriver : MonoBehaviour
        {
            HudView _view;
            public void Init(HudView view) => _view = view;
            void LateUpdate() => _view.Tick();
        }

        const float BarHeight = 34f;

        // The status bar: date, time, weather, the forecast and (underground) the mine floor on the left, gold on the right, across the whole top
        // of the screen. It is a little see-through (Options: "Status bar see-through"), so the map shows behind it.
        void BuildClockPanel(Transform canvas)
        {
            var panel = UiKit.Panel(canvas, "ClockPanel", UiKit.PanelColor);
            _clockPanel = panel;
            Fadable(panel);
            panel.raycastTarget = false;
            var rt = panel.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, BarHeight);
            rt.anchoredPosition = Vector2.zero;
            var row = UiKit.HStack(panel.transform, "Row", 18f);
            UiKit.Stretch((RectTransform)row.transform);
            row.padding = new RectOffset(12, 12, 2, 2);
            row.childAlignment = TextAnchor.MiddleLeft;
            _date = BarLabel(row.transform, 17f, TextAlignmentOptions.Left, UiKit.TextColor);
            _time = BarLabel(row.transform, 22f, TextAlignmentOptions.Left, UiKit.Accent);
            _weather = BarLabel(row.transform, 15f, TextAlignmentOptions.Left, UiKit.DimText);
            _forecast = BarLabel(row.transform, 13f, TextAlignmentOptions.Left, UiKit.DimText);
            _mineFloor = BarLabel(row.transform, 17f, TextAlignmentOptions.Left, UiKit.Accent);
            var spacer = UiKit.Panel(row.transform, "Spacer", Color.clear);
            spacer.raycastTarget = false;
            spacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _gold = BarLabel(row.transform, 20f, TextAlignmentOptions.Right, UiKit.TextColor);
            _dateIcon = BarIcon(_date, 20f);
            _timeIcon = BarIcon(_time, 22f);
            _weatherIcon = BarIcon(_weather, 20f);
            _goldIcon = BarIcon(_gold, 22f);
            _mineFloor.gameObject.SetActive(false);
        }

        static TextMeshProUGUI BarLabel(Transform row, float size, TextAlignmentOptions align, Color color)
        {
            var label = UiKit.Label(row, "", size, align, color);
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        // The icon sits in a margin at the left edge of its label.
        static Image BarIcon(TextMeshProUGUI label, float size)
        {
            label.margin = new Vector4(size + 4f, 0f, 0f, 0f);
            return Icon(label.rectTransform, size);
        }

        void ApplyTransparency()
        {
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            var t = settings != null ? settings.HudTransparency : 0.2f;
            if (Mathf.Approximately(t, _transparency)) return;
            _transparency = t;
            var c = UiKit.PanelColor;
            _clockPanel.color = new Color(c.r, c.g, c.b, 1f - t);
        }

        // A small picture at the left edge of a label (the labels are right-aligned, so the left is free). Hidden until it has a sprite.
        static Image Icon(RectTransform label, float size)
        {
            var icon = UiKit.Panel(label, "Icon", Color.white);
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(size, size), Vector2.zero);
            icon.enabled = false;
            return icon;
        }

        static void Show(Image icon, string art)
        {
            var sprite = art != null ? UiArt.Get(art) : null;
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        void BuildEnergy(Transform canvas)
        {
            var frame = UiKit.Panel(canvas, "EnergyBar", UiKit.PanelColor);
            Fadable(frame);
            UiKit.Place(frame.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(34, 140), new Vector2(-12, 12));
            var back = UiKit.Panel(frame.transform, "Back", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Stretch(back.rectTransform, 5f);
            back.rectTransform.offsetMax = new Vector2(-5f, -5f);
            back.rectTransform.offsetMin = new Vector2(5f, 18f);

            _energyFill = UiKit.Panel(back.transform, "Fill", new Color(0.45f, 0.80f, 0.30f));
            UiKit.Stretch(_energyFill.rectTransform);

            _energyLabel = UiKit.Label(frame.transform, "", 13f, TextAlignmentOptions.Center);
            UiKit.Place(_energyLabel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(34, 16), new Vector2(0, 1));

            BuildFatigue(canvas);
            BuildHealth(canvas);
        }

        // The health bar (bottom left, above the hotbar): shown while hurt or underground.
        void BuildHealth(Transform canvas)
        {
            var frame = UiKit.Panel(canvas, "HealthBar", UiKit.PanelColor);
            Fadable(frame);
            _healthFrame = frame.gameObject;
            UiKit.Place(frame.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(170, 22), new Vector2(12, 70));
            var back = UiKit.Panel(frame.transform, "Back", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Stretch(back.rectTransform, 4f);
            _healthFill = UiKit.Panel(back.transform, "Fill", UiPalette.Health);
            UiKit.Stretch(_healthFill.rectTransform);
            _healthLabel = UiKit.Label(frame.transform, "", 13f, TextAlignmentOptions.Center);
            UiKit.Stretch(_healthLabel.rectTransform);
            _healthIcon = Icon(_healthLabel.rectTransform, 18f);
            _healthFrame.SetActive(false);
        }

        // The late-night fatigue meter, beside the energy bar. Shown only while the rating is above zero.
        void BuildFatigue(Transform canvas)
        {
            var frame = UiKit.Panel(canvas, "FatigueBar", UiKit.PanelColor);
            Fadable(frame);
            _fatigueFrame = frame.gameObject;
            UiKit.Place(frame.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(34, 140), new Vector2(-52, 12));
            var back = UiKit.Panel(frame.transform, "Back", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Stretch(back.rectTransform, 5f);
            back.rectTransform.offsetMax = new Vector2(-5f, -5f);
            back.rectTransform.offsetMin = new Vector2(5f, 18f);

            _fatigueFill = UiKit.Panel(back.transform, "Fill", UiPalette.Fatigue);
            UiKit.Stretch(_fatigueFill.rectTransform);

            _fatigueLabel = UiKit.Label(frame.transform, "", 12f, TextAlignmentOptions.Center);
            UiKit.Place(_fatigueLabel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(34, 16), new Vector2(0, 1));
            _fatigueFrame.SetActive(false);
        }

        void BuildHotbar(Transform canvas)
        {
            var bar = UiKit.Panel(canvas, "Hotbar", UiKit.PanelColor);
            Fadable(bar);
            const float slot = 44f;
            const float gap = 3f;
            var width = InputNames.HotbarSlots * slot + (InputNames.HotbarSlots + 1) * gap;
            UiKit.Place(bar.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(width, slot + gap * 2), new Vector2(0, 8));

            for (var i = 0; i < InputNames.HotbarSlots; i++)
            {
                var bg = UiKit.Panel(bar.transform, $"Slot{i}", UiKit.PanelLight);
                UiKit.Place(bg.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(slot, slot),
                    new Vector2(gap + i * (slot + gap), 0));

                var icon = UiKit.Panel(bg.transform, "Icon", Color.white);
                UiKit.Stretch(icon.rectTransform, 5f);
                icon.preserveAspect = true;
                icon.enabled = false;

                var count = UiKit.Label(bg.transform, "", 13f, TextAlignmentOptions.BottomRight);
                UiKit.Stretch(count.rectTransform, 2f);

                var key = UiKit.Label(bg.transform, KeyLabel(i), 10f, TextAlignmentOptions.TopLeft, UiKit.DimText);
                UiKit.Stretch(key.rectTransform, 2f);

                var quality = UiKit.Panel(bg.transform, "Quality", Color.clear);
                quality.rectTransform.anchorMin = quality.rectTransform.anchorMax = new Vector2(1f, 1f);
                quality.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                quality.rectTransform.sizeDelta = new Vector2(9f, 9f);
                quality.rectTransform.anchoredPosition = new Vector2(-8f, -8f);
                quality.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                quality.raycastTarget = false;
                quality.enabled = false;

                _slots[i] = new HotbarSlot { Background = bg, Icon = icon, Count = count, Quality = quality };
            }
        }

        static string KeyLabel(int i) => i < 9 ? (i + 1).ToString() : i == 9 ? "0" : i == 10 ? "-" : "=";

        // Top-left stack that hosts widgets supplied by modules through GameHooks.AddHudWidget.
        void BuildExtensions(Transform canvas)
        {
            if (_ui.Session.Hooks.HudWidgets.Count == 0) return;
            var stack = UiKit.VStack(canvas, "ExtensionWidgets", 4f);
            UiKit.Place((RectTransform)stack.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(220, 200), new Vector2(10, -(BarHeight + 8f)));
            foreach (var factory in _ui.Session.Hooks.HudWidgets)
            {
                var widget = factory();
                widget.Build(stack.transform);
                _widgets.Add(widget);
            }
        }

        TextMeshProUGUI _tracker;
        float _trackerTimer;

        // The quest tracker: below the status bar at the right. It follows the backpack, so it is looked at a few times a second as well as when something changes.
        void BuildQuestTracker(Transform canvas)
        {
            _tracker = UiKit.Label(canvas, "", 15f, TextAlignmentOptions.TopRight, UiKit.Accent);
            _tracker.name = "QuestTracker";
            _tracker.raycastTarget = false;
            UiKit.Place(_tracker.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(340, 170), new Vector2(-10, -(BarHeight + 8f)));
            _tracker.rectTransform.pivot = new Vector2(1, 1);
        }

        void UpdateQuestTracker(bool force)
        {
            if (_tracker == null) return;
            _trackerTimer -= Time.unscaledDeltaTime;
            if (!force && _trackerTimer > 0f) return;
            _trackerTimer = 0.5f;
            var s = _ui.Session;
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            var on = s != null && s.InGame && s.Story != null && (settings == null || settings.QuestTracker);
            var text = on ? QuestTracker.Text(s) : string.Empty;
            if (_tracker.text != text) _tracker.text = text;
        }

        Image _helpPanel;
        TextMeshProUGUI _help;
        int _helpSlot = -2;
        string _helpItem;
        float _helpTimer;
        const float HelpSeconds = 7f;

        // What the picked item is and how to use it, above the item bar for a few seconds whenever the pick changes (and when the game starts). The mouse has the
        // same text when it rests on a slot, but a pad or the keyboard never gets there (playtest 2026-10-09: "I can select tools but have no idea how to use them").
        void BuildSelectionHelp(Transform canvas)
        {
            _helpPanel = UiKit.Panel(canvas, "SelectionHelp", new Color(UiKit.PanelColor.r, UiKit.PanelColor.g, UiKit.PanelColor.b, 0.85f));
            _helpPanel.raycastTarget = false;
            UiKit.Place(_helpPanel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(640, 84), new Vector2(0, 112));
            _help = UiKit.Label(_helpPanel.transform, "", 15f, TextAlignmentOptions.Center);
            UiKit.Stretch(_help.rectTransform, 8f);
            _help.raycastTarget = false;
            _helpPanel.gameObject.SetActive(false);
        }

        void UpdateSelectionHelp()
        {
            if (_helpPanel == null) return;
            var s = _ui.Session;
            if (s == null || !s.InGame || _ui.AnyModalOpen) { _helpPanel.gameObject.SetActive(false); return; }
            var slot = s.State.SelectedHotbar;
            var stack = slot >= 0 && slot < s.Backpack.Capacity ? s.Backpack.Get(slot) : null;
            var itemId = stack?.ItemId;
            if (slot != _helpSlot || itemId != _helpItem)
            {
                _helpSlot = slot;
                _helpItem = itemId;
                _helpTimer = itemId != null ? HelpSeconds : 0f;
                var text = itemId != null ? HotbarTooltip.Text(s, itemId) : null;
                if (text != null) _help.text = text;
            }
            if (_helpTimer > 0f) _helpTimer -= Time.unscaledDeltaTime;
            var show = _helpTimer > 0f && !_tooltipShown;
            if (_helpPanel.gameObject.activeSelf != show) _helpPanel.gameObject.SetActive(show);
        }

        void BuildToast(Transform canvas)
        {
            _toast = UiKit.Label(canvas, "", 20f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Place(_toast.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(700, 36), new Vector2(0, 70));
            _toast.alpha = 0f;
        }

        void Tick()
        {
            UpdateFade();
            ApplyTransparency();
            UpdateHotbarPointer();
            TickToast();
            UpdateQuestTracker(_dirty);
            UpdateSelectionHelp();
            if (!_dirty) return;
            _dirty = false;
            var s = _ui.Session;
            if (s == null || !s.InGame) return;

            foreach (var widget in _widgets) widget.Refresh(s);

            var d = s.Clock.Now;
            _date.text = L.Get("hud.date", L.Get(DayKeys[d.DayOfWeek]), d.Day, L.Get(SeasonKeys[(int)d.Season]));
            _time.text = d.ClockString();
            _weather.text = L.Get("weather." + s.State.Weather);
            _forecast.text = string.IsNullOrEmpty(s.State.ForecastWeather) ? string.Empty
                : L.Get("hud.forecast", L.Get("weather." + s.State.ForecastWeather));
            _gold.text = L.Get("hud.gold", s.State.Gold);
            Show(_dateIcon, "hud_season_" + SeasonArt[(int)d.Season]);
            Show(_timeIcon, "hud_clock_face");
            Show(_weatherIcon, "hud_weather_" + s.State.Weather);
            Show(_goldIcon, "hud_gold");
            var floor = s.State.Mine.Floor;
            _mineFloor.gameObject.SetActive(floor > 0);
            if (floor > 0) _mineFloor.text = L.Get("hud.mine_floor", floor);

            var fraction = s.State.MaxEnergy > 0 ? Mathf.Clamp01((float)s.State.Energy / s.State.MaxEnergy) : 0f;
            _energyFill.rectTransform.anchorMax = new Vector2(1f, fraction);
            _energyFill.color = fraction > 0.25f ? UiPalette.Energy : UiPalette.Low;
            _energyLabel.text = s.State.Energy.ToString();

            var hurt = s.State.Health < s.State.MaxHealth || s.State.Mine.Floor > 0;
            _healthFrame.SetActive(hurt);
            if (hurt)
            {
                _healthFill.color = UiPalette.Health;
                _healthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)s.State.Health / Mathf.Max(1, s.State.MaxHealth)), 1f);
                _healthLabel.text = L.Get("hud.health", s.State.Health, s.State.MaxHealth);
                Show(_healthIcon, "hud_health_icon");
            }

            var tired = s.FatigueRating;
            _fatigueFrame.SetActive(tired > 0f);
            if (tired > 0f)
            {
                _fatigueFill.color = UiPalette.Fatigue;
                _fatigueFill.rectTransform.anchorMax = new Vector2(1f, Mathf.Clamp01(tired));
                _fatigueLabel.text = L.Get("hud.fatigue", Mathf.RoundToInt(tired * 100f));
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                var stack = i < s.Backpack.Capacity ? s.Backpack.Get(i) : null;
                slot.Background.color = i == s.State.SelectedHotbar ? UiKit.Accent : UiKit.PanelLight;

                Sprite icon = null;
                if (stack != null && s.Db.TryGetItem(stack.ItemId, out var item)) icon = item.Icon;
                slot.Icon.sprite = icon;
                slot.Icon.enabled = icon != null;
                slot.Count.text = stack != null && stack.Count > 1 ? stack.Count.ToString() : "";
                var quality = stack != null ? stack.Quality : 0;
                slot.Quality.enabled = quality > 0;
                slot.Quality.color = SlotBadges.TintOf(quality);
            }
        }

        // The mouse over a hotbar slot: a click selects the item, and resting there shows a label saying what the item is and, for a tool, how to use it.
        void UpdateHotbarPointer()
        {
            var s = _ui.Session;
            var mouse = Mouse.current;
            var shown = -1;
            if (mouse != null && s != null && s.InGame && !_ui.AnyModalOpen)
            {
                var position = mouse.position.ReadValue();
                for (var i = 0; i < _slots.Length && shown < 0; i++)
                    if (_slots[i] != null && _slots[i].Background.gameObject.activeInHierarchy
                        && RectTransformUtility.RectangleContainsScreenPoint(_slots[i].Background.rectTransform, position, null)) shown = i;
                if (shown >= 0 && mouse.leftButton.wasPressedThisFrame && s.State.SelectedHotbar != shown)
                {
                    s.State.SelectedHotbar = shown;
                    _ui.Bus.Publish(new StatsChanged());
                }
                var stack = shown >= 0 && shown < s.Backpack.Capacity ? s.Backpack.Get(shown) : null;
                var text = stack != null ? HotbarTooltip.Text(s, stack.ItemId) : null;
                if (text != null) { _ui.ShowHover(text, position); _tooltipShown = true; return; }
            }
            if (_tooltipShown) { _ui.HideHover(); _tooltipShown = false; }
        }

        public string HotbarTooltipText => _tooltipShown ? _ui.HoverText : null;

        void TickToast()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                _toast.alpha = Mathf.Clamp01(_toastTimer / 0.4f);
                return;
            }
            if (_toasts.Count > 0)
            {
                _toast.text = _toasts.Dequeue();
                _toast.alpha = 1f;
                _toastTimer = 2.2f;
            }
        }
    }
}
