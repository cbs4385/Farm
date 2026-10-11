using System.Collections.Generic;
using Farm.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The rain of coins that answers a change of gold (owner, 2026-10-10): when gold is gained, coins fly in from the upper part of the screen and pour into the gold
    // counter at the top right; when gold is spent or lost, coins burst out of the counter and fall away. A layer of small pictures over the screen, driven by Tick.
    public sealed class CoinRain
    {
        const string CoinArt = "fx_coin_fly";
        const float CoinSize = 22f, FlightTime = 0.75f, FallTime = 1.0f, Gravity = 900f;

        sealed class Coin
        {
            public Image Image;
            public Vector2 Start, Control, End, Velocity;
            public float Delay, Age;
            public bool Gain;
        }

        readonly RectTransform _layer;
        readonly List<Coin> _coins = new List<Coin>();
        readonly Stack<Image> _spare = new Stack<Image>();
        int _arrived;

        public CoinRain(Transform canvas)
        {
            var go = new GameObject("CoinRain", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            _layer = (RectTransform)go.transform;
            _layer.anchorMin = Vector2.zero;
            _layer.anchorMax = Vector2.one;
            _layer.offsetMin = _layer.offsetMax = Vector2.zero;
        }

        public int Active => _coins.Count;
        public int Arrived => _arrived;                 // how many coins have landed in the counter (for the counter's little pulse and for tests)

        // How many coins for an amount of gold: a few for a little, a downpour for a lot.
        public static int CountFor(int amount) => Mathf.Clamp(Mathf.Abs(amount) / 8 + 4, 4, 28);

        // Starts a rain for a gain (true) or a loss (false) of `amount` gold, to or from the counter whose picture is `target`.
        public void Burst(bool gain, int amount, RectTransform target)
        {
            if (target == null) return;
            var end = LocalOf(target);
            var count = CountFor(amount);
            var width = _layer.rect.width;
            var height = _layer.rect.height;
            for (var i = 0; i < count; i++)
            {
                var coin = new Coin { Gain = gain, Image = Take(), Delay = i * (gain ? 0.045f : 0.02f) };
                if (gain)
                {
                    coin.Start = new Vector2(Random.Range(-0.35f, 0.35f) * width, Random.Range(0.05f, 0.4f) * height);        // up and across the screen
                    coin.End = end;
                    coin.Control = new Vector2((coin.Start.x + end.x) * 0.5f + Random.Range(-60f, 60f), Mathf.Max(coin.Start.y, end.y) + Random.Range(20f, 120f));
                }
                else
                {
                    coin.Start = end + new Vector2(Random.Range(-8f, 8f), Random.Range(-6f, 6f));
                    coin.Velocity = new Vector2(Random.Range(-260f, 90f), Random.Range(120f, 380f));
                }
                coin.Image.rectTransform.anchoredPosition = coin.Start;
                coin.Image.color = new Color(1f, 1f, 1f, 0f);
                _coins.Add(coin);
            }
        }

        Vector2 LocalOf(RectTransform target)
        {
            var world = target.TransformPoint(target.rect.center);
            return _layer.InverseTransformPoint(world);
        }

        Image Take()
        {
            Image image;
            if (_spare.Count > 0) { image = _spare.Pop(); image.gameObject.SetActive(true); }
            else
            {
                var go = new GameObject("Coin", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_layer, false);
                image = go.GetComponent<Image>();
                image.raycastTarget = false;
                image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                image.rectTransform.sizeDelta = new Vector2(CoinSize, CoinSize);
                var sprite = UiArt.Get(CoinArt);
                image.sprite = sprite;
                image.enabled = sprite != null;
            }
            image.rectTransform.localScale = Vector3.one;
            return image;
        }

        public void Tick(float dt)
        {
            for (var i = _coins.Count - 1; i >= 0; i--)
            {
                var c = _coins[i];
                if (c.Delay > 0f) { c.Delay -= dt; continue; }
                c.Age += dt;
                var rt = c.Image.rectTransform;
                if (c.Gain)
                {
                    var t = Mathf.Clamp01(c.Age / FlightTime);
                    var e = t * t;                                              // speeds up as it nears the counter
                    var a = Vector2.Lerp(c.Start, c.Control, e);
                    var b = Vector2.Lerp(c.Control, c.End, e);
                    rt.anchoredPosition = Vector2.Lerp(a, b, e);
                    rt.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.6f, e);
                    c.Image.color = new Color(1f, 1f, 1f, Mathf.Clamp01(c.Age * 8f));
                    if (t >= 1f) { _arrived++; Release(i); }
                }
                else
                {
                    c.Velocity += new Vector2(0f, -Gravity * dt);
                    rt.anchoredPosition += c.Velocity * dt;
                    var t = Mathf.Clamp01(c.Age / FallTime);
                    c.Image.color = new Color(1f, 1f, 1f, Mathf.Clamp01(c.Age * 10f) * (1f - t * t));
                    if (t >= 1f) Release(i);
                }
            }
        }

        void Release(int index)
        {
            var c = _coins[index];
            c.Image.gameObject.SetActive(false);
            _spare.Push(c.Image);
            _coins.RemoveAt(index);
        }

        // Puts everything away at once (a new game, the main menu).
        public void Clear()
        {
            for (var i = _coins.Count - 1; i >= 0; i--) Release(i);
        }
    }
}
