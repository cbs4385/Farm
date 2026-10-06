using UnityEngine;

namespace Farm.Gameplay
{
    // The farmer fishing: a rod held out toward the water, a line from its tip, and a bobber that rides the ripples where the cast landed.
    // Drawn from stretched pixels (no extra art) and only there while the fishing screen is open.
    public sealed class FishingPose : MonoBehaviour
    {
        const float Pixel = 1f / 16f;

        SpriteRenderer _rod, _line, _bobberTop, _bobberBottom;
        SpriteRenderer _body;
        PlayerController _controller;
        Vector3 _target;
        float _clock;

        public bool IsActive { get; private set; }
        public Vector3 BobberPosition { get; private set; }
        public Vector3 TipPosition { get; private set; }

        public static FishingPose For(GameObject player) => player.TryGetComponent<FishingPose>(out var pose) ? pose : player.AddComponent<FishingPose>();

        // Where the hand holds the rod and where its tip points, from the feet. (pure)
        public static (Vector2 hand, Vector2 tip) Rod(Vector2Int facing)
        {
            var side = facing.x != 0 ? facing.x : 1;
            var hand = new Vector2(side * 0.22f, 0.8f);
            var reach = facing.x != 0 ? 0.75f : 0.4f;
            var tip = hand + new Vector2(side * reach, facing.y > 0 ? 0.85f : 0.6f);
            return (hand, tip);
        }

        // The bobber's height above the water at a moment: it rides the ripples, a pixel up and down. (pure)
        public static float Bob(float time) => Mathf.Round(Mathf.Sin(time * 3.2f)) * Pixel;

        public void Show(Vector3 targetWorld)
        {
            _target = targetWorld;
            _clock = 0f;
            if (_rod == null) Build();
            IsActive = true;
            SetVisible(true);
            Layout();
        }

        public void Hide()
        {
            IsActive = false;
            if (_rod != null) SetVisible(false);
        }

        void Build()
        {
            _body = GetComponentInChildren<SpriteRenderer>();
            _controller = GetComponent<PlayerController>();
            _rod = Make("Rod", new Color(0.55f, 0.37f, 0.2f));
            _line = Make("Line", new Color(1f, 1f, 1f, 0.85f));
            _bobberTop = Make("BobberTop", new Color(0.95f, 0.95f, 0.95f));
            _bobberBottom = Make("BobberBottom", new Color(0.9f, 0.2f, 0.2f));
        }

        SpriteRenderer Make(string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EnemySprites.Pixel();
            sr.color = color;
            sr.sortingOrder = (_body != null ? _body.sortingOrder : 10) + 1;
            return sr;
        }

        void SetVisible(bool on)
        {
            _rod.enabled = _line.enabled = _bobberTop.enabled = _bobberBottom.enabled = on;
        }

        // A stretched pixel from a to b (the pixel sprite starts at its left edge, so it needs no offset).
        static void Stretch(SpriteRenderer sr, Vector3 a, Vector3 b, float thickness)
        {
            var d = b - a;
            sr.transform.position = new Vector3(a.x, a.y, 0f);
            sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            sr.transform.localScale = new Vector3(d.magnitude, thickness, 1f);
        }

        void LateUpdate()
        {
            if (!IsActive) return;
            _clock += Time.unscaledDeltaTime;
            Layout();
        }

        void Layout()
        {
            var facing = _controller != null ? _controller.Facing : Vector2Int.down;
            var (hand, tip) = Rod(facing);
            var feet = transform.position;
            TipPosition = feet + (Vector3)tip;
            BobberPosition = _target + Vector3.up * Bob(_clock);
            Stretch(_rod, feet + (Vector3)hand, TipPosition, Pixel * 1.5f);
            Stretch(_line, TipPosition, BobberPosition + Vector3.up * Pixel, Pixel * 0.5f);
            var corner = BobberPosition + new Vector3(-Pixel, 0f, 0f);
            _bobberBottom.transform.rotation = _bobberTop.transform.rotation = Quaternion.identity;
            _bobberBottom.transform.position = corner;
            _bobberBottom.transform.localScale = new Vector3(Pixel * 2f, Pixel * 1.5f, 1f);
            _bobberTop.transform.position = corner + Vector3.up * Pixel * 1.5f;
            _bobberTop.transform.localScale = new Vector3(Pixel * 2f, Pixel, 1f);
        }
    }
}
