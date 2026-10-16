using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farm.UI
{
    // The right stick of a pad scrolls the list on the open screen: pages of plain text (the gazette, a long letter) have nothing to focus, so the cursor can never
    // bring the lower part into view (playtest 2026-10-14, pad only).
    public static class PadScroll
    {
        public const float Speed = 700f;          // reference-canvas units per second at full tilt
        const float Dead = 0.25f;
        static readonly List<ScrollRect> Found = new List<ScrollRect>();

        // `sensitivity` is the controller sensitivity setting: the tilt of the stick counts that many times (never past full), so the page scrolls faster or slower with the slider.
        public static void Tick(GameObject root, float dt, float sensitivity = 1f)
        {
            var pad = Gamepad.current;
            if (pad == null || root == null) return;
            var tilt = Mathf.Clamp(pad.rightStick.ReadValue().y * sensitivity, -1f, 1f);
            if (Mathf.Abs(tilt) < Dead) return;
            var rect = Pick(root);
            if (rect == null) return;
            var range = rect.content.rect.height - ViewHeight(rect);
            rect.verticalNormalizedPosition = Step(rect.verticalNormalizedPosition, tilt, range, dt);
        }

        // The list that holds the focused control, else the first one on screen that has more to show than fits. (pure given the scene)
        public static ScrollRect Pick(GameObject root)
        {
            Found.Clear();
            root.GetComponentsInChildren<ScrollRect>(false, Found);
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            ScrollRect first = null;
            foreach (var rect in Found)
            {
                if (rect.content == null || rect.content.rect.height - ViewHeight(rect) <= 0f) continue;
                if (selected != null && selected.transform.IsChildOf(rect.content)) return rect;
                if (first == null) first = rect;
            }
            return first;
        }

        // Up on the stick moves the view up the page. (pure)
        public static float Step(float normalized, float tilt, float range, float dt)
        {
            if (range <= 0f) return normalized;
            return Mathf.Clamp01(normalized + tilt * Speed * dt / range);
        }

        static float ViewHeight(ScrollRect rect) => (rect.viewport != null ? rect.viewport : (RectTransform)rect.transform).rect.height;
    }
}
