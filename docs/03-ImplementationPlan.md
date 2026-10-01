# Implementation Plan (for AI agentic workers)

Read `CLAUDE.md`, `docs/01-GameDesign.md`, `docs/02-TechnicalDesign.md`, and the ADRs in `docs/adr/` first. This plan is an ordered backlog of **tasks**. Each task has an ID, dependencies, deliverables, and acceptance criteria (AC). Track status in `docs/STATUS.md` (one line per task: `T-xxx | status | agent/date | notes`).

**Where we are:** Milestones 0 and 1 are complete (git tags `m0`, `m1`); the extension points for the optional horror layer (ADR 0002) are built. **Next: Milestone 2.** Task rows for finished milestones keep their original wording with an "As built" note where the result differs; see `STATUS.md` for details and `adr/0001-m1-design-deviations.md` for why.

## How agents work this plan

1. Pick the lowest-numbered task whose dependencies are all `done` and which nobody has `in-progress`. Mark it `in-progress` in `docs/STATUS.md` before starting.
2. Work on a branch `task/T-xxx-short-name`. Keep changes scoped to the task.
3. Definition of Done for **every** task:
   - Project compiles with **zero errors** and no new warnings (Editor console / batch build log).
   - All EditMode and PlayMode tests pass; new logic has tests; a bug fix has a test that failed before the fix.
   - `Farm/Validate Data` passes (once T-040 exists).
   - `.meta` files committed alongside new assets; no `Library/Temp/Logs/UserSettings/Builds` committed.
   - No hardcoded paths, no user-facing text outside `L.Get`, no Windows-only APIs.
   - **One MonoBehaviour per file, named like the file.**
   - If the task touches scenes, scene components, UI layering or serialization: **build the Windows player and run the capture checks** in `docs/QA.md` (the Editor and tests can miss build-only failures). For input-driven features, add a test that uses simulated real input.
   - If it touches NPCs, dialogue, events, weather, maps, the day cycle or story state: it uses **conditions, flags/vars and the hooks** (ADR 0002) rather than hard-coding, keeps horror-specific content out of core assemblies, and respects `HorrorLevel`.
   - Docs updated if behavior/architecture changed (add an ADR for decisions).
   - `docs/STATUS.md` updated; commit message references the task ID.
4. **Concurrency rules**: tasks in different modules/folders may run in parallel. Do **not** edit the same scene/prefab concurrently - scene edits are owned by the task that lists them. Prefer new prefabs over modifying shared ones. Only one Unity process may open the project; close the Editor before headless runs.
5. If a task is ambiguous or blocked, write the question/assumption in `docs/STATUS.md` (and an ADR if architectural) and choose the most conservative option consistent with the design docs; do not expand scope. Open design questions live in GDD section 9.
6. Every milestone ends with a **milestone gate** task: build Windows + Linux, run the automated and manual checks in `docs/QA.md`, tag the repo `mN`.

Legend: `[P]` = parallelizable with other tasks of the same milestone once deps are met.

---

## Milestone 0 - Foundation (DONE, tag `m0`)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-001 | Initialize git, `.gitignore` (Unity), `.gitattributes` + Git LFS for png/psd/aseprite/wav/ogg/mp3/fbx; first commit | - | Repo clean; `Library/` ignored; LFS tracked patterns verified |
| T-002 | Project settings: company/product name, `bundleVersion 0.0.1`, Windows+Linux targets, scripting backend, Input System, Linear color space, .NET Standard 2.1 | T-001 | `ProjectSettings` committed. As built: Mono for dev, IL2CPP via build flag; applied by `ProjectConfigurator` |
| T-003 | Folder layout + **asmdefs** per Tech Design s2 | T-001 | Compiles; dependency direction enforced; sample tests |
| T-004 | Remove template cruft; add Localization, Newtonsoft JSON, Cinemachine | T-003 | Clean compile |
| T-005 | `docs/BUILD.md`, **BuildScript** (`BuildWindows`, `BuildLinux`), CI workflow | T-003 | As built: Mono builds verified; IL2CPP and CI **not yet verified** |
| T-006 | Bootstrap scene + `ServiceLocator`, `EventBus`, `SceneLoader` (fade), logging | T-003 | Boot -> MainMenu; tests |
| T-007 | Pixel Perfect Camera, point-filter import rules, test scene | T-004 | Verified at 1x-4x on Windows and Linux; `PixelSnapCamera` added |
| T-008 | Placeholder art generator following naming conventions; `docs/ASSET_LICENSES.md` | T-003 | As built: individual PNGs (atlases later) |
| **T-009** | **M0 gate** | all M0 | Both builds boot; tag `m0` |

## Milestone 1 - Vertical slice (DONE, tag `m1`)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-010 | Input: actions, `InputService`, rebinding persisted | T-006 | As built: generated asset (ADR 0001); gamepad untested on hardware |
| T-011 | `GameClock` + calendar (pause stack, pass-out) | T-006 | Done; debug overlay moved to T-043 |
| T-012 [P] | Item system: definitions, database, stacks, inventory | T-003 | Done |
| T-013 | Player controller, camera follow, bounds | T-007, T-010 | As built: custom camera instead of Cinemachine (ADR 0001) |
| T-014 | Map framework: layers, warps, spawn points; Farm + FarmHouse | T-013 | Done |
| T-015 | Tools + hotbar + energy | T-012, T-014 | Hoe, watering can, seeds work; axe/pickaxe/scythe wait for T-032 |
| T-016 | Farming: planting, watering, growth, harvest, seasons; 6 spring crops | T-011, T-015 | Done |
| T-017 | Day cycle: sleep, summary, pass-out, lighting | T-011, T-014 | Done (sunny/rain only; T-030 extends) |
| T-018 | HUD + backpack UI | T-012, T-015 | As built: click-to-move instead of drag/drop |
| T-019 | Shipping bin, selling, shop | T-018, T-016 | As built: temporary stall on the farm until T-031 |
| T-020 | Save system: slots, atomic writes, `.bak`, migrations, autosave | T-016, T-017 | Done |
| T-021 | Main menu, options, settings | T-010, T-020 | Done; HorrorLevel control arrives with X-009 |
| T-022 | Localization: string tables, `L.Get`, lint tests | T-018 | As built: in-house table (ADR 0001) |
| T-023 | Audio service | T-006 | As built: logical buses + placeholder blips |
| **T-024** | **M1 gate** | all M1 | Done; tag `m1` |

## Extension points (DONE, ADR 0002)
Conditions language, moon phase, flags/vars/module data, day-cycle hooks, weather modifiers, atmosphere layers, text filters, content packs, map-loaded event, conditional warps/objects, HUD widgets, module system, inert `Farm.Mythos` skeleton, `HorrorLevel` setting. Tested in EditMode and PlayMode.

## Milestone 2 - World and living village (NEXT)
Every task below must also meet the extension-point rule in the Definition of Done. The "Hooks" column states what that means for the task.

| ID | Task | Deps | Deliverables / AC | Hooks (ADR 0002) |
|---|---|---|---|---|
| **T-043** | **Developer/QA tools** (development builds only): in-game console or hotkeys to skip time (hour/day/season), set weather, set/clear flags and variables, give items/gold, teleport; launch flag `-farmDebug`; stripped from release | T-011 | Season change, rain, passing out, flags testable in seconds; excluded from release builds (test) | Set flags/vars through `GameSession` |
| T-030 | Weather system (sunny/rain/storm/snow/wind): **weather as data** (`WeatherDefinition`: id, name key, tint, particles, waters crops...), deterministic roll, forecast, VFX, lighting from the definition | T-017 | Deterministic with seed; tests; no weather special cases in lighting code | Modifiers and new weather definitions can be added by modules |
| T-031 | Village, Forest, Beach maps + 6 building interiors (placeholder art, colliders, warps, ambience); move the general store into the village | T-014 | All warps round-trip; no stuck spots | Forest has a condition-gated warp slot for a future Woods map (closed in the base game); map ids reserved |
| T-032 | Skills + XP + levels, tool upgrades, energy/backpack upgrades, axe/pickaxe/scythe interactions with trees/rocks/weeds | T-015 | Skill XP persisted; upgrade flow end-to-end | - |
| T-033 | Foraging + resource nodes; farm clutter regrowth | T-031 | Data-driven spawn tables; saved state | Spawn tables accept conditions |
| T-034 | Dialogue system (Yarn Spinner ADR first) + portrait UI + choices | T-022 | Test dialogue with branches and flags | Lines/choices accept `Condition`; can set flags/vars; text via `L.Get` (filters apply); a `Dialogue` input map |
| T-035 | NPC framework: definitions, schedules, A* pathing, off-screen simulation, friendship, talk, gifts, birthdays | T-031, T-034 | 3 NPCs with full-week schedules; tests for schedule resolution | Schedule entries, dialogue sets, gift reactions accept `Condition`; optional allegiance field (unused by the base game) |
| T-036 | Calendar UI, Social tab, Map screen, Skills page, Journal/quests UI | T-035, T-032 | Gamepad navigable | Journal supports extra pages from modules |
| T-037 | Crafting + cooking + recipes, chests, furnace, keg, preserves jar | T-032 | Machines process across day boundaries; tests | Recipes accept conditions |
| T-038 | Remaining crops (24 + 4 trees), quality, fertilizer, sprinklers, scarecrow, greenhouse | T-016, T-037 | Data complete; growth tests per crop | Crop data supports content packs |
| T-039 | Quest/flag system + mailbox letters + Help Wanted board + tutorial chain | T-034 | Quests persisted; tutorial teaches the loop | Availability accepts `Condition` |
| T-040 | **Data validator** (`Farm/Validate Data` + test): ids unique, refs resolve, string keys exist, sprites assigned, schedules valid | T-035 | Fails build on invalid data | Validates every `Condition` string (`Conditions.Validate`), warp/object conditions in scenes, and string keys from modules |
| T-041 | Event/cutscene engine + 1 sample heart event | T-035 | Data-driven script plays; skip works | Events have a `Condition` and "once" flag; steps can set flags/vars |
| **T-044** | **Hook conformance and HorrorLevel-0 equivalence tests**: scripted playthrough with test modules registered vs. none (and `HorrorLevel` 0) produces identical game state; every hook still exercised in PlayMode | T-035 | Tests in CI; extended at each later gate | Enforces GDD success criterion 6 |
| **T-042** | **M2 gate**: a full year with 3 NPCs; no softlocks; builds Win+Linux; tag `m2` | all M2 | `docs/QA.md` extended; T-044 passes | - |

## Milestone 3 - Adventure content (mine, fishing, combat, animals)

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-050 | Fishing: rod, cast, bite, timing mini-game, ~20 fish, fish shop, bait/tackle | T-032, T-031 | Data-driven tables with `Condition` (night, weather, moon); catch-eligibility tests |
| T-051 | Mine generator (seeded) + floors, nodes, ladders, elevators | T-032 | Determinism tests; 40 floors reachable |
| T-052 | Combat: health, weapons, enemy FSM, 5 enemy types, boss at floor 40 | T-051 | PlayMode test: kill -> drop; balance in data |
| T-053 | Farm animals: coop/barn, feeding/petting, products | T-031, T-037 | Daily loop works and saves |
| T-054 | Remaining NPCs to 12 (**split per NPC, T-054a..l**): data, schedules, dialogue, gifts, heart events | T-041 | Each validated by T-040; allegiance/secret fields filled only as data and only when the lore is decided |
| T-055 | Community Hall bundles (6 rooms), rewards, story ending | T-039 | Completable; its relation to the mythos arc follows GDD open question 2 |
| T-056 | Festivals (4) using the cutscene engine | T-041 | Calendar-driven; participation flags saved; accept conditions for variants |
| T-057 | Traveling merchant, collections tab, shipping stats, professions | T-032 | - |
| **T-058** | **M3 gate**: content-complete alpha; 2-year playthrough; perf pass; builds Win+Linux; tag `m3` | all M3 | Perf budgets met; T-044 still passes |

## Milestone 4 - Polish and platform

| ID | Task | Deps | Deliverables / AC |
|---|---|---|---|
| T-060 | Final art integration pass (swap placeholders by name; atlases; animations; lighting polish) | art ready | No missing sprites; visual QA on both OS |
| T-061 | Audio pass: music per season/location, ambience, full SFX set, real `AudioMixer` | audio ready | Mixer snapshots; music/ambience layers switchable by condition or mood layer |
| T-062 | Steamworks integration (`FARM_STEAM`): achievements, Auto-Cloud, overlay-safe pause, Steam Input glyphs | T-020 | Runs without Steam; verified with Steam on Win+Linux; achievement text spoiler-free |
| T-063 | Accessibility + controller polish: UI size, colorblind aids, full gamepad coverage, Steam Deck layout, on-screen keyboard | M3 | Steam Deck checklist |
| T-064 | Performance optimization; use `unity:optimize-*` skills | M3 | Budgets met; `docs/PERF.md` |
| T-065 | Save-compat hardening: migrations tested with archived saves from every milestone tag, **including flags/vars/module data**; crash handler + log file | T-020 | Old saves load |
| T-066 | Balance pass (economy, XP, energy, time), difficulty/QoL options | M3 | `docs/balance/` |
| T-067 | Localization audit; swap the `L` backend to Unity Localization if more languages are wanted | T-022 | - |
| T-068 | Bug-fix burn-down; soak tests ("bot plays 1 year") | M3 | Zero known S1/S2 bugs |
| **T-069** | **M4 gate - Release Candidate** | all M4 | Release checklist below |

## Milestone 5 - Steam release

| ID | Task | Deps | Deliverables |
|---|---|---|---|
| T-070 | Steamworks partner setup (human): App ID, depots, branches | human | VDFs for Windows and Linux in `Steam/` |
| T-071 | SteamPipe upload scripts + `docs/RELEASE.md`; test on `beta` on both OSes (incl. Steam Deck) | T-070 | Download -> install -> play verified |
| T-072 | Store page assets, trailer, screenshots, description, age rating, EULA/privacy, **content survey and intensity-setting description if the horror layer ships** | - | Checklist complete |
| T-073 | Demo build (optional, spring only) | T-069 | Separate app/branch |
| T-074 | Release, hotfix process, patch pipeline, `CHANGELOG.md` | T-071 | Live |

## Release checklist (T-069 / T-074)
- [ ] Win + Linux IL2CPP release builds from CI, version stamped
- [ ] Full playthrough (1 year, all systems) on both OS, no crashes
- [ ] Fresh-install + upgrade-from-prior-save tested
- [ ] Steam achievements/cloud verified on both OS; offline mode OK
- [ ] 60 FPS on Steam Deck target; memory < 2 GB
- [ ] No placeholder art/audio/text left; all asset licenses documented (`ASSET_LICENSES.md`)
- [ ] Controller-only and keyboard-only playthroughs
- [ ] Localization keys complete; no missing-key strings visible
- [ ] Crash log location documented; no debug UI or developer tools (T-043) in release
- [ ] Equivalence test (T-044) passes; if the horror layer ships: playthroughs at HorrorLevel 0, 1 and 2, intensity option and content notes present, store content survey filled

## Milestone 6 - Mythos layer (outline; after 1.0 or as an update; see GDD open question 1)
All work lives in `Farm.Mythos` and data; respect `HorrorLevel` everywhere. **Decision gates first:** answer GDD section 9 (at least questions 1-4) and write the lore bible before implementing content.

| ID | Task | Deps |
|---|---|---|
| X-000 | Lore bible and content boundaries (`docs/mythos/LORE.md` finished; setting, god, cult, paths, NPC roles, endings) | GDD s9 answers |
| X-001 | Dread and lore variables, dread meter HUD widget, dread-driven atmosphere layers | M2, X-000 |
| X-002 | Woods map(s) and gated entry; map-loaded hooks for fog, sounds, hidden objects | T-031 |
| X-003 | Cult NPC secrets: allegiances, hidden schedules, conditional dialogue, heart-event variants (per NPC) | T-035, T-041 |
| X-004 | Offerings, rituals and forbidden items (content pack), cult hall maps | T-037 |
| X-005 | Night events and dreams (day-cycle hooks), sleepwalking, blight on crops | T-017 |
| X-006 | Fog and blood-moon weather; moon-phase events | T-030 |
| X-007 | Mutated and strange crops (content pack) | T-038 |
| X-008 | Text distortion, audio and visual distortion at high dread | T-061 |
| X-009 | HorrorLevel option in Options, content notes, safe-mode checks (level 0 changes nothing, level 1 removes the most disturbing text/imagery); **must land before any content ships** | T-021 |
| X-010 | Endings (resist / ignore / join), balance, QA at all three levels, store/content-survey updates | X-001..X-009 |

## Suggested next agent actions (start here)
1. **T-043** (developer/QA tools) first: it makes every later check faster (skip days, set flags).
2. In parallel: **T-030** (weather as data), **T-031** (village maps) and **T-034** (dialogue ADR + system).
3. Then **T-035** (NPC framework, needs T-031 and T-034), with **T-040** (validator) growing alongside, **T-044** as soon as NPCs exist, and T-036/T-039/T-041 after that.
4. Update `docs/STATUS.md` and the QA checklist as each lands.
