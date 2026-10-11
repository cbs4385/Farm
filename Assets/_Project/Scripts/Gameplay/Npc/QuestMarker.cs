using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A golden "!" that bobs over a villager who is waiting on something the player carries (see QuestTargets), so they can be found in a crowd or across a field.
    public sealed class QuestMarker : MonoBehaviour
    {
        static Sprite _sprite;
        static QuestMarker() => TestResets.Add(() => _sprite = null);

        SpriteRenderer _renderer;
        GameSession _session;
        string _npc;
        float _check;

        public bool Showing => _renderer != null && _renderer.enabled;

        public void Setup(string npcId) => _npc = npcId;

        // A bold exclamation mark in the project's dark outline and gold, 8 x 13 pixels, made once.
        static Sprite Mark()
        {
            if (_sprite != null) return _sprite;
            const int w = 8, h = 13;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color32(0, 0, 0, 0);
            var ink = new Color32(0x23, 0x0b, 0x07, 255);
            var gold = new Color32(0xf2, 0xc6, 0x4a, 255);
            var px = new Color32[w * h];
            for (var i = 0; i < px.Length; i++) px[i] = clear;
            void Set(int x, int y, Color32 c) { px[(h - 1 - y) * w + x] = c; }           // row 0 at the top
            for (var y = 0; y < 13; y++) for (var x = 1; x < 7; x++) Set(x, y, y >= 9 && y <= 10 ? clear : ink);
            for (var y = 1; y <= 7; y++) for (var x = 2; x < 6; x++) Set(x, y, gold);
            for (var y = 11; y <= 11; y++) for (var x = 2; x < 6; x++) Set(x, y, gold);
            Set(2, 12, ink); Set(5, 12, ink);
            for (var y = 8; y <= 10; y++) for (var x = 1; x < 7; x++) Set(x, y, clear);
            for (var y = 8; y <= 8; y++) for (var x = 2; x < 6; x++) Set(x, y, ink);
            for (var y = 11; y <= 12; y++) for (var x = 2; x < 6; x++) Set(x, y, y == 11 ? gold : ink);
            tex.SetPixels32(px);
            tex.Apply();
            _sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
            return _sprite;
        }

        void Start()
        {
            var go = new GameObject("QuestMark", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            _renderer = go.GetComponent<SpriteRenderer>();
            _renderer.sprite = Mark();
            _renderer.sortingOrder = 13;
            _renderer.enabled = false;
            ServiceLocator.TryGet(out _session);
        }

        void Update()
        {
            if (_renderer == null || string.IsNullOrEmpty(_npc)) return;
            _check -= Time.unscaledDeltaTime;
            if (_check <= 0f)
            {
                _check = 0.4f;
                if (_session == null) ServiceLocator.TryGet(out _session);
                _renderer.enabled = QuestTargets.Wants(_session, _npc);
            }
            if (_renderer.enabled) _renderer.transform.localPosition = new Vector3(0f, 1.55f + Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 0.12f, 0f);
        }
    }
}
