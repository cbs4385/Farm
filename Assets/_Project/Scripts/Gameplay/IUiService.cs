using System;

namespace Farm.Gameplay
{
    // Implemented by the UI layer. Gameplay code talks to the UI only through this, so Gameplay never depends on UI.
    public interface IUiService
    {
        void SetHudVisible(bool visible);
        void ToggleInventory();
        // The game menu (skills, social, calendar, map, journal, crafting), optionally on a given tab id.
        void ShowGameMenu(string tab = null);
        void ShowShop(string shopId);
        void ShowConfirm(string messageKey, Action onYes, Action onNo = null);
        void ShowUpgrades(string shopId);
        void ShowMessage(string messageKey, Action onClose = null);
        void ShowDialogue(DialogueRunner runner, Action onClosed = null);
        // The cooking list of a station (the kitchen), and a chest's contents next to the backpack.
        void ShowCrafting(string station);
        void ShowChest(string objectId);
        void ShowLetter(LetterDefinition letter, Action onClosed);
        void ShowBoard();
        void ShowDaySummary(DaySummary summary, Action onContinue);
        void ShowPause();
        void ShowOptions();
        bool AnyModalOpen { get; }
    }
}

namespace Farm.Gameplay
{
    // Tab ids of the game menu (see IUiService.ShowGameMenu).
    public static class MenuTabs
    {
        public const string Skills = "skills";
        public const string Social = "social";
        public const string Calendar = "calendar";
        public const string Map = "map";
        public const string Journal = "journal";
        public const string Crafting = "crafting";
    }
}
