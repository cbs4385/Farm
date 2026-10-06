using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
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
        TextMeshProUGUI _date, _time, _weather, _forecast, _gold, _energyLabel, _toast;
        GameObject _streamBadge;
        TextMeshProUGUI _streamText;
        string _streamShown;
        Image _energyFill;
        Image _dateIcon, _timeIcon, _weatherIcon, _goldIcon, _healthIcon;
        static readonly string[] SeasonArt = { "spring", "summer", "fall", "winter" };
        bool _dirty = true;
        float _toastTimer;
        HudDriver _driver;

        public HudView(UiService ui, Transform canvas)
        {
            _ui = ui;
            BuildClockPanel(canvas);
            BuildEnergy(canvas);
            BuildHotbar(canvas);
            BuildToast(canvas);
            BuildStreamBadge(canvas);
            BuildExtensions(canvas);

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

        // Stream mode (T-145): a small badge that tells viewers what content level this game is running at.
        void BuildStreamBadge(Transform canvas)
        {
            var panel = UiKit.Panel(canvas, "StreamBadge", UiKit.PanelColor);
            UiKit.Place(panel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(230, 30), new Vector2(10, -10));
            panel.raycastTarget = false;
            _streamBadge = panel.gameObject;
            _streamText = UiKit.Label(panel.transform, "", 16f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Stretch(_streamText.rectTransform);
            _streamBadge.SetActive(false);
        }

        void UpdateStreamBadge()
        {
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            var text = settings != null && settings.StreamMode ? L.Get("stream.badge", L.Get("options.horror." + settings.HorrorLevel)) : null;
            if (text == _streamShown) return;
            _streamShown = text;
            _streamBadge.SetActive(text != null);
            if (text != null) _streamText.text = text;
        }

        void BuildClockPanel(Transform canvas)
        {
            var panel = UiKit.Panel(canvas, "ClockPanel", UiKit.PanelColor);
            Fadable(panel);
            UiKit.Place(panel.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(190, 136), new Vector2(-10, -10));
            var stack = UiKit.VStack(panel.transform, "Stack", 2f, 8);
            UiKit.Stretch((RectTransform)stack.transform);
            _date = UiKit.Label(stack.transform, "", 17f, TextAlignmentOptions.Right);
            _time = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Right, UiKit.Accent);
            _weather = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Right, UiKit.DimText);
            _forecast = UiKit.Label(stack.transform, "", 13f, TextAlignmentOptions.Right, UiKit.DimText);
            _gold = UiKit.Label(stack.transform, "", 20f, TextAlignmentOptions.Right);
            _dateIcon = Icon(_date.rectTransform, 20f);
            _timeIcon = Icon(_time.rectTransform, 22f);
            _weatherIcon = Icon(_weather.rectTransform, 20f);
            _goldIcon = Icon(_gold.rectTransform, 22f);
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

                _slots[i] = new HotbarSlot { Background = bg, Icon = icon, Count = count };
            }
        }

        static string KeyLabel(int i) => i < 9 ? (i + 1).ToString() : i == 9 ? "0" : i == 10 ? "-" : "=";

        // Top-left stack that hosts widgets supplied by modules through GameHooks.AddHudWidget.
        void BuildExtensions(Transform canvas)
        {
            if (_ui.Session.Hooks.HudWidgets.Count == 0) return;
            var stack = UiKit.VStack(canvas, "ExtensionWidgets", 4f);
            UiKit.Place((RectTransform)stack.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(220, 200), new Vector2(10, -10));
            foreach (var factory in _ui.Session.Hooks.HudWidgets)
            {
                var widget = factory();
                widget.Build(stack.transform);
                _widgets.Add(widget);
            }
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
            UpdateStreamBadge();
            TickToast();
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
            }
        }

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
