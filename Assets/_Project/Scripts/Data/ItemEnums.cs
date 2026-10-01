using System;
using Farm.Core;

namespace Farm.Data
{
    public enum ItemCategory { Misc = 0, Seed, Crop, Forage, Fish, Resource, Tool, Food, Artisan }

    public enum ToolType { None = 0, Hoe, WateringCan, Axe, Pickaxe, Scythe, Rod }

    [Flags]
    public enum SeasonMask { None = 0, Spring = 1, Summer = 2, Fall = 4, Winter = 8, All = 15 }

    public static class SeasonMaskExtensions
    {
        public static bool Includes(this SeasonMask mask, Season season) => ((int)mask & (1 << (int)season)) != 0;
    }
}
