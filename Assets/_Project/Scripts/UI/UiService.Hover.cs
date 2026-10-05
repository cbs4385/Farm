using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The hover label: a small panel that follows the mouse and names what it is over (a shipping bin, a door, a villager...).
    public sealed partial class UiService
    {
        RectTransform _hover;
        TextMeshProUGUI _hoverText;

        public string HoverText => _hover != null && _hover.gameObject.activeSelf ? _hoverText.text : null;

        public void ShowHover(string text, Vector2 screenPosition)
        {
            if (string.IsNullOrEmpty(text)) { HideHover(); return; }
            if (_hover == null)
            {
                var panel = UiKit.Panel(_screenCanvas.transform, "HoverLabel", new Color(UiKit.PanelColor.r, UiKit.PanelColor.g, UiKit.PanelColor.b, 0.95f));
                panel.raycastTarget = false;
                _hover = panel.rectTransform;
                _hover.pivot = new Vector2(0f, 1f);
                _hover.anchorMin = _hover.anchorMax = new Vector2(0f, 0f);
                _hoverText = UiKit.Label(_hover, "", 17f, TextAlignmentOptions.Left, UiKit.TextColor);
                _hoverText.raycastTarget = false;
                UiKit.Stretch(_hoverText.rectTransform, 6f);
                _hoverText.textWrappingMode = TextWrappingModes.NoWrap;
            }
            _hoverText.text = text;
            _hoverText.ForceMeshUpdate();
            var size = _hoverText.GetPreferredValues(text);
            _hover.sizeDelta = new Vector2(size.x + 14f, size.y + 10f);
            _hover.gameObject.SetActive(true);
            _hover.SetAsLastSibling();
            // Screen position to the overlay canvas, kept on screen.
            var canvas = (RectTransform)_screenCanvas.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPosition + new Vector2(18f, -12f), null, out var local))
            {
                var half = canvas.rect.size * 0.5f;
                local.x = Mathf.Clamp(local.x, -half.x, half.x - _hover.sizeDelta.x);
                local.y = Mathf.Clamp(local.y, -half.y + _hover.sizeDelta.y, half.y);
                _hover.anchoredPosition = local + half;
            }
        }

        public void HideHover()
        {
            if (_hover != null) _hover.gameObject.SetActive(false);
        }
    }
}
