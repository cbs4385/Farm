# Farm - agent instructions

A cozy farming/life sim (original content) for **Steam on Windows and Linux**, with a cosmic-horror layer that **ships with 1.0** (a New England village whose cult keeps a sleeping Elder God in the woods from waking). The horror is player-tunable (intensity off/mild/full, default full) and the cozy game must stay complete at "off". Unity 6000.6.2f1, URP 2D, Input System. Milestones 0 to 3 and 3b are built (see `docs/STATUS.md` for what is partial and what no person has played yet); `docs/mythos/M3b-COMPLETION-PLAN.md` lists what is left of the horror layer, then polish and release.

## Read first
1. `docs/README.md` - index of all documents.
2. `docs/01-GameDesign.md` - what we are building (scope caps and content guidelines are binding; open questions in section 9).
3. `docs/02-TechnicalDesign.md` - architecture as built, conventions, tooling.
4. `docs/03-ImplementationPlan.md` - ordered task backlog and Definition of Done.
5. `docs/adr/` - decisions: `0001` deviations from the original design, `0002` extension points for the horror layer.
6. `docs/STATUS.md` - current task status. Claim a task there before starting.
7. `docs/BUILD.md` (commands) and `docs/QA.md` (checks) before running or verifying anything.

## Non-negotiables
- Original assets, names, text only. Never copy Stardew Valley content, or text/characters from existing Lovecraft-inspired works.
- Data-driven (ScriptableObjects/JSON), game state in plain serializable C#, assembly definitions per module.
- Stable string ids for items/maps/NPCs/flags/variables/weather; never rename once shipped (save compatibility).
- Linux-safe: `Application.persistentDataPath`, case-correct paths, no Windows-only APIs. The game must run without Steam.
- Commit `.meta` files with assets; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`, `BuildsDev/`.
- Every task leaves the project compiling with all tests green. Add tests for logic; a bug fix needs a test that failed first.
- **One MonoBehaviour per file, named like the file.** (Breaking this corrupted a scene in player builds only.)
- User-facing text only through `L.Get(key)` with keys in `Resources/Localization/en.json`.
- Do not hand-edit scene/prefab YAML; generate them with the editor scripts (`SceneSetup`, `MapBuilder`, `ContentGenerator`, ...).
- When you change scenes, scene components, UI layering or serialization, also build the Windows player and look at capture screenshots (`docs/QA.md`): the Editor and tests can miss build-only problems.

## Extension points and the horror layer (ADR 0002; lore in `docs/mythos/LORE.md`)
- Two meters: the god's **wakefulness** (world) and the player's **dread** (personal). Horror crops and rare goods are never sold by the general store (items opt in to shops via `SoldIn`).
- Optional layers plug in through generic hooks: conditions, flags/vars, module data, day-cycle hooks, weather modifiers, atmosphere layers, text filters, content packs, map hook, gated warps, conditional objects, HUD widgets, modules.
- **Do not put horror/cult content in core assemblies** (`Farm.Core/Data/Gameplay/UI`). It belongs in `Farm.Mythos` and data. Nothing may depend on `Farm.Mythos`.
- NPC, dialogue, event, quest, weather, schedule and map work must use conditions and the hooks, not hard-coded checks.
- Respect `SettingsData.HorrorLevel` (0 off, 1 mild, 2 full) in every module; at 0 the game must behave exactly like the base game.
- Hooks must be failure-isolated and tested; new story state goes in flags/vars/module data, not new `GameState` fields.

## Unity operation
- Only one Unity instance may open this project. Close the Editor before headless runs (`Temp/UnityLockfile` and `Unity.exe` show whether it is open).
- No Unity MCP server is configured for this project; work through headless Editor runs and editor scripts (`docs/BUILD.md`). Skills: `unity:unity-cli`, `unity:unity-package-management`. Package manifest edits are acceptable only while the Editor is open.
- Tests: `-runTests -testPlatform EditMode` (add `-nographics`) and `PlayMode` (needs graphics); never pass `-quit` with `-runTests`.
- Player QA flags: `-farmScene`, `-farmOpen`, `-farmCapture` (see `docs/QA.md`). For hard-to-reach states build with `-development` (output `BuildsDev/`) and use the developer console (F1) or `-farmCommands`; never let developer tools into a release build (the build guard fails if they do).

## Conventions
See Tech Design section 4. Namespaces `Farm.<Module>`; private fields `_camelCase`; no per-frame allocations; no magic strings; comments only for non-obvious "why".

## Working with the owner
The owner playtests by hand and reports problems; reproduce first, fix, and add a test (ideally with simulated real input). Keep `docs/STATUS.md` honest about what was verified by a person versus by automation.
