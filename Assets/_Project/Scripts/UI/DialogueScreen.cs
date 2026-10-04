using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farm.UI
{
    // The dialogue box (T-034, T-096): speaker portrait (with expressions), name, text that types out at the player's speed
    // with {pause}/{speed} cues, an emote bubble, numbered choices with tone tags, optional auto-advance and a conversation
    // log. Submit or a click finishes the typing and then moves on; Cancel skips ahead to the next choice (running the
    // effects on the way); 1-9 pick a choice; L (or the previous-tab button) opens the log.
    public sealed class DialogueScreen : UiScreen
    {
        static readonly Dictionary<string, string> ToneColors = new Dictionary<string, string>
        {
            { "kind", "#9fd48a" }, { "honest", "#8ec5e6" }, { "playful", "#f0b46a" }, { "shy", "#d9a0d9" }, { "curt", "#c9a39a" },
        };

        readonly Image _portrait;
        readonly TextMeshProUGUI _name;
        readonly TextMeshProUGUI _text;
        readonly RectTransform _choices;
        readonly GameObject _portraitBox;
        readonly GameObject _emoteBubble;
        readonly TextMeshProUGUI _emoteText;
        readonly GameObject _logPanel;
        readonly TextMeshProUGUI _logText;
        readonly DialogueBacklog _backlog = new DialogueBacklog();
        DialogueRunner _runner;
        Action _onClosed;
        Typewriter _type;
        int _blippedAt;
        readonly ChoiceCountdown _countdown = new ChoiceCountdown();
        readonly TextMeshProUGUI _countdownLabel;
        float _autoTimer;
        bool _choicesShown;

        public DialogueScreen(UiService ui) : base(ui)
        {
            // A transparent full-screen layer: the world stays visible, and a click anywhere advances the text.
            var layer = UiKit.Panel(ui.ScreenCanvas.transform, "Dialogue", new Color(0f, 0f, 0f, 0.25f));
            UiKit.Stretch(layer.rectTransform);
            Root = layer.gameObject;
            // Not a Button: a Selectable would take the keyboard focus (and Submit) away from the choices.
            var clickAnywhere = Root.AddComponent<EventTrigger>();
            var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener(_ => OnClick());
            clickAnywhere.triggers.Add(click);

            var frame = UiKit.Panel(Root.transform, "Frame", UiKit.PanelColor);
            UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 200f), new Vector2(0f, 16f));
            frame.gameObject.AddComponent<FitToCanvas>();

            var row = UiKit.HStack(frame.transform, "Row", 14f, TextAnchor.UpperLeft);
            UiKit.Stretch((RectTransform)row.transform, 12f);

            var box = UiKit.Panel(row.transform, "PortraitBox", UiKit.PanelLight);
            UiKit.Size(box.gameObject, 128f, 128f);
            _portraitBox = box.gameObject;
            _portrait = UiKit.Panel(box.transform, "Portrait", Color.white);
            UiKit.Stretch(_portrait.rectTransform, 4f);
            _portrait.preserveAspect = true;

            var bubble = UiKit.Panel(box.transform, "Emote", UiKit.TextColor);
            UiKit.Place(bubble.rectTransform, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(46f, 34f), new Vector2(-6f, -6f));
            _emoteBubble = bubble.gameObject;
            _emoteText = UiKit.Label(bubble.transform, "", 20f, TextAlignmentOptions.Center, UiKit.PanelColor);
            UiKit.Stretch(_emoteText.rectTransform);
            _emoteBubble.SetActive(false);

            var column = UiKit.VStack(row.transform, "Column", 4f);
            UiKit.Size(column.gameObject, -1f, -1f, 1f, 1f);
            _name = UiKit.Label(column.transform, "", 20f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_name.gameObject, -1f, 26f);
            _text = UiKit.Label(column.transform, "", 20f);
            UiKit.Size(_text.gameObject, -1f, -1f, 1f, 1f);

            var choices = UiKit.VStack(column.transform, "Choices", 3f);
            _choices = (RectTransform)choices.transform;

            var hint = UiKit.Label(frame.transform, L.Get("dialogue.log_hint"), 13f, TextAlignmentOptions.Right, UiKit.DimText);
            UiKit.Place(hint.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(160f, 18f), new Vector2(-10f, -4f));
            hint.raycastTarget = false;
            _countdownLabel = UiKit.Label(frame.transform, "", 16f, TextAlignmentOptions.Right, UiKit.Accent);
            UiKit.Place(_countdownLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(260f, 20f), new Vector2(-10f, -24f));
            _countdownLabel.raycastTarget = false;
            _countdownLabel.gameObject.SetActive(false);

            // The conversation log: the last lines, newest at the bottom.
            var log = UiKit.Panel(Root.transform, "DialogueLog", new Color(0f, 0f, 0f, 0.82f));
            UiKit.Stretch(log.rectTransform);
            _logPanel = log.gameObject;
            var logFrame = UiKit.Panel(log.transform, "LogFrame", UiKit.PanelColor);
            UiKit.Place(logFrame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 400f), Vector2.zero);
            var title = UiKit.Label(logFrame.transform, L.Get("dialogue.log"), 22f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500f, 32f), new Vector2(16f, -10f));
            var body = UiKit.Panel(logFrame.transform, "LogBody", new Color(0f, 0f, 0f, 0f));
            UiKit.Stretch(body.rectTransform);
            body.rectTransform.offsetMin = new Vector2(16f, 40f);
            body.rectTransform.offsetMax = new Vector2(-16f, -48f);
            body.gameObject.AddComponent<RectMask2D>();
            _logText = UiKit.Label(body.transform, "", 18f, TextAlignmentOptions.BottomLeft);
            UiKit.Stretch(_logText.rectTransform);
            _logText.overflowMode = TextOverflowModes.Overflow;
            _logText.textWrappingMode = TextWrappingModes.Normal;
            var close = UiKit.Label(logFrame.transform, L.Get("dialogue.log_close"), 14f, TextAlignmentOptions.Right, UiKit.DimText);
            UiKit.Place(close.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(300f, 22f), new Vector2(-16f, 10f));
            _logPanel.SetActive(false);

            Root.SetActive(false);
        }

        static SettingsData Settings => ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;

        public void OpenDialogue(DialogueRunner runner, Action onClosed)
        {
            _runner = runner;
            _onClosed = onClosed;
            _backlog.Clear();
            _logPanel.SetActive(false);
            if (runner.Finished) { Finish(); return; }
            Show();
            Open();
        }

        void Show()
        {
            var line = _runner.Current;
            var npc = !string.IsNullOrEmpty(line.Speaker) ? Ui.Session.Npcs.Get(line.Speaker) : null;
            var speakerName = string.IsNullOrEmpty(line.Speaker) ? string.Empty : L.Get($"npc.{line.Speaker}.name");
            _name.text = speakerName;
            var accent = SpeakerStyle.AccentFor(line.Speaker);
            _name.color = accent;
            _portraitBox.GetComponent<Image>().color = Color.Lerp(UiKit.PanelLight, accent, 0.45f);
            _portraitBox.SetActive(!string.IsNullOrEmpty(line.Speaker));
            _portrait.sprite = npc != null ? npc.PortraitFor(line.Expression) : null;
            _portrait.color = npc != null && _portrait.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            _text.text = line.Text;
            _text.fontSize = 20f * UiKit.TextScale * (Settings != null ? Settings.DialogueTextFactor : 1f);
            _text.fontStyle = string.IsNullOrEmpty(line.Speaker) ? FontStyles.Italic : FontStyles.Normal;
            _text.maxVisibleCharacters = 0;
            ShowEmote(line.Emote);
            _backlog.Add(speakerName, Regex.Replace(line.Text, "<[^>]+>", string.Empty));
            _autoTimer = 0f;
            _choicesShown = false;
            _blippedAt = 0;
            _type = null;
            PlayLineSound(line);
            RebuildChoices(showNow: false);
            // The first line of a conversation is set up before the box is on screen, when the text has no layout yet:
            // typing starts on the first Tick (or at once if the box is already showing).
            if (Root.activeInHierarchy) BeginTyping();
        }

        // Lays the line out and starts the typewriter. TextMeshPro only knows the character count once it has a layout.
        void BeginTyping()
        {
            if (_runner == null || _runner.Current == null) return;
            _text.ForceMeshUpdate();
            var settings = Settings;
            _type = new Typewriter(_text.textInfo.characterCount, _runner.Current.Cues,
                DialoguePacing.CpsFor(settings != null ? settings.EffectiveDialogueSpeed : DialoguePacing.DefaultSpeed));
            _text.maxVisibleCharacters = _type.Done ? int.MaxValue : _type.Visible;
            RebuildChoices(showNow: _type.Done);
        }

        void ShowEmote(string emote)
        {
            var glyph = DialogueVocabulary.EmoteGlyph(emote);
            _emoteBubble.SetActive(glyph != null && _portraitBox.activeSelf);
            if (glyph != null) _emoteText.text = glyph;
        }

        void RebuildChoices(bool showNow)
        {
            UiKit.ClearChildren(_choices);
            _choicesShown = false;
            if (!showNow || _runner.Current == null || !_runner.Current.HasOptions) return;
            var options = _runner.Current.Options;
            Button highlighted = null;
            for (var position = 0; position < options.Count; position++)
            {
                var p = position;
                var option = options[position];
                var button = UiKit.MakeButton(_choices, ChoiceLabel(position, option), () => Choose(p), 640f, 30f);
                button.name = "Choice" + option.Index;
                if (option.IsDefault) highlighted = button;
            }
            _choicesShown = true;
            StartCountdown(options.Count);
            Ui.FocusFirst(Root);
            if (highlighted != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(highlighted.gameObject);
        }

        // A talking blip every few revealed characters, in the speaker's voice (T-132).
        void Blip()
        {
            var line = _runner?.Current;
            var settings = Settings;
            if (line == null || string.IsNullOrEmpty(line.Speaker) || (settings != null && !settings.VoiceBlips)) { _blippedAt = _type != null ? _type.Visible : 0; return; }
            var profile = VoiceProfiles.For(string.IsNullOrEmpty(line.Voice) ? line.Speaker : line.Voice);
            if (VoiceSynth.ShouldBlip(_blippedAt, _type.Visible, profile.CharsPerBlip))
            {
                if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.PlayVoice(string.IsNullOrEmpty(line.Voice) ? line.Speaker : line.Voice, _type.Visible);
            }
            _blippedAt = _type.Visible;
        }

        // A line can ask for a sound when it appears: "signature" is the speaker's own, otherwise a sound name.
        static void PlayLineSound(DialogueLine line)
        {
            if (string.IsNullOrEmpty(line.Sfx) || !ServiceLocator.TryGet<AudioService>(out var audio)) return;
            var settings = Settings;
            if (line.Sfx == "signature")
            {
                if (!string.IsNullOrEmpty(line.Speaker) && (settings == null || settings.VoiceBlips)) audio.PlaySignature(line.Speaker);
            }
            else if (System.Enum.TryParse<Sfx>(line.Sfx, true, out var sfx)) audio.Play(sfx);
        }

        // The optional chat-vote timer (T-145): shown while a real choice waits, and the default is taken when it runs out.
        void StartCountdown(int optionCount)
        {
            var settings = Settings;
            if (settings != null && settings.ChoiceTimer > 0 && optionCount >= 2) _countdown.Start(settings.ChoiceTimer);
            else _countdown.Stop();
            _countdownLabel.gameObject.SetActive(_countdown.Running);
            UpdateCountdownLabel();
        }

        void UpdateCountdownLabel()
        {
            if (_countdown.Running) _countdownLabel.text = L.Get("dialogue.timer", _countdown.SecondsLeft);
        }

        static string ChoiceLabel(int position, DialogueOption option)
        {
            var sb = new StringBuilder();
            if (position < 9) sb.Append(position + 1).Append(". ");
            sb.Append(option.Text);
            if (!string.IsNullOrEmpty(option.Tone) && ToneColors.TryGetValue(option.Tone, out var color))
                sb.Append("  <color=").Append(color).Append("><size=75%>(").Append(L.Get("dialogue.tone." + option.Tone)).Append(")</size></color>");
            return sb.ToString();
        }

        bool Revealing => _type != null && !_type.Done;

        public override void Tick()
        {
            if (_runner == null || _runner.Current == null) return;
            var keyboard = Keyboard.current;
            var ready = Time.frameCount != OpenedFrame;

            var logKey = ready && ((keyboard != null && keyboard.lKey.wasPressedThisFrame) || Ui.Input.Ui[InputNames.TabPrev].WasPressedThisFrame());
            if (_logPanel.activeSelf)
            {
                if (logKey || (ready && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame())) HideLog();
                return;
            }
            if (logKey) { ShowLog(); return; }

            if (_type == null) BeginTyping();
            var submit = ready && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame();
            if (Revealing)
            {
                if (submit) { FinishTyping(); return; }
                _type.Advance(Time.unscaledDeltaTime);
                _text.maxVisibleCharacters = _type.Done ? int.MaxValue : _type.Visible;
                Blip();
                if (_type.Done) RebuildChoices(showNow: true);
                return;
            }

            if (_runner.Current.HasOptions)
            {
                if (!_choicesShown) RebuildChoices(showNow: true);
                if (_countdown.Running)
                {
                    UpdateCountdownLabel();
                    if (_countdown.Tick(Time.unscaledDeltaTime))
                    {
                        _countdownLabel.gameObject.SetActive(false);
                        Choose(ChoiceCountdown.Choose(_runner.Current.Options));
                        return;
                    }
                }
                if (ready && keyboard != null)
                {
                    var count = Math.Min(9, _runner.Current.Options.Count);
                    for (var i = 0; i < count; i++)
                        if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame) { Choose(i); return; }
                }
                return;
            }

            if (submit) { Continue(); return; }
            var settings = Settings;
            if (settings != null && settings.AutoAdvance)
            {
                _autoTimer += Time.unscaledDeltaTime;
                if (_autoTimer >= DialoguePacing.AutoAdvanceSeconds(_text.textInfo.characterCount)) Continue();
            }
        }

        void OnClick()
        {
            if (_runner == null || _runner.Current == null || Time.frameCount == OpenedFrame) return;
            if (_logPanel.activeSelf) { HideLog(); return; }
            if (_type == null) BeginTyping();
            if (Revealing) { FinishTyping(); return; }
            if (!_runner.Current.HasOptions) Continue();
        }

        void FinishTyping()
        {
            _type?.Finish();
            _text.maxVisibleCharacters = int.MaxValue;
            RebuildChoices(showNow: true);
        }

        void Continue()
        {
            if (_runner.Current.HasOptions) return;
            _runner.Advance();
            AfterStep();
        }

        void Choose(int position)
        {
            if (position < 0 || _runner.Current == null || position >= _runner.Current.Options.Count) return;
            _backlog.Add(string.Empty, L.Get("dialogue.you") + ": " + Regex.Replace(_runner.Current.Options[position].Text, "<[^>]+>", string.Empty));
            _runner.Choose(position);
            AfterStep();
        }

        void AfterStep()
        {
            _countdown.Stop();
            _countdownLabel.gameObject.SetActive(false);
            if (_runner.Finished) { Finish(); return; }
            Show();
        }

        // Escape: close the log if it is open; otherwise finish the typing, then skip ahead to the next choice (or the
        // end), running effects on the way.
        public override void OnCancel()
        {
            if (_logPanel.activeSelf) { HideLog(); return; }
            if (_runner == null || _runner.Current == null) { Finish(); return; }
            if (_type == null) BeginTyping();
            if (Revealing) { FinishTyping(); return; }
            if (_runner.Current.HasOptions)
            {
                // A menu with a default choice (Goodbye, Back): Escape picks it.
                var fallback = _runner.Current.Options.FindIndex(o => o.IsDefault);
                if (fallback >= 0) { Choose(fallback); return; }
            }
            while (!_runner.Finished && _runner.Current != null && !_runner.Current.HasOptions) _runner.Advance();
            AfterStep();
        }

        void ShowLog()
        {
            var sb = new StringBuilder();
            foreach (var entry in _backlog.Entries)
            {
                if (sb.Length > 0) sb.Append('\n');
                if (!string.IsNullOrEmpty(entry.Speaker)) sb.Append("<color=#f3bf4d>").Append(entry.Speaker).Append("</color>  ");
                sb.Append(entry.Text);
            }
            _logText.text = sb.ToString();
            _logPanel.SetActive(true);
        }

        void HideLog() => _logPanel.SetActive(false);

        void Finish()
        {
            var callback = _onClosed;
            _onClosed = null;
            _runner = null;
            _type = null;
            _logPanel.SetActive(false);
            _emoteBubble.SetActive(false);
            Close();
            callback?.Invoke();
        }
    }
}
