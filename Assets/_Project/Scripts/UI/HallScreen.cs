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
            var list = UiKit.Scroll(stack.transform, "List", out _list);                 // six rooms with up to four things each do not fit the screen at once
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            UiKit.MakeButton(stack.transform, L.Get("hall.donate_all"), () => { HallRooms.DonateAll(Ui.Session); Rebuild(); }, 280f, 34f).name = "DonateAll";
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
                var header = UiKit.Label(_list, L.Get(def.TitleKey) + (done ? "  " + L.Get("hall.restored") : string.Empty), 19f, TextAlignmentOptions.Left, done ? UiKit.Accent : UiKit.TextColor);
                UiKit.Size(header.gameObject, -1f, 28f);
                if (done) continue;
                var captured = room;
                foreach (var o in def.Objectives)
                {
                    var objective = o;
                    var row = UiKit.HStack(_list, "Need", 8f, TextAnchor.MiddleLeft);
                    UiKit.Size(row.gameObject, -1f, 30f);
                    var met = QuestLog.ObjectiveMet(s, def, objective);
                    var label = UiKit.Label(row.transform, (met ? "[x] " : "[ ] ") + L.Get(objective.Text) + QuestLog.ProgressText(s, def, objective), 15f, TextAlignmentOptions.Left, met ? UiKit.Accent : UiKit.DimText);
                    UiKit.Size(label.gameObject, -1f, 28f, 1f);
                    if (string.IsNullOrEmpty(objective.GiveItem) || met) continue;
                    var has = s.Backpack.Count(objective.GiveItem);
                    var button = UiKit.MakeButton(row.transform, has > 0 ? L.Get("hall.donate") : L.Get("hall.none"), () =>
                    {
                        HallRooms.Donate(s, captured, objective);
                        Rebuild();
                    }, 200f, 28f);
                    button.name = "Donate_" + objective.GiveItem;
                    button.interactable = has > 0;
                }
            }
        }
    }
}
