using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The rules of fishing (T-050), pure and tested: which fish can bite, which one does, how long it takes, how big the
    // timing zone is, and how good the catch is.
    public static class FishingModel
    {
        public static List<FishRow> Eligible(IEnumerable<FishRow> table, string spot, IWorldQuery world) =>
            table.Where(f => f.Spot == spot && f.Seasons.Includes(world.Now.Season)
                             && (string.IsNullOrWhiteSpace(f.Condition) || (Conditions.TryEvaluate(f.Condition, world, out var ok) && ok))).ToList();

        // Weighted pick; luck makes the rarer fish likelier (and bad luck the common ones).
        public static FishRow? Pick(IReadOnlyList<FishRow> eligible, float luck, float roll)
        {
            if (eligible.Count == 0) return null;
            var max = eligible.Max(f => f.Weight);
            float W(FishRow f) => Math.Max(0.01f, f.Weight * (1f + luck * (1f - f.Weight / max)));
            var total = eligible.Sum(W);
            var at = roll * total;
            foreach (var f in eligible)
            {
                if (at < W(f)) return f;
                at -= W(f);
            }
            return eligible[eligible.Count - 1];
        }

        // Seconds until something bites; bait and the Fishing level shorten it.
        public static float BiteDelay(int level, bool bait, float roll)
        {
            var baseDelay = 2f + roll * 4f;
            return Math.Max(1f, baseDelay * (1f - 0.04f * (Math.Max(1, level) - 1)) * (bait ? 0.6f : 1f));
        }

        // The width (0..1 of the bar) of the zone to hit; harder fish and lower levels give a narrower one.
        public static float ZoneWidth(int level, float difficulty) =>
            Math.Min(0.6f, Math.Max(0.12f, 0.34f + 0.02f * (Math.Max(1, level) - 1) - 0.25f * difficulty));

        public static bool Judge(float marker, float zoneCenter, float zoneWidth, out bool perfect)
        {
            var d = Math.Abs(marker - zoneCenter);
            perfect = d <= zoneWidth * 0.15f;
            return d <= zoneWidth / 2f;
        }

        public static int Quality(int level, float luck, bool perfect, float roll)
        {
            var gold = 0.01f * (level - 1) + (perfect ? 0.2f : 0f) + 0.08f * Math.Max(0f, luck);
            var silver = 0.04f * (level - 1) + (perfect ? 0.3f : 0f) + 0.12f * Math.Max(0f, luck);
            if (roll < gold) return 2;
            return roll < gold + silver ? 1 : 0;
        }

        public static int Xp(FishRow fish) => 8 + (int)(fish.Difficulty * 30f);
    }

    public enum FishingState { Waiting, Bite, Bar, Done }

    // One cast, as a small state machine driven by Tick(dt) and Press(): wait for a bite, react in time, hit the zone.
    public sealed class FishingSession
    {
        public const float BiteWindow = 1.4f;
        const float BarSpeedBase = 0.8f;

        readonly FishRow? _fish;
        readonly int _level;
        readonly float _luck;
        readonly float _qualityRoll;
        float _timer;
        float _dir = 1f;

        public FishingState State { get; private set; } = FishingState.Waiting;
        public float Marker { get; private set; }
        public float ZoneCenter { get; }
        public float ZoneWidth { get; }
        public bool Caught { get; private set; }
        public bool Perfect { get; private set; }
        public int Quality { get; private set; }
        public FishRow? Fish => _fish;
        public bool NothingBites => !_fish.HasValue;

        // Rolls are in [0, 1): which fish, when it bites, where the zone is, how good the catch is.
        public FishingSession(IReadOnlyList<FishRow> eligible, int level, float luck, bool bait, float fishRoll, float biteRoll, float zoneRoll, float qualityRoll,
            float biteSpeed = 1f)
        {
            _fish = FishingModel.Pick(eligible, luck, fishRoll);
            _level = level;
            _luck = luck;
            _qualityRoll = qualityRoll;
            _timer = Math.Max(0.8f, FishingModel.BiteDelay(level, bait, biteRoll) / Math.Max(0.5f, biteSpeed)) + (_fish.HasValue ? 0f : 3f);
            ZoneWidth = FishingModel.ZoneWidth(level, _fish?.Difficulty ?? 0f);
            ZoneCenter = 0.2f + zoneRoll * 0.6f;
        }

        public void Tick(float dt)
        {
            switch (State)
            {
                case FishingState.Waiting:
                    _timer -= dt;
                    if (_timer <= 0f)
                    {
                        if (!_fish.HasValue) { State = FishingState.Done; return; }   // nothing bit: the line comes up empty
                        State = FishingState.Bite;
                        _timer = BiteWindow;
                    }
                    break;
                case FishingState.Bite:
                    _timer -= dt;
                    if (_timer <= 0f) State = FishingState.Done;                      // too slow: it got away
                    break;
                case FishingState.Bar:
                    var speed = BarSpeedBase + (_fish?.Difficulty ?? 0f) * 1.2f;
                    Marker += _dir * speed * dt;
                    if (Marker >= 1f) { Marker = 1f; _dir = -1f; }
                    if (Marker <= 0f) { Marker = 0f; _dir = 1f; }
                    break;
            }
        }

        // The player pressed the action button.
        public void Press()
        {
            if (State == FishingState.Bite) { State = FishingState.Bar; Marker = 0f; return; }
            if (State == FishingState.Bar)
            {
                Caught = FishingModel.Judge(Marker, ZoneCenter, ZoneWidth, out var perfect);
                Perfect = Caught && perfect;
                if (Caught) Quality = FishingModel.Quality(_level, _luck, Perfect, _qualityRoll);
                State = FishingState.Done;
            }
        }

        public void Cancel() => State = FishingState.Done;
    }
}
