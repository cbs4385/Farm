using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine.InputSystem;

namespace Farm.UI
{
    // The rows of the keyboard and mouse table in Options: an action, its primary binding and its secondary one (-1 when it has no second key).
    // Bindings are told apart by their original path, so rebinding a key never moves a binding to another column or another row.
    public static class KeyTable
    {
        public enum Group { Actions, Walking, ItemSlots, Menus }

        public readonly struct Spec
        {
            public readonly Group Group;
            public readonly string Label;
            public readonly InputAction Action;
            public readonly int Primary, Secondary;
            public Spec(Group group, string label, InputAction action, int primary, int secondary) { Group = group; Label = label; Action = action; Primary = primary; Secondary = secondary; }
        }

        public static List<Spec> Rows(InputService input)
        {
            var rows = new List<Spec>();
            foreach (var name in InputNames.Rebindable) Add(rows, Group.Actions, L.Get("action." + name), input.Gameplay[name]);
            AddParts(rows, Group.Walking, input.Move, "action.Move");
            for (var i = 0; i < InputNames.HotbarSlots; i++) Add(rows, Group.ItemSlots, L.Get("action.Hotbar", i + 1), input.Hotbar(i));
            foreach (var name in InputNames.UiRebindable) Add(rows, Group.Menus, L.Get("action." + name), input.Ui[name]);
            AddParts(rows, Group.Menus, input.Ui[InputNames.Navigate], null);
            return rows;
        }

        static bool IsKeyOrMouse(InputBinding b) => !b.isComposite && !b.isPartOfComposite && b.path != null && (b.path.StartsWith("<Keyboard>") || b.path.StartsWith("<Mouse>"));

        static void Add(List<Spec> rows, Group group, string label, InputAction action)
        {
            int primary = -1, secondary = -1;
            for (var i = 0; i < action.bindings.Count; i++)
                if (IsKeyOrMouse(action.bindings[i]) && action.bindings[i].path.StartsWith("<Keyboard>")) { primary = i; break; }
            for (var i = 0; i < action.bindings.Count; i++)
            {
                if (!IsKeyOrMouse(action.bindings[i]) || i == primary) continue;
                if (primary < 0) primary = i;
                else { secondary = i; break; }
            }
            if (primary >= 0) rows.Add(new Spec(group, label, action, primary, secondary));
        }

        // A walking-style action: its keyboard composites (the first is the primary keys, the second the other keys), one row per direction.
        static void AddParts(List<Spec> rows, Group group, InputAction action, string labelKey)
        {
            var composites = new List<int>();
            for (var i = 0; i + 1 < action.bindings.Count; i++)
                if (action.bindings[i].isComposite && action.bindings[i + 1].path != null && action.bindings[i + 1].path.StartsWith("<Keyboard>")) composites.Add(i);
            if (composites.Count == 0) return;
            for (var part = 0; part < InputNames.MoveParts.Length; part++)
            {
                var label = labelKey != null ? L.Get(labelKey + InputNames.MoveParts[part]) : L.Get("options.part", L.Get("action." + action.name), InputNames.MoveParts[part]);
                rows.Add(new Spec(group, label, action, composites[0] + 1 + part, composites.Count > 1 ? composites[1] + 1 + part : -1));
            }
        }
    }
}
