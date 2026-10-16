using System;
using UnityEngine;

namespace Farm.Gameplay
{
    // Running: the farmer moves 1.75 times as fast while the run button is held and costs energy for every few seconds of it. (pure)
    public sealed class RunModel
    {
        public const float SpeedFactor = 1.75f;
        public const float SecondsPerEnergy = 3f;       // a run across the village (about 20 seconds) costs 7 energy; the walk costs nothing

        double _carry;

        // The speed factor for this frame, and the energy to take off for the time run (0 or more whole points, carried over).
        public float Step(bool held, bool moving, int energy, float deltaTime, bool relaxed, out int cost)
        {
            cost = 0;
            if (!held || !moving || energy <= 0) { return 1f; }
            _carry += deltaTime / (double)(relaxed ? SecondsPerEnergy * 2f : SecondsPerEnergy);
            while (_carry >= 1.0 - 1e-6) { _carry = Math.Max(0.0, _carry - 1.0); cost++; }
            return SpeedFactor;
        }

        public void Reset() => _carry = 0.0;

        // A stick's reading with the sensitivity setting applied: the tilt is scaled and capped at full. A key or d-pad (already full) is untouched. (pure)
        public static Vector2 ApplyStickSensitivity(Vector2 raw, float sensitivity)
        {
            if (raw.sqrMagnitude <= 0f) return raw;
            return Vector2.ClampMagnitude(raw * sensitivity, 1f);
        }
    }
}
