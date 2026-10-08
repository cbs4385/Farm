using System.Collections;
using System.Collections.Generic;
using System.IO;
using Farm.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Gameplay
{
    // T-146: photo mode. F8 hides the HUD and stops the clock; 1 to 6 put an emote over the nearest villager, Tab picks the next villager,
    // Enter or F8 saves a PNG into <data>/Photos, Esc leaves. Created on every map.
    public sealed class PhotoMode : MonoBehaviour
    {
        static readonly string[] Emotes = { "heart", "note", "sparkle", "exclaim", "question", "ellipsis" };
        const float Reach = 6f;

        GameSession _session;
        NpcManager _npcs;
        PlayerController _player;
        bool _on, _shooting;
        int _target, _emote = -1;
        GameObject _bubble;
        string _notice;
        float _noticeUntil;

        public static bool Active { get; private set; }
        public static string LastPhotoFile { get; private set; }
        public static void ResetForTests() { Active = false; LastPhotoFile = null; }

        public void Init(GameSession session, NpcManager npcs, PlayerController player)
        {
            _session = session; _npcs = npcs; _player = player;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            if ((keyboard == null && pad == null) || _session == null || !_session.InGame) return;
            bool Key_(Key key) => keyboard != null && keyboard[key].wasPressedThisFrame;
            if (!_on)
            {
                // F8, or the left stick pressed in on a pad.
                if ((Key_(Key.F8) || (pad != null && pad.leftStickButton.wasPressedThisFrame)) && !AnyModal()) Enter();
                return;
            }
            if (_shooting) return;
            // On a pad: B leaves, A takes the photo, Y picks the next villager, X clears the bubble, LB and RB step through the emotes.
            if (Key_(Key.Escape) || (pad != null && pad.buttonEast.wasPressedThisFrame)) { Leave(); return; }
            if (Key_(Key.F8) || Key_(Key.Enter) || (pad != null && pad.buttonSouth.wasPressedThisFrame)) { StartCoroutine(Shoot()); return; }
            if (Key_(Key.Tab) || (pad != null && pad.buttonNorth.wasPressedThisFrame)) { _target++; ClearBubble(); }
            for (var i = 0; i < Emotes.Length; i++)
                if (Key_(Key.Digit1 + i)) Pose(Emotes[i]);
            if (pad != null)
            {
                if (pad.rightShoulder.wasPressedThisFrame) { _emote = (_emote + 1) % Emotes.Length; Pose(Emotes[_emote]); }
                if (pad.leftShoulder.wasPressedThisFrame) { _emote = (_emote + Emotes.Length - 1) % Emotes.Length; Pose(Emotes[_emote]); }
            }
            if (Key_(Key.Digit0) || (pad != null && pad.buttonWest.wasPressedThisFrame)) ClearBubble();
        }

        static bool AnyModal() => ServiceLocator.TryGet<IUiService>(out var ui) && ui.AnyModalOpen;

        void Enter()
        {
            _on = true; Active = true;
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(false);
            _session.Clock.Pause();
            if (_player != null) _player.enabled = false;
        }

        void Leave()
        {
            ClearBubble();
            _on = false; Active = false;
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(true);
            _session.Clock.Resume();
            if (_player != null) _player.enabled = true;
        }

        void OnDestroy()
        {
            if (_on) { Active = false; if (_session != null) _session.Clock.Resume(); if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(true); }
        }

        // Villagers within reach of the player, nearest first.
        List<Transform> Nearby()
        {
            var list = new List<Transform>();
            if (_npcs == null || _player == null) return list;
            foreach (var actor in _npcs.Actors.Values)
                if (actor != null && Vector2.Distance(actor.transform.position, _player.transform.position) <= Reach) list.Add(actor.transform);
            list.Sort((a, b) => Vector2.Distance(a.position, _player.transform.position).CompareTo(Vector2.Distance(b.position, _player.transform.position)));
            return list;
        }

        void Pose(string emote)
        {
            var near = Nearby();
            if (near.Count == 0) return;
            ClearBubble();
            _bubble = EmoteBubbleFactory.Create(near[_target % near.Count], DialogueVocabulary.EmoteGlyph(emote));
        }

        void ClearBubble()
        {
            if (_bubble != null) Destroy(_bubble);
            _bubble = null;
        }

        // The picture is the world as the main camera sees it (lighting and weather included), rendered into a texture: no screen overlay,
        // no hint and no HUD can end up in it, and it does not depend on the end of the frame.
        IEnumerator Shoot()
        {
            _shooting = true;
            yield return null;
            AudioService.PlayIfAvailable(Sfx.Shutter);
            var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam == null) { _shooting = false; yield break; }
            var width = Mathf.Max(64, Screen.width);
            var height = Mathf.Max(64, Screen.height);
            var rt = RenderTexture.GetTemporary(width, height, 24);
            var previous = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = previous;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var active = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(rt);

            var dir = Path.Combine(GameServices.DataRootPath, "Photos");
            Directory.CreateDirectory(dir);
            var name = $"farm_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
            Destroy(tex);
            LastPhotoFile = name;
            _notice = L.Get("photo.saved", name);
            _noticeUntil = Time.unscaledTime + 3f;
            _shooting = false;
        }

        // The hint is drawn with IMGUI: it exists only while photo mode is on and is never in the picture (it is skipped while shooting).
        void OnGUI()
        {
            if (!_on || _shooting) return;
            GUI.Label(new Rect(10, 10, Screen.width - 20, 24), L.Get("photo.hint"));
            if (Time.unscaledTime < _noticeUntil) GUI.Label(new Rect(10, 34, Screen.width - 20, 24), _notice);
        }
    }
}
