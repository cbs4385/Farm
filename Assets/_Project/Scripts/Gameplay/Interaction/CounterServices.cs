using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // What a business offers at its counters. Every counter of a business gives all of it (playtest 2026-10-14: the carpenter's shop was at one counter and its upgrades at
    // the next, and the general store's pack counter bought nothing): buying and selling open in the shop window, which has a button for the upgrades.
    public static class CounterServices
    {
        public static bool HasShop(GameSession session, string shopId) =>
            ShopCatalog.For(session.Db, shopId, session.World).Count > 0 || shopId == Farm.Gameplay.InteractionMenu.SellerBusiness;

        public static bool HasUpgrades(GameSession session, string shopId) => session.UpgradeTable.All.Any(d => d.ShopId == shopId);

        // What a counter opens: the shop when there is one (it leads on to the upgrades), else the upgrades, else the empty shop.
        public static bool OpensShop(GameSession session, string shopId) => HasShop(session, shopId) || !HasUpgrades(session, shopId);

        // `upgradesFirst`: the counter was made for the upgrades (the anvil side), so they open first and the shop is a button away; the shop counters do the reverse.
        public static void Use(PlayerActions player, string shopId, bool upgradesFirst = false)
        {
            var session = player.Session;
            if (!BusinessHoursRegistry.IsOpen(shopId, session.Clock.Now))
            {
                session.Toast(BusinessHoursRegistry.ClosedMessage(shopId));
                return;
            }
            if (upgradesFirst && HasUpgrades(session, shopId)) session.Ui?.ShowUpgrades(shopId);
            else if (OpensShop(session, shopId)) session.Ui?.ShowShop(shopId);
            else session.Ui?.ShowUpgrades(shopId);
        }
    }
}
