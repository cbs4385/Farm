using System;
using System.Text;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // "Report a bug" (main menu and pause menu): a subject, a description, an optional copy of the save; pressing the button packs the game's logs
    // and system details into a zip (BugReport) and opens the player's mail program addressed to the developers. The screen then says where the
    // file is so that it can be attached. Nothing leaves the computer until the player sends the mail.
    public sealed class BugReportScreen : UiScreen
    {
        readonly TMP_InputField _subject, _body;
        readonly Toggle _includeSave;
        readonly TextMeshProUGUI _status, _doneText;
        readonly GameObject _form, _done;
        BugReportService.Outcome _result;

        public string LastFileName { get; private set; }

        public BugReportScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "BugReport", new Vector2(680f, 580f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("bug.title"), 26f, TextAlignmentOptions.Left, UiKit.Accent);

            _form = UiKit.VStack(stack.transform, "Form", 8f).gameObject;
            UiKit.Size(_form, -1f, -1f, 1f, 1f);
            UiKit.Label(_form.transform, L.Get("bug.explain", BugReport.Address), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Label(_form.transform, L.Get("bug.subject"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _subject = UiKit.MakeInput(_form.transform, L.Get("bug.subject_hint"), string.Empty, BugReport.MaxSubject, 640f);
            UiKit.Label(_form.transform, L.Get("bug.body"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _body = UiKit.MakeInput(_form.transform, L.Get("bug.body_hint"), string.Empty, BugReport.MaxBody, 640f);
            _body.lineType = TMP_InputField.LineType.MultiLineNewline;
            _body.textComponent.alignment = TextAlignmentOptions.TopLeft;
            if (_body.placeholder is TMP_Text ph) ph.alignment = TextAlignmentOptions.TopLeft;
            UiKit.Size(_body.gameObject, 640f, 190f);
            _includeSave = UiKit.MakeToggle(_form.transform, L.Get("bug.include_save"), true, null, 640f);
            _status = UiKit.Label(_form.transform, string.Empty, 15f, TextAlignmentOptions.Left, UiKit.Danger);
            var row = UiKit.HStack(_form.transform, "Buttons", 12f, TextAnchor.MiddleCenter);
            UiKit.MakeButton(row.transform, L.Get("bug.create"), Send, 300f, 38f).name = "Send";
            UiKit.MakeButton(row.transform, L.Get("ui.cancel"), Close, 160f, 38f).name = "Cancel";

            _done = UiKit.VStack(stack.transform, "Done", 10f).gameObject;
            UiKit.Size(_done, -1f, -1f, 1f, 1f);
            _doneText = UiKit.Label(_done.transform, string.Empty, 17f, TextAlignmentOptions.Left);
            UiKit.Size(_doneText.gameObject, -1f, -1f, 1f, 1f);
            var doneRow = UiKit.HStack(_done.transform, "DoneButtons", 12f, TextAnchor.MiddleCenter);
            UiKit.MakeButton(doneRow.transform, L.Get("bug.open_folder"), () => BugReportService.OpenFolder(_result), 260f, 38f).name = "OpenFolder";
            UiKit.MakeButton(doneRow.transform, L.Get("ui.close"), Close, 160f, 38f).name = "Close";
            root.SetActive(false);
        }

        public override void Open()
        {
            var inGame = Ui.Session != null && Ui.Session.InGame;
            _subject.text = string.Empty;
            _body.text = string.Empty;
            _status.text = string.Empty;
            _includeSave.SetIsOnWithoutNotify(inGame);
            _includeSave.gameObject.SetActive(inGame);
            _form.SetActive(true);
            _done.SetActive(false);
            base.Open();
            _subject.ActivateInputField();
        }

        // Fills in a report without typing (tests and tools).
        public void Fill(string subject, string body, bool includeSave)
        {
            _subject.text = subject;
            _body.text = body;
            _includeSave.SetIsOnWithoutNotify(includeSave);
        }

        public void Send()
        {
            var problem = BugReport.Validate(_subject.text, _body.text);
            if (problem != null) { _status.text = L.Get(problem); return; }

            var session = Ui.Session != null && Ui.Session.InGame ? Ui.Session : null;
            var result = BugReportService.Send(session, _subject.text, _body.text, session != null && _includeSave.isOn);
            if (!result.Ok) { _status.text = L.Get(result.Error ?? "bug.failed"); return; }

            _result = result;
            LastFileName = result.FileName;
            _doneText.text = L.Get("bug.done", BugReport.Address, result.FileName);
            _form.SetActive(false);
            _done.SetActive(true);
        }
    }
}
