using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Platform;

namespace Farm.Gameplay
{
    // T-062: achievements as data. Each is a condition over the story state (flags, vars, quests, the calendar), so layers add
    // their own through Register without the core knowing them. Titles and descriptions are generic on purpose: they must not
    // spoil quests or the optional horror story. Earned ones are kept as flags (`ach.<id>`) and handed to the platform.
    public readonly struct AchievementDefinition
    {
        public readonly string Id, Condition;
        public AchievementDefinition(string id, string condition) { Id = id; Condition = condition; }
        public string TitleKey => $"ach.{Id}.name";
        public string DescriptionKey => $"ach.{Id}.desc";
    }

    public static class Achievements
    {
        static readonly List<AchievementDefinition> CoreList = new List<AchievementDefinition>
        {
            new AchievementDefinition("first_furrows", "quest:tut_farm=done"),
            new AchievementDefinition("first_harvest", "quest:tut_harvest=done"),
            new AchievementDefinition("neighbour", "quest:tut_village=done"),
            new AchievementDefinition("hall", "flag:hall.restored"),
            new AchievementDefinition("second_year", "year>=2"),
            new AchievementDefinition("third_year", "year>=3"),
        };
        static readonly List<AchievementDefinition> Extra = new List<AchievementDefinition>();

        public static IEnumerable<AchievementDefinition> All => CoreList.Concat(Extra);

        public static string FlagFor(string id) => "ach." + id;

        // Optional layers add theirs (once per id).
        public static void Register(AchievementDefinition def)
        {
            if (All.All(a => a.Id != def.Id)) Extra.Add(def);
        }

        public static void ClearExtraForTests() => Extra.Clear();

        public static bool IsEarned(GameSession s, string id) => s.HasFlag(FlagFor(id));

        // Evaluates every unearned achievement; returns the ids unlocked now. A failing condition never stops the others.
        public static List<string> Check(GameSession s)
        {
            var unlocked = new List<string>();
            if (!s.InGame || _checking) return unlocked;
            _checking = true;
            try { CheckAll(s, unlocked); }
            finally { _checking = false; }
            return unlocked;
        }

        static bool _checking;
        static readonly HashSet<string> Broken = new HashSet<string>();

        static void CheckAll(GameSession s, List<string> unlocked)
        {
            foreach (var a in All.ToList())
            {
                if (IsEarned(s, a.Id)) continue;
                bool met;
                try { met = Conditions.Evaluate(a.Condition, s.World); }
                catch (System.Exception e)
                {
                    if (Broken.Add(a.Id)) Log.Warn($"Achievement {a.Id} cannot be checked: {e.Message}");   // once; its layer may not be installed
                    continue;
                }
                if (!met) continue;
                s.SetFlag(FlagFor(a.Id));
                PlatformServices.Current.UnlockAchievement(a.Id);
                s.Toast(L.Get("ach.unlocked", L.Get(a.TitleKey)));
                unlocked.Add(a.Id);
            }
        }

        // After loading a save: tell the platform about everything already earned (it ignores repeats).
        public static void SyncToPlatform(GameSession s)
        {
            foreach (var a in All) if (IsEarned(s, a.Id)) PlatformServices.Current.UnlockAchievement(a.Id);
        }
    }
}
