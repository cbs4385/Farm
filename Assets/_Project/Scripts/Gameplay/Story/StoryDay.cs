using Farm.Core;

namespace Farm.Gameplay
{
    // What the story systems do each morning (after the overnight logic): mail arrives, quests start and expire, new
    // help-wanted jobs are posted, the day's random event happens. Called by GameSession.EndDay, and once at the start of a
    // new game so the first letter is waiting.
    public static class StoryDay
    {
        public static void Dawn(GameSession s, DaySummary summary)
        {
            var today = s.Clock.Now.TotalDays;
            QuestLog.Expire(s.State, today);
            HelpWanted.Refresh(s.State, s.Story, s.Db, s.World, today);
            Mail.Deliver(s);
            QuestLog.Tick(s);
            RandomEvents.Draw(s, summary);
            if (summary != null && s.State.Mailbox.Count > 0) summary.Notes.Add(new SummaryNote("summary.mail", new object[] { s.State.Mailbox.Count }));
        }

        public static void NewGame(GameSession s)
        {
            Mail.Deliver(s);
            QuestLog.Tick(s);
        }
    }
}
