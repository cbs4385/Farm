using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Torches burn down (owner, 2026-10-12). A torch carried in the backpack lights itself when it is dark where the player is (in the mine, and outdoors from dusk to dawn),
    // burns for an hour of game time, and goes out by itself in daylight and indoors (where the building is lit already); a burning torch that is put out is not used up
    // meanwhile. The minutes left of the torch that is alight are a story variable, so a save keeps them.
    public static class TorchBurn
    {
        public const int Minutes = 60;
        public const string LeftVar = "torch.left";

        // Is it dark enough here to need a torch? (pure) The mine always is; a building never is (it is lit inside); outdoors it is from dusk to dawn.
        public static bool Needed(string mapId, int minuteOfDay)
        {
            if (mapId == MapIds.Mine) return true;
            if (MapIds.IsInterior(mapId)) return false;
            return DayNightLighting.LampsOn(minuteOfDay);
        }

        public static bool Needed(GameSession s) => s != null && s.InGame && Needed(s.State.CurrentMap, s.Clock.Now.MinuteOfDay);

        public static int Left(GameSession s) => s == null ? 0 : s.GetVar(LeftVar);

        // Is a torch giving light right now?
        public static bool IsBurning(GameSession s) => Needed(s) && Left(s) > 0;

        // The fraction of the burning torch that is left, 0 to 1.
        public static float Fraction(GameSession s) => Left(s) / (float)Minutes;

        // Lights a torch from the backpack when it is dark and none is alight. Returns whether a torch is giving light.
        public static bool Refresh(GameSession s, out bool lightedOne, out bool noneLeft)
        {
            lightedOne = false;
            noneLeft = false;
            if (!Needed(s)) return false;
            if (Left(s) > 0) return true;
            if (!s.Backpack.Has(ItemIds.Torch)) { noneLeft = true; return false; }
            s.Backpack.Remove(ItemIds.Torch, 1);
            s.SetVar(LeftVar, Minutes);
            lightedOne = true;
            s.NotifyChanged();
            return true;
        }

        // One game minute goes by. Returns true when the torch that was alight has burned out.
        public static bool OnMinute(GameSession s)
        {
            if (!IsBurning(s)) return false;
            var left = Left(s) - 1;
            s.SetVar(LeftVar, left);
            return left <= 0;
        }
    }
}
