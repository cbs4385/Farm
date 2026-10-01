using System;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farm.UI
{
    // Volume, display, text size and key rebinding. Changes apply immediately and are saved when the screen closes.
    public sealed class OptionsScreen : UiScreen
    {
        static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440),
        };

        readonly SettingsStore _store;
        readonly RectTransform _content;
        readonly ScrollRect _scroll;
        InputActionRebindingExtensions.RebindingOperation _rebind;
        bool _rebinding;

        public OptionsScreen(UiService ui) : base(ui)
        {
            _store = ServiceLocator.Get<SettingsStore>();
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Options", new Vector2(620, 470), out var root);
            Root = root;
            // Fill the canvas height (with margins) so the Back button is always on screen, whatever the window size.
            frame.anchorMin = new Vector2(0.5f, 0f);
            frame.anchorMax = new Vector2(0.5f, 1f);
            frame.offsetMin = new Vector2(-310f, 24f);
            frame.offsetMax = new Vector2(310f, -24f);

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("options.title"), 26f, TextAlignmentOptions.Left, UiKit.Accent);

            var scroll = UiKit.Rect("Scroll", stack.transform);
            UiKit.Size(scroll.gameObject, -1f, -1f, -1f, 1f);
            // Invisible but raycastable: without a Graphic here the wheel does nothing over blank space.
            var catcher = scroll.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            var rect = scroll.gameObject.AddComponent<ScrollRect>();
            scroll.gameObject.AddComponent<RectMask2D>();
            var content = UiKit.VStack(scroll, "Content", 6f, 4);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            rect.content = crt;
            rect.horizontal = false;
            rect.scrollSensitivity = 30f;
            rect.movementType = ScrollRect.MovementType.Clamped;
            rect.verticalScrollbar = UiKit.MakeScrollbar(scroll);
            rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            _content = crt;
            _scroll = rect;

            UiKit.MakeButton(stack.transform, L.Get("ui.back"), Close, 160f, 34f);
            root.SetActive(false);
        }

        public override void Open()
        {
            Build();
            base.Open();
        }

        public override void Close()
        {
            CancelRebind();
            _store.Save();
            base.Close();
        }

        public override void OnCancel()
        {
            if (_rebinding) return;
            Close();
        }

        // Keep the gamepad/keyboard-selected row inside the scroll viewport.
        public override void Tick()
        {
            var selected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
            if (selected == null || !selected.transform.IsChildOf(_content)) return;
            var viewport = (RectTransform)_scroll.transform;
            var item = (RectTransform)selected.transform;
            var corners = new Vector3[4];
            item.GetWorldCorners(corners);
            var view = new Vector3[4];
            viewport.GetWorldCorners(view);
            var overTop = corners[1].y - view[1].y;
            var underBottom = view[0].y - corners[0].y;
            if (overTop <= 0f && underBottom <= 0f) return;
            var contentHeight = _content.rect.height - viewport.rect.height;
            if (contentHeight <= 0f) return;
            var worldPerUnit = viewport.lossyScale.y;
            var delta = (overTop > 0f ? overTop : -underBottom) / worldPerUnit;
            _scroll.verticalNormalizedPosition = Mathf.Clamp01(_scroll.verticalNormalizedPosition + delta / contentHeight);
        }

        void Build()
        {
            UiKit.ClearChildren(_content);
            var s = _store.Current;
            var audio = ServiceLocator.TryGet<AudioService>(out var a) ? a : null;

            Section(L.Get("options.audio"));
            VolumeRow(L.Get("options.master"), s.MasterVolume, v => { s.MasterVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.music"), s.MusicVolume, v => { s.MusicVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.sfx"), s.SfxVolume, v => { s.SfxVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.ambience"), s.AmbienceVolume, v => { s.AmbienceVolume = v; audio?.ApplySettings(s); });

            Section(L.Get("options.display"));
            var res = Row(L.Get("options.resolution"));
            Button resButton = null;
            resButton = UiKit.MakeButton(res, $"{s.ResolutionWidth}x{s.ResolutionHeight}", () =>
            {
                var next = NextResolution(s.ResolutionWidth, s.ResolutionHeight);
                s.ResolutionWidth = next.x;
                s.ResolutionHeight = next.y;
                UiKit.SetButtonText(resButton, $"{next.x}x{next.y}");
                DisplaySettings.Apply(s);
            }, 220f, 30f);
            var fs = Row(L.Get("options.fullscreen"));
            UiKit.MakeToggle(fs, L.Get("ui.on"), s.Fullscreen, on => { s.Fullscreen = on; DisplaySettings.Apply(s); }, 220f);
            var vs = Row(L.Get("options.vsync"));
            UiKit.MakeToggle(vs, L.Get("ui.on"), s.VSync, on => { s.VSync = on; DisplaySettings.Apply(s); }, 220f);

            var ts = Row(L.Get("options.text_size"));
            var tsSlider = UiKit.MakeSlider(ts, Mathf.InverseLerp(0.75f, 1.5f, s.TextScale), v =>
            {
                s.TextScale = Mathf.Lerp(0.75f, 1.5f, v);
                Ui.ApplyUiScale(s.TextScale);
            });
            tsSlider.name = "TextScale";

            var lang = Row(L.Get("options.language"));
            UiKit.Label(lang, L.Get("language.en"), 18f, TextAlignmentOptions.Left, UiKit.DimText);

            Section(L.Get("options.controls"));
            foreach (var action in InputNames.Rebindable) RebindRow(action);
            var reset = UiKit.MakeButton(_content, L.Get("options.reset_bindings"), () =>
            {
                Ui.Input.ResetBindings();
                s.BindingOverridesJson = string.Empty;
                Build();
                Ui.FocusFirst(Root);
            }, 260f, 30f);
            reset.name = "ResetBindings";
        }

        void Section(string title)
        {
            var label = UiKit.Label(_content, title, 20f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(label.gameObject, -1f, 30f);
        }

        RectTransform Row(string label)
        {
            var row = UiKit.HStack(_content, label, 10f);
            UiKit.Size(row.gameObject, -1f, 32f);
            var text = UiKit.Label(row.transform, label, 18f);
            UiKit.Size(text.gameObject, 200f, 30f);
            return (RectTransform)row.transform;
        }

        void VolumeRow(string label, float value, Action<float> onChanged)
        {
            var row = Row(label);
            UiKit.MakeSlider(row, value, v => onChanged(v));
        }

        static Vector2Int NextResolution(int w, int h)
        {
            for (var i = 0; i < Resolutions.Length; i++)
                if (Resolutions[i].x == w && Resolutions[i].y == h) return Resolutions[(i + 1) % Resolutions.Length];
            return Resolutions[0];
        }

        // ---- rebinding -----------------------------------------------------------------------------------------

        void RebindRow(string actionName)
        {
            var row = Row(L.Get("action." + actionName));
            var action = Ui.Input.Gameplay[actionName];
            var index = KeyboardBindingIndex(action);
            Button button = null;
            button = UiKit.MakeButton(row, BindingText(action, index), () => StartRebind(action, index, button), 220f, 30f);
        }

        static int KeyboardBindingIndex(InputAction action)
        {
            for (var i = 0; i < action.bindings.Count; i++)
                if (!action.bindings[i].isComposite && action.bindings[i].path.StartsWith("<Keyboard>")) return i;
            return 0;
        }

        static string BindingText(InputAction action, int index) => action.GetBindingDisplayString(index);

        void StartRebind(InputAction action, int index, Button button)
        {
            if (_rebinding) return;
            _rebinding = true;
            action.Disable();
            UiKit.SetButtonText(button, L.Get("options.press_key"));

            _rebind = action.PerformInteractiveRebinding(index)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(op =>
                {
                    FinishRebind();
                    _store.Current.BindingOverridesJson = Ui.Input.ExportOverrides();
                    UiKit.SetButtonText(button, BindingText(action, index));
                })
                .OnCancel(op =>
                {
                    FinishRebind();
                    UiKit.SetButtonText(button, BindingText(action, index));
                });
            _rebind.Start();
        }

        void FinishRebind()
        {
            _rebind?.Dispose();
            _rebind = null;
            // Defer so the Escape that cancelled the rebind does not also close this screen.
            Ui.StartCoroutine(ClearRebindFlag());
        }

        System.Collections.IEnumerator ClearRebindFlag()
        {
            yield return null;
            _rebinding = false;
        }

        void CancelRebind()
        {
            _rebind?.Cancel();
            _rebind?.Dispose();
            _rebind = null;
            _rebinding = false;
        }
    }
}
