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
