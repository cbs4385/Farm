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

        TextMeshProUGUI _date, _time, _weather, _forecast, _gold, _energyLabel, _toast;
        Image _energyFill;
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

        void BuildClockPanel(Transform canvas)
        {
            var panel = UiKit.Panel(canvas, "ClockPanel", UiKit.PanelColor);
            UiKit.Place(panel.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(190, 136), new Vector2(-10, -10));
            var stack = UiKit.VStack(panel.transform, "Stack", 2f, 8);
            UiKit.Stretch((RectTransform)stack.transform);
            _date = UiKit.Label(stack.transform, "", 17f, TextAlignmentOptions.Right);
            _time = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Right, UiKit.Accent);
            _weather = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Right, UiKit.DimText);
            _forecast = UiKit.Label(stack.transform, "", 13f, TextAlignmentOptions.Right, UiKit.DimText);
            _gold = UiKit.Label(stack.transform, "", 20f, TextAlignmentOptions.Right);
        }

        void BuildEnergy(Transform canvas)
        {
            var frame = UiKit.Panel(canvas, "EnergyBar", UiKit.PanelColor);
            UiKit.Place(frame.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(34, 140), new Vector2(-12, 12));
            var back = UiKit.Panel(frame.transform, "Back", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Stretch(back.rectTransform, 5f);
            back.rectTransform.offsetMax = new Vector2(-5f, -5f);
            back.rectTransform.offsetMin = new Vector2(5f, 18f);

            _energyFill = UiKit.Panel(back.transform, "Fill", new Color(0.45f, 0.80f, 0.30f));
            UiKit.Stretch(_energyFill.rectTransform);

            _energyLabel = UiKit.Label(frame.transform, "", 13f, TextAlignmentOptions.Center);
            UiKit.Place(_energyLabel.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(34, 16), new Vector2(0, 1));
        }

        void BuildHotbar(Transform canvas)
        {
            var bar = UiKit.Panel(canvas, "Hotbar", UiKit.PanelColor);
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

            var fraction = s.State.MaxEnergy > 0 ? Mathf.Clamp01((float)s.State.Energy / s.State.MaxEnergy) : 0f;
            _energyFill.rectTransform.anchorMax = new Vector2(1f, fraction);
            _energyFill.color = fraction > 0.25f ? new Color(0.45f, 0.80f, 0.30f) : UiKit.Danger;
            _energyLabel.text = s.State.Energy.ToString();

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
