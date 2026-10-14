using System;
using Farm.Core;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // A small window that asks for a name (a farm animal's): the field starts with the current name, Enter or the button keeps what is typed.
    public sealed class NamePromptScreen : UiScreen
    {
        public const int MaxLength = 16;
        readonly TextMeshProUGUI _title, _status;
        readonly TMP_InputField _input;
        Action<string> _onDone;
        string _current;

        public NamePromptScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "NamePrompt", new Vector2(460f, 250f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, string.Empty, 22f, TextAlignmentOptions.Left, UiKit.Accent);
            _input = UiKit.MakeInput(stack.transform, L.Get("name.hint"), string.Empty, MaxLength, 400f);
            _status = UiKit.Label(stack.transform, string.Empty, 15f, TextAlignmentOptions.Left, UiKit.Danger);
            var row = UiKit.HStack(stack.transform, "Buttons", 12f, TextAnchor.MiddleCenter);
            UiKit.MakeButton(row.transform, L.Get("ui.ok"), Accept, 160f, 38f).name = "Ok";
            UiKit.MakeButton(row.transform, L.Get("name.keep"), Close, 200f, 38f).name = "Keep";
            _input.onSubmit.AddListener(_ => Accept());
            root.SetActive(false);
        }

        public void Ask(string title, string current, Action<string> onDone)
        {
            _title.text = title;
            _current = current;
            _onDone = onDone;
            _input.text = current;
            _status.text = string.Empty;
            Open();
            _input.ActivateInputField();
        }

        // Fills in and accepts without typing (tests and tools).
        public void Submit(string name) { _input.text = name; Accept(); }

        public void Accept()
        {
            var text = _input.text.Trim();
            if (text.Length == 0) { _status.text = L.Get("name.empty"); return; }
            if (!NameFilter.IsAllowed(text)) { _status.text = L.Get("newgame.name_blocked"); return; }
            var done = _onDone;
            _onDone = null;
            Close();
            if (text != _current) done?.Invoke(text);
        }
    }
}
