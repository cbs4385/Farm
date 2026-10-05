using Farm.Data;

namespace Farm.Gameplay
{
    public enum DiscardResult { Ok, Empty, Protected }

    // Throwing things away (playtest feedback, 2026-10-04): the backpack fills quickly and the only way to empty it was to sell. A stack can be
    // discarded for good; tools cannot, so a player cannot lose their hoe by a slip of the mouse.
    public static class InventoryRules
    {
        public static bool CanDiscard(ItemDefinition item) => item != null && !item.IsTool;

        // Removes the whole stack in `slot`. Returns what happened; `removed` is how many were thrown away.
        public static DiscardResult Discard(GameSession session, int slot, out int removed)
        {
            removed = 0;
            var stack = slot >= 0 && slot < session.Backpack.Capacity ? session.Backpack.Get(slot) : null;
            if (stack == null) return DiscardResult.Empty;
            if (!session.Db.TryGetItem(stack.ItemId, out var item) || !CanDiscard(item)) return DiscardResult.Protected;
            removed = session.Backpack.RemoveFromSlot(slot, stack.Count).Count;
            return DiscardResult.Ok;
        }
    }
}
