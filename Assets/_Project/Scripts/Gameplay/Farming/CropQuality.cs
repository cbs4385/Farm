using System;
using Farm.Data;

namespace Farm.Gameplay
{
    // How good a harvested crop is: the Farming level, quality fertilizer and the player's luck raise the chance of a
    // better quality (silver, gold). Pure and tested; `roll` is a number in [0, 1).
    public static class CropQuality
    {
        public const int Normal = 0, Silver = 1, Gold = 2;

        public static float GoldChance(int level, string fertilizer, float luck) =>
            0.012f * (Math.Max(1, level) - 1) + (fertilizer == ItemIds.FertilizerQuality ? 0.12f : 0f) + 0.08f * Math.Max(0f, luck);

        public static float SilverChance(int level, string fertilizer, float luck) =>
            0.04f * (Math.Max(1, level) - 1) + (fertilizer == ItemIds.FertilizerQuality ? 0.25f : 0f) + 0.12f * Math.Max(0f, luck);

        public static int Roll(int level, string fertilizer, float luck, float roll)
        {
            var gold = GoldChance(level, fertilizer, luck);
            if (roll < gold) return Gold;
            return roll < gold + SilverChance(level, fertilizer, luck) ? Silver : Normal;
        }
    }
}
