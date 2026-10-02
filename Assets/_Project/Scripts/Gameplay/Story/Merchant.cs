using Farm.Core;

namespace Farm.Gameplay
{
    // The traveling merchant (T-057): on some days a stall appears in the village selling things the shops do not (ore, coal),
    // with part of the stock rotating. Deterministic from the world seed and the day.
    public static class Merchant
    {
        public const float Chance = 0.3f;

        public static bool IsHere(int worldSeed, int totalDays) => WeatherRoller.Unit(totalDays * 13 + 1, worldSeed ^ 0x5851F42D) < Chance;

        // Which third of the rotating stock is out today.
        public static int Rotation(int totalDays) => totalDays % 3;

        // Condition atoms: `merchant:today` (the stall is up) and `rotate:<0..2>` (this part of the stock is out).
        public static void RegisterConditions()
        {
            Conditions.Register("merchant", (arg, w) => w is IGameQuery q && q.MerchantHere());
            Conditions.Register("rotate", (arg, w) => int.TryParse(arg, out var n) && Rotation(w.Now.TotalDays) == n);
        }
    }
}
