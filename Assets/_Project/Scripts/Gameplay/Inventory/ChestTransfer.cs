namespace Farm.Gameplay
{
    // Moves a whole stack between the backpack and a chest (the chest screen's click). A stack that goes into a chest remembers the
    // backpack slot it came from and goes back there when taken out, if that slot is free (so the tool bar is as the player left it).
    public static class ChestTransfer
    {
        // Returns how many items did not fit on the other side (they stay where they were).
        public static int Move(Inventory source, int slot, Inventory target, bool intoChest)
        {
            var stack = source.Get(slot);
            if (stack == null) return 0;
            var removed = source.RemoveFromSlot(slot, stack.Count);
            var leftover = intoChest
                ? target.Add(removed.ItemId, removed.Count, removed.Quality, removed.Mark, home: slot + 1)
                : target.Add(removed.ItemId, removed.Count, removed.Quality, removed.Mark, preferredSlot: stack.Home - 1);
            if (leftover > 0) source.Add(removed.ItemId, leftover, removed.Quality, removed.Mark, preferredSlot: slot, home: stack.Home);   // no room: put the rest back
            return leftover;
        }
    }
}
