# Task status

Format: `T-xxx | status (todo/in-progress/done/blocked) | agent/date | notes`

T-001 | done | claude/2026-10-01 | repo initialized, LFS attrs, gitignore
T-002 | done | claude/2026-10-01 | `ProjectConfigurator` applied. Standalone backend is Mono for dev; release builds pass `-scriptingBackend il2cpp` (see docs/BUILD.md)
T-003 | done | claude/2026-10-01 | asmdefs + tests; 8 EditMode + 1 PlayMode pass
T-004 | done | claude/2026-10-01 | Welcome/SampleScene/iet-framework removed; Localization 1.5.13, Newtonsoft 3.2.2, Cinemachine 3.1.7 added
T-005 | done (partial) | claude/2026-10-01 | `BuildScript` Win+Linux Mono builds succeed. NOT verified: IL2CPP builds (Linux IL2CPP module not installed locally), CI workflow (no git remote yet)
T-006 | done | claude/2026-10-01 | Bootstrap -> MainMenu works; ServiceLocator, EventBus, SceneLoader(fade), Log
T-007 | done (partial) | claude/2026-10-01 | PixelPerfectCamera (480x270, PPU16) + `PixelPerfectTest` scene + texture postprocessor. NOT verified: visual shimmer check at 1x-4x (needs a human with the Editor)
T-008 | done (partial) | claude/2026-10-01 | 43 placeholder PNGs (individual files, not sliced sheets/atlases; sprite atlases deferred to T-060)
T-009 | done (partial) | claude/2026-10-01 | Win + Linux builds boot to Bootstrap->MainMenu headless (Linux via WSL2 Ubuntu). Tag m0 applied. Linux not tested with a real GPU window
