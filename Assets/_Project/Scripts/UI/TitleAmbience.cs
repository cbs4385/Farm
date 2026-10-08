using Farm.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The little moving things over the title picture: autumn leaves drifting down on the breeze, smoke curling from the farmhouse chimney, and the pale
    // lights over the woods twinkling. Everything is made once and moved in place, so nothing is allocated while the menu is open. Positions are fractions
    // of the picture (u across, v up from the bottom), so they stay on the right spot however the picture is cropped to the screen.
    public sealed class TitleAmbience : MonoBehaviour
    {
        const int LeafCount = 14, PuffCount = 7;
        const float PuffLife = 7f;
        static readonly Vector2 Chimney = new Vector2(0.154f, 0.61f);
        // The twinkling lights in the picture, as (u, v up from the bottom).
        static readonly Vector2[] Stars =
        {
            new Vector2(0.561f, 0.722f), new Vector2(0.587f, 0.743f), new Vector2(0.712f, 0.774f), new Vector2(0.853f, 0.819f),
            new Vector2(0.905f, 0.805f), new Vector2(0.955f, 0.837f), new Vector2(0.683f, 0.700f),
        };

        static Sprite _puff;

        RectTransform _picture;
        RectTransform[] _leaf, _puffs, _stars;
        Image[] _leafImage, _puffImage, _starImage;
        float[] _leafU, _leafV, _leafSpeed, _leafFall, _leafPhase, _leafSpin, _puffAge;

        public int LeafCountShown => _leaf != null ? _leaf.Length : 0;

        public static TitleAmbience Create(RectTransform picture)
        {
            var host = new GameObject("Ambience", typeof(RectTransform));
            host.transform.SetParent(picture, false);
            var rt = (RectTransform)host.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var ambience = host.AddComponent<TitleAmbience>();
            ambience.Build(rt);
            return ambience;
        }

        void Build(RectTransform picture)
        {
            _picture = picture;
            var random = new System.Random(2026);
            float Next() => (float)random.NextDouble();

            _puffAge = new float[PuffCount];
            _puffs = new RectTransform[PuffCount]; _puffImage = new Image[PuffCount];
            for (var i = 0; i < PuffCount; i++)
            {
                (_puffs[i], _puffImage[i]) = Piece("Smoke" + i, PuffSprite(), 26f, new Color(0.88f, 0.84f, 0.86f, 0f));
                _puffAge[i] = PuffLife * i / PuffCount;
            }

            var leaves = UiArt.Get("fx_autumn_leaf");
            _leaf = new RectTransform[LeafCount]; _leafImage = new Image[LeafCount];
            _leafU = new float[LeafCount]; _leafV = new float[LeafCount]; _leafSpeed = new float[LeafCount];
            _leafFall = new float[LeafCount]; _leafPhase = new float[LeafCount]; _leafSpin = new float[LeafCount];
            for (var i = 0; i < LeafCount; i++)
            {
                (_leaf[i], _leafImage[i]) = Piece("Leaf" + i, leaves, 20f, Color.white);
                _leafImage[i].enabled = leaves != null;
                _leafU[i] = Next() * 1.1f - 0.05f; _leafV[i] = Next();
                _leafSpeed[i] = 0.018f + Next() * 0.03f; _leafFall[i] = 0.035f + Next() * 0.05f;
                _leafPhase[i] = Next() * 6.28f; _leafSpin[i] = (Next() - 0.5f) * 140f;
            }

            _stars = new RectTransform[Stars.Length]; _starImage = new Image[Stars.Length];
            for (var i = 0; i < Stars.Length; i++)
                (_stars[i], _starImage[i]) = Piece("Light" + i, PuffSprite(), 9f, new Color(0.88f, 0.84f, 1f, 0.8f));
        }

        (RectTransform, Image) Piece(string name, Sprite sprite, float size, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_picture, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(size, size);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = colour;
            image.raycastTarget = false;
            return (rt, image);
        }

        // A soft round dot, drawn once.
        static Sprite PuffSprite()
        {
            if (_puff != null) return _puff;
            const int n = 32;
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d)));
                }
            texture.Apply();
            return _puff = Sprite.Create(texture, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 32f);
        }

        void Update()
        {
            var t = Time.unscaledTime;
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            var scale = Mathf.Max(0.3f, _picture.rect.height / 720f);                       // the pieces are sized for a 720 pixel tall picture

            // Leaves: down and across on the wind, rocking, turning; back to the top when they leave the bottom.
            var gust = 0.8f + 0.5f * Mathf.Sin(t * (2f * Mathf.PI / TitleSway.GustPeriod));
            for (var i = 0; i < _leaf.Length; i++)
            {
                _leafU[i] += _leafSpeed[i] * gust * dt;
                _leafV[i] -= _leafFall[i] * dt;
                if (_leafV[i] < -0.04f || _leafU[i] > 1.05f) { _leafV[i] = 1.04f; _leafU[i] = (_leafU[i] > 1.05f ? 0f : _leafU[i]) - 0.1f * Mathf.Abs(Mathf.Sin(_leafPhase[i])); }
                var u = _leafU[i] + 0.012f * Mathf.Sin(t * 1.4f + _leafPhase[i]);
                Place(_leaf[i], u, _leafV[i], scale * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(_leafPhase[i]))));
                _leaf[i].localRotation = Quaternion.Euler(0f, 0f, t * _leafSpin[i] + _leafPhase[i] * 30f);
            }

            // Smoke: up from the chimney, widening, thinning, leaning with the wind.
            for (var i = 0; i < _puffs.Length; i++)
            {
                _puffAge[i] += dt;
                if (_puffAge[i] > PuffLife) _puffAge[i] -= PuffLife;
                var life = _puffAge[i] / PuffLife;
                var u = Chimney.x + life * 0.045f * gust + 0.006f * Mathf.Sin(t * 0.9f + i);
                var v = Chimney.y + life * 0.17f;
                Place(_puffs[i], u, v, scale * (0.5f + life * 1.6f));
                var alpha = 0.34f * Mathf.Sin(life * Mathf.PI) * (1f - life * 0.4f);
                _puffImage[i].color = new Color(0.88f, 0.84f, 0.86f, alpha);
            }

            // The lights over the woods: each slowly brightens and dims.
            for (var i = 0; i < _stars.Length; i++)
            {
                Place(_stars[i], Stars[i].x, Stars[i].y, scale * (0.8f + 0.4f * Mathf.Abs(Mathf.Sin(t * 0.9f + i * 1.7f))));
                _starImage[i].color = new Color(0.88f, 0.84f, 1f, 0.25f + 0.65f * Mathf.Abs(Mathf.Sin(t * 1.1f + i * 2.3f)));
            }
        }

        static void Place(RectTransform rt, float u, float v, float scale)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(u, v);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
