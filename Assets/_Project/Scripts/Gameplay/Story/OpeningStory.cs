namespace Farm.Gameplay
{
    // How the player came by the farm: told once, in a few short pages, when a new game first reaches the farm (playtest request, 2026-10-05).
    public static class OpeningStory
    {
        public const string SeenFlag = "story.intro_seen";
        public const int WelcomeGold = 100;
        public static readonly string[] Pages = { "intro.page1", "intro.page2", "intro.page3", "intro.page4", "intro.page5" };

        public static void Show(IUiService ui, GameSession session)
        {
            if (session.HasFlag(SeenFlag)) return;
            session.SetFlag(SeenFlag);
            session.AddGold(WelcomeGold);   // the note folded into Tilda's letter, read on the bus
            Run(ui.ShowMessage);
        }

        // Each page opens when the one before is closed.
        public static void Run(System.Action<string, System.Action> showPage, int index = 0)
        {
            if (index >= Pages.Length) return;
            showPage(Pages[index], () => Run(showPage, index + 1));
        }
    }
}
