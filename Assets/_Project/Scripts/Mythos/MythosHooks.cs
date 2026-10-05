using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Keeps the wakefulness step variable, the atmosphere and the announcements in line with the god's state.
    public static class WakefulnessService
    {
        public static int Wakefulness(GameSession s) => s.GetVar(MythosIds.Vars.Wakefulness);

        // Recomputes the step, announces a new one, and pushes the world's mood.
        public static void SyncStep(GameSession s, MythosSave save)
        {
            var w = Wakefulness(s);
            var step = WakefulnessModel.Step(w);
            s.SetVar(MythosIds2.Step, step);
            if (step > save.AnnouncedStep)
            {
                save.AnnouncedStep = step;
                s.Publish(new WakefulnessStepChanged(step, w));
            }
            ApplyAtmosphere(s);
        }

        // The layer's mood over the light: the ladder's tint for the current step, scaled by the intensity setting.
        public static void ApplyAtmosphere(GameSession s)
        {
            if (!ServiceLocator.TryGet<AtmosphereService>(out var atmosphere)) return;
            var scale = MythosLevel.Scale(s);
            if (scale <= 0f) { atmosphere.Stack.Remove(MythosIds.Atmosphere.Dread); return; }
            var step = WakefulnessLadder.For(Wakefulness(s));
            var dread = s.GetVar(MythosIds.Vars.Dread) / (float)DreadModel.Max;
            atmosphere.Stack.Set(MythosIds.Atmosphere.Dread, step.Tint, Mathf.Clamp01((step.TintStrength + dread * 0.15f) * scale), 5);
        }
    }

    // Overnight (X-001, X-005): the god's wakefulness rises, dread follows, dreams come, rituals are planned and run.
    public sealed class MythosDayHook : DayCycleHook
    {
        readonly GameSession _s;
        public MythosDayHook(GameSession session) { _s = session; }

        public override int Order => 100;

        // Before the day ends: a ritual still running finishes (the player may be asleep).
        public override void OnNightFalls(DayCycleContext context)
        {
            if (!MythosLevel.On(_s)) return;
            RitualDirector.Progress(_s, GameDateTime.DayEndMinute);
            _s.SetFlag(MythosIds2.RitualFlag, false);
        }

        public override void OnDawn(DayCycleContext context)
        {
            if (!MythosLevel.On(_s)) return;
            var save = RitualDirector.Load(_s);
            var now = context.Clock.Now;

            if (!save.Sealed)
            {
                var wake = WakefulnessModel.AfterDay(_s.GetVar(MythosIds.Vars.Wakefulness), now.Day);
                _s.SetVar(MythosIds.Vars.Wakefulness, wake);
            }
            WakefulnessService.SyncStep(_s, save);
            var step = _s.GetVar(MythosIds2.Step);
            _s.SetVar(MythosIds.Vars.Dread, DreadModel.Morning(_s.GetVar(MythosIds.Vars.Dread), step, _s.GetVar(MythosIds.Vars.Lore)));

            if (RitualDirector.SeasonIndex(now) >= 1) _s.SetFlag(MythosIds2.Interference);
            if (save.PlannedSeason != RitualDirector.SeasonIndex(now) && !save.Sealed) RitualDirector.PlanSeason(_s, save);
            _s.SetFlag(MythosIds2.RitualFlag, RitualDirector.IsRitualDay(now) && !save.Sealed && save.ResolvedSeason != save.PlannedSeason);

            // The woods open to the player when summer comes (the cult's first season has passed).
            if (RitualDirector.SeasonIndex(now) >= 1 && !_s.HasFlag(MapIds.WoodsOpenFlag))
            {
                _s.SetFlag(MapIds.WoodsOpenFlag);
                context.Note("mythos.woods_open");   // the day summary says the way is open; the quest "The Path Opens" says where it leads
            }

            if (MythosMutation.Mutate(_s, now.TotalDays) > 0) context.Note("mythos.mutation");

            // Dreams (full intensity gives the stranger ones): a line in the day summary at higher dread.
            var dread = _s.GetVar(MythosIds.Vars.Dread);
            if (dread >= 15 && WeatherRoller.Unit(now.TotalDays * 29 + 3, _s.State.WorldSeed ^ 0x77) < Math.Min(0.7f, dread / 100f + 0.15f))
                context.Note(MythosLevel.Full(_s) && dread >= 40 ? "mythos.dream.full" : "mythos.dream.mild");

            RitualDirector.Store(_s, save);
        }
    }

    // Dread makes outcomes worse (X-001): luck, weather, events and friendships all lean the wrong way, at half strength on mild.
    public sealed class DreadLuckModifier : ILuckModifier
    {
        readonly GameSession _s;
        public DreadLuckModifier(GameSession s) { _s = s; }
        public int Order => 500;
        public float Modify(float luck, GameState state) =>
            luck - DreadModel.LuckPenalty(_s.GetVar(MythosIds.Vars.Dread), MythosLevel.Scale(_s));
    }

    public sealed class DreadWeatherWeights : IWeatherWeightModifier
    {
        readonly GameSession _s;
        public DreadWeatherWeights(GameSession s) { _s = s; }
        public int Order => 100;

        public void Adjust(GameDateTime date, WeatherWeights weights, GameState state)
        {
            var scale = MythosLevel.Scale(_s);
            if (scale <= 0f) return;
            weights.Add(MythosIds.Weather.Fog, DreadModel.FogWeight(_s.GetVar(MythosIds.Vars.Dread), scale) + WakefulnessLadder.For(_s.GetVar(MythosIds.Vars.Wakefulness)).FogWeight * scale);
            weights.Scale(WeatherIds.Storm, 1f + _s.GetVar(MythosIds.Vars.Dread) / 100f * scale);
        }
    }

    // On the full-moon nights of a restless god (step 10+) the moon turns red (full intensity only).
    public sealed class BloodMoonWeather : IWeatherModifier
    {
        readonly GameSession _s;
        public BloodMoonWeather(GameSession s) { _s = s; }
        public int Order => 100;

        public string Modify(GameDateTime date, string weather, GameState state) =>
            MythosLevel.Full(_s) && date.MoonPhase == MoonPhase.Full && _s.GetVar(MythosIds2.Step) >= 10 ? MythosIds.Weather.BloodMoon : weather;
    }

    public sealed class DreadEventWeights : IEventWeightModifier
    {
        readonly GameSession _s;
        public DreadEventWeights(GameSession s) { _s = s; }
        public int Order => 100;

        public float Modify(RandomEventDefinition def, float weight, GameState state)
        {
            var scale = MythosLevel.Scale(_s);
            if (scale <= 0f) return weight;
            var dread = _s.GetVar(MythosIds.Vars.Dread);
            return def.Mood == "bad" ? weight * DreadModel.BadEventFactor(dread, scale)
                 : def.Mood == "good" ? weight / DreadModel.BadEventFactor(dread, scale) : weight;
        }
    }

    public sealed class DreadFriendshipDecay : IFriendshipDecayModifier
    {
        readonly GameSession _s;
        public DreadFriendshipDecay(GameSession s) { _s = s; }
        public int Order => 100;
        public float Modify(string npcId, float rate, GameState state) =>
            rate * DreadModel.DecayFactor(_s.GetVar(MythosIds.Vars.Dread), MythosLevel.Scale(_s));
    }

    // Hidden night schedules (X-003): on a ritual night the Keepers walk to the altar after the day's work; a Keeper who is
    // sick or away stays at home. Built from each Keeper's own workday so the shops still open.
    public sealed class MythosScheduleSource : INpcScheduleSource
    {
        // Where each Keeper stands at the altar clearing in Harrow Wood (cells of the Woods map).
        static readonly Dictionary<string, (int x, int y)> AltarCells = new Dictionary<string, (int, int)>
        {
            { "tilda", (17, 16) }, { "marcus", (21, 16) }, { "odalys", (19, 21) }, { "dorian", (16, 19) }, { "wren", (22, 19) },
        };

        public IEnumerable<NpcScheduleEntry> EntriesFor(NpcDefinition npc)
        {
            if (!MythosCast.IsKeeper(npc.Id)) yield break;
            var work = npc.Schedule.FirstOrDefault(e => e.Id == "workday");
            if (work == null) yield break;

            var ritual = new NpcScheduleEntry { Id = "ritual_night", Priority = 20, Condition = $"horror:1 && flag:{MythosIds2.RitualFlag} && !flag:{MythosIds2.Sick(npc.Id)} && !flag:{MythosIds2.Away(npc.Id)}" };
            foreach (var stop in work.Stops.Where(st => st.Minute < 1100)) ritual.Stops.Add(Copy(stop));
            var cell = AltarCells[npc.Id];
            var last = ritual.Stops[ritual.Stops.Count - 1];
            ritual.Stops.Add(new NpcStop { Minute = Math.Max(1230, last.Minute + 120), Map = MapIds.Woods, X = cell.x, Y = cell.y, Facing = "up" });
            ritual.Stops.Add(new NpcStop { Minute = 1560, Map = npc.HomeMap, X = npc.HomeX, Y = npc.HomeY, Facing = "down" });
            yield return ritual;

            // Unavailable: stays in bed.
            yield return new NpcScheduleEntry
            {
                Id = "unavailable", Priority = 30, Condition = $"horror:1 && (flag:{MythosIds2.Sick(npc.Id)} || flag:{MythosIds2.Away(npc.Id)})",
                Stops = new List<NpcStop> { new NpcStop { Minute = 360, Map = npc.HomeMap, X = npc.HomeX, Y = npc.HomeY } },
            };
        }

        static NpcStop Copy(NpcStop s) => new NpcStop { Minute = s.Minute, Map = s.Map, X = s.X, Y = s.Y, Facing = s.Facing };
    }
}
