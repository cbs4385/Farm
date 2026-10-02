namespace Farm.Gameplay
{
    // The effect verbs that talk to story systems (friendship, quests, mail, events, recipes). Registered with the
    // defaults in Effects.
    public static class StoryEffects
    {
        public static void RegisterAll()
        {
            Effects.Register("friend", 2, 2, (s, a) => NpcInteractions.AddPoints(s, a[0], Effects.Int(a[1])));
            // Remembers the current year under a key (annual events); `unseen:<key>` is true until then.
            Effects.Register("mark", 1, 1, (s, a) => s.SetVar(a[0], s.Clock.Now.Year));
            Effects.Register("energy", 1, 1, (s, a) => s.RestoreEnergy(Effects.Int(a[0])));
            Effects.Register("learn", 1, 1, (s, a) => s.LearnRecipe(a[0]));
            Effects.Register("quest.start", 1, 1, (s, a) =>
            {
                var def = s.Story.Quest(a[0]);
                if (def != null) QuestLog.Start(s, def);
            });
            // Turning a quest in from a dialogue: says so when something is still missing.
            Effects.Register("quest.done", 1, 1, (s, a) =>
            {
                var def = s.Story.Quest(a[0]);
                if (def != null && !QuestLog.TryComplete(s, def)) s.Toast(Farm.Core.L.Get("quest.not_ready"));
            });
            Effects.Register("mail", 1, 1, (s, a) => Mail.Send(s, a[0]));
            Effects.Register("crows", 1, 1, (s, a) => Crows.Strike(s, Effects.Int(a[0])));
            Effects.Register("event", 1, 1, (s, a) => EventRunner.Trigger(s, a[0]));
        }
    }
}
