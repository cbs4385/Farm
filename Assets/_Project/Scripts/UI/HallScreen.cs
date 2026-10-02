using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // The Community Hall's bundle board: each room with what it needs and a Donate button.
    public sealed class HallScreen : UiScreen
    {
        readonly RectTransform _list;

        public HallScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Hall", new Vector2(780f, 480f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 6f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("hall.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            _list = (RectTransform)list.transform;
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenHall()
        {
            HallRooms.StartAll(Ui.Session);
            Rebuild();
            Open();
        }

        void Rebuild()
        {
            var s = Ui.Session;
            UiKit.ClearChildren(_list);
            foreach (var room in HallRooms.Rooms)
            {
                var def = s.Story.Quest(room);
                if (def == null) continue;
                var done = HallRooms.IsRestored(s.State, room);
                var row = UiKit.HStack(_list, room, 8f);
                UiKit.Size(row.gameObject, -1f, 54f);
                var text = UiKit.VStack(row.transform, "Text", 0f);
                UiKit.Size(text.gameObject, -1f, 50f, 1f);
                UiKit.Label(text.transform, L.Get(def.TitleKey) + (done ? "  " + L.Get("hall.restored") : string.Empty), 19f, TextAlignmentOptions.Left, done ? UiKit.Accent : UiKit.TextColor);
                var needs = string.Join(", ", def.Objectives.Select(o => (QuestLog.ObjectiveMet(s, o) ? "[x] " : "[ ] ") + L.Get(o.Text)));
                UiKit.Label(text.transform, done ? string.Empty : needs, 13f, TextAlignmentOptions.Left, UiKit.DimText);
                if (done) continue;
                var captured = room;
                var button = UiKit.MakeButton(row.transform, L.Get("hall.donate"), () => { HallRooms.Donate(s, captured); Rebuild(); }, 110f, 34f);
                button.name = "Donate";
                button.interactable = QuestLog.ObjectivesMet(s, def);
            }
        }
    }
}
