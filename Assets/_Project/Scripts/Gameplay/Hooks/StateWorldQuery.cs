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

        // Narrative atoms (T-092). Each is optional: without one the atom sees nothing (-1, 0, false, empty).
        public System.Func<int> FestivalDays;
        public System.Func<string, int> BirthdayDays;
        public System.Func<int> CropCounter;
        public System.Func<string, bool> HeardLookup;
        public System.Func<string, string> MoodLookup;

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
        public int FestivalDaysAway() => FestivalDays != null ? FestivalDays() : -1;
        public int BirthdayDaysAway(string npcId) => BirthdayDays != null ? BirthdayDays(npcId) : -1;
        public int CropCount() => CropCounter != null ? CropCounter() : 0;
        public int AnimalCount() => _state.Animals.Count;
        public bool HeardLine(string dialogueId) => HeardLookup != null && HeardLookup(dialogueId);
        public string FarmName() => _state.FarmName;
        public string PlayerName() => _state.PlayerName;
        public string MoodOf(string npcId) => MoodLookup != null ? MoodLookup(npcId) : "content";
        public int Gold() => _state.Gold;
        public int HallRoomsRestored() => HallRooms.RestoredCount(_state);
        public bool UpgradeDone(string upgradeId) => _state.UpgradesDone.Contains(upgradeId);

        public int ShippedCount()
        {
            var total = 0;
            foreach (var n in _state.ShippedTotals.Values) total += n;
            return total;
        }

        public string UpgradeState()
        {
            if (_state.PendingUpgrades.Count == 0) return "none";
            var today = Now.TotalDays;
            foreach (var p in _state.PendingUpgrades) if (p.ReadyDay <= today) return "ready";
            return "waiting";
        }
    }
}
