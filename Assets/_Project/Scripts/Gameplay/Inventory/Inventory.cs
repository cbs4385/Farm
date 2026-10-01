using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    [Serializable]
    public sealed class ItemStack
    {
        public string ItemId;
        public int Count;
        public int Quality;   // 0 normal, 1 silver, 2 gold, 3 iridium

        public ItemStack() { }

        public ItemStack(string itemId, int count, int quality = 0)
        {
            ItemId = itemId;
            Count = count;
            Quality = quality;
        }

        public ItemStack Clone() => new ItemStack(ItemId, Count, Quality);
    }

    [Serializable]
    public sealed class InventoryData
    {
        public int Capacity;
        public List<ItemStack> Slots = new List<ItemStack>();   // null entries are empty slots
    }

    // Fixed-size slot inventory used by the backpack, chests, and shop buy-back. Pure C#: stack limits come from
    // the supplied lookup so it can be tested without any assets.
    public sealed class Inventory
    {
        readonly Func<string, int> _maxStack;
        ItemStack[] _slots;

        public Inventory(int capacity, Func<string, int> maxStack)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _maxStack = maxStack ?? throw new ArgumentNullException(nameof(maxStack));
            _slots = new ItemStack[capacity];
        }

        public int Capacity => _slots.Length;

        public event Action Changed;

        // Returns the stack in the slot (do not mutate), or null if empty.
        public ItemStack Get(int slot) => _slots[slot];

        public int Count(string itemId)
        {
            var total = 0;
            foreach (var s in _slots)
                if (s != null && s.ItemId == itemId) total += s.Count;
            return total;
        }

        public bool Has(string itemId, int count = 1) => Count(itemId) >= count;

        public bool CanAdd(string itemId, int count, int quality = 0)
        {
            var space = 0;
            var max = _maxStack(itemId);
            foreach (var s in _slots)
            {
                if (s == null) space += max;
                else if (s.ItemId == itemId && s.Quality == quality) space += max - s.Count;
                if (space >= count) return true;
            }
            return space >= count;
        }

        // Returns how many items did NOT fit.
        public int Add(string itemId, int count, int quality = 0)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("itemId required", nameof(itemId));
            if (count <= 0) return 0;
            var max = _maxStack(itemId);
            var remaining = count;
            var changed = false;

            for (var i = 0; i < _slots.Length && remaining > 0; i++)
            {
                var s = _slots[i];
                if (s == null || s.ItemId != itemId || s.Quality != quality || s.Count >= max) continue;
                var add = Math.Min(remaining, max - s.Count);
                s.Count += add;
                remaining -= add;
                changed = true;
            }

            for (var i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i] != null) continue;
                var add = Math.Min(remaining, max);
                _slots[i] = new ItemStack(itemId, add, quality);
                remaining -= add;
                changed = true;
            }

            if (changed) Changed?.Invoke();
            return remaining;
        }

        // Removes up to `count` items (lowest slots first). Returns how many were actually removed.
        public int Remove(string itemId, int count)
        {
            var remaining = count;
            var changed = false;
            for (var i = 0; i < _slots.Length && remaining > 0; i++)
            {
                var s = _slots[i];
                if (s == null || s.ItemId != itemId) continue;
                var take = Math.Min(remaining, s.Count);
                s.Count -= take;
                remaining -= take;
                if (s.Count == 0) _slots[i] = null;
                changed = true;
            }
            if (changed) Changed?.Invoke();
            return count - remaining;
        }

        // Removes from one specific slot. Returns the removed stack (or null).
        public ItemStack RemoveFromSlot(int slot, int count)
        {
            var s = _slots[slot];
            if (s == null || count <= 0) return null;
            var take = Math.Min(count, s.Count);
            var removed = new ItemStack(s.ItemId, take, s.Quality);
            s.Count -= take;
            if (s.Count == 0) _slots[slot] = null;
            Changed?.Invoke();
            return removed;
        }

        // Swaps two slots, or merges `from` into `to` when they hold the same item and quality.
        public void Move(int from, int to)
        {
            if (from == to) return;
            var a = _slots[from];
            var b = _slots[to];
            if (a == null) return;

            if (b != null && a.ItemId == b.ItemId && a.Quality == b.Quality)
            {
                var max = _maxStack(a.ItemId);
                var moved = Math.Min(a.Count, max - b.Count);
                b.Count += moved;
                a.Count -= moved;
                if (a.Count == 0) _slots[from] = null;
            }
            else
            {
                _slots[from] = b;
                _slots[to] = a;
            }
            Changed?.Invoke();
        }

        // Grows the inventory (backpack upgrades). Never shrinks.
        public void Resize(int newCapacity)
        {
            if (newCapacity <= _slots.Length) return;
            Array.Resize(ref _slots, newCapacity);
            Changed?.Invoke();
        }

        public InventoryData ToData()
        {
            var data = new InventoryData { Capacity = _slots.Length };
            foreach (var s in _slots) data.Slots.Add(s?.Clone());
            return data;
        }

        public static Inventory FromData(InventoryData data, Func<string, int> maxStack)
        {
            var inv = new Inventory(Math.Max(1, data.Capacity), maxStack);
            for (var i = 0; i < data.Slots.Count && i < inv._slots.Length; i++)
                inv._slots[i] = data.Slots[i]?.Clone();
            return inv;
        }
    }
}
