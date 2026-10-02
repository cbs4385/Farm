using Farm.Core;

namespace Farm.Gameplay
{
    // IWorldQuery over a GameState + clock, used by conditions during the day cycle, in the session, and in tests.
    public sealed class StateWorldQuery : IWorldQuery, IGameQuery
    {
        readonly GameState _state;
        readonly GameClock _clock;

        // The live backpack count (GameState.Backpack is only refreshed when saving); without it `has:` sees nothing.
        public System.Func<string, int> ItemCounter;

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

        public int Hearts(string npcId) => _state.Npcs.TryGetValue(npcId, out var n) ? FriendshipModel.Hearts(n.Points) : 0;
        public int ItemCount(string itemId) => ItemCounter != null ? ItemCounter(itemId) : 0;
        public string QuestState(string questId) => _state.Quests.TryGetValue(questId, out var q) ? q.Status : "new";
        public bool KnowsRecipe(string recipeId) => _state.Recipes.Contains(recipeId);
        public bool MerchantHere() => Merchant.IsHere(_state.WorldSeed, Now.TotalDays);
    }
}
