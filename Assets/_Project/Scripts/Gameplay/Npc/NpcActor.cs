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
        // it (two villagers in a corridor could wait for each other for ever). Another villager stops it too, but only for VillagerGiveUp seconds:
        // after that it walks on, so two villagers in a corridor cannot wait for each other for ever.
        public const float VillagerGiveUp = 4f;
        static readonly List<NpcActor> Active = new List<NpcActor>();
        float _villagerWait;
        public const float RerouteAfter = 1.5f;
        const float ProbeSize = ActorBody.Size + 0.04f;          // as wide as the villager's body, or it would stop with its body already touching the player
        float _waitFor;
        float _fromT;                            // the schedule's progress along the leg when the route was last changed (0 at the start of a leg)
        Collider2D _playerBody;
        float _nextPlayerSearch;
        BoxCollider2D _body;

        // True while this villager is held up by something in the way.
        public bool Waiting { get; private set; }

        // Asleep in bed, and not to be talked to. The beds run north to south with the pillow at the top, so a sleeper is drawn as the head and shoulders
        // (the top of their picture) on the pillow, as if tucked in, rather than as a figure turned on its side across the bed.
        public bool Sleeping { get; private set; }
        SleepIcon _icon;
        public bool SleepIconShown => _icon != null && _icon.Shown;
        const float SleepKeep = 0.5f;                                                    // the top half of the picture stays above the blanket
        static readonly Dictionary<Sprite, Sprite> SleepSprites = new Dictionary<Sprite, Sprite>();

        // The top part of a villager's picture (null when they have none). Made once per picture; it shares the texture, so it costs nothing.
        public static Sprite SleepSpriteOf(Sprite whole)
        {
            if (whole == null) return null;
            if (SleepSprites.TryGetValue(whole, out var cached) && cached != null) return cached;
            var r = whole.textureRect;
            var part = new Rect(r.x, r.y + r.height * (1f - SleepKeep), r.width, r.height * SleepKeep);
            var sprite = Sprite.Create(whole.texture, part, new Vector2(0.5f, 0.5f), whole.pixelsPerUnit);
            sprite.name = whole.name + "_asleep";
            return SleepSprites[whole] = sprite;
        }

        void SetSleeping(bool sleeping)
        {
            if (Sleeping == sleeping) return;
            Sleeping = sleeping;
            if (_renderer != null)
            {
                _renderer.sortingOrder = sleeping ? 10 : 9;
                if (_definition != null) _renderer.sprite = sleeping ? SleepSpriteOf(_definition.SpriteFor(Vector2Int.down)) ?? _renderer.sprite : _definition.SpriteFor(_facing);
            }
            if (!sleeping && _icon != null) _icon.Hide();
        }

        public NpcDefinition Definition => _definition;
        public bool IsWalking { get; private set; }
        public Vector2Int Facing => _facing;
        public Vector3Int Cell { get; private set; }
        public bool IsPlaced { get; private set; }

        void OnEnable() { CellOccupants.Add(this); Active.Add(this); }
        void OnDisable() { CellOccupants.Remove(this); Active.Remove(this); }

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
                _villagerWait = 0f;
                var asleep = place.Facing == NpcSchedule.SleepFacing;
                SetSleeping(asleep);
                SetFacing(asleep ? Vector2Int.down : NpcSchedule.FacingVector(place.Facing));
                // The picture is moved so that it lies on the pillow, at the top (north end) of the bed.
                if (Sleeping && _renderer != null)
                {
                    var bed = map.CellCenter(cell) + (Vector3)NpcHomes.SleepOffset;
                    transform.position += bed + new Vector3(0f, NpcHomes.PillowLift, 0f) - _renderer.bounds.center;
                    (_icon ??= SleepIcon.Create(transform)).Show(bed);
                }
                YieldBodyToPlayer();
                return;
            }

            SetSleeping(false);
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
            // The player stops a villager everywhere along its route, the first and last steps too; only a solid thing (a seat) is excused there.
            var villagerNear = IsPlaced && !atEnds && VillagerAt(proposed, ProbeSize);
            if (!villagerNear) _villagerWait = 0f;
            var blockedByVillager = villagerNear && _villagerWait < VillagerGiveUp;
            if (blockedByVillager) _villagerWait += Time.deltaTime;
            if (IsPlaced && (PlayerAt(proposed, ProbeSize) || (!atEnds && SolidAt(proposed, ProbeSize)) || blockedByVillager))
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

        Collider2D FindPlayerBody()
        {
            if (_playerBody == null && Time.unscaledTime >= _nextPlayerSearch)
            {
                _nextPlayerSearch = Time.unscaledTime + 0.5f;
                var player = Object.FindAnyObjectByType<PlayerController>();
                _playerBody = player != null ? player.GetComponent<Collider2D>() : null;
            }
            return _playerBody;
        }

        // Would a villager's body, `size` wide, centred on `centre` touch the player?
        bool PlayerAt(Vector3 centre, float size)
        {
            var body = FindPlayerBody();
            if (body == null) return false;
            var p = body.bounds;
            var half = size * 0.5f;
            return p.min.x < centre.x + half && p.max.x > centre.x - half && p.min.y < centre.y + half && p.max.y > centre.y - half;
        }

        // Is another villager's body in the way of a box of this size at `centre`? Only when the step brings this villager closer to it, so that
        // stepping aside (or away) is always allowed.
        bool VillagerAt(Vector3 centre, float size)
        {
            var reach = (size + ActorBody.Size) * 0.5f;
            foreach (var other in Active)
            {
                if (other == this || !other.IsPlaced) continue;
                var d = other.transform.position - centre;
                if (Mathf.Abs(d.x) >= reach || Mathf.Abs(d.y) >= reach) continue;
                var now = other.transform.position - transform.position;
                if (d.sqrMagnitude < now.sqrMagnitude - 0.0001f) return true;
            }
            return false;
        }

        // Is another villager's body within a box of this size at `centre` (used to plan a route round them)?
        bool VillagerNear(Vector3 centre, float size)
        {
            var reach = (size + ActorBody.Size) * 0.5f;
            foreach (var other in Active)
            {
                if (other == this || !other.IsPlaced) continue;
                var d = other.transform.position - centre;
                if (Mathf.Abs(d.x) < reach && Mathf.Abs(d.y) < reach) return true;
            }
            return false;
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
                (x, y) => { var c = new Vector3Int(x, y, 0); return CellOccupants.IsTaken(c, this) || SolidAt(map.CellCenter(c), 0.9f) || VillagerNear(map.CellCenter(c), 0.9f); });
            if (path == null || path.Count < 2) return;
            _points.Clear();
            _lengths.Clear();
            _points.Add(transform.position);
            // The first step is taken straight along its axis, not cut diagonally across the corner of whoever is in the way.
            var first = map.CellCenter(new Vector3Int(path[1].x, path[1].y, 0));
            var vertical = path[1].x == path[0].x;
            _points.Add(vertical ? new Vector3(transform.position.x, first.y, first.z) : new Vector3(first.x, transform.position.y, first.z));
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
            _body.enabled = !PlayerAt(transform.position, ActorBody.Size - 0.06f);
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
            if (_pose == null && !Sleeping && _renderer != null && _definition != null) _renderer.sprite = _definition.SpriteFor(facing);
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
            // Talking to someone asleep wakes them: they get up, and their reaction (grumpy, or warm to a friend) is the first thing they say.
            string reaction = null;
            if (Sleeping)
            {
                if (!NpcWake.Wake(player.Session, _definition.Id, byPlayer: true))
                {
                    player.Session.Toast(Farm.Core.L.Get("npc.asleep", Farm.Core.L.Get(_definition.NameKey)));
                    return;
                }
                reaction = NpcWake.TakeReaction(player.Session, _definition.Id);
            }
            // Turn to face the player, like someone who has been spoken to.
            var toPlayer = player.transform.position - transform.position;
            SetFacing(Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y)
                ? (toPlayer.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (toPlayer.y > 0 ? Vector2Int.up : Vector2Int.down));
            if (NpcManager.Current != null) NpcManager.Current.Hold(_definition.Id);       // stand still for a while
            NpcInteractions.Talk(player.Session, _definition, reaction);
        }
    }
}
