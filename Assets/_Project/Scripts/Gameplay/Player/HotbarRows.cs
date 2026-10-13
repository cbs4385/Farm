using UnityEngine;

namespace Farm.Gameplay
{
    // The item bar shows twelve slots at a time. A bigger backpack has more rows of twelve: the bar shows the row that holds the selected slot, and the row buttons (or
    // the keys, or the triggers of a pad) move the selection to the same place in the next row (playtest 2026-10-13).
    public static class HotbarRows
    {
        public const int Width = 12;

        public static int Rows(int capacity) => Mathf.Max(1, (capacity + Width - 1) / Width);
        public static int RowOf(int slot) => Mathf.Max(0, slot) / Width;
        public static int ColumnOf(int slot) => Mathf.Max(0, slot) % Width;
        public static int Slot(int row, int column) => row * Width + Mathf.Clamp(column, 0, Width - 1);

        // The slot a number key picks: that place in the row the bar shows now.
        public static int Pick(int selected, int column, int capacity) => Mathf.Min(Slot(RowOf(selected), column), Mathf.Max(0, capacity - 1));

        // One place along the row, round to the start at the end.
        public static int Step(int selected, int delta, int capacity)
        {
            var row = RowOf(selected);
            var inRow = Mathf.Min(Width, capacity - row * Width);
            if (inRow <= 0) return selected;
            return Slot(row, ((ColumnOf(selected) + delta) % inRow + inRow) % inRow);
        }

        // The same place in the next (or the previous) row, round to the first at the end; a short last row takes its last slot.
        public static int ChangeRow(int selected, int delta, int capacity)
        {
            var rows = Rows(capacity);
            var row = ((RowOf(selected) + delta) % rows + rows) % rows;
            return Mathf.Min(Slot(row, ColumnOf(selected)), Mathf.Max(0, capacity - 1));
        }
    }
}
