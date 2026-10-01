# Farm - agent instructions

A Stardew-Valley-style farming/life sim (original content) for **Steam on Windows and Linux**. Unity 6000.6.2f1, URP 2D, Input System.

## Read first
1. `docs/01-GameDesign.md` - what we are building (scope caps are binding).
2. `docs/02-TechnicalDesign.md` - architecture, conventions, tooling.
3. `docs/03-ImplementationPlan.md` - ordered task backlog and Definition of Done.
4. `docs/STATUS.md` - current task status (create if missing). Claim a task here before starting.

## Non-negotiables
- Original assets, names, text only. Never copy Stardew Valley content.
- Data-driven (ScriptableObjects/JSON), game state in plain serializable C#, assembly definitions per module.
- Stable string IDs for items/maps/NPCs/flags; never rename once shipped (save compatibility).
- Linux-safe: `Application.persistentDataPath`, case-correct paths, no Windows-only APIs. Game must run without Steam.
- Commit `.meta` files with assets; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Every task leaves the project compiling with tests green. Add tests for logic.
- Do not hand-edit scene/prefab YAML unless trivial; use Unity MCP or editor scripts.

## Unity operation
- Only one Unity instance may open this project. If the Editor is open (`Temp/UnityLockfile`, `Unity.exe` running), use the Unity MCP server (`unity-mcp`); if it is not connected, ask the user to start it rather than launching a second instance.
- Headless runs: use the `unity:unity-cli` and `unity:unity-package-management` skills. Package installs via manifest edit are acceptable only while the Editor is open (it resolves them).
- Tests: EditMode first (`-runTests -testPlatform EditMode`, no `-quit` problems documented in the skill).

## Conventions
See Tech Design section 4. Namespaces `Farm.<Module>`; private fields `_camelCase`; no per-frame allocations; no magic strings.

## Extension points (planned horror layer)
Read `docs/adr/0002-mythos-extension-points.md` before touching NPCs, dialogue, events, weather, maps or the day cycle. Use `Conditions`, flags/vars, hooks and content packs instead of hard-coding; keep horror content in `Farm.Mythos`; respect `SettingsData.HorrorLevel`.
