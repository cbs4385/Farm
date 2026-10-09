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
            if (!TryGetComponent<TileSway>(out _)) gameObject.AddComponent<TileSway>();
            RefreshAll();
        }

        public void RefreshAll()
        {
            _map.Soil.ClearAllTiles();
            _map.Crops.ClearAllTiles();
            _map.Nodes?.ClearAllTiles();
            _lean.Clear();
            if (_grid != null) foreach (var t in _grid.Tiles) Draw(t);
            if (_nodeGrid != null) foreach (var n in _nodeGrid.Nodes) DrawNode(n.X, n.Y);
        }

        readonly Dictionary<Vector3Int, int> _lean = new Dictionary<Vector3Int, int>();
        readonly List<Vector3Int> _gone = new List<Vector3Int>();

        // Leans every growing crop, fruit tree and soft (walk-through) forage plant the way the wind does at `time`. Only tiles whose lean
        // changed are touched; a tile that is redrawn loses its matrix, so redrawing forgets its cached lean and the next pass sets it again.
        public void ApplySway(float time, float strength = 1f)
        {
            if (_map == null || _map.Crops == null) return;
            _gone.Clear();
            foreach (var kv in _lean)
                if (_map.Crops.GetTile(kv.Key) == null && (_map.Nodes == null || _map.Nodes.GetTile(kv.Key) == null)) _gone.Add(kv.Key);
            foreach (var cell in _gone) _lean.Remove(cell);

            if (_grid != null)
                foreach (var t in _grid.Tiles)
                {
                    if (t.Crop == null || t.Crop.Withered || t.Crop.Stage < 1) continue;
                    SetLean(_map.Crops, new Vector3Int(t.X, t.Y, 0), Sway.LeanPixels(time, t.X, t.Y, strength));
                }
            if (_nodeGrid != null && _map.Nodes != null && _nodeCatalog != null)
                foreach (var n in _nodeGrid.Nodes)
                {
                    var def = _nodeCatalog.Get(n.TypeId);
                    if (def == null || def.Solid) continue;                                      // solid nodes keep their collider still
                    SetLean(_map.Nodes, new Vector3Int(n.X, n.Y, 0), Sway.LeanPixels(time, n.X, n.Y, strength));
                }
        }

        void SetLean(Tilemap map, Vector3Int cell, int lean)
        {
            if (_lean.TryGetValue(cell, out var current) && current == lean) return;
            _lean[cell] = lean;
            map.SetTransformMatrix(cell, lean == 0 ? Matrix4x4.identity : Sway.Shear(lean));
        }

        // Draws (or clears) the resource node standing on a cell.
        public void RefreshNode(Vector3Int cell)
        {
            _lean.Remove(cell);
            _map.Nodes?.SetTransformMatrix(cell, Matrix4x4.identity);      // SetTile keeps the old matrix: start upright
            DrawNode(cell.x, cell.y);
        }

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

        // Solid nodes block walking through their cell; weeds do not.
        Tile NodeTileFor(ResourceNodeDefinition def)
        {
            if (!_nodeTiles.TryGetValue(def.Id, out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = def.Sprite;
                tile.name = def.Sprite.name;
                tile.colliderType = def.Solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;      // one cell, however tall the picture is (a tree's crown is not a wall)
                _nodeTiles[def.Id] = tile;
            }
            return tile;
        }

        public void RefreshCell(Vector3Int cell)
        {
            if (_grid == null) return;
            _lean.Remove(cell);
            _map.Crops.SetTransformMatrix(cell, Matrix4x4.identity);       // SetTile keeps the old matrix: start upright
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
                cropSprite = tile.Crop.Withered ? def.SpriteForWithered(tile.Crop.Stage) : def.SpriteForStage(tile.Crop.Stage);
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
