using System;

namespace Farm.Gameplay
{
    // Moving things about on the item bar without opening the backpack (playtest 2026-10-15: "my pickaxe is far from my sword"). The rules are plain logic over the
    // backpack model, the same Inventory.Move the backpack window uses, so a move here and a move there can never differ; the mouse (a drag), the keyboard and a pad
    // all end in Move below, and PlayerActions only reads the buttons. While a stack is picked up the selected slot is the one it would be put down on, so the bar
    // shows the right row and the picked item can be seen where it will land.
    public sealed class HotbarRearrange
    {
        public const float PadTapSeconds = 0.45f;          // the pad's button is a tap (rearrange) or a hold (photo mode): the left stick has no other free button

        public bool Active { get; private set; }
        public int Picked { get; private set; } = -1;
        public int Cursor { get; private set; } = -1;

        // Picks up what is in the slot. False (and nothing happens) when the slot is empty.
        public bool Begin(Inventory inv, int slot)
        {
            if (inv == null || slot < 0 || slot >= inv.Capacity || inv.Get(slot) == null) return false;
            Active = true;
            Picked = Cursor = slot;
            return true;
        }

        public int Step(Inventory inv, int delta) => Cursor = Active ? HotbarRows.Step(Cursor, delta, inv.Capacity) : Cursor;
        public int ChangeRow(Inventory inv, int delta) => Cursor = Active ? HotbarRows.ChangeRow(Cursor, delta, inv.Capacity) : Cursor;
        public int PickColumn(Inventory inv, int column) => Cursor = Active ? HotbarRows.Pick(Cursor, column, inv.Capacity) : Cursor;
        public int Goto(Inventory inv, int slot) => Cursor = Active ? Math.Max(0, Math.Min(slot, inv.Capacity - 1)) : Cursor;

        // Puts the picked stack down on the cursor's slot (swapping, or merging a stack of the same kind), and ends the pick up. Returns the slot the picked item went to.
        public int Drop(Inventory inv)
        {
            if (!Active) return -1;
            var target = Cursor;
            if (Picked >= 0 && Picked < inv.Capacity && target >= 0 && target < inv.Capacity) inv.Move(Picked, target);
            End();
            return target;
        }

        // Ends the pick up with nothing moved; returns the slot that was picked (the selection goes back to it).
        public int Cancel()
        {
            var was = Picked;
            End();
            return was;
        }

        void End()
        {
            Active = false;
            Picked = Cursor = -1;
        }

        // The one move every way of rearranging the bar ends in (a drag with the mouse, a drop with the keyboard or a pad). Swaps, or merges the same kind; the slot that
        // was selected follows its item when that is the one moved. Returns the selected slot afterwards.
        public static int Move(Inventory inv, int from, int to, int selected)
        {
            if (inv == null || from == to || from < 0 || to < 0 || from >= inv.Capacity || to >= inv.Capacity || inv.Get(from) == null) return selected;
            inv.Move(from, to);
            return selected == from ? to : selected;
        }
    }
}
