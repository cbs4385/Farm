using System.Collections.Generic;
using Farm.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Draws a FarmGrid onto the map's Soil and Crops tilemaps. Tile objects are created at runtime and cached.
    public sealed class FarmMapView : MonoBehaviour
    {
        [SerializeField] FarmMap _map;
        [SerializeField] Sprite _tilled;
        [SerializeField] Sprite _watered;

        readonly Dictionary<Sprite, Tile> _tiles = new Dictionary<Sprite, Tile>();
        FarmGrid _grid;
        GameDatabase _db;

        public void Configure(FarmMap map, Sprite tilled, Sprite watered)
        {
            _map = map;
            _tilled = tilled;
            _watered = watered;
        }

        public void Bind(FarmGrid grid, GameDatabase db)
        {
            _grid = grid;
            _db = db;
            RefreshAll();
        }

        public void RefreshAll()
        {
            _map.Soil.ClearAllTiles();
            _map.Crops.ClearAllTiles();
            if (_grid == null) return;
            foreach (var t in _grid.Tiles) Draw(t);
        }

        public void RefreshCell(Vector3Int cell)
        {
            if (_grid == null) return;
            if (_grid.TryGetTile(cell.x, cell.y, out var tile)) Draw(tile);
            else
            {
                _map.Soil.SetTile(cell, null);
                _map.Crops.SetTile(cell, null);
            }
        }

        void Draw(FarmTile tile)
        {
            var cell = new Vector3Int(tile.X, tile.Y, 0);
            _map.Soil.SetTile(cell, TileFor(tile.Watered ? _watered : _tilled));

            Sprite cropSprite = null;
            if (tile.Crop != null && _db != null && _db.TryGetCrop(tile.Crop.CropId, out var def))
                cropSprite = def.SpriteForStage(tile.Crop.Stage);
            _map.Crops.SetTile(cell, cropSprite != null ? TileFor(cropSprite) : null);
        }

        Tile TileFor(Sprite sprite)
        {
            if (sprite == null) return null;
            if (!_tiles.TryGetValue(sprite, out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.name = sprite.name;
                _tiles[sprite] = tile;
            }
            return tile;
        }
    }
}
