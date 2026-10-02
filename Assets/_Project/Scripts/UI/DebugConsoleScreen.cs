#if UNITY_EDITOR || DEVELOPMENT_BUILD
// Developer console (task T-043). Compiled only in the Editor and development builds; see DebugCommandProcessor.
using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.UI
{
    // Opened with F1 (or the backtick key). Type a command and press Enter; Up/Down recall earlier commands.
    // Developer-only text, deliberately not localized.
    public sealed class DebugConsoleScreen : UiScreen
    {
        const string TitleText = "Developer console  (F1 to close, 'help' for commands)";
        const string PlaceholderText = "type a command...";
        const int MaxOutputLines = 14;

        readonly DebugCommandProcessor _processor;
        readonly TMP_InputField _input;
        readonly TextMeshProUGUI _output;
        readonly List<string> _lines = new List<string>();
        readonly List<string> _history = new List<string>();
        int _historyIndex;

        public DebugConsoleScreen(UiService ui) : base(ui)
        {
            _processor = new DebugCommandProcessor(ui.Session);

            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "DebugConsole", new Vector2(800, 400), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 6f, 12);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, TitleText, 18f, TextAlignmentOptions.Left, UiKit.Accent);

            _output = UiKit.Label(stack.transform, string.Empty, 14f, TextAlignmentOptions.TopLeft);
            UiKit.Size(_output.gameObject, -1f, -1f, -1f, 1f);
            _input = UiKit.MakeInput(stack.transform, PlaceholderText, string.Empty, 200, 760f);
            _input.onSubmit.AddListener(text => Submit(text));
            root.SetActive(false);
        }

        public override void Open()
        {
            base.Open();
            _input.text = string.Empty;
            _input.ActivateInputField();
        }

        public override void Tick()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.upArrowKey.wasPressedThisFrame && _history.Count > 0)
            {
                _historyIndex = Mathf.Max(0, _historyIndex - 1);
                _input.text = _history[_historyIndex];
                _input.caretPosition = _input.text.Length;
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame && _history.Count > 0)
            {
                _historyIndex = Mathf.Min(_history.Count, _historyIndex + 1);
                _input.text = _historyIndex < _history.Count ? _history[_historyIndex] : string.Empty;
                _input.caretPosition = _input.text.Length;
            }
        }

        // Runs one command line. Public so tests can drive the console without typing.
        public DebugCommandResult Submit(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return DebugCommandResult.Success(string.Empty);

            if (_history.Count == 0 || _history[_history.Count - 1] != line) _history.Add(line);
            _historyIndex = _history.Count;
            AddLine("> " + line);

            var result = _processor.Execute(line);
            if (!string.IsNullOrEmpty(result.Message)) AddLine(result.Ok ? result.Message : "! " + result.Message);

            _input.text = string.Empty;
            if (IsOpen) _input.ActivateInputField();

            if (result.Ok && result.ReloadScene)
            {
                Close();
                ServiceLocator.Get<SceneLoader>().Load(Ui.Session.State.CurrentMap, 0.2f);
            }
            return result;
        }

        void AddLine(string text)
        {
            foreach (var line in text.Split('\n')) _lines.Add(line);
            if (_lines.Count > MaxOutputLines) _lines.RemoveRange(0, _lines.Count - MaxOutputLines);
            _output.text = string.Join("\n", _lines);
        }
    }
}
#endif
