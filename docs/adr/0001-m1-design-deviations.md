# ADR 0001: Milestone 1 deviations from the design docs

Status: accepted (2026-10-01). Revisit at the milestone noted for each item.

| Area | Design said | M1 does | Why / revisit |
|---|---|---|---|
| Camera (T-013) | Cinemachine follow + confiner | Custom `CameraFollow` (map-bounds clamp using the real ortho size) + `PixelSnapCamera` | Pixel snapping and PixelPerfectCamera interplay was verified with a few lines of code; Cinemachine is installed for cutscenes (T-041). |
| Localization (T-022) | Unity Localization string tables | `L.Get(key)` over `Resources/Localization/en.json`, with a lint test that every key exists and no UI literal text is used | Unity Localization needs Addressables content builds in players; not worth the risk for one language. Swap the backend behind `L` in T-067. |
| Audio (T-023) | AudioMixer groups | Logical buses (master/music/sfx/ambience) as volume multipliers; generated placeholder blips | No audio assets exist. Real mixer + snapshots in T-061. |
| Inventory UI (T-018) | Drag/drop for mouse | Mouse drag and drop (`InventorySlotDrag`), plus click-to-pick then click-to-place for keyboard and gamepad | Resolved: drag/drop added on top of the shared move path. |
| Input (T-010) | Extend `InputSystem_Actions` | Generated `Resources/FarmInput.inputactions` (editor tool); template asset left unused | Reproducible from code; bindings reviewed in `InputAssetGenerator`. |
| UI authoring | uGUI + TMP | Built in code via `UiKit` (no prefabs) | Avoids hand-written YAML; all controls are Selectables so gamepad navigation works. |
| General store (T-019) | In town | A "temporary stall" next to the shipping bin on the farm | The town map arrives in M2 (T-031). |
| Weather (T-017/T-030) | 4 weather types | Deterministic sunny/rain (rain: crops grow without watering, tinted light) | Full system is T-030. |
| Tools | All tools act on the world | Hoe, watering can, seeds work; axe/pickaxe/scythe have no targets; watering can has no water capacity | Trees/rocks/weeds are T-032/T-033. |
| Debug overlay (T-011) | Clock debug overlay | Not built; the HUD shows the clock | Not needed until balancing (T-066). |
| Renderer | - | The 2D renderer logs "post-processing shader stripped" warnings in builds | Harmless: no post-processing is used yet. Revisit with T-060 lighting polish. |
| Unity MCP | Use the `unity-mcp` server when the Editor is open | Not configured; all work is headless Editor runs and editor scripts | The server was never installed in the manifest; revisit if a bridge is added. |
| Sprite import | Sliced sprite sheets and atlases | One PNG per sprite, imported as Single sprites by `TextureImportPostprocessor` | Atlases: resolved, six Sprite Atlas V2 assets built by `AtlasBuilder` (final art replaces sprites by name). |

## Lessons recorded
1. **One MonoBehaviour per file, named like the file.** Two scene components in `Interactables.cs` corrupted the Farm scene data in player builds (the Editor and tests did not show it). The player build + capture run in `docs/QA.md` catches this.
2. **Sprite import mode.** The project's default texture preset imports sprites as *Multiple*, which silently ignores the single-sprite pivot, so the avatar was drawn a tile below its logical position. The import postprocessor now forces *Single*, and tests check mode and pivots.
3. **Draw order of full-screen overlays.** The scene fade used the maximum canvas order and hid the day summary during sleep. Order is now HUD 10 < fade 50 < menus and dialogs 100, and a test raycasts at the Continue button.
4. **Invisible feedback looks like a bug.** A 2x2-pixel seedling and silent refusals made working planting look broken. Placeholder art must stay clearly visible (test-enforced) and refused actions explain themselves.
5. **Tests that call methods directly miss input problems.** Add tests with simulated real keyboard/mouse input for input-driven features.
