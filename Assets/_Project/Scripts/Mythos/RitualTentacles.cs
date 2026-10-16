using System;

namespace Farm.Mythos
{
    // The tentacles that rise out of the ground round the altar on a ritual night (full intensity only, like the rest of the scene). Pure timing, so it can be tested: how high
    // each of the six stands, from 0 (under the ground) to 1 (its whole picture), `offset` game minutes into the ritual.
    //   - During the leader's words they come up one after another, a slot every three minutes, each taking twelve minutes to rise.
    //   - While an offering lies on the altar the tentacle of its turn stands tallest and reaches for it; the rest stand a little lower.
    //   - When the last offering is consumed they sink back over fifteen minutes.
    //   - If the ritual is disrupted (the player took an offering, or it failed) they cringe down to a low crouch and stay there until they sink.
    public static class RitualTentacles
    {
        public const int Slots = 6, StaggerMinutes = 3, RiseMinutes = 12, SinkMinutes = 15;
        public const float Resting = 0.78f, Cringing = 0.12f;

        public static int RiseStart(int slot) => slot * StaggerMinutes;

        // The slot that reaches for the offering on the altar now, or -1 (the leader is speaking, or the ritual is over).
        public static int Reaching(int offset, int offerings)
        {
            var sacrifice = RitualModel.CurrentSacrifice(offset, offerings);
            return sacrifice < 0 ? -1 : sacrifice % Slots;
        }

        public static float Target(int slot, int offset, int offerings, bool disrupted)
        {
            if (offerings <= 0 || offset < 0 || slot < 0 || slot >= Slots) return 0f;
            var end = RitualModel.DurationMinutes(offerings);
            if (offset >= end + SinkMinutes) return 0f;
            var risen = Math.Max(0f, Math.Min(1f, (offset - RiseStart(slot)) / (float)RiseMinutes));
            var height = risen * (Reaching(offset, offerings) == slot ? 1f : Resting);
            if (disrupted) height = Math.Min(height, Cringing);
            if (offset >= end) height *= 1f - Math.Min(1f, (offset - end) / (float)SinkMinutes);
            return height;
        }

        // How many pixel rows of a picture `pictureHeight` tall show at a height.
        public static int Rows(float height, int pictureHeight) => (int)Math.Round(Math.Max(0f, Math.Min(1f, height)) * pictureHeight, MidpointRounding.AwayFromZero);

        // Where the tentacles stand: round an oval about the altar, offsets in cells from the altar. Slot 0 is front left; the ones with a positive y stand behind the altar.
        public static (float x, float y) Place(int slot)
        {
            var angle = (slot + 0.5f) * (float)(Math.PI * 2.0) / Slots + 0.35f;
            return ((float)Math.Cos(angle) * 3.4f, (float)Math.Sin(angle) * 2.3f - 0.35f);
        }
    }
}
