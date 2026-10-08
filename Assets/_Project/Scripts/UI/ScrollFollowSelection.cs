using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // Keeps the focused row of a scrolling list in view when the keyboard or a gamepad moves the focus (the mouse wheel and the scrollbar scroll on their own).
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollFollowSelection : MonoBehaviour
    {
        ScrollRect _scroll;
        GameObject _last;

        void Awake() => _scroll = GetComponent<ScrollRect>();

        void LateUpdate()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == _last) return;
            _last = selected;
            if (selected != null && _scroll.content != null && selected.transform.IsChildOf(_scroll.content)) Bring((RectTransform)selected.transform);
        }

        // Scrolls just far enough that the row shows completely. (pure given the layout)
        public void Bring(RectTransform row)
        {
            Canvas.ForceUpdateCanvases();
            var content = _scroll.content;
            var room = content.rect.height - ((RectTransform)_scroll.viewport).rect.height;
            if (room <= 0f) return;
            var top = -content.InverseTransformPoint(row.TransformPoint(new Vector3(0f, row.rect.yMax, 0f))).y;      // how far below the top of the list the row starts
            var offset = (1f - _scroll.verticalNormalizedPosition) * room;                                            // how far the list is scrolled now
            var view = ((RectTransform)_scroll.viewport).rect.height;
            if (top < offset) offset = top;
            else if (top + row.rect.height > offset + view) offset = top + row.rect.height - view;
            _scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(offset / room);
        }
    }
}
