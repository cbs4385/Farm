using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // The first-days coach (playtest 2026-10-10: "I prefer a game where I am spoon fed instructions as I do that activity"): one short tip at a time, for the thing to do next,
    // in a card above the hotbar: till, plant, water, wait for the crops, harvest, ship. It follows what the player has done (the quest statistics), so it moves on by
    // itself, never repeats a step that is done, goes away after the first shipment, and can be skipped for good or switched off in Options. Pure given the session.
    public static class TutorialCoach
    {
        public const string SkipFlag = "tutorial.coach.skipped";
        public const string RestingFlag = "tutorial.coach.resting";          // the player has slept on the crops: no tip until one is ripe

        public static string StepOf(GameSession s)
        {
            if (s == null || !s.InGame || s.HasFlag(SkipFlag)) return null;
            var settings = GameSession.Settings;
            if (settings != null && !settings.TutorialTips) return null;
            if (s.GetVar(QuestLog.Stats.Shipped) >= 1) return null;
            if (s.GetVar(QuestLog.Stats.Tilled) < 3) return "till";
            if (s.GetVar(QuestLog.Stats.Planted) < 3) return "plant";
            if (s.GetVar(QuestLog.Stats.Watered) < 3) return "water";
            if (s.GetVar(QuestLog.Stats.Harvested) < 1)
            {
                if (HasRipeCrop(s)) return "harvest";
                return s.HasFlag(RestingFlag) ? null : "wait";          // after the first night the tips rest until a crop is ripe
            }
            return "ship";
        }

        public static int Number(string step) => step == "till" ? 1 : step == "plant" ? 2 : step == "water" ? 3 : step == "wait" || step == "harvest" ? 4 : 5;

        // The card's words, or empty when there is no tip.
        public static string Text(GameSession s)
        {
            var step = StepOf(s);
            return step == null ? string.Empty : L.Get("tutorial.coach.step", Number(step), L.Get("tutorial.coach." + step));
        }

        public static void Skip(GameSession s) => s.SetFlag(SkipFlag);

        // A new day has begun. If the tip was "go to sleep and wait", the player has done it: the tips rest, with a word to say so, and come back when a crop is ripe.
        public static void OnNewDay(GameSession s)
        {
            if (StepOf(s) != "wait") return;
            s.SetFlag(RestingFlag);
            s.Toast(L.Get("tutorial.coach.resting"));
        }

        static bool HasRipeCrop(GameSession s)
        {
            var grid = s.GetGrid(MapIds.Farm);
            return grid != null && grid.Tiles.Any(t => t.Crop != null && !t.Crop.Withered && s.Db.TryGetCrop(t.Crop.CropId, out var def) && t.Crop.Stage >= def.MatureStage);
        }
    }
}
