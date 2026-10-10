using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // The map tab of the menu: a little picture of the valley that follows the real geography (WorldMapLayout), with a picture for each building in
    // the place where its door is, the gold pin where the farmer is, and a label that appears when the mouse rests on a building or a region: its
    // name, whether the shop is open, and which villagers you have met are there.
    public sealed class MapPage : MenuPage
    {
        static readonly Color Parchment = new Color(0.84f, 0.74f, 0.55f), Frame = new Color(0.36f, 0.25f, 0.16f), Road = new Color(0.72f, 0.60f, 0.42f);
        static readonly Color Grass = new Color(0.45f, 0.65f, 0.34f), Cobble = new Color(0.76f, 0.68f, 0.52f), Sand = new Color(0.92f, 0.82f, 0.58f),
            Water = new Color(0.36f, 0.58f, 0.78f), ForestGreen = new Color(0.27f, 0.47f, 0.30f), WoodsGreen = new Color(0.16f, 0.30f, 0.22f),
            Rock = new Color(0.50f, 0.45f, 0.40f), Field = new Color(0.52f, 0.36f, 0.22f), Ink = new Color(0.20f, 0.13f, 0.08f);

        RectTransform _picture;
        RectTransform _places;
        GameObject _woods;                 // the woods and the road to them: drawn only once they are open
        UiService _ui;

        public override string Id => MenuTabs.Map;

        protected override void Build(UiService ui, RectTransform content)
        {
            _ui = ui;
            var title = UiKit.Label(content, L.Get("map.title"), 24f, TextAlignmentOptions.TopLeft, UiKit.Accent);
            UiKit.Place(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(400, 32), new Vector2(10, -6));

            var area = UiKit.Rect("Area", content);
            area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(10f, 8f); area.offsetMax = new Vector2(-10f, -42f);
            _picture = UiKit.Rect("Picture", area);
            _picture.anchorMin = _picture.anchorMax = _picture.pivot = new Vector2(0.5f, 0.5f);
            var fit = _picture.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = WorldMapLayout.Width / WorldMapLayout.Height;

            var paper = _picture.gameObject.AddComponent<Image>();
            paper.color = Parchment;
            paper.raycastTarget = false;
            Scenery();
            _places = UiKit.Rect("Places", _picture);
            UiKit.Stretch(_places);
        }

        // ---- the picture ---------------------------------------------------------------------------------------------------

        static void Fit(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0 / WorldMapLayout.Width, y0 / WorldMapLayout.Height);
            rt.anchorMax = new Vector2(x1 / WorldMapLayout.Width, y1 / WorldMapLayout.Height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Image Box(Transform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var image = UiKit.Panel(parent, name, color);
            image.raycastTarget = false;
            Fit(image.rectTransform, x0, y0, x1, y1);
            return image;
        }

        static void Region(Transform parent, WorldMapLayout.Region r, Color fill)
        {
            Box(parent, r.Map + "_Edge", r.X0 - 3f, r.Y0 - 3f, r.X1 + 3f, r.Y1 + 3f, Frame);
            Box(parent, r.Map, r.X0, r.Y0, r.X1, r.Y1, fill);
        }

        static Image Picture(Transform parent, string name, string sprite, Vector2 centre, float size)
        {
            var image = UiKit.Panel(parent, name, Color.white);
            image.sprite = UiArt.Get(sprite);
            image.enabled = image.sprite != null;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Fit(image.rectTransform, centre.x - size * 0.5f, centre.y - size * 0.5f, centre.x + size * 0.5f, centre.y + size * 0.5f);
            return image;
        }

        static void Caption(Transform parent, string map, WorldMapLayout.Region r)
        {
            var label = UiKit.Label(parent, L.Get("map." + map), 13f, TextAlignmentOptions.TopLeft, Ink);
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            Fit(label.rectTransform, r.X0 + 4f, r.Y1 - 20f, r.X1, r.Y1 - 2f);
        }

        // The land, the roads and the little drawings (fields, trees, waves). Static: made once.
        void Scenery()
        {
            var w = WorldMapLayout.Width; var h = WorldMapLayout.Height;
            Box(_picture, "Border", 0f, 0f, w, 3f, Frame); Box(_picture, "BorderTop", 0f, h - 3f, w, h, Frame);
            Box(_picture, "BorderLeft", 0f, 0f, 3f, h, Frame); Box(_picture, "BorderRight", w - 3f, 0f, w, h, Frame);

            var farm = WorldMapLayout.Farm; var village = WorldMapLayout.Village; var forest = WorldMapLayout.Forest;
            var woods = WorldMapLayout.Woods; var beach = WorldMapLayout.Beach; var hill = WorldMapLayout.MineHill;
            Region(_picture, farm, Grass); Region(_picture, village, Cobble); Region(_picture, forest, ForestGreen);
            Region(_picture, beach, Sand); Region(_picture, hill, Rock);
            Box(_picture, "Sea", beach.X0, beach.Y0, beach.X1, beach.Y0 + beach.Height * 5f / 24f, Water);

            // The roads: from the farm east along the village road, the lane from the beach up through the village and the forest, the track to the mine.
            var road = 6f;
            Box(_picture, "RoadEast", farm.X1 - 6f, 141f - road, village.X1 - 8f, 141f + road, Road);
            Box(_picture, "LaneNorthSouth", 390f - road, beach.Y0 + beach.Height * 5f / 24f, 390f + road, forest.Y1 - 4f, Road);
            Box(_picture, "MineTrack", 390f, 300f - 4f, hill.X0 + 30f, 300f + 4f, Road);

            // The farm: a fence around the land and ploughed fields.
            Box(_picture, "FarmFieldA", 120f, 105f, 170f, 150f, Field);
            Box(_picture, "FarmFieldB", 180f, 105f, 255f, 150f, Field);
            Box(_picture, "FarmFieldC", 140f, 200f, 255f, 255f, Field);
            for (var i = 0; i < 4; i++)
            {
                Box(_picture, "RowA" + i, 124f, 110f + i * 10f, 166f, 113f + i * 10f, new Color(0.40f, 0.27f, 0.16f));
                Box(_picture, "RowB" + i, 184f, 110f + i * 10f, 251f, 113f + i * 10f, new Color(0.40f, 0.27f, 0.16f));
            }

            // The ponds of the farm, the village and the forest, and the village square's fountain and clock tower, where they really are.
            foreach (var (region, pond) in WorldMapLayout.Ponds())
            {
                var rows = WorldMapLayout.PondRows(region, pond);
                for (var i = 0; i < rows.Count; i++) Box(_picture, "Pond_" + region.Map + i, rows[i].xMin, rows[i].yMin, rows[i].xMax, rows[i].yMax, Water);
            }
            foreach (var (cell, key) in WorldMapLayout.Landmarks())
            {
                var at = village.At(cell.x + 0.5f, cell.y);
                var isFountain = key.EndsWith("fountain");
                Box(_picture, "Landmark_" + key, at.x - 7f, at.y - 5f, at.x + 7f, at.y + 5f, isFountain ? Water : new Color(0.55f, 0.50f, 0.45f));
                Box(_picture, "LandmarkTop_" + key, at.x - 3f, at.y + 5f, at.x + 3f, at.y + (isFountain ? 7f : 11f), isFountain ? new Color(0.85f, 0.93f, 0.97f) : new Color(0.40f, 0.35f, 0.30f));
            }

            // Trees in the forest and the woods; waves on the sea; rocks on the hill.
            var trees = new[] { (310f, 330f), (322f, 306f), (345f, 350f), (360f, 318f), (420f, 345f), (440f, 320f), (462f, 352f), (480f, 332f), (332f, 362f), (448f, 298f) };
            foreach (var (x, y) in trees) Picture(_picture, "Tree", "ui_map_forest", new Vector2(x, y), 20f);
            for (var i = 0; i < 9; i++)
                Box(_picture, "Wave" + i, beach.X0 + 14f + i * 30f, beach.Y0 + 6f + (i % 2) * 5f, beach.X0 + 28f + i * 30f, beach.Y0 + 8f + (i % 2) * 5f, new Color(0.80f, 0.90f, 0.96f));
            foreach (var (x, y) in new[] { (530f, 330f), (560f, 345f), (600f, 335f), (625f, 305f), (540f, 300f) }) Box(_picture, "Stone", x, y, x + 9f, y + 6f, new Color(0.36f, 0.32f, 0.29f));

            foreach (var region in new[] { farm, village, forest, beach }) Caption(_picture, region.Map, region);

            // The woods, north of the forest, are shown only when they are open (Refresh).
            var woodsPicture = UiKit.Rect("Woods", _picture);
            UiKit.Stretch(woodsPicture);
            _woods = woodsPicture.gameObject;
            Region(woodsPicture, woods, WoodsGreen);
            Box(woodsPicture, "LaneToTheWoods", 390f - road, forest.Y1 - 6f, 390f + road, woods.Y0 + 6f, Road);
            foreach (var (x, y) in new[] { (338f, 400f), (362f, 424f), (420f, 404f), (448f, 424f), (456f, 392f) }) Picture(woodsPicture, "DarkTree", "ui_map_woods", new Vector2(x, y), 20f);
            Caption(woodsPicture, MapIds.Woods, woods);
        }

        // ---- the places: rebuilt each time the tab is shown ------------------------------------------------------------------

        public override void Refresh(UiService ui)
        {
            _ui = ui;
            ui.HideHover();
            var session = ui.Session;
            UiKit.ClearChildren(_places);
            var woodsOpen = session.HasFlag(MapIds.WoodsOpenFlag);
            _woods.SetActive(woodsOpen);
            var spots = WorldMapLayout.Spots(session.State, woodsOpen);
            var here = session.State.CurrentMap;

            // The regions answer the mouse too: the farm, the village, the forest and the beach say who is out and about there.
            foreach (var region in new[] { WorldMapLayout.Farm, WorldMapLayout.Village, WorldMapLayout.Forest, WorldMapLayout.Beach })
            {
                var spot = new WorldMapLayout.Spot { Map = region.Map, Position = region.Centre };
                var hotspot = Box(_places, "Region_" + region.Map, region.X0, region.Y0, region.X1, region.Y1, Color.clear);
                hotspot.raycastTarget = true;
                Hover(hotspot.gameObject, spot, false);
            }

            foreach (var spot in spots)
            {
                var built = spot.UnlockFlag == null || session.HasFlag(spot.UnlockFlag);
                if (spot.Map == here)
                    Box(_places, "Here_" + spot.Map, spot.Position.x - spot.Size * 0.6f, spot.Position.y - spot.Size * 0.6f, spot.Position.x + spot.Size * 0.6f, spot.Position.y + spot.Size * 0.6f, new Color(0.99f, 0.80f, 0.25f, 0.9f));
                var icon = Picture(_places, "Spot_" + spot.Map, spot.Icon, spot.Position, spot.Size);
                icon.color = built ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                icon.raycastTarget = true;
                Hover(icon.gameObject, spot, true);
            }

            // The pin: where the farmer stands (in a building: over the building).
            var player = Object.FindAnyObjectByType<PlayerController>();
            var tile = player != null ? new Vector2(Mathf.Floor(player.transform.position.x), Mathf.Floor(player.transform.position.y)) : Vector2.zero;
            var at = WorldMapLayout.PlayerPosition(here, tile, spots);
            if (at.HasValue)
            {
                var indoors = !(here == MapIds.Farm || here == MapIds.Village || here == MapIds.Forest || here == MapIds.Beach);
                var spotSize = spots.FirstOrDefault(sp => sp.Map == here)?.Size ?? WorldMapLayout.IconSize;
                var pin = Picture(_places, "YouAreHere", "ui_map_here", at.Value + new Vector2(0f, indoors ? spotSize * 0.9f : 10f), 22f);
                pin.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        // ---- the hover label --------------------------------------------------------------------------------------------------

        void Hover(GameObject target, WorldMapLayout.Spot spot, bool atTheIcon)
        {
            var trigger = target.AddComponent<EventTrigger>();
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(data =>
            {
                var position = atTheIcon ? (Vector2)target.transform.position : ((PointerEventData)data).position;
                _ui.ShowHover(HoverText(_ui.Session, spot), position);
            });
            trigger.triggers.Add(enter);
            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => _ui.HideHover());
            trigger.triggers.Add(exit);
            if (atTheIcon) MakeFocusable(target, trigger, spot);
        }

        // The same label for someone with no mouse: each building icon can take keyboard and gamepad focus (the arrows or the stick move between the
        // icons by position) and the label shows at the focused one, which is tinted gold. The regions stay mouse-only (they overlap the icons).
        void MakeFocusable(GameObject target, EventTrigger trigger, WorldMapLayout.Spot spot)
        {
            var button = target.AddComponent<Button>();
            button.targetGraphic = target.GetComponent<Image>();
            var colors = button.colors;
            colors.normalColor = colors.highlightedColor = Color.white;
            colors.selectedColor = colors.pressedColor = new Color(1f, 0.82f, 0.38f, 1f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            select.callback.AddListener(_ => _ui.ShowHover(HoverText(_ui.Session, spot), target.transform.position));
            trigger.triggers.Add(select);
            var deselect = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselect.callback.AddListener(_ => _ui.HideHover());
            trigger.triggers.Add(deselect);
        }

        // The label of a place: its name, whether a shop is open, a farm building's state, the villagers met who are there, and "you are here". (pure given the session)
        public static string HoverText(GameSession session, WorldMapLayout.Spot spot)
        {
            var text = L.Get("map." + spot.Map);
            if (spot.UnlockFlag != null && !session.HasFlag(spot.UnlockFlag)) text += "\n" + L.Get("map.not_built");
            else if (!string.IsNullOrEmpty(spot.Business)) text += "\n" + L.Get(BusinessHoursRegistry.IsOpen(spot.Business, session.Clock.Now) ? "map.open" : "map.closed");
            else if (!string.IsNullOrEmpty(spot.Condition))
            {
                var byDay = Conditions.TryEvaluate(NpcHomes.OpenCondition, session.World, out var day) && day;
                var welcome = Conditions.TryEvaluate(spot.Condition, session.World, out var open) && open;
                text += "\n" + L.Get(byDay ? "map.home_open" : welcome ? "map.home_friend" : "map.home_closed");
            }
            var names = session.Npcs.All
                .Where(npc => session.State.Npcs.TryGetValue(npc.Id, out var s) && s.Met && NpcLocator.MapOf(session, npc) == spot.Map)
                .Select(npc => L.Get(npc.NameKey)).ToList();
            if (names.Count > 0) text += "\n" + L.Get("map.here_names", string.Join(", ", names));
            if (spot.Map == session.State.CurrentMap) text += "\n" + L.Get("map.you_are_here");
            return text;
        }
    }
}
