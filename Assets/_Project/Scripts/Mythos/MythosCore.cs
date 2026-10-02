using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The intensity setting as the layer sees it (X-009): 0 off (nothing happens), 1 mild (half-strength, no distorted
    // text, no explicit ritual imagery), 2 full. Read live so changing it in Options takes effect at once.
    public static class MythosLevel
    {
        public static int Of(GameSession s) => s != null ? s.HorrorLevel : 0;
        public static bool On(GameSession s) => Of(s) > 0;
        public static bool Full(GameSession s) => Of(s) >= 2;
        public static float Scale(GameSession s) => Of(s) <= 0 ? 0f : Of(s) == 1 ? 0.5f : 1f;
    }

    // The saved state of the layer, kept as module data (never in GameState).
    [Serializable]
    public sealed class OfferingSlot
    {
        public string NpcId;          // the Keeper who lays it
        public string ItemId;         // what is offered (an item id)
        public string RefKind;        // world-object kind when it is a real thing of the player's, else empty (the village's own)
        public string RefId;
        public bool Taken;            // the player took it before it was consumed
        public bool Consumed;
        public bool Placed;           // laid on the altar (a real object is removed from the world at that moment)
        public bool Missing;          // a real object that was gone when its turn came
    }

    [Serializable]
    public sealed class MythosSave
    {
        public int PlannedSeason = -1;                    // year * 4 + season the plan belongs to
        public List<OfferingSlot> Plan = new List<OfferingSlot>();
        public int ResolvedSeason = -1;
        public bool LastSucceeded;
        public int Successes, Failures;
        public int AnnouncedStep;                         // the highest wakefulness step already announced
        public bool Sealed;                               // the true fix was found: Nharoth sleeps for good
        public int EndingShown;                           // 0 none, else an Ending number
        public int RitualsWitnessed;
    }

    public static class MythosIds2
    {
        public const string Step = "mythos.step";                       // var: the wakefulness step (0..20), for conditions
        public const string RitualFlag = "mythos.ritual_tonight";       // flag: set in the evening of a ritual night
        public const string Interference = "mythos.interference";       // flag: the player may now interfere (from the first summer)
        public const string Sealed = "mythos.sealed";
        public static string Sick(string npc) => $"npc.{npc}.sick";
        public static string Away(string npc) => $"npc.{npc}.away";
    }

    // Who belongs to the Keepers of the Covenant (X-003): about a third of the twelve villagers, including the shopkeeper the
    // player meets first. The base game never reads this.
    public static class MythosCast
    {
        // In the order they are drawn into rituals (rotated by season so each takes a turn).
        public static readonly string[] Keepers = { "tilda", "marcus", "odalys", "dorian", "wren" };
        public static readonly string[] Resisters = { "hazel" };
        public const string Leader = "odalys";

        public static bool IsKeeper(string npcId) => Keepers.Contains(npcId);
        public static string AllegianceOf(string npcId) => IsKeeper(npcId) ? "keeper" : Resisters.Contains(npcId) ? "resister" : "unaware";

        public static bool Available(GameSession s, string npcId) =>
            !s.HasFlag(MythosIds2.Sick(npcId)) && !s.HasFlag(MythosIds2.Away(npcId));
    }

    // The twenty steps of Nharoth's waking (X-008): each 5% of wakefulness changes the world a little more. Everything the
    // ladder does is data here (tint, extra fog, a dread floor) plus the `mythos.step` variable that story data reads.
    public readonly struct LadderStep
    {
        public readonly int Number;
        public readonly Color Tint;
        public readonly float TintStrength;
        public readonly float FogWeight;       // added to the fog weather's odds every season
        public readonly int DreadFloor;
        public readonly string NoteKey;        // a line for the journal's lore page

        public LadderStep(int number, Color tint, float strength, float fog, int dreadFloor)
        { Number = number; Tint = tint; TintStrength = strength; FogWeight = fog; DreadFloor = dreadFloor; NoteKey = "mythos.step." + number; }
    }

    public static class WakefulnessLadder
    {
        public static readonly LadderStep[] Steps = Build();

        static LadderStep[] Build()
        {
            var steps = new LadderStep[WakefulnessModel.StepCount + 1];
            for (var i = 0; i <= WakefulnessModel.StepCount; i++)
            {
                var t = i / (float)WakefulnessModel.StepCount;
                // From a faint cold green, through a sickly yellow, to a deep fire red.
                var tint = t < 0.5f ? Color.Lerp(new Color(0.85f, 0.95f, 0.9f), new Color(0.95f, 0.9f, 0.6f), t * 2f)
                                    : Color.Lerp(new Color(0.95f, 0.9f, 0.6f), new Color(0.95f, 0.35f, 0.2f), (t - 0.5f) * 2f);
                steps[i] = new LadderStep(i, tint, Mathf.Lerp(0f, 0.55f, t), Mathf.Lerp(0f, 14f, t), i * 2);
            }
            return steps;
        }

        public static LadderStep For(int wakefulness) => Steps[WakefulnessModel.Step(wakefulness)];
    }

    // Dread's arithmetic (X-001): the player's unease, 0..100, drifting towards a target set by how awake the god is and what
    // the player has learned. Pure.
    public static class DreadModel
    {
        public const int Max = 100;

        public static int Target(int step, int lore) => Math.Min(Max, step * 3 + Math.Min(20, lore * 2));

        // One morning's change: a step towards the target, but never below the ladder's floor.
        public static int Morning(int dread, int step, int lore)
        {
            var target = Target(step, lore);
            var next = dread + (int)Math.Round((target - dread) * 0.25f) + (target > dread ? 1 : 0);
            return Math.Max(WakefulnessLadder.Steps[Math.Min(step, WakefulnessModel.StepCount)].DreadFloor, Math.Min(Max, Math.Max(0, next)));
        }

        // Luck lost to dread (at most 0.6 at full strength).
        public static float LuckPenalty(int dread, float scale) => dread / (float)Max * 0.6f * scale;
        public static float BadEventFactor(int dread, float scale) => 1f + dread / (float)Max * scale;
        public static float DecayFactor(int dread, float scale) => 1f + dread / 50f * scale;
        public static float FogWeight(int dread, float scale) => dread / (float)Max * 25f * scale;
    }
}
