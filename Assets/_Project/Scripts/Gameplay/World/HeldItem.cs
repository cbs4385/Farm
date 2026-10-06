using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The farmer's carry poses, drawn from the item's own icon (no extra art): whatever bulky thing is selected in the hotbar is held in front
    // of the body (behind it when facing away), and a fresh harvest, catch or find is held up over the head for a moment.
    public sealed class HeldItem : MonoBehaviour
    {
        public const float HoistSeconds = 1.1f;
        public const float CarryScale = 0.6f;
        public const float HoistScale = 0.85f;
        const float PixelHeight = 1f / 16f;

        SpriteRenderer _body;
        SpriteRenderer _icon;
        WalkBob _bob;
        GameSession _session;
        PlayerController _controller;
        string _shownId;
        Sprite _carried;
        Sprite _hoisted;
        float _hoistLeft;

        public bool IsCarrying { get; private set; }
        public bool IsHoisting => _hoistLeft > 0f;
        public Sprite ShownSprite => _icon != null && _icon.enabled ? _icon.sprite : null;

        // Things that are carried in the arms rather than swung or planted. (pure)
        public static bool Carries(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Crop:
                case ItemCategory.Forage:
                case ItemCategory.Fish:
                case ItemCategory.Artisan:
                case ItemCategory.Resource:
                case ItemCategory.Furniture:
                case ItemCategory.Machine:
                case ItemCategory.Animal:
                    return true;
                default:
                    return false;
            }
        }

        // Where the carried thing sits, from the farmer's feet: in front of the chest, or at the side in a side view. (pure)
        public static Vector2 CarryOffset(Vector2Int facing)
        {
            if (facing.x != 0) return new Vector2(facing.x * 0.3f, 0.7f);
            return new Vector2(0f, facing.y > 0 ? 0.85f : 0.65f);
        }

        // Held away from the viewer when the farmer faces up. (pure)
        public static bool BehindBody(Vector2Int facing) => facing.y > 0 && facing.x == 0;

        // How high above the feet the held-up thing is `t` (0..1) through a hoist: it rises fast, then hangs there. (pure)
        public static float HoistHeight(float t)
        {
            var rise = Mathf.Clamp01(t / 0.25f);
            return 1.7f + 0.45f * (1f - (1f - rise) * (1f - rise));
        }

        public static HeldItem For(GameObject player) => player.TryGetComponent<HeldItem>(out var held) ? held : player.AddComponent<HeldItem>();

        // Lifts the item's picture over the head for a moment.
        public void Hoist(Sprite sprite)
        {
            if (sprite == null) return;
            _hoisted = sprite;
            _hoistLeft = HoistSeconds;
        }

        void Awake()
        {
            _body = GetComponentInChildren<SpriteRenderer>();
            _bob = GetComponent<WalkBob>();
            _controller = GetComponent<PlayerController>();
            var go = new GameObject("HeldItem");
            go.transform.SetParent(transform, false);
            _icon = go.AddComponent<SpriteRenderer>();
            _icon.enabled = false;
        }

        void LateUpdate()
        {
            if (_icon == null) return;
            if (_hoistLeft > 0f) _hoistLeft = Mathf.Max(0f, _hoistLeft - Time.deltaTime);
            var facing = _controller != null ? _controller.Facing : Vector2Int.down;
            var order = _body != null ? _body.sortingOrder : 10;

            if (_hoistLeft > 0f && _hoisted != null)
            {
                IsCarrying = false;
                Place(_hoisted, new Vector2(0f, HoistHeight(1f - _hoistLeft / HoistSeconds)), HoistScale, order + 1);
                return;
            }

            _carried = CarriedSprite();
            IsCarrying = _carried != null;
            if (!IsCarrying) { _icon.enabled = false; return; }
            var offset = CarryOffset(facing);
            if (_bob != null && _bob.IsRaised) offset.y += PixelHeight;                 // it bobs with the body
            Place(_carried, offset, CarryScale, BehindBody(facing) ? order - 1 : order + 1);
        }

        void Place(Sprite sprite, Vector2 offset, float scale, int order)
        {
            _icon.sprite = sprite;
            _icon.sortingOrder = order;
            _icon.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            _icon.transform.localScale = new Vector3(scale, scale, 1f);
            _icon.enabled = true;
        }

        Sprite CarriedSprite()
        {
            // Looked up each time: the session is replaced by a new game or a load, and may not have started one yet.
            if (!ServiceLocator.TryGet(out GameSession session) || session.State == null || session.Backpack == null) { _shownId = null; _carried = null; return null; }
            _session = session;
            var stack = _session.Backpack.Get(_session.State.SelectedHotbar);
            if (stack == null) { _shownId = null; _carried = null; return null; }
            if (stack.ItemId != _shownId)
            {
                _shownId = stack.ItemId;
                _carried = _session.Db.TryGetItem(stack.ItemId, out var item) && Carries(item.Category) ? item.Icon : null;
            }
            return _carried;
        }
    }
}
