using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // A few small specks thrown up by an action (dirt from the hoe, drops from the can, leaves from a harvest) that arc, fall and fade.
    // Made on the spot and destroyed after their life; no art needed.
    // The effect pictures (names in UiArt) that actions throw.
    public static class Fx
    {
        public const string DigDirt = "fx_dig_dirt", WaterDrop = "fx_water_drop", ChopChip = "fx_chop_chip", OreSpark = "fx_ore_spark",
            HarvestPop = "fx_harvest_pop", HeartPop = "fx_heart_pop", HitStar = "fx_hit_star", Splash = "fx_rain_splash", Dust = "fx_dust",
            LevelUp = "fx_level_up", Sparkle = "fx_sparkle", CoinFly = "fx_coin_fly";
    }

    public sealed class ActionPuff : MonoBehaviour
    {
        public const float Life = 0.45f;
        public const float PictureScale = 0.6f;                 // 16 px pictures are drawn smaller than a tile

        static Sprite _speck;
        SpriteRenderer[] _bits;
        Color _color;
        int _count;
        float _age;

        // `art` names a picture from UiArt (fx_dig_dirt ...) to throw instead of plain specks; without one (or without that picture) it is specks.
        public static void Burst(Vector3 position, Color color, int count = 5, string art = null)
        {
            var go = new GameObject("ActionPuff");
            go.transform.position = position;
            go.AddComponent<ActionPuff>().Begin(color, count, art);
        }

        // A burst of pink specks over someone who is pleased (a petted animal, a villager given a gift), and a little hop of joy.
        public static void Hearts(Vector3 position, WalkBob hopper = null)
        {
            Burst(position, new Color(1f, 0.45f, 0.6f), 3, Fx.HeartPop);
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

        void Begin(Color color, int count, string art)
        {
            _count = Mathf.Max(1, count);
            var picture = string.IsNullOrEmpty(art) ? null : UiArt.Get(art);
            _color = picture != null ? Color.white : color;                    // a picture keeps its own colours
            _speck ??= MakeSpeck();
            _bits = new SpriteRenderer[_count];
            for (var i = 0; i < _count; i++)
            {
                var child = new GameObject("Speck" + i);
                child.transform.SetParent(transform, false);
                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = picture != null ? picture : _speck;
                if (picture != null) child.transform.localScale = Vector3.one * PictureScale;
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
