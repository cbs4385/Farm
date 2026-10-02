# Technical Design

Unity **6000.6.2f1**, URP 2D renderer (`Assets/Settings`), Input System 1.20, C# (.NET Standard 2.1 / Unity default). Targets: Windows x64 and Linux x64 (Mono for development, **IL2CPP for release**).

This document describes the architecture **as built** (Milestones 0-1 plus the extension points of ADR 0002) and the design for what is still to come. Where the implementation differs from the original plan, `adr/0001-m1-design-deviations.md` says why.

## 1. Guiding principles
1. **Data-driven**: items, crops, NPCs, recipes, schedules, dialogue, and festivals are `ScriptableObject` assets or JSON data - never hardcoded.
2. **Deterministic, serializable simulation**: game state lives in plain C# classes (`GameState`) that serialize to JSON. MonoBehaviours are views/controllers, not the source of truth.
3. **Assembly definitions** separate modules to keep compile times small and dependencies one-way.
4. **Testable**: logic in plain C# is covered by EditMode tests; scenes and flows by PlayMode tests, including tests that press real (simulated) keys.
5. **Cross-platform safe**: no hardcoded Windows paths, use `Application.persistentDataPath`, case-sensitive asset paths (Linux!), no `System.Drawing`, forward slashes.
6. **Small, reviewable changes** by agents; each task leaves the project compiling with all tests green.
7. **Optional layers plug in, they are not woven in.** Core assemblies know nothing about the horror layer. Optional content registers through generic hooks, conditions and content packs (section 3.18). Core code must never reference `Farm.Mythos`.
8. **The player controls intensity.** `SettingsData.HorrorLevel` (0/1/2) is respected by every hook and module; at 0 the game must behave exactly like the base game.
9. **Failure isolation.** A faulty hook, module or optional data must be logged and skipped, never allowed to break sleeping, loading a map, or saving.

## 2. Folder layout (as built)

```
Assets/
  _Project/
    Art/            Placeholders/ (generated sprites), Tiles/ (tile assets)
    Audio/          (empty until real audio)
    Data/           Items/, Crops/ (generated ScriptableObjects)
    Resources/      FarmInput.inputactions, GameDatabase.asset, Localization/en.json, Packs/ (optional ContentPacks)
    Scenes/         Bootstrap, MainMenu, Farm, FarmHouse, PixelPerfectTest
    Scripts/
      Core/         Farm.Core      Bootstrapper, SceneLoader, ServiceLocator, EventBus, Log, CommandLine, ScreenshotCapture
        Time/       GameDateTime (calendar, MoonPhase), GameClock
        Save/       AtomicFile, SettingsData/SettingsStore
        Localization/ L (string tables, filters, extra tables)
        Conditions/ Conditions (condition language), IWorldQuery
      Data/         Farm.Data      ItemDefinition, CropDefinition, GameDatabase, ContentPack, ItemIds, enums
      Gameplay/     Farm.Gameplay  GameServices, DisplaySettings, IUiService, PixelSnapCamera
        Input/      InputNames, InputService
        State/      GameState, GameSession, SaveService, DayCycle
        Farming/    FarmGrid
        Inventory/  Inventory, ItemStack
        Player/     PlayerController, PlayerActions
        Interaction/ IInteractable, ShippingBin, ShopCounter, Bed
        World/      FarmMap, FarmMapView, MapSceneController, CameraFollow, Warp, SpawnPoint, ConditionalObject, DayNightLighting
        Audio/      AudioService
        Hooks/      GameHooks, GameModules, StateWorldQuery, AtmosphereStack, AtmosphereService
      UI/           Farm.UI        UiKit, UiService, HudView, InventoryScreen, ShopScreen (+Confirm, DaySummary), PauseScreen, OptionsScreen, MainMenu
      Platform/     Farm.Platform  (reserved for Steam / platform services)
      Mythos/       Farm.Mythos    MythosIds, MythosModule (inert skeleton of the optional horror layer)
      Editor/       Farm.Editor    ProjectConfigurator, TextureImportPostprocessor, PlaceholderArtGenerator, ContentGenerator,
                                   InputAssetGenerator, TmpSetup, SceneSetup, MapBuilder, BuildScript
    Tests/
      EditMode/     Farm.Tests.EditMode
      PlayMode/     Farm.Tests.PlayMode
  Settings/         (URP assets; the template input asset is unused)
docs/               (these documents)
```
Dependencies: `Core <- Data <- Gameplay <- UI`; `Platform` and `Mythos` depend on Core (Mythos also on Data and Gameplay); **nothing in core depends on Mythos**; Editor and Tests depend on whatever they test. No reverse dependencies.

## 3. Architecture

### 3.1 Bootstrapping and services
- The `Bootstrap` scene (build index 0) runs `Bootstrapper`, which creates the persistent `Services` root (`DontDestroyOnLoad`), registers core services, raises `Bootstrapper.ServicesCreated` so higher layers add theirs, then loads `MainMenu` (or `-farmScene <name>` for QA).
- `ServiceLocator` holds: `EventBus`, `SceneLoader`, `SettingsStore`, `GameDatabase`, `InputService`, `SaveService`, `GameSession`, `AtmosphereService`, `AudioService`, `UiService` (also as `IUiService`). Registered by `GameServices` (Gameplay) and `UiService` (UI) via `[RuntimeInitializeOnLoadMethod]`.
- **Modules initialize last** (`GameModules.InitializeAll`), once every service exists.
- Scenes load in single mode; the persistent root carries state between them. Map scenes started directly in the Editor create a throwaway dev game (`BeginDevGame`).
- Tests can redirect saves and settings with `GameServices.DataRootOverride` and reset services with `Bootstrapper.ResetForTests`.

### 3.2 Events
- Typed `EventBus` (`Publish<T>/Subscribe<T>`), handler exceptions isolated and logged. Events today: `MinuteChanged`, `PassOutTimeReached`, `DayEnded`, `DayStarted`, `SeasonChanged`, `StatsChanged`, `ToastRequested`, `DayCycleFinished`, `FlagChanged`, `VarChanged`. Always unsubscribe in `OnDestroy`/`OnDisable`.

### 3.3 Time
- `GameDateTime` (year, season, day, minute-of-day 360..1560, `MoonPhase`, `DayOfWeek`) and `GameClock` (7 real seconds = 10 game minutes, ref-counted pause, stops at 02:00 and raises `PassOutTimeReached`; `StartNextDay` raises `DayEnded`, `SeasonChanged`, `DayStarted`). Plain C# and fully unit-tested.

### 3.3b Late-night fatigue (design, T-046)
- Today the clock stops at 02:00 and the player passes out. The decided replacement (GDD decisions R-X): the day runs 06:00 to 06:00. The first time the clock reaches 22:00 a one-time message is shown; from 22:00 a fatigue rating grows with time awake (assumed linear to its maximum at 06:00); it reduces the energy recovered by sleeping (up to 100%) and luck (up to 50%); a fatigue meter beside the energy bar shows it, only while it is positive; at 06:00 the player falls asleep. It is part of the base game, not the horror layer. (An earlier quick-time-event design was withdrawn.)
- Built and tested (pure logic, `Gameplay/Night`): `FatigueModel` (the rating by time of day, linear from 22:00 to 06:00; meter visibility; penalty arithmetic; luck applied to good luck only; energy after sleep; the one-time warning trigger) and `FatigueState` (what is carried across sleeps: a sleep in a bed clears it, a collapse elsewhere carries the fatigue into the next day, and only the luck penalty and meter use the carried value, not the next night's energy recovery). `EnergyActionCompleted` is still published by `PlayerActions` (no system uses it now).
- Business hours (`Gameplay/Shop`): `BusinessHours` (open and close minute, weekly day off) and `BusinessHoursRegistry`, with the condition atom `open:<shopId>` (an unregistered shop is always open). The agreed hours and days off (GDD decision AF) are registered at startup by `RegisterDefaults`; the village maps (T-031) use the condition on doors and shop screens.
- Still to build: the longer `GameDateTime` day (touches the clock, lighting, HUD and tests), the HUD meter and message, applying fatigue to sleeping and to luck (through `ILuckModifier`), the 06:00 automatic sleep, shop hours (09:00-17:00) and NPC night schedules.

### 3.4 Input
- `Resources/FarmInput.inputactions` is **generated** by `InputAssetGenerator` (maps: `Gameplay`, `UI`; hotbar 1-12, UseTool, Interact, Inventory, Pause, prev/next). `InputService` instantiates a private copy (rebinding never edits the shared asset), ref-counts gameplay blocking while menus are open, and persists binding overrides in settings. A `Dialogue` map arrives with T-034.

### 3.5 World model
- Each map is a scene with Tilemap layers (Ground, Soil, Crops, Walls) and a `FarmMap` describing them; `MapSceneController` binds the scene to the running game (places the player at the spawn point, draws the farm grid, sets camera bounds, raises `GameHooks.MapLoaded`).
- **Farm tile data** lives in `FarmGrid` (dictionary of tilled tiles, each with an optional crop), stored per map in `GameState.Maps`. `FarmMapView` draws it onto tilemaps; no GameObject per tile.
- Warps (`Warp`, with optional **condition** and blocked-message) move between map scenes through `MapTravel`; state persists in `GameSession`. `ConditionalObject` shows or hides scene content from a condition.
- Camera: `CameraFollow` clamps to map bounds using the real ortho size; `PixelSnapCamera` snaps to the art-pixel grid (see 3.14). Collision uses Tilemap colliders and Physics2D.

### 3.6 Items and inventory
- `ItemDefinition` (stable string id, name/description keys, icon, category, stack size, prices, tool type, crop id) and `CropDefinition` ScriptableObjects, generated from the tables in `ContentGenerator`. Ids look like `crop.parsnip`, `seed.parsnip`, `tool.hoe`; **never rename shipped ids**.
- `GameDatabase` (`Resources/GameDatabase`) is the registry; `ContentPack` assets under `Resources/Packs` are merged in at startup (id clashes with core are rejected and logged).
- **Shops are data.** Each `ItemDefinition` lists the shops that sell it (`SoldIn`, e.g. `general`) and may add a `SaleCondition`; `ShopCatalog.For(db, shopId, world)` builds a shop's stock. Content that does not opt in is sold nowhere, so pack items (e.g. horror seeds) can never leak into the general store.
- `ItemStack { itemId, count, quality }` and `Inventory` (slots, add/remove/move with stack limits, change event). Backpack uses it; chests and shops will too.

### 3.7 Tools and actions
- `PlayerActions` turns input into world actions on the tile in front of the player: tools by `ToolType` (hoe, watering can implemented; axe, pickaxe, scythe await targets), seeds, food, harvesting and interaction (`IInteractable`). Failures explain themselves with a toast. A formal `IToolAction` strategy arrives with T-032.

### 3.8 Farming
- `FarmGrid` rules (pure, tested): till, water, plant (season check), growth per watered day (or any rainy day), regrow crops, harvest, out-of-season death on the night the season changes. Nightly processing is called from `DayCycle`. A `CropDefinition.GrowCondition` keeps a crop dormant (no growth, no death) while it does not hold; day-cycle hooks can change crops in place (swap `CropInstance.CropId`) to mutate them.

### 3.9 NPCs (design, M2)
- `NpcDefinition` (id, schedules, dialogue sets, gift tastes, portraits/sprites, optional allegiance). Schedules are data: entries of `(time, mapId, tile, facing, animation)` each with an optional **condition**. Pathing: A* on map walkability grids; NPCs are simulated off-screen by teleporting along the schedule. `FriendshipState` per NPC in `GameState`. Dialogue sets, gift reactions and heart events also accept conditions.

### 3.10 Dialogue / events / quests (design, M2)
- Data-driven dialogue via **Yarn Spinner** (preferred, MIT) or a minimal in-house format; decide in an ADR at T-034. Lines, choices, events and cutscene steps accept a `Condition` and can set flags and variables. Text goes through `L.Get`, so text filters apply. `QuestDefinition` + `QuestState` use the global flags/variables.

### 3.11 Mining (design, M3)
- `MineGenerator` takes `(seed, floor)` and returns a layout; ore/monster tables from data; enemies use a simple FSM; layouts regenerate each day with a day-seeded RNG.

### 3.12 Save system
- `GameState` (versioned, `SaveVersion`) -> JSON via Newtonsoft. `SaveService`: 3 slots under `persistentDataPath/saves/slotN/save.json`, atomic write (`AtomicFile`), `.bak` fallback when the main file is corrupt, `ISaveMigration` chain by version, rejection of saves from newer versions. Autosave when sleeping.
- **Additive fields need no migration** (older saves load with defaults); a migration is required only when an existing field's shape or meaning changes. Story state uses `Flags`, `Vars` and per-module `ModuleData` (JSON blobs keyed by module id), so optional layers never change the core schema.
- Steam Cloud: Auto-Cloud pointing at the saves folder (Windows `%AppData%/LocalLow/<Company>/<Product>`, Linux `~/.config/unity3d/<Company>/<Product>`); verify per platform.

### 3.13 UI
- **uGUI + TextMeshPro, built in code** by `UiKit` (no hand-authored prefabs). Reference resolution 960x540 per canvas; the UI-size option scales the reference resolution. All controls are `Selectable`s, so gamepad and keyboard navigation work.
- `UiService` (persistent) owns the EventSystem, the HUD (`HudView`), the modal stack (blocks gameplay input and pauses the clock while any screen is open) and implements `IUiService`, the only way gameplay code talks to the UI.
- **Draw order:** HUD canvas 10, scene fade 50, menus and dialogs 100 (so the day summary shows over the black fade).
- HUD extension: modules add widgets through `GameHooks.AddHudWidget`; the HUD hosts them top-left.
- All text via `L.Get(key)`; lint tests forbid literal UI text and check that every key exists.

### 3.14 Rendering
- Pixel Perfect Camera (URP 2D): reference 480x270, PPU 16, upscale RT off. **`PixelSnapCamera`** must be on every gameplay camera (it snaps the camera to 1/16 unit after follow logic; without it the view shimmers at 2x+).
- **Sprite import rules** (`TextureImportPostprocessor`, enforced by tests): everything under `Art/` is a *Single* sprite, PPU 16, point filter, uncompressed, no mipmaps; characters (`player_*`, `npc_*`) pivot at their feet. (Multiple mode silently ignores the pivot.)
- `DayNightLighting` drives a global `Light2D` from the time of day (gradient), weather, and the **atmosphere stack**; indoor maps use a fixed warm light.
- Use the `unity:2d-pixel-perfect` skill when diagnosing blur/jitter.

### 3.15 Audio
- `AudioService`: logical buses (master, music, sfx, ambience) as volume multipliers and generated placeholder blips; settings applied from `SettingsData`. A real `AudioMixer` with groups and snapshots replaces it at T-061.

### 3.16 Platform / Steam
- `IPlatformService` (achievements, cloud, user name, overlay hooks) in `Farm.Platform`; `NullPlatformService` default, `SteamPlatformService` behind `FARM_STEAM`. **The game must run without Steam.** Achievements are data mapped to Steam API names, unlocked via `EventBus`.

### 3.17 Settings
- `SettingsData` JSON at `persistentDataPath/settings.json`: volumes, resolution, fullscreen, vsync, language, text scale, key binding overrides, **HorrorLevel**. `SettingsStore` loads, clamps and saves atomically.

### 3.18 Extension points and optional layers (ADR 0002)
The horror layer (and any future optional content) plugs into the base game through generic mechanisms. Core code calls them and knows nothing about what they do.

| Mechanism | API | Used for |
|---|---|---|
| Module | `IGameModule`, `GameModules.Register/InitializeAll`, `ModuleContext` (bus, session, hooks, db, settings, `HorrorLevel`) | The entry point of an optional layer, in its own assembly |
| Story state | `GameSession.SetFlag/HasFlag/SetVar/GetVar/AddVar`, `FlagChanged`/`VarChanged`, `GetModuleData<T>/SetModuleData` | Cult standing, dread, knowledge, a module's private data |
| Conditions | `Conditions.Evaluate(expr, session.World)`; atoms `flag var season weather moon map hour day year open`; `Conditions.Register` for more; `Validate` for tools | Gating in data: schedules, dialogue, warps, events, shop stock, quests |
| Day cycle | `IDayCycleHook` (`OnNightFalls`, `OnDawn`), `DayCycleContext` (notes, wake location) | Dreams, blight, offerings, sleepwalking |
| Weather | `IWeatherModifier` chain | Fog, blood moon |
| Atmosphere | `AtmosphereStack.Set(id, tint, strength, priority)` | Mood tinting of the light |
| Text | `L.AddFilter`, `L.AddTable` | Distorted text, strings for new content |
| Content | `ContentPack` under `Resources/Packs` | Extra items and crops |
| Crops | `CropDefinition.GrowCondition`; in-place crop changes from day-cycle hooks | Crops that only grow at certain dread levels; mutating ordinary crops |
| Shops | `ItemDefinition.SoldIn` / `SaleCondition`, `ShopCatalog` | Keeping horror seeds out of the general store; special sellers |
| World objects | `IWorldObjectSource` (kinds `animal`, `plant`, `crafted`), `ItemStack.Mark`, `GameHooks.EnumerateWorldObjects/WorldObjectExists/ConsumeWorldObject` | Choosing, marking and consuming real things in the world (ritual offerings) without knowing how each system stores them |
| Luck | `ILuckModifier`, `GameHooks.ComputeLuck`, `GameSession.Luck` (-1..+1, neutral 0) | Dread making outcomes less favourable in every roll-based system |
| Maps | `GameHooks.MapLoaded`, `Warp.Condition`, `ConditionalObject` | Gated areas, hidden objects, spawning |
| HUD | `GameHooks.AddHudWidget` | Meters and indicators |

**Rules.** Hooks run in registration order sorted by `Order`; each is wrapped so a failure is logged and skipped. Modules must check `HorrorLevel`. Never rename shipped flag, variable, weather or map ids. Story state goes in flags/vars/module data, not new `GameState` fields. `MythosModule` today is inert (it only registers weather names); the horror layer ships with 1.0 (plan Milestone 3b), so it will gain real hooks there.

**Requirements for upcoming systems** are listed in ADR 0002 (weather as data, gated woods path, conditions on dialogue/events/schedules/quests, journal pages, validator coverage, audio layers, the Options control).

## 4. Coding conventions
- C# 9 features OK. `namespace Farm.<Module>`. PascalCase types/methods, `_camelCase` private fields, `camelCase` locals.
- **One MonoBehaviour per file, and the file must have the same name.** Violating this corrupted a scene in player builds while the Editor and tests looked fine (ADR 0001). Plain classes may share files.
- No `FindObjectOfType` / `GameObject.Find` in hot paths; no per-frame allocations in Update (avoid LINQ, string concatenation, `new` lists).
- Use `[SerializeField] private` over public fields. Use `[RequireComponent]` and `OnValidate` for sanity checks.
- No magic strings: item ids / map ids / flags / vars in constants or data. Story ids for optional layers live with that layer (e.g. `MythosIds`).
- User-facing text only through `L.Get`.
- Never edit `.meta` files by hand; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`, `BuildsDev/`. **Always commit `.meta` files with their assets.**
- Prefer creating assets via editor scripts (see `Farm.Editor`) over hand-writing YAML scenes/prefabs; do not hand-edit `.unity`/`.prefab` YAML except for trivial, verified changes.
- Comments only for non-obvious "why".

## 5. Testing strategy
- **EditMode (NUnit, ~209 tests)**: clock/calendar/moon, inventory, farm growth, day cycle, save/migration/backup, settings, input bindings and rebinding, localization lint and hooks, content validation, sprite import rules, conditions language, hooks and modules, atmosphere, content packs, session flags/vars/module data.
- **PlayMode (~17 tests)**: boot to menu, new game, the full farming loop, sleep through the UI (including that the Continue button is on top of the fade), pass-out at 2 AM, warps keep state, save/load, options scrolling, the avatar/cursor alignment, **real simulated keyboard and mouse input** (`InputTestFixture`), and a test module that exercises every extension point in the real game.
- **Player-build checks**: the Editor and tests can miss build-only failures (scene serialization, stripping, draw order). For changes touching scenes, scripts on scenes, or UI layering, also build and run the player with the QA flags in `docs/QA.md` (`-farmScene`, `-farmOpen`, `-farmCapture`) and look at the screenshots.
- **Data validators**: `Farm/Validate Data` (T-040) will check ids, references, localization keys, schedules and every `Condition` string.
- **Tests that must exist for any new hook or layer**: it works with a test module, a failure inside it is isolated, and with `HorrorLevel` 0 behaviour is unchanged (T-044).
- Run: see `docs/BUILD.md` (no `-quit` with `-runTests`). Every task adds or updates tests where logic is testable; when a bug is found, add a test that fails first.

## 6. Tooling for agents
- No Unity MCP server is configured for this project today (`unity-mcp` was unreachable and is not installed in the manifest). Agents work through **headless Editor runs** (`-executeMethod`, `-runTests`) and editor scripts; see `docs/BUILD.md`. If an MCP bridge is added later, prefer it while the Editor is open.
- **Unity CLI** skill `unity:unity-cli` for builds/tests; `unity:unity-package-management` for packages (manifest edits are acceptable only while the Editor is open).
- Related skills: `unity:ui`, `unity:2d-pixel-perfect`, `unity:localization`, `unity:manage-sprite-atlas`, `unity:tilemap-*`, `unity:urp-postprocessing`, `unity:optimize-*`.
- Only one Unity process may open the project at a time; close the Editor before headless runs. QA aids built into the player: `-farmScene`, `-farmOpen`, `-farmCapture` (screenshots plus a `[Perf]` line). **Developer tools** (T-043): a console on F1 and `-farmCommands` exist only in the Editor and development builds (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`); `BuildScript` fails a release build if the debug type names appear in its output (`ReleaseGuard`). Command logic is `DebugCommandProcessor` (Gameplay, unit-tested); the screen is `DebugConsoleScreen` (UI).

## 7. Packages
In use: URP, Input System, 2D (animation, tilemap, tilemap extras, spriteshape, aseprite, psdimporter), Timeline, uGUI + TextMeshPro (essentials imported by `TmpSetup`), Test Framework (+ Input System test framework), Newtonsoft JSON, Visual Studio/Rider IDE, `com.unity.pipeline` (experimental).
Installed but not yet used: Cinemachine (for cutscenes, T-041), Unity Localization (see ADR 0001; backend swap at T-067).
To add later: Yarn Spinner (decision at T-034), Addressables (only if needed), Steamworks.NET (late milestone).
Removed: `com.unity.learn.iet-framework`, the template Welcome folder and sample scene.

## 8. Build and release engineering
- `Farm.Editor.BuildScript`: `BuildWindows`, `BuildLinux` (and `BuildFromCI` for GameCI) with `-scriptingBackend il2cpp|mono` (default Mono), `-development`, `-buildOutput <dir>`; output `Builds/<Windows|Linux>/<version>/`. Version from `PlayerSettings.bundleVersion`. Scenes come from Build Settings (kept in sync by `SceneSetup`).
- IL2CPP release builds for Windows and Linux are verified (Linux cross-compiles with the `com.unity.toolchain.win-x86_64-linux-x86_64` and `com.unity.sysroot.linux-x86_64` packages). Linux builds also need a real-GPU check (Vulkan primary, OpenGL Core fallback); WSLg runs have used OpenGL Core.
- Steam: SteamPipe depots per OS (`app_build.vdf`, `depot_build_windows.vdf`, `depot_build_linux.vdf`), Linux executable bit, launch options per OS, Steam Runtime ("sniper") considerations.
- CI: `.github/workflows/ci.yml` (GameCI tests + Windows/Linux IL2CPP builds). Written, **not yet run** (no remote, no Unity license secrets).
- Source control: git with the Unity `.gitignore` and **Git LFS** for binary art/audio (initialized in M0).

## 9. Placeholder art policy
- Agents must be able to progress without final art. `PlaceholderArtGenerator` writes simple sprites to `Art/Placeholders` (one PNG per sprite, named `<category>_<name>[_<frame>]`, e.g. `crop_parsnip_2`, `player_idle_down`), imported under the rules in 3.14, and `ContentGenerator`/`MapBuilder` wire them into data and scenes. Final art replaces them by name. `AtlasBuilder` (Farm/Setup/Create Sprite Atlases) packs them into six Sprite Atlas V2 assets by name prefix (Tiles, Characters, Crops, Items, World, UI) with pixel-art settings; a test requires every sprite to belong to exactly one group.
- Placeholder crop sprites must stay clearly visible (test-enforced). All art/audio/font sources and licenses are recorded in `docs/ASSET_LICENSES.md`. Only CC0 / properly licensed / owned assets are allowed.

## 10. Performance budgets
- 60 FPS on Steam Deck at 1280x800 (measured today: ~60 fps vsynced on a desktop GPU at 1280x800); < 200 draw calls on a typical map; GC alloc < 1 KB/frame in steady state; map load < 3 s; save write < 200 ms.

## 11. Risks and decisions log (ADRs go in `docs/adr/NNNN-title.md`)
| Risk | Mitigation |
|---|---|
| Scope creep (genre is huge) | Strict milestone gates; vertical slice first; 1.0 content caps in GDD; the horror layer is an optional module |
| Save-compat breakage | Versioned saves + migrations + stable ids; story state in flags/vars/module data |
| Linux-only bugs found late | Build and smoke-test Linux at every milestone end; real hardware check in M4 |
| Build-only failures (serialization, draw order) | Player-build capture checks; one-MonoBehaviour-per-file rule |
| Art volume | Placeholder pipeline; art tracked as separate backlog; consistent specs |
| Steamworks lock-in | `IPlatformService` abstraction; game runs without Steam |
| Agents conflicting edits to scenes/prefabs | Task ownership by area, small scenes, avoid concurrent scene edits |
| Horror content hurts the cozy audience or the age rating | HorrorLevel setting with "off"; content guidelines in the GDD; honest Steam content disclosure and marketing; the cozy game stays complete at "off" |
| 1.0 is large now that the horror layer ships with it | Hooks already built; Milestone 3b scheduled before the RC gate; scope-protection list in the plan (cut breadth, not the core loop); decide cuts before M3 |
| The god's awakening ending feels unfair or hits players who never engaged | Wakefulness reachable only through the player's own choices over a long time, recoverable, foreshadowed, and tested for avoidability (X-010) |
| Hooks rot while nothing uses them | A test module exercises every hook in PlayMode; hook-parity requirements on M2 tasks |
| Horror layer leaks into core code | Separate assembly, dependency rule (nothing depends on Mythos), review rule in CLAUDE.md |
| Lore undecided blocks content | Decisions recorded in GDD section 9; open question F (scope protection) answered before Milestone 3; all other design questions are decided; lore bible X-000 comes first |
