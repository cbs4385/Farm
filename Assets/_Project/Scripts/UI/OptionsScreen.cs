using System;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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
        GameObject _followed;
        InputActionRebindingExtensions.RebindingOperation _rebind;
        bool _rebinding;
        IDisposable _padWait;
        public const string KeyPrefix = "Key_", PadPrefix = "Pad_", AltPrefix = "Alt_", DpadPrefix = "Dpad_";          // the names of the rebind buttons (the tests find them)

        public OptionsScreen(UiService ui) : base(ui)
        {
            _store = ui.Game.Settings;
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
            var content = UiKit.VStack(scroll, "Content", 16f, 6);          // room between the lines (playtest 2026-10-13: the labels were too close together)
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

        // Keep the gamepad/keyboard-selected row inside the scroll viewport. Only when the selection moves: following it every frame pulled the list back
        // whenever the mouse wheel scrolled the clicked row out of view (playtest 2026-10-14: "the wheel stops moving down at spots").
        public override void Tick()
        {
            var selected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
            if (selected == _followed) return;
            _followed = selected;
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
            var audio = Ui.Game.Audio;

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

            var hudRow = Row(L.Get("options.hud_transparency"));
            var hudSlider = UiKit.MakeSlider(hudRow, s.HudTransparency / 0.6f, v => s.HudTransparency = v * 0.6f);
            hudSlider.name = "HudTransparency";

            // Content intensity (X-009): off, mild or full. Takes effect at once; saved with the other settings.
            Section(L.Get("options.content"));
            var horror = Row(L.Get("options.horror"));
            Button horrorButton = null;
            horrorButton = UiKit.MakeButton(horror, L.Get("options.horror." + s.HorrorLevel), () =>
            {
                s.HorrorLevel = (s.HorrorLevel + 1) % 3;
                UiKit.SetButtonText(horrorButton, L.Get("options.horror." + s.HorrorLevel));
            }, 220f, 30f);
            horrorButton.name = "HorrorLevel";
            var notes = Row(L.Get("options.content_notes"));
            UiKit.MakeButton(notes, L.Get("options.show"), () => Ui.ShowMessage("content.notes"), 220f, 30f).name = "ContentNotes";

            Section(L.Get("options.access"));
            // The font (any installed font) and the colour of ordinary text; both restyle every window at once.
            var fontRow = Row(L.Get("options.font"));
            var fonts = UiTextStyle.FontChoices();
            var fontAt = Mathf.Max(0, System.Array.IndexOf(fonts, s.FontName));
            Button fontName = null;
            string FontLabel(int i) => i == 0 ? L.Get("options.font.default") : fonts[i];
            void ChooseFont(int step)
            {
                fontAt = (fontAt + step + fonts.Length) % fonts.Length;
                s.FontName = fonts[fontAt];
                UiKit.SetButtonText(fontName, FontLabel(fontAt));
                Ui.ApplyTextStyle(s);
            }
            UiKit.MakeButton(fontRow, L.Get("options.font.previous"), () => ChooseFont(-1), 40f, 30f).name = "FontPrevious";
            fontName = UiKit.MakeButton(fontRow, FontLabel(fontAt), () => ChooseFont(0), 220f - 100f, 30f);
            fontName.name = "FontName";
            UiKit.MakeButton(fontRow, L.Get("options.font.next"), () => ChooseFont(1), 40f, 30f).name = "FontNext";
            TextColorPicker("options.title_color", UiKit.Accent, () => s.TitleColor, v => s.TitleColor = v, s);
            TextColorPicker("options.body_color", UiKit.DefaultTextColor, () => s.BodyColor, v => s.BodyColor = v, s);
            var cb = Row(L.Get("options.colorblind"));
            UiKit.MakeToggle(cb, L.Get("ui.on"), s.ColorblindPalette, on => s.ColorblindPalette = on, 220f);
            var calm = Row(L.Get("options.reduce_flashes"));
            UiKit.MakeToggle(calm, L.Get("ui.on"), s.ReduceFlashes, on => s.ReduceFlashes = on, 220f);
            var speed = Row(L.Get("options.dialogue_speed"));
            Button speedButton = null;
            speedButton = UiKit.MakeButton(speed, L.Get("options.dialogue_speed." + s.DialogueSpeed), () =>
            {
                s.DialogueSpeed = (s.DialogueSpeed + 1) % 4;
                UiKit.SetButtonText(speedButton, L.Get("options.dialogue_speed." + s.DialogueSpeed));
            }, 220f, 30f);
            speedButton.name = "DialogueSpeed";
            var stream = Row(L.Get("options.stream_mode"));
            UiKit.MakeToggle(stream, L.Get("ui.on"), s.StreamMode, on => s.StreamMode = on, 220f).name = "StreamMode";
            var timer = Row(L.Get("options.choice_timer"));
            Button timerButton = null;
            timerButton = UiKit.MakeButton(timer, ChoiceTimerLabel(s.ChoiceTimer), () =>
            {
                var steps = SettingsData.ChoiceTimerSteps;
                var next = steps[(System.Array.IndexOf(steps, s.ChoiceTimer) + 1) % steps.Length];
                s.ChoiceTimer = next;
                UiKit.SetButtonText(timerButton, ChoiceTimerLabel(next));
            }, 220f, 30f);
            timerButton.name = "ChoiceTimer";
            var blips = Row(L.Get("options.voice_blips"));
            UiKit.MakeToggle(blips, L.Get("ui.on"), s.VoiceBlips, on => s.VoiceBlips = on, 220f).name = "VoiceBlips";
            var barksRow = Row(L.Get("options.barks"));
            UiKit.MakeToggle(barksRow, L.Get("ui.on"), s.Barks, on => s.Barks = on, 220f).name = "Barks";
            var chat = Row(L.Get("options.chat_menu"));
            UiKit.MakeToggle(chat, L.Get("ui.on"), s.ChatMenu, on => s.ChatMenu = on, 220f).name = "ChatMenu";
            var auto = Row(L.Get("options.auto_advance"));
            UiKit.MakeToggle(auto, L.Get("ui.on"), s.AutoAdvance, on => s.AutoAdvance = on, 220f).name = "AutoAdvance";

            Section(L.Get("options.gameplay"));
            var hoverRow = Row(L.Get("options.hover_labels"));
            UiKit.MakeToggle(hoverRow, L.Get("ui.on"), s.HoverLabels, on => s.HoverLabels = on, 220f).name = "HoverLabels";
            var tipsRow = Row(L.Get("options.tutorial_tips"));
            UiKit.MakeToggle(tipsRow, L.Get("ui.on"), s.TutorialTips, on => s.TutorialTips = on, 220f).name = "TutorialTips";
            var trackerRow = Row(L.Get("options.quest_tracker"));
            UiKit.MakeToggle(trackerRow, L.Get("ui.on"), s.QuestTracker, on => s.QuestTracker = on, 220f).name = "QuestTracker";
            var aim = Row(L.Get("options.mouse_aim"));
            UiKit.MakeToggle(aim, L.Get("ui.on"), s.MouseAim, on => s.MouseAim = on, 220f).name = "MouseAim";
            var easy = Row(L.Get("options.relaxed_energy"));
            UiKit.MakeToggle(easy, L.Get("ui.on"), s.RelaxedEnergy, on => s.RelaxedEnergy = on, 220f);
            var dl = Row(L.Get("options.day_length"));
            Button dlButton = null;
            dlButton = UiKit.MakeButton(dl, L.Get("options.day_length." + s.DayLength), () =>
            {
                s.DayLength = (s.DayLength + 1) % SettingsData.DayLengthCount;
                UiKit.SetButtonText(dlButton, L.Get("options.day_length." + s.DayLength));
                Ui.Game.Session?.ApplySettings();
            }, 220f, 30f);
            dlButton.name = "DayLength";

            var lang = Row(L.Get("options.language"));
            UiKit.Label(lang, L.Get("language.en"), 18f, TextAlignmentOptions.Left, UiKit.DimText);

            Section(L.Get("options.controls"));
            Section(L.Get("options.keys"));
            foreach (var name in InputNames.Rebindable) KeyRow(L.Get("action." + name), Ui.Input.Gameplay[name], -1);
            var move = Ui.Input.Move;
            var composite = move.bindings.ToList().FindIndex(x => x.isComposite);
            for (var i = 0; i < InputNames.MoveParts.Length && composite >= 0; i++) KeyRow(L.Get("action.Move" + InputNames.MoveParts[i]), move, composite + 1 + i);
            for (var i = 0; i < InputNames.HotbarSlots; i++) KeyRow(L.Get("action.Hotbar", i + 1), Ui.Input.Hotbar(i), -1);
            foreach (var name in InputNames.UiRebindable) KeyRow(L.Get("action." + name), Ui.Input.Ui[name], -1);
            AlternateRows();

            Section(L.Get("options.pad_buttons"));
            var hint = UiKit.Label(_content, L.Get("options.pad_hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 40f);
            foreach (var name in InputNames.PadRebindable) PadRow(L.Get("options.pad_row", L.Get("action." + name)), Ui.Input.Gameplay[name]);
            foreach (var name in InputNames.UiPadRebindable) PadRow(L.Get("options.pad_row", L.Get("action." + name)), Ui.Input.Ui[name]);
            DpadRows();

            var resetPad = UiKit.MakeButton(_content, L.Get("options.reset_pad"), () =>
            {
                Ui.Input.ResetPadBindings();
                s.BindingOverridesJson = Ui.Input.ExportOverrides();
                Ui.Game.Input.Hints.NotifyChanged();
                Build();
                Ui.FocusFirst(Root);
            }, 260f, 30f);
            resetPad.name = "ResetPadBindings";
            var reset = UiKit.MakeButton(_content, L.Get("options.reset_bindings"), () =>
            {
                Ui.Input.ResetBindings();
                s.BindingOverridesJson = string.Empty;
                Ui.Game.Input.Hints.NotifyChanged();
                Build();
                Ui.FocusFirst(Root);
            }, 260f, 30f);
            reset.name = "ResetBindings";
        }

        static string ChoiceTimerLabel(int seconds) => seconds <= 0 ? L.Get("ui.off") : L.Get("options.choice_timer.seconds", seconds);

        void Section(string title)
        {
            var label = UiKit.Label(_content, title, 20f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(label.gameObject, -1f, 38f);
        }

        RectTransform Row(string label)
        {
            var row = UiKit.HStack(_content, label, 10f);
            UiKit.Size(row.gameObject, -1f, 36f);
            var text = UiKit.Label(row.transform, label, 18f);
            UiKit.Size(text.gameObject, 200f, 34f);
            return (RectTransform)row.transform;
        }

        // A colour picker for one kind of text: a swatch and red, green and blue sliders, and a button that returns to the game's colour.
        void TextColorPicker(string labelKey, Color fallback, Func<string> get, Action<string> set, SettingsData settings)
        {
            var start = UiTextStyle.Parse(get()) ?? fallback;
            var head = Row(L.Get(labelKey));
            var swatch = UiKit.Panel(head, "Swatch", start);
            UiKit.Size(swatch.gameObject, 220f, 26f);
            var sliders = new Slider[3];
            var building = true;
            void Changed()
            {
                if (building) return;
                var c = new Color(sliders[0].value, sliders[1].value, sliders[2].value);
                swatch.color = c;
                set(UiTextStyle.ToHex(c));
                Ui.ApplyTextStyle(settings);
            }
            var keys = new[] { "options.color.red", "options.color.green", "options.color.blue" };
            var values = new[] { start.r, start.g, start.b };
            for (var i = 0; i < 3; i++)
            {
                sliders[i] = UiKit.MakeSlider(Row(L.Get(keys[i])), values[i], _ => Changed());
                sliders[i].name = labelKey + "." + i;
            }
            building = false;
            var reset = Row(string.Empty);
            UiKit.MakeButton(reset, L.Get("options.color.reset"), () =>
            {
                set(string.Empty);
                building = true;
                sliders[0].SetValueWithoutNotify(fallback.r);
                sliders[1].SetValueWithoutNotify(fallback.g);
                sliders[2].SetValueWithoutNotify(fallback.b);
                building = false;
                swatch.color = fallback;
                Ui.ApplyTextStyle(settings);
            }, 220f, 30f).name = labelKey + ".reset";
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

        // One keyboard key: the action's first keyboard binding, or the binding at `index` (a part of the walking keys).
        void KeyRow(string label, InputAction action, int index)
        {
            var row = Row(label);
            if (index < 0) index = KeyboardBindingIndex(action);
            var at = index;
            Button button = null;
            button = UiKit.MakeButton(row, BindingText(action, at), () => StartRebind(action, at, button), 220f, 30f);
            button.name = KeyPrefix + action.name + "_" + at;
            _shown.Add((action.actionMap.name, action.name, at));
        }

        // The keys and mouse buttons that have a row above are not listed again.
        readonly System.Collections.Generic.HashSet<(string map, string action, int index)> _shown = new System.Collections.Generic.HashSet<(string, string, int)>();

        // The pointer itself is not a button: it has no row.
        static readonly System.Collections.Generic.HashSet<string> PointerOnly = new System.Collections.Generic.HashSet<string> { InputNames.Point, InputNames.Click, InputNames.RightClick, InputNames.ScrollWheel };

        static string ActionLabel(InputAction action) => L.Has("action." + action.name) ? L.Get("action." + action.name) : action.name;

        // Every other key and mouse button of the game (the second walking keys, Tab beside I, the mouse buttons, the keys of the menus...), one row each, so that nothing is fixed.
        void AlternateRows()
        {
            Section(L.Get("options.alternates"));
            foreach (var map in new[] { Ui.Input.Gameplay, Ui.Input.Ui })
                foreach (var action in map.actions)
                {
                    if (PointerOnly.Contains(action.name)) continue;
                    for (var i = 0; i < action.bindings.Count; i++)
                    {
                        var b = action.bindings[i];
                        if (b.isComposite || b.path == null || !(b.path.StartsWith("<Keyboard>") || b.path.StartsWith("<Mouse>"))) continue;
                        if (_shown.Contains((map.name, action.name, i))) continue;
                        var label = b.isPartOfComposite ? L.Get("options.alt_part", ActionLabel(action), b.name) : L.Get("options.alt", ActionLabel(action));
                        var row = Row(label);
                        var at = i;
                        var captured = action;
                        Button button = null;
                        button = UiKit.MakeButton(row, BindingText(action, at), () => StartRebind(captured, at, button), 220f, 30f);
                        button.name = AltPrefix + action.actionMap.name + "_" + action.name + "_" + at;
                    }
                }
        }

        // The D-pad's four directions (walking and the menus) can be moved to other buttons; the sticks are analog and stay what they are.
        void DpadRows()
        {
            foreach (var action in new[] { Ui.Input.Move, Ui.Input.Ui[InputNames.Navigate] })
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var b = action.bindings[i];
                    if (!b.isPartOfComposite || b.path == null || !b.path.StartsWith("<Gamepad>/dpad")) continue;
                    var row = Row(L.Get("options.dpad_part", ActionLabel(action), b.name));
                    var at = i;
                    var captured = action;
                    Button button = null;
                    button = UiKit.MakeButton(row, BindingNames.Describe(action, at), () => StartDpadRebind(captured, at, button), 220f, 30f);
                    button.name = DpadPrefix + action.actionMap.name + "_" + action.name + "_" + at;
                }
        }

        void StartDpadRebind(InputAction action, int index, Button button)
        {
            if (_rebinding) return;
            _rebinding = true;
            UiKit.SetButtonText(button, L.Get("options.press_pad"));
            _padWait = InputSystem.onAnyButtonPress.Call(control =>
            {
                if (!_rebinding) return;
                var path = control.device is Gamepad ? BindingNames.PadPathOf(control) : null;          // a key or the mouse cancels
                if (path != null)
                {
                    var before = action.bindings[index].effectivePath;
                    action.ApplyBindingOverride(index, path);
                    var swapped = BindingNames.SwapIfClash(action, index, before);
                    if (swapped != null) Ui.Session?.Toast(L.Get("options.swapped", ActionLabel(action), ActionLabel(swapped)));
                    _store.Current.BindingOverridesJson = Ui.Input.ExportOverrides();
                    Ui.Game.Input.Hints.NotifyChanged();
                }
                FinishRebind();
                UiKit.SetButtonText(button, BindingNames.Describe(action, index));
            });
        }

        static int KeyboardBindingIndex(InputAction action)
        {
            for (var i = 0; i < action.bindings.Count; i++)
                if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite && action.bindings[i].path.StartsWith("<Keyboard>")) return i;
            return 0;
        }

        static string BindingText(InputAction action, int index) => action.GetBindingDisplayString(index);

        void StartRebind(InputAction action, int index, Button button)
        {
            if (_rebinding) return;
            _rebinding = true;
            action.Disable();
            var before = action.bindings[index].effectivePath;
            UiKit.SetButtonText(button, L.Get("options.press_key"));

            _rebind = action.PerformInteractiveRebinding(index)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithControlsHavingToMatchPath("<Mouse>")          // a mouse button can be a key's partner or replace it
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(op =>
                {
                    FinishRebind();
                    if (action.actionMap.enabled) action.Enable();
                    var swapped = BindingNames.SwapIfClash(action, index, before);
                    _store.Current.BindingOverridesJson = Ui.Input.ExportOverrides();
                    UiKit.SetButtonText(button, BindingText(action, index));
                    Ui.Game.Input.Hints.NotifyChanged();
                    if (swapped != null) { Ui.Session?.Toast(L.Get("options.swapped", L.Get("action." + action.name), L.Get("action." + swapped.name))); Build(); }
                })
                .OnCancel(op =>
                {
                    FinishRebind();
                    if (action.actionMap.enabled) action.Enable();
                    UiKit.SetButtonText(button, BindingText(action, index));
                });
            _rebind.Start();
        }

        // ---- the controller's buttons (any pad: an Xbox style one, or a generic one the system sees as a joystick) ----

        static string PadText(InputAction action)
        {
            var pad = BindingNames.IndexOf(action, BindingNames.Gamepad);
            var text = pad >= 0 ? BindingNames.Describe(action, pad) : L.Get("options.pad_none");
            var joy = BindingNames.IndexOf(action, BindingNames.Joystick);
            if (BindingNames.IsOverridden(action, joy)) text += " / " + BindingNames.Describe(action, joy);
            return text;
        }

        void PadRow(string label, InputAction action)
        {
            if (BindingNames.IndexOf(action, BindingNames.Gamepad) < 0) return;
            var row = Row(label);
            Button button = null;
            button = UiKit.MakeButton(row, PadText(action), () => StartPadRebind(action, button), 220f, 30f);
            button.name = PadPrefix + action.actionMap.name + "_" + action.name;
        }

        void StartPadRebind(InputAction action, Button button)
        {
            if (_rebinding) return;
            _rebinding = true;
            UiKit.SetButtonText(button, L.Get("options.press_pad"));
            _padWait = InputSystem.onAnyButtonPress.Call(control =>
            {
                if (!_rebinding) return;
                // A key or the mouse cancels; a button on any pad becomes the action's.
                var family = control.device is Gamepad ? BindingNames.Gamepad : BindingNames.Joystick;
                var at = BindingNames.IndexOf(action, family);
                var before = at >= 0 ? action.bindings[at].effectivePath : null;
                if (BindingNames.RebindPad(action, control))
                {
                    var swapped = BindingNames.SwapIfClash(action, at, before);
                    if (swapped != null) Ui.Session?.Toast(L.Get("options.swapped", L.Get("action." + action.name), L.Get("action." + swapped.name)));
                    _store.Current.BindingOverridesJson = Ui.Input.ExportOverrides();
                    Ui.Game.Input.Hints.NotifyChanged();
                }
                FinishRebind();
                UiKit.SetButtonText(button, PadText(action));
            });
        }

        void FinishRebind()
        {
            _rebind?.Dispose();
            _rebind = null;
            _padWait?.Dispose();
            _padWait = null;
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
            _padWait?.Dispose();
            _padWait = null;
            _rebinding = false;
        }
    }
}
