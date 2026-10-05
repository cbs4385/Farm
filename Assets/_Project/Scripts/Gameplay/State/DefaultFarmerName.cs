using Farm.Core;

namespace Farm.Gameplay
{
    // The farmer name offered on the new game screen: the signed-in user's account name. Never in stream mode (the account name can be
    // someone's real name, and the UI must not show those on stream; see StreamModeTests), and never an unfit name.
    public static class DefaultFarmerName
    {
        public const string Fallback = "Farmer";
        public const int MaxLength = 16;

        public static string Offer(bool streamMode, string accountName) =>
            streamMode ? Fallback : NameFilter.FromAccountName(accountName, MaxLength, Fallback);

        public static string ForThisUser() => Offer(GameSession.Settings != null && GameSession.Settings.StreamMode, System.Environment.UserName);
    }
}
