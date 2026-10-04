using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // The pure parts of the dialogue box (T-096): how fast text types out, the {pause}/{speed} cues, auto-advance timing
    // and the backlog. The screen only draws what these decide, so they are tested without a scene.
    public static class DialoguePacing
    {
        // The player's text speed setting: 0 slow, 1 normal, 2 fast, 3 instant.
        public static readonly float[] CharactersPerSecond = { 45f, 90f, 180f, float.PositiveInfinity };
        public const int DefaultSpeed = 1, InstantSpeed = 3;

        public static float CpsFor(int speedSetting) =>
            CharactersPerSecond[Math.Max(0, Math.Min(CharactersPerSecond.Length - 1, speedSetting))];

        // How long a finished line stays before auto-advance moves on: a base read time plus time per character.
        public static float AutoAdvanceSeconds(int characters) => 1.5f + Math.Min(characters, 200) * 0.03f;
    }

    // Reveals a line character by character, honouring cues. Advance(dt) every frame; Visible is the number of
    // characters to show (TextMeshPro maxVisibleCharacters).
    public sealed class Typewriter
    {
        readonly int _total;
        readonly List<TextCue> _cues;
        readonly float _baseCps;
        float _revealed;
        float _speed = 1f;
        float _pauseLeft;
        int _nextCue;

        public Typewriter(int totalCharacters, IReadOnlyList<TextCue> cues, float baseCps)
        {
            _total = Math.Max(0, totalCharacters);
            _cues = new List<TextCue>(cues ?? new TextCue[0]);
            _cues.Sort((a, b) => a.Index.CompareTo(b.Index));
            _baseCps = baseCps;
            if (float.IsPositiveInfinity(baseCps)) Finish();
        }

        public int Visible { get; private set; }
        public bool Done => Visible >= _total;
        public bool Pausing => _pauseLeft > 0f;

        public void Finish()
        {
            Visible = _total;
            _revealed = _total;
            _pauseLeft = 0f;
            _nextCue = _cues.Count;
        }

        public void Advance(float dt)
        {
            var guard = 0;
            while (dt > 0f && !Done && guard++ < 10000)
            {
                if (_pauseLeft > 0f)
                {
                    var used = Math.Min(_pauseLeft, dt);
                    _pauseLeft -= used;
                    dt -= used;
                    continue;
                }
                ApplyDueCues();
                if (_pauseLeft > 0f) continue;

                var nextStop = _nextCue < _cues.Count ? Math.Min(_cues[_nextCue].Index, _total) : _total;
                var cps = _baseCps * _speed;
                var need = nextStop - _revealed;
                var time = need / cps;
                if (time > dt) { _revealed += dt * cps; dt = 0f; }
                else { _revealed = nextStop; dt -= time; }
                Visible = Math.Min(_total, (int)_revealed);
            }
            if (!Done) ApplyDueCues();      // a cue at the character just reached starts now, so Pausing is accurate this frame
        }

        // Cues whose character has been reached take effect: a speed change applies, a pause is added up.
        void ApplyDueCues()
        {
            var position = (int)_revealed;
            while (_nextCue < _cues.Count && _cues[_nextCue].Index <= position)
            {
                var cue = _cues[_nextCue++];
                if (cue.Speed > 0f) _speed = cue.Speed;
                if (cue.Pause > 0f) _pauseLeft += cue.Pause;
            }
        }
    }

    // The optional timer on choices, for a chat that votes (T-145). Start it when the choices appear, Tick it every frame; when it
    // fires the dialogue box takes the default choice (or the first).
    public sealed class ChoiceCountdown
    {
        float _left;

        public bool Running { get; private set; }
        public float Remaining => Running ? _left : 0f;
        public int SecondsLeft => Running ? (int)System.Math.Ceiling(_left) : 0;

        public void Start(float seconds)
        {
            Running = seconds > 0f;
            _left = seconds;
        }

        public void Stop() { Running = false; _left = 0f; }

        // True on the frame the time runs out (once).
        public bool Tick(float dt)
        {
            if (!Running) return false;
            _left -= dt;
            if (_left > 0f) return false;
            Stop();
            return true;
        }

        // The option the timer picks: the default one if there is one, otherwise the first.
        public static int Choose(IReadOnlyList<DialogueOption> options)
        {
            if (options == null || options.Count == 0) return -1;
            for (var i = 0; i < options.Count; i++) if (options[i].IsDefault) return i;
            return 0;
        }
    }

    // The last lines of a conversation, newest last (the backlog the player can open).
    public sealed class DialogueBacklog
    {
        public const int Capacity = 30;

        public readonly struct Entry
        {
            public readonly string Speaker, Text;
            public Entry(string speaker, string text) { Speaker = speaker; Text = text; }
        }

        readonly List<Entry> _entries = new List<Entry>();
        public IReadOnlyList<Entry> Entries => _entries;

        public void Add(string speaker, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (_entries.Count > 0 && _entries[_entries.Count - 1].Text == text && _entries[_entries.Count - 1].Speaker == speaker) return;
            _entries.Add(new Entry(speaker ?? string.Empty, text));
            if (_entries.Count > Capacity) _entries.RemoveAt(0);
        }

        public void Clear() => _entries.Clear();
    }
}
