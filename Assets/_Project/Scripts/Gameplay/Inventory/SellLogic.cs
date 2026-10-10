namespace Farm.Gameplay
{
    // Selling to the general store's keeper in conversation (playtest 2026-10-09): the same things the shipping bin takes, paid in gold at once at the same prices
    // (quality and professions included) instead of overnight.
    public static class SellLogic
    {
        public static bool CanSell(GameSession s, int slot) => ShippingLot.CanShip(s, slot);

        public static bool AnythingToSell(GameSession s)
        {
            for (var i = 0; i < s.Backpack.Capacity; i++)
                if (CanSell(s, i)) return true;
            return false;
        }

        public static int ValueOf(GameSession s, int slot, int count) => ShippingLot.ValueOf(s, slot, count);

        // Sells up to `count` of the stack in `slot`. Returns the gold paid (0 when nothing was sold).
        public static int Sell(GameSession s, int slot, int count)
        {
            if (!CanSell(s, slot) || count <= 0) return 0;
            var gold = ValueOf(s, slot, count);
            var removed = s.Backpack.RemoveFromSlot(slot, count);
            if (removed == null) return 0;
            s.AddGold(gold);
            return gold;
        }
    }
}
