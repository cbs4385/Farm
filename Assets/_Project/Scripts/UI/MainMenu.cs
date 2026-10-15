using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    public sealed class MainMenuScreen : UiScreen
    {
        readonly Button _continue;
        readonly NewGameScreen _newGame;
        readonly LoadGameScreen _load;

        public MainMenuScreen(UiService ui) : base(ui)
        {
            var scrim = UiKit.Panel(ui.ScreenCanvas.transform, "MainMenu", new Color(0.10f, 0.20f, 0.12f, 1f));       // the colour behind the picture (and without it)
            UiKit.Stretch(scrim.rectTransform);
            Root = scrim.gameObject;
            AddPicture(scrim.transform);

            // The title sits in the calm sky at the top of the picture; the buttons are on a dark plate below it so they read over any part of the picture.
            var title = UiKit.Label(scrim.transform, L.Get("game.title"), 64f, TextAlignmentOptions.Center, UiKit.Accent);
            title.enableAutoSizing = true;                                  // "Wetherell Farm Saga" is long: one or two lines at a size that fits
            title.fontSizeMin = 28f; title.fontSizeMax = 64f;
            title.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900f, 130f), new Vector2(0f, -24f));
            // A dark copy just behind and below the letters, so they read over the trees as well as over the sky.
            var shadow = UiKit.Label(scrim.transform, L.Get("game.title"), 64f, TextAlignmentOptions.Center, new Color(0.10f, 0.05f, 0.03f, 0.85f));
            shadow.name = "TitleShadow";
            shadow.enableAutoSizing = true; shadow.fontSizeMin = 28f; shadow.fontSizeMax = 64f;
            shadow.textWrappingMode = TextWrappingModes.Normal;
            UiKit.Place(shadow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900f, 130f), new Vector2(3f, -28f));
            shadow.transform.SetSiblingIndex(title.transform.GetSiblingIndex());           // behind the title

            var plate = UiKit.Panel(scrim.transform, "ButtonPlate", new Color(0.09f, 0.06f, 0.04f, 0.78f));
            UiKit.Place(plate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320f, 346f), new Vector2(0f, -56f));
            var stack = UiKit.VStack(plate.transform, "Stack", 12f, 14, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);
            _continue = UiKit.MakeButton(stack.transform, L.Get("menu.continue"), Continue, 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.new_game"), () => _newGame.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.load_game"), () => _load.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.options"), () => Ui.Options.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.report_bug"), () => Ui.BugReporter.Open(), 260f, 40f).name = "ReportBug";
            UiKit.MakeButton(stack.transform, L.Get("menu.quit"), Quit, 260f, 40f);

            var version = UiKit.Label(scrim.transform, $"v{Application.version}", 14f, TextAlignmentOptions.BottomRight, UiKit.DimText);
            UiKit.Stretch(version.rectTransform, 8f);

            _newGame = new NewGameScreen(ui, this);
            _load = new LoadGameScreen(ui, this);
            Root.SetActive(false);
        }

        public const string PicturePath = "Title/title_background";
        public TitleBackdrop Backdrop { get; private set; }

        // The picture behind the menu, scaled to fill the screen (cropped, never stretched) and moved gently by the breeze. Without the picture the menu is
        // the plain colour it used to be.
        void AddPicture(Transform parent)
        {
            var picture = Resources.Load<Texture2D>(PicturePath);
            if (picture == null) return;
            var holder = UiKit.Rect("Picture", parent);
            holder.anchorMin = holder.anchorMax = holder.pivot = new Vector2(0.5f, 0.5f);
            var fit = holder.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = picture.width / (float)picture.height;
            holder.localScale = new Vector3(1.04f, 1.04f, 1f);               // a little over, so the moving edges never show the colour behind

            var backdrop = UiKit.Rect("Backdrop", holder);
            UiKit.Stretch(backdrop);
            Backdrop = backdrop.gameObject.AddComponent<TitleBackdrop>();
            Backdrop.raycastTarget = false;
            Backdrop.Show(picture);
            TitleAmbience.Create(holder);

            var shade = UiKit.Panel(parent, "Shade", new Color(0.04f, 0.02f, 0.06f, 0.22f));          // takes a little of the glare off the picture
            shade.raycastTarget = false;
            UiKit.Stretch(shade.rectTransform);
        }

        public override void Open()
        {
            base.Open();
            _continue.interactable = SaveSlots.MostRecentSlot(Ui) >= 0;
            if (!_continue.interactable) Ui.FocusFirst(Root);
            OfferCloudMigration();
        }

        // Once, when Steam Cloud is available and saves from an earlier version are on this computer. Any way of closing the question
        // (No, Esc) keeps them local and counts as the answer, so it never comes back.
        void OfferCloudMigration()
        {
            var saves = Ui.Game?.Saves;
            if (saves == null || !saves.ShouldOfferMigration()) return;
            Ui.ShowConfirm("cloud.migrate.offer", () =>
            {
                var moved = saves.MigrateLocalToCloud(out var failed);
                Ui.ShowText(failed > 0 ? L.Get("cloud.migrate.partial", moved, failed) : L.Get("cloud.migrate.done", moved));
            }, saves.DeclineMigration);
        }

        public override void OnCancel() { }

        public void OpenNewGame() => _newGame.Open();
        public void OpenLoad() => _load.Open();

        void Continue()
        {
            var slot = SaveSlots.MostRecentSlot(Ui);
            if (slot >= 0) SaveSlots.LoadAndStart(Ui, slot, error => { });
        }

        void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    static class SaveSlots
    {
        public static int MostRecentSlot(UiService ui)
        {
            var saves = ui.Game.Saves;
            var best = -1;
            var bestTime = System.DateTime.MinValue;
            for (var i = 0; i < SaveService.SlotCount; i++)
            {
                if (!saves.Summarize(i).Exists) continue;
                var t = saves.LastWriteUtc(i);
                if (best < 0 || t > bestTime) { best = i; bestTime = t; }
            }
            return best;
        }

        public const string CloudIcon = "ui_save_cloud";
        public const string LocalIcon = "ui_save_local";
        public const float IconSize = 26f;

        // The small picture that says where a saved game lives (an empty slot keeps the space but shows nothing).
        public static Image AddLocationIcon(Transform row, SaveLocation location, bool visible = true)
        {
            var icon = UiKit.Panel(row, "Location", visible ? Color.white : Color.clear);
            icon.sprite = UiArt.Get(location == SaveLocation.Cloud ? CloudIcon : LocalIcon);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            if (icon.sprite == null || !visible) icon.color = Color.clear;
            UiKit.Size(icon.gameObject, IconSize, IconSize);
            if (visible) UiTip.Add(icon.gameObject, location == SaveLocation.Cloud ? "save.where_cloud" : "save.where_local");
            else UiTip.Skip(icon.gameObject);
            return icon;
        }

        public static string Describe(SlotSummary s)
        {
            if (!s.Exists) return L.Get("slot.empty", s.Slot + 1);
            var season = L.Get("season." + ((Season)s.SeasonIndex).ToString().ToLowerInvariant());
            return L.Get("slot.summary", s.Slot + 1, s.FarmName, season, s.Day, s.Year, s.Gold);
        }

        public static void LoadAndStart(UiService ui, int slot, System.Action<string> onError)
        {
            var session = ui.Game.Session;
            if (!session.BeginLoad(slot, out var error)) { onError(error); return; }
            EnterGame(ui, session);
        }

        public static void EnterGame(UiService ui, GameSession session)
        {
            ui.CloseAllModals();
            ui.Game.Scenes.Load(session.State.CurrentMap);
        }
    }

    public sealed class NewGameScreen : UiScreen
    {
        readonly TMP_InputField _name;
        readonly TMP_InputField _farm;
        readonly RectTransform _slots;
        readonly TextMeshProUGUI _status;
        readonly Image _avatarPreview;
        readonly GameObject _whereRow;
        readonly Image _whereIcon;
        readonly Button _whereButton;
        SaveLocation _where = SaveLocation.Cloud;
        AvatarScreen _creator;
        int _opens;
        AvatarData _avatar = AvatarOptions.Default();

        public AvatarData Avatar => _avatar;

        public NewGameScreen(UiService ui, UiScreen parent) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "NewGame", new Vector2(520, 548), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("newgame.title"), 26f, TextAlignmentOptions.Left, UiKit.Accent);

            UiKit.Label(stack.transform, L.Get("newgame.player_name"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _name = UiKit.MakeInput(stack.transform, L.Get("newgame.player_name"), DefaultFarmerName.Fallback, DefaultFarmerName.MaxLength, 300f);
            UiKit.Label(stack.transform, L.Get("newgame.farm_name"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _farm = UiKit.MakeInput(stack.transform, L.Get("newgame.farm_name"), "Meadow", 20, 300f);
            // The farmer: a small preview and the button that opens the creator.
            var farmer = UiKit.HStack(stack.transform, "Farmer", 10f);
            UiKit.Size(farmer.gameObject, -1f, 64f);
            var stage = UiKit.Panel(farmer.transform, "Stage", UiKit.PanelLight);
            UiKit.Size(stage.gameObject, 44f, 62f);
            _avatarPreview = UiKit.Panel(stage.transform, "Preview", Color.white);
            _avatarPreview.preserveAspect = true;
            _avatarPreview.raycastTarget = true;
            UiTip.Add(_avatarPreview.gameObject, "uitip.avatar.preview");
            UiKit.Place(_avatarPreview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 64f), Vector2.zero);
            UiKit.MakeButton(farmer.transform, L.Get("newgame.customize"), OpenCreator, 300f, 38f).name = "Customize";
            // Where the save lives. Always shown: without Steam Cloud it says so and the game is kept on this computer.
            var where = UiKit.HStack(stack.transform, "SaveWhere", 8f);
            _whereRow = where.gameObject;
            UiKit.Size(_whereRow, -1f, 34f);
            _whereIcon = SaveSlots.AddLocationIcon(where.transform, _where);
            UiTip.Add(_whereIcon.gameObject, () => L.Get(NewGameWhere.Shown(Ui.Game.Saves.CloudAvailable, _where) == SaveLocation.Cloud ? "save.where_cloud" : "save.where_local"));      // follows the toggle
            _whereButton = UiKit.MakeButton(where.transform, "", ToggleWhere, 420f, 34f);
            _whereButton.name = "SaveWhereButton";
            UiKit.Label(stack.transform, L.Get("newgame.choose_slot"), 16f, TextAlignmentOptions.Left, UiKit.DimText);

            var slots = UiKit.VStack(stack.transform, "Slots", 4f);
            _slots = (RectTransform)slots.transform;
            _status = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Left, UiKit.Danger);
            UiKit.MakeButton(stack.transform, L.Get("ui.back"), Close, 160f, 32f);
            root.SetActive(false);
        }

        public override void Open()
        {
            _name.text = DefaultFarmerName.Fallback;
            _avatar = AvatarOptions.Random(System.Environment.TickCount + _opens++ * 7919);   // as if Surprise Me was pressed; Customize changes it
            Ui.Game.Saves.SyncAll();
            _whereRow.SetActive(true);
            ShowWhere();
            Rebuild();
            base.Open();
        }

        void OpenCreator()
        {
            _creator ??= new AvatarScreen(Ui);
            _creator.OpenWith(_avatar, look => { _avatar = look; ShowAvatar(); });
        }

        void ToggleWhere()
        {
            if (!NewGameWhere.CanChoose(Ui.Game.Saves.CloudAvailable)) return;
            _where = _where == SaveLocation.Cloud ? SaveLocation.Local : SaveLocation.Cloud;
            ShowWhere();
        }

        void ShowWhere()
        {
            var available = Ui.Game.Saves.CloudAvailable;
            _whereIcon.sprite = UiArt.Get(NewGameWhere.Shown(available, _where) == SaveLocation.Cloud ? SaveSlots.CloudIcon : SaveSlots.LocalIcon);
            UiKit.SetButtonText(_whereButton, L.Get(NewGameWhere.LabelKey(available, _where)));
            _whereButton.interactable = NewGameWhere.CanChoose(available);
        }

        void ShowAvatar() => _avatarPreview.sprite = AvatarSprites.For(_avatar).Down;

        void Rebuild()
        {
            ShowAvatar();
            _status.text = string.Empty;
            UiKit.ClearChildren(_slots);
            var saves = Ui.Game.Saves;
            for (var i = 0; i < SaveService.SlotCount; i++)
            {
                var slot = i;
                var summary = saves.Summarize(slot);
                var row = UiKit.HStack(_slots, "Slot" + slot, 6f);
                SaveSlots.AddLocationIcon(row.transform, summary.Location, summary.Exists);
                UiKit.MakeButton(row.transform, SaveSlots.Describe(summary), () => Begin(slot, summary.Exists), 426f, 34f);
            }
        }

        void Begin(int slot, bool occupied)
        {
            if (string.IsNullOrWhiteSpace(_name.text) || string.IsNullOrWhiteSpace(_farm.text))
            {
                _status.text = L.Get("newgame.need_names");
                return;
            }
            if (!NameFilter.IsAllowed(_name.text) || !NameFilter.IsAllowed(_farm.text))
            {
                _status.text = L.Get("newgame.name_blocked");
                return;
            }
            if (occupied) Ui.ShowConfirm("confirm.overwrite", () => Start(slot));
            else Start(slot);
        }

        void Start(int slot)
        {
            Ui.Game.Saves.StartSlot(slot, _where);                         // falls back to local by itself when the cloud is not available
            var session = Ui.Game.Session;
            session.BeginNewGame(_name.text, _farm.text, slot, _avatar.Clone(), toolsOnRack: true);
            session.PendingIntro = true;
            SaveSlots.EnterGame(Ui, session);
        }
    }

    public sealed class LoadGameScreen : UiScreen
    {
        readonly RectTransform _slots;
        readonly TextMeshProUGUI _status;
        readonly TextMeshProUGUI _legend;

        public LoadGameScreen(UiService ui, UiScreen parent) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "LoadGame", new Vector2(580, 320), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("menu.load_game"), 26f, TextAlignmentOptions.Left, UiKit.Accent);
            var slots = UiKit.VStack(stack.transform, "Slots", 4f);
            _slots = (RectTransform)slots.transform;
            _legend = UiKit.Label(stack.transform, L.Get("slot.legend"), 14f, TextAlignmentOptions.Left, UiKit.DimText);
            _status = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Left, UiKit.Danger);
            UiKit.MakeButton(stack.transform, L.Get("ui.back"), Close, 160f, 32f);
            root.SetActive(false);
        }

        public override void Open()
        {
            _status.text = string.Empty;
            UiKit.ClearChildren(_slots);
            var saves = Ui.Game.Saves;
            saves.SyncAll();
            var anyCloud = saves.CloudAvailable;
            for (var i = 0; i < SaveService.SlotCount; i++)
            {
                var slot = i;
                var summary = saves.Summarize(slot);
                var row = UiKit.HStack(_slots, "Slot" + slot, 6f);
                SaveSlots.AddLocationIcon(row.transform, summary.Location, summary.Exists);
                anyCloud |= summary.Exists && summary.Location == SaveLocation.Cloud;
                var b = UiKit.MakeButton(row.transform, SaveSlots.Describe(summary),
                    () => SaveSlots.LoadAndStart(Ui, slot, error => _status.text = L.Get("load.failed", error)), 400f, 34f);
                b.interactable = summary.Exists;
                // Playtest 2026-10-10: there was no way to remove a saved game. Deleting asks first.
                var del = UiKit.MakeButton(row.transform, L.Get("load.delete"), () => Ui.ShowConfirm("confirm.delete_save", () => { Ui.Game.Saves.Delete(slot); Open(); }), 82f, 34f);
                del.name = "Delete" + slot;
                del.interactable = summary.Exists;
            }
            _legend.gameObject.SetActive(anyCloud);                       // the legend only matters when the cloud is in play
            base.Open();
        }
    }
}
