using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The memories tab (T-102): festivals and heart events the player has seen, to replay. Two panes (playtest 2026-10-07: the old page packed every
    // villager's scenes into cramped rows, wrapped names a letter to a line and filled the "Special" row with question marks): the groups on the
    // left, each with how many scenes have been seen, and the scenes of the chosen group on the right in a scrolling list. Scenes not seen yet are
    // only counted ("5 more to be seen"). Replays change nothing in the save (see Memories).
    public sealed class MemoriesPage : MenuPage
    {
        RectTransform _groups;
        RectTransform _scenes;
        TextMeshProUGUI _title;
        UiService _ui;
        string _selected;

        public override string Id => MenuTabs.Memories;

        public string Selected => _selected;

        protected override void Build(UiService ui, RectTransform content)
        {
            _ui = ui;
            var stack = UiKit.VStack(content, "Memories", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(_title.gameObject, -1f, 32f);
            var hint = UiKit.Label(stack.transform, L.Get("memories.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 22f);

            var panes = UiKit.HStack(stack.transform, "Panes", 12f, TextAnchor.UpperLeft);
            panes.childForceExpandHeight = true;
            UiKit.Size(panes.gameObject, -1f, -1f, 1f, 1f);
            var left = UiKit.Scroll(panes.transform, "Groups", out _groups);
            UiKit.Size(left.gameObject, 250f, -1f, 0f, 1f);
            var right = UiKit.Scroll(panes.transform, "Scenes", out _scenes);
            UiKit.Size(right.gameObject, -1f, -1f, 1f, 1f);
        }

        public override void OnMenuOpened() => _selected = null;

        // One group of memories: festivals, a villager, or the specials.
        public sealed class Group
        {
            public string Id, Name;
            public List<EventDefinition> Events = new List<EventDefinition>();
        }

        // The groups in the order they are listed: festivals, the villagers who have scenes, then the specials. (pure)
        public static List<Group> Groups(GameSession s, List<EventDefinition> all)
        {
            var groups = new List<Group>();
            void Add(string id, string name)
            {
                var events = all.Where(e => Memories.GroupOf(e) == id).ToList();
                if (events.Count > 0) groups.Add(new Group { Id = id, Name = name, Events = events });
            }
            Add(Memories.FestivalGroup, L.Get("memories.group." + Memories.FestivalGroup));
            foreach (var npc in s.Npcs.All) Add(npc.Id, StoryTokens.ShortName(L.Get(npc.NameKey)));
            Add(Memories.OtherGroup, L.Get("memories.group." + Memories.OtherGroup));
            return groups;
        }

        public override void Refresh(UiService ui)
        {
            var s = ui.Session;
            var all = Memories.All(s.Story);
            _title.text = L.Get("memories.title", all.Count(e => Memories.Unlocked(s.State, e)), all.Count);
            var groups = Groups(s, all);
            if (groups.Count == 0) return;
            if (_selected == null || groups.All(g => g.Id != _selected))
                _selected = (groups.FirstOrDefault(g => g.Events.Any(e => Memories.Unlocked(s.State, e))) ?? groups[0]).Id;

            UiKit.ClearChildren(_groups);
            foreach (var g in groups)
            {
                var seen = g.Events.Count(e => Memories.Unlocked(s.State, e));
                var id = g.Id;
                var button = UiKit.MakeButton(_groups, L.Get("memories.group_count", g.Name, seen, g.Events.Count), () => { _selected = id; Refresh(_ui); }, 230f, 32f);
                button.name = "Group_" + g.Id;
                UiKit.Size(button.gameObject, -1f, 32f);
                if (g.Id == _selected) Mark(button);
            }

            var group = groups.First(g => g.Id == _selected);
            UiKit.ClearChildren(_scenes);
            var head = UiKit.Label(_scenes, L.Get("memories.seen_of", group.Name, group.Events.Count(e => Memories.Unlocked(s.State, e)), group.Events.Count), 20f,
                TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Size(head.gameObject, -1f, 30f);
            var unseen = 0;
            foreach (var e in group.Events)
            {
                if (Memories.Unlocked(s.State, e)) Scene(s, e);
                else unseen++;
            }
            if (unseen > 0)
            {
                var more = UiKit.Label(_scenes, L.Get(unseen == group.Events.Count ? "memories.none_yet" : "memories.more", unseen), 16f, TextAlignmentOptions.Left, UiKit.DimText);
                UiKit.Size(more.gameObject, -1f, 28f);
            }
        }

        // The chosen group stands out.
        static void Mark(Button button)
        {
            var colors = button.colors;
            colors.normalColor = new Color(0.62f, 0.45f, 0.20f);
            button.colors = colors;
        }

        void Scene(GameSession s, EventDefinition ev)
        {
            var id = ev.Id;
            var button = UiKit.MakeButton(_scenes, L.Get(Memories.TitleKey(ev)), () => Replay(id), 400f, 34f);
            button.name = "Memory_" + id;
            UiKit.Size(button.gameObject, -1f, 34f);
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) { text.alignment = TextAlignmentOptions.Left; text.margin = new Vector4(12f, 0f, 8f, 0f); text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Ellipsis; }
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
