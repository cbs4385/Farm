using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // The Village Gazette (T-146): this week's issue, laid out to be screenshot-friendly: a masthead, one headline, three news items and
    // three small corners.
    public sealed class GazettePage : MenuPage
    {
        RectTransform _body;

        public override string Id => MenuTabs.Gazette;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Gazette", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            var body = UiKit.VStack(stack.transform, "Body", 6f);
            UiKit.Size(body.gameObject, -1f, -1f, 1f, 1f);
            _body = (RectTransform)body.transform;
        }

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_body);
            var s = ui.Session;
            var issue = Gazette.Build(s.Story, s.World, s.State.WorldSeed);
            var now = s.Clock.Now;
            Line(L.Get("gazette.masthead"), 30f, UiKit.Accent, TextAlignmentOptions.Center);
            Line(L.Get("gazette.issue", issue.Number, L.Get("season." + now.Season.ToString().ToLowerInvariant()), now.Year), 15f, UiKit.DimText, TextAlignmentOptions.Center);
            if (issue.Headline != null) Line(Text(s, issue.Headline), 24f, UiKit.TextColor, TextAlignmentOptions.Center);
            foreach (var id in issue.News) Line("- " + Text(s, id), 17f, UiKit.TextColor, TextAlignmentOptions.TopLeft);
            Corner(s, "gazette.corner.farm", issue.Farm);
            Corner(s, "gazette.corner.weather", issue.Weather);
            Corner(s, "gazette.corner.classified", issue.Classified);
        }

        static string Text(GameSession s, string dialogueId)
        {
            var key = s.Story.Dialogue(dialogueId)?.Nodes[0].Text;
            return key == null ? string.Empty : RichText.Plain(s.StoryText(key, System.Array.Empty<object>()));
        }

        void Corner(GameSession s, string titleKey, string dialogueId)
        {
            if (dialogueId == null) return;
            Line(L.Get(titleKey), 15f, UiKit.Accent, TextAlignmentOptions.TopLeft);
            Line(Text(s, dialogueId), 16f, UiKit.TextColor, TextAlignmentOptions.TopLeft);
        }

        void Line(string text, float size, Color color, TextAlignmentOptions align)
        {
            var label = UiKit.Label(_body, text, size, align, color);
            label.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Size(label.gameObject, -1f, size * 1.35f * (1 + text.Length / 80), 1f);
        }
    }
}
