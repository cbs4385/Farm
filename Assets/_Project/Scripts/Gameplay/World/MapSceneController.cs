using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // One per map scene. Binds the scene to the running game: places the player, wires the camera and farm view,
    // and starts a throwaway dev game when the scene is played directly from the Editor.
    public sealed class MapSceneController : MonoBehaviour
    {
        [SerializeField] FarmMap _map;
        [SerializeField] FarmMapView _view;
        [SerializeField] PlayerController _player;
        [SerializeField] CameraFollow _camera;

        EventBus _bus;
        GameSession _session;

        public void Configure(FarmMap map, FarmMapView view, PlayerController player, CameraFollow camera)
        {
            _map = map; _view = view; _player = player; _camera = camera;
        }

        void Start()
        {
            Bootstrapper.InitializeServices();   // no-op when the Bootstrap scene already ran
            _bus = ServiceLocator.Get<EventBus>();
            _session = ServiceLocator.Get<GameSession>();
            if (!_session.InGame)
            {
                Log.Warn($"Scene '{_map.MapId}' started without a game; creating a throwaway dev game.");
                _session.BeginDevGame();
            }
            L.SetLanguage(ServiceLocator.Get<SettingsStore>().Current.Language);

            _session.State.CurrentMap = _map.MapId;
            PlacePlayer(_session.State.SpawnPoint);

            _view.Bind(_session.GetGrid(_map.MapId), _session.Db);

            _camera.SetTarget(_player.transform);
            _camera.SetBounds(_map.WorldBounds);
            _camera.Snap();

            _bus.Subscribe<PassOutTimeReached>(OnPassOut);
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(true);
            ServiceLocator.Get<InputService>().EnableGameplay();
            StartCoroutine(OpenRequestedScreen());
        }

        // QA aid: `-farmOpen inventory|shop|pause|options|summary` opens a screen shortly after the scene starts.
        System.Collections.IEnumerator OpenRequestedScreen()
        {
            var which = CommandLine.GetArg("-farmOpen");
            if (string.IsNullOrEmpty(which) || !ServiceLocator.TryGet<IUiService>(out var ui)) yield break;
            for (var i = 0; i < 5; i++) yield return null;   // let the UI service finish its own startup
            switch (which)
            {
                case "inventory": ui.ToggleInventory(); break;
                case "shop": ui.ShowShop("general"); break;
                case "pause": ui.ShowPause(); break;
                case "options": ui.ShowOptions(); break;
                case "summary":
                    var s = new DaySummary { Earnings = 245, GoldAfter = 745, NewWeather = WeatherIds.Rain };
                    s.Shipped.Add(new ItemStack("crop.parsnip", 4));
                    s.Shipped.Add(new ItemStack("crop.potato", 2, 1));
                    ui.ShowDaySummary(s, () => { });
                    break;
            }
        }

        void OnDestroy()
        {
            _bus?.Unsubscribe<PassOutTimeReached>(OnPassOut);
        }

        void OnPassOut(PassOutTimeReached _)
        {
            _session.StartSleep(true);
        }

        void PlacePlayer(string spawnId)
        {
            SpawnPoint fallback = null;
            foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
            {
                if (sp.Id == spawnId) { _player.transform.position = sp.transform.position; return; }
                if (sp.Id == "default") fallback = sp;
            }
            if (fallback != null) _player.transform.position = fallback.transform.position;
            else Log.Warn($"No spawn point '{spawnId}' in map '{_map.MapId}'.");
        }
    }
}
