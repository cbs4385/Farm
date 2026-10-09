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
        public const string Acorn = "resource.acorn";     // dropped by felled trees; planted on grass it grows into a tree
        public const string CopperBar = "resource.copperbar";
        public const string IronBar = "resource.ironbar";
        public const string GoldBar = "resource.goldbar";
        public const string Sword = "tool.sword";
        public const string Hammer = "tool.hammer";         // the builder's mallet: lifts and sets down buildings, furniture and fixtures
        public const string Coal = "resource.coal";
        public const string CopperOre = "resource.copperore";
        public const string IronOre = "resource.ironore";
        public const string GoldOre = "resource.goldore";

        public const string FertilizerQuality = "fertilizer.quality";
        public const string FertilizerSpeed = "fertilizer.speed";

        public static string Machine(string placeableId) => $"machine.{placeableId}";

        public static string Seed(string cropId) => $"seed.{cropId}";
        public static string Crop(string cropId) => $"crop.{cropId}";
    }
}
