namespace Farm.Gameplay
{
    // The cadence of footsteps: one step each time the player has walked a stride, never while standing still. Pure, so it can be tested.
    public sealed class Footsteps
    {
        public const float Stride = 1.35f;      // world units (tiles) between steps
        float _walked;
        int _count;

        public int Count => _count;

        // Feed the distance walked this frame; true when a step lands. Alternates the pitch (see Pitch) so it does not sound mechanical.
        public bool Walk(float distance)
        {
            if (distance <= 0f) { _walked = 0f; return false; }
            _walked += distance;
            if (_walked < Stride) return false;
            _walked -= Stride;
            _count++;
            return true;
        }

        public float Pitch => (_count & 1) == 0 ? 0.92f : 1.08f;
    }
}
