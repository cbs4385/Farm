# Technical Design

Unity **6000.6.2f1**, URP 2D renderer (already configured in `Assets/Settings`), Input System 1.20 (already installed), C# (.NET Standard 2.1 / Unity default). Targets: Windows x64 (Mono or IL2CPP), Linux x64 (**IL2CPP recommended** for release; Mono for dev iteration).

## 1. Guiding principles
1. **Data-driven**: items, crops, NPCs, recipes, schedules, dialogue, and festivals are `ScriptableObject` assets or JSON/Yarn-style data - never hardcoded.
2. **Deterministic, serializable simulation**: game state lives in plain C# classes (`GameState`) that serialize to JSON. MonoBehaviours are views/controllers, not the source of truth.
3. **Assembly definitions** separate modules to keep compile times small and dependencies one-way.
4. **Testable**: logic in plain C# is covered by EditMode tests; scenes by a few PlayMode smoke tests.
5. **Cross-platform safe**: no hardcoded Windows paths, use `Application.persistentDataPath`, case-sensitive asset paths (Linux!), no `System.Drawing`, forward slashes.
6. **Small, reviewable changes** by agents; each task leaves the project compiling with all tests green.

## 2. Folder layout (create as needed)

```
Assets/
  _Project/
    Art/            Sprites, Tilesets, Animations, UI, Placeholders
    Audio/          Music, SFX, Mixers
    Data/           ScriptableObject definitions (Items, Crops, NPCs, Recipes, Dialogue...)
    Prefabs/
    Scenes/         Bootstrap, MainMenu, Farm, Town, Interiors/, Mine, Beach, Forest
    Scripts/
      Core/         asmdef Farm.Core        (events, service locator, time, save, utils; no UnityEditor)
      Data/         asmdef Farm.Data        (SO definitions + databases)
      Gameplay/     asmdef Farm.Gameplay    (player, farming, inventory, NPC, mining, fishing...)
      UI/           asmdef Farm.UI
      Platform/     asmdef Farm.Platform    (Steam, save-cloud, platform abstractions)
      Editor/       asmdef Farm.Editor      (editor-only tools, validators, build scripts)
    Tests/
      EditMode/     asmdef Farm.Tests.EditMode
      PlayMode/     asmdef Farm.Tests.PlayMode
  Settings/         (existing URP/Input assets)
docs/               (these documents)
```
Dependencies: `Core <- Data <- Gameplay <- UI`; `Platform` depends on `Core`; Editor/Tests depend on whatever they test. **No reverse dependencies.**

## 3. Architecture

### 3.1 Bootstrapping
- `Bootstrap` scene loads first: creates persistent services (`DontDestroyOnLoad` root `Services`), loads settings, then loads `MainMenu`.
- Services registered in a simple `ServiceLocator` (or `Services.Get<T>()`): `GameClock`, `SaveService`, `InputService`, `AudioService`, `SceneLoader`, `ItemDatabase`, `EventBus`, `LocalizationService`, `PlatformService` (Steam/null).
- Scenes are loaded additively where useful (persistent UI scene + map scene).

### 3.2 Events
- Typed `EventBus` (`Publish<T>/Subscribe<T>`) for decoupling (e.g., `DayStarted`, `ItemAdded`, `CropHarvested`, `NpcGifted`). Always unsubscribe in `OnDisable`.

### 3.3 Time
- `GameClock` owns `GameDateTime` (year, season, day, minutes). Real-time tick speed configurable (default 7 real seconds = 10 game minutes). Emits `MinuteTick`, `HourChanged`, `DayEnded`, `DayStarted`, `SeasonChanged`. Pausable via ref-counted pause requests (menus, dialogue, cutscenes).

### 3.4 Input
- Input System with the existing `InputSystem_Actions.inputactions` extended: Move, Interact, UseTool, CycleTool/Hotbar1-12, OpenInventory, OpenMap, Pause, UI navigation. Action maps: `Gameplay`, `UI`, `Dialogue`. Rebinding persisted to settings. Gamepad + keyboard/mouse bindings.

### 3.5 World model
- Each map (`MapId`) has: Tilemap layers (Ground, Paths, Objects, Buildings, Front, Overlay), a `MapRuntimeState` (tile soil/crop data, placed objects, spawned forageables, chests) saved in `GameState`.
- **Farm tile data** is stored in dictionaries `Vector2Int -> TileState` (not as GameObjects). Crops render via a pooled renderer or a dedicated Tilemap layer; avoid one GameObject per tile.
- Collision with Tilemap Colliders (Composite) + `Physics2D`; y-sorting via Sorting Group / custom axis (`TransparencySortMode.CustomAxis (0,1,0)`).
- Map transitions: `Warp` triggers with target `MapId` + spawn point; fade via `SceneLoader`.

### 3.6 Items and inventory
- `ItemDefinition : ScriptableObject` (id string, display name key, sprite, category, stack size, sell price, edible data, tool data, etc.), subtype SOs where needed. Unique stable string IDs (e.g., `crop.parsnip`, `seed.parsnip`) - **never rename shipped IDs**.
- `ItemStack { string itemId; int count; int quality; }` plain class. `Inventory` class with slots, add/remove/move, events. Backpack, chests, shop use the same type.
- `ItemDatabase` loads all definitions (Addressables not required at 1.0; use a generated registry asset listing every definition and validate in editor).

### 3.7 Tools and actions
- `ToolDefinition` + `IToolAction` strategy (Hoe, WateringCan, Axe, Pickaxe, Scythe, Rod). Player computes target tile in facing direction, asks the world `IInteractable`/`ITileTarget` objects whether the action applies, applies energy cost, plays animation, fires events.

### 3.8 Farming
- `CropDefinition` (growth stage days, seasons, regrow days, harvest item, sprite per stage). `CropInstance` (cropId, stage, daysInStage, watered, fertilizerId) saved per tile. On `DayEnded`: advance, apply weather watering, kill out-of-season, spawn weeds.

### 3.9 NPCs
- `NpcDefinition` (id, schedules, dialogue sets, gift tastes, portraits/sprites). Schedules are data: list of `(time, mapId, tile, facing, animation)` keyed by season/day/weather conditions. Pathing: A* on map walkability grids (own implementation or a lightweight library); NPCs can be simulated off-screen by teleporting along the schedule when the player is on another map.
- `FriendshipState` per NPC in `GameState`.

### 3.10 Dialogue / events / quests
- Data-driven dialogue via **Yarn Spinner** (preferred, MIT) or a minimal in-house JSON format; decision to be recorded in an ADR in Plan M1. Cutscene commands (move actor, face, emote, camera, wait, give item, set flag) implemented as a command interpreter.
- `QuestDefinition` + `QuestState` with objectives; global `Flags` set in `GameState` for story/events.

### 3.11 Mining
- `MineGenerator` takes `(seed, floor)` and returns a layout (cellular automata / random rooms), ore/monster tables from data. Enemies use a simple FSM (idle, chase, attack, knockback). Layouts regenerate each day with a day-seeded RNG.

### 3.12 Save system
- `GameState` (versioned, `int saveVersion`) -> JSON via `Newtonsoft.Json` (`com.unity.nuget.newtonsoft-json`) or `JsonUtility` for flat data. Prefer Newtonsoft (dictionaries, polymorphism).
- Save slots (3) under `Application.persistentDataPath/saves/slotN/`. Write to temp file then atomic rename; keep one `.bak`. Autosave on `DayEnded` (after sleeping). Migrations: `ISaveMigration` chain by version.
- Steam Cloud: handled by Steam Auto-Cloud config pointing at the saves folder (Windows `%AppData%/LocalLow/...`, Linux `~/.config/unity3d/<Company>/<Product>`); verify path per platform.

### 3.13 UI
- **UI Toolkit** for menus/HUD *or* uGUI; **decision: uGUI + TextMeshPro for in-game HUD/inventory (easier world-space/pixel-perfect/gamepad nav) unless ADR says otherwise.** All text via Localization string tables.
- MVP pattern: `*View` (MonoBehaviour) bound to `*Presenter` (plain C#) reading state.
- Gamepad navigation required for every screen (EventSystem selection, no mouse-only controls).

### 3.14 Rendering
- Pixel Perfect Camera component (URP 2D): reference 480x270, PPU 16, crop-frame off, upscale render texture off. Point filtering, no compression on pixel sprites, Sprite Atlas (V2) per category.
- URP 2D Lights: global light with color-over-time gradient for day/night; point lights for lamps. Weather via particle systems.
- Use the `unity:2d-pixel-perfect` skill when diagnosing blur/jitter.

### 3.15 Audio
- `AudioService` with pooled sources, mixer groups, music crossfade, snapshot per location. Volume settings saved.

### 3.16 Platform / Steam
- `IPlatformService` (achievements, cloud, user name, overlay hooks). Implementations: `NullPlatformService` (default, used in editor/tests/non-Steam) and `SteamPlatformService` behind scripting define `FARM_STEAM`. Steamworks.NET added later in the plan; **game must run without Steam**.
- Achievements defined in a data asset mapped to Steam API names; unlocked via EventBus.

### 3.17 Settings
- `SettingsData` JSON (volume, resolution/fullscreen, vsync, language, text scale, keybinds, controller prefs) at `persistentDataPath/settings.json`.

## 4. Coding conventions
- C# 9 features OK. `namespace Farm.<Module>`. One public type per file, filename = type. PascalCase types/methods, `_camelCase` private fields, `camelCase` locals.
- No `FindObjectOfType` / `GameObject.Find` in hot paths; no per-frame allocations in Update (avoid LINQ, string concatenation, `new` lists).
- Use `[SerializeField] private` over public fields. Use `[RequireComponent]` and `OnValidate` for sanity checks.
- No magic strings: item IDs / map IDs / flags in constants or data.
- Never edit `.meta` files by hand; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`. **Always commit `.meta` files with their assets.**
- Prefer creating assets via editor scripts / Unity MCP (see Tooling) over hand-writing YAML scenes/prefabs. Do not hand-edit `.unity`/`.prefab` YAML except for trivial, verified changes.
- Comments only for non-obvious "why".

## 5. Testing strategy
- **EditMode (NUnit)**: inventory, time/calendar math, crop growth, economy, friendship, save/migration, mine generation determinism, schedule resolution, data validation (every item referenced exists, ids unique, sprites assigned).
- **PlayMode**: boot-to-farm smoke, sleep->next-day, save/load roundtrip, scene warp.
- **Data validators**: editor menu `Farm/Validate Data` and a test that runs it; fails CI on missing refs.
- Run: `unity test` (see `unity-cli` skill) or `Unity.exe -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml` (no `-quit`).
- Every feature PR/task adds or updates tests where logic is testable.

## 6. Tooling for agents
- **Unity MCP** (`unity-mcp` server; skill `unity-mcp-skill`) for scene/prefab/asset manipulation when the Editor is open. If it fails to connect (ECONNREFUSED), start the MCP bridge in the Editor (Window > MCP for Unity) or fall back to editor scripts run via `-executeMethod`.
- **Unity CLI** skill `unity:unity-cli` for builds/tests; `unity:unity-package-management` for packages (do not hand-edit manifest unless the Editor is open - see note in repo `CLAUDE.md`).
- Related skills: `unity:ui`, `unity:2d-pixel-perfect`, `unity:localization`, `unity:manage-sprite-atlas`, `unity:tilemap-*`, `unity:urp-postprocessing`, `unity:optimize-*`.
- Only one Unity process may open the project at a time. Before running a headless command, check `Temp/UnityLockfile` / running `Unity.exe`; if the Editor is open, use MCP instead or ask the user to close it.

## 7. Packages
Already present: URP, Input System, 2D (animation, tilemap, tilemap extras, spriteshape, aseprite, psdimporter), Timeline, uGUI, Test Framework, Visual Studio/Rider IDE, `com.unity.pipeline` (experimental, agent/CLI connectivity).
To add (via Package Manager): `com.unity.localization`, `com.unity.nuget.newtonsoft-json`, `com.unity.addressables` (optional, later), `com.unity.cinemachine` (camera follow/bounds), Yarn Spinner (decision pending), TextMeshPro (part of uGUI package in Unity 6), Steamworks.NET (via git URL/OpenUPM, late milestone).
Remove unused template packages later (`com.unity.learn.iet-framework`, `Assets/Welcome`).

## 8. Build and release engineering
- Scripting: IL2CPP for release builds on both platforms (Linux IL2CPP requires the Linux Build Support module installed with the Editor, cross-compiling from Windows works with the Linux IL2CPP module).
- Editor build script `Farm.Editor.BuildScript` with methods `BuildWindows`, `BuildLinux`, outputting to `Builds/<platform>/<version>/`. Version from `PlayerSettings.bundleVersion`, bumped per release.
- Graphics APIs: Windows (D3D11, D3D12 optional, Vulkan fallback); Linux (Vulkan primary, OpenGLCore fallback). Test on a real Linux machine or VM and Steam Deck.
- Steam: SteamPipe depots per OS (`app_build.vdf`, `depot_build_windows.vdf`, `depot_build_linux.vdf`), Linux executable bit set, launch options per OS. Steam Runtime considerations for Linux (Unity players run on the "sniper" runtime; verify).
- CI (optional but recommended): GameCI GitHub Actions for tests + builds (Windows & Linux), build artifacts uploaded.
- Source control: git with Unity `.gitignore` and **Git LFS** for binary art/audio. Repo is currently not a git repo - initialize in Milestone 0.

## 9. Placeholder art policy
- Agents must be able to progress without final art. Generate placeholders programmatically (colored 16x16 / 16x32 sprites with labels, simple tile patterns) via an editor tool `Farm/Generate Placeholder Art`, saved in `Art/Placeholders` using the **same naming and slice conventions** as final art (`<category>_<name>_<frame>`), so final sprites can replace them by GUID-preserving overwrite or reassignment in the data assets.
- All final art/audio sources and licenses recorded in `docs/ASSET_LICENSES.md`. Only CC0 / properly licensed / owned assets are allowed.

## 10. Performance budgets
- 60 FPS on Steam Deck at 1280x800; < 200 draw calls on typical map (atlas + batching); GC alloc < 1 KB/frame in steady state; map load < 3 s; save write < 200 ms.

## 11. Risks and decisions log (ADRs go in `docs/adr/NNNN-title.md`)
| Risk | Mitigation |
|---|---|
| Scope creep (genre is huge) | Strict milestone gates; vertical slice first; 1.0 content caps in GDD |
| Save-compat breakage | Versioned saves + migrations + stable IDs from M1 |
| Linux-only bugs found late | Build and smoke-test Linux at every milestone end |
| Art volume | Placeholder pipeline; art tracked as separate backlog; consistent specs |
| Steamworks lock-in | `IPlatformService` abstraction; game runs without Steam |
| Agents conflicting edits to scenes/prefabs | Task ownership by area, small scenes, prefab-per-feature, avoid concurrent scene edits |
