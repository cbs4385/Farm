using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The on-screen body of a villager. It only renders the NpcPlacement it is given (a pure function of the clock), so it
    // is always where the schedule says, and walking is smooth along an A* path without ever teleporting in view.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class NpcActor : MonoBehaviour, IInteractable
    {
        NpcDefinition _definition;
        SpriteRenderer _renderer;
        Vector2Int _facing = Vector2Int.down;

        // The path of the leg being walked, as cell centres, with cumulative lengths.
        (int fx, int fy, int tx, int ty) _pathKey = (int.MinValue, 0, 0, 0);
        readonly List<Vector3> _points = new List<Vector3>();
        readonly List<float> _lengths = new List<float>();

        public NpcDefinition Definition => _definition;
        public bool IsWalking { get; private set; }
        public Vector2Int Facing => _facing;
        public Vector3Int Cell { get; private set; }

        public void Setup(NpcDefinition definition)
        {
            _definition = definition;
            name = "Npc_" + definition.Id;
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = 9;
            _renderer.sprite = definition.SpriteFor(_facing);
        }

        public void Apply(NpcPlacement place, FarmMap map, WalkGrid grid)
        {
            IsWalking = place.Walking;
            if (!place.Walking)
            {
                var cell = new Vector3Int(place.ToX, place.ToY, 0);
                transform.position = map.CellCenter(cell);
                Cell = cell;
                SetFacing(NpcSchedule.FacingVector(place.Facing));
                return;
            }

            var key = (place.FromX, place.FromY, place.ToX, place.ToY);
            if (!key.Equals(_pathKey))
            {
                _pathKey = key;
                BuildPath(place, map, grid);
            }

            var total = _lengths.Count > 0 ? _lengths[_lengths.Count - 1] : 0f;
            var distance = Mathf.Clamp01(place.T) * total;
            var i = 1;
            while (i < _lengths.Count - 1 && _lengths[i] < distance) i++;
            if (_points.Count < 2)
            {
                if (_points.Count == 1) transform.position = _points[0];
                return;
            }
            var segment = _lengths[i] - _lengths[i - 1];
            var t = segment > 0f ? Mathf.Clamp01((distance - _lengths[i - 1]) / segment) : 1f;
            var a = _points[i - 1]; var b = _points[i];
            transform.position = Vector3.Lerp(a, b, t);
            Cell = map.WorldToCell(transform.position);
            var direction = b - a;
            if (direction.sqrMagnitude > 0.0001f)
                SetFacing(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? (direction.x > 0 ? Vector2Int.right : Vector2Int.left)
                    : (direction.y > 0 ? Vector2Int.up : Vector2Int.down));
        }

        void BuildPath(NpcPlacement place, FarmMap map, WalkGrid grid)
        {
            _points.Clear();
            _lengths.Clear();
            var path = grid != null ? grid.FindPath(place.FromX, place.FromY, place.ToX, place.ToY) : null;
            if (path == null) path = new List<(int x, int y)> { (place.FromX, place.FromY), (place.ToX, place.ToY) };   // no way found: straight line
            foreach (var (x, y) in path) _points.Add(map.CellCenter(new Vector3Int(x, y, 0)));
            var length = 0f;
            _lengths.Add(0f);
            for (var i = 1; i < _points.Count; i++)
            {
                length += Vector3.Distance(_points[i - 1], _points[i]);
                _lengths.Add(length);
            }
        }

        // Used by cutscenes, which place and move the actor themselves while the schedule is suspended.
        public void SetCell(FarmMap map, Vector3Int cell)
        {
            transform.position = map.CellCenter(cell);
            Cell = cell;
        }

        public void SetWorld(Vector3 position, FarmMap map)
        {
            transform.position = position;
            Cell = map.WorldToCell(position);
        }

        public void SetFacing(Vector2Int facing)
        {
            _facing = facing;
            if (_renderer != null && _definition != null) _renderer.sprite = _definition.SpriteFor(facing);
        }

        public void Interact(PlayerActions player)
        {
            if (_definition == null) return;
            // Turn to face the player, like someone who has been spoken to.
            var toPlayer = player.transform.position - transform.position;
            SetFacing(Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y)
                ? (toPlayer.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (toPlayer.y > 0 ? Vector2Int.up : Vector2Int.down));
            NpcInteractions.Talk(player.Session, _definition);
        }
    }
}
