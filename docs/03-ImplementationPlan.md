# Implementation Plan (for AI agentic workers)

Read `CLAUDE.md`, `docs/01-GameDesign.md`, and `docs/02-TechnicalDesign.md` first. This plan is an ordered backlog of **tasks**. Each task has an ID, dependencies, deliverables, and acceptance criteria (AC). Track status in `docs/STATUS.md` (one line per task: `T-xxx | status | agent/date | notes`).

## How agents work this plan

1. Pick the lowest-numbered task whose dependencies are all `done` and which nobody has `in-progress`. Mark it `in-progress` in `docs/STATUS.md` before starting.
2. Work on a branch `task/T-xxx-short-name` (after T-001 initializes git). Keep changes scoped to the task.
3. Definition of Done for **every** task:
   - Project compiles with **zero errors** and no new warnings (check Editor console / batch build log).
   - All EditMode tests pass; new logic has tests.
   - `Farm/Validate Data` passes (once T-040 exists).
   - `.meta` files committed alongside new assets; no `Library/Temp/Logs/UserSettings` committed.
   - No hardcoded paths, strings that should be localized, or Windows-only APIs.
   - Docs updated if behavior/architecture changed (add an ADR for decisions).
   - `docs/STATUS.md` updated; commit message references the task ID.
4. **Concurrency rules**: tasks in different modules/folders may run in parallel. Do **not** edit the same scene/prefab concurrently - scene edits are owned by the task that lists them. Prefer new prefabs over modifying shared ones. Only one process may have the Unity project open: if the Editor is running, use Unity MCP; do not launch a second Unity instance.
5. If a task is ambiguous or blocked, write the question/assumption in `docs/STATUS.md` (and an ADR if architectural) and choose the most conservative option consistent with the design docs; do not expand scope.
6. Every milestone ends with a **milestone gate** task: build Windows + Linux, run the smoke checklist, tag the repo `mN`.

Legend: `[P]` = parallelizable with other tasks of the same milestone once deps are met.

---

## Milestone 0 - Foundation (project hygiene)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-001 | Initialize git, `.gitignore` (Unity), `.gitattributes` + Git LFS for png/psd/aseprite/wav/ogg/mp3/fbx; first commit | - | Repo clean; `Library/` ignored; LFS tracked patterns verified |
| T-002 | Project settings: company/product name, `bundleVersion 0.0.1`, Windows+Linux build targets enabled, IL2CPP scripting backend for standalone, Active Input Handling = Input System, color space Linear, API compat .NET Standard 2.1, default resolution/fullscreen dialog off | T-001 | `ProjectSettings` committed; editor `Linux Build Support (IL2CPP)` module presence noted in `docs/BUILD.md` |
| T-003 | Create folder layout + **asmdefs** per Tech Design s2 (Core, Data, Gameplay, UI, Platform, Editor, Tests.EditMode, Tests.PlayMode) | T-001 | Compiles; dependency direction enforced; sample passing test in each test asmdef |
| T-004 | Remove template cruft (`Assets/Welcome`, `SampleScene`, iet-framework, `Unity.2D.Welcome.csproj` refs); add packages: Localization, Newtonsoft JSON, Cinemachine | T-003 | Clean compile; manifest updated via Package Manager API/Editor |
| T-005 | `docs/BUILD.md` and **BuildScript** (`BuildWindows`, `BuildLinux`) + CI workflow (GameCI) running EditMode tests and building both targets | T-003 | `Builds/Windows` and `Builds/Linux` produced from CLI; documented commands; Linux build verified to launch (VM/WSL2/Deck or documented limitation) |
| T-006 | Bootstrap scene + `ServiceLocator`, `EventBus`, `SceneLoader` (fade), logging wrapper | T-003 | Boot -> MainMenu placeholder; EditMode tests for EventBus; PlayMode smoke |
| T-007 | Pixel Perfect Camera + URP 2D renderer tuned (480x270 ref, PPU 16, point filter defaults via `TextureImporter` preset), test scene with grid | T-004 | Visual test: no shimmer when moving at 1x/2x/3x/4x; use `unity:2d-pixel-perfect` skill |
| T-008 | Placeholder art generator editor tool (tiles, player, NPC, crops, items, UI frames) following naming conventions; `docs/ASSET_LICENSES.md` | T-003 | `Farm/Generate Placeholder Art` creates sliced sprites + sprite atlases |
| **T-009** | **Milestone 0 gate** | all M0 | Both platform builds start into Bootstrap -> MainMenu; tag `m0` |

## Milestone 1 - Vertical slice core (player, time, farming, save)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-010 | Input: extend `InputSystem_Actions` (Gameplay/UI/Dialogue maps), `InputService` wrapper, rebinding persisted in settings | T-006 | Keyboard+gamepad both drive all actions; EditMode test for binding persistence |
| T-011 | `GameClock` + calendar (`GameDateTime`, seasons, day/night events, pause stack) with tests | T-006 | Unit tests for rollover (day/season/year), pause ref-count; debug overlay |
| T-012 [P] | Item system: `ItemDefinition` SOs, `ItemDatabase`, `ItemStack`, `Inventory` + tests; ID validation | T-003 | 100% logic tested (stack limits, overflow, swap) |
| T-013 | Player controller: 4/8-dir movement, collision, animation (placeholder), facing, y-sorting, camera follow w/ Cinemachine + map bounds | T-007, T-010 | Walks around test map at 60 FPS, no tunneling, gamepad/keyboard |
| T-014 | World/Map framework: Tilemap layer convention, `MapId`, `MapRuntimeState`, warps with fade, spawn points; Farm + FarmHouse interior placeholder maps | T-013 | Walk between farm and house; state persists across warps |
| T-015 | Tool system + hotbar + energy: Hoe, Watering Can, Axe, Pickaxe, Scythe; `IToolAction`; tile targeting cursor | T-012, T-014 | Till/water tiles with energy cost; hotbar select via keys/scroll/gamepad |
| T-016 | Farming: `CropDefinition`, `FarmTileState`, planting, watering, daily growth, harvesting, seasons kill crops; 6 spring crops | T-011, T-015 | Tests for growth/regrow/out-of-season; visual crops render from tile state |
| T-017 | Day cycle: sleep in bed -> day summary (shipping total) -> new day; pass-out at 02:00; day/night lighting gradient | T-011, T-014 | Full day loop playable; lighting smooth |
| T-018 | HUD + Inventory/backpack UI (uGUI, gamepad navigable), tooltips, item pickup popups | T-012, T-015 | Drag/drop (mouse), move (gamepad); localized strings |
| T-019 | Shipping bin + sell flow + gold; General Store shop UI (buy seeds) | T-018, T-016 | Earn gold from crop -> buy seeds -> replant |
| T-020 | **Save system**: `GameState`, JSON, 3 slots, atomic write, `.bak`, autosave on sleep, migration framework, load into correct map/time | T-016, T-017 | Roundtrip tests (inventory, tiles, crops, time, gold); manual save/load in game; corrupt-file fallback to `.bak` |
| T-021 | Main menu (new/continue/load/options/quit), options (audio, display, keybinds, language), `SettingsData` | T-010, T-020 | Settings persist; resolution/fullscreen work on both OS |
| T-022 | Localization setup: String Tables (en), helper `L.Get(key)`; lint test: no raw user-facing strings in UI prefabs | T-018 | All existing UI text localized |
| T-023 | Basic audio service + mixer + placeholder SFX/music hooks | T-006 | Volume sliders affect groups |
| **T-024** | **M1 gate - Vertical Slice**: new game -> farm for a spring season -> sell -> save/load -> quit; builds on Win+Linux; perf check; tag `m1` | all M1 | Smoke checklist in `docs/QA.md` passes on both OS |

## Milestone 2 - World and living town

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-030 | Weather system (sunny/rain/storm/snow/wind): daily roll, forecast, VFX, auto-water, lighting | T-017 | Deterministic with seed; tests |
| T-031 | Town, Forest, Beach maps + 6 building interiors (placeholder art, correct colliders, warps, ambience) | T-014 | All warps round-trip; no stuck spots |
| T-032 | Remaining tool/skill system: skills + XP + levels, tool upgrades (blacksmith), energy upgrades, backpack upgrades, Scythe/axe/pickaxe interactions with trees/rocks/weeds | T-015 | Skill XP persisted; upgrade flow end-to-end |
| T-033 | Foraging + resource nodes spawn per season/map; farm clutter regrowth | T-031 | Data-driven spawn tables; saved state |
| T-034 | Dialogue system (Yarn Spinner ADR first) + portrait dialogue UI + choices + localization tie-in | T-022 | Test dialogue with branches and flags |
| T-035 | NPC framework: `NpcDefinition`, schedules, A* pathfinding, off-screen simulation, `FriendshipState`, talk, gifts (loved/liked/neutral/disliked), birthdays | T-031, T-034 | 3 NPCs with full-week schedules; tests for schedule resolution |
| T-036 | Calendar UI, Social tab, Map screen, Skills page, Journal/quests UI | T-035, T-032 | Gamepad navigable |
| T-037 | Crafting + cooking + recipes (learned via skills/friendship), chests, furnace, keg, preserves jar (timed machines in world time, saved) | T-032 | Machines process across day boundaries; tests |
| T-038 | Remaining crops (24 + 4 trees), quality system, fertilizer, sprinklers, scarecrow, greenhouse | T-016, T-037 | Data complete; growth tests per crop |
| T-039 | Quest/flag system + mailbox letters + Help Wanted board + tutorial quest chain (first 3 days) | T-034 | Quests persisted; tutorial teaches loop |
| T-040 | **Data validator** (`Farm/Validate Data` + test): ids unique, refs resolve, localization keys exist, sprites assigned, schedules valid | T-035 | Fails build on invalid data |
| T-041 | Event/cutscene engine (actor moves, camera, emotes, dialogue, give item, set flag) + 1 sample heart event | T-035 | Data-driven script file plays; skip works |
| **T-042** | **M2 gate**: play through a full year with 3 NPCs; no softlocks; builds Win+Linux; tag `m2` | all M2 | `docs/QA.md` extended |

## Milestone 3 - Adventure content (mine, fishing, combat, animals)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-050 | Fishing: rod, cast, bite, timing mini-game, ~20 fish (location/season/time/weather), fish shop, bait/tackle (basic) | T-032, T-031 | Data-driven fish tables; tests for catch eligibility |
| T-051 | Mine generator (seeded) + floor transitions, ore/rock/gem nodes, ladders/stairs, elevator checkpoints | T-032 | Determinism tests; 40 floors reachable |
| T-052 | Combat: health, weapons (sword/dagger/club), enemy base FSM, 5 enemy types, drops, knockback, death penalty, boss at floor 40 | T-051 | PlayMode test: enemy kill -> drop; balance data in SOs |
| T-053 | Farm animals: coop/barn building (carpenter), feeding/petting, products, hay/silo | T-031, T-037 | Daily loop works and saves |
| T-054 | Remaining NPCs to 12 (data, schedules, dialogue, gifts, 3-4 heart events each) - **content tasks split per NPC (T-054a..l)** | T-041 | Each NPC validated by T-040 |
| T-055 | Community Hall bundles (6 rooms), rewards, story ending sequence | T-039 | Completable end-to-end |
| T-056 | Festivals (4) with scripted events/mini-games using cutscene engine | T-041 | Calendar-driven; saved participation flags |
| T-057 | Traveling merchant, collections tab, shipping stats, skill professions | T-032 | - |
| **T-058** | **M3 gate**: content-complete alpha; full 2-year playthrough; perf pass; builds Win+Linux; tag `m3` | all M3 | Perf budgets from Tech Design s10 met |

## Milestone 4 - Polish and platform

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-060 | Final art integration pass (swap placeholders by name; atlases; animations; lighting polish) | art ready | No missing sprites; visual QA on both OS |
| T-061 | Audio pass: music per season/location, ambience, full SFX set | audio ready | Mixer snapshots; no clipping |
| T-062 | Steamworks integration (`FARM_STEAM`): init, achievements (~30), cloud saves via Auto-Cloud, overlay-safe pause, Steam Input glyphs | T-020 | Game still runs without Steam; verified with Steam client on Win+Linux |
| T-063 | Accessibility + controller polish: text scale, colorblind aids, full gamepad coverage, Steam Deck layout (1280x800), on-screen keyboard for names | M3 | Steam Deck checklist |
| T-064 | Performance optimization (profiling, atlases, pooling, GC), load-time pass; use `unity:optimize-*` skills | M3 | Budgets met; report in `docs/PERF.md` |
| T-065 | Save-compat hardening: migration tests with archived saves from every milestone tag; crash/exception handler + log file | T-020 | Old saves load |
| T-066 | Balance pass (economy, XP, energy, time), difficulty/QoL options | M3 | Spreadsheet in `docs/balance/` |
| T-067 | Localization-ready audit; (optional) additional languages | T-022 | - |
| T-068 | Bug-fix burn-down from QA; soak tests (automated "bot plays 1 year") | M3 | Zero known S1/S2 bugs |
| **T-069** | **M4 gate - Release Candidate** | all M4 | Release checklist below |

## Milestone 5 - Steam release

| ID | Task | Deps | Deliverables |
|---|---|---|---|
| T-070 | Steamworks partner setup (human task): App ID, depots, branches | human | `app_build.vdf`, depot VDFs for Windows and Linux in `Steam/` |
| T-071 | SteamPipe upload scripts + `docs/RELEASE.md`; test on `beta` branch on both OSes (incl. Steam Deck) | T-070 | Download -> install -> play verified |
| T-072 | Store page assets, trailer, screenshots, capsules, description (human + agent drafts), age rating, EULA/privacy | - | Checklist complete |
| T-073 | Demo build (optional, spring only) | T-069 | Separate app/branch |
| T-074 | Release, launch-week hotfix process, patch pipeline, `CHANGELOG.md` | T-071 | Live |

## Release checklist (T-069 / T-074)
- [ ] Win + Linux IL2CPP release builds from CI, version stamped
- [ ] Full playthrough (1 year, all systems) on both OS, no crashes
- [ ] Fresh-install + upgrade-from-prior-save tested
- [ ] Steam achievements/cloud verified on both OS; offline mode OK
- [ ] 60 FPS on Steam Deck target; memory < 2 GB
- [ ] No placeholder art/audio/text left; all asset licenses documented
- [ ] Controller-only and keyboard-only playthroughs
- [ ] Localization keys complete; no missing-key strings visible
- [ ] Crash log location documented; no debug UI in release

## Suggested first agent actions (start here)
1. T-001, then T-002 + T-003 (sequential), then T-004.
2. In parallel after T-003: T-005 (build/CI), T-008 (placeholders), T-006 (bootstrap).
3. Then T-007, T-010, T-011, T-012 in parallel; converge on T-013/T-014.
