using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Farm.UI
{
    // The settings, as a list of categories on the left and one short page of rows on the right (sound, display, accessibility, dialogue, gameplay, keyboard and mouse,
    // controller, content and language). Changes apply immediately and are saved when the screen closes.
    //   - Every control is one stop in a fixed chain (OptionsNav): up and down go to the row above or below, left and right change a slider, a choice or a toggle, and walk
    //     along a row of keys. Nothing is chosen by what happens to be nearest on screen (the first version's long list did that, and a pad could land on a row scrolled out
    //     of view or on the Back button; playtest 2026-10-16, controller only).
    //   - The shoulder buttons (Q and E on the keyboard) change the page from anywhere, and the page is remembered, so the controller page is one press away.
    //   - The focused row is lit and kept in view with a row of room on each side; the right stick scrolls and, if that leaves the focus off screen, brings it back on.
    //   - Waiting for a key or a controller button gives up after a few seconds, so a player with only a pad (or only a keyboard) is never stuck in it.
    public sealed class OptionsScreen : UiScreen
    {
        public static readonly string[] Groups = { "sound", "display", "accessibility", "dialogue", "gameplay", "keys", "pad", "content" };

        public const string CategoryPrefix = "Category_";
        public const float RebindSeconds = 5f;
        public const string KeyPrefix = "Key_", PadPrefix = "Pad_", AltPrefix = "Alt_", DpadPrefix = "Dpad_";          // the names of the rebind buttons (the tests find them)

        static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440),
        };

        const float CellWidth = 150f, LabelWidth = 230f, StickDead = 0.25f;

        readonly SettingsStore _store;
        readonly RectTransform _content;
        readonly ScrollRect _scroll;
        readonly TextMeshProUGUI _pageTitle, _position;
        readonly List<Button> _categories = new List<Button>();
        readonly List<List<Selectable>> _rows = new List<List<Selectable>>();          // the controls of the page showing, a list per row
        readonly List<Transform> _rowRoots = new List<Transform>();
        readonly List<Image> _rowSkins = new List<Image>();                            // behind each row that is one (null for a lone button)
        readonly Vector3[] _corners = new Vector3[4], _view = new Vector3[4];
        string _page = Groups[0];                                                      // kept while the game runs
        string _positionShown;
        InputActionRebindingExtensions.RebindingOperation _rebind;
        bool _rebinding;
        int _timeoutToken;
        IDisposable _padWait;

        public OptionsScreen(UiService ui) : base(ui)
        {
            _store = ui.Game.Settings;
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Options", new Vector2(820, 470), out var root);
            Root = root;
            // Fill the canvas height (with margins) so the foot of the screen is always on screen, whatever the window size.
            frame.anchorMin = new Vector2(0.5f, 0f);
            frame.anchorMax = new Vector2(0.5f, 1f);
            frame.offsetMin = new Vector2(-410f, 24f);
            frame.offsetMax = new Vector2(410f, -24f);

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("options.title"), 26f, TextAlignmentOptions.Left, UiKit.Accent);

            var body = UiKit.HStack(stack.transform, "Body", 12f, TextAnchor.UpperLeft);
            body.childForceExpandHeight = true;
            body.childControlWidth = true;
            body.childForceExpandWidth = false;          // the list keeps its width and the page takes the rest
            UiKit.Size(body.gameObject, -1f, -1f, 1f, 1f);

            // The categories, and Back under them (the mouse's way out; the pad and the keyboard close with B and Esc, so it is outside the focus chain).
            var list = UiKit.VStack(body.transform, "Categories", 6f);
            UiKit.Size(list.gameObject, 190f, -1f, 0f, 1f);
            foreach (var id in Groups)
            {
                var captured = id;
                var button = UiKit.MakeButton(list.transform, L.Get("options.group." + id), () => SelectPage(captured, true), -1f, 34f);
                button.name = CategoryPrefix + id;
                _categories.Add(button);
            }
            var spacer = UiKit.Rect("Spacer", list.transform);
            UiKit.Size(spacer.gameObject, -1f, -1f, -1f, 1f);
            var back = UiKit.MakeButton(list.transform, L.Get("ui.back"), Close, -1f, 34f);
            back.navigation = new Navigation { mode = Navigation.Mode.None };

            var page = UiKit.VStack(body.transform, "Page", 6f);
            UiKit.Size(page.gameObject, -1f, -1f, 1f, 1f);
            _pageTitle = UiKit.Label(page.transform, string.Empty, 22f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_pageTitle.gameObject, -1f, 30f);
            var scroll = UiKit.Scroll(page.transform, "Scroll", out _content);
            UiKit.Size(scroll.gameObject, -1f, -1f, 1f, 1f);
            _scroll = scroll.GetComponent<ScrollRect>();
            scroll.GetComponent<ScrollFollowSelection>().Margin = 44f;
            _content.GetComponent<VerticalLayoutGroup>().spacing = 6f;

            var foot = UiKit.HStack(stack.transform, "Foot", 10f);
            UiKit.Size(foot.gameObject, -1f, 26f);
            var hint = UiKit.Label(foot.transform, L.Get("options.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 26f, 1f);
            _position = UiKit.Label(foot.transform, string.Empty, 15f, TextAlignmentOptions.Right, UiKit.DimText);
            UiKit.Size(_position.gameObject, 80f, 26f);
            root.SetActive(false);
        }

        public string CurrentPage => _page;

        // Where the pad cursor starts: the first control of the page.
        public override GameObject InitialFocus => _rows.Count > 0 ? _rows[0][0].gameObject : null;

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

        // Shows the page of a category by id (see Groups; "language" is on the content page), and builds the screen again if it is showing.
        public void Expand(params string[] ids)
        {
            if (ids == null || ids.Length == 0) return;
            _page = ids[0] == "language" ? "content" : ids[0];
            if (Root != null && Root.activeSelf) { Build(); Ui.FocusScreen(this); }
        }

        public bool IsOpen(string id) => _page == id;

        void SelectPage(string id, bool focus)
        {
            if (id != _page) AudioService.PlayIfAvailable(Sfx.PageTurn);
            _page = id;
            Build();
            if (focus) Ui.FocusScreen(this);
        }

        void StepPage(int by)
        {
            var at = Array.IndexOf(Groups, _page);
            SelectPage(Groups[(at + by + Groups.Length) % Groups.Length], true);
        }

        public override void Tick()
        {
            if (Time.frameCount == OpenedFrame) return;
            if (!_rebinding)
            {
                var input = Ui.Input.Ui;
                if (input[InputNames.TabNext].WasPressedThisFrame()) StepPage(1);
                else if (input[InputNames.TabPrev].WasPressedThisFrame()) StepPage(-1);
            }
            ShowFocus();
            BringFocusOnScreen();
        }

        // ---- the focus ----

        // The row that holds the focus is lit, and the footer says which row of the page it is.
        void ShowFocus()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            var at = -1;
            for (var i = 0; i < _rowRoots.Count; i++)
            {
                var on = selected != null && selected.transform.IsChildOf(_rowRoots[i]);
                if (on) at = i;
                if (_rowSkins[i] != null) _rowSkins[i].color = on ? new Color(UiKit.Accent.r, UiKit.Accent.g, UiKit.Accent.b, 0.22f) : Color.clear;
            }
            var text = at < 0 ? string.Empty : $"{at + 1} / {_rows.Count}";
            if (text == _positionShown) return;
            _positionShown = text;
            _position.text = text;
        }

        // The right stick scrolls the list on its own: if that leaves the focused control off screen, the focus moves to the nearest row that is on screen, so the next
        // press of up or down goes on from what the player is looking at.
        void BringFocusOnScreen()
        {
            var pad = Gamepad.current;
            if (pad == null || Mathf.Abs(pad.rightStick.ReadValue().y) < StickDead) return;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null || !selected.transform.IsChildOf(_content)) return;
            var viewport = (RectTransform)_scroll.transform;
            viewport.GetWorldCorners(_view);
            var item = (RectTransform)selected.transform;
            if (Visible(item)) return;
            item.GetWorldCorners(_corners);
            var above = _corners[0].y > _view[1].y;          // scrolled past the top of the page: take the first control showing; past the foot: the last
            var showing = _rows.SelectMany(r => r).Where(c => Visible((RectTransform)c.transform)).ToList();
            if (showing.Count > 0) Select(above ? showing[0] : showing[showing.Count - 1]);
        }

        static void Select(Selectable s) => EventSystem.current.SetSelectedGameObject(s.gameObject);

        bool Visible(RectTransform item)
        {
            item.GetWorldCorners(_corners);
            return _corners[0].y >= _view[0].y - 0.5f && _corners[1].y <= _view[1].y + 0.5f;
        }

        // ---- building ----

        void Build()
        {
            UiKit.ClearChildren(_content);
            _rows.Clear();
            _rowRoots.Clear();
            _rowSkins.Clear();
            _positionShown = null;
            BuildPage(_page);
            _pageTitle.text = L.Get("options.group." + _page);
            for (var i = 0; i < _categories.Count; i++)
                ((Image)_categories[i].targetGraphic).color = Groups[i] == _page ? new Color(1f, 0.85f, 0.45f) : Color.white;
            UiTextStyle.Refresh(_content);
            Wire();
            _scroll.verticalNormalizedPosition = 1f;
        }

        // Collects the controls row by row and chains them (OptionsNav).
        void Wire()
        {
            foreach (Transform child in _content)
            {
                if (!child.gameObject.activeSelf) continue;
                var cells = child.GetComponentsInChildren<Selectable>(false).Where(s => s.IsInteractable() && s.navigation.mode != Navigation.Mode.None).ToList();
                if (cells.Count == 0) continue;
                _rows.Add(cells);
                _rowRoots.Add(child);
                _rowSkins.Add(child.GetComponent<Selectable>() == null ? child.GetComponent<Image>() : null);
            }
            var current = _categories[Array.IndexOf(Groups, _page)];
            OptionsNav.Wire(_rows, current);
            OptionsNav.WireList(_categories.Cast<Selectable>().ToList(), _rows.Count > 0 ? _rows[0][0] : null);
        }

        void BuildPage(string id)
        {
            var s = _store.Current;
            switch (id)
            {
                case "sound": SoundPage(s); break;
                case "display": DisplayPage(s); break;
                case "accessibility": AccessibilityPage(s); break;
                case "dialogue": DialoguePage(s); break;
                case "gameplay": GameplayPage(s); break;
                case "keys": KeysPage(s); break;
                case "pad": PadPage(s); break;
                default: ContentPage(s); break;
            }
        }

        void SoundPage(SettingsData s)
        {
            var audio = Ui.Game.Audio;
            VolumeRow(L.Get("options.master"), s.MasterVolume, v => { s.MasterVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.music"), s.MusicVolume, v => { s.MusicVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.sfx"), s.SfxVolume, v => { s.SfxVolume = v; audio?.ApplySettings(s); });
            VolumeRow(L.Get("options.ambience"), s.AmbienceVolume, v => { s.AmbienceVolume = v; audio?.ApplySettings(s); });
        }

        void DisplayPage(SettingsData s)
        {
            Choice(Row(L.Get("options.resolution")), "Resolution", Resolutions.Length,
                () => Mathf.Max(0, Array.FindIndex(Resolutions, r => r.x == s.ResolutionWidth && r.y == s.ResolutionHeight)),
                i => { s.ResolutionWidth = Resolutions[i].x; s.ResolutionHeight = Resolutions[i].y; DisplaySettings.Apply(s); },
                i => $"{Resolutions[i].x}x{Resolutions[i].y}");
            ToggleRow(L.Get("options.fullscreen"), s.Fullscreen, on => { s.Fullscreen = on; DisplaySettings.Apply(s); }, "Fullscreen");
            ToggleRow(L.Get("options.vsync"), s.VSync, on => { s.VSync = on; DisplaySettings.Apply(s); }, "VSync");

            var tsSlider = UiKit.MakeSlider(Row(L.Get("options.text_size")), Mathf.InverseLerp(0.75f, 1.5f, s.TextScale), v =>
            {
                s.TextScale = Mathf.Lerp(0.75f, 1.5f, v);
                Ui.ApplyUiScale(s.TextScale);
            });
            tsSlider.name = "TextScale";

            var hudSlider = UiKit.MakeSlider(Row(L.Get("options.hud_transparency")), s.HudTransparency / 0.6f, v => s.HudTransparency = v * 0.6f);
            hudSlider.name = "HudTransparency";
        }

        void AccessibilityPage(SettingsData s)
        {
            // The font (any installed font) and the colour of ordinary text; both restyle every window at once.
            var fontRow = Row(L.Get("options.font"));
            var fonts = UiTextStyle.FontChoices();
            var fontAt = Mathf.Max(0, Array.IndexOf(fonts, s.FontName));
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
            ToggleRow(L.Get("options.colorblind"), s.ColorblindPalette, on => s.ColorblindPalette = on, "Colorblind");
            ToggleRow(L.Get("options.reduce_flashes"), s.ReduceFlashes, on => s.ReduceFlashes = on, "ReduceFlashes");
            ToggleRow(L.Get("options.stream_mode"), s.StreamMode, on => s.StreamMode = on, "StreamMode");
        }

        void DialoguePage(SettingsData s)
        {
            Choice(Row(L.Get("options.dialogue_speed")), "DialogueSpeed", 4, () => s.DialogueSpeed, i => s.DialogueSpeed = i, i => L.Get("options.dialogue_speed." + i));
            var steps = SettingsData.ChoiceTimerSteps;
            Choice(Row(L.Get("options.choice_timer")), "ChoiceTimer", steps.Length, () => Mathf.Max(0, Array.IndexOf(steps, s.ChoiceTimer)), i => s.ChoiceTimer = steps[i], i => ChoiceTimerLabel(steps[i]));
            ToggleRow(L.Get("options.voice_blips"), s.VoiceBlips, on => s.VoiceBlips = on, "VoiceBlips");
            ToggleRow(L.Get("options.barks"), s.Barks, on => s.Barks = on, "Barks");
            ToggleRow(L.Get("options.chat_menu"), s.ChatMenu, on => s.ChatMenu = on, "ChatMenu");
            ToggleRow(L.Get("options.auto_advance"), s.AutoAdvance, on => s.AutoAdvance = on, "AutoAdvance");
        }

        void GameplayPage(SettingsData s)
        {
            ToggleRow(L.Get("options.hover_labels"), s.HoverLabels, on => s.HoverLabels = on, "HoverLabels");
            ToggleRow(L.Get("options.tutorial_tips"), s.TutorialTips, on => s.TutorialTips = on, "TutorialTips");
            ToggleRow(L.Get("options.quest_tracker"), s.QuestTracker, on => s.QuestTracker = on, "QuestTracker");
            ToggleRow(L.Get("options.mouse_aim"), s.MouseAim, on => s.MouseAim = on, "MouseAim");
            ToggleRow(L.Get("options.relaxed_energy"), s.RelaxedEnergy, on => s.RelaxedEnergy = on, "RelaxedEnergy");
            Choice(Row(L.Get("options.day_length")), "DayLength", SettingsData.DayLengthCount, () => s.DayLength, i => { s.DayLength = i; Ui.Game.Session?.ApplySettings(); },
                i => L.Get("options.day_length." + i));
        }

        // Content intensity (X-009): off, mild or full. Takes effect at once; saved with the other settings. The language is on this page too (there is only English).
        void ContentPage(SettingsData s)
        {
            Choice(Row(L.Get("options.horror")), "HorrorLevel", 3, () => s.HorrorLevel, i => s.HorrorLevel = i, i => L.Get("options.horror." + i));
            UiKit.MakeButton(Row(L.Get("options.content_notes")), L.Get("options.show"), () => Ui.ShowMessage("content.notes"), 220f, 30f).name = "ContentNotes";
            UiKit.Label(Row(L.Get("options.language")), L.Get("language.en"), 18f, TextAlignmentOptions.Left, UiKit.DimText);
        }

        // Keyboard and mouse: one table, an action on each row with its primary and its secondary key.
        void KeysPage(SettingsData s)
        {
            var mouseSens = Row(L.Get("options.mouse_sensitivity"));          // how far the pointer travels in the world for a movement of the mouse
            UiKit.MakeSlider(mouseSens, Mathf.InverseLerp(SettingsData.MinMouseSensitivity, SettingsData.MaxMouseSensitivity, s.MouseSensitivity),
                v => s.MouseSensitivity = Mathf.Lerp(SettingsData.MinMouseSensitivity, SettingsData.MaxMouseSensitivity, v)).name = "MouseSensitivity";
            TableHeader(L.Get("options.col.action"), L.Get("options.col.primary"), L.Get("options.col.secondary"));
            KeyTable.Group? shown = null;
            foreach (var spec in KeyTable.Rows(Ui.Input))
            {
                if (shown != spec.Group) { SubHeading(L.Get("options.keys." + spec.Group)); shown = spec.Group; }
                KeyRow(spec);
            }
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

        // The controller: its sensitivity is the first row of the page, so it is the first thing the cursor is on when the page opens.
        void PadPage(SettingsData s)
        {
            var padSens = Row(L.Get("options.controller_sensitivity"));          // how far a tilt of the stick goes toward full walking speed
            UiKit.MakeSlider(padSens, Mathf.InverseLerp(SettingsData.MinControllerSensitivity, SettingsData.MaxControllerSensitivity, s.ControllerSensitivity),
                v => s.ControllerSensitivity = Mathf.Lerp(SettingsData.MinControllerSensitivity, SettingsData.MaxControllerSensitivity, v)).name = "ControllerSensitivity";
            var hint = UiKit.Label(_content, L.Get("options.pad_hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 40f);
            TableHeader(L.Get("options.col.action"), L.Get("options.col.button"), null);
            foreach (var name in InputNames.PadRebindable) PadRow(Ui.Input.Gameplay[name]);
            foreach (var name in InputNames.UiPadRebindable) PadRow(Ui.Input.Ui[name]);
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
        }

        static string ChoiceTimerLabel(int seconds) => seconds <= 0 ? L.Get("ui.off") : L.Get("options.choice_timer.seconds", seconds);

        void SubHeading(string title)
        {
            var label = UiKit.Label(_content, title, 17f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(label.gameObject, -1f, 30f);
        }

        // The column titles of a table (the third is left out in a two column one).
        void TableHeader(string action, string first, string second)
        {
            var row = UiKit.HStack(_content, "Header", 10f);
            UiKit.Size(row.gameObject, -1f, 28f);
            UiKit.Size(UiKit.Label(row.transform, action, 16f, TextAlignmentOptions.Left, UiKit.DimText).gameObject, LabelWidth, 26f);
            UiKit.Size(UiKit.Label(row.transform, first, 16f, TextAlignmentOptions.Center, UiKit.DimText).gameObject, CellWidth, 26f);
            if (second != null) UiKit.Size(UiKit.Label(row.transform, second, 16f, TextAlignmentOptions.Center, UiKit.DimText).gameObject, CellWidth, 26f);
        }

        // A row is named after `label` (the tests find rows by that name); `shown` is the text when it should differ. It has a backdrop that lights up while the focus is in it.
        RectTransform Row(string label, string shown = null)
        {
            var row = UiKit.HStack(_content, label, 10f);
            UiKit.Size(row.gameObject, -1f, 38f);
            var skin = row.gameObject.AddComponent<Image>();
            skin.color = Color.clear;
            skin.raycastTarget = false;
            var text = UiKit.Label(row.transform, shown ?? label, 18f);
            text.enableAutoSizing = true;          // a long name gets smaller rather than a third line over the next row
            text.fontSizeMin = 12f;
            text.fontSizeMax = 18f;
            UiKit.Size(text.gameObject, LabelWidth, 36f);
            return (RectTransform)row.transform;
        }

        // A button that cycles through `count` choices: a click goes to the next, left and right go back and forward.
        Button Choice(RectTransform row, string name, int count, Func<int> get, Action<int> set, Func<int, string> label)
        {
            Button button = null;
            void Apply(int index)
            {
                set(((index % count) + count) % count);
                UiKit.SetButtonText(button, label(get()));
            }
            button = UiKit.MakeButton(row, label(get()), () => Apply(get() + 1), 220f, 30f);
            button.name = name;
            button.gameObject.AddComponent<ValueStepper>().Step = by => Apply(get() + by);
            return button;
        }

        // A labelled toggle: A flips it, right turns it on and left turns it off.
        Toggle ToggleRow(string label, bool value, Action<bool> set, string name)
        {
            var toggle = UiKit.MakeToggle(Row(label), L.Get("ui.on"), value, on => set(on), 220f);
            toggle.name = name;
            toggle.gameObject.AddComponent<ValueStepper>().Step = by => toggle.isOn = by > 0;
            return toggle;
        }

        // The game's fixed text colours: white, black, then the farmer creator's shirt colours (so the reds are here too).
        static readonly string[] TextColorPresets = new[] { "ffffff", "000000" }.Concat(AvatarOptions.ShirtColors.Select(h => h.Substring(1))).ToArray();

        // A colour picker for one kind of text: a swatch and red, green and blue sliders, a row of preset colours and a button that returns to the game's colour.
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
            // The fixed colours are presets: a click moves the three sliders to that colour.
            var presets = Row(string.Empty);
            presets.GetComponent<HorizontalLayoutGroup>().spacing = 4f;          // seventeen swatches in a row must fit the page
            foreach (var hex in TextColorPresets)
            {
                var c = UiTextStyle.Parse(hex) ?? Color.white;
                UiKit.MakeSwatch(presets, c, () =>
                {
                    building = true;
                    sliders[0].SetValueWithoutNotify(c.r); sliders[1].SetValueWithoutNotify(c.g); sliders[2].SetValueWithoutNotify(c.b);
                    building = false;
                    Changed();
                }, out _, 22f).name = labelKey + ".preset." + hex;
            }
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

        // ---- rebinding -----------------------------------------------------------------------------------------

        static string ActionLabel(InputAction action) => L.Has("action." + action.name) ? L.Get("action." + action.name) : action.name;

        // One row of the key table: the action, then a rebind button for its primary and for its secondary key (a dimmed dash when it has no second one).
        void KeyRow(KeyTable.Spec spec)
        {
            var row = Row(spec.Label);
            var action = spec.Action;
            Cell(row, action, spec.Primary, KeyPrefix + action.name + "_" + spec.Primary);
            if (spec.Secondary >= 0) Cell(row, action, spec.Secondary, AltPrefix + action.actionMap.name + "_" + action.name + "_" + spec.Secondary);
            else UiKit.Size(UiKit.Label(row, L.Get("options.none_key"), 18f, TextAlignmentOptions.Center, UiKit.DimText).gameObject, CellWidth, 30f);
        }

        void Cell(RectTransform row, InputAction action, int index, string buttonName)
        {
            Button button = null;
            button = UiKit.MakeButton(row, BindingText(action, index), () => StartRebind(action, index, button), CellWidth, 30f);
            button.name = buttonName;
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
                    button = UiKit.MakeButton(row, BindingNames.Describe(action, at), () => StartDpadRebind(captured, at, button), CellWidth, 30f);
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
            GiveUpAfterAWhile(() => UiKit.SetButtonText(button, BindingNames.Describe(action, index)));
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
            GiveUpAfterAWhile(() => _rebind?.Cancel());          // its OnCancel puts the button's text back
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

        // The row keeps the long name (the tests find it by that) and shows the action's own.
        void PadRow(InputAction action)
        {
            if (BindingNames.IndexOf(action, BindingNames.Gamepad) < 0) return;
            var row = Row(L.Get("options.pad_row", ActionLabel(action)), ActionLabel(action));
            Button button = null;
            button = UiKit.MakeButton(row, PadText(action), () => StartPadRebind(action, button), CellWidth, 30f);
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
            GiveUpAfterAWhile(() => UiKit.SetButtonText(button, PadText(action)));
        }

        // Waiting for a key or a button ends by itself after RebindSeconds with nothing changed: any button of a pad can be the new binding, so B cannot be the way out, and
        // a player with only a pad has no Esc. `restore` puts the button's text back.
        void GiveUpAfterAWhile(Action restore) => Ui.StartCoroutine(Timeout(++_timeoutToken, restore));

        System.Collections.IEnumerator Timeout(int token, Action restore)
        {
            var end = Time.unscaledTime + RebindSeconds;
            while (Time.unscaledTime < end)
            {
                if (token != _timeoutToken) yield break;
                yield return null;
            }
            if (token != _timeoutToken || !_rebinding) yield break;
            if (_rebind != null) { restore(); yield break; }          // a key wait: cancelling it puts the text back and finishes it
            CancelRebind();
            restore();
        }

        void FinishRebind()
        {
            _timeoutToken++;
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
            _timeoutToken++;
            _rebind?.Cancel();
            _rebind?.Dispose();
            _rebind = null;
            _padWait?.Dispose();
            _padWait = null;
            _rebinding = false;
        }
    }
}
