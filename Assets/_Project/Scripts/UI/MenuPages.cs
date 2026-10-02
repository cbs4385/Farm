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
        }
    }

    // ---- social ------------------------------------------------------------------------------------------------------

    public sealed class SocialPage : MenuPage
    {
        RectTransform _list;

        public override string Id => MenuTabs.Social;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Social", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("social.title"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            _list = (RectTransform)list.transform;
        }

        public override void Refresh(UiService ui)
        {
            UiKit.ClearChildren(_list);
            var session = ui.Session;
            foreach (var npc in session.Npcs.All)
            {
                var state = session.State.Npcs.TryGetValue(npc.Id, out var s) ? s : null;
                var met = state != null && state.Met;
                var row = UiKit.HStack(_list, npc.Id, 10f);
                UiKit.Size(row.gameObject, -1f, 34f);

                var portrait = UiKit.Panel(row.transform, "Portrait", met ? Color.white : new Color(0.2f, 0.15f, 0.1f));
                portrait.sprite = met ? npc.Portrait : null;
                portrait.preserveAspect = true;
                UiKit.Size(portrait.gameObject, 30f, 30f);

                var name = UiKit.Label(row.transform, met ? L.Get(npc.NameKey) : L.Get("social.unknown"), 20f);
                UiKit.Size(name.gameObject, 190f, 32f);

                var hearts = UiKit.HStack(row.transform, "Hearts", 2f);
                UiKit.Size(hearts.gameObject, 190f, 32f);
                var count = state != null ? FriendshipModel.Hearts(state.Points) : 0;
                for (var i = 0; i < FriendshipModel.MaxHearts; i++)
                {
                    var heart = UiKit.Panel(hearts.transform, "Heart" + i, i < count ? new Color(0.90f, 0.30f, 0.35f) : new Color(0.25f, 0.18f, 0.16f));
                    UiKit.Size(heart.gameObject, 16f, 16f);
                }

                var detail = met ? Detail(session, npc, state) : string.Empty;
                var info = UiKit.Label(row.transform, detail, 16f, TextAlignmentOptions.Left, UiKit.DimText);
                UiKit.Size(info.gameObject, -1f, 32f, 1f);
            }
        }

        static string Detail(GameSession session, NpcDefinition npc, NpcState state)
        {
            var birthday = L.Get("social.birthday", L.Get("season." + npc.BirthdaySeason.ToString().ToLowerInvariant()), npc.BirthdayDay);
            var talked = state.TalkedToday ? L.Get("social.talked") : L.Get("social.not_talked");
            var place = NpcLocator.Where(session, npc);
            var where = place.Walking
                ? L.Get("social.walking", L.Get("map." + place.DestinationMap))
                : L.Get("social.at", L.Get("map." + place.Map));
            return $"{birthday} - {talked} - {where}";
        }
    }

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

    // ---- map ---------------------------------------------------------------------------------------------------------

    public sealed class MapPage : MenuPage
    {
        sealed class Place { public Image Box; public TextMeshProUGUI Label; }
        readonly Dictionary<string, Place> _places = new Dictionary<string, Place>();

        public override string Id => MenuTabs.Map;

        // Schematic, north up: the forest at the top of the lane, the beach at the bottom, the farm at the west end of the
        // road, the four shops north of the road and the saloon and clinic south of it.
        static readonly (string map, float x, float y, float w)[] Layout =
        {
            (MapIds.Forest, 0.50f, 0.90f, 150f),
            (MapIds.GeneralStore, 0.22f, 0.72f, 130f), (MapIds.Blacksmith, 0.40f, 0.72f, 130f),
            (MapIds.Carpenter, 0.60f, 0.72f, 130f), (MapIds.Library, 0.78f, 0.72f, 130f),
            (MapIds.Village, 0.50f, 0.48f, 260f),
            (MapIds.Farm, 0.10f, 0.48f, 130f), (MapIds.FarmHouse, 0.10f, 0.24f, 130f), (MapIds.Greenhouse, 0.10f, 0.72f, 130f),
            (MapIds.Saloon, 0.30f, 0.24f, 130f), (MapIds.Clinic, 0.70f, 0.24f, 130f),
            (MapIds.Beach, 0.50f, 0.08f, 150f),
        };

        protected override void Build(UiService ui, RectTransform content)
        {
            var title = UiKit.Label(content, L.Get("map.title"), 24f, TextAlignmentOptions.TopLeft, UiKit.Accent);
            UiKit.Place(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 32), new Vector2(10, -6));
            foreach (var (map, x, y, w) in Layout)
            {
                var box = UiKit.Panel(content, map, UiKit.PanelLight);
                UiKit.Place(box.rectTransform, new Vector2(x, y), new Vector2(0.5f, 0.5f), new Vector2(w, 54f), Vector2.zero);
                var label = UiKit.Label(box.transform, "", 14f, TextAlignmentOptions.Center);
                UiKit.Stretch(label.rectTransform, 3f);
                _places[map] = new Place { Box = box, Label = label };
            }
        }

        public override void Refresh(UiService ui)
        {
            var session = ui.Session;
            var here = session.State.CurrentMap;
            var who = new Dictionary<string, List<string>>();
            foreach (var npc in session.Npcs.All)
            {
                if (!(session.State.Npcs.TryGetValue(npc.Id, out var s) && s.Met)) continue;
                var map = NpcLocator.MapOf(session, npc);
                if (!who.TryGetValue(map, out var list)) who[map] = list = new List<string>();
                list.Add(L.Get(npc.NameKey));
            }
            foreach (var pair in _places)
            {
                var text = L.Get("map." + pair.Key);
                if (who.TryGetValue(pair.Key, out var names)) text += "\n" + string.Join(", ", names);
                if (pair.Key == here) text += "\n" + L.Get("map.you_are_here");
                pair.Value.Label.text = text;
                pair.Value.Box.color = pair.Key == here ? new Color(0.62f, 0.45f, 0.20f) : UiKit.PanelLight;
            }
        }
    }
}
