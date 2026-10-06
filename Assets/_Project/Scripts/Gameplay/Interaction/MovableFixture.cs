using UnityEngine;

namespace Farm.Gameplay
{
    // A piece of a room that is part of the scene but that the player may move with the builder's mallet (the bed, the kitchen). Where it stands is
    // saved (GameState.Fixtures); without an entry it stays where the scene put it. A linked spawn point (the place one wakes up beside the bed)
    // goes with it.
    public sealed class MovableFixture : MonoBehaviour
    {
        [SerializeField] string _id;
        [SerializeField] string _spawnId;
        [SerializeField] Vector2Int _spawnOffset;
        [SerializeField] Vector2Int _size = Vector2Int.one;      // the cells it covers, from the cell it is saved at upwards and to the right (the double bed is 2 x 2)

        public string Id { get => _id; set => _id = value; }
        public string SpawnId { get => _spawnId; set => _spawnId = value; }
        [SerializeField] bool _walkable;      // a rug: one may walk over it, and it may lie under other furniture
        public bool Walkable { get => _walkable; set => _walkable = value; }
        public Vector2Int SpawnOffset { get => _spawnOffset; set => _spawnOffset = value; }
        public Vector2Int Size { get => new Vector2Int(Mathf.Max(1, _size.x), Mathf.Max(1, _size.y)); set => _size = value; }

        // The cells it covers when its saved cell is `origin`.
        public System.Collections.Generic.IEnumerable<Vector3Int> Footprint(Vector3Int origin)
        {
            var size = Size;
            for (var y = 0; y < size.y; y++)
                for (var x = 0; x < size.x; x++) yield return origin + new Vector3Int(x, y, 0);
        }

        public bool Covers(FarmMap map, Vector3Int cell)
        {
            foreach (var c in Footprint(Cell(map))) if (c == cell) return true;
            return false;
        }

        // Where its middle stands when the saved cell is `origin`.
        public Vector3 CenterAt(FarmMap map, Vector3Int origin) => map.CellCenter(origin) + new Vector3((Size.x - 1) * 0.5f, (Size.y - 1) * 0.5f, 0f);

        // Moves the fixture to the cell the save says (when it says anything).
        public void Apply(GameSession session, FarmMap map)
        {
            foreach (var f in session.State.Fixtures)
                if (f.Map == map.MapId && f.Id == _id) { Place(map, new Vector3Int(f.X, f.Y, 0)); return; }
        }

        // Puts it on a cell and moves its spawn point; the caller records it with Remember.
        public void Place(FarmMap map, Vector3Int cell)
        {
            transform.position = CenterAt(map, cell);
            if (string.IsNullOrEmpty(_spawnId)) return;
            foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
                if (sp.Id == _spawnId) sp.transform.position = map.CellCenter(cell + new Vector3Int(_spawnOffset.x, _spawnOffset.y, 0));
        }

        public void Remember(GameSession session, FarmMap map, Vector3Int cell)
        {
            var list = session.State.Fixtures;
            var entry = list.Find(f => f.Map == map.MapId && f.Id == _id);
            if (entry == null) { entry = new FixtureState { Map = map.MapId, Id = _id }; list.Add(entry); }
            entry.X = cell.x; entry.Y = cell.y;
        }

        // While it is being carried it is neither seen nor in the way.
        public void SetCarried(bool carried)
        {
            foreach (var r in GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = !carried;
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = !carried;
        }

        public Vector3Int Cell(FarmMap map) => map.WorldToCell(transform.position - new Vector3((Size.x - 1) * 0.5f, (Size.y - 1) * 0.5f, 0f));
    }
}
