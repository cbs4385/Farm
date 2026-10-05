using UnityEngine;

namespace Farm.Gameplay
{
    // Leans a scene object (a tree, a bramble) in the wind. The picture moves to a child that turns about the object's base, so the
    // object's collider stays where it is.
    public sealed class ObjectSway : MonoBehaviour
    {
        const float Interval = 0.15f;

        Transform _pivot;
        float _next;
        int _lean;

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) { enabled = false; return; }
            var halfHeight = sr.sprite.bounds.extents.y;
            var pivot = new GameObject("SwayPivot").transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = new Vector3(0f, -halfHeight, 0f);      // the base of the picture
            var visual = new GameObject("SwayVisual");
            visual.transform.SetParent(pivot, false);
            visual.transform.localPosition = new Vector3(0f, halfHeight, 0f);
            var copy = visual.AddComponent<SpriteRenderer>();
            copy.sprite = sr.sprite;
            copy.color = sr.color;
            copy.sharedMaterial = sr.sharedMaterial;
            copy.sortingLayerID = sr.sortingLayerID;
            copy.sortingOrder = sr.sortingOrder;
            sr.enabled = false;
            _pivot = pivot;
        }

        void Update()
        {
            if (_pivot == null || Time.time < _next) return;
            _next = Time.time + Interval;
            var p = transform.position;
            var lean = Sway.LeanPixels(Time.time, Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Sway.Current());
            if (lean == _lean) return;
            _lean = lean;
            _pivot.localRotation = Quaternion.Euler(0f, 0f, Sway.AngleFor(lean));
        }
    }
}
