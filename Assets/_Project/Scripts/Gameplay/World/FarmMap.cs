using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Scene-side description of a map: tilemap layers and ground rules. Holds no gameplay state.
    public sealed class FarmMap : MonoBehaviour
    {
        [SerializeField] string _mapId = MapIds.Farm;
        [SerializeField] Tilemap _ground;
        [SerializeField] Tilemap _soil;
        [SerializeField] Tilemap _crops;
        [SerializeField] Tilemap _nodes;
        [SerializeField] Tilemap _walls;
        [SerializeField] float _clutterDensity;   // share of open cells that start with a tree, rock or weed (0 = none)
        [SerializeField] string[] _tillableTileNames = { "tile_grass", "tile_dirt" };
        [SerializeField] bool _allowFarming = true;

        HashSet<string> _tillable;

        public string MapId => _mapId;
        public Tilemap Ground => _ground;
        public Tilemap Soil => _soil;
        public Tilemap Crops => _crops;
        public Tilemap Nodes => _nodes;
        public Tilemap Walls => _walls;
        public float ClutterDensity { get => _clutterDensity; set => _clutterDensity = value; }

        // Grass or dirt with no wall tile on it (does not depend on physics colliders having been built yet).
        public bool IsOpenGround(Vector3Int cell)
        {
            var tile = _ground.GetTile(cell);
            if (tile == null) return false;
            _tillable ??= new HashSet<string>(_tillableTileNames);
            return _tillable.Contains(tile.name) && (_walls == null || _walls.GetTile(cell) == null);
        }
        public bool AllowFarming => _allowFarming;

        public Vector3Int WorldToCell(Vector3 world) => _ground.WorldToCell(world);
        public Vector3 CellCenter(Vector3Int cell) => _ground.GetCellCenterWorld(cell);

        // Bounds of the painted ground in world space (camera limits).
        public Bounds WorldBounds
        {
            get
            {
                _ground.CompressBounds();
                var b = _ground.cellBounds;
                var min = _ground.CellToWorld(b.min);
                var max = _ground.CellToWorld(b.max);
                var bounds = new Bounds();
                bounds.SetMinMax(min, max);
                return bounds;
            }
        }

        // Ground cells whose tile is one of `tileNames`, with no wall tile on them.
        public IEnumerable<Vector3Int> CellsOn(IReadOnlyCollection<string> tileNames)
        {
            _ground.CompressBounds();
            foreach (var cell in _ground.cellBounds.allPositionsWithin)
            {
                var tile = _ground.GetTile(cell);
                if (tile == null || !System.Linq.Enumerable.Contains(tileNames, tile.name)) continue;
                if (_walls != null && _walls.GetTile(cell) != null) continue;
                yield return cell;
            }
        }

        // Any floor (not only farmland) with no wall tile and nothing solid on it: chests, machines and the like may go here.
        public bool CanPlaceAt(Vector3Int cell, bool ignoreBodies = false)
        {
            if (_ground.GetTile(cell) == null) return false;
            if (_walls != null && _walls.GetTile(cell) != null) return false;
            if (ignoreBodies) return true;
            foreach (var h in Physics2D.OverlapPointAll(CellCenter(cell)))
                if (!h.isTrigger && !h.CompareTag("Player")) return false;
            return true;
        }

        public bool IsWater(Vector3Int cell)
        {
            var tile = _ground.GetTile(cell);
            return tile != null && WaterShore.IsWaterTile(tile.name);
        }

        public bool IsTillable(Vector3Int cell)
        {
            if (!_allowFarming) return false;
            var tile = _ground.GetTile(cell);
            if (tile == null) return false;
            _tillable ??= new HashSet<string>(_tillableTileNames);
            if (!_tillable.Contains(tile.name)) return false;

            // Anything solid standing on the tile (house, bin, rock...) blocks tilling.
            var hits = Physics2D.OverlapPointAll(CellCenter(cell));
            foreach (var h in hits)
                if (!h.isTrigger && !h.CompareTag("Player")) return false;
            return true;
        }

        public void Configure(string mapId, Tilemap ground, Tilemap soil, Tilemap crops, bool allowFarming, Tilemap nodes = null, Tilemap walls = null)
        {
            _nodes = nodes;
            _walls = walls;
            _mapId = mapId;
            _ground = ground;
            _soil = soil;
            _crops = crops;
            _allowFarming = allowFarming;
        }
    }
}
