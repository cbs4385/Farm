using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The memories tab (T-102): festivals and heart events the player has seen, grouped by villager, to replay. Scenes not
    // seen yet show as locked. Replays change nothing in the save (see Memories).
    public sealed class MemoriesPage : MenuPage
    {
        RectTransform _body;
        TextMeshProUGUI _title;

        public override string Id => MenuTabs.Memories;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Memories", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_title.gameObject, -1f, 32f);
            var hint = UiKit.Label(stack.transform, L.Get("memories.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 22f);
            var body = UiKit.VStack(stack.transform, "Body", 4f);
            UiKit.Size(body.gameObject, -1f, -1f, 1f, 1f);
            _body = (RectTransform)body.transform;
        }

        public override void Refresh(UiService ui)
        {
            var s = ui.Session;
            var all = Memories.All(s.Story);
            _title.text = L.Get("memories.title", all.Count(e => Memories.Unlocked(s.State, e)), all.Count);
            UiKit.ClearChildren(_body);

            // One row of festivals and specials, then two villagers a row, each with their scenes.
            foreach (var group in new[] { Memories.FestivalGroup, Memories.OtherGroup })
            {
                var special = all.Where(e => Memories.GroupOf(e) == group).ToList();
                if (special.Count == 0) continue;
                var row = Row();
                Name(row, L.Get("memories.group." + group));
                foreach (var e in special) Entry(row, s, e, 160f);
            }
            var villagers = s.Npcs.All.Where(n => all.Any(e => Memories.GroupOf(e) == n.Id)).ToList();
            for (var i = 0; i < villagers.Count; i += 2)
            {
                var row = Row();
                for (var j = i; j < i + 2 && j < villagers.Count; j++)
                {
                    var npc = villagers[j];
                    Name(row, StoryTokens.ShortName(L.Get(npc.NameKey)));
                    foreach (var e in all.Where(e => Memories.GroupOf(e) == npc.Id)) Entry(row, s, e, 160f);
                }
            }
        }

        RectTransform Row()
        {
            var row = UiKit.HStack(_body, "Row", 6f);
            UiKit.Size(row.gameObject, -1f, 34f);
            return (RectTransform)row.transform;
        }

        static void Name(Transform row, string text)
        {
            var label = UiKit.Label(row, text, 17f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(label.gameObject, 72f, 32f);
        }

        void Entry(Transform row, GameSession s, EventDefinition ev, float width)
        {
            var unlocked = Memories.Unlocked(s.State, ev);
            var label = unlocked ? L.Get(Memories.TitleKey(ev)) : L.Get("memories.locked");
            var id = ev.Id;
            var button = UiKit.MakeButton(row, label, () => Replay(id), width, 30f);
            button.name = "Memory_" + id;
            button.interactable = unlocked;
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) { text.enableAutoSizing = true; text.fontSizeMin = 11f; text.fontSizeMax = 18f; }
        }

        void Replay(string eventId)
        {
            var ui = (UiService)ServiceLocator.Get<IUiService>();
            if (!Memories.CanStart(ui.Session, eventId)) return;
            ui.CloseAllModals();
            Memories.Start(ui.Session, eventId);
        }
    }
}
