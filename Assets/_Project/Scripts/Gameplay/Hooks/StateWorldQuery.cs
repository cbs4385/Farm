using Farm.Core;

namespace Farm.Gameplay
{
    // IWorldQuery over a GameState + clock, used by conditions during the day cycle, in the session, and in tests.
    public sealed class StateWorldQuery : IWorldQuery
    {
        readonly GameState _state;
        readonly GameClock _clock;

        public StateWorldQuery(GameState state, GameClock clock)
        {
            _state = state;
            _clock = clock;
        }

        public bool HasFlag(string flag) => _state.Flags.Contains(flag);
        public int GetVar(string name) => _state.Vars.TryGetValue(name, out var v) ? v : 0;
        public GameDateTime Now => _clock != null ? _clock.Now : _state.GetDate();
        public string Weather => _state.Weather;
        public string MapId => _state.CurrentMap;
    }
}
