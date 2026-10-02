using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // Keeps the selected child of a ScrollRect in view, so keyboard and gamepad navigation can reach every row of a long list.
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollSelectionFollower : MonoBehaviour
    {
        ScrollRect _scroll;
        GameObject _last;

        void Awake() => _scroll = GetComponent<ScrollRect>();

        void LateUpdate()
        {
            var system = EventSystem.current;
            if (system == null || _scroll.content == null) return;
            var selected = system.currentSelectedGameObject;
            if (selected == null || selected == _last || !selected.transform.IsChildOf(_scroll.content)) { _last = selected; return; }
            _last = selected;

            var target = (RectTransform)selected.transform;
            var viewport = _scroll.viewport != null ? _scroll.viewport : (RectTransform)_scroll.transform;
            var content = _scroll.content;
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var top = content.InverseTransformPoint(corners[1]).y;
            var bottom = content.InverseTransformPoint(corners[0]).y;
            var viewHeight = viewport.rect.height;
            var scrollable = content.rect.height - viewHeight;
            if (scrollable <= 0f) return;

            // Content-local y of the visible window's top edge, derived from the normalized scroll position.
            var windowTop = -(1f - _scroll.verticalNormalizedPosition) * scrollable;
            var windowBottom = windowTop - viewHeight;
            if (top > windowTop) windowTop = top;
            else if (bottom < windowBottom) windowTop = bottom + viewHeight;
            else return;
            _scroll.verticalNormalizedPosition = Mathf.Clamp01(1f + windowTop / scrollable);
        }
    }
}
