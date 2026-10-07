using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // ---- skills ------------------------------------------------------------------------------------------------------

    public sealed class SkillsPage : MenuPage
    {
        sealed class Row { public TextMeshProUGUI Level, Xp; public RectTransform Fill; }
        readonly Dictionary<string, Row> _rows = new Dictionary<string, Row>();
        RectTransform _offers;

        public override string Id => MenuTabs.Skills;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Skills", 10f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("skills.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            foreach (var skill in SkillIds.All)
            {
                var row = UiKit.HStack(stack.transform, skill, 12f);
                UiKit.Size(row.gameObject, -1f, 44f);
                var name = UiKit.Label(row.transform, L.Get("skill." + skill), 22f);
                UiKit.Size(name.gameObject, 160f, 40f);
                var level = UiKit.Label(row.transform, "", 22f, TextAlignmentOptions.Left, UiKit.Accent);
                UiKit.Size(level.gameObject, 90f, 40f);

                var bar = UiKit.Panel(row.transform, "Bar", new Color(0.08f, 0.06f, 0.04f, 1f));
                UiKit.Size(bar.gameObject, 360f, 20f);
                var fill = UiKit.Panel(bar.transform, "Fill", UiKit.Accent);
                fill.rectTransform.anchorMin = Vector2.zero;
                fill.rectTransform.anchorMax = new Vector2(0f, 1f);
                fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

                var xp = UiKit.Label(row.transform, "", 18f, TextAlignmentOptions.Right, UiKit.DimText);
                UiKit.Size(xp.gameObject, 150f, 40f);
                _rows[skill] = new Row { Level = level, Xp = xp, Fill = fill.rectTransform };
            }
            var offers = UiKit.VStack(stack.transform, "Offers", 4f);
            _offers = (RectTransform)offers.transform;
        }

        public override void Refresh(UiService ui)
        {
            foreach (var pair in _rows)
            {
                var xp = ui.Session.GetSkillXp(pair.Key);
                var level = SkillModel.LevelForXp(xp);
                pair.Value.Level.text = L.Get("skills.level", level);
                pair.Value.Fill.anchorMax = new Vector2(SkillModel.Progress(xp), 1f);
                pair.Value.Xp.text = level >= SkillModel.MaxLevel
                    ? L.Get("skills.max")
                    : L.Get("skills.xp", xp - SkillModel.XpForLevel(level), SkillModel.XpForLevel(level + 1) - SkillModel.XpForLevel(level));
            }

            // Professions: chosen ones, and the choices waiting at levels 5 and 10.
            UiKit.ClearChildren(_offers);
            foreach (var skill in SkillIds.All)
            {
                var chosen = Professions.Rows.Where(r => r.Skill == skill && ui.Session.State.Professions.Contains(r.Id)).ToList();
                if (chosen.Count > 0)
                    UiKit.Label(_offers, L.Get("skill." + skill) + ": " + string.Join(", ", chosen.Select(c => L.Get("profession." + c.Id))), 14f, TextAlignmentOptions.Left, UiKit.DimText);
                var offers = Professions.Offers(ui.Session.State, skill, ui.Session.GetSkillLevel(skill));
                foreach (var tier in offers.GroupBy(o => o.Level))
                {
                    var row = UiKit.HStack(_offers, "Offer_" + skill + tier.Key, 6f);
                    UiKit.Size(row.gameObject, -1f, 30f);
                    UiKit.Label(row.transform, L.Get("skills.choose", L.Get("skill." + skill), tier.Key), 15f, TextAlignmentOptions.Left, UiKit.Accent);
                    foreach (var option in tier)
                    {
                        var id = option.Id;
                        UiKit.MakeButton(row.transform, L.Get("profession." + id), () => { Professions.Choose(ui.Session, id); Refresh(ui); }, 150f, 28f).name = "Choose_" + id;
                    }
                }
            }
        }
    }

    // ---- social ------------------------------------------------------------------------------------------------------

    // ---- calendar ----------------------------------------------------------------------------------------------------

    public sealed class CalendarPage : MenuPage
    {
        TextMeshProUGUI _title;
        RectTransform _grid;
        int _season;
        int _year;

        public override string Id => MenuTabs.Calendar;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Calendar", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);

            var header = UiKit.HStack(stack.transform, "Header", 8f);
            UiKit.Size(header.gameObject, -1f, 36f);
            UiKit.MakeButton(header.transform, L.Get("calendar.prev"), () => Step(-1, ui), 120f, 32f).name = "Prev";
            _title = UiKit.Label(header.transform, "", 24f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Size(_title.gameObject, -1f, 34f, 1f);
            UiKit.MakeButton(header.transform, L.Get("calendar.next"), () => Step(1, ui), 120f, 32f).name = "Next";

            var grid = UiKit.Rect("Grid", stack.transform);
            UiKit.Size(grid.gameObject, -1f, -1f, 1f, 1f);
            _grid = grid;
        }

        void Step(int delta, UiService ui)
        {
            _season += delta;
            if (_season > 3) { _season = 0; _year++; }
            if (_season < 0) { _season = 3; _year = Mathf.Max(1, _year - 1); }
            Refresh(ui);
        }

        public override void Refresh(UiService ui)
        {
            var now = ui.Session.Clock.Now;
            if (_year == 0) { _season = (int)now.Season; _year = now.Year; }   // first view since the menu opened: this month

            _title.text = L.Get("calendar.title", L.Get("season." + ((Season)_season).ToString().ToLowerInvariant()), _year);
            UiKit.ClearChildren(_grid);
            var names = new Dictionary<int, List<string>>();
            foreach (var npc in ui.Session.Npcs.All)
            {
                if (!(ui.Session.State.Npcs.TryGetValue(npc.Id, out var s) && s.Met) || (int)npc.BirthdaySeason != _season) continue;
                if (!names.TryGetValue(npc.BirthdayDay, out var list)) names[npc.BirthdayDay] = list = new List<string>();
                list.Add(L.Get(npc.NameKey));
            }

            var cellW = 118f; var cellH = 52f; var headerH = 22f;
            var days = new[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };
            for (var c = 0; c < 7; c++)
            {
                var head = UiKit.Label(_grid, L.Get("day." + days[c]), 16f, TextAlignmentOptions.Center, UiKit.DimText);
                UiKit.Place(head.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(cellW, headerH), new Vector2(c * (cellW + 4f) + 6f, 0f));
            }
            for (var day = 1; day <= GameDateTime.DaysPerSeason; day++)
            {
                var col = (day - 1) % 7; var row = (day - 1) / 7;
                var date = new GameDateTime(_year, (Season)_season, day);
                var isToday = date.Year == now.Year && date.Season == now.Season && date.Day == now.Day;
                var phase = date.MoonPhase;
                var color = isToday ? new Color(0.62f, 0.45f, 0.20f) : UiKit.PanelLight;
                var cell = UiKit.Panel(_grid, "Day" + day, color);
                UiKit.Place(cell.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(cellW, cellH),
                    new Vector2(col * (cellW + 4f) + 6f, -(headerH + 2f + row * (cellH + 4f))));
                var number = UiKit.Label(cell.transform, day.ToString(), 18f, TextAlignmentOptions.TopLeft, isToday ? Color.white : UiKit.TextColor);
                UiKit.Stretch(number.rectTransform, 4f);
                var notes = new List<string>();
                if (phase == MoonPhase.New && day == 1) notes.Add(L.Get("calendar.new_moon"));
                if (phase == MoonPhase.Full && (day == 15)) notes.Add(L.Get("calendar.full_moon"));
                if (names.TryGetValue(day, out var birthdays)) notes.AddRange(birthdays.Select(n => L.Get("calendar.birthday", n)));
                foreach (var fest in ui.Session.Story.Events)
                    if (!string.IsNullOrEmpty(fest.Calendar) && fest.CalendarSeason == _season && fest.CalendarDay == day) notes.Add(L.Get(fest.Calendar));
                if (notes.Count > 0)
                {
                    var note = UiKit.Label(cell.transform, string.Join("\n", notes), 12f, TextAlignmentOptions.BottomRight, UiKit.Accent);
                    UiKit.Stretch(note.rectTransform, 4f);
                }
            }
        }

        // Opening the menu again returns to the current month.
        public override void OnMenuOpened() => _year = 0;
    }
}
