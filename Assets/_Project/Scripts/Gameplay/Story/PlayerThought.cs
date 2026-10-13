using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Once the first seeds are in and watered there is nothing to do on the farm for days (playtest 2026-10-13: "what am I supposed to do for the next three days?").
    // The farmer thinks it aloud, in a bubble over their head, once: go and look around the village.
    public sealed class PlayerThought : MonoBehaviour
    {
        public const string SeenFlag = "tutorial.explore_thought";
        public const float ShownSeconds = 9f;

        GameSession _session;
        Transform _player;
        float _check;

        public void Init(GameSession session, Transform player) { _session = session; _player = player; }

        // The first crops are planted and watered, and the thought has not been had yet. (pure given the session)
        public static bool Wanted(GameSession s)
        {
            if (s == null || !s.InGame || s.HasFlag(SeenFlag) || s.HasFlag(TutorialCoach.SkipFlag)) return false;
            if (s.Settings != null && !s.Settings.TutorialTips) return false;
            return s.GetVar(QuestLog.Stats.Planted) >= 3 && s.GetVar(QuestLog.Stats.Watered) >= 3;
        }

        void Update()
        {
            _check -= Time.unscaledDeltaTime;
            if (_check > 0f || _session == null || _player == null) return;
            _check = 1f;
            var ui = _session.Ui;
            if (ui != null && ui.AnyModalOpen) return;
            if (!Wanted(_session)) return;
            _session.SetFlag(SeenFlag);
            var bubble = SpeechBubbleFactory.Create(_player, L.Get("thought.explore"), _session.Settings?.TextScale ?? 1f);
            Destroy(bubble, ShownSeconds);
        }
    }
}
