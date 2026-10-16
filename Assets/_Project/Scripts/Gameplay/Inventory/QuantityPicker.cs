using System;
using Farm.Data;

namespace Farm.Gameplay
{
    // The arithmetic behind the "how many?" window that opens when a stack is chosen to be sold, shipped or bought. Pure, so that it can be tested. The number is always kept
    // between `min` and `max`, and a stack given away can never be asked for more than the backpack holds.
    public static class QuantityPicker
    {
        public static int Clamp(int value, int min, int max) => max < min ? min : Math.Max(min, Math.Min(max, value));

        public static int Nudge(int value, int delta, int min, int max) => Clamp(value + delta, min, max);

        // The number typed into the box: digits only (a minus sign or a letter is ignored), nothing typed is the minimum, a number too big for an int is the maximum.
        public static int Parse(string text, int min, int max)
        {
            if (string.IsNullOrEmpty(text)) return min;
            long value = 0;
            var any = false;
            foreach (var c in text)
            {
                if (c < '0' || c > '9') continue;
                any = true;
                value = value * 10 + (c - '0');
                if (value > int.MaxValue) return max;
            }
            return any ? Clamp((int)value, min, max) : min;
        }

        // How many of the stack in `slot` can be given away: the whole stack, never more than the backpack holds (0 when the slot is empty).
        public static int MaxToGive(GameSession s, int slot)
        {
            var stack = slot >= 0 && slot < s.Backpack.Capacity ? s.Backpack.Get(slot) : null;
            return stack == null ? 0 : stack.Count;
        }

        // How many of an item can be bought now: what the gold pays for and the backpack has room for.
        public static int MaxToBuy(GameSession s, ItemDefinition item)
        {
            if (item == null || item.BuyPrice <= 0) return 0;
            var low = 0;
            var high = s.State.Gold / item.BuyPrice;
            while (low < high)                                          // room in the backpack only ever shrinks as the number grows, so the biggest that fits can be found by halving
            {
                var mid = (low + high + 1) / 2;
                if (s.Backpack.CanAdd(item.Id, mid)) low = mid; else high = mid - 1;
            }
            return low;
        }
    }
}
