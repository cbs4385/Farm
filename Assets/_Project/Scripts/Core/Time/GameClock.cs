using System;

namespace Farm.Core
{
    public readonly struct MinuteChanged { public readonly GameDateTime Now; public MinuteChanged(GameDateTime now) { Now = now; } }
    public readonly struct PassOutTimeReached { public readonly GameDateTime Now; public PassOutTimeReached(GameDateTime now) { Now = now; } }
    public readonly struct DayEnded
    {
        public readonly GameDateTime Date; public readonly bool PassedOut;
        public DayEnded(GameDateTime date, bool passedOut) { Date = date; PassedOut = passedOut; }
    }
    public readonly struct DayStarted { public readonly GameDateTime Date; public DayStarted(GameDateTime date) { Date = date; } }
    public readonly struct SeasonChanged
    {
        public readonly Season Season; public readonly int Year;
        public SeasonChanged(Season season, int year) { Season = season; Year = year; }
    }

    // Owns the in-game time. Plain C#: the game drives it with Tick(deltaTime) so it is fully unit-testable.
    // At 06:00 the next morning the clock stops and publishes PassOutTimeReached; whoever handles that calls StartNextDay(true).
    public sealed class GameClock
    {
        public const int MinutesPerStep = 10;
        public const float DefaultSecondsPerStep = 7f;

        readonly EventBus _bus;
        GameDateTime _now;
        float _accumulator;
        int _pauseCount;
        bool _passOutPublished;

        public GameClock(GameDateTime start, EventBus bus = null)
        {
            _now = start;
            _bus = bus;
            _passOutPublished = start.IsDayOver;
        }

        public GameDateTime Now => _now;
        public bool IsPaused => _pauseCount > 0;
        public float SecondsPerStep { get; set; } = DefaultSecondsPerStep;

        // Ref-counted: every Pause() must be matched by a Resume() (menus, dialogue, cutscenes).
        public void Pause() => _pauseCount++;

        public void Resume()
        {
            if (_pauseCount == 0) throw new InvalidOperationException("Resume() without matching Pause().");
            _pauseCount--;
        }

        public void Tick(float deltaSeconds)
        {
            if (IsPaused || _now.IsDayOver) return;
            _accumulator += deltaSeconds;
            while (_accumulator >= SecondsPerStep && !_now.IsDayOver)
            {
                _accumulator -= SecondsPerStep;
                AdvanceMinutes(MinutesPerStep);
            }
        }

        public void AdvanceMinutes(int minutes)
        {
            if (minutes <= 0 || _now.IsDayOver) return;
            _now = _now.WithMinuteOfDay(_now.MinuteOfDay + minutes);
            _bus?.Publish(new MinuteChanged(_now));
            if (_now.IsDayOver && !_passOutPublished)
            {
                _passOutPublished = true;
                _bus?.Publish(new PassOutTimeReached(_now));
            }
        }

        public void StartNextDay(bool passedOut)
        {
            var ended = _now;
            _bus?.Publish(new DayEnded(ended, passedOut));

            _now = ended.StartOfNextDay();
            _accumulator = 0f;
            _passOutPublished = false;

            if (_now.Season != ended.Season) _bus?.Publish(new SeasonChanged(_now.Season, _now.Year));
            _bus?.Publish(new DayStarted(_now));
        }

        // Used when loading a save. Publishes nothing.
        public void SetTime(GameDateTime time)
        {
            _now = time;
            _accumulator = 0f;
            _passOutPublished = time.IsDayOver;
        }
    }
}
