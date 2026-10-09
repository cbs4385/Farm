using System;
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
        int _recheckedMinute = -1;
        bool _skip;
        EventStage _stage;
        bool _replay;                       // a memory: no effects, no clock change, nothing marked as seen
        bool _memoryStarted;
        readonly List<Background> _background = new List<Background>();
        readonly Dictionary<string, string> _expressions = new Dictionary<string, string>();   // sticky `expression` steps, per speaker

        sealed class Background { public string Actor; public bool Done; }

        public static EventDirector Current { get; private set; }
        public bool IsPlaying { get; private set; }

        public void Init(FarmMap map, GameSession session, NpcManager npcs, PlayerController player)
        {
            _map = map; _session = session; _npcs = npcs; _player = player;
            _input = ServiceLocator.Get<InputService>();
            _stage = new EventStage(map, session, npcs, player, () => _skip);
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
            if (next != null) StartCoroutine(Play(next, _session.MemoryId == next.Id));
        }

        EventDefinition PopNext()
        {
            // During a memory replay only that scene plays, and only once.
            if (_session.MemoryId != null)
            {
                if (_memoryStarted) return null;
                _memoryStarted = true;
                return _session.Story.Event(_session.MemoryId);
            }
            while (_session.PendingEvents.Count > 0)
            {
                var id = _session.PendingEvents[0];
                _session.PendingEvents.RemoveAt(0);
                var queued = _session.Story.Event(id);
                if (queued != null) return queued;
            }
            if (_checkedTriggers) return Recheck();
            _checkedTriggers = true;

            // The first scene of a new day also allows "dawn" events; remember that this day was already offered.
            var today = _session.Clock.Now.TotalDays;
            var firstOfDay = _session.GetVar("event.dawn_checked") != today + 1;
            _session.SetVar("event.dawn_checked", today + 1);
            var found = EventRunner.FindTriggered(_session, _map.MapId, firstOfDay);
            return found.Count > 0 ? found[0] : null;
        }

        // Events marked `recheck` are looked for again whenever the clock has moved on (every ten game minutes) while their map is on screen.
        EventDefinition Recheck()
        {
            var minute = _session.Clock.Now.MinuteOfDay;
            if (minute == _recheckedMinute) return null;
            _recheckedMinute = minute;
            foreach (var e in EventRunner.FindTriggered(_session, _map.MapId, false)) if (e.Recheck) return e;
            return null;
        }

        // ---- playback -------------------------------------------------------------------------------------------------

        IEnumerator Play(EventDefinition ev, bool replay = false)
        {
            IsPlaying = true;
            _skip = false;
            _replay = replay;
            if (!replay && ev.Once) _session.State.EventsSeen.Add(ev.Id);
            _input.BlockGameplay();
            if (replay || !ev.RunClock) _session.Clock.Pause();
            _player.Stop();
            if (replay) PrepareCast(ev);

            _background.Clear();
            _expressions.Clear();
            var step = 0;
            for (var guard = 0; step < ev.Steps.Count && !_skip && guard < EventFlow.MaxSteps; guard++)
            {
                var current = ev.Steps[step];
                if (EventFlow.ShouldRun(current, _session.World))
                {
                    if (current.Async) StartBackground(current);
                    else yield return Run(current);
                    step = EventFlow.Next(ev.Steps, step, _session.World);
                }
                else step++;
            }

            // A skip runs what the rest of the scene would have (following its branches) and clears the stage.
            if (_skip && !replay) EventRunner.RunSkipped(_session, ev, Mathf.Min(step, ev.Steps.Count));
            _background.Clear();
            _stage.Reset();
            _npcs.ReleaseAll();
            if (replay || !ev.RunClock) _session.Clock.Resume();
            _input.UnblockGameplay();
            IsPlaying = false;
            _replay = false;
            if (replay) Memories.Finish(_session);       // no EventFinished: reactions and quests must not hear about a replay
            else _session.Publish(new EventFinished(ev.Id, _skip));
        }

        // A memory may be replayed when a villager is elsewhere: put everyone the scene needs on stage.
        void PrepareCast(EventDefinition ev)
        {
            var index = 0;
            foreach (var id in Memories.Cast(ev))
                _npcs.Take(id, Memories.CastCell(ev, index++));
        }

        Action<string> EffectSink => _replay ? (Action<string>)(_ => { }) : (e => Effects.Run(_session, e));

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
                case "advance": if (!_replay) _session.Clock.AdvanceMinutes(step.Minutes); break;
                case "fadeout": yield return ServiceLocator.Get<SceneLoader>().FadeTo(1f, Mathf.Max(0.05f, step.Seconds > 0 ? step.Seconds : 0.4f)); break;
                case "fadein": yield return ServiceLocator.Get<SceneLoader>().FadeTo(0f, Mathf.Max(0.05f, step.Seconds > 0 ? step.Seconds : 0.4f)); break;
                case "effects": if (!_replay) Effects.RunAll(_session, step.Effects); break;
                case "emote": yield return _stage.Emote(step.Actor, step.Name, step.Seconds); break;
                case "expression": _expressions[step.Actor] = step.Name == "neutral" ? null : step.Name; break;
                case "anim": yield return _stage.Anim(step.Actor, step.Name, step.Seconds); break;
                case "camera": yield return _stage.CameraStep(step); break;
                case "sfx": _stage.Sfx(step.Name); break;
                case "music": _stage.Music(step.Name); break;
                case "lighting": yield return _stage.LightingStep(step.Name, step.Seconds); break;
                case "letterbox": yield return _stage.LetterboxStep(step.Name == "on", step.Seconds); break;
                case "spawn": _stage.Spawn(step.Id, step.Name, step.X, step.Y); break;
                case "despawn": _stage.Despawn(step.Id); break;
                case "waitFor": yield return WaitForBackground(step.Actor); break;
                case "parallel": yield return RunParallel(step); break;
                case "label": case "branch": break;          // control flow is handled by EventFlow
                default: Log.Warn($"Unknown event step '{step.Type}'."); break;
            }
        }

        // ---- background steps and parallel groups --------------------------------------------------------------------

        void StartBackground(EventStep step)
        {
            var bg = new Background { Actor = step.Actor };
            _background.Add(bg);
            StartCoroutine(RunBackground(step, bg));
        }

        IEnumerator RunBackground(EventStep step, Background bg)
        {
            yield return Run(step);
            bg.Done = true;
        }

        // Waits for the background steps of one actor (or all of them when no actor is named).
        IEnumerator WaitForBackground(string actor)
        {
            while (!_skip)
            {
                var pending = false;
                foreach (var bg in _background)
                    if (!bg.Done && (string.IsNullOrEmpty(actor) || bg.Actor == actor)) { pending = true; break; }
                if (!pending) yield break;
                PollSkip();
                yield return null;
            }
        }

        IEnumerator RunParallel(EventStep group)
        {
            var started = new List<Background>();
            foreach (var child in group.Steps)
            {
                if (!EventFlow.ShouldRun(child, _session.World)) continue;
                var bg = new Background { Actor = child.Actor };
                started.Add(bg);
                StartCoroutine(RunBackground(child, bg));
            }
            while (!_skip && started.Exists(b => !b.Done)) { PollSkip(); yield return null; }
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
            var expression = !string.IsNullOrEmpty(step.Expression) ? step.Expression
                : step.Speaker != null && _expressions.TryGetValue(step.Speaker, out var sticky) ? sticky : null;
            var graph = new DialogueGraph
            {
                Id = "event.say", Start = "n",
                Nodes = { new DialogueNode { Id = "n", Speaker = step.Speaker, Text = step.Text, Expression = expression, Emote = step.Emote } },
            };
            yield return ShowDialogue(new DialogueRunner(graph, _session.World, _session.StoryText, EffectSink));
        }

        IEnumerator Dialogue(string id)
        {
            var graph = _session.Story.Dialogue(id);
            if (graph == null) yield break;
            yield return ShowDialogue(new DialogueRunner(graph, _session.World, _session.StoryText, EffectSink));
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
