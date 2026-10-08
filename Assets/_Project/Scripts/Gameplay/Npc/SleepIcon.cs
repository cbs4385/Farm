using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The little "Z z z" that drifts over a villager asleep in bed (fx_sleep_zzz). It is a child of the villager but keeps itself upright, because the
    // sleeper is turned on their side.
    public sealed class SleepIcon : MonoBehaviour
    {
        public const string ObjectName = "SleepIcon";
        const float Scale = 0.9f;

        SpriteRenderer _renderer;
        Vector3 _anchor;

        public bool Shown => gameObject.activeSelf;

        public static SleepIcon Create(Transform owner)
        {
            var go = new GameObject(ObjectName);
            go.transform.SetParent(owner, false);
            var icon = go.AddComponent<SleepIcon>();
            icon._renderer = go.AddComponent<SpriteRenderer>();
            icon._renderer.sprite = UiArt.Get(Fx.SleepZzz);
            icon._renderer.sortingOrder = 11;
            go.transform.localScale = new Vector3(Scale, Scale, 1f);
            go.SetActive(false);
            return icon;
        }

        // Shows the icon above `anchor` (the middle of the bed).
        public void Show(Vector3 anchor)
        {
            _anchor = anchor;
            if (_renderer != null && _renderer.sprite == null) return;      // no picture, no icon: never an error
            gameObject.SetActive(true);
            Place();
        }

        public void Hide() => gameObject.SetActive(false);

        void LateUpdate() => Place();

        void Place()
        {
            transform.rotation = Quaternion.identity;
            var t = Time.time;
            transform.position = _anchor + new Vector3(0.45f, 0.75f + 0.06f * Mathf.Sin(t * 2f), 0f);
            if (_renderer != null) _renderer.color = new Color(1f, 1f, 1f, 0.75f + 0.25f * Mathf.Sin(t * 2f + 1f));
        }
    }
}
