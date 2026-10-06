using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Puffs of smoke rising from a chimney: a few small soft squares that drift up, widen and fade, each a little behind the one before.
    public sealed class RoofSmoke : MonoBehaviour
    {
        const int Puffs = 4;
        const float Life = 3.2f, Rise = 1.3f;
        const float ArtScale = 0.5f;               // the drawn puffs are a tile wide; the chimney's are smaller

        static Sprite _puff;
        SpriteRenderer[] _puffs;
        bool _art;

        public static float Progress(float time, int index) => Mathf.Repeat(time / Life + index / (float)Puffs, 1f);

        void Awake()
        {
            _puff ??= MakePuff();
            _puffs = new SpriteRenderer[Puffs];
            _art = UiArt.Get("fx_puff_0") != null;
            for (var i = 0; i < Puffs; i++)
            {
                var go = new GameObject("Puff" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = UiArt.Get("fx_puff_" + i % 3) ?? _puff;
                if (sr.sprite != _puff) go.transform.localScale = Vector3.one * ArtScale;
                sr.sortingOrder = 6;
                _puffs[i] = sr;
            }
        }

        static Sprite MakePuff()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "smoke_puff" };
            var clear = new Color32(0, 0, 0, 0);
            var smoke = new Color32(0xd8, 0xd8, 0xe0, 255);
            for (var y = 0; y < 4; y++)
                for (var x = 0; x < 4; x++)
                    tex.SetPixel(x, y, (x == 0 || x == 3) && (y == 0 || y == 3) ? clear : smoke);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f), 16f);
        }

        void Update()
        {
            for (var i = 0; i < _puffs.Length; i++)
            {
                var t = Progress(Time.time, i);
                var wobble = Mathf.Sin((Time.time + i) * 1.7f) * 0.06f;
                _puffs[i].transform.localPosition = new Vector3(0.05f + wobble + t * 0.25f, 0.7f + t * Rise, 0f);
                _puffs[i].transform.localScale = Vector3.one * ((0.7f + t * 1.1f) * (_art ? ArtScale : 1f));
                _puffs[i].color = new Color(1f, 1f, 1f, 0.75f * (1f - t));
            }
        }
    }
}
