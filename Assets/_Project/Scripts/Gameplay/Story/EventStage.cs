using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The things a scene can do to the world besides moving people and showing lines (T-100): emote bubbles, procedural
    // gestures, camera focus and shake, lighting moods, props, letterbox bars, sounds and music cues. Owned by the
    // EventDirector; everything it changes is undone by Reset when the scene ends (or is skipped).
    public sealed class EventStage
    {
        readonly FarmMap _map;
        readonly GameSession _session;
        readonly NpcManager _npcs;
        readonly PlayerController _player;
        readonly Func<bool> _skipping;
        readonly Dictionary<string, GameObject> _props = new Dictionary<string, GameObject>();
        readonly List<GameObject> _bubbles = new List<GameObject>();
        readonly List<(Transform t, Vector3 home)> _gesturing = new List<(Transform, Vector3)>();
        CameraFollow _camera;
        DayNightLighting _lighting;
        bool _letterbox;

        public EventStage(FarmMap map, GameSession session, NpcManager npcs, PlayerController player, Func<bool> skipping)
        {
            _map = map; _session = session; _npcs = npcs; _player = player; _skipping = skipping;
        }

        CameraFollow Camera => _camera != null ? _camera : (_camera = UnityEngine.Object.FindAnyObjectByType<CameraFollow>());
        DayNightLighting Lighting => _lighting != null ? _lighting : (_lighting = UnityEngine.Object.FindAnyObjectByType<DayNightLighting>());

        public Transform ActorTransform(string actor)
        {
            if (string.IsNullOrEmpty(actor)) return null;
            if (actor == "player") return _player != null ? _player.transform : null;
            var npc = _npcs != null ? _npcs.Take(actor, null) : null;
            return npc != null ? npc.transform : null;
        }

        IEnumerator Wait(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end && !_skipping()) yield return null;
        }

        // ---- emotes and gestures ------------------------------------------------------------------------------------

        public IEnumerator Emote(string actor, string emote, float seconds)
        {
            var target = ActorTransform(actor);
            if (target == null) { Log.Warn($"Emote: no actor '{actor}' here."); yield break; }
            var bubble = EmoteBubbleFactory.Create(target, DialogueVocabulary.EmoteGlyph(emote));
            _bubbles.Add(bubble);
            yield return Wait(seconds > 0f ? seconds : 1.4f);
            if (bubble != null) UnityEngine.Object.Destroy(bubble);
            _bubbles.Remove(bubble);
        }

        // Procedural gestures until animation frames exist (T-131): they move the actor a little and put it back.
        public IEnumerator Anim(string actor, string name, float seconds)
        {
            var target = ActorTransform(actor);
            if (target == null) { Log.Warn($"Anim: no actor '{actor}' here."); yield break; }
            var duration = seconds > 0f ? seconds : 0.8f;
            var home = target.position;
            _gesturing.Add((target, home));
            for (var t = 0f; t < duration && !_skipping(); t += Time.deltaTime)
            {
                var p = t / duration;
                target.position = home + GestureOffset(name, p);
                yield return null;
            }
            if (target != null) target.position = home;
            _gesturing.RemoveAll(g => g.t == target);
        }

        // The offset (in world units) of a gesture at progress 0..1.
        public static Vector3 GestureOffset(string name, float p)
        {
            switch (name)
            {
                case "hop": return Vector3.up * Mathf.Abs(Mathf.Sin(p * Mathf.PI * 2f)) * 0.25f;
                case "jiggle": return new Vector3(Mathf.Sin(p * Mathf.PI * 14f) * 0.04f, Mathf.Abs(Mathf.Sin(p * Mathf.PI * 7f)) * 0.06f, 0f);
                case "nod": return Vector3.down * Mathf.Sin(p * Mathf.PI * 2f) * 0.07f;
                case "sway": return new Vector3(Mathf.Sin(p * Mathf.PI * 2f) * 0.08f, 0f, 0f);
                case "dance": return new Vector3(Mathf.Sin(p * Mathf.PI * 6f) * 0.1f, Mathf.Abs(Mathf.Sin(p * Mathf.PI * 6f)) * 0.14f, 0f);
                default: return Vector3.zero;    // look: the caller turns the actor instead
            }
        }

        // ---- camera --------------------------------------------------------------------------------------------------

        public IEnumerator CameraStep(EventStep step)
        {
            var cam = Camera;
            if (cam == null) yield break;
            var blend = step.Seconds > 0f ? step.Seconds : 0.8f;
            switch (step.Name)
            {
                case "focus":
                {
                    var target = ActorTransform(step.Actor);
                    if (target != null) cam.FocusOn(target, blend);
                    else cam.FocusOn(_map.CellCenter(new Vector3Int(step.X, step.Y, 0)), blend);
                    yield return Wait(blend);
                    break;
                }
                case "pan":
                {
                    var target = ActorTransform(step.Actor);
                    cam.FocusOn(target != null ? target.position : _map.CellCenter(new Vector3Int(step.X, step.Y, 0)), blend);
                    yield return Wait(blend);
                    break;
                }
                case "shake":
                {
                    var seconds = step.Seconds > 0f ? step.Seconds : 0.5f;
                    cam.Shake(step.Value > 0f ? step.Value : 0.12f, seconds);
                    yield return Wait(seconds);
                    break;
                }
                case "reset":
                    cam.ReleaseFocus(blend);
                    yield return Wait(blend);
                    break;
            }
        }

        // ---- light, props, bars, sound ------------------------------------------------------------------------------------

        public IEnumerator LightingStep(string preset, float seconds)
        {
            var light = Lighting;
            if (light == null) yield break;
            var fade = seconds > 0f ? seconds : 1f;
            if (preset == "reset") light.ClearOverride(fade); else light.SetOverride(DayNightLighting.PresetColor(preset), fade);
            yield return Wait(fade);
        }

        public IEnumerator LetterboxStep(bool on, float seconds)
        {
            _letterbox = on;
            var fade = seconds > 0f ? seconds : 0.5f;
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetLetterbox(on, fade);
            yield return Wait(fade);
        }

        public void Spawn(string id, string itemId, int x, int y)
        {
            Despawn(id);
            var go = new GameObject("Prop_" + id, typeof(SpriteRenderer));
            var renderer = go.GetComponent<SpriteRenderer>();
            if (_session.Db.TryGetItem(itemId, out var item)) renderer.sprite = item.Icon;
            renderer.sortingOrder = 150;
            go.transform.position = _map.CellCenter(new Vector3Int(x, y, 0)) + Vector3.up * 0.2f;
            _props[id] = go;
        }

        public void Despawn(string id)
        {
            if (id != null && _props.TryGetValue(id, out var go))
            {
                if (go != null) UnityEngine.Object.Destroy(go);
                _props.Remove(id);
            }
        }

        public bool HasProp(string id) => id != null && _props.ContainsKey(id);

        public void Sfx(string name)
        {
            if (Enum.TryParse<Farm.Gameplay.Sfx>(name, true, out var sfx)) AudioService.PlayIfAvailable(sfx);
        }

        public void Music(string cue) => _session.Publish(new MusicCue(cue));

        // Puts everything back: camera on the player, normal light, no bars, no props, no bubbles, gestures undone.
        public void Reset()
        {
            Camera?.ReleaseFocus(0f);
            Lighting?.ClearOverride(0f);
            if (_letterbox && ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetLetterbox(false, 0f);
            _letterbox = false;
            foreach (var go in _props.Values) if (go != null) UnityEngine.Object.Destroy(go);
            _props.Clear();
            foreach (var b in _bubbles) if (b != null) UnityEngine.Object.Destroy(b);
            _bubbles.Clear();
            foreach (var (t, home) in _gesturing) if (t != null) t.position = home;
            _gesturing.Clear();
        }
    }
}
