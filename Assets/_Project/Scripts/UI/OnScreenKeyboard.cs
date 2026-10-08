using System.Collections.Generic;
using Farm.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farm.UI
{
    // A keyboard for a pad (an Xbox controller can move a cursor over keys, but cannot type): the stick or the D-pad moves between the keys, A presses one,
    // X deletes the last letter, Y puts in a space, Menu is Done and B backs out without keeping the changes. It opens when a text field is chosen with
    // A while the player is using a pad (OnScreenKeyboardOpener), edits the text in step so the field behind always shows it, and gives the field back
    // when it is done. Capital letters at the start of a word are automatic; the Shift key changes the case of the next letter.
    public sealed class OnScreenKeyboard : UiScreen
    {
        static readonly string[] Rows = { "1234567890", "qwertyuiop", "asdfghjkl'", "zxcvbnm-.,!" };

        readonly TextMeshProUGUI _display;
        readonly List<Button> _letters = new List<Button>();
        readonly List<TextMeshProUGUI> _letterText = new List<TextMeshProUGUI>();
        readonly TextMeshProUGUI _shiftLabel;
        TMP_InputField _field;
        string _original;
        bool _shift, _capsNext;

        public string Text { get; private set; } = string.Empty;
        public bool Capitals => _shift != _capsNext;

        public OnScreenKeyboard(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "OnScreenKeyboard", new Vector2(640, 420), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 14, TextAnchor.UpperCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            var box = UiKit.Panel(stack.transform, "Entry", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Size(box.gameObject, -1f, 44f);
            _display = UiKit.Label(box.transform, "", 24f, TextAlignmentOptions.Left);
            UiKit.Stretch(_display.rectTransform, 10f);

            foreach (var row in Rows)
            {
                var line = UiKit.HStack(stack.transform, "Row", 6f, TextAnchor.MiddleCenter);
                line.childForceExpandWidth = false;
                UiKit.Size(line.gameObject, -1f, 44f);
                foreach (var ch in row)
                {
                    var key = ch;
                    var button = UiKit.MakeButton(line.transform, key.ToString(), () => Type(key), 48f, 42f);
                    button.name = "Key_" + key;
                    _letters.Add(button);
                    _letterText.Add(button.GetComponentInChildren<TextMeshProUGUI>());
                }
            }

            var bottom = UiKit.HStack(stack.transform, "Bottom", 6f, TextAnchor.MiddleCenter);
            bottom.childForceExpandWidth = false;
            UiKit.Size(bottom.gameObject, -1f, 46f);
            var shift = UiKit.MakeButton(bottom.transform, L.Get("osk.shift"), () => { _shift = !_shift; ShowCase(); }, 90f, 42f);
            shift.name = "Key_Shift";
            _shiftLabel = shift.GetComponentInChildren<TextMeshProUGUI>();
            UiKit.MakeButton(bottom.transform, L.Get("osk.space"), () => Type(' '), 170f, 42f).name = "Key_Space";
            UiKit.MakeButton(bottom.transform, L.Get("osk.delete"), Backspace, 100f, 42f).name = "Key_Delete";
            UiKit.MakeButton(bottom.transform, L.Get("osk.done"), Done, 100f, 42f).name = "Key_Done";
            UiKit.MakeButton(bottom.transform, L.Get("ui.back"), OnCancel, 100f, 42f).name = "Key_Cancel";
            UiKit.Label(stack.transform, L.Get("osk.hint"), 14f, TextAlignmentOptions.Center, UiKit.DimText);
            root.SetActive(false);
        }

        public void OpenFor(TMP_InputField field)
        {
            _field = field;
            _original = field.text;
            Text = field.text ?? string.Empty;
            _shift = false;
            _capsNext = Text.Length == 0;
            Refresh();
            Open();
            FocusKey('q');
        }

        void FocusKey(char key)
        {
            var button = _letters.Find(b => b.name == "Key_" + key);
            if (button != null && UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        // Types one character: a letter takes the case that is showing; the first letter of a word is capital unless Shift was used to turn that round.
        public void Type(char ch)
        {
            if (_field == null || Text.Length >= _field.characterLimit && _field.characterLimit > 0) return;
            if (char.IsLetter(ch) && Capitals) ch = char.ToUpperInvariant(ch);
            Text += ch;
            _shift = false;
            _capsNext = ch == ' ' || ch == '-' || ch == '.' || ch == '!';
            Refresh();
        }

        public void Backspace()
        {
            if (Text.Length == 0) return;
            Text = Text.Substring(0, Text.Length - 1);
            _shift = false;
            _capsNext = Text.Length == 0 || Text[Text.Length - 1] == ' ';
            Refresh();
        }

        void Refresh()
        {
            _field.text = Text;
            _display.text = Text + "_";
            ShowCase();
        }

        void ShowCase()
        {
            for (var i = 0; i < _letterText.Count; i++)
            {
                var c = _letters[i].name[4];
                _letterText[i].text = char.IsLetter(c) && Capitals ? char.ToUpperInvariant(c).ToString() : c.ToString();
            }
            if (_shiftLabel != null) _shiftLabel.color = _shift ? UiKit.Accent : UiKit.TextColor;
        }

        // Keeps what was typed and gives the text field back.
        public void Done()
        {
            if (_field != null) _field.text = Text;
            Finish();
        }

        // B: leaves the text as it was when the keyboard opened.
        public override void OnCancel()
        {
            if (_field != null) _field.text = _original;
            Finish();
        }

        void Finish()
        {
            var field = _field;
            _field = null;
            Close();
            if (field != null && UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(field.gameObject);
        }

        // The pad's shortcuts, so a name can be typed without leaving the letters: X deletes, Y spaces, Menu is Done.
        public override void Tick()
        {
            if (_field != null && _field.isFocused) _field.DeactivateInputField();          // choosing the field also starts key entry in it: not while this is open
            var pad = Gamepad.current;
            if (pad == null || Time.frameCount == OpenedFrame) return;
            if (pad.buttonWest.wasPressedThisFrame) Backspace();
            else if (pad.buttonNorth.wasPressedThisFrame) Type(' ');
            else if (pad.startButton.wasPressedThisFrame) Done();
        }
    }

    // On a text field: choosing it with A while a pad is in use opens the on-screen keyboard instead of waiting for keys that a pad does not have.
    public sealed class OnScreenKeyboardOpener : MonoBehaviour, UnityEngine.EventSystems.ISubmitHandler
    {
        static OnScreenKeyboard _keyboard;

        public static void Reset() => _keyboard = null;

        public void OnSubmit(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (ControlPrompts.Kind != InputKind.Gamepad) return;
            var field = GetComponent<TMP_InputField>();
            if (field == null || !ServiceLocator.TryGet<UiService>(out var ui)) return;
            field.DeactivateInputField();
            if (_keyboard == null || _keyboard.Root == null) _keyboard = new OnScreenKeyboard(ui);
            _keyboard.OpenFor(field);
        }
    }
}
