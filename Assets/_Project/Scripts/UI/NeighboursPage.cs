using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The Neighbours page (T-106): a list of villagers with their hearts, and for the selected one a detail panel with their stage,
    // birthday, where they are, the tastes the player has found out, what they have told the player, an open favor and a hint at what
    // comes next. Names stay "???" until the villager is met.
    public sealed class SocialPage : MenuPage
    {
        RectTransform _list, _detail;
        string _selected;

        public override string Id => MenuTabs.Social;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Social", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("social.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var body = UiKit.HStack(stack.transform, "Body", 12f);
            UiKit.Size(body.gameObject, -1f, -1f, 1f, 1f);
            // Twelve villagers do not fit in the page at every screen shape and text size, so the list scrolls (it used to squeeze the rows instead).
            var list = UiKit.Scroll(body.transform, "List", out _list);
            UiKit.Size(list.gameObject, 340f, -1f, -1f, 1f);
            var detail = UiKit.Scroll(body.transform, "Detail", out _detail);      // a long entry (tastes, what they told you, a favor) scrolls too
            UiKit.Size(detail.gameObject, -1f, -1f, 1f, 1f);
        }

        public override void OnMenuOpened() => _selected = null;

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_list);
            var session = ui.Session;
            if (_selected == null)
                _selected = (session.Npcs.All.FirstOrDefault(n => session.State.Npcs.TryGetValue(n.Id, out var st) && st.Met) ?? session.Npcs.All.First()).Id;
            foreach (var npc in session.Npcs.All)
            {
                var state = session.State.Npcs.TryGetValue(npc.Id, out var s) ? s : null;
                var met = state != null && state.Met;
                var id = npc.Id;
                var button = UiKit.MakeButton(_list, string.Empty, () => { _selected = id; Refresh(ui); }, 322f, 31f);
                button.name = "Neighbor_" + npc.Id;
                var row = UiKit.HStack(button.transform, "Row", 8f, TextAnchor.MiddleLeft);
                UiKit.Stretch((RectTransform)row.transform);
                row.padding = new RectOffset(4, 4, 1, 1);
                row.childForceExpandWidth = false;

                var portrait = UiKit.Panel(row.transform, "Portrait", met ? Color.white : new Color(0.2f, 0.15f, 0.1f));
                portrait.sprite = met ? npc.Portrait : null;
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
                UiKit.Size(portrait.gameObject, 26f, 26f);

                var name = UiKit.Label(row.transform, met ? L.Get(npc.NameKey) : L.Get("social.unknown"), 17f, TextAlignmentOptions.Left, id == _selected ? UiKit.Accent : UiKit.TextColor);
                name.raycastTarget = false;
                UiKit.Size(name.gameObject, 150f, 28f);

                var hearts = UiKit.HStack(row.transform, "Hearts", 1f);
                UiKit.Size(hearts.gameObject, 130f, 28f);
                var count = state != null ? FriendshipModel.Hearts(state.Points) : 0;
                for (var h = 0; h < FriendshipModel.MaxHearts; h++)
                {
                    var heart = UiKit.Panel(hearts.transform, "Heart" + h, h < count ? new Color(0.90f, 0.30f, 0.35f) : new Color(0.25f, 0.18f, 0.16f));
                    heart.raycastTarget = false;
                    UiKit.Size(heart.gameObject, 11f, 11f);
                }
            }
            ShowDetail(session);
        }

        void ShowDetail(GameSession session)
        {
            UiKit.ClearChildren(_detail);
            var npc = session.Npcs.Get(_selected);
            if (npc == null) return;
            var state = session.State.Npcs.TryGetValue(npc.Id, out var st) ? st : null;
            var info = NeighbourJournal.Build(npc, session.State, session.Story, InteractionState.Load(session));
            if (!info.Met)
            {
                Line(L.Get("neighbours.unmet"), 18f, UiKit.DimText);
                return;
            }
            var head = UiKit.HStack(_detail, "Head", 10f);
            UiKit.Size(head.gameObject, -1f, 70f);
            var portrait = UiKit.Panel(head.transform, "Portrait", Color.white);
            portrait.sprite = npc.Portrait;
            portrait.preserveAspect = true;
            UiKit.Size(portrait.gameObject, 64f, 64f);
            var names = UiKit.VStack(head.transform, "Names", 2f);
            UiKit.Size(names.gameObject, -1f, 66f, 1f);
            UiKit.Label(names.transform, L.Get(npc.NameKey), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            UiKit.Label(names.transform, L.Get("neighbours.stage", L.Get(RelationshipStages.Key(info.Stage)), info.Hearts), 17f, TextAlignmentOptions.Left);

            var birthday = L.Get("social.birthday", L.Get("season." + npc.BirthdaySeason.ToString().ToLowerInvariant()), npc.BirthdayDay);
            var place = NpcLocator.Where(session, npc);
            var where = place.Walking ? L.Get("social.walking", L.Get("map." + place.DestinationMap)) : L.Get("social.at", L.Get("map." + place.Map));
            Line($"{birthday} - {(state != null && state.TalkedToday ? L.Get("social.talked") : L.Get("social.not_talked"))} - {where}", 16f, UiKit.DimText);

            string Items(IEnumerable<string> ids) => string.Join(", ", ids.Select(i => session.Db.TryGetItem(i, out var item) ? L.Get(item.NameKey) : i));
            var tastes = new List<string>();
            if (info.Loves.Count > 0) tastes.Add(L.Get("neighbours.loves", Items(info.Loves)));
            if (info.Likes.Count > 0) tastes.Add(L.Get("neighbours.likes", Items(info.Likes)));
            if (info.Dislikes.Count > 0) tastes.Add(L.Get("neighbours.dislikes", Items(info.Dislikes)));
            var first = StoryTokens.ShortName(L.Get(npc.NameKey));
            if (tastes.Count == 0) Line(L.Get("neighbours.unknown_tastes", first), 16f, UiKit.DimText);
            foreach (var t in tastes) Line(t, 17f, UiKit.TextColor);

            if (info.Told.Count > 0) Line(L.Get("neighbours.told", string.Join("; ", info.Told.Select(k => L.Get(k)))), 16f, UiKit.TextColor);
            if (info.QuestTitleKey != null) Line(L.Get("neighbours.quest", L.Get(info.QuestTitleKey)), 16f, UiKit.Accent);
            Line(info.Hint == "hint.at" ? L.Get("neighbours.hint.at", first, info.HintHearts) : L.Get("neighbours." + info.Hint, first), 16f, UiKit.DimText);
        }

        void Line(string text, float size, Color color)
        {
            var label = UiKit.Label(_detail, text, size, TextAlignmentOptions.TopLeft, color);
            label.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Size(label.gameObject, -1f, size * 1.4f * (1 + text.Length / 62), 1f);
        }
    }
}
