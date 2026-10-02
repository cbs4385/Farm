using System;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // The dialogue box (T-034): speaker portrait and name, text that types out, and choice buttons. Submit or a click
    // finishes the typing and then moves on; Cancel skips ahead to the next choice (running the effects on the way).
    public sealed class DialogueScreen : UiScreen
    {
        const float CharactersPerSecond = 90f;

        readonly Image _portrait;
        readonly TextMeshProUGUI _name;
        readonly TextMeshProUGUI _text;
        readonly RectTransform _choices;
        readonly GameObject _portraitBox;
        DialogueRunner _runner;
        Action _onClosed;
        float _revealed;

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

            var row = UiKit.HStack(frame.transform, "Row", 14f, TextAnchor.UpperLeft);
            UiKit.Stretch((RectTransform)row.transform, 12f);

            var box = UiKit.Panel(row.transform, "PortraitBox", UiKit.PanelLight);
            UiKit.Size(box.gameObject, 128f, 128f);
            _portraitBox = box.gameObject;
            _portrait = UiKit.Panel(box.transform, "Portrait", Color.white);
            UiKit.Stretch(_portrait.rectTransform, 4f);
            _portrait.preserveAspect = true;

            var column = UiKit.VStack(row.transform, "Column", 4f);
            UiKit.Size(column.gameObject, -1f, -1f, 1f, 1f);
            _name = UiKit.Label(column.transform, "", 20f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_name.gameObject, -1f, 26f);
            _text = UiKit.Label(column.transform, "", 20f);
            UiKit.Size(_text.gameObject, -1f, -1f, 1f, 1f);

            var choices = UiKit.VStack(column.transform, "Choices", 3f);
            _choices = (RectTransform)choices.transform;
            Root.SetActive(false);
        }

        public void OpenDialogue(DialogueRunner runner, Action onClosed)
        {
            _runner = runner;
            _onClosed = onClosed;
            if (runner.Finished) { Finish(); return; }
            Show();
            Open();
        }

        void Show()
        {
            var line = _runner.Current;
            var npc = !string.IsNullOrEmpty(line.Speaker) ? Ui.Session.Npcs.Get(line.Speaker) : null;
            _name.text = string.IsNullOrEmpty(line.Speaker) ? string.Empty : L.Get($"npc.{line.Speaker}.name");
            _portraitBox.SetActive(!string.IsNullOrEmpty(line.Speaker));
            _portrait.sprite = npc != null ? npc.Portrait : null;
            _portrait.color = npc != null && npc.Portrait != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            _text.text = line.Text;
            _text.fontStyle = string.IsNullOrEmpty(line.Speaker) ? FontStyles.Italic : FontStyles.Normal;
            _text.ForceMeshUpdate();
            _text.maxVisibleCharacters = 0;
            _revealed = 0f;
            RebuildChoices(showNow: false);
        }

        void RebuildChoices(bool showNow)
        {
            UiKit.ClearChildren(_choices);
            if (!showNow || _runner.Current == null) return;
            foreach (var option in _runner.Current.Options)
            {
                var index = option.Index;
                var button = UiKit.MakeButton(_choices, option.Text, () => Choose(OptionPosition(index)), 640f, 30f);
                button.name = "Choice" + option.Index;
            }
            Ui.FocusFirst(Root);
        }

        int OptionPosition(int choiceIndex)
        {
            var options = _runner.Current.Options;
            for (var i = 0; i < options.Count; i++) if (options[i].Index == choiceIndex) return i;
            return -1;
        }

        bool Revealing => _text.maxVisibleCharacters < _text.textInfo.characterCount;

        public override void Tick()
        {
            if (_runner == null || _runner.Current == null) return;
            if (_text.textInfo.characterCount == 0) _text.ForceMeshUpdate();
            var submit = Time.frameCount != OpenedFrame && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame();
            if (Revealing)
            {
                if (submit) { FinishTyping(); return; }
                _revealed += Time.unscaledDeltaTime * CharactersPerSecond;
                _text.maxVisibleCharacters = Mathf.Min((int)_revealed, _text.textInfo.characterCount);
                if (!Revealing) RebuildChoices(showNow: true);
                return;
            }
            if (_choices.childCount == 0 && _runner.Current.HasOptions) RebuildChoices(showNow: true);
            if (!_runner.Current.HasOptions && submit) Continue();
        }

        void OnClick()
        {
            if (_runner == null || _runner.Current == null || Time.frameCount == OpenedFrame) return;
            if (Revealing) { FinishTyping(); return; }
            if (!_runner.Current.HasOptions) Continue();
        }

        void FinishTyping()
        {
            _text.maxVisibleCharacters = int.MaxValue;
            _revealed = float.MaxValue;
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
            if (position < 0) return;
            _runner.Choose(position);
            AfterStep();
        }

        void AfterStep()
        {
            if (_runner.Finished) { Finish(); return; }
            Show();
        }

        // Escape: finish the typing, then skip ahead to the next choice (or the end), running effects on the way.
        public override void OnCancel()
        {
            if (_runner == null || _runner.Current == null) { Finish(); return; }
            if (Revealing) { FinishTyping(); return; }
            while (!_runner.Finished && _runner.Current != null && !_runner.Current.HasOptions) _runner.Advance();
            AfterStep();
        }

        void Finish()
        {
            var callback = _onClosed;
            _onClosed = null;
            _runner = null;
            Close();
            callback?.Invoke();
        }
    }
}
