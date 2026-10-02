using System;

namespace Farm.Gameplay
{
    // Scales good luck down by the player's fatigue (decision AA-c). Registered by the session itself, after every
    // module's modifiers, so a tired player is less lucky whatever else is going on. Bad luck is left alone.
    public sealed class FatigueLuckModifier : ILuckModifier
    {
        readonly Func<int> _minuteOfDay;

        public FatigueLuckModifier(Func<int> minuteOfDay) { _minuteOfDay = minuteOfDay; }

        public int Order => 1000;

        public float Modify(float luck, GameState state) =>
            new FatigueState(state != null ? state.FatigueCarried : 0f).Luck(luck, _minuteOfDay());
    }
}
