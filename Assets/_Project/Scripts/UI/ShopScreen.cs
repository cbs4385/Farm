using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // Buy screen. M1 has a single "general" shop selling every seed that has a buy price.
    public sealed class ShopScreen : UiScreen
    {
        readonly RectTransform _list;
        readonly TextMeshProUGUI _gold;
        readonly TextMeshProUGUI _title;
        string _shopId = "general";

        public ShopScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Shop", new Vector2(560, 400), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _gold = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Right);

            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, -1f, 1f);
            _list = (RectTransform)list.transform;

            var close = UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f);
            close.name = "Close";
            root.SetActive(false);
        }

        public void OpenShop(string shopId)
        {
            _shopId = shopId;
            _title.text = L.Get("shop." + shopId + ".title");
            Rebuild();
            Open();
        }

        public override void Open()
        {
            if (IsOpen) return;
            base.Open();
        }

        void Rebuild()
        {
            var session = Ui.Session;
            _gold.text = L.Get("hud.gold", session.State.Gold);
            UiKit.ClearChildren(_list);

            var stock = ShopCatalog.For(session.Db, _shopId, session.World);

            foreach (var item in stock)
            {
                var row = UiKit.HStack(_list, item.Id, 8f);
                UiKit.Size(row.gameObject, -1f, 38f);

                var icon = UiKit.Panel(row.transform, "Icon", Color.white);
                icon.sprite = item.Icon;
                icon.preserveAspect = true;
                UiKit.Size(icon.gameObject, 32f, 32f);

                var name = UiKit.Label(row.transform, L.Get(item.NameKey), 18f);
                UiKit.Size(name.gameObject, -1f, 32f, 1f);
                var price = UiKit.Label(row.transform, L.Get("shop.price", item.BuyPrice), 18f, TextAlignmentOptions.Right, UiKit.Accent);
                UiKit.Size(price.gameObject, 70f, 32f);

                var captured = item;
                UiKit.MakeButton(row.transform, L.Get("shop.buy1"), () => Buy(captured, 1), 70f, 32f);
                UiKit.MakeButton(row.transform, L.Get("shop.buy5"), () => Buy(captured, 5), 70f, 32f);
            }
        }

        void Buy(ItemDefinition item, int count)
        {
            var session = Ui.Session;
            var total = item.BuyPrice * count;
            if (session.State.Gold < total) { session.Toast(L.Get("toast.not_enough_gold")); return; }
            if (!session.Backpack.CanAdd(item.Id, count)) { session.Toast(L.Get("toast.inventory_full")); return; }

            session.TrySpendGold(total);
            session.Backpack.Add(item.Id, count);
            _gold.text = L.Get("hud.gold", session.State.Gold);
            session.Toast(L.Get("toast.bought", count, L.Get(item.NameKey)));
        }
    }

    public sealed class ConfirmDialog : UiScreen
    {
        readonly TextMeshProUGUI _message;
        System.Action _yes, _no;

        public ConfirmDialog(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Confirm", new Vector2(420, 170), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 14f, 16, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            _message = UiKit.Label(stack.transform, "", 20f, TextAlignmentOptions.Center);

            var row = UiKit.HStack(stack.transform, "Buttons", 12f, TextAnchor.MiddleCenter);
            UiKit.MakeButton(row.transform, L.Get("ui.yes"), () => Answer(true), 140f, 36f);
            UiKit.MakeButton(row.transform, L.Get("ui.no"), () => Answer(false), 140f, 36f);
            root.SetActive(false);
        }

        public void OpenConfirm(string messageKey, System.Action yes, System.Action no)
        {
            _message.text = L.Get(messageKey);
            _yes = yes;
            _no = no;
            Open();
        }

        public override void OnCancel() => Answer(false);

        void Answer(bool yes)
        {
            var callback = yes ? _yes : _no;
            _yes = _no = null;
            Close();
            callback?.Invoke();
        }
    }

    // A message the player reads and dismisses (the one-time late-night warning, later tutorials).
    public sealed class MessageDialog : UiScreen
    {
        readonly TextMeshProUGUI _message;
        System.Action _onClose;

        public MessageDialog(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Message", new Vector2(520, 230), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 14f, 18, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            _message = UiKit.Label(stack.transform, "", 19f, TextAlignmentOptions.Center);
            var row = UiKit.HStack(stack.transform, "Buttons", 12f, TextAnchor.MiddleCenter);
            UiKit.MakeButton(row.transform, L.Get("ui.close"), Dismiss, 160f, 36f);
            root.SetActive(false);
        }

        public void OpenMessage(string messageKey, System.Action onClose)
        {
            _message.text = L.Get(messageKey);
            _onClose = onClose;
            Open();
        }

        public override void OnCancel() => Dismiss();

        void Dismiss()
        {
            var callback = _onClose;
            _onClose = null;
            Close();
            callback?.Invoke();
        }
    }

    public sealed class DaySummaryScreen : UiScreen
    {
        readonly RectTransform _list;
        readonly TextMeshProUGUI _title;
        readonly TextMeshProUGUI _footer;
        System.Action _onContinue;

        public DaySummaryScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "DaySummary", new Vector2(520, 420), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 26f, TextAlignmentOptions.Center, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 2f);
            UiKit.Size(list.gameObject, -1f, -1f, -1f, 1f);
            _list = (RectTransform)list.transform;
            _footer = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Center);
            UiKit.MakeButton(stack.transform, L.Get("ui.continue"), Continue, 200f, 38f);
            root.SetActive(false);
        }

        public void OpenSummary(DaySummary summary, System.Action onContinue)
        {
            _onContinue = onContinue;
            _title.text = summary.PassedOut ? L.Get("summary.passed_out") : L.Get("summary.title");

            UiKit.ClearChildren(_list);
            if (summary.Shipped.Count == 0)
            {
                UiKit.Label(_list, L.Get("summary.nothing_shipped"), 18f, TextAlignmentOptions.Center, UiKit.DimText);
            }
            else
            {
                foreach (var stack in summary.Shipped)
                {
                    Ui.Session.Db.TryGetItem(stack.ItemId, out var item);
                    var name = item != null ? L.Get(item.NameKey) : stack.ItemId;
                    var value = item != null ? DayCycle.SellValue(item, stack.Quality, stack.Count) : 0;
                    UiKit.Label(_list, L.Get("summary.line", stack.Count, name, value), 17f);
                }
            }

            var footer = L.Get("summary.earned", summary.Earnings);
            if (summary.PassOutGoldLoss > 0) footer += "\n" + L.Get("summary.pass_out_loss", summary.PassOutGoldLoss);
            if (summary.CropsDied > 0) footer += "\n" + L.Get("summary.crops_died", summary.CropsDied);
            footer += "\n" + L.Get("summary.tomorrow", L.Get("weather." + summary.NewWeather));
            foreach (var note in summary.Notes) footer += "\n" + L.Get(note.Key, note.Args);   // lines added by day-cycle hooks
            _footer.text = footer;
            Open();
        }

        public override void OnCancel() { }   // must be acknowledged with the Continue button

        void Continue()
        {
            var cb = _onContinue;
            _onContinue = null;
            Close();
            cb?.Invoke();
        }
    }

    public sealed class PauseScreen : UiScreen
    {
        public PauseScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Pause", new Vector2(320, 340), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 10f, 18, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("pause.title"), 26f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.MakeButton(stack.transform, L.Get("pause.resume"), Close, 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.save"), Save, 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.options"), () => Ui.Options.Open(), 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.main_menu"), ToMainMenu, 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.quit"), Quit, 240f, 38f);
            root.SetActive(false);
        }

        void Save()
        {
            Ui.Session.Toast(Ui.Session.Save() ? L.Get("toast.saved") : L.Get("toast.not_saved"));
            Close();
        }

        void ToMainMenu()
        {
            Close();
            Ui.SetHudVisible(false);
            Ui.Session.EndGame();
            ServiceLocator.Get<SceneLoader>().Load(SceneNames.MainMenu);
        }

        void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
