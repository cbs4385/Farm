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
                var spawn = CommandLine.GetArg("-farmSpawn");   // QA aid: start at a named spawn point
                if (!string.IsNullOrEmpty(spawn)) _session.State.SpawnPoint = spawn;
            }
            L.SetLanguage(ServiceLocator.Get<SettingsStore>().Current.Language);

            _session.State.CurrentMap = _map.MapId;
            if (_map.MapId != MapIds.Mine) _session.State.Mine.Floor = 0;
            if (_player.GetComponent<PlayerCombat>() == null) _player.gameObject.AddComponent<PlayerCombat>();
            var mine = FindFirstObjectByType<MineController>();
            if (mine != null) mine.Build(_session, _map);
            PlacePlayer(_session.State.SpawnPoint);

            if (_map.ClutterDensity > 0f) _session.EnsureClutter(_map.MapId, ClutterCandidates(), _map.ClutterDensity);
            _session.RunSpawns(_map.MapId, SpawnCandidates);
            _view.Bind(_session.GetGrid(_map.MapId), _session.Db, _session.GetNodes(_map.MapId), _session.Nodes);

            _camera.SetTarget(_player.transform);
            _camera.SetBounds(_map.WorldBounds);
            _camera.Snap();

            _bus.Subscribe<PassOutTimeReached>(OnPassOut);
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(true);
            ServiceLocator.Get<InputService>().EnableGameplay();

            // Let optional modules react to the map (spawn objects, add atmosphere, hide/show things).
            _session.Hooks.RaiseMapLoaded(new MapLoadedContext
            {
                MapId = _map.MapId, Session = _session, Map = _map, View = _view, Player = _player, Camera = _camera,
            });

            if (_map.MapId == MapIds.Coop || _map.MapId == MapIds.Barn) new GameObject("Animals").AddComponent<AnimalManager>().Init(_map, _session);

            // Chests, machines, sprinklers and scarecrows the player has placed here.
            new GameObject("PlacedObjects").AddComponent<PlacedObjectsView>().Init(_map, _session);

            // Villagers whose schedule puts them on this map (they walk in and out as the clock runs).
            var npcs = new GameObject("Npcs").AddComponent<NpcManager>();
            npcs.Init(_map, _session);
            new GameObject("Events").AddComponent<EventDirector>().Init(_map, _session, npcs, _player);

            // Outdoor maps show the day's weather (rain, snow, wind...) drawn from its WeatherDefinition.
            var lighting = FindFirstObjectByType<DayNightLighting>();
            if (lighting == null || !lighting.IsIndoor)
                new GameObject("WeatherEffects").AddComponent<WeatherEffects>();

            StartCoroutine(OpenRequestedScreen());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RunStartupCommands();
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static bool _startupCommandsRan;

        // Development builds only: `-farmCommands "date summer 15;time 22:30"` runs developer commands once at start,
        // so QA runs can reach a particular state (see docs/QA.md).
        void RunStartupCommands()
        {
            var text = CommandLine.GetArg("-farmCommands");
            if (_startupCommandsRan || string.IsNullOrEmpty(text)) return;
            _startupCommandsRan = true;

            var processor = new DebugCommandProcessor(_session);
            var reload = false;
            foreach (var line in text.Split(';'))
            {
                var result = processor.Execute(line);
                Log.Info($"[farmCommands] {line} -> {(result.Ok ? "ok" : "FAILED")}: {result.Message}");
                reload |= result.Ok && result.ReloadScene;
            }
            if (reload) ServiceLocator.Get<SceneLoader>().Load(_session.State.CurrentMap, 0f);
        }
#endif

        // QA aid: `-farmOpen inventory|shop|pause|options|message|upgrades|summary|sleep` opens a screen shortly after the scene starts.
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
                case "message": ui.ShowMessage("late_night.warning"); break;
                case "upgrades": ui.ShowUpgrades("blacksmith"); break;
                case "sleep": _session.StartSleep(false); break;   // fade, summary over black, wait for Continue
                case "crops": PlantShowcase(); break;
                case "summary":
                    var s = new DaySummary { Earnings = 245, GoldAfter = 745, NewWeather = WeatherIds.Rain };
                    s.Shipped.Add(new ItemStack("crop.parsnip", 4));
                    s.Shipped.Add(new ItemStack("crop.potato", 2, 1));
                    ui.ShowDaySummary(s, () => { });
                    break;
            }
        }

        // QA aid: one parsnip at every growth stage in a row near the spawn point.
        void PlantShowcase()
        {
            var grid = _session.GetGrid(_map.MapId);
            if (!_session.Db.TryGetCrop("parsnip", out var parsnip)) return;
            for (var stage = 0; stage <= parsnip.MatureStage; stage++)
            {
                var x = 9 + stage * 2;
                grid.Till(x, 15);
                grid.Plant(x, 15, parsnip, Season.Spring);
                if (grid.TryGetTile(x, 15, out var tile)) tile.Crop.Stage = stage;
            }
            _view.RefreshAll();
        }

        // Free cells a spawn table may use: its ground tiles, away from doors, spawn points and anything usable.
        System.Collections.Generic.IReadOnlyList<(int x, int y)> SpawnCandidates(Farm.Data.SpawnTableDefinition table)
        {
            var keepClear = KeepClearCells();
            var grid = _session.GetGrid(_map.MapId);
            var cells = new System.Collections.Generic.List<(int x, int y)>();
            foreach (var cell in _map.CellsOn(table.GroundTiles))
            {
                if (grid.IsTilled(cell.x, cell.y) || NearAny(keepClear, cell, 3)) continue;
                cells.Add((cell.x, cell.y));
            }
            return cells;
        }

        System.Collections.Generic.List<Vector3Int> KeepClearCells()
        {
            var keepClear = new System.Collections.Generic.List<Vector3Int>();
            foreach (var w in FindObjectsByType<Warp>(FindObjectsSortMode.None)) keepClear.Add(_map.WorldToCell(w.transform.position));
            foreach (var s in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None)) keepClear.Add(_map.WorldToCell(s.transform.position));
            foreach (var u in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (u is IInteractable) keepClear.Add(_map.WorldToCell(u.transform.position));
            return keepClear;
        }

        static bool NearAny(System.Collections.Generic.List<Vector3Int> points, Vector3Int cell, int radius)
        {
            foreach (var k in points)
                if (Mathf.Abs(k.x - cell.x) <= radius && Mathf.Abs(k.y - cell.y) <= radius) return true;
            return false;
        }

        // Cells where starting clutter may grow: open grass or dirt, away from doors, spawns and anything usable.
        System.Collections.Generic.IEnumerable<(int x, int y)> ClutterCandidates()
        {
            var keepClear = KeepClearCells();
            var grid = _session.GetGrid(_map.MapId);
            _map.Ground.CompressBounds();
            foreach (var cell in _map.Ground.cellBounds.allPositionsWithin)
            {
                if (!_map.IsOpenGround(cell) || grid.IsTilled(cell.x, cell.y)) continue;
                if (!NearAny(keepClear, cell, 3)) yield return (cell.x, cell.y);
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
