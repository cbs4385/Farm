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
        NodeGrid _nodeGrid;
        NodeCatalog _nodeCatalog;
        GameDatabase _db;

        public void Configure(FarmMap map, Sprite tilled, Sprite watered)
        {
            _map = map;
            _tilled = tilled;
            _watered = watered;
        }

        public void Bind(FarmGrid grid, GameDatabase db, NodeGrid nodes = null, NodeCatalog nodeCatalog = null)
        {
            _grid = grid;
            _db = db;
            _nodeGrid = nodes;
            _nodeCatalog = nodeCatalog;
            RefreshAll();
        }

        public void RefreshAll()
        {
            _map.Soil.ClearAllTiles();
            _map.Crops.ClearAllTiles();
            _map.Nodes?.ClearAllTiles();
            if (_grid != null) foreach (var t in _grid.Tiles) Draw(t);
            if (_nodeGrid != null) foreach (var n in _nodeGrid.Nodes) DrawNode(n.X, n.Y);
        }

        // Draws (or clears) the resource node standing on a cell.
        public void RefreshNode(Vector3Int cell) => DrawNode(cell.x, cell.y);

        void DrawNode(int x, int y)
        {
            if (_map.Nodes == null) return;
            var cell = new Vector3Int(x, y, 0);
            if (_nodeGrid == null || !_nodeGrid.TryGet(x, y, out var node) || _nodeCatalog == null)
            {
                _map.Nodes.SetTile(cell, null);
                return;
            }
            var def = _nodeCatalog.Get(node.TypeId);
            _map.Nodes.SetTile(cell, def?.Sprite != null ? NodeTileFor(def) : null);
        }

        readonly Dictionary<string, Tile> _nodeTiles = new Dictionary<string, Tile>();

        // Solid nodes block walking through their tile collider; weeds do not.
        Tile NodeTileFor(ResourceNodeDefinition def)
        {
            if (!_nodeTiles.TryGetValue(def.Id, out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = def.Sprite;
                tile.name = def.Sprite.name;
                tile.colliderType = def.Solid ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
                _nodeTiles[def.Id] = tile;
            }
            return tile;
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
