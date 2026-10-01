# Build and test

Unity **6000.6.2f1**. Only one Unity instance can open the project at a time: close the Editor before running any headless command below (or the command fails on the project lock). Commands below are for Git Bash on Windows from the repo root.

## One-time setup (after a fresh clone)
1. Open the project once in the Editor so packages resolve and `.meta` files exist (close it again afterwards).
2. Run these once, in order (menu items under **Farm >**, or headless with `-executeMethod`):

| Step | Menu | Headless method (append `-batchmode -nographics -projectPath . -logFile -`) |
|---|---|---|
| Project settings | Setup > Apply Project Settings | `Farm.Editor.ProjectConfigurator.ApplyAndExit` |
| TextMeshPro resources | Setup > Import TMP Essentials | `Farm.Editor.TmpSetup.ImportAndExit` |
| Input actions asset | Setup > Generate Input Actions | `Farm.Editor.InputAssetGenerator.GenerateAndExit` |
| Placeholder art + items, crops, database | Setup > Generate Content | `Farm.Editor.ContentGenerator.GenerateAllAndExit` |
| Scenes, tiles, build settings | Setup > Create M0 Scenes (builds all scenes) | `Farm.Editor.SceneSetup.CreateScenesAndExit` |

Re-run Generate Content and Create Scenes after changing sprite import rules, content tables or map layout (scene and asset references are regenerated). The generators are idempotent.

## Editor modules required
| Target | Module (Hub name) | Notes |
|---|---|---|
| Windows | Windows Build Support (Mono) | Installed |
| Windows IL2CPP | Windows Build Support (IL2CPP) + Visual Studio C++ workload | Needed for release builds; not verified |
| Linux | Linux Build Support (Mono) | Installed on the dev machine |
| Linux IL2CPP | Linux Build Support (IL2CPP) | **Not installed locally**; CI has it. Add via Hub > Installs > Add modules |

## Commands (Editor closed)
```bash
UNITY="C:/Program Files/Unity/Hub/Editor/6000.6.2f1/Editor/Unity.exe"

# EditMode tests (never pass -quit with -runTests)
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Builds/results-editmode.xml -logFile Builds/editmode.log

# PlayMode tests (needs graphics: omit -nographics)
"$UNITY" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Builds/results-playmode.xml -logFile Builds/playmode.log

# Builds -> Builds/<Windows|Linux>/<version>/   options: -scriptingBackend il2cpp  -development  -buildOutput <dir>
"$UNITY" -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildWindows -logFile Builds/build-win.log
"$UNITY" -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildLinux -logFile Builds/build-linux.log
```
Exit code 0 = success; a non-zero code with `error CS` lines in the log means a compile error. `Builds/` and `BuildsDev/` are git-ignored. After every run, read the log for compile errors and the XML for test failures.

## CI
`.github/workflows/ci.yml` (GameCI): EditMode + PlayMode tests, then Windows and Linux IL2CPP builds as artifacts. Requires repo secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`. **Not yet run**: the repo has no remote.

## Running a built player
- Windows: `Builds/Windows/<ver>/Farm.exe` (add `-screen-width 1280 -screen-height 720 -screen-fullscreen 0` for a window).
- Linux under WSL2 (WSLg): copy the folder into the WSL filesystem (not `/mnt/c`, to keep the executable bit), `chmod +x Farm`, run `./Farm`. WSLg uses OpenGL Core; real-GPU/Vulkan and Steam Deck checks are a Milestone 4 requirement.
- Logs: `-logFile <path>`; the player's default log is under `%USERPROFILE%/AppData/LocalLow/<Company>/Farm` (Windows) or `~/.config/unity3d/<Company>/Farm` (Linux). Saves and settings live in the same folder (`saves/slotN/save.json`, `settings.json`).

## QA flags (player builds)
| Flag | Effect |
|---|---|
| `-farmScene <Scene>` | Start in a scene after bootstrapping services (`Farm`, `FarmHouse`, `MainMenu`, `PixelPerfectTest`) |
| `-farmOpen <what>` | After the scene starts: `inventory`, `shop`, `pause`, `options`, `summary`, `sleep` (full sleep flow), `crops` (one crop per growth stage); in the main menu `newgame`, `options` |
| `-farmCapture <dir>` | Save 6 screenshots (`shot_<w>x<h>_<n>.png`), log a `[Perf]` line, then quit |
| `-screen-width/-height/-screen-fullscreen` | Standard Unity window flags (also stop saved display settings overriding the window) |

Example (pixel-perfect check at 2x): `Farm.exe -screen-width 960 -screen-height 540 -screen-fullscreen 0 -farmScene PixelPerfectTest -farmCapture <dir>`. A frame is pixel-perfect if downsampling by the scale factor and upsampling back reproduces it exactly.

## Test hooks (for tests only)
`GameServices.DataRootOverride` redirects saves and settings to a temp folder; `Bootstrapper.ResetForTests()` rebuilds the persistent services; `GameModules.ClearForTests()` and `Conditions.ClearCustomForTests()` clear registries.
