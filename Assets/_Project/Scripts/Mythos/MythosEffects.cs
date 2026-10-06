using System;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Story effects the layer adds (ADR 0003): dread:<n>, lore:<n>, standing:<n>, sick:<npc>, away:<npc>, seal, ending:<id>. Each
    // does nothing at intensity 0.
    public static class MythosEffects
    {
        public static void Register()
        {
            Effects.Register("dread", 1, 1, (s, a) =>
            {
                if (!MythosLevel.On(s)) return;
                s.AddVar(MythosIds.Vars.Dread, Mathf.RoundToInt(Effects.Int(a[0]) * MythosLevel.Scale(s) * (Effects.Int(a[0]) > 0 ? 1f : 2f)), 0, DreadModel.Max);
            });
            Effects.Register("lore", 1, 1, (s, a) => { if (MythosLevel.On(s)) s.AddVar(MythosIds.Vars.Lore, Effects.Int(a[0]), 0, 50); });
            Effects.Register("standing", 1, 1, (s, a) => { if (MythosLevel.On(s)) s.AddVar(MythosIds.Vars.CultStanding, Effects.Int(a[0]), -20, 50); });
            // Making a Keeper unavailable (a way to disrupt a ritual). Not before the player may interfere.
            Effects.Register("sick", 1, 1, (s, a) => { if (MythosLevel.On(s) && s.HasFlag(MythosIds2.Interference) && MythosCast.IsKeeper(a[0])) s.SetFlag(MythosIds2.Sick(a[0])); });
            Effects.Register("away", 1, 1, (s, a) => { if (MythosLevel.On(s) && s.HasFlag(MythosIds2.Interference) && MythosCast.IsKeeper(a[0])) s.SetFlag(MythosIds2.Away(a[0])); });
            Effects.Register("seal", 0, 0, (s, a) => MythosEnding.Seal(s));
            Effects.Register("ending", 1, 1, (s, a) => MythosEnding.Finish(s, a[0]));
        }
    }

    // The endings (X-010): resist (the true fix), join, ignore (the quiet curdling), and the fiery one when Nharoth wakes.
    public static class MythosEnding
    {
        public static string IllustrationPath(string ending) => "Endings/ending_" + ending;

        public const string Awakened = "awakened", Sealed = "sealed", Joined = "joined", Ignored = "ignored";

        // The true fix: Nharoth is put to sleep for good. Wakefulness is reset and no longer rises.
        public static void Seal(GameSession s)
        {
            if (!MythosLevel.On(s)) return;
            var save = RitualDirector.Load(s);
            save.Sealed = true;
            RitualDirector.Store(s, save);
            s.SetVar(MythosIds.Vars.Wakefulness, 0);
            s.SetVar(MythosIds.Vars.Dread, 0);
            s.SetVar(MythosIds2.Step, 0);
            s.SetFlag(MythosIds2.Sealed);
            s.SetFlag(MythosIds2.RitualFlag, false);
            WakefulnessService.ApplyAtmosphere(s);
        }

        public static void OnStep(GameSession s, WakefulnessStepChanged e)
        {
            if (MythosLevel.On(s) && WakefulnessModel.IsFullyAwake(e.Wakefulness)) EventRunner.Trigger(s, "mythos_ending_awakening");
        }

        // Marks an ending reached. The fiery one returns to the main menu (the world is over); the others let play continue.
        public static void Finish(GameSession s, string ending)
        {
            if (!MythosLevel.On(s)) return;
            s.SetVar("ending." + ending, 1);
            s.SetFlag("ending." + ending);
            var save = RitualDirector.Load(s);
            save.EndingShown = Array.IndexOf(new[] { Awakened, Sealed, Joined, Ignored }, ending) + 1;
            RitualDirector.Store(s, save);
            // Every ending gets its picture; the fiery one then says goodbye and returns to the main menu (the world is over).
            System.Action after = null;
            if (ending == Awakened)
                after = () =>
                {
                    if (ServiceLocator.TryGet<IUiService>(out var ui2))
                        ui2.ShowMessage("mythos.ending.awakened", () =>
                        {
                            s.Save();
                            if (ServiceLocator.TryGet<SceneLoader>(out var loader)) { s.EndGame(); loader.Load(SceneNames.MainMenu); }
                        });
                };
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowIllustration(IllustrationPath(ending), "mythos.ending." + ending + ".caption", after);
            else after?.Invoke();
        }
    }
}
