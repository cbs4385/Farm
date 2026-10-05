using UnityEngine;

namespace Farm.Gameplay
{
    // Makes the growing crops, fruit trees and soft forage on a map lean in the wind. A few times a second it asks the map view to
    // set the lean of every plant tile; the view only touches tiles whose lean changed.
    public sealed class TileSway : MonoBehaviour
    {
        const float Interval = 0.15f;

        FarmMapView _view;
        float _next;

        public float Strength = 1f;

        void Awake() => _view = GetComponent<FarmMapView>();

        void Update()
        {
            if (_view == null || Time.time < _next) return;
            _next = Time.time + Interval;
            _view.ApplySway(Time.time, Strength * Sway.Current());
        }
    }
}
