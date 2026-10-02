using System;
using Farm.Data;

namespace Farm.Gameplay
{
    // What a tool tier does (pure arithmetic): tiers are 0 basic, 1 copper, 2 iron, 3 gold. Better tools cost less
    // energy and hit harder (a swing deals 1 plus the tier in damage to a tree or rock).
    public static class ToolModel
    {
        public const int MaxTier = 3;
        static readonly float[] EnergyMultiplier = { 1f, 0.85f, 0.7f, 0.55f };
        static readonly string[] TierKeys = { "tier.basic", "tier.copper", "tier.iron", "tier.gold" };

        public static int ClampTier(int tier) => Math.Max(0, Math.Min(MaxTier, tier));

        public static int EnergyCost(int baseCost, int tier) =>
            baseCost <= 0 ? 0 : Math.Max(1, (int)Math.Round(baseCost * EnergyMultiplier[ClampTier(tier)], MidpointRounding.AwayFromZero));

        public static int Damage(int tier) => 1 + ClampTier(tier);

        public static string TierKey(int tier) => TierKeys[ClampTier(tier)];

        public static string ItemId(ToolType tool)
        {
            switch (tool)
            {
                case ToolType.Hoe: return ItemIds.Hoe;
                case ToolType.WateringCan: return ItemIds.WateringCan;
                case ToolType.Axe: return ItemIds.Axe;
                case ToolType.Pickaxe: return ItemIds.Pickaxe;
                case ToolType.Scythe: return ItemIds.Scythe;
                default: return null;
            }
        }
    }
}
