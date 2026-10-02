using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Farm.UI
{
    public abstract class UiScreen
    {
        protected readonly UiService Ui;
        public GameObject Root { get; protected set; }
        public int OpenedFrame { get; private set; } = -1;
        public bool IsOpen => Root != null && Root.activeSelf;

        protected UiScreen(UiService ui) { Ui = ui; }

        public virtual void Open()
        {
            if (IsOpen) return;
            OpenedFrame = Time.frameCount;
            Root.SetActive(true);
            Root.transform.SetAsLastSibling();
            Ui.PushModal(this);
        }

        public virtual void Close()
        {
            if (!IsOpen) return;
            Root.SetActive(false);
            Ui.PopModal(this);
        }

        // Escape / gamepad B while this screen is on top.
        public virtual void OnCancel() => Close();
        public virtual void Tick() { }
    }

    // Persistent UI root: EventSystem, HUD, modal stack, and the IUiService used by gameplay code.
    public sealed class UiService : MonoBehaviour, IUiService
    {
        readonly List<UiScreen> _modals = new List<UiScreen>();
        GameClock _pausedClock;
        bool _hudVisible;

        EventBus _bus;
        InputService _input;
        GameSession _session;
        Canvas _hudCanvas;
        Canvas _screenCanvas;
        HudView _hud;
        InventoryScreen _inventory;
        ShopScreen _shop;
        ConfirmDialog _confirm;
        DaySummaryScreen _summary;
        PauseScreen _pause;
        OptionsScreen _options;

        public Canvas ScreenCanvas => _screenCanvas;
        public InputService Input => _input;
        public GameSession Session => _session;
        public EventBus Bus => _bus;
        public bool AnyModalOpen => _modals.Count > 0;

        public OptionsScreen Options => _options ?? (_options = new OptionsScreen(this));

        // Hooked up before the first scene loads; creates the UI service on the persistent root.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetHook() => _hooked = false;

        static bool _hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            if (_hooked) return;
            _hooked = true;
            Bootstrapper.ServicesCreated += root =>
            {
                var ui = root.AddComponent<UiService>();
                ServiceLocator.Register<IUiService>(ui);
                ServiceLocator.Register(ui);
            };
        }

        void Start()
        {
            _bus = ServiceLocator.Get<EventBus>();
            _session = ServiceLocator.Get<GameSession>();
            _input = ServiceLocator.Get<InputService>();

            var settings = ServiceLocator.Get<SettingsStore>().Current;
            L.SetLanguage(settings.Language);

            CreateEventSystem();
            _hudCanvas = UiKit.CreateCanvas("HudCanvas", 10, transform);
            _screenCanvas = UiKit.CreateCanvas("ScreenCanvas", 100, transform);   // above the scene fade (SceneLoader.FadeSortingOrder)
            ApplyUiScale(settings.TextScale);
            _hud = new HudView(this, _hudCanvas.transform);
            SetHudVisible(_hudVisible);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (CommandLine.GetArg("-farmOpen") == "console") StartCoroutine(OpenConsoleSoon());
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // QA aid for development builds: `-farmOpen console` opens the developer console shortly after start.
        System.Collections.IEnumerator OpenConsoleSoon()
        {
            for (var i = 0; i < 20; i++) yield return null;
            if (_modals.Count == 0) DebugConsole.Open();
        }
#endif

        // Text size option: scales the whole UI by shrinking the canvas reference resolution.
        public void ApplyUiScale(float scale)
        {
            foreach (var canvas in new[] { _hudCanvas, _screenCanvas })
                if (canvas != null) UiKit.SetCanvasScale(canvas, scale);
        }

        void CreateEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.transform.SetParent(transform, false);
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset = _input.Asset;
            module.move = InputActionReference.Create(_input.Ui[InputNames.Navigate]);
            module.submit = InputActionReference.Create(_input.Ui[InputNames.Submit]);
            module.cancel = InputActionReference.Create(_input.Ui[InputNames.Cancel]);
            module.point = InputActionReference.Create(_input.Ui[InputNames.Point]);
            module.leftClick = InputActionReference.Create(_input.Ui[InputNames.Click]);
            module.scrollWheel = InputActionReference.Create(_input.Ui[InputNames.ScrollWheel]);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsoleScreen _debugConsole;

        // Developer console (T-043): F1 or the backtick key. Compiled out of release builds.
        public DebugConsoleScreen DebugConsole => _debugConsole ?? (_debugConsole = new DebugConsoleScreen(this));

        public void ToggleDebugConsole()
        {
            if (DebugConsole.IsOpen) DebugConsole.Close();
            else if (_modals.Count == 0) DebugConsole.Open();    // never stacked on top of another screen
        }
#endif

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.f1Key.wasPressedThisFrame || keyboard.backquoteKey.wasPressedThisFrame))
                ToggleDebugConsole();
#endif
            if (_modals.Count == 0) return;
            var top = _modals[_modals.Count - 1];
            top.Tick();
            if (_input.Ui[InputNames.Cancel].WasPressedThisFrame() && Time.frameCount != top.OpenedFrame) top.OnCancel();
        }

        // ---- modal stack ---------------------------------------------------------------------------------------

        public void PushModal(UiScreen screen)
        {
            if (_modals.Count == 0)
            {
                _input.BlockGameplay();
                if (_session.InGame)
                {
                    _pausedClock = _session.Clock;
                    _pausedClock.Pause();
                }
            }
            _modals.Add(screen);
            FocusFirst(screen.Root);
        }

        public void PopModal(UiScreen screen)
        {
            _modals.Remove(screen);
            if (_modals.Count == 0)
            {
                _input.UnblockGameplay();
                if (_pausedClock != null)
                {
                    _pausedClock.Resume();
                    _pausedClock = null;
                }
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                FocusFirst(_modals[_modals.Count - 1].Root);
            }
        }

        public void CloseAllModals()
        {
            foreach (var m in _modals.ToArray()) m.Close();
        }

        public void FocusFirst(GameObject root)
        {
            if (EventSystem.current == null || root == null) return;
            foreach (var s in root.GetComponentsInChildren<Selectable>(false))
            {
                if (!s.IsInteractable()) continue;
                EventSystem.current.SetSelectedGameObject(s.gameObject);
                return;
            }
        }

        // ---- IUiService ----------------------------------------------------------------------------------------

        public void SetHudVisible(bool visible)
        {
            _hudVisible = visible;
            if (_hudCanvas != null) _hudCanvas.gameObject.SetActive(visible);
        }

        public void ToggleInventory()
        {
            _inventory ??= new InventoryScreen(this);
            if (_inventory.IsOpen) _inventory.Close(); else _inventory.Open();
        }

        public void ShowShop(string shopId)
        {
            _shop ??= new ShopScreen(this);
            _shop.OpenShop(shopId);
        }

        public void ShowConfirm(string messageKey, Action onYes, Action onNo = null)
        {
            _confirm ??= new ConfirmDialog(this);
            _confirm.OpenConfirm(messageKey, onYes, onNo);
        }

        public void ShowDaySummary(DaySummary summary, Action onContinue)
        {
            _summary ??= new DaySummaryScreen(this);
            _summary.OpenSummary(summary, onContinue);
        }

        public void ShowOptions() => Options.Open();

        public void ShowPause()
        {
            _pause ??= new PauseScreen(this);
            _pause.Open();
        }
    }
}
