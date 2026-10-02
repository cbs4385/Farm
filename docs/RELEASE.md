# Release and Steam (T-062, T-070, T-071)

## What is built
- `Farm.Platform.IPlatformServices` with a `NullPlatform` default: the game runs without Steam. `SteamPlatform` (Steamworks.NET) compiles only with the scripting define `FARM_STEAM` and the package installed; if `SteamAPI.Init` fails it leaves the null platform in place.
- **Achievements** (`Achievements.cs`): condition-based (the story-state condition language), saved as flags `ach.<id>`, unlocked once, re-sent to the platform after loading a save, a toast in game. Layers register their own (the horror layer adds four ending achievements with generic, spoiler-free text). API names on Steam must equal the ids below.
- **Overlay-safe pause:** the clock pauses while the window loses focus (Steam overlay, alt-tab) and resumes on return.
- **Crash/diagnostic log:** `logs/farm.log` (previous run in `farm.prev.log`) under the persistent data path, errors and exceptions only, capped at 2 MB.

## Achievement ids
`first_furrows`, `first_harvest`, `neighbour`, `hall`, `second_year`, `third_year`, `end_sealed`, `end_joined`, `end_ignored`, `end_awakened`. Titles are in `en.json` (`ach.<id>.name`/`.desc`) and must stay spoiler-free.

## Steam Auto-Cloud (configure in the Steamworks partner site)
Root: `Application.persistentDataPath` (Windows `%USERPROFILE%/AppData/LocalLow/<company>/<product>`, Linux `~/.config/unity3d/<company>/<product>`). Include `saves/*/save.json` (and `.bak`) and `settings.json`; exclude `logs/`.

## Still to do (needs the owner)
Steam App ID, depots and branches (T-070), SteamPipe scripts (T-071), Steam Input glyphs and the on-screen keyboard (Steam-only APIs), verification with Steam running on Windows and Linux/Steam Deck.

## Release process (T-071, T-074)
1. **Version.** `bundleVersion` in Project Settings is the version (semantic, `x.y.z`); builds go to `Builds/<OS>/<version>`. Bump it, add a `CHANGELOG.md` entry, commit, tag `v<version>`.
2. **Build** both players (release, IL2CPP for candidates): `docs/BUILD.md`. The build guard fails if developer tools leak in.
3. **Gate.** Run the EditMode and PlayMode suites and the manual checks in `docs/QA.md`; walk the release checklist in the plan (the automatable lines are `ReleaseChecklistTests`).
4. **Upload to `beta`:** `STEAMCMD=... STEAM_USER=... Steam/upload.sh beta` (needs the App and depot ids in `Steam/*.vdf`; the script refuses placeholders and refuses the `default` branch). Install from Steam on Windows, Linux and a Steam Deck, play, check achievements and Auto-Cloud, check offline mode.
5. **Go live:** in the partner site, set `beta` live on `default`, only after step 4 passes. Announce with the changelog text.
6. **Hotfix:** branch from the release tag (`hotfix/<version>`), fix with a failing-first test, bump the patch version, repeat 2 to 5 (upload to `beta`, verify, promote). Saves are forward-compatible only: never lower `GameState.CurrentVersion`; any change to save shape adds a migration and a fixture under `Tests/EditMode/Saves`.
7. **Patch pipeline:** SteamPipe uploads only changed files, so patches are small as long as the build is deterministic enough; keep `Builds/` out of git and archive the shipped folders.
