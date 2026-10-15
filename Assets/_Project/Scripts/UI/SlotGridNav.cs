using System.Collections.Generic;
using UnityEngine.UI;

namespace Farm.UI
{
    // The pad's and the arrow keys' way across a grid of item slots, written down instead of left to Unity's "automatic" guess (which picks the nearest control in a
    // direction by position, and so could leave the grid for a button of the item bar behind the window or skip over a short last row; playtest 2026-10-15: "the
    // controller is selecting things I don't want to select"). Rows of `columns` slots, no wrapping: at an edge the cursor stays where it is. Down from the row above a
    // short last row goes to the last slot, and down from the last row goes to `below` (a button under the grid) when there is one.
    public static class SlotGridNav
    {
        public enum Direction { Up, Down, Left, Right }

        // The slot reached from `index` in a grid of `count` slots, or -1 when the cursor stays.
        public static int Next(int index, int count, int columns, Direction direction)
        {
            if (index < 0 || index >= count || columns <= 0) return -1;
            switch (direction)
            {
                case Direction.Left: return index % columns > 0 ? index - 1 : -1;
                case Direction.Right: return index % columns < columns - 1 && index + 1 < count ? index + 1 : -1;
                case Direction.Up: return index >= columns ? index - columns : -1;
                default:
                    if (index + columns < count) return index + columns;
                    return index / columns < (count - 1) / columns ? count - 1 : -1;          // a short last row below: its last slot
            }
        }

        // Gives every slot explicit neighbours (see Next). `below` is where down from the last row goes (null: nowhere); its own navigation is the caller's.
        public static void Link(IList<Selectable> slots, int columns, Selectable below = null)
        {
            var count = slots.Count;
            for (var i = 0; i < count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = At(slots, Next(i, count, columns, Direction.Up)),
                    selectOnDown = At(slots, Next(i, count, columns, Direction.Down)),
                    selectOnLeft = At(slots, Next(i, count, columns, Direction.Left)),
                    selectOnRight = At(slots, Next(i, count, columns, Direction.Right)),
                };
                if (nav.selectOnDown == null && i / columns == (count - 1) / columns) nav.selectOnDown = below;
                slots[i].navigation = nav;
            }
        }

        static Selectable At(IList<Selectable> slots, int index) => index >= 0 ? slots[index] : null;
    }
}
