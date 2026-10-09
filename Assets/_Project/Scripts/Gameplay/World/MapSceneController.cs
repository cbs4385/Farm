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
            _player.ApplyAvatar(_session.State.Avatar);
            if (_map.MapId != MapIds.Mine) _session.State.Mine.Floor = 0;
            if (_player.GetComponent<PlayerCombat>() == null) _player.gameObject.AddComponent<PlayerCombat>();
            var mine = FindAnyObjectByType<MineController>();
            if (mine != null) mine.Build(_session, _map);
            foreach (var fixture in FindObjectsByType<MovableFixture>(FindObjectsSortMode.None)) fixture.Apply(_session, _map);         // the bed and the kitchen where the player put them
            if (FindAnyObjectByType<FarmBuildingsView>() is FarmBuildingsView buildings) buildings.Rebuild(_session);           // the farm's own buildings, before anyone is placed at their doors
            PlacePlayer(_session.State.SpawnPoint);
            var daylight = FindAnyObjectByType<DayNightLighting>();
            if (daylight != null && !daylight.IsIndoor && _map.MapId != MapIds.Mine)           // tufts and flowers over the grass, in the colours of the season
                gameObject.AddComponent<GroundDecor>().Build(_map, _session.State.WorldSeed, _session.Clock.Now.Season);
            if (_session.MemoryRestorePosition && _session.MemoryId == null)       // back from a memory replay
            {
                _player.transform.position = _session.MemoryReturnPosition;
                _session.MemoryRestorePosition = false;
            }

            _player.gameObject.AddComponent<HammerMode>().Init(_player.GetComponent<PlayerActions>(), _map, _session);
            if (_map.ClutterDensity > 0f) _session.EnsureClutter(_map.MapId, ClutterCandidates(), _map.ClutterDensity);
            _session.RunSpawns(_map.MapId, SpawnCandidates);
            _view.Bind(_session.GetGrid(_map.MapId), _session.Db, _session.GetNodes(_map.MapId), _session.Nodes);

            _camera.SetTarget(_player.transform);
            _camera.SetBounds(_map.WorldBounds);
            _camera.Snap();

            _bus.Subscribe<PassOutTimeReached>(OnPassOut);
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.SetHudVisible(true);
            ServiceLocator.Get<InputService>().EnableGameplay();
            if (_session.PendingIntro && _map.MapId == MapIds.Farm && ServiceLocator.TryGet<IUiService>(out var introUi))
            {
                _session.PendingIntro = false;
                OpeningStory.Show(introUi, _session);
            }

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
            var events = new GameObject("Events").AddComponent<EventDirector>();
            events.Init(_map, _session, npcs, _player);
            if (_map.MapId == MapIds.Village || (_map.MapId == MapIds.FarmHouse && CatBond.Adopted(_session.State))) new GameObject("VillageCat").AddComponent<VillageCat>().Init(_map, _session, npcs, _session.State.WorldSeed);
            new GameObject("Barks").AddComponent<BarkDirector>().Init(_session, npcs, _player, events);
            new GameObject("PhotoMode").AddComponent<PhotoMode>().Init(_session, npcs, _player);
            new GameObject("Hover").AddComponent<HoverInspector>().Init(_session, _map);
            new GameObject("Ambience").AddComponent<AmbienceDirector>().Init(_session, FindAnyObjectByType<DayNightLighting>());

            // Outdoor maps show the day's weather (rain, snow, wind...) drawn from its WeatherDefinition.
            var lighting = FindAnyObjectByType<DayNightLighting>();
            if (lighting == null || !lighting.IsIndoor)
                new GameObject("WeatherEffects").AddComponent<WeatherEffects>();

            new GameObject("WaterSparkle").AddComponent<WaterSparkle>().Init(_map);      // glints on any open water (it removes itself if there is none)

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
            if (reload) StartCoroutine(ReloadWhenIdle());
        }

        // The scene that is starting is still being loaded when these commands run, and the loader ignores a second request: wait for it.
        System.Collections.IEnumerator ReloadWhenIdle()
        {
            var loader = ServiceLocator.Get<SceneLoader>();
            while (loader.IsLoading) yield return null;
            loader.Load(_session.State.CurrentMap, 0f);
        }
#endif

        // QA aid: `-farmOpen inventory|shop|pause|options|message|upgrades|summary|sleep|dialogue|chatmenu|memories|neighbours|gossip|stream|tour|help` opens a screen shortly after the scene starts.
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
                case "bugreport": ui.ShowBugReport(); break;
                case "message": ui.ShowMessage("late_night.warning"); break;
                case "upgrades": ui.ShowUpgrades("blacksmith"); break;
                case "sleep": _session.StartSleep(false); break;   // fade, summary over black, wait for Continue
                case "tour": ui.ShowMenuTour(); break;
                case "help": ui.ShowGameMenu(MenuTabs.Help); break;
                case "crops": PlantShowcase(); break;
                case "dialogue": OpenDialogueShowcase(ui); break;
                case "chatmenu": OpenChatMenuShowcase(); break;
                case "stream":
                {
                    // QA only: stream mode with a 30 second chat-vote timer, on a conversation at its choices.
                    var settings = ServiceLocator.Get<SettingsStore>().Current;
                    settings.StreamMode = true;
                    settings.ChoiceTimer = 30;
                    settings.DialogueSpeed = 3;
                    settings.TextScale = 1.4f;       // also proves the dialogue box fits a very large UI size
                    ui.ApplyUiScale(settings.TextScale);
                    OpenDialogueShowcase(ui);
                    break;
                }
                case "memories":
                    // QA only (a throwaway game): pretend a few scenes were seen so the tab shows both states.
                    foreach (var id in new[] { "festival_spring", "festival_fall", "wren_heart2", "wren_heart5", "hazel_heart2", "bram_heart2", "tilda_heart2", "juno_heart2" })
                        _session.State.EventsSeen.Add(id);
                    ui.ShowGameMenu(MenuTabs.Memories);
                    break;
                case "map":
                    // QA only (a throwaway game): meet a few villagers so the hover labels name them, then open the map tab.
                    foreach (var id in new[] { "wren", "hazel", "bram", "tilda", "juno" }) _session.State.Npcs[id] = new NpcState { Met = true };
                    ui.ShowGameMenu(MenuTabs.Map);
                    break;
                case "neighbours":
                {
                    // QA only (a throwaway game): meet a few villagers at different stages, with a gift or two and a topic found out.
                    foreach (var (id, hearts) in new[] { ("wren", 6), ("hazel", 3), ("bram", 9), ("tilda", 2), ("juno", 4) })
                        _session.State.Npcs[id] = new NpcState { Met = true, Points = hearts * FriendshipModel.PointsPerHeart + 20 };
                    var inter = InteractionState.Load(_session);
                    inter.GiftDay["wren|artisan.wine"] = 1; inter.GiftDay["wren|forage.clam"] = 2; inter.GiftDay["bram|forage.truffle"] = 1;
                    inter.TopicTimes["wren.gossip"] = 2;
                    InteractionState.Store(_session, inter);
                    _session.State.Quests["wren_stew"] = new QuestProgress { Status = "active" };
                    ui.ShowGameMenu(MenuTabs.Social);
                    break;
                }
                case "gossip":
                {
                    // QA only (a throwaway game): a few rare lines heard and one story solved.
                    foreach (var id in new[] { "wren", "hazel", "bram", "tilda" }) _session.State.Npcs[id] = new NpcState { Met = true };
                    var memory = LineMemory.Load(_session);
                    foreach (var id in new[] { "wren", "hazel", "bram" })
                    {
                        var set = _session.Story.Set($"npc.{id}.talk");
                        var rare = System.Linq.Enumerable.FirstOrDefault(set.Entries, GossipBook.IsRare);
                        if (rare != null) memory.Record(set.Id, rare.Dialogue, 5, 0);
                    }
                    LineMemory.Store(_session, memory);
                    _session.SetFlag("storyline.pie_feud"); _session.SetFlag("storyline.lost_umbrella"); _session.SetFlag("storydone.pie_feud");
                    ui.ShowGameMenu(MenuTabs.Gossip);
                    break;
                }
                case "avatar":
                    ui.ShowAvatarCreator(_session.State.Avatar, look => { _session.State.Avatar = look; _player.ApplyAvatar(look); });
                    break;
                case "shipping":
                    _session.Backpack.Add("crop.parsnip", 12);
                    _session.Backpack.Add("crop.potato", 7, 1);
                    _session.Backpack.Add("forage.mushroom", 3);
                    ui.ShowShipping();
                    break;
                case "gazette":
                    foreach (var id in new[] { "wren", "hazel", "bram", "tilda" }) _session.State.Npcs[id] = new NpcState { Met = true };
                    ui.ShowGameMenu(MenuTabs.Gazette);
                    break;
                case "summary":
                    var s = new DaySummary { Earnings = 245, GoldAfter = 745, NewWeather = WeatherIds.Rain };
                    s.Shipped.Add(new ItemStack("crop.parsnip", 4));
                    s.Shipped.Add(new ItemStack("crop.potato", 2, 1));
                    ui.ShowDaySummary(s, () => { });
                    break;
            }
        }

        // QA aid: a real conversation (Tilda's introduction) opened at its choices, to check the dialogue box layout.
        void OpenDialogueShowcase(IUiService ui)
        {
            var source = _session.Story.Dialogue("tilda.first");
            if (source == null) return;
            var graph = new DialogueGraph { Id = source.Id, Start = "n1", Nodes = source.Nodes };
            ui.ShowDialogue(new DialogueRunner(graph, _session.World, _session.StoryText, _ => { }));
        }

        // QA aid: Wren's chat menu opened at the social submenu (the longest list of choices), to check the dialogue box layout.
        void OpenChatMenuShowcase()
        {
            var graph = InteractionMenu.Build(_session.Story, "wren", _session.World, InteractionState.Load(_session), _session.Clock.Now.TotalDays, L.Has);
            if (graph == null) return;
            graph.Start = "social";
            _session.BeginDialogueGraph(graph);
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
            foreach (var w in FindObjectsByType<Warp>()) keepClear.Add(_map.WorldToCell(w.transform.position));
            foreach (var s in FindObjectsByType<SpawnPoint>()) keepClear.Add(_map.WorldToCell(s.transform.position));
            foreach (var u in FindObjectsByType<MonoBehaviour>())
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
            foreach (var sp in FindObjectsByType<SpawnPoint>())
            {
                if (sp.Id == spawnId) { _player.transform.position = sp.transform.position; return; }
                if (sp.Id == "default") fallback = sp;
            }
            if (fallback != null) _player.transform.position = fallback.transform.position;
            else Log.Warn($"No spawn point '{spawnId}' in map '{_map.MapId}'.");
        }
    }
}
