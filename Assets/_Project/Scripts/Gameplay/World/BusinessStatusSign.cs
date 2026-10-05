using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The little tag by a business's door: green while it is open, red while it is closed (playtest: "there is no indication that a village
    // business is closed"). It follows the same hours as the door itself (BusinessHoursRegistry).
    public sealed class BusinessStatusSign : MonoBehaviour, IInteractable
    {
        [SerializeField] string _businessId;
        [SerializeField] Sprite _open, _closed;
        SpriteRenderer _renderer;
        GameSession _session;
        float _next;

        public bool IsOpenNow { get; private set; } = true;

        public string HoverLabel => IsOpenNow ? L.Get("hover.sign_open") : BusinessHoursRegistry.ClosedMessage(_businessId);

        public void Configure(string businessId, Sprite open, Sprite closed)
        {
            _businessId = businessId; _open = open; _closed = closed;
        }

        public void Interact(PlayerActions player)
        {
            if (!string.IsNullOrEmpty(_businessId)) player.Session.Toast(IsOpenNow ? L.Get("hover.sign_open") : BusinessHoursRegistry.ClosedMessage(_businessId));
        }

        void Awake() => _renderer = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 0.5f;
            if (_session == null && !ServiceLocator.TryGet(out _session)) return;
            if (!_session.InGame || string.IsNullOrEmpty(_businessId)) return;
            IsOpenNow = BusinessHoursRegistry.IsOpen(_businessId, _session.Clock.Now);
            if (_renderer != null) _renderer.sprite = IsOpenNow ? _open : _closed;
        }
    }
}
