using System.Collections;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Plays events (T-041) on the map that is loaded: walks actors, shows lines, fades, runs effects. Created by the map
    // scene controller. Gameplay input is blocked and (unless the event asks to keep the clock running) the clock is paused
    // while a scene plays; Escape skips to the end, still running the effects the scene would have run.
    public sealed class EventDirector : MonoBehaviour
    {
        const float WalkSpeed = 3.5f;       // cells per second
        const int WarmupFrames = 12;

        FarmMap _map;
        GameSession _session;
        NpcManager _npcs;
        PlayerController _player;
        InputService _input;
        int _frames;
        bool _checkedTriggers;
        bool _skip;

        public static EventDirector Current { get; private set; }
        public bool IsPlaying { get; private set; }

        public void Init(FarmMap map, GameSession session, NpcManager npcs, PlayerController player)
        {
            _map = map; _session = session; _npcs = npcs; _player = player;
            _input = ServiceLocator.Get<InputService>();
            Current = this;
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        void Update()
        {
            if (_session == null || !_session.InGame || IsPlaying) return;
            if (++_frames < WarmupFrames) return;
            if (_session.IsSleeping || _input.GameplayBlocked) return;
            if (ServiceLocator.TryGet<IUiService>(out var ui) && ui.AnyModalOpen) return;
            var loader = ServiceLocator.TryGet<SceneLoader>(out var l) ? l : null;
            if (loader != null && loader.IsLoading) return;

            var next = PopNext();
            if (next != null) StartCoroutine(Play(next));
        }

        EventDefinition PopNext()
        {
            while (_session.PendingEvents.Count > 0)
            {
                var id = _session.PendingEvents[0];
                _session.PendingEvents.RemoveAt(0);
                var queued = _session.Story.Event(id);
                if (queued != null) return queued;
            }
            if (_checkedTriggers) return null;
            _checkedTriggers = true;

            // The first scene of a new day also allows "dawn" events; remember that this day was already offered.
            var today = _session.Clock.Now.TotalDays;
            var firstOfDay = _session.GetVar("event.dawn_checked") != today + 1;
            _session.SetVar("event.dawn_checked", today + 1);
            var found = EventRunner.FindTriggered(_session, _map.MapId, firstOfDay);
            return found.Count > 0 ? found[0] : null;
        }

        // ---- playback -------------------------------------------------------------------------------------------------

        IEnumerator Play(EventDefinition ev)
        {
            IsPlaying = true;
            _skip = false;
            if (ev.Once) _session.State.EventsSeen.Add(ev.Id);
            _input.BlockGameplay();
            if (!ev.RunClock) _session.Clock.Pause();
            _player.Stop();

            var step = 0;
            for (; step < ev.Steps.Count && !_skip; step++)
                yield return Run(ev.Steps[step]);

            if (_skip) EventRunner.RunSkipped(_session, ev, step);
            _npcs.ReleaseAll();
            if (!ev.RunClock) _session.Clock.Resume();
            _input.UnblockGameplay();
            IsPlaying = false;
            _session.Publish(new EventFinished(ev.Id, _skip));
        }

        IEnumerator Run(EventStep step)
        {
            switch (step.Type)
            {
                case "say": yield return Say(step); break;
                case "dialogue": yield return Dialogue(step.Dialogue); break;
                case "move": yield return Move(step.Actor, step.X, step.Y); break;
                case "place": Place(step.Actor, step.X, step.Y); break;
                case "face": Face(step.Actor, NpcSchedule.FacingVector(step.Facing)); break;
                case "wait": yield return Wait(step.Seconds); break;
                case "advance": _session.Clock.AdvanceMinutes(step.Minutes); break;
                case "fadeout": yield return ServiceLocator.Get<SceneLoader>().FadeTo(1f, Mathf.Max(0.05f, step.Seconds > 0 ? step.Seconds : 0.4f)); break;
                case "fadein": yield return ServiceLocator.Get<SceneLoader>().FadeTo(0f, Mathf.Max(0.05f, step.Seconds > 0 ? step.Seconds : 0.4f)); break;
                case "effects": Effects.RunAll(_session, step.Effects); break;
                default: Log.Warn($"Unknown event step '{step.Type}'."); break;
            }
        }

        // Escape (or the gamepad's back button) skips the rest of the scene.
        void PollSkip()
        {
            if (_input.Ui[InputNames.Cancel].WasPressedThisFrame()) _skip = true;
        }

        IEnumerator Wait(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end && !_skip) { PollSkip(); yield return null; }
        }

        IEnumerator Say(EventStep step)
        {
            var graph = new DialogueGraph
            {
                Id = "event.say", Start = "n",
                Nodes = { new DialogueNode { Id = "n", Speaker = step.Speaker, Text = step.Text } },
            };
            yield return ShowDialogue(new DialogueRunner(graph, _session.World, _session.StoryText, e => Effects.Run(_session, e)));
        }

        IEnumerator Dialogue(string id)
        {
            var graph = _session.Story.Dialogue(id);
            if (graph == null) yield break;
            yield return ShowDialogue(new DialogueRunner(graph, _session.World, _session.StoryText, e => Effects.Run(_session, e)));
        }

        IEnumerator ShowDialogue(DialogueRunner runner)
        {
            if (!ServiceLocator.TryGet<IUiService>(out var ui)) yield break;
            var done = false;
            ui.ShowDialogue(runner, () => done = true);
            while (!done) yield return null;
            // Escape closed the lines; a skip keeps the scene going only to its end.
        }

        // ---- actors -----------------------------------------------------------------------------------------------------

        void Place(string actor, int x, int y)
        {
            var cell = new Vector3Int(x, y, 0);
            if (actor == "player") { _player.Teleport(_map.CellCenter(cell)); return; }
            var npc = _npcs.Take(actor, cell);
            if (npc != null) npc.SetCell(_map, cell);
        }

        void Face(string actor, Vector2Int facing)
        {
            if (actor == "player") { _player.Face(facing); return; }
            _npcs.Take(actor, null)?.SetFacing(facing);
        }

        IEnumerator Move(string actor, int x, int y)
        {
            var goal = new Vector3Int(x, y, 0);
            Vector3Int start;
            NpcActor npc = null;
            if (actor == "player") start = _map.WorldToCell(_player.transform.position);
            else
            {
                npc = _npcs.Take(actor, null);
                if (npc == null) { Place(actor, x, y); yield break; }
                start = npc.Cell;
            }

            var path = _npcs.Grid().FindPath(start.x, start.y, goal.x, goal.y);
            if (path == null) path = new List<(int x, int y)> { (start.x, start.y), (goal.x, goal.y) };

            for (var i = 1; i < path.Count && !_skip; i++)
            {
                var from = _map.CellCenter(new Vector3Int(path[i - 1].x, path[i - 1].y, 0));
                var to = _map.CellCenter(new Vector3Int(path[i].x, path[i].y, 0));
                var facing = Mathf.Abs(to.x - from.x) > Mathf.Abs(to.y - from.y)
                    ? (to.x > from.x ? Vector2Int.right : Vector2Int.left)
                    : (to.y > from.y ? Vector2Int.up : Vector2Int.down);
                if (actor == "player") _player.Face(facing); else npc.SetFacing(facing);

                for (var t = 0f; t < 1f && !_skip; t += Time.deltaTime * WalkSpeed)
                {
                    PollSkip();
                    var at = Vector3.Lerp(from, to, t);
                    if (actor == "player") _player.Teleport(at); else npc.SetWorld(at, _map);
                    yield return null;
                }
            }
            var end = _map.CellCenter(goal);
            if (actor == "player") _player.Teleport(end); else npc.SetWorld(end, _map);
        }
    }
}
