using UnityEngine;

namespace Farm.Gameplay
{
    // A building's picture is drawn over a player who stands behind it, and under one who stands in front of it or in its doorway (playtest 2026-10-12: the avatar vanished
    // into the building when walking up to its door). The line is the top of the door's row: a player north of it is behind the building. The sign and anything else under
    // the same object keep their place relative to the picture.
    public sealed class BuildingDepth : MonoBehaviour, IMapBound
    {
        public const int DefaultFront = 8;          // under villagers (9) and the player (10), over ground things (4)

        // Saved with the scene: the map builder sets these when it makes the scene, and the game reads them back.
        [SerializeField] float _lineY;
        [SerializeField] int _behind = 11;
        [SerializeField] int _front = DefaultFront;

        SpriteRenderer[] _renderers;
        int[] _offsets;
        Transform _player;
        bool _over = true;

        public float LineY => _lineY;
        public int BehindOrder => _behind;

        // Is the picture drawn over a player at this height? (pure)
        public static bool IsOverPlayer(float playerY, float lineY) => playerY >= lineY;

        // `behind`: the picture's order while the player is behind it; `front`: its order while the player is in front. Call after the children have been added.
        public void Configure(float lineY, int behind, int front = DefaultFront)
        {
            _lineY = lineY; _behind = behind; _front = front;
            Gather();
            Apply(true);
        }

        // The renderers and where each sits relative to the picture's own order (which, as saved, is the "behind" order).
        void Gather()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _offsets = new int[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++) _offsets[i] = _renderers[i].sortingOrder - _behind;
        }

        void Awake()
        {
            if (_renderers == null) Gather();
        }

        public void Bind(MapContext map) => _player = map.Player != null ? map.Player.transform : null;

        void LateUpdate()
        {
            if (_player == null || _renderers == null) return;
            var over = IsOverPlayer(_player.position.y, _lineY);
            if (over != _over) Apply(over);
        }

        void Apply(bool over)
        {
            _over = over;
            var baseOrder = over ? _behind : _front;
            for (var i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].sortingOrder = baseOrder + _offsets[i];
        }
    }
}
