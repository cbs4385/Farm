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

        public string Id { get => _id; set => _id = value; }
        public string SpawnId { get => _spawnId; set => _spawnId = value; }
        public Vector2Int SpawnOffset { get => _spawnOffset; set => _spawnOffset = value; }

        // Moves the fixture to the cell the save says (when it says anything).
        public void Apply(GameSession session, FarmMap map)
        {
            foreach (var f in session.State.Fixtures)
                if (f.Map == map.MapId && f.Id == _id) { Place(map, new Vector3Int(f.X, f.Y, 0)); return; }
        }

        // Puts it on a cell and moves its spawn point; the caller records it with Remember.
        public void Place(FarmMap map, Vector3Int cell)
        {
            transform.position = map.CellCenter(cell);
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

        public Vector3Int Cell(FarmMap map) => map.WorldToCell(transform.position);
    }
}
