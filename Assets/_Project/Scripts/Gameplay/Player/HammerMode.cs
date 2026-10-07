using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The builder's mallet. Swing it at a building, a fixture (the bed, the kitchen) or something that was set down: it is lifted and a ghost follows the
    // target square, green where it may go and red where it may not. Swing again to put it down; the Interact button puts it back where it was.
    // Everything that is lifted stays in the saved state at its old place until it is put down, so saving in between loses nothing.
    public sealed class HammerMode : MonoBehaviour
    {
        enum Kind { None, Building, Fixture, Placed }
        int _turns;                                 // quarter turns the carried piece will be put down with

        static readonly Color Good = new Color(0.45f, 0.95f, 0.45f, 0.55f);
        static readonly Color Bad = new Color(0.95f, 0.35f, 0.35f, 0.55f);

        PlayerActions _actions;
        FarmMap _map;
        GameSession _session;

        Kind _kind;
        string _buildingType;
        MovableFixture _fixture;
        PlacedObject _placed;
        Vector3Int _origin;                       // where the thing was lifted from

        readonly List<SpriteRenderer> _ghost = new List<SpriteRenderer>();
        Sprite _square;
        Sprite _picture;                          // the picture of a fixture or an object

        public bool Carrying => _kind != Kind.None;
        public string CarryingBuilding => _kind == Kind.Building ? _buildingType : null;
        public string LastRefusal { get; private set; }

        public void Init(PlayerActions actions, FarmMap map, GameSession session)
        {
            _actions = actions; _map = map; _session = session;
        }

        void Update()
        {
            if (!Carrying) return;
            if (_session == null || !_session.InGame || !HoldingTheMallet()) { Cancel(); return; }
            if (ServiceLocator.TryGet<InputService>(out var input) && input.Rotate.WasPressedThisFrame()) Rotate();
            ShowGhost(_actions.Target);
        }

        // Turns what is carried a quarter turn (R). Buildings, and anything but furniture that was set down, do not turn. Returns whether it turned.
        public bool Rotate()
        {
            if (!CanTurn()) return false;
            _turns = (_turns + 1) & 3;
            AudioService.PlayIfAvailable(Sfx.Plant, 0.5f, 1.2f);
            return true;
        }

        bool CanTurn() => _kind == Kind.Fixture || (_kind == Kind.Placed && _placed != null && _session.Placeables.Get(_placed.TypeId) is PlaceableDefinition def && def.Kind == PlaceableKind.Decor);

        bool HoldingTheMallet()
        {
            var stack = _session.Backpack.Get(_session.State.SelectedHotbar);
            return stack != null && stack.ItemId == ItemIds.Hammer;
        }

        // ---- the swing --------------------------------------------------------------------------------------------

        public void Use(Vector3Int cell)
        {
            if (Carrying) { PutDown(cell); return; }
            LiftAt(cell);
        }

        void LiftAt(Vector3Int cell)
        {
            _origin = cell;
            _turns = 0;
            // A building (on the farm).
            var buildings = FindFirstObjectByType<FarmBuildingsView>();
            if (buildings != null)
            {
                foreach (var at in _session.State.FarmBuildings)
                {
                    var type = FarmBuildings.Type(at.TypeId);
                    if (type == null || !FarmBuildings.Covers(type, at, cell.x, cell.y)) continue;
                    _kind = Kind.Building;
                    _buildingType = type.Id;
                    buildings.Rebuild(_session, hide: type.Id);
                    Told("build.lifted");
                    return;
                }
            }
            // A fixture of the room.
            MovableFixture lifted = null;
            foreach (var candidate in FindObjectsByType<MovableFixture>(FindObjectsSortMode.None))
                if (candidate.Covers(_map, cell) && (lifted == null || lifted.Walkable)) lifted = candidate;      // a piece standing on a rug is lifted before the rug
            if (lifted != null)
            {
                _kind = Kind.Fixture;
                _fixture = lifted;
                _picture = lifted.GetComponentInChildren<SpriteRenderer>()?.sprite;
                _origin = lifted.Cell(_map);
                _turns = lifted.Turns;
                lifted.SetCarried(true);
                Told("build.lifted_turn");
                return;
            }
            // Something set down (a chest, a machine, furniture).
            var objects = _session.GetObjects(_map.MapId);
            var placed = objects.At(cell.x, cell.y);
            if (placed != null)
            {
                _kind = Kind.Placed;
                _placed = placed;
                _turns = placed.Turns;
                var actor = PlacedObjectsView.Current != null ? PlacedObjectsView.Current.At(cell) : null;
                _picture = actor != null ? actor.GetComponent<SpriteRenderer>().sprite : null;
                if (actor != null) PlacedObjectsView.Current.Despawn(placed.Id);
                Told(CanTurn() ? "build.lifted_turn" : "build.lifted");
                return;
            }
            _session.Toast(L.Get("build.nothing"));
        }

        void PutDown(Vector3Int cell)
        {
            var why = Why(cell);
            LastRefusal = why;
            if (why != null) { _session.Toast(L.Get(why)); AudioService.PlayIfAvailable(Sfx.Error); return; }
            switch (_kind)
            {
                case Kind.Building:
                {
                    var type = FarmBuildings.Type(_buildingType);
                    var at = FarmBuildings.Find(_session.State, _buildingType);
                    at.X = cell.x - type.DoorX;
                    at.Y = cell.y;
                    FindFirstObjectByType<FarmBuildingsView>().Rebuild(_session);
                    break;
                }
                case Kind.Fixture:
                    _fixture.SetCarried(false);
                    _fixture.Turns = _turns;
                    _fixture.Place(_map, cell);
                    _fixture.Remember(_session, _map, cell);
                    break;
                case Kind.Placed:
                    _session.GetObjects(_map.MapId).Move(_placed.Id, cell.x, cell.y);
                    _placed.Turns = _turns;
                    PlacedObjectsView.Current?.Spawn(_placed);
                    break;
            }
            AudioService.PlayIfAvailable(Sfx.Plant);
            Clear();
            _session.Toast(L.Get("build.placed"));
            _session.NotifyChanged();
        }

        // Puts what is carried back where it was lifted from. Returns true when something was being carried.
        public bool Cancel()
        {
            if (!Carrying) return false;
            switch (_kind)
            {
                case Kind.Building: FindFirstObjectByType<FarmBuildingsView>()?.Rebuild(_session); break;
                case Kind.Fixture: if (_fixture != null) _fixture.SetCarried(false); break;
                case Kind.Placed: PlacedObjectsView.Current?.Spawn(_placed); break;
            }
            Clear();
            return true;
        }

        void Clear()
        {
            _kind = Kind.None;
            _buildingType = null;
            _fixture = null;
            _placed = null;
            _picture = null;
            foreach (var r in _ghost) if (r != null) r.gameObject.SetActive(false);
        }

        void Told(string key) => _session.Toast(L.Get(key));

        // ---- where it may go ------------------------------------------------------------------------------------------

        // null when the thing may be put down on `cell`, else the string key that says why not. (For a building the cell is where its door goes.)
        string Why(Vector3Int cell)
        {
            switch (_kind)
            {
                case Kind.Building:
                {
                    var type = FarmBuildings.Type(_buildingType);
                    _map.Ground.CompressBounds();
                    var b = _map.Ground.cellBounds;
                    var result = FarmBuildings.CanPlace(type, cell.x - type.DoorX, cell.y, b.xMax, b.yMax, _session.State.FarmBuildings, (x, y) => OpenForBuilding(new Vector3Int(x, y, 0)));
                    return result == FarmBuildings.Placement.Ok ? null : "build.refused." + result.ToString().ToLowerInvariant();
                }
                case Kind.Fixture:
                    foreach (var c in _fixture.Footprint(cell, _turns))
                    {
                        var refused = WhyNotOnCell(c, _fixture.Walkable, ignoreBodies: _fixture.Walkable);
                        if (refused != null) return refused;
                    }
                    return null;
                case Kind.Placed:
                {
                    var walkable = _session.Placeables.Get(_placed.TypeId) is PlaceableDefinition def && def.Walkable;      // a rug may lie in a doorway
                    return WhyNotOnCell(cell, walkable);
                }
            }
            return "placeable.blocked";
        }

        string WhyNotOnCell(Vector3Int cell, bool walkable, bool ignoreBodies = false)
        {
            if (!_map.CanPlaceAt(cell, ignoreBodies)) return "placeable.blocked";
            var objects = _session.GetObjects(_map.MapId);
            var there = objects.At(cell.x, cell.y);
            if (there != null && there != _placed) return "placeable.blocked";
            if (_session.GetGrid(_map.MapId).IsTilled(cell.x, cell.y) || _session.GetNodes(_map.MapId).Has(cell.x, cell.y)) return "placeable.blocked";
            if (!ignoreBodies && cell == _map.WorldToCell(_actions.transform.position)) return "placeable.blocked";
            if (!ignoreBodies && CellOccupants.IsTaken(cell)) return "placeable.blocked";
            if (DecorRules.BlocksDoor(cell, walkable, DoorCells())) return "placeable.decor_door";
            return null;
        }

        // A building needs open ground: grass or dirt, no wall, nothing planted, dug, growing or set down there, and nobody standing on it.
        bool OpenForBuilding(Vector3Int c) =>
            _map.IsOpenGround(c) && !_session.GetGrid(_map.MapId).IsTilled(c.x, c.y) && !_session.GetNodes(_map.MapId).Has(c.x, c.y)
            && !_session.GetObjects(_map.MapId).Has(c.x, c.y) && !CellOccupants.IsTaken(c);

        IEnumerable<Vector3Int> DoorCells()
        {
            foreach (var warp in FindObjectsByType<Warp>(FindObjectsSortMode.None))
            {
                var col = warp.GetComponent<Collider2D>();
                if (col == null) continue;
                yield return _map.WorldToCell(col.bounds.center);
            }
        }

        // ---- the ghost ------------------------------------------------------------------------------------------------

        void ShowGhost(Vector3Int cell)
        {
            _square ??= RuntimeSprites.Square(Color.white, 16);
            var ok = Why(cell) == null;
            var color = ok ? Good : Bad;
            var cells = new List<Vector3Int>();
            if (_kind == Kind.Building)
            {
                var type = FarmBuildings.Type(_buildingType);
                var x0 = cell.x - type.DoorX;
                for (var y = cell.y; y < cell.y + type.H; y++)
                    for (var x = x0; x < x0 + type.W; x++) cells.Add(new Vector3Int(x, y, 0));
            }
            else cells.Add(cell);

            if (_kind == Kind.Fixture && _fixture.Size != Vector2Int.one)       // one picture, as big as the piece, instead of a square per cell
            {
                var big = EnsureGhost(0);
                big.transform.rotation = Quaternion.Euler(0f, 0f, 90f * _turns);
                big.transform.position = _fixture.CenterAt(_map, cell, _turns);
                big.sprite = _picture != null ? _picture : _square;
                big.color = new Color(color.r, color.g, color.b, 0.85f);
                for (var i = 1; i < _ghost.Count; i++) _ghost[i].gameObject.SetActive(false);
                return;
            }

            for (var i = 0; i < cells.Count; i++)
            {
                var r = EnsureGhost(i);
                r.transform.position = _map.CellCenter(cells[i]);
                r.transform.rotation = _kind == Kind.Building ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f * _turns);
                r.sprite = _kind != Kind.Building && _picture != null ? _picture : _square;
                r.color = _kind != Kind.Building && _picture != null ? new Color(color.r, color.g, color.b, 0.85f) : color;
            }
            for (var i = cells.Count; i < _ghost.Count; i++) _ghost[i].gameObject.SetActive(false);
        }

        SpriteRenderer EnsureGhost(int i)
        {
            while (i >= _ghost.Count)
            {
                var go = new GameObject("BuildGhost");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 20;
                _ghost.Add(sr);
            }
            var r = _ghost[i];
            r.gameObject.SetActive(true);
            return r;
        }
    }
}
