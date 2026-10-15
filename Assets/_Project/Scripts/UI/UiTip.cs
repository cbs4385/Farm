using System;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // A name for an icon or a control that shows no words of its own, shown with the hover label when the mouse rests on it or a pad's cursor selects it (playtest 2026-10-15: every icon
    // should say what it is). Text is a key into the language file, or a function for names that change (an item). The coverage test (UiTipCoverageFlowTests) fails for any picture in
    // the interface that has neither a tip above it nor a deliberate exemption (Skip: decoration; HasOwn: the screen already shows a richer tooltip).
    public sealed class UiTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, ISelectHandler, IDeselectHandler
    {
        public enum Kind { Label, Decoration, OwnTooltip }

        // Parts of the interface that take no mouse clicks (so that clicks reach the ground behind them) but still have a name: the HUD tests the pointer against them itself.
        static readonly System.Collections.Generic.List<UiTip> Polled = new System.Collections.Generic.List<UiTip>();
        static UiTip() => TestResets.Add(() => Polled.Clear());

        public static UiTip AddPolled(GameObject go, string key)
        {
            var tip = Add(go, key);
            if (!Polled.Contains(tip)) Polled.Add(tip);
            return tip;
        }

        public static string PolledAt(Vector2 screenPosition)
        {
            for (var i = Polled.Count - 1; i >= 0; i--)
            {
                var tip = Polled[i];
                if (tip == null) { Polled.RemoveAt(i); continue; }
                if (!tip.isActiveAndEnabled) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)tip.transform, screenPosition, null)) return tip.Current;
            }
            return null;
        }

        Func<string> _text;
        bool _shown;

        public Kind Mode { get; private set; }
        public string Current => _text?.Invoke();

        public static UiTip Add(GameObject go, string key) => Add(go, () => L.Get(key));

        public static UiTip Add(GameObject go, Func<string> text)
        {
            var tip = go.GetComponent<UiTip>();
            if (tip == null) tip = go.AddComponent<UiTip>();
            tip.Mode = Kind.Label;
            tip._text = text;
            return tip;
        }

        // A picture that is only there to look nice (a frame, a flourish): no name wanted.
        public static UiTip Skip(GameObject go) => Mark(go, Kind.Decoration);

        // Something whose screen already shows a fuller tooltip of its own (the backpack grid).
        public static UiTip HasOwn(GameObject go) => Mark(go, Kind.OwnTooltip);

        static UiTip Mark(GameObject go, Kind kind)
        {
            var tip = go.GetComponent<UiTip>();
            if (tip == null) tip = go.AddComponent<UiTip>();
            tip.Mode = kind;
            tip._text = null;
            return tip;
        }

        // The tip that names a graphic: its own, or the nearest one above it (a slot names the icon inside it).
        public static UiTip For(Transform t) => t != null ? t.GetComponentInParent<UiTip>(true) : null;

        public void OnPointerEnter(PointerEventData e) => Show(e.position);
        public void OnPointerMove(PointerEventData e) { if (_shown) Show(e.position); }
        public void OnPointerExit(PointerEventData e) => Hide();
        public void OnSelect(BaseEventData e) => Show(RectTransformUtility.WorldToScreenPoint(null, transform.position));
        public void OnDeselect(BaseEventData e) => Hide();
        void OnDisable() => Hide();

        void Show(Vector2 position)
        {
            if (Mode != Kind.Label) return;
            var ui = GameContext.Current?.Ui;
            var text = Current;
            if (ui == null || string.IsNullOrEmpty(text)) return;
            ui.ShowHover(text, position);
            _shown = true;
        }

        void Hide()
        {
            if (!_shown) return;
            _shown = false;
            GameContext.Current?.Ui?.HideHover();
        }
    }
}
