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
        void SetLetterbox(bool on, float seconds) { }      // cinematic bars for scenes (T-100)
        void ApplyUiScale(float scale) { }                 // the text size option (and stream mode) rescale the whole UI
        // The cooking list of a station (the kitchen), and a chest's contents next to the backpack.
        void ShowCrafting(string station);
        void ShowChest(string objectId);
        void ShowAvatarCreator(AvatarData look, System.Action<AvatarData> onDone) { }     // the farmer creator (also reachable for QA with -farmOpen avatar)
        void ShowShipping() { }        // the shipping bin as a window: pick items and amounts, ship them as one lot
        void ShowLetter(LetterDefinition letter, Action onClosed);
        void ShowBoard();
        void ShowElevator();
        void ShowHall();
        void ShowFishing(FishingSession session, Action<FishingSession> onDone);
        void ShowDaySummary(DaySummary summary, Action onContinue);
        void ShowPause();
        void ShowOptions();
        bool AnyModalOpen { get; }
        bool PointerOverUi => false;
        void ShowHover(string text, UnityEngine.Vector2 screenPosition) { }     // a small label next to the mouse (hover help)
        void HideHover() { }        // the mouse is over a button or panel, so it is not aiming at the world
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
        public const string Collections = "collections";
        public const string Crafting = "crafting";
        public const string Memories = "memories";
        public const string Gossip = "gossip";
        public const string Gazette = "gazette";
    }
}
