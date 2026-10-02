using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The services counter: tools handed in and ready to collect, and the upgrades on offer with their price.
    public sealed class UpgradeScreen : UiScreen
    {
        readonly RectTransform _list;
        readonly TextMeshProUGUI _gold;
        readonly TextMeshProUGUI _title;
        string _shopId = "blacksmith";

        public UpgradeScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Upgrades", new Vector2(640, 430), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _gold = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Right);

            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, -1f, 1f);
            _list = (RectTransform)list.transform;

            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenFor(string shopId)
        {
            _shopId = shopId;
            _title.text = L.Get("services." + shopId + ".title");
            Rebuild();
            Open();
        }

        void Rebuild()
        {
            var session = Ui.Session;
            _gold.text = L.Get("hud.gold", session.State.Gold);
            UiKit.ClearChildren(_list);
            var today = session.Clock.Now;

            // Tools waiting at this counter.
            foreach (var pending in session.State.PendingUpgrades.Where(p => PendingHere(p)).ToList())
            {
                var row = Row(pending.ToolItemId);
                var title = session.ToolTitle(pending.ToolItemId, pending.Tier);
                var left = Upgrades.DaysLeft(pending, today);
                Text(row, left == 0 ? L.Get("upgrade.ready", title) : L.Get("upgrade.waiting", title, left), 1f);
                if (left == 0) UiKit.MakeButton(row.transform, L.Get("upgrade.collect"), () => { session.CollectUpgrades(); Rebuild(); }, 110f, 32f);
            }

            var offers = Upgrades.Offered(session.UpgradeTable, _shopId, session.State, session.Backpack);
            if (offers.Count == 0 && !session.State.PendingUpgrades.Any(PendingHere))
                UiKit.Label(_list, L.Get("upgrade.nothing"), 18f, TextAlignmentOptions.Center, UiKit.DimText);

            foreach (var def in offers)
            {
                var row = Row(def.Id);
                Text(row, NameOf(session, def), 1f);
                Text(row, CostOf(session, def), 0f, 230f, UiKit.Accent, TextAlignmentOptions.Right);
                var captured = def;
                UiKit.MakeButton(row.transform, L.Get("upgrade.buy"), () => { session.BuyUpgrade(captured); Rebuild(); }, 80f, 32f);
            }
        }

        bool PendingHere(PendingUpgrade p) =>
            Ui.Session.UpgradeTable.All.Any(d => d.Kind == UpgradeKind.Tool && d.ShopId == _shopId && d.ToolItemId == p.ToolItemId);

        static string NameOf(GameSession session, UpgradeDefinition def) =>
            def.Kind == UpgradeKind.Tool ? session.ToolTitle(def.ToolItemId, def.Tier) : L.Get(def.NameKey);

        static string CostOf(GameSession session, UpgradeDefinition def)
        {
            var cost = L.Get("shop.price", def.GoldCost);
            if (string.IsNullOrEmpty(def.MaterialItemId)) return cost;
            var material = session.Db.TryGetItem(def.MaterialItemId, out var item) ? L.Get(item.NameKey) : def.MaterialItemId;
            return cost + " + " + def.MaterialCount + " " + material;
        }

        HorizontalLayoutGroup Row(string name)
        {
            var row = UiKit.HStack(_list, name, 8f);
            UiKit.Size(row.gameObject, -1f, 38f);
            return row;
        }

        static void Text(HorizontalLayoutGroup row, string text, float flex, float width = -1f, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var label = UiKit.Label(row.transform, text, 17f, align, color ?? UiKit.TextColor);
            UiKit.Size(label.gameObject, width, 32f, flex);
        }
    }
}
