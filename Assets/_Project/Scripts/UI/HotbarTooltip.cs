using Farm.Core;
using Farm.Data;
using Farm.Gameplay;

namespace Farm.UI
{
    // The text of the label that appears when the mouse rests on a hotbar slot: what the item is and, for a tool, how and on what to use it.
    public static class HotbarTooltip
    {
        public static string Text(GameSession session, string itemId)
        {
            if (itemId == null || !session.Db.TryGetItem(itemId, out var item)) return null;
            var text = item.IsTool ? session.ToolTitle(item.Id, session.ToolTier(item.Id)) : L.Get(item.NameKey);
            var description = L.Get(item.DescriptionKey);
            if (!string.IsNullOrEmpty(description)) text += "\n" + description;
            if (item.IsTool)
            {
                var use = UseKey(item.ToolType);
                if (use != null) text += "\n" + L.Get("hotbar.how_to_use", L.Get(use));
            }
            else if (item.SellPrice > 0) text += "\n" + L.Get("inventory.sell_value", item.SellPrice);
            return text;
        }

        public static string UseKey(ToolType tool) => tool == ToolType.None ? null : "tool.use." + tool.ToString().ToLowerInvariant();
    }
}
