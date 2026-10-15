using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Rearranging the item bar on the main screen: the mouse drags a slot onto another, the keyboard and a pad pick a slot up and put it down (PlayerActions reads the
    // buttons, the bar draws the state). All of it ends in HotbarRearrange.Move / Inventory.Move, the backpack window's own move.
    public sealed partial class GameSession
    {
        public HotbarRearrange BarRearrange { get; } = new HotbarRearrange();

        public bool Rearranging => BarRearrange.Active;

        void SelectOnBar(int slot, string source)
        {
            var before = State.SelectedHotbar;
            State.SelectedHotbar = Mathf.Clamp(slot, 0, Mathf.Max(InputNames.HotbarSlots, Backpack.Capacity) - 1);
            HotbarRows.Note(before, State.SelectedHotbar, source);
            _bus.Publish(new StatsChanged());
        }

        // Picks up the stack in the selected slot. A slot with nothing in it says so.
        public bool BeginRearrange()
        {
            if (!BarRearrange.Begin(Backpack, State.SelectedHotbar)) { Toast(L.Get("toast.rearrange_empty")); return false; }
            _bus.Publish(new StatsChanged());
            return true;
        }

        public void RearrangeStep(int delta) { BarRearrange.Step(Backpack, delta); SelectOnBar(BarRearrange.Cursor, "rearranging"); }
        public void RearrangeRow(int delta) { BarRearrange.ChangeRow(Backpack, delta); SelectOnBar(BarRearrange.Cursor, "rearranging"); }

        // Puts the picked stack down on the cursor's slot, or, with a slot given, on that one (a number key, a click on the bar).
        public void RearrangeDrop(int slot = -1)
        {
            if (!BarRearrange.Active) return;
            if (slot >= 0) BarRearrange.Goto(Backpack, slot);
            var target = BarRearrange.Drop(Backpack);
            SelectOnBar(target, "rearranged");
        }

        public void RearrangeCancel()
        {
            if (!BarRearrange.Active) return;
            SelectOnBar(BarRearrange.Cancel(), "rearrange cancelled");
        }

        // A slot dragged with the mouse and let go over another slot of the bar.
        public void MoveOnBar(int from, int to)
        {
            var selected = HotbarRearrange.Move(Backpack, from, to, State.SelectedHotbar);
            SelectOnBar(selected, "dragged on the bar");
        }
    }
}
