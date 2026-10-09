namespace Farm.Gameplay
{
    // Moves stacks between the backpack and a chest (the chest screen). A stack that goes into a chest remembers the backpack slot it came from and goes back
    // there when taken out by a plain click, if that slot is free (so the tool bar is as the player left it). The player can also move just part of a stack, or
    // drop a stack on any slot to put it exactly there (playtests 2026-10-09: tools always went back to the same spot, and a whole stack was the only choice).
    public static class ChestTransfer
    {
        // Moves `count` of the stack in `slot` (all of it by default) to the other side, merging into stacks that are there and filling the first free slot, or
        // `preferredSlot` first. Returns how many items did not fit on the other side (they stay where they were).
        public static int Move(Inventory source, int slot, Inventory target, bool intoChest, int count = int.MaxValue)
        {
            var stack = source.Get(slot);
            if (stack == null) return 0;
            var removed = source.RemoveFromSlot(slot, System.Math.Min(count, stack.Count));
            var home = stack.Home;
            var leftover = intoChest
                ? target.Add(removed.ItemId, removed.Count, removed.Quality, removed.Mark, home: slot + 1)
                : target.Add(removed.ItemId, removed.Count, removed.Quality, removed.Mark, preferredSlot: home - 1);
            if (leftover > 0) source.Add(removed.ItemId, leftover, removed.Quality, removed.Mark, preferredSlot: slot, home: home);   // no room: put the rest back
            return leftover;
        }

        // Half of a stack, rounded up (one of a single item).
        public static int Half(int count) => (count + 1) / 2;

        // Puts the stack in `fromSlot` exactly on `toSlot`: into an empty slot, merged into the same kind of stack there (what does not fit stays), or swapped with a
        // different one. Within one inventory this is the usual move. Returns true when something moved.
        public static bool Place(Inventory from, int fromSlot, Inventory to, int toSlot, bool intoChest)
        {
            var a = from.Get(fromSlot);
            if (a == null) return false;
            if (from == to)
            {
                if (fromSlot == toSlot) return false;
                from.Move(fromSlot, toSlot);
                return true;
            }
            var b = to.Get(toSlot);
            if (b == null)
            {
                var whole = from.Take(fromSlot);
                whole.Home = intoChest ? fromSlot + 1 : 0;
                to.Put(toSlot, whole);
                return true;
            }
            if (to.SameKind(a, b))
            {
                var room = to.MaxStackOf(b.ItemId) - b.Count;
                if (room <= 0) return false;
                var moved = from.RemoveFromSlot(fromSlot, System.Math.Min(room, a.Count));
                to.Add(moved.ItemId, moved.Count, moved.Quality, moved.Mark, home: intoChest ? fromSlot + 1 : 0);
                return true;
            }
            // Different things: they change places. A stack that lands in the chest remembers the backpack slot it left (or took the place of).
            var first = from.Take(fromSlot);
            var second = to.Take(toSlot);
            first.Home = intoChest ? fromSlot + 1 : 0;
            second.Home = intoChest ? 0 : toSlot + 1;
            to.Put(toSlot, first);
            from.Put(fromSlot, second);
            return true;
        }
    }
}
