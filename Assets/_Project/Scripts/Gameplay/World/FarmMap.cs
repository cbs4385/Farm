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
        [SerializeField] string[] _tillableTileNames = { "tile_grass", "tile_dirt" };
        [SerializeField] bool _allowFarming = true;

        HashSet<string> _tillable;

        public string MapId => _mapId;
        public Tilemap Ground => _ground;
        public Tilemap Soil => _soil;
        public Tilemap Crops => _crops;
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

        public void Configure(string mapId, Tilemap ground, Tilemap soil, Tilemap crops, bool allowFarming)
        {
            _mapId = mapId;
            _ground = ground;
            _soil = soil;
            _crops = crops;
            _allowFarming = allowFarming;
        }
    }
}
