using System;
using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    // A lot for the shipping bin (playtest feedback, 2026-10-04: the bin should be a window where several things can be moved over and sold
    // together). The lot is how many of each backpack stack the player has put aside; nothing leaves the pack until the lot is shipped.
    public sealed class ShippingLot
    {
        readonly Dictionary<int, int> _qty = new Dictionary<int, int>();

        // True when the stack in `slot` can be sold: not a tool, and it has a price.
        public static bool CanShip(GameSession s, int slot)
        {
            var stack = slot >= 0 && slot < s.Backpack.Capacity ? s.Backpack.Get(slot) : null;
            return stack != null && s.Db.TryGetItem(stack.ItemId, out var item) && !item.IsTool && item.SellPrice > 0;
        }

        public int Get(int slot) => _qty.TryGetValue(slot, out var n) ? n : 0;

        // Sets how many of the stack go in the lot, kept between none and the whole stack. Returns the new amount.
        public int Set(GameSession s, int slot, int quantity)
        {
            if (!CanShip(s, slot)) { _qty.Remove(slot); return 0; }
            var q = Math.Max(0, Math.Min(quantity, s.Backpack.Get(slot).Count));
            if (q == 0) _qty.Remove(slot); else _qty[slot] = q;
            return q;
        }

        public int Add(GameSession s, int slot, int delta) => Set(s, slot, Get(slot) + delta);
        public int All(GameSession s, int slot) => Set(s, slot, int.MaxValue);
        public int None(int slot) { _qty.Remove(slot); return 0; }

        public int ItemCount => _qty.Values.Sum();
        public bool IsEmpty => _qty.Count == 0;

        // What the lot is worth (the same sum the shipping bin pays overnight, including quality and professions).
        public int Gold(GameSession s) => _qty.Sum(pair => ValueOf(s, pair.Key, pair.Value));

        public static int ValueOf(GameSession s, int slot, int count)
        {
            var stack = slot >= 0 && slot < s.Backpack.Capacity ? s.Backpack.Get(slot) : null;
            if (stack == null || !s.Db.TryGetItem(stack.ItemId, out var item)) return 0;
            return (int)(DayCycle.SellValue(item, stack.Quality, Math.Min(count, stack.Count)) * Professions.SellMultiplier(s.State, item));
        }

        // Moves the whole lot into the bin. Returns how many items were shipped.
        public int Ship(GameSession s)
        {
            var shipped = 0;
            foreach (var pair in _qty.OrderBy(p => p.Key).ToList())
                if (s.ShipSlot(pair.Key, pair.Value)) shipped += pair.Value;
            _qty.Clear();
            return shipped;
        }
    }
}
