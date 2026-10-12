using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A small envelope that bobs over the mailbox while it holds a letter or a parcel (playtest 2026-10-10: "how can you tell when you have mail, the box looks the same").
    public sealed class MailIndicator : MonoBehaviour, IMapBound
    {
        [SerializeField] Sprite _icon;
        [SerializeField] Vector3 _offset = new Vector3(0f, 1.6f, 0f);

        SpriteRenderer _renderer;
        GameSession _session;
        float _check;

        public bool Showing => _renderer != null && _renderer.enabled;

        public void Configure(Sprite icon) => _icon = icon;

        // Is there anything to collect? (pure given the session, so it can be tested)
        public static bool HasMail(GameSession session) => session != null && session.InGame && (session.State.Mailbox.Count > 0 || Parcels.Count(session) > 0);

        void Start()
        {
            var go = new GameObject("MailIcon", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = _offset;
            _renderer = go.GetComponent<SpriteRenderer>();
            _renderer.sprite = _icon;
            _renderer.sortingOrder = 12;
            _renderer.enabled = false;
        }

        public void Bind(MapContext map) => _session = map.Session;

        void Update()
        {
            if (_renderer == null || _icon == null) return;
            _check -= Time.unscaledDeltaTime;
            if (_check <= 0f)
            {
                _check = 0.4f;
                _renderer.enabled = HasMail(_session);
            }
            if (_renderer.enabled) _renderer.transform.localPosition = _offset + new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.08f, 0f);
        }
    }
}
