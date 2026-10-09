using System;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // A letter from the mailbox: sender, subject and text. Closing it runs the letter's effects.
    public sealed class LetterScreen : UiScreen
    {
        readonly TextMeshProUGUI _head, _body;
        Action _onClose;

        public LetterScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Letter", new Vector2(620f, 380f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 18);
            UiKit.Stretch((RectTransform)stack.transform);
            _head = UiKit.Label(stack.transform, "", 22f, TextAlignmentOptions.Left, UiKit.Accent);
            _body = UiKit.Label(stack.transform, "", 19f);
            UiKit.Size(_body.gameObject, -1f, -1f, 1f, 1f);
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenLetter(LetterDefinition letter, Action onClose)
        {
            var from = string.IsNullOrEmpty(letter.Sender) ? string.Empty : "  (" + L.Get("letter.from", L.Get($"npc.{letter.Sender}.name")) + ")";
            _head.text = Ui.Session.StoryText(letter.SubjectKey, new object[0]) + from;
            _body.text = Ui.Session.StoryText(letter.BodyKey, new object[0]);
            _onClose = onClose;
            Open();
        }

        public override void Close()
        {
            var cb = _onClose;
            _onClose = null;
            base.Close();
            cb?.Invoke();
        }
    }

    // The help-wanted board: jobs with their reward, accepted and delivered with buttons.
    public sealed class BoardScreen : UiScreen
    {
        readonly RectTransform _list;

        public BoardScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Board", new Vector2(640f, 380f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("board.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            _list = (RectTransform)list.transform;
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenBoard()
        {
            Rebuild();
            Open();
        }

        void Rebuild()
        {
            var s = Ui.Session;
            UiKit.ClearChildren(_list);
            if (s.State.Board.Count == 0) UiKit.Label(_list, L.Get("board.empty"), 18f, TextAlignmentOptions.Center, UiKit.DimText);
            foreach (var job in s.State.Board.ToList())
            {
                var row = UiKit.HStack(_list, job.Id, 8f);
                UiKit.Size(row.gameObject, -1f, 40f);
                var name = s.Db.TryGetItem(job.ItemId, out var item) ? L.Get(item.NameKey) : job.ItemId;
                var text = UiKit.Label(row.transform, L.Get("board.job", job.Count, name), 18f);
                UiKit.Size(text.gameObject, -1f, 34f, 1f);
                var days = Math.Max(0, job.ExpiresDay - s.Clock.Now.TotalDays);
                UiKit.Label(row.transform, L.Get("board.reward", job.Reward) + " - " + L.Get("board.expires", days), 15f, TextAlignmentOptions.Right, UiKit.Accent);
                var captured = job;
                if (!job.Accepted) UiKit.MakeButton(row.transform, L.Get("board.accept"), () => { HelpWanted.Accept(s, captured); Rebuild(); }, 90f, 32f).name = "Accept";
                var deliver = UiKit.MakeButton(row.transform, L.Get("board.deliver"), () => { HelpWanted.Deliver(s, captured); Rebuild(); }, 90f, 32f);
                deliver.name = "Deliver";
                deliver.interactable = HelpWanted.CanDeliver(s, job);
            }
        }
    }

    // The journal tab: quests, kept letters, accepted jobs.
    public sealed class JournalPage : MenuPage
    {
        RectTransform _list;
        public override string Id => MenuTabs.Journal;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Journal", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("menu.tab.journal"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 3f);
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            _list = (RectTransform)list.transform;
        }

        public override void Refresh(UiService ui)
        {
            var s = ui.Session;
            UiKit.ClearChildren(_list);
            UiKit.Label(_list, L.Get("journal.quests"), 20f, TextAlignmentOptions.Left, UiKit.Accent);
            var any = false;
            foreach (var q in QuestLog.Active(s))
            {
                any = true;
                UiKit.Label(_list, L.Get(q.TitleKey), 18f);
                foreach (var o in q.Objectives)
                    UiKit.Label(_list, (QuestLog.ObjectiveMet(s, q, o) ? "[x] " : "[ ] ") + L.Get(o.Text) + QuestLog.ProgressText(s, q, o), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            }
            if (!any) UiKit.Label(_list, L.Get("journal.none"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Label(_list, L.Get("journal.done", QuestLog.Done(s).Count()), 15f, TextAlignmentOptions.Left, UiKit.DimText);

            UiKit.Label(_list, L.Get("journal.jobs"), 20f, TextAlignmentOptions.Left, UiKit.Accent);
            foreach (var job in s.State.Board.Where(j => j.Accepted))
                UiKit.Label(_list, L.Get("board.job", job.Count, s.Db.TryGetItem(job.ItemId, out var it) ? L.Get(it.NameKey) : job.ItemId), 15f);

            foreach (var page in s.Hooks.JournalPages)
            {
                if (!page.Visible) continue;
                UiKit.Label(_list, L.Get(page.TitleKey), 20f, TextAlignmentOptions.Left, UiKit.Accent);
                UiKit.Label(_list, page.Body(), 14f, TextAlignmentOptions.Left, UiKit.DimText);
            }

            UiKit.Label(_list, L.Get("journal.letters"), 20f, TextAlignmentOptions.Left, UiKit.Accent);
            foreach (var id in s.State.MailKept)
            {
                var letter = s.Story.Letter(id);
                if (letter == null) continue;
                var captured = letter;
                UiKit.MakeButton(_list, s.StoryText(letter.SubjectKey, new object[0]), () => ui.ShowLetter(captured, null), 400f, 28f);
            }
        }
    }
}
