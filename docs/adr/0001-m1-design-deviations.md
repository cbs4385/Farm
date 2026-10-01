# ADR 0001: Milestone 1 deviations from the design docs

Status: accepted (2026-10-01). Revisit at the milestone noted for each item.

| Area | Design said | M1 does | Why / revisit |
|---|---|---|---|
| Camera (T-013) | Cinemachine follow + confiner | Custom `CameraFollow` (map-bounds clamp using the real ortho size) + `PixelSnapCamera` | Pixel snapping and PixelPerfectCamera interplay was verified with a few lines of code; Cinemachine is installed for cutscenes (T-041). |
| Localization (T-022) | Unity Localization string tables | `L.Get(key)` over `Resources/Localization/en.json`, with a lint test that every key exists and no UI literal text is used | Unity Localization needs Addressables content builds in players; not worth the risk for one language. Swap the backend behind `L` in T-067. |
| Audio (T-023) | AudioMixer groups | Logical buses (master/music/sfx/ambience) as volume multipliers; generated placeholder blips | No audio assets exist. Real mixer + snapshots in T-061. |
| Inventory UI (T-018) | Drag/drop for mouse | Click-to-pick then click-to-place for every input device | Same code path for mouse, keyboard and gamepad. Drag/drop can be added on top. |
| Input (T-010) | Extend `InputSystem_Actions` | Generated `Resources/FarmInput.inputactions` (editor tool); template asset left unused | Reproducible from code; bindings reviewed in `InputAssetGenerator`. |
| UI authoring | uGUI + TMP | Built in code via `UiKit` (no prefabs) | Avoids hand-written YAML; all controls are Selectables so gamepad navigation works. |
| General store (T-019) | In town | A "temporary stall" next to the shipping bin on the farm | The town map arrives in M2 (T-031). |
| Weather (T-017/T-030) | 4 weather types | Deterministic sunny/rain (rain: crops grow without watering, tinted light) | Full system is T-030. |
| Tools | All tools act on the world | Hoe, watering can, seeds work; axe/pickaxe/scythe have no targets; watering can has no water capacity | Trees/rocks/weeds are T-032/T-033. |
| Debug overlay (T-011) | Clock debug overlay | Not built; the HUD shows the clock | Not needed until balancing (T-066). |
| Renderer | - | The 2D renderer logs "post-processing shader stripped" warnings in builds | Harmless: no post-processing is used yet. Revisit with T-060 lighting polish. |

## Lesson recorded
Every MonoBehaviour must live in a file with the same name. Two scene components in `Interactables.cs` corrupted the Farm scene data in player builds (editor and tests did not show it). Keep one MonoBehaviour per file; the standalone build + capture run in `docs/QA.md` catches this.
