using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // What the player does at the festival: dance round the maypole, enter the Tidewatch fishing contest, put a crop or preserve in the harvest show, hang a lantern and
    // sit down to the feast. Each takes part once a year for its prize (flags named by year); the contest and the show are judged at five in the afternoon. The pure rules are
    // in Festivals; the functions with a GameSession are tested without a scene.
    public static class FestivalActivities
    {
        public const string Danced = "danced", ContestStarted = "contest", ContestScore = "score", ContestClaimed = "claimed", ShowEntered = "entered", ShowScore = "showscore",
            ShowClaimed = "showclaimed", LanternDone = "lantern", FeastDone = "feast";

        public enum Open { Yes, TooEarly, Over, NotToday }

        // Whether the festival of this kind is on now.
        public static Open Window(GameSession s, FestivalKind kind)
        {
            var now = s.Clock.Now;
            var row = Festivals.Today(now);
            if (row == null || row.Kind != kind) return Open.NotToday;
            if (now.MinuteOfDay < Festivals.OpenMinute) return Open.TooEarly;
            return now.MinuteOfDay >= Festivals.CloseMinute ? Open.Over : Open.Yes;
        }

        static bool Closed(GameSession s, FestivalKind kind)
        {
            switch (Window(s, kind))
            {
                case Open.TooEarly: s.Toast(L.Get("festival.too_early", Clock(Festivals.OpenMinute))); return true;
                case Open.Over: s.Toast(L.Get("festival.over")); return true;
                case Open.NotToday: s.Toast(L.Get("festival.not_today")); return true;
                default: return false;
            }
        }

        public static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";

        public static void Use(PlayerActions player, FestivalStationKind kind)
        {
            var s = player.Session;
            switch (kind)
            {
                case FestivalStationKind.Maypole: Dance(player); break;
                case FestivalStationKind.Scoreboard: Contest(s); break;
                case FestivalStationKind.JudgingTable: Show(s); break;
                case FestivalStationKind.FeastTable: Feast(s); break;
                case FestivalStationKind.LanternRack: HangLantern(s); break;
            }
        }

        // ---- Blossom Day: the dance ----

        public static void Dance(PlayerActions player)
        {
            var s = player.Session;
            if (Closed(s, FestivalKind.Dance)) return;
            var row = Festivals.Get("spring");
            var year = s.Clock.Now.Year;
            var partner = NearestVillager(player);
            if (partner != null) partner.StartDancing();
            var first = !s.HasFlag(Festivals.Key(row, year, Danced));
            var started = player.Controller.StartDance(Festivals.DanceSeconds, () => FinishDance(s, partner));
            if (!started) return;
            AudioService.PlayIfAvailable(Sfx.Gift);
            if (!first) s.Toast(L.Get("festival.dance.again"));
        }

        // The dance was danced to the end: the reward, once a year.
        public static void FinishDance(GameSession s, NpcActor partner)
        {
            partner?.StopDancing();
            var row = Festivals.Get("spring");
            var key = Festivals.Key(row, s.Clock.Now.Year, Danced);
            if (s.HasFlag(key)) return;
            s.SetFlag(key);
            s.RestoreEnergy(Festivals.DanceEnergy);
            if (partner != null)
            {
                NpcInteractions.AddPoints(s, partner.Definition.Id, Festivals.DanceFriendship);
                s.Toast(L.Get("festival.dance.done", L.Get(partner.Definition.NameKey)));
            }
            else s.Toast(L.Get("festival.dance.alone"));
            s.AddVar("stat.festival", 1);
        }

        // ---- Tidewatch: the fishing contest ----

        public static int ContestPoints(GameSession s) => s.GetVar(Festivals.Key(Festivals.Get("summer"), s.Clock.Now.Year, ContestScore));

        // A fish was caught (FishingActions): while the contest is on, in the village, it counts.
        public static int RecordCatch(GameSession s, string mapId, string fishItemId, int quality)
        {
            if (mapId != MapIds.Village || Window(s, FestivalKind.FishingContest) != Open.Yes) return 0;
            var row = Festivals.Get("summer");
            var year = s.Clock.Now.Year;
            if (!s.HasFlag(Festivals.Key(row, year, ContestStarted)) || s.Clock.Now.MinuteOfDay >= Festivals.ContestUntilMinute) return 0;
            if (!s.Db.TryGetItem(fishItemId, out var fish)) return 0;
            var points = Festivals.CatchPoints(fish.SellPrice, quality);
            s.AddVar(Festivals.Key(row, year, ContestScore), points);
            s.Toast(L.Get("festival.contest.points", points, ContestPoints(s)));
            return points;
        }

        public static void Contest(GameSession s)
        {
            if (Closed(s, FestivalKind.FishingContest)) return;
            var row = Festivals.Get("summer");
            var year = s.Clock.Now.Year;
            var minute = s.Clock.Now.MinuteOfDay;
            var started = s.HasFlag(Festivals.Key(row, year, ContestStarted));
            if (!started)
            {
                if (minute >= Festivals.ContestUntilMinute) { s.Toast(L.Get("festival.contest.missed")); return; }
                s.SetFlag(Festivals.Key(row, year, ContestStarted));
                s.SetVar(Festivals.Key(row, year, ContestScore), 0);
                s.Toast(L.Get("festival.contest.start", Clock(Festivals.ContestUntilMinute)));
                return;
            }
            var table = Festivals.Table(Festivals.ContestEntrants, s.State.WorldSeed, year, false, ContestPoints(s));
            if (minute < Festivals.ContestUntilMinute)
            {
                s.Toast(L.Get("festival.contest.standing", ContestPoints(s), Festivals.Place(table), table.Count, Clock(Festivals.ContestUntilMinute)));
                return;
            }
            Award(s, row, year, ContestClaimed, table, Festivals.ContestTrophy, "festival.contest.result");
        }

        // ---- Harvest Fair: the show ----

        public static bool Acceptable(ItemDefinition item) =>
            item != null && (item.Category == ItemCategory.Crop || item.Category == ItemCategory.Artisan || item.Category == ItemCategory.Food || item.Category == ItemCategory.Forage);

        public static void Show(GameSession s)
        {
            if (Closed(s, FestivalKind.CropShow)) return;
            var row = Festivals.Get("fall");
            var year = s.Clock.Now.Year;
            var minute = s.Clock.Now.MinuteOfDay;
            var entered = s.HasFlag(Festivals.Key(row, year, ShowEntered));
            if (!entered)
            {
                if (minute >= Festivals.ResultsMinute) { s.Toast(L.Get("festival.show.missed")); return; }
                var slot = s.State.SelectedHotbar;
                var stack = s.Backpack.Get(slot);
                if (stack == null || !s.Db.TryGetItem(stack.ItemId, out var item) || !Acceptable(item)) { s.Toast(L.Get("festival.show.pick")); return; }
                var score = SellLogic.ValueOf(s, slot, 1);
                s.Backpack.RemoveFromSlot(slot, 1);
                s.SetFlag(Festivals.Key(row, year, ShowEntered));
                s.SetVar(Festivals.Key(row, year, ShowScore), score);
                s.Toast(L.Get("festival.show.entered", L.Get(item.NameKey), Clock(Festivals.ResultsMinute)));
                return;
            }
            if (minute < Festivals.ResultsMinute) { s.Toast(L.Get("festival.show.waiting", Clock(Festivals.ResultsMinute))); return; }
            var table = Festivals.Table(Festivals.ShowEntrants, s.State.WorldSeed, year, true, s.GetVar(Festivals.Key(row, year, ShowScore)));
            Award(s, row, year, ShowClaimed, table, Festivals.ShowRibbon, "festival.show.result");
        }

        // The prize of a place, once a year: gold, and for the first place a keepsake.
        static void Award(GameSession s, FestivalRow row, int year, string claimedKey, List<Festivals.Standing> table, string keepsake, string textKey)
        {
            var key = Festivals.Key(row, year, claimedKey);
            if (s.HasFlag(key)) { s.Toast(L.Get("festival.claimed")); return; }
            s.SetFlag(key);
            var place = Festivals.Place(table);
            var gold = Festivals.Prize(place);
            s.AddGold(gold);
            if (place == 1) s.GiveItem(keepsake, 1);
            var winner = table[0].Name.Length == 0 ? L.Get("festival.you") : L.Get("npc." + table[0].Name + ".name");
            s.Toast(L.Get(textKey, L.Get("festival.place." + System.Math.Min(place, 4)), gold, winner));
            if (place == 1) AudioService.PlayIfAvailable(Sfx.Gift);
            s.AddVar("stat.festival", 1);
        }

        // ---- Lantern Feast ----

        public static void HangLantern(GameSession s)
        {
            if (Closed(s, FestivalKind.Lanterns)) return;
            var row = Festivals.Get("winter");
            var year = s.Clock.Now.Year;
            var key = Festivals.Key(row, year, LanternDone);
            if (s.HasFlag(key)) { s.Toast(L.Get("festival.lantern.again")); return; }
            s.SetFlag(key);
            var wish = Festivals.WishFor(s.State.WorldSeed, year);
            var neighbour = NearestVillager(null, s);
            if (neighbour != null) NpcInteractions.AddPoints(s, neighbour.Definition.Id, Festivals.LanternFriendship);
            s.Toast(L.Get("festival.lantern.hung", L.Get("festival.wish." + wish)));
            AudioService.PlayIfAvailable(Sfx.Gift);
            s.AddVar("stat.festival", 1);
        }

        public static void Feast(GameSession s)
        {
            if (Closed(s, FestivalKind.Lanterns)) return;
            var row = Festivals.Get("winter");
            var year = s.Clock.Now.Year;
            if (s.Clock.Now.MinuteOfDay < Festivals.ResultsMinute) { s.Toast(L.Get("festival.feast.later", Clock(Festivals.ResultsMinute))); return; }
            var key = Festivals.Key(row, year, FeastDone);
            if (s.HasFlag(key)) { s.Toast(L.Get("festival.feast.again")); return; }
            s.SetFlag(key);
            s.RestoreEnergy(s.State.MaxEnergy);
            var neighbour = NearestVillager(null, s);
            if (neighbour != null) NpcInteractions.AddPoints(s, neighbour.Definition.Id, Festivals.FeastFriendship);
            s.Toast(L.Get("festival.feast.eaten", neighbour != null ? L.Get(neighbour.Definition.NameKey) : L.Get("festival.the_village")));
            AudioService.PlayIfAvailable(Sfx.Gift);
            s.AddVar("stat.festival", 1);
        }

        // ---- who is near ----

        public const float NeighbourRange = 7f;

        // The villager nearest to the player on this map (awake, not walking away), or null.
        public static NpcActor NearestVillager(PlayerActions player, GameSession session = null)
        {
            var s = player != null ? player.Session : session;
            var npcs = s?.Game?.Map?.Npcs;
            var from = player != null ? player.transform.position : s?.Game?.Map?.Player != null ? s.Game.Map.Player.transform.position : Vector3.zero;
            if (npcs == null) return null;
            return npcs.Actors.Values.Where(a => a != null && a.isActiveAndEnabled && !a.Sleeping && Vector2.Distance(a.transform.position, from) <= NeighbourRange)
                .OrderBy(a => Vector2.Distance(a.transform.position, from)).FirstOrDefault();
        }
    }
}
