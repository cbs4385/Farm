using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // On the village map: villagers who see the player damage the green mutter about it to themselves, and one of them comes over to tell the player off (once a day).
    // The damage is remembered (VillageTrees.DamagedVar) and the villagers put the trees back overnight.
    public sealed class VillageWatch : MonoBehaviour
    {
        public const float WitnessRange = 11f;
        public const int MutterLines = 5;          // for trees; rocks and soil have MinorMutterLines
        public const int MinorMutterLines = 3;
        public const int ScoldLines = 3;
        public const string ScoldedVar = "village.scolded";          // the day (counted from 1) of the last telling-off
        const float MutterGap = 12f;
        const float WalkSpeed = 3.2f;

        GameSession _session;
        NpcManager _npcs;
        PlayerController _player;
        FarmMap _map;
        float _mutterReadyAt;
        bool _scolding;
        readonly List<GameObject> _bubbles = new List<GameObject>();

        public static int Muttered { get; private set; }
        public static int Scolds { get; private set; }
        public static string LastScolder { get; private set; }
        static VillageWatch() => TestResets.Add(() => { Muttered = 0; Scolds = 0; LastScolder = null; });

        public void Init(GameSession session, NpcManager npcs, PlayerController player, FarmMap map)
        {
            _session = session; _npcs = npcs; _player = player; _map = map;
            _session.Subscribe<VillageDamaged>(OnDamaged);
        }

        void OnDestroy()
        {
            _session?.Unsubscribe<VillageDamaged>(OnDamaged);
            foreach (var b in _bubbles) if (b != null) Destroy(b);
        }

        // Pure: the line a villager mutters, so that two witnesses never say the same one.
        public static int MutterFor(string npcId, int today, int salt = 0, VillageDamage kind = VillageDamage.Tree) =>
            (Mathf.Abs((npcId ?? "").GetHashCode()) + today * 7 + salt) % (kind == VillageDamage.Tree ? MutterLines : MinorMutterLines);
        public static int ScoldFor(string npcId, int today) => (Mathf.Abs((npcId ?? "").GetHashCode()) + today) % ScoldLines;

        public static string KindPart(VillageDamage kind) => kind == VillageDamage.Rock ? "rock." : kind == VillageDamage.Soil ? "till." : "";
        public static string MutterKey(VillageDamage kind, int line) => "village.mutter." + KindPart(kind) + line;
        public static string ScoldKey(VillageDamage kind, int variant, int part) => $"village.scold.{KindPart(kind)}{variant}.{part}";

        List<NpcActor> Witnesses()
        {
            var origin = _player.transform.position;
            return _npcs.Actors.Values
                .Where(a => a != null && a.isActiveAndEnabled && !a.Sleeping && Vector2.Distance(a.transform.position, origin) <= WitnessRange)
                .OrderBy(a => Vector2.Distance(a.transform.position, origin))
                .ToList();
        }

        void OnDamaged(VillageDamaged e)
        {
            var today = _session.Clock.Now.TotalDays;
            if (e.Cleared) _session.SetVar(VillageTrees.DamagedVar, today + 1);
            var witnesses = Witnesses();
            if (witnesses.Count == 0) return;
            if (Time.time >= _mutterReadyAt)
            {
                _mutterReadyAt = Time.time + MutterGap;
                for (var i = 0; i < Mathf.Min(2, witnesses.Count); i++)
                {
                    var w = witnesses[i];
                    var bubble = SpeechBubbleFactory.Create(w.transform, L.Get(MutterKey(e.Kind, MutterFor(w.Definition.Id, today, i, e.Kind))));
                    _bubbles.Add(bubble);
                    Destroy(bubble, Barks.ShowSeconds);
                    Muttered++;
                }
            }
            if (e.Cleared && !_scolding && _session.GetVar(ScoldedVar) != today + 1)
            {
                _session.SetVar(ScoldedVar, today + 1);
                StartCoroutine(Scold(witnesses[0], e.Kind));
            }
        }

        // The nearest witness walks over and gives the player a piece of their mind.
        IEnumerator Scold(NpcActor who, VillageDamage kind)
        {
            _scolding = true;
            yield return new WaitForSeconds(2.5f);
            var waited = 0f;
            while ((UiAccess.AnyModalOpen || (FindAnyObjectByType<EventDirector>()?.IsPlaying ?? false)) && waited < 30f) { waited += Time.deltaTime; yield return null; }
            if (who == null || _npcs == null) { _scolding = false; yield break; }

            var id = who.Definition.Id;
            var actor = _npcs.Take(id, null);
            if (actor != null)
            {
                var goal = StandBeside(_map.WorldToCell(_player.transform.position), actor.Cell);
                var path = _npcs.Grid().FindPath(actor.Cell.x, actor.Cell.y, goal.x, goal.y);
                if (path != null)
                    for (var i = 1; i < path.Count; i++)
                    {
                        var from = _map.CellCenter(new Vector3Int(path[i - 1].x, path[i - 1].y, 0));
                        var to = _map.CellCenter(new Vector3Int(path[i].x, path[i].y, 0));
                        var d = to - from;
                        actor.SetFacing(Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x > 0 ? Vector2Int.right : Vector2Int.left) : (d.y > 0 ? Vector2Int.up : Vector2Int.down));
                        for (var t = 0f; t < 1f; t += Time.deltaTime * WalkSpeed) { actor.SetWorld(Vector3.Lerp(from, to, t), _map); yield return null; }
                    }
                var toward = (Vector2)(_player.transform.position - actor.transform.position);
                actor.SetFacing(Mathf.Abs(toward.x) > Mathf.Abs(toward.y) ? (toward.x > 0 ? Vector2Int.right : Vector2Int.left) : (toward.y > 0 ? Vector2Int.up : Vector2Int.down));
            }
            LastScolder = id;
            Scolds++;
            var graph = ScoldGraph(id, ScoldFor(id, _session.Clock.Now.TotalDays), kind);
            if (!_session.BeginDialogueGraph(graph, () => { _npcs.ReleaseAll(); _scolding = false; })) { _npcs.ReleaseAll(); _scolding = false; }
        }

        // A free cell next to the player, the nearest one to where the villager is.
        Vector3Int StandBeside(Vector3Int player, Vector3Int from)
        {
            var best = player;
            var bestDistance = int.MaxValue;
            foreach (var step in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
            {
                var c = new Vector3Int(player.x + step.x, player.y + step.y, 0);
                if (!_map.IsOpenGround(c)) continue;
                var d = Mathf.Abs(c.x - from.x) + Mathf.Abs(c.y - from.y);
                if (d < bestDistance) { bestDistance = d; best = c; }
            }
            return best;
        }

        // Two lines from the villager, then it is over (nothing to choose: they have said their piece).
        public static DialogueGraph ScoldGraph(string npcId, int variant, VillageDamage kind = VillageDamage.Tree)
        {
            return new DialogueGraph
            {
                Id = "village.scold",
                Start = "n0",
                Nodes = new List<DialogueNode>
                {
                    new DialogueNode { Id = "n0", Speaker = npcId, Text = ScoldKey(kind, variant, 0), Emote = "exclaim", Next = "n1" },
                    new DialogueNode { Id = "n1", Speaker = npcId, Text = ScoldKey(kind, variant, 1) },
                },
            };
        }
    }
}
