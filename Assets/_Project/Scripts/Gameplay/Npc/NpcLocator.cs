using Farm.Data;

namespace Farm.Gameplay
{
    // Where a villager is right now, for screens that show it (the social tab, the map). Uses the same schedule
    // function as the on-screen actors, so what the menu says is what the player would see.
    public static class NpcLocator
    {
        public static NpcPlacement Where(GameSession session, NpcDefinition npc)
        {
            var plan = NpcSchedule.PlanFor(npc, session.World, session.Hooks.ScheduleEntriesFor(npc));
            var minute = session.Clock.PreciseMinuteOfDay;
            return NpcWake.Adjust(session, npc.Id, NpcSchedule.Where(npc, plan, minute), minute);
        }

        // The map the villager is on or heading for.
        public static string MapOf(GameSession session, NpcDefinition npc)
        {
            var place = Where(session, npc);
            return place.Walking ? place.DestinationMap : place.Map;
        }
    }
}
