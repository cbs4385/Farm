using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The villagers at the festival (village map only, created with the map): on Blossom Day the ones by the maypole dance in time with the music, at the Tidewatch the ones on
    // the shore cast their lines, at the Harvest Fair and the Lantern Feast they cheer and wave now and then. Their positions come from the schedule (FestivalSchedule); this
    // only gives them something to do while they stand there, so it is all animation and nothing that has to be saved.
    public sealed class FestivalGrounds : MonoBehaviour
    {
        public static readonly Vector2Int Maypole = new Vector2Int(29, 17);          // east of the village square's gazebo
        public const float DanceRadius = 6.5f;
        public const float TurnSeconds = 1.1f;

        GameSession _session;
        NpcManager _npcs;
        FarmMap _map;
        readonly Dictionary<string, float> _next = new Dictionary<string, float>();
        readonly Dictionary<string, float> _clear = new Dictionary<string, float>();
        readonly List<NpcActor> _dancers = new List<NpcActor>();

        public void Init(MapContext context)
        {
            _session = context.Session;
            _npcs = context.Npcs;
            _map = context.Map;
        }

        // The cells the villagers dance on: on the road round the maypole. (pure)
        public static bool InDanceRing(Vector3 position, Vector3 maypole) => Vector2.Distance(position, maypole) <= DanceRadius;

        void Update()
        {
            if (_session == null || !_session.InGame || _npcs == null) return;
            var now = _session.Clock.Now;
            var row = Festivals.Today(now);
            var open = row != null && Festivals.IsOpen(now) && !(_session.Ui?.AnyModalOpen ?? false);
            foreach (var actor in _npcs.Actors.Values)
            {
                if (actor == null) continue;
                var id = actor.Definition.Id;
                var still = open && !actor.IsWalking && !actor.Sleeping && !actor.Sitting;
                switch (open ? row.Kind : (FestivalKind?)null)
                {
                    case FestivalKind.Dance:
                        var dance = still && InDanceRing(actor.transform.position, _map.CellCenter(new Vector3Int(Maypole.x, Maypole.y, 0)));
                        if (dance != actor.Dancing) actor.SetDancing(dance);
                        break;
                    case FestivalKind.FishingContest:
                        Do(actor, id, still, 5f, 12f, () => actor.Strike(ToolType.Rod));
                        break;
                    case FestivalKind.CropShow:
                    case FestivalKind.Lanterns:
                        Do(actor, id, still, 6f, 16f, () => Cheer(actor));
                        break;
                    default:
                        if (actor.Dancing) actor.SetDancing(false);
                        break;
                }
                if (_clear.TryGetValue(id, out var until) && Time.time >= until) { _clear.Remove(id); actor.ClearPose(); }
            }
        }

        // Turning to the four sides in turn, each dancer a beat apart from the next. (pure)
        public static Vector2Int DanceFacing(string npcId, float time = -1f)
        {
            if (time < 0f) time = Time.time;
            var offset = Mathf.Abs(npcId.GetHashCode()) % 4;
            switch (((int)(time / TurnSeconds) + offset) % 4)
            {
                case 1: return Vector2Int.left;
                case 3: return Vector2Int.right;
                default: return Vector2Int.down;
            }
        }

        void Do(NpcActor actor, string id, bool still, float min, float max, System.Action act)
        {
            if (!still) return;
            if (!_next.TryGetValue(id, out var at)) { _next[id] = Time.time + Random.Range(min * 0.3f, max); return; }
            if (Time.time < at) return;
            _next[id] = Time.time + Random.Range(min, max);
            act();
        }

        void Cheer(NpcActor actor)
        {
            if (!actor.ShowPose("wave")) return;
            _clear[actor.Definition.Id] = Time.time + 1.6f;
        }
    }
}
