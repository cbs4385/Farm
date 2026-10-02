using System;

namespace Farm.Gameplay
{
    public static class SkillIds
    {
        public const string Farming = "farming";
        public const string Foraging = "foraging";
        public const string Mining = "mining";
        public const string Fishing = "fishing";
        public const string Combat = "combat";

        public static readonly string[] All = { Farming, Foraging, Mining, Fishing, Combat };
    }

    // Skill levels 1-10 earned by use. A level is reached at a total amount of XP (pure arithmetic).
    public static class SkillModel
    {
        public const int MaxLevel = 10;

        // Total XP needed to reach level 2, 3, ... 10.
        static readonly int[] Thresholds = { 100, 250, 450, 700, 1000, 1400, 1900, 2500, 3200 };

        public static int LevelForXp(int xp)
        {
            var level = 1;
            foreach (var t in Thresholds) if (xp >= t) level++;
            return Math.Min(level, MaxLevel);
        }

        // Total XP at which `level` is reached (0 for level 1).
        public static int XpForLevel(int level)
        {
            if (level <= 1) return 0;
            return Thresholds[Math.Min(level, MaxLevel) - 2];
        }

        // 0..1 progress from the current level towards the next (1 at the cap).
        public static float Progress(int xp)
        {
            var level = LevelForXp(xp);
            if (level >= MaxLevel) return 1f;
            var from = XpForLevel(level);
            var to = XpForLevel(level + 1);
            return (xp - from) / (float)(to - from);
        }

        public static bool IsKnown(string skill) => Array.IndexOf(SkillIds.All, skill) >= 0;
    }
}
