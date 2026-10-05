using UnityEngine;

namespace Farm.Gameplay
{
    // A few small specks thrown up by an action (dirt from the hoe, drops from the can, leaves from a harvest) that arc, fall and fade.
    // Made on the spot and destroyed after their life; no art needed.
    public sealed class ActionPuff : MonoBehaviour
    {
        public const float Life = 0.45f;

        static Sprite _speck;
        SpriteRenderer[] _bits;
        Color _color;
        int _count;
        float _age;

        public static void Burst(Vector3 position, Color color, int count = 5)
        {
            var go = new GameObject("ActionPuff");
            go.transform.position = position;
            go.AddComponent<ActionPuff>().Begin(color, count);
        }

        // A burst of pink specks over someone who is pleased (a petted animal, a villager given a gift), and a little hop of joy.
        public static void Hearts(Vector3 position, WalkBob hopper = null)
        {
            Burst(position, new Color(1f, 0.45f, 0.6f), 5);
            if (hopper != null) hopper.Lunge(Vector2Int.up);
        }

        // Where speck i of count is after `t` (0..1) of its life, relative to the burst point, and how opaque it is. (pure)
        public static (Vector2 offset, float alpha) BitAt(float t, int index, int count)
        {
            t = Mathf.Clamp01(t);
            var side = count <= 1 ? 0f : index / (float)(count - 1) * 2f - 1f;              // -1 .. 1 across the burst
            var lift = 0.55f + 0.2f * (index % 2);
            var x = side * 0.32f * t;
            var y = lift * t - 0.9f * t * t;                                                  // up, then falling
            return (new Vector2(x, y), 1f - t * t);
        }

        void Begin(Color color, int count)
        {
            _color = color;
            _count = Mathf.Max(1, count);
            _speck ??= MakeSpeck();
            _bits = new SpriteRenderer[_count];
            for (var i = 0; i < _count; i++)
            {
                var child = new GameObject("Speck" + i);
                child.transform.SetParent(transform, false);
                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = _speck;
                sr.sortingOrder = 12;
                _bits[i] = sr;
            }
            Apply();
        }

        static Sprite MakeSpeck()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "action_speck" };
            for (var y = 0; y < 2; y++) for (var x = 0; x < 2; x++) tex.SetPixel(x, y, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 16f);
        }

        void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Life) { Destroy(gameObject); return; }
            Apply();
        }

        void Apply()
        {
            var t = _age / Life;
            for (var i = 0; i < _bits.Length; i++)
            {
                var (offset, alpha) = BitAt(t, i, _count);
                _bits[i].transform.localPosition = new Vector3(offset.x, offset.y, 0f);
                _bits[i].color = new Color(_color.r, _color.g, _color.b, alpha);
            }
        }
    }
}
