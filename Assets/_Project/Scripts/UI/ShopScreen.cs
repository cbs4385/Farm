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
        readonly Button _sellButton;

        public bool SellButtonShown => _sellButton != null && _sellButton.gameObject.activeSelf;

        // The sell window over the counter; the counter comes back when it is closed.
        public void SellToTheShop()
        {
            var shop = _shopId;
            Close();
            Ui.ShowSell(() => Ui.ShowShop(shop));
        }

        public ShopScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Shop", new Vector2(560, 400), out var root);
            Root = root;

            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _gold = UiKit.Label(stack.transform, "", 18f, TextAlignmentOptions.Right);

            // A scrolling list: the carpenter sells twelve pieces of furniture and six animals, and the animals, the dearest, are last (playtest 2026-10-13: "unable to buy animals").
            var scroll = UiKit.Rect("Scroll", stack.transform);
            UiKit.Size(scroll.gameObject, -1f, -1f, -1f, 1f);
            var catcher = scroll.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scroll.gameObject.AddComponent<RectMask2D>();
            scroll.gameObject.AddComponent<ScrollSelectionFollower>();
            var list = UiKit.VStack(scroll, "List", 4f);
            var fitter = list.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var listRect = (RectTransform)list.transform;
            listRect.anchorMin = new Vector2(0, 1);
            listRect.anchorMax = new Vector2(1, 1);
            listRect.pivot = new Vector2(0.5f, 1f);
            listRect.offsetMin = listRect.offsetMax = Vector2.zero;
            scrollRect.content = listRect;
            scrollRect.viewport = scroll;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.verticalScrollbar = UiKit.MakeScrollbar(scroll);
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            _list = listRect;

            // The general store also buys (sooziq, playtest 2026-10-13: "I don't see how Tilda buys items"): the way to sell was a choice in Tilda's conversation menu that
            // nobody found, so the counter has the button too.
            var bar = UiKit.HStack(stack.transform, "Bar", 10f, TextAnchor.MiddleCenter);
            UiKit.Size(bar.gameObject, -1f, 34f);
            _sellButton = UiKit.MakeButton(bar.transform, L.Get("shop.sell"), SellToTheShop, 240f, 34f);
            _sellButton.name = "SellToShop";
            var close = UiKit.MakeButton(bar.transform, L.Get("ui.close"), Close, 160f, 34f);
            close.name = "Close";
            root.SetActive(false);
        }

        public void OpenShop(string shopId)
        {
            _shopId = shopId;
            _title.text = L.Get("shop." + shopId + ".title");
            _sellButton.gameObject.SetActive(shopId == InteractionMenu.SellerBusiness);
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
                var nameText = L.Get(item.NameKey);
                if (!string.IsNullOrEmpty(item.CropId) && session.Db.TryGetCrop(item.CropId, out var crop))       // which seasons a seed grows in, under its name, before it is bought (playtest 2026-10-14)
                {
                    var seasons = crop.IsTree ? crop.FruitSeasons : crop.Seasons;
                    var now = seasons.Includes(session.Clock.Now.Season);
                    var tint = ColorUtility.ToHtmlStringRGB(now ? UiKit.DimText : new Color(0.90f, 0.45f, 0.35f));
                    nameText += $"\n<size=70%><color=#{tint}>{L.Get(crop.IsTree ? "shop.fruits_in" : "shop.grows_in", CropInfo.SeasonsText(seasons))}</color></size>";
                }
                UiKit.Size(row.gameObject, -1f, nameText.Contains('\n') ? 50f : 38f);

                var icon = UiKit.Panel(row.transform, "Icon", Color.white);
                icon.sprite = item.Icon;
                icon.preserveAspect = true;
                UiKit.Size(icon.gameObject, 32f, 32f);

                var name = UiKit.Label(row.transform, nameText, 18f);
                UiKit.Size(name.gameObject, -1f, 46f, 1f);
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

            var row = UiKit.PinnedRow(frame, 12f, 300f);
            UiKit.MakeButton(row, L.Get("ui.yes"), () => Answer(true), 140f, 36f).name = "Yes";
            UiKit.MakeButton(row, L.Get("ui.no"), () => Answer(false), 140f, 36f).name = "No";
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
            var row = UiKit.PinnedRow(frame, 12f, 180f);
            UiKit.MakeButton(row, L.Get("ui.close"), Dismiss, 160f, 36f).name = "Close";
            root.SetActive(false);
        }

        public void OpenMessage(string messageKey, System.Action onClose) => OpenText(L.Get(messageKey), onClose);

        public void OpenText(string text, System.Action onClose)
        {
            _message.text = text;
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
            UiKit.MakeButton(UiKit.PinnedRow(frame, 8f, 220f), L.Get("ui.continue"), Continue, 200f, 38f).name = "Continue";
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

        // Playtest 2026-10-08: a pad player could not get past this screen without the mouse. A, Enter or Space (the Submit control) now continues whatever has the
        // focus (or nothing), so no pad or keyboard player is ever stuck here.
        public override void Tick()
        {
            if (Time.frameCount != OpenedFrame && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame()) Continue();
        }

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
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Pause", new Vector2(320, 450), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 10f, 18, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("pause.title"), 26f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.MakeButton(stack.transform, L.Get("pause.resume"), Close, 240f, 38f);
            _saveLabel = UiKit.MakeButton(stack.transform, L.Get("pause.save"), Save, 240f, 38f).GetComponentInChildren<TextMeshProUGUI>();
            UiKit.MakeButton(stack.transform, L.Get("pause.options"), () => Ui.Options.Open(), 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.tour"), () => Ui.ShowMenuTour(), 240f, 38f).name = "MenuTour";        // the tour can be seen again whenever it is forgotten
            UiKit.MakeButton(stack.transform, L.Get("pause.report_bug"), () => Ui.BugReporter.Open(), 240f, 38f).name = "ReportBug";
            UiKit.MakeButton(stack.transform, L.Get("pause.main_menu"), ToMainMenu, 240f, 38f);
            UiKit.MakeButton(stack.transform, L.Get("pause.quit"), Quit, 240f, 38f);
            root.SetActive(false);
        }

        TextMeshProUGUI _saveLabel;

        // Playtest 2026-10-10: "when you click save game, there is no pop up or anything". The menu stays open and the button itself says what happened.
        void Save()
        {
            var saved = Ui.Session.Save();
            Ui.Session.Toast(saved ? L.Get("toast.saved") : L.Get("toast.not_saved"));
            if (_saveLabel != null) _saveLabel.text = saved ? L.Get("pause.saved") : L.Get("toast.not_saved");
        }

        public override void Open()
        {
            base.Open();
            if (_saveLabel != null) _saveLabel.text = L.Get("pause.save");
        }

        void ToMainMenu()
        {
            Close();
            Ui.SetHudVisible(false);
            Ui.Session.EndGame();
            Ui.Game.Scenes.Load(SceneNames.MainMenu);
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
