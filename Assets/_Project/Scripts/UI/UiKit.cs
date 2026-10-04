using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Farm.UI
{
    // Small helpers for building uGUI in code. Everything is anchored/laid out so it scales with the canvas
    // (960x540 reference) and every interactive element is a Selectable, so gamepad navigation just works.
    public static class UiKit
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(960, 540);

        public static readonly Color PanelColor = new Color(0.16f, 0.11f, 0.08f, 1f);
        public static readonly Color PanelLight = new Color(0.30f, 0.21f, 0.14f, 1f);
        public static readonly Color TextColor = new Color(0.97f, 0.92f, 0.80f, 1f);
        public static readonly Color DimText = new Color(0.75f, 0.68f, 0.55f, 1f);
        public static readonly Color Accent = new Color(0.95f, 0.75f, 0.30f, 1f);
        public static readonly Color Danger = new Color(0.85f, 0.30f, 0.25f, 1f);
        public static readonly Color Scrim = new Color(0f, 0f, 0f, 0.55f);

        // Text size is applied per canvas (SetCanvasScale), so labels use their authored sizes.
        public static float TextScale = 1f;

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void SetCanvasScale(Canvas canvas, float scale)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.referenceResolution = ReferenceResolution / Mathf.Clamp(scale, 0.5f, 2f);
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt, float inset = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, float size = 18f,
            TextAlignmentOptions align = TextAlignmentOptions.Left, Color? color = null)
        {
            var rt = Rect("Label", parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size * TextScale;
            t.alignment = align;
            t.color = color ?? TextColor;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        public static LayoutElement Size(GameObject go, float w = -1f, float h = -1f, float flexW = -1f, float flexH = -1f)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (w >= 0) le.preferredWidth = w;
            if (h >= 0) le.preferredHeight = h;
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
            return le;
        }

        public static VerticalLayoutGroup VStack(Transform parent, string name, float spacing = 6f, int padding = 0,
            TextAnchor align = TextAnchor.UpperLeft)
        {
            var rt = Rect(name, parent);
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childAlignment = align;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        public static HorizontalLayoutGroup HStack(Transform parent, string name, float spacing = 6f,
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            var rt = Rect(name, parent);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childAlignment = align;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            return h;
        }

        public static void StyleSelectable(Selectable s)
        {
            var colors = s.colors;
            colors.normalColor = PanelLight;
            colors.highlightedColor = new Color(0.50f, 0.36f, 0.20f, 1f);
            colors.selectedColor = new Color(0.62f, 0.45f, 0.20f, 1f);
            colors.pressedColor = new Color(0.78f, 0.58f, 0.25f, 1f);
            colors.disabledColor = new Color(0.25f, 0.2f, 0.17f, 0.6f);
            colors.colorMultiplier = 1f;
            s.colors = colors;
            s.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        }

        public static Button MakeButton(Transform parent, string text, UnityAction onClick, float width = 220f, float height = 34f)
        {
            var img = Panel(parent, text, Color.white);   // Selectable tint colours are multiplied onto this
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            StyleSelectable(button);
            button.onClick.AddListener(() => Farm.Gameplay.AudioService.PlayIfAvailable(Farm.Gameplay.Sfx.Click));
            img.gameObject.AddComponent<UiHoverSound>();
            if (onClick != null) button.onClick.AddListener(onClick);
            Size(img.gameObject, width, height);

            var label = Label(img.transform, text, 18f, TextAlignmentOptions.Center);
            Stretch(label.rectTransform, 4f);
            return button;
        }

        public static void SetButtonText(Button b, string text)
        {
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null) t.text = text;
        }

        public static Slider MakeSlider(Transform parent, float value, UnityAction<float> onChanged, float width = 220f)
        {
            var root = Rect("Slider", parent);
            Size(root.gameObject, width, 24f);
            var slider = root.gameObject.AddComponent<Slider>();

            var bg = Panel(root, "Background", new Color(0.08f, 0.06f, 0.04f, 1f));
            Place(bg.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(width, 8f), Vector2.zero);
            bg.rectTransform.anchorMax = new Vector2(1, 0.5f);
            bg.rectTransform.sizeDelta = new Vector2(0, 8f);

            var fillArea = Rect("Fill Area", root);
            fillArea.anchorMin = new Vector2(0, 0.5f);
            fillArea.anchorMax = new Vector2(1, 0.5f);
            fillArea.sizeDelta = new Vector2(-12f, 8f);
            var fill = Panel(fillArea, "Fill", Accent);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0, 1);
            fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

            // Slider forces the handle's vertical anchors to stretch, so the handle is as tall as this area.
            var handleArea = Rect("Handle Slide Area", root);
            handleArea.anchorMin = new Vector2(0f, 0.5f);
            handleArea.anchorMax = new Vector2(1f, 0.5f);
            handleArea.sizeDelta = new Vector2(-12f, 20f);
            var handle = Panel(handleArea, "Handle", Color.white);
            handle.rectTransform.sizeDelta = new Vector2(12f, 0f);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            StyleSelectable(slider);
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        // Vertical scrollbar docked to the right edge of a ScrollRect.
        public static Scrollbar MakeScrollbar(RectTransform scrollRoot)
        {
            var track = Panel(scrollRoot, "Scrollbar", new Color(0.08f, 0.06f, 0.04f, 1f));
            track.rectTransform.anchorMin = new Vector2(1f, 0f);
            track.rectTransform.anchorMax = new Vector2(1f, 1f);
            track.rectTransform.pivot = new Vector2(1f, 0.5f);
            track.rectTransform.sizeDelta = new Vector2(10f, 0f);
            track.rectTransform.anchoredPosition = Vector2.zero;

            var area = Rect("Sliding Area", track.transform);
            Stretch(area);
            var handle = Panel(area, "Handle", Color.white);
            Stretch(handle.rectTransform);

            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;
            bar.direction = Scrollbar.Direction.BottomToTop;
            StyleSelectable(bar);
            bar.navigation = new Navigation { mode = Navigation.Mode.None };   // keep gamepad focus on the rows
            return bar;
        }

        public static Toggle MakeToggle(Transform parent, string text, bool value, UnityAction<bool> onChanged, float width = 220f)
        {
            var root = Panel(parent, text, Color.white);
            Size(root.gameObject, width, 30f);
            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = root;

            var box = Panel(root.transform, "Box", new Color(0.08f, 0.06f, 0.04f, 1f));
            Place(box.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20f, 20f), new Vector2(8f, 0f));
            var check = Panel(box.transform, "Check", Accent);
            Stretch(check.rectTransform, 4f);
            toggle.graphic = check;

            var label = Label(root.transform, text, 18f, TextAlignmentOptions.Left);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(36f, 0f);

            toggle.SetIsOnWithoutNotify(value);
            StyleSelectable(toggle);
            if (onChanged != null) toggle.onValueChanged.AddListener(onChanged);
            return toggle;
        }

        public static TMP_InputField MakeInput(Transform parent, string placeholder, string value, int maxLength, float width = 260f)
        {
            var root = Panel(parent, "Input", Color.white);
            Size(root.gameObject, width, 34f);
            var input = root.gameObject.AddComponent<TMP_InputField>();

            var viewport = Rect("Text Area", root.transform);
            Stretch(viewport, 6f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var ph = Label(viewport, placeholder, 18f, TextAlignmentOptions.Left, DimText);
            Stretch(ph.rectTransform);
            ph.fontStyle = FontStyles.Italic;
            var text = Label(viewport, string.Empty, 18f, TextAlignmentOptions.Left);
            Stretch(text.rectTransform);
            text.raycastTarget = true;

            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = ph;
            input.characterLimit = maxLength;
            input.text = value;
            input.targetGraphic = root;
            StyleSelectable(input);
            var c = input.colors;
            c.normalColor = new Color(0.08f, 0.06f, 0.04f, 1f);
            c.highlightedColor = new Color(0.16f, 0.12f, 0.08f, 1f);
            c.selectedColor = new Color(0.22f, 0.16f, 0.10f, 1f);
            input.colors = c;
            return input;
        }

        public static void ClearChildren(Transform t)
        {
            // Detach first so layout/indexing is correct immediately; Destroy itself is deferred to end of frame.
            for (var i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i);
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        // Full-screen dim layer + centred panel. Returns the panel's content rect.
        public static RectTransform ModalFrame(Transform canvas, string name, Vector2 size, out GameObject root)
        {
            var scrim = Panel(canvas, name, Scrim);
            Stretch(scrim.rectTransform);
            root = scrim.gameObject;

            var frame = Panel(scrim.transform, "Frame", PanelColor);
            Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            frame.gameObject.AddComponent<FitToCanvas>();       // a bigger UI size must not push the panel off the screen
            return frame.rectTransform;
        }

        public static string Plural(int n, string singular, string plural) => n == 1 ? singular : plural;
    }
}
