using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Shows the cells a sprinkler waters (playtest 2026-10-14: "highlight the area where a sprinkler works so I don't have to count out squares"): while a sprinkler is in hand
    // the cells it would water if put on the square, and while the square rests on a placed sprinkler the cells that one waters.
    public sealed class SprinklerRangeOverlay : MonoBehaviour
    {
        static readonly Color Tint = new Color(0.35f, 0.65f, 1f, 0.28f);

        readonly List<SpriteRenderer> _cells = new List<SpriteRenderer>();
        GameSession _session;
        FarmMap _map;
        PlayerActions _actions;
        Sprite _square;

        public void Init(PlayerActions actions, FarmMap map, GameSession session)
        {
            _actions = actions; _map = map; _session = session;
        }

        // The range of the sprinkler the square would put down or rests on, or 0 when there is none. (pure given the session)
        public static int RangeAt(GameSession session, string mapId, Vector3Int target, out Vector3Int center)
        {
            center = target;
            if (session == null || !session.InGame) return 0;
            var placed = session.GetObjects(mapId).At(target.x, target.y);
            if (placed != null)
            {
                var def = session.Placeables.Get(placed.TypeId);
                return def != null && def.Kind == PlaceableKind.Sprinkler ? def.Range : 0;
            }
            var held = session.Backpack.Get(session.State.SelectedHotbar);
            if (held == null || !session.Db.TryGetItem(held.ItemId, out var item) || string.IsNullOrEmpty(item.PlaceableId)) return 0;
            var held2 = session.Placeables.Get(item.PlaceableId);
            return held2 != null && held2.Kind == PlaceableKind.Sprinkler ? held2.Range : 0;
        }

        void LateUpdate()
        {
            if (_session == null || _actions == null || _map == null || !_session.InGame) { Hide(0); return; }
            var range = RangeAt(_session, _map.MapId, _actions.Target, out var center);
            if (range <= 0) { Hide(0); return; }
            _square ??= RuntimeSprites.Square(Color.white, 16);
            var used = 0;
            foreach (var (x, y) in Sprinklers.Covered(center.x, center.y, range))
            {
                var r = Cell(used++);
                r.transform.position = _map.CellCenter(new Vector3Int(x, y, 0));
            }
            Hide(used);
        }

        SpriteRenderer Cell(int i)
        {
            while (i >= _cells.Count)
            {
                var go = new GameObject("SprinklerRange");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _square;
                sr.color = Tint;
                sr.sortingOrder = 5;
                _cells.Add(sr);
            }
            var cell = _cells[i];
            cell.gameObject.SetActive(true);
            return cell;
        }

        void Hide(int from)
        {
            for (var i = from; i < _cells.Count; i++) _cells[i].gameObject.SetActive(false);
        }
    }
}
