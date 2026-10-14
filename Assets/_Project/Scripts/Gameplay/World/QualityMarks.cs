using UnityEngine;

namespace Farm.Gameplay
{
    // The small diamond that says an item is better than ordinary (silver, gold, iridium), for pictures drawn in the world: the carried item, the product over an animal.
    // The inventory slots draw the same diamond in the UI (SlotBadges), with these tints.
    public static class QualityMarks
    {
        static readonly Color[] Tints =
        {
            Color.clear,
            new Color(0.82f, 0.86f, 0.92f),     // silver
            new Color(0.97f, 0.78f, 0.22f),     // gold
            new Color(0.78f, 0.45f, 0.95f),     // iridium
        };

        static Sprite _diamond;

        public static Color TintOf(int quality) => quality > 0 && quality < Tints.Length ? Tints[quality] : Color.clear;

        public static Sprite Diamond
        {
            get
            {
                if (_diamond != null) return _diamond;
                const int size = 9;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "quality_diamond" };
                var centre = size / 2;
                for (var y = 0; y < size; y++)
                    for (var x = 0; x < size; x++)
                    {
                        var d = Mathf.Abs(x - centre) + Mathf.Abs(y - centre);
                        texture.SetPixel(x, y, d > centre ? Color.clear : d == centre ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.white);          // a dark rim round a tinted fill
                    }
                texture.Apply();
                _diamond = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
                return _diamond;
            }
        }

        // A hidden diamond renderer under `parent`; turn it on with Set.
        public static SpriteRenderer Attach(Transform parent, Vector3 localPosition, float scale)
        {
            var go = new GameObject("Quality");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Diamond;
            renderer.enabled = false;
            return renderer;
        }

        // Shows the diamond for the quality (hidden for ordinary things), in front of `order`.
        public static void Set(SpriteRenderer mark, int quality, int order)
        {
            if (mark == null) return;
            mark.enabled = quality > 0;
            if (quality <= 0) return;
            mark.color = TintOf(quality);
            mark.sortingOrder = order + 1;
        }
    }
}
