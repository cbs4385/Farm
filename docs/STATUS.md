# Task status

Format: `T-xxx | status (todo/in-progress/done/blocked) | agent/date | notes`

T-001 | done | claude/2026-10-01 | repo initialized, LFS attrs, gitignore
T-002 | done | claude/2026-10-01 | `ProjectConfigurator` applied. Standalone backend is Mono for dev; release builds pass `-scriptingBackend il2cpp` (see docs/BUILD.md)
T-003 | done | claude/2026-10-01 | asmdefs + tests; 8 EditMode + 1 PlayMode pass
T-004 | done | claude/2026-10-01 | Welcome/SampleScene/iet-framework removed; Localization 1.5.13, Newtonsoft 3.2.2, Cinemachine 3.1.7 added
T-005 | done (partial) | claude/2026-10-01 | `BuildScript` Win+Linux Mono builds succeed. NOT verified: IL2CPP builds (Linux IL2CPP module not installed locally), CI workflow (no git remote yet)
T-006 | done | claude/2026-10-01 | Bootstrap -> MainMenu works; ServiceLocator, EventBus, SceneLoader(fade), Log
T-007 | done (partial) | claude/2026-10-01 | PixelPerfectCamera (480x270, PPU16) + `PixelPerfectTest` scene + texture postprocessor. Automated check: player screenshots at 1x-4x are pixel-block aligned on Windows and on Linux (WSLg, OpenGL Core). Found+fixed camera sub-pixel offset by adding `PixelSnapCamera`. Stutter seen with sine-curve pan was resolved by a constant-speed pan (user-confirmed smooth at 5 u/s, 3x window)
T-008 | done (partial) | claude/2026-10-01 | 43 placeholder PNGs (individual files, not sliced sheets/atlases; sprite atlases deferred to T-060)
T-009 | done (partial) | claude/2026-10-01 | Win + Linux builds boot to Bootstrap->MainMenu headless (Linux via WSL2 Ubuntu). Tag m0 applied. Linux not tested with a real GPU window

T-007 follow-up | done | claude/2026-10-01 | Stutter was from the sine-curve test pan (uneven px steps). Constant 5 u/s pan is smooth. Watch for judder again in T-013 with real player movement; if seen, add sub-pixel smoothing (render at ref res, offset upscale by camera remainder)

T-011 | done | claude/2026-10-01 | GameDateTime, GameClock (pause stack, pass-out, day events); debug overlay deferred to HUD (T-018)
T-012 | done (logic) | claude/2026-10-01 | ItemDefinition/CropDefinition/GameDatabase SOs, ItemStack, Inventory + tests. Content assets generated in T-016
T-016 | in-progress | claude/2026-10-01 | FarmGrid logic + tests done; content (6 crops), scene rendering, tools pending
T-020 | in-progress | claude/2026-10-01 | GameState, SaveService (3 slots, atomic, .bak fallback, migrations), SettingsStore + tests done; in-game wiring pending
