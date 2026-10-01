namespace Farm.Data
{
    // Stable ids referenced from code. Shipped ids must never change (save compatibility).
    public static class ItemIds
    {
        public const string Hoe = "tool.hoe";
        public const string WateringCan = "tool.wateringcan";
        public const string Axe = "tool.axe";
        public const string Pickaxe = "tool.pickaxe";
        public const string Scythe = "tool.scythe";

        public const string Wood = "resource.wood";
        public const string Stone = "resource.stone";
        public const string Fiber = "resource.fiber";

        public static string Seed(string cropId) => $"seed.{cropId}";
        public static string Crop(string cropId) => $"crop.{cropId}";
    }
}
