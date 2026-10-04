using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // The Gossip Book (T-144): how many of the rare and legendary lines each met villager has been heard to say, the latest few in their
    // own words, and the village stories of this game that are solved. Hidden lines are never shown, only counted.
    public sealed class GossipPage : MenuPage
    {
        RectTransform _body;

        public override string Id => MenuTabs.Gossip;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Gossip", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("gossip.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var hint = UiKit.Label(stack.transform, L.Get("gossip.hint"), 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(hint.gameObject, -1f, 22f);
            var body = UiKit.VStack(stack.transform, "Body", 4f);
            UiKit.Size(body.gameObject, -1f, -1f, 1f, 1f);
            _body = (RectTransform)body.transform;
        }

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_body);
            var s = ui.Session;
            var memory = LineMemory.Load(s);
            var data = GossipBook.Build(s.Npcs.All.Select(n => n.Id), s.Story, memory, s.State.Flags);
            Text(L.Get("gossip.total", data.FoundTotal, data.RareTotal), 20f, UiKit.TextColor);
            Text(L.Get("gossip.stories", data.StorylinesThisGame, data.StorylinesSolved), 17f, UiKit.DimText);
            foreach (var key in data.SolvedTitleKeys) Text(L.Get("gossip.solved", L.Get(key)), 16f, UiKit.Accent);

            foreach (var entry in data.Villagers.Where(v => s.State.Npcs.TryGetValue(v.Npc, out var st) && st.Met))
            {
                var npc = s.Npcs.Get(entry.Npc);
                var row = UiKit.HStack(_body, "Row_" + entry.Npc, 8f);
                UiKit.Size(row.gameObject, -1f, 24f);
                var name = UiKit.Label(row.transform, StoryTokens.ShortName(L.Get(npc.NameKey)), 17f, TextAlignmentOptions.Left);
                UiKit.Size(name.gameObject, 110f, 24f);
                var count = UiKit.Label(row.transform, L.Get("gossip.count", entry.Found.Count, entry.Total), 16f, TextAlignmentOptions.Left, entry.Found.Count > 0 ? UiKit.Accent : UiKit.DimText);
                UiKit.Size(count.gameObject, 70f, 24f);
            }

            var latest = data.Villagers.SelectMany(v => v.Found.Select(d => (npc: v.Npc, id: d, day: memory.LastHeardDay($"npc.{v.Npc}.talk", d))))
                .OrderByDescending(x => x.day).Take(4).ToList();
            foreach (var f in latest)
            {
                var key = s.Story.Dialogue(f.id)?.Nodes.FirstOrDefault()?.Text;
                if (key == null) continue;
                var who = StoryTokens.ShortName(L.Get(s.Npcs.Get(f.npc).NameKey));
                Text($"{who}: \"{RichText.Plain(s.StoryText(key, System.Array.Empty<object>()))}\"", 15f, UiKit.TextColor);
            }
        }

        void Text(string text, float size, Color color)
        {
            var label = UiKit.Label(_body, text, size, TextAlignmentOptions.TopLeft, color);
            label.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Size(label.gameObject, -1f, size * 1.35f * (1 + text.Length / 90), 1f);
        }
    }
}
