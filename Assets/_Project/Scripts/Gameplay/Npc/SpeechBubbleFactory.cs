using UnityEngine;

namespace Farm.Gameplay
{
    // The bubble a bark appears in (T-125): a cream panel over the villager's head with the line wrapped to a few short rows.
    // Flat shapes and the built-in font, like the emote bubble; art replaces the panel when the UI frames exist.
    public static class SpeechBubbleFactory
    {
        const int WrapColumns = 22;
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

        // Breaks text into rows of at most `columns` characters, at spaces.
        public static string Wrap(string text, int columns = WrapColumns)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var sb = new System.Text.StringBuilder();
            var row = 0;
            foreach (var word in text.Split(' '))
            {
                if (row > 0 && row + 1 + word.Length > columns) { sb.Append('\n'); row = 0; }
                else if (row > 0) { sb.Append(' '); row++; }
                sb.Append(word);
                row += word.Length;
            }
            return sb.ToString();
        }

        public static GameObject Create(Transform actor, string text)
        {
            var wrapped = Wrap(text);
            var rows = wrapped.Split('\n');
            var longest = 0;
            foreach (var r in rows) if (r.Length > longest) longest = r.Length;

            var root = new GameObject("Bark");
            root.transform.SetParent(actor, false);
            root.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var back = new GameObject("Panel", typeof(SpriteRenderer));
            back.transform.SetParent(root.transform, false);
            back.transform.localPosition = new Vector3(0f, rows.Length * 0.08f, 0f);
            back.transform.localScale = new Vector3(0.17f * longest + 0.4f, 0.28f * rows.Length + 0.25f, 1f);
            var sr = back.GetComponent<SpriteRenderer>();
            sr.sprite = Panel();
            sr.color = new Color(0.97f, 0.92f, 0.80f, 0.95f);
            sr.sortingOrder = 300;

            var label = new GameObject("Text", typeof(TextMesh));
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = new Vector3(0f, rows.Length * 0.08f, -0.01f);
            var mesh = label.GetComponent<TextMesh>();
            mesh.text = wrapped;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 0.085f;
            mesh.fontSize = 36;
            mesh.color = new Color(0.16f, 0.11f, 0.08f, 1f);
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var renderer = label.GetComponent<MeshRenderer>();
            renderer.material = mesh.font.material;
            renderer.sortingOrder = 301;
            return root;
        }
    }
}
