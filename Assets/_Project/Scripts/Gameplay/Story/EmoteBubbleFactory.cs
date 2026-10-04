using UnityEngine;

namespace Farm.Gameplay
{
    // Builds the small speech-bubble that floats over an actor during an emote (T-100). A flat panel with a short glyph;
    // sprites replace the glyphs when the emote art exists (T-130).
    public static class EmoteBubbleFactory
    {
        static Sprite _panel;

        static Sprite Panel()
        {
            if (_panel != null) return _panel;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _panel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _panel;
        }

        public static GameObject Create(Transform actor, string glyph)
        {
            var root = new GameObject("Emote");
            root.transform.SetParent(actor, false);
            root.transform.localPosition = new Vector3(0.35f, 1.15f, 0f);

            var back = new GameObject("Panel", typeof(SpriteRenderer));
            back.transform.SetParent(root.transform, false);
            back.transform.localScale = new Vector3(0.7f, 0.5f, 1f);
            var sr = back.GetComponent<SpriteRenderer>();
            sr.sprite = Panel();
            sr.color = new Color(0.97f, 0.92f, 0.80f, 1f);
            sr.sortingOrder = 300;

            var text = new GameObject("Glyph", typeof(TextMesh));
            text.transform.SetParent(root.transform, false);
            text.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            var mesh = text.GetComponent<TextMesh>();
            mesh.text = glyph;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 0.1f;
            mesh.fontSize = 36;
            mesh.color = new Color(0.16f, 0.11f, 0.08f, 1f);
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var renderer = text.GetComponent<MeshRenderer>();
            renderer.material = mesh.font.material;
            renderer.sortingOrder = 301;
            return root;
        }
    }
}
