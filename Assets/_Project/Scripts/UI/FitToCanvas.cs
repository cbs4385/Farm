using UnityEngine;

namespace Farm.UI
{
    // Keeps a fixed-size panel on the screen (T-145). Panels are built at a size that suits the default UI size; when the player
    // makes the UI bigger the canvas gets smaller in canvas units, so a 900 wide panel would run off the screen. This shrinks
    // the panel to the canvas (with a margin) and lets it return to its own size when there is room.
    public sealed class FitToCanvas : MonoBehaviour
    {
        const float Margin = 12f;

        RectTransform _rect;
        RectTransform _canvas;
        Vector2 _wanted;

        void Awake()
        {
            _rect = (RectTransform)transform;
            _wanted = _rect.sizeDelta;
        }

        void LateUpdate()
        {
            if (_canvas == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                _canvas = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
                if (_canvas == null) return;
            }
            // Only an axis whose anchors are together has a size of its own; a stretched axis (the options screen fills the
            // height) already follows the canvas and its sizeDelta is a margin.
            var area = _canvas.rect;
            var size = _rect.sizeDelta;
            if (_rect.anchorMin.x == _rect.anchorMax.x) size.x = Mathf.Min(_wanted.x, area.width - 2f * Margin);
            if (_rect.anchorMin.y == _rect.anchorMax.y) size.y = Mathf.Min(_wanted.y, area.height - 2f * Margin);
            if (_rect.sizeDelta != size) _rect.sizeDelta = size;
        }
    }
}
