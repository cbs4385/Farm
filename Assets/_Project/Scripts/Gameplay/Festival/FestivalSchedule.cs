using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // On a festival day every villager leaves their post, goes to the festival ground in the village at ten and goes home at nine (a schedule entry of their own, so
    // that where they are is still a pure function of the clock). Registered by the session for every game; the horror layer's entries have higher priority and win on a night
    // of a ritual or when a Keeper is unavailable.
    public sealed class FestivalSchedule : INpcScheduleSource
    {
        public const int Priority = 15;

        public IEnumerable<NpcScheduleEntry> EntriesFor(NpcDefinition npc)
        {
            var index = Festivals.IndexOf(npc.Id);
            if (index < 0 || NpcHomes.Of(npc.Id) == null) yield break;
            foreach (var row in Festivals.Rows)
            {
                var cell = Festivals.CellFor(row, index);
                var entry = new NpcScheduleEntry { Id = "festival_" + row.Id, Condition = row.Condition, Priority = Priority };
                entry.Stops.Add(NpcHomes.Stop(GameDateTime.DayStartMinute, npc.Id));
                entry.Stops.Add(NpcHomes.Depart(Festivals.OpenMinute, npc.Id, MapIds.Village, cell.x, cell.y, cell.facing));
                entry.Stops.Add(NpcHomes.Stop(Festivals.CloseMinute, npc.Id));
                foreach (var e in NpcHomes.WithSleep(npc.Id, new[] { entry })) yield return e;
            }
        }
    }
}
