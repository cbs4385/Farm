using Farm.Core;
using Farm.Data;
using Farm.Gameplay;

namespace Farm.UI
{
    // The words of an item's tooltip: its name (with its quality), what it is, and what it sells for. Shown where a grid of items has no tooltip box of its own (the chest).
    public static class ItemTip
    {
        public static string Text(GameSession session, ItemStack stack)
        {
            if (stack == null || !session.Db.TryGetItem(stack.ItemId, out var item)) return null;
            var name = item.IsTool ? session.ToolTitle(item.Id, session.ToolTier(item.Id)) : SlotBadges.NameWithQuality(L.Get(item.NameKey), stack.Quality);
            var text = name + (stack.Count > 1 ? "  x" + stack.Count : string.Empty);
            var description = L.Get(item.DescriptionKey);
            if (!string.IsNullOrEmpty(description)) text += "\n" + description;
            if (item.SellPrice > 0) text += "\n" + L.Get("inventory.sell_value", item.SellPrice);
            return text;
        }
    }
}
