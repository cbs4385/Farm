using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Created on every map: now and then a villager near the player says a short line in a bubble (T-125). Quiet while any screen or
    // scene is up, off when the player turns Barks off, and rate-limited (see Barks.Cooldowns).
    public sealed class BarkDirector : MonoBehaviour
    {
        const float CheckEvery = 0.5f;

        GameSession _session;
        NpcManager _npcs;
        PlayerController _player;
        EventDirector _events;
        readonly Barks.Cooldowns _cooldowns = new Barks.Cooldowns();
        float _nextCheck;
        int _counter;
        GameObject _bubble;
        float _hideAt;

        // Test hooks: the last bark spoken.
        public static string LastNpc { get; private set; }
        public static string LastText { get; private set; }
        public static int Spoken { get; private set; }

        static BarkDirector() => TestResets.Add(ResetForTests);
        public static void ResetForTests() { LastNpc = null; LastText = null; Spoken = 0; _requested = null; }

        // QA and the developer console: the villager speaks as soon as they are on this map, ignoring range, gaps and the Barks option.
        static string _requested;
        public static void Request(string npcId) => _requested = npcId;

        public void Init(GameSession session, NpcManager npcs, PlayerController player, EventDirector events)
        {
            _session = session; _npcs = npcs; _player = player; _events = events;
            _nextCheck = Time.time + 5f;       // a short quiet after arriving
        }

        void Update()
        {
            if (_bubble != null && Time.time >= _hideAt) { Destroy(_bubble); _bubble = null; }
            if (_requested != null && _npcs != null && _npcs.Actors.TryGetValue(_requested, out var forced) && forced != null)
            {
                var id = Barks.Pick(_session, _requested, _counter++);
                var text = id == null ? null : Barks.TextOf(_session, id);
                if (_bubble != null) Destroy(_bubble);
                if (!string.IsNullOrEmpty(text)) Say(_requested, forced, text);
                _requested = null;
                return;
            }
            if (Time.time < _nextCheck || _session == null || !_session.InGame) return;
            _nextCheck = Time.time + CheckEvery;
            if (_bubble != null || !Enabled()) return;
            if (Random.value > Barks.ChancePerCheck) return;
            TrySpeak();
        }

        bool Enabled()
        {
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            if (settings != null && !settings.Barks) return false;
            if (_events != null && _events.IsPlaying) return false;
            return !(UiAccess.AnyModalOpen);
        }

        void TrySpeak()
        {
            if (_player == null || _npcs == null) return;
            var origin = _player.transform.position;
            foreach (var pair in _npcs.Actors)
            {
                var actor = pair.Value;
                if (actor == null || !_cooldowns.Ready(pair.Key, Time.time)) continue;
                var p = actor.transform.position;
                if (!Barks.InRange(p.x - origin.x, p.y - origin.y)) continue;
                var id = Barks.Pick(_session, pair.Key, _counter++);
                var text = id == null ? null : Barks.TextOf(_session, id);
                if (string.IsNullOrEmpty(text)) continue;
                Say(pair.Key, actor, text);
                return;
            }
        }

        void Say(string npcId, NpcActor actor, string text)
        {
            _cooldowns.Mark(npcId, Time.time);
            _bubble = SpeechBubbleFactory.Create(actor.transform, text);
            _hideAt = Time.time + Barks.ShowSeconds;
            LastNpc = npcId; LastText = text; Spoken++;
        }

        void OnDestroy() { if (_bubble != null) Destroy(_bubble); }
    }
}
