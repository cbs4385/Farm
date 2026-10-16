using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The focus chain of one page of the Options: every control is a stop in a fixed order, so up and down always go to the row above or below (never to whatever happens to
    // be nearest, which could be a row scrolled out of view or a button outside the list), and the page ends where it ends. Rows are lists of controls, left to right.
    public static class OptionsNav
    {
        // A slider and a control with a ValueStepper use left and right for their own value.
        public static bool KeepsSideways(Selectable s) => s is Slider || s.GetComponent<ValueStepper>() != null;

        // Up and down keep to the column (the same position in the next row, or its last control when that row is shorter); left and right walk along the row, and left from
        // the first control of a row goes to `list` (the category list; null for none). Nothing wraps round.
        public static void Wire(IReadOnlyList<List<Selectable>> rows, Selectable list)
        {
            for (var i = 0; i < rows.Count; i++)
                for (var j = 0; j < rows[i].Count; j++)
                {
                    var cell = rows[i][j];
                    var nav = new Navigation { mode = Navigation.Mode.Explicit };
                    if (i > 0) nav.selectOnUp = rows[i - 1][Mathf.Min(j, rows[i - 1].Count - 1)];
                    if (i < rows.Count - 1) nav.selectOnDown = rows[i + 1][Mathf.Min(j, rows[i + 1].Count - 1)];
                    if (!KeepsSideways(cell))
                    {
                        nav.selectOnLeft = j > 0 ? rows[i][j - 1] : list;
                        nav.selectOnRight = j < rows[i].Count - 1 ? rows[i][j + 1] : null;
                    }
                    cell.navigation = nav;
                }
        }

        // The category list: up and down walk it (no wrap), right goes into the page.
        public static void WireList(IReadOnlyList<Selectable> list, Selectable intoPage)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit };
                if (i > 0) nav.selectOnUp = list[i - 1];
                if (i < list.Count - 1) nav.selectOnDown = list[i + 1];
                nav.selectOnRight = intoPage;
                list[i].navigation = nav;
            }
        }
    }
}
