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
            var scrim = UiKit.Panel(ui.ScreenCanvas.transform, "MainMenu", new Color(0.10f, 0.20f, 0.12f, 1f));
            UiKit.Stretch(scrim.rectTransform);
            Root = scrim.gameObject;

            var stack = UiKit.VStack(scrim.transform, "Stack", 12f, 0, TextAnchor.MiddleCenter);
            UiKit.Place((RectTransform)stack.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320, 380), Vector2.zero);

            var title = UiKit.Label(stack.transform, L.Get("game.title"), 54f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Size(title.gameObject, -1f, 90f);
            _continue = UiKit.MakeButton(stack.transform, L.Get("menu.continue"), Continue, 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.new_game"), () => _newGame.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.load_game"), () => _load.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.options"), () => Ui.Options.Open(), 260f, 40f);
            UiKit.MakeButton(stack.transform, L.Get("menu.quit"), Quit, 260f, 40f);

            var version = UiKit.Label(scrim.transform, $"v{Application.version}", 14f, TextAlignmentOptions.BottomRight, UiKit.DimText);
            UiKit.Stretch(version.rectTransform, 8f);

            _newGame = new NewGameScreen(ui, this);
            _load = new LoadGameScreen(ui, this);
            Root.SetActive(false);
        }

        public override void Open()
        {
            base.Open();
            _continue.interactable = SaveSlots.MostRecentSlot(Ui) >= 0;
            if (!_continue.interactable) Ui.FocusFirst(Root);
        }

        public override void OnCancel() { }

        public void OpenNewGame() => _newGame.Open();

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
            var saves = ServiceLocator.Get<SaveService>();
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

        public static string Describe(SlotSummary s)
        {
            if (!s.Exists) return L.Get("slot.empty", s.Slot + 1);
            var season = L.Get("season." + ((Season)s.SeasonIndex).ToString().ToLowerInvariant());
            return L.Get("slot.summary", s.Slot + 1, s.FarmName, season, s.Day, s.Year, s.Gold);
        }

        public static void LoadAndStart(UiService ui, int slot, System.Action<string> onError)
        {
            var session = ServiceLocator.Get<GameSession>();
            if (!session.BeginLoad(slot, out var error)) { onError(error); return; }
            EnterGame(ui, session);
        }

        public static void EnterGame(UiService ui, GameSession session)
        {
            ui.CloseAllModals();
            ServiceLocator.Get<SceneLoader>().Load(session.State.CurrentMap);
        }
    }

    public sealed class NewGameScreen : UiScreen
    {
        readonly TMP_InputField _name;
        readonly TMP_InputField _farm;
        readonly RectTransform _slots;
        readonly TextMeshProUGUI _status;
        readonly Image _avatarPreview;
        AvatarScreen _creator;
        AvatarData _avatar = AvatarOptions.Default();

        public AvatarData Avatar => _avatar;

        public NewGameScreen(UiService ui, UiScreen parent) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "NewGame", new Vector2(520, 500), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("newgame.title"), 26f, TextAlignmentOptions.Left, UiKit.Accent);

            UiKit.Label(stack.transform, L.Get("newgame.player_name"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _name = UiKit.MakeInput(stack.transform, L.Get("newgame.player_name"), "Farmer", 16, 300f);
            UiKit.Label(stack.transform, L.Get("newgame.farm_name"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            _farm = UiKit.MakeInput(stack.transform, L.Get("newgame.farm_name"), "Meadow", 20, 300f);
            // The farmer: a small preview and the button that opens the creator.
            var farmer = UiKit.HStack(stack.transform, "Farmer", 10f);
            UiKit.Size(farmer.gameObject, -1f, 64f);
            var stage = UiKit.Panel(farmer.transform, "Stage", UiKit.PanelLight);
            UiKit.Size(stage.gameObject, 44f, 62f);
            _avatarPreview = UiKit.Panel(stage.transform, "Preview", Color.white);
            _avatarPreview.preserveAspect = true;
            _avatarPreview.raycastTarget = false;
            UiKit.Place(_avatarPreview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 64f), Vector2.zero);
            UiKit.MakeButton(farmer.transform, L.Get("newgame.customize"), OpenCreator, 300f, 38f).name = "Customize";
            UiKit.Label(stack.transform, L.Get("newgame.choose_slot"), 16f, TextAlignmentOptions.Left, UiKit.DimText);

            var slots = UiKit.VStack(stack.transform, "Slots", 4f);
            _slots = (RectTransform)slots.transform;
            _status = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Left, UiKit.Danger);
            UiKit.MakeButton(stack.transform, L.Get("ui.back"), Close, 160f, 32f);
            root.SetActive(false);
        }

        public override void Open()
        {
            Rebuild();
            base.Open();
        }

        void OpenCreator()
        {
            _creator ??= new AvatarScreen(Ui);
            _creator.OpenWith(_avatar, look => { _avatar = look; ShowAvatar(); });
        }

        void ShowAvatar() => _avatarPreview.sprite = AvatarSprites.For(_avatar).Down;

        void Rebuild()
        {
            ShowAvatar();
            _status.text = string.Empty;
            UiKit.ClearChildren(_slots);
            var saves = ServiceLocator.Get<SaveService>();
            for (var i = 0; i < SaveService.SlotCount; i++)
            {
                var slot = i;
                var summary = saves.Summarize(slot);
                UiKit.MakeButton(_slots, SaveSlots.Describe(summary), () => Begin(slot, summary.Exists), 460f, 34f);
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
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame(_name.text, _farm.text, slot, _avatar.Clone());
            session.PendingIntro = true;
            SaveSlots.EnterGame(Ui, session);
        }
    }

    public sealed class LoadGameScreen : UiScreen
    {
        readonly RectTransform _slots;
        readonly TextMeshProUGUI _status;

        public LoadGameScreen(UiService ui, UiScreen parent) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "LoadGame", new Vector2(520, 300), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("menu.load_game"), 26f, TextAlignmentOptions.Left, UiKit.Accent);
            var slots = UiKit.VStack(stack.transform, "Slots", 4f);
            _slots = (RectTransform)slots.transform;
            _status = UiKit.Label(stack.transform, "", 15f, TextAlignmentOptions.Left, UiKit.Danger);
            UiKit.MakeButton(stack.transform, L.Get("ui.back"), Close, 160f, 32f);
            root.SetActive(false);
        }

        public override void Open()
        {
            _status.text = string.Empty;
            UiKit.ClearChildren(_slots);
            var saves = ServiceLocator.Get<SaveService>();
            for (var i = 0; i < SaveService.SlotCount; i++)
            {
                var slot = i;
                var summary = saves.Summarize(slot);
                var b = UiKit.MakeButton(_slots, SaveSlots.Describe(summary),
                    () => SaveSlots.LoadAndStart(Ui, slot, error => _status.text = L.Get("load.failed", error)), 460f, 34f);
                b.interactable = summary.Exists;
            }
            base.Open();
        }
    }
}
