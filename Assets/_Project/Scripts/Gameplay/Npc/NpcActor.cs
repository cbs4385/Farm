using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The on-screen body of a villager. It only renders the NpcPlacement it is given (a pure function of the clock), so it
    // is always where the schedule says, and walking is smooth along an A* path without ever teleporting in view.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class NpcActor : MonoBehaviour, IInteractable, ICellOccupant
    {
        public string HoverLabel => _definition != null ? Farm.Core.L.Get(_definition.NameKey) : null;

        NpcDefinition _definition;
        SpriteRenderer _renderer;
        Vector2Int _facing = Vector2Int.down;

        // The path of the leg being walked, as cell centres, with cumulative lengths.
        (int fx, int fy, int tx, int ty) _pathKey = (int.MinValue, 0, 0, 0);
        readonly List<Vector3> _points = new List<Vector3>();
        readonly List<float> _lengths = new List<float>();

        // Playtest 2026-10-07: villagers walked into the player and pushed them, and walked through whatever was in the way. A walking villager now
        // stops short of anything solid on its way (the player, a piece of furniture put down since the map loaded), which holds its schedule back
        // (NpcManager) until the way is clear, and after RerouteAfter seconds of waiting finds a way round. Other villagers and animals do not block
        // it (two villagers in a corridor could wait for each other for ever).
        public const float RerouteAfter = 1.5f;
        const float ProbeSize = ActorBody.Size + 0.04f;          // as wide as the villager's body, or it would stop with its body already touching the player
        float _waitFor;
        float _fromT;                            // the schedule's progress along the leg when the route was last changed (0 at the start of a leg)
        Collider2D _playerBody;
        float _nextPlayerSearch;
        BoxCollider2D _body;

        // True while this villager is held up by something in the way.
        public bool Waiting { get; private set; }

        public NpcDefinition Definition => _definition;
        public bool IsWalking { get; private set; }
        public Vector2Int Facing => _facing;
        public Vector3Int Cell { get; private set; }
        public bool IsPlaced { get; private set; }

        void OnEnable() => CellOccupants.Add(this);
        void OnDisable() => CellOccupants.Remove(this);

        public void Setup(NpcDefinition definition)
        {
            _definition = definition;
            name = "Npc_" + definition.Id;
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = 9;
            ActorBody.Add(gameObject);
            var bodyTransform = transform.Find(ActorBody.BodyName);
            _body = bodyTransform != null ? bodyTransform.GetComponent<BoxCollider2D>() : null;
            (TryGetComponent<WalkBob>(out var bob) ? bob : gameObject.AddComponent<WalkBob>()).Breathes = true;
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
                IsPlaced = true;
                Waiting = false;
                _waitFor = 0f;
                SetFacing(NpcSchedule.FacingVector(place.Facing));
                YieldBodyToPlayer();
                return;
            }

            var key = (place.FromX, place.FromY, place.ToX, place.ToY);
            if (!key.Equals(_pathKey))
            {
                _pathKey = key;
                _fromT = 0f;
                _waitFor = 0f;
                BuildPath(place, map, grid);
            }

            var total = _lengths.Count > 0 ? _lengths[_lengths.Count - 1] : 0f;
            var progress = _fromT >= 1f ? 1f : Mathf.Clamp01((Mathf.Clamp01(place.T) - _fromT) / (1f - _fromT));
            var distance = progress * total;
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
            var proposed = Vector3.Lerp(a, b, t);

            // Something solid at the next step (but not on the cell it leaves or the one it is going to: a villager may sit on a seat that is a solid thing).
            var atEnds = (proposed - _points[0]).sqrMagnitude < 0.36f || (proposed - _points[_points.Count - 1]).sqrMagnitude < 0.36f;
            if (IsPlaced && !atEnds && SolidAt(proposed, ProbeSize))
            {
                Waiting = true;
                _waitFor += Time.deltaTime;
                if (_waitFor >= RerouteAfter) Reroute(place, map, grid);
                YieldBodyToPlayer();
                return;
            }
            Waiting = false;
            _waitFor = 0f;
            transform.position = proposed;
            Cell = map.WorldToCell(transform.position);
            IsPlaced = true;
            YieldBodyToPlayer();
            var direction = b - a;
            if (direction.sqrMagnitude > 0.0001f)
                SetFacing(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? (direction.x > 0 ? Vector2Int.right : Vector2Int.left)
                    : (direction.y > 0 ? Vector2Int.up : Vector2Int.down));
        }

        // Is anything solid in a box of this size at `centre`? The player counts; this villager's own body, triggers and other actors do not.
        bool SolidAt(Vector3 centre, float size)
        {
            foreach (var hit in Physics2D.OverlapBoxAll(centre, new Vector2(size, size), 0f))
            {
                if (hit.isTrigger || hit.transform.IsChildOf(transform)) continue;
                if (hit.GetComponentInParent<ICellOccupant>() != null) continue;
                return true;
            }
            return false;
        }

        // Held up for a while: a new route from where it stands to where it is going, round whatever is in the way (the villager keeps its place in
        // the schedule: the new route is walked over the rest of the leg). With no way round it keeps waiting.
        void Reroute(NpcPlacement place, FarmMap map, WalkGrid grid)
        {
            _waitFor = 0f;
            if (grid == null) return;
            var from = map.WorldToCell(transform.position);
            var path = grid.FindPath(from.x, from.y, place.ToX, place.ToY,
                (x, y) => { var c = new Vector3Int(x, y, 0); return CellOccupants.IsTaken(c, this) || SolidAt(map.CellCenter(c), 0.9f); });
            if (path == null || path.Count < 2) return;
            _points.Clear();
            _lengths.Clear();
            _points.Add(transform.position);
            for (var i = 1; i < path.Count; i++) _points.Add(map.CellCenter(new Vector3Int(path[i].x, path[i].y, 0)));
            var length = 0f;
            _lengths.Add(0f);
            for (var i = 1; i < _points.Count; i++)
            {
                length += Vector3.Distance(_points[i - 1], _points[i]);
                _lengths.Add(length);
            }
            _fromT = Mathf.Clamp01(place.T);
        }

        // The body of a villager never pushes the player: if the schedule puts it on top of the player (it arrives where the player stands), it is
        // not solid until the player has stepped out of it.
        void YieldBodyToPlayer()
        {
            if (_body == null) return;
            if (_playerBody == null && Time.unscaledTime >= _nextPlayerSearch)
            {
                _nextPlayerSearch = Time.unscaledTime + 0.5f;
                var player = Object.FindAnyObjectByType<PlayerController>();
                _playerBody = player != null ? player.GetComponent<Collider2D>() : null;
            }
            if (_playerBody == null) { _body.enabled = true; return; }
            var p = _playerBody.bounds;
            var half = (ActorBody.Size - 0.06f) * 0.5f;
            var c = transform.position;
            var overlapping = p.min.x < c.x + half && p.max.x > c.x - half && p.min.y < c.y + half && p.max.y > c.y - half;
            _body.enabled = !overlapping;
        }

        void BuildPath(NpcPlacement place, FarmMap map, WalkGrid grid)
        {
            _points.Clear();
            _lengths.Clear();
            // Around whoever is standing in the way when the walk starts; with no such way, the plain route (a villager never gets stuck).
            var path = grid != null ? grid.FindPath(place.FromX, place.FromY, place.ToX, place.ToY, (x, y) => CellOccupants.IsTaken(new Vector3Int(x, y, 0), this)) ?? grid.FindPath(place.FromX, place.FromY, place.ToX, place.ToY) : null;
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
            IsPlaced = true;
        }

        public void SetWorld(Vector3 position, FarmMap map)
        {
            transform.position = position;
            Cell = map.WorldToCell(position);
            IsPlaced = true;
        }

        public void SetFacing(Vector2Int facing)
        {
            _facing = facing;
            if (_pose == null && _renderer != null && _definition != null) _renderer.sprite = _definition.SpriteFor(facing);
        }

        // T-131: a full-body pose (wave, sit, shrug, point) replaces the idle sprite until ClearPose. False when this villager has no such pose.
        Sprite _pose;
        public bool ShowPose(string pose)
        {
            var sprite = _definition != null ? _definition.PoseFor(pose) : null;
            if (sprite == null || _renderer == null) return false;
            _pose = sprite;
            _renderer.sprite = sprite;
            return true;
        }

        public void ClearPose()
        {
            _pose = null;
            if (_renderer != null && _definition != null) _renderer.sprite = _definition.SpriteFor(_facing);
        }

        public string PoseShown => _pose != null ? _pose.name : null;

        public void Interact(PlayerActions player)
        {
            if (_definition == null) return;
            // Turn to face the player, like someone who has been spoken to.
            var toPlayer = player.transform.position - transform.position;
            SetFacing(Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y)
                ? (toPlayer.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (toPlayer.y > 0 ? Vector2Int.up : Vector2Int.down));
            if (NpcManager.Current != null) NpcManager.Current.Hold(_definition.Id);       // stand still for a while
            NpcInteractions.Talk(player.Session, _definition);
        }
    }
}
