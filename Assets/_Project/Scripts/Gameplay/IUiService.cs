using System;

namespace Farm.Gameplay
{
    // Implemented by the UI layer. Gameplay code talks to the UI only through this, so Gameplay never depends on UI.
    public interface IUiService
    {
        void SetHudVisible(bool visible);
        void ToggleInventory();
        void ShowShop(string shopId);
        void ShowConfirm(string messageKey, Action onYes, Action onNo = null);
        void ShowDaySummary(DaySummary summary, Action onContinue);
        void ShowPause();
        bool AnyModalOpen { get; }
    }
}
