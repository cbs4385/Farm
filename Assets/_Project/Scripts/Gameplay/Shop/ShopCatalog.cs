using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // What a shop offers today. Items opt in to shops (ItemDefinition.SoldIn) and may add a daily condition, so
    // optional content is only ever sold where its data says so: the general store never stocks, say, strange
    // seeds from a content pack unless that pack lists "general" for them.
    public static class ShopCatalog
    {
        public const string MerchantShopId = "merchant";

        public static List<ItemDefinition> For(GameDatabase db, string shopId, IWorldQuery world)
        {
            if (shopId == MerchantShopId && !(world != null && Conditions.TryEvaluate("merchant:today", world, out var up) && up)) return new List<ItemDefinition>();          // no stall, no stock
            return db.AllItems
                .Where(i => i.BuyPrice > 0 && i.SoldIn.Contains(shopId))
                .Where(i => string.IsNullOrWhiteSpace(i.SaleCondition) || (world != null && Conditions.TryEvaluate(i.SaleCondition, world, out var ok) && ok))
                .OrderBy(i => i.BuyPrice)
                .ToList();
        }
    }
}
