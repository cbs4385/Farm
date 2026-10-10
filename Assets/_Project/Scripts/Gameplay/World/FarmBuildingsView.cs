using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // Draws the farm's greenhouse, coop and barn from the saved game state and gives each its door: a warp to the building's map, with the
    // spawn point where one comes back out. Everything is rebuilt from the state, so a building can be moved and the farm redrawn.
    public sealed class FarmBuildingsView : MonoBehaviour
    {
        [SerializeField] FarmMap _map;
        [SerializeField] TileBase _wall;
        [SerializeField] TileBase _roof;
        [SerializeField] TileBase _door;
        [SerializeField] TileBase _hiddenWall;                          // collision with nothing drawn: what stands under a building's picture
        [SerializeField] Sprite _coopPicture, _barnPicture;             // the kit's chicken coop and barn (the greenhouse keeps the tiles)

        // Where the door is in each picture (pixels from its left edge), so that the picture can stand with its door on the building's door cell.
        const int CoopDoorPixel = 22, BarnDoorPixel = 48;

        readonly List<GameObject> _made = new List<GameObject>();
        readonly List<Vector3Int> _wallCells = new List<Vector3Int>();
        readonly List<Vector3Int> _doorCells = new List<Vector3Int>();
        readonly Dictionary<Vector3Int, TileBase> _groundBefore = new Dictionary<Vector3Int, TileBase>();

        public void Configure(FarmMap map, TileBase wall, TileBase roof, TileBase door, TileBase hiddenWall = null, Sprite coopPicture = null, Sprite barnPicture = null)
        {
            _map = map; _wall = wall; _roof = roof; _door = door;
            _hiddenWall = hiddenWall; _coopPicture = coopPicture; _barnPicture = barnPicture;
        }

        // The picture for a kind of building, with where its door is, or null when it is drawn from tiles.
        Sprite PictureFor(FarmBuildingType type, out int doorPixel)
        {
            doorPixel = 0;
            if (_hiddenWall == null) return null;
            if (type.Id == "coop" && _coopPicture != null) { doorPixel = CoopDoorPixel; return _coopPicture; }
            if (type.Id == "barn" && _barnPicture != null) { doorPixel = BarnDoorPixel; return _barnPicture; }
            return null;
        }

        // Clears what was drawn and draws every building where the state says it stands.
        // `hide` names a building that is being carried: it is left out until it is put down (the saved state still holds it).
        public void Rebuild(GameSession session, string hide = null)
        {
            if (_map == null || session == null) return;
            FarmBuildings.EnsureDefaults(session.State);
            Build(hide == null ? session.State.FarmBuildings : session.State.FarmBuildings.FindAll(b => b.TypeId != hide));
        }

        // Draws these buildings (also used by the tests, which have no game).
        public void Build(IReadOnlyList<FarmBuildingState> buildings)
        {
            if (_map == null) return;
            Clear();
            foreach (var at in buildings)
            {
                var type = FarmBuildings.Type(at.TypeId);
                if (type == null) continue;
                var door = FarmBuildings.DoorCell(type, at);
                var picture = PictureFor(type, out var doorPixel);
                if (picture != null) { BuildPicture(type, at, door, picture, doorPixel); MakeDoor(type, door); continue; }
                for (var y = at.Y; y < at.Y + type.H; y++)
                    for (var x = at.X; x < at.X + type.W; x++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        if (x == door.x && y == door.y)
                        {
                            _groundBefore[cell] = _map.Ground.GetTile(cell);
                            _map.Ground.SetTile(cell, _door);
                            _doorCells.Add(cell);
                            continue;
                        }
                        _map.Walls.SetTile(cell, FarmBuildings.IsRoof(type, at, y) ? _roof : _wall);
                        _wallCells.Add(cell);
                    }
                MakeDoor(type, door);
            }
        }

        // A building drawn as a picture: hidden collision on the cells the picture covers (except the door), the door tile under it, the picture drawn over a player
        // who stands behind it with its door on the door cell.
        void BuildPicture(FarmBuildingType type, FarmBuildingState at, (int x, int y) door, Sprite picture, int doorPixel)
        {
            var width = picture.rect.width;
            var left = door.x + 0.5f - doorPixel / 16f;
            var right = left + width / 16f;
            var firstCell = Mathf.FloorToInt(left + 0.5f);
            var lastCell = Mathf.CeilToInt(right - 0.5f) - 1;
            for (var y = at.Y; y < at.Y + type.H; y++)
                for (var x = firstCell; x <= lastCell; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (x == door.x && y == door.y)
                    {
                        _groundBefore[cell] = _map.Ground.GetTile(cell);
                        _map.Ground.SetTile(cell, _door);
                        _doorCells.Add(cell);
                        continue;
                    }
                    _map.Walls.SetTile(cell, _hiddenWall);
                    _wallCells.Add(cell);
                }
            var go = new GameObject("Picture_" + type.Id);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(left + width / 32f, door.y + 0.5f, 0f);          // the picture's pivot is half a cell above its foot
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = picture;
            renderer.sortingOrder = 11;                                                           // over a player who stands behind it (the player is at 10)
            HoverNote.Add(go, "hover.farm." + type.Id, null, new Vector2(width / 16f, picture.rect.height / 16f * 0.75f), new Vector2(0f, picture.rect.height / 32f * 0.75f - 0.5f));
            _made.Add(go);
        }

        void MakeDoor(FarmBuildingType type, (int x, int y) door)
        {
            var go = new GameObject("Door_" + type.Id);
            go.transform.SetParent(transform, false);
            go.transform.position = _map.CellCenter(new Vector3Int(door.x, door.y, 0));
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            var warp = go.AddComponent<Warp>();
            warp.TargetMap = type.InteriorMap;
            warp.TargetSpawn = "default";
            warp.Condition = "flag:" + type.UnlockFlag;
            warp.BlockedMessageKey = type.LockedKey;
            _made.Add(go);

            var spawn = new GameObject("Spawn_" + type.ReturnSpawn);
            spawn.transform.SetParent(transform, false);
            spawn.transform.position = _map.CellCenter(new Vector3Int(door.x, door.y - 2, 0));
            spawn.AddComponent<SpawnPoint>().Id = type.ReturnSpawn;
            _made.Add(spawn);
        }

        void Clear()
        {
            foreach (var cell in _wallCells) _map.Walls.SetTile(cell, null);
            foreach (var cell in _doorCells) _map.Ground.SetTile(cell, _groundBefore.TryGetValue(cell, out var before) && before != null ? before : null);
            _wallCells.Clear();
            _doorCells.Clear();
            _groundBefore.Clear();
            foreach (var go in _made)
                if (go != null) { go.SetActive(false); Destroy(go); }          // gone at once for anyone looking, freed at the end of the frame
            _made.Clear();
        }
    }
}
