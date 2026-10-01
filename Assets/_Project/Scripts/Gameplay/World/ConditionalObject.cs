using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Shows or hides scene objects depending on a condition (see Conditions): hidden altars, symbols that appear
    // after a flag is set, a gate that only exists at night. Re-evaluated when flags, variables or the hour change.
    // If no targets are assigned, its direct children are the targets.
    public sealed class ConditionalObject : MonoBehaviour
    {
        [SerializeField] string _condition;
        [SerializeField] bool _invert;
        [SerializeField] GameObject[] _targets;

        EventBus _bus;
        GameSession _session;
        int _lastHour = -1;

        public string Condition { get => _condition; set => _condition = value; }
        public bool Invert { get => _invert; set => _invert = value; }

        void Start()
        {
            _bus = ServiceLocator.Get<EventBus>();
            _session = ServiceLocator.Get<GameSession>();
            _bus.Subscribe<FlagChanged>(OnChanged);
            _bus.Subscribe<VarChanged>(OnChanged);
            _bus.Subscribe<DayStarted>(OnChanged);
            _bus.Subscribe<MinuteChanged>(OnMinute);
            Refresh();
        }

        void OnDestroy()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<FlagChanged>(OnChanged);
            _bus.Unsubscribe<VarChanged>(OnChanged);
            _bus.Unsubscribe<DayStarted>(OnChanged);
            _bus.Unsubscribe<MinuteChanged>(OnMinute);
        }

        void OnChanged(FlagChanged _) => Refresh();
        void OnChanged(VarChanged _) => Refresh();
        void OnChanged(DayStarted _) => Refresh();

        void OnMinute(MinuteChanged e)
        {
            if (e.Now.Hour == _lastHour) return;   // conditions only look at the hour, so refresh hourly
            _lastHour = e.Now.Hour;
            Refresh();
        }

        public void Refresh()
        {
            if (_session == null || !_session.InGame) return;
            var on = Conditions.Evaluate(_condition, _session.World) != _invert;
            if (_targets != null && _targets.Length > 0)
            {
                foreach (var t in _targets) if (t != null) t.SetActive(on);
            }
            else
            {
                foreach (Transform child in transform) child.gameObject.SetActive(on);
            }
        }
    }
}
