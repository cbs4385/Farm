using Farm.Core;

namespace Farm.Gameplay
{
    // The farmer name offered on the new game screen: "Farmer", for everyone. It used to be the signed-in user's account name, which can be someone's real name and startled
    // players (playtest 2026-10-14); the field is there to type any name over it.
    public static class DefaultFarmerName
    {
        public const string Fallback = "Farmer";
        public const int MaxLength = 16;
    }
}
