# Build and test

Unity **6000.6.2f1**. Only one Unity instance can open the project at a time: close the Editor before running any headless command below (or the command fails on the project lock).

## One-time setup (after a fresh clone)
1. Open the project once in the Editor so packages resolve and `.meta` files exist.
2. Run **Farm > Setup > Apply Project Settings**, **Farm > Generate Placeholder Art**, then **Farm > Setup > Create M0 Scenes**
   (headless equivalents: `-executeMethod Farm.Editor.ProjectConfigurator.ApplyAndExit`, `...PlaceholderArtGenerator.GenerateAndExit`, `...SceneSetup.CreateScenesAndExit`).

## Editor modules required
| Target | Module (Hub name) | Notes |
|---|---|---|
| Windows | Windows Build Support (Mono) | Installed |
| Windows IL2CPP | Windows Build Support (IL2CPP) + Visual Studio C++ workload | Needed for release builds |
| Linux | Linux Build Support (Mono) | Installed on dev machine (6000.6.2f1) |
| Linux IL2CPP | Linux Build Support (IL2CPP) | **Not installed on dev machine**; CI (GameCI image) has it. Add via Hub > Installs > Add modules |

## Commands (run from the repo root, Editor closed)
```bash
UNITY="C:/Program Files/Unity/Hub/Editor/6000.6.2f1/Editor/Unity.exe"

# EditMode tests (do NOT pass -quit with -runTests)
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Builds/results-editmode.xml -logFile -

# PlayMode tests (needs graphics: omit -nographics)
"$UNITY" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Builds/results-playmode.xml -logFile -

# Builds -> Builds/<Windows|Linux>/<version>/  (add: -scriptingBackend il2cpp  -development)
"$UNITY" -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildWindows -logFile -
"$UNITY" -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildLinux -logFile -
```
Exit code 0 = success. `Builds/` is git-ignored.

## CI
`.github/workflows/ci.yml` (GameCI): EditMode+PlayMode tests, then Windows and Linux IL2CPP builds as artifacts. Requires repo secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` (see GameCI docs). The repo has no remote yet, so CI has not run.

## Linux verification
Linux player cannot run on Windows directly. Use WSL2 (WSLg) with Ubuntu: `./Builds/Linux/<ver>/Farm` (needs `libgtk-3-0`/Vulkan or Mesa), or run `-batchmode -nographics` for a headless boot check. Real GPU testing on a Linux machine/Steam Deck is a Milestone 4 requirement.
