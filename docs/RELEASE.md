# Release and Steam (T-062, T-070, T-071)

## What is built
- `Farm.Platform.IPlatformServices` with a `NullPlatform` default: the game runs without Steam. `SteamPlatform` (Steamworks.NET) compiles only with the scripting define `FARM_STEAM` and the package installed; if `SteamAPI.Init` fails it leaves the null platform in place.
- **Achievements** (`Achievements.cs`): condition-based (the story-state condition language), saved as flags `ach.<id>`, unlocked once, re-sent to the platform after loading a save, a toast in game. Layers register their own (the horror layer adds four ending achievements with generic, spoiler-free text). API names on Steam must equal the ids below.
- **Overlay-safe pause:** the clock pauses while the window loses focus (Steam overlay, alt-tab) and resumes on return.
- **Crash/diagnostic log:** `logs/farm.log` (previous run in `farm.prev.log`) under the persistent data path, errors and exceptions only, capped at 2 MB.

## Achievement ids
`first_furrows`, `first_harvest`, `neighbour`, `hall`, `second_year`, `third_year`, `end_sealed`, `end_joined`, `end_ignored`, `end_awakened`. Titles are in `en.json` (`ach.<id>.name`/`.desc`) and must stay spoiler-free.

## Cloud saves (Steam Cloud through the Remote Storage API)
**What the player sees.** Starting a new game on Steam asks where it lives: Steam Cloud (the default) or this computer only. When cloud saves are available for the first time and saves from an earlier version are on this computer, the main menu asks once whether to move them to the cloud; any answer (including Esc) is remembered in `cloud.json` and the question is not asked again, and "No" keeps them local. The load list and the new-game list show a cloud or a computer icon (`ui_save_cloud`, `ui_save_local`) in front of each saved game. Without Steam, with the cloud switched off in Steam, or offline, nothing is offered and every game is local.

**How it works** (`SaveService.Cloud.cs`, `Farm.Platform.ICloudStorage`, `SteamCloudStorage` behind `FARM_STEAM`):
- Every slot keeps its working copy at `saves/slotN/save.json` under `Application.persistentDataPath`; a slot whose `saves/slotN/location.json` says `cloud` is also written to the store as `saves/slotN.json` after every save (a slot without that file is local, so saves from before this feature load unchanged).
- Sync (at start-up of the load and new-game screens, before a load, and before the migration offer): a slot missing locally is pulled from the cloud (a second computer); a cloud slot missing in the cloud is pushed; a failed write leaves the local copy and is retried by the next sync.
- **Conflict rule: newest wins, nothing is lost.** When the local and the cloud copy of a slot differ, the one with the later modification time wins (a copy that is not valid JSON always loses; a tie goes to the cloud). The loser is kept as `saves/slotN/conflict-<time>.json` (the last three are kept).
- Deleting a cloud save also deletes the cloud copy; if the cloud refuses, the delete is remembered in `cloud.json` and retried, so a deleted game is never pulled back.
- Settings (`settings.json`) are not synced.

**What the owner must configure in the Steamworks partner site (app 5408390).** Not verified by a person yet.
1. App Admin > Application > **Cloud**: tick *Enable cloud support* (turn it on for the app before the build that uses it is live; Steam Cloud is also a per-player switch in Steam settings, which the game respects).
2. Set **Byte quota per user** and **Number of files allowed per user**. The game writes at most three files (`saves/slot0.json` to `saves/slot2.json`), each a few hundred KB at most for a long game; ask for at least 10 MB and 10 files to leave room (a quota that is too small makes writes fail, which the game treats as "keep it local"). **Seen on 2026-10-15 (first real run, by the owner): both fields were at 0, the default, and Steam refused every write; the log said "writing 'saves/slot0.json' ... was refused (WriteFailed) ... quota unknown (Steam did not answer)". With 10000000 bytes and 20 files, saved and *published*, and Steam restarted, the move of two saved games to the cloud succeeded.**
3. **Do not configure Auto-Cloud root paths for `saves/`.** The game uses the Remote Storage API so that only saves the player chose to keep in the cloud are uploaded; an Auto-Cloud rule over `saves/*/save.json` would upload local games too and fight with the game's own sync. (The earlier plan, Auto-Cloud on `saves/*/save.json`, is dropped.) If Steam later offers Auto-Cloud for `settings.json`, add only that.
4. Publish the Steamworks changes (the Cloud settings need a *Publish* in the partner site, like the depots).
5. **What the build does (wired in 0.7.0).** `BuildScript` gives every player build the scripting define `FARM_STEAM` (through `extraScriptingDefines`, so the Editor and the tests never see it; `-noSteam` makes a build without it). With it, the vendored Steamworks.NET (`Assets/ThirdParty/Steamworks.NET`, see `docs/ASSET_LICENSES.md`) is compiled in and its native library goes into the build (`steam_api64.dll`, `libsteam_api.so`, `steam_api.bundle`). At start-up (`SteamPlatform`, before the first scene) the game calls `SteamAPI.Init()` for app 5408390; if Steam is not running, the app is not owned or the library is missing, the game stays on `NullPlatform` and every save is local. `SteamAPI.RunCallbacks()` runs every frame (`SteamPump`) and `SteamAPI.Shutdown()` when the game quits. `SteamAPI.RestartAppIfNecessary` is deliberately **not** called (`SteamPlatform.RequireSteamLaunch`, off): the same files are also given out as plain downloads that must run without Steam, and the relaunch would quit them. No `steam_appid.txt` is shipped; when testing a build outside the Steam client, put one containing `5408390` next to `Farm.exe` by hand.
6. **Problems the player is told about** (one toast per trouble, re-armed by the next successful cloud write; the game stays on its local copy): `cloud.err.offline` (Steam or its cloud is switched off or unreachable), `cloud.err.quota` (the byte quota is used up), `cloud.err.write` (Steam refused the write). Nothing local is ever deleted by the migration: a slot counts as moved only after the cloud hands the same text back.
7. Test on a `beta` branch with the script in `docs/QA.md` ("Steam Cloud check"). **Real Steam Cloud has not been verified by a person.**

## Still to do (needs the owner)
Steam App ID, depots and branches (T-070), SteamPipe scripts (T-071), Steam Input glyphs and the on-screen keyboard (Steam-only APIs), verification with Steam running on Windows and Linux/Steam Deck.

## Release process (T-071, T-074)
1. **Version.** `bundleVersion` in Project Settings is the version (semantic, `x.y.z`); builds go to `Builds/<OS>/<version>`. Bump it, add a `CHANGELOG.md` entry, commit, tag `v<version>`.
2. **Build** both players (release, IL2CPP for candidates): `docs/BUILD.md`. The build guard fails if developer tools leak in.
3. **Gate.** Run the EditMode and PlayMode suites and the manual checks in `docs/QA.md`; walk the release checklist in the plan (the automatable lines are `ReleaseChecklistTests`).
4. **Upload to `beta`:** `STEAMCMD=... STEAM_USER=... Steam/upload.sh beta` (needs the App and depot ids in `Steam/*.vdf`; the script refuses placeholders and refuses the `default` branch). Install from Steam on Windows, Linux and a Steam Deck, play, check achievements and cloud saves (section above), check offline mode.
5. **Go live:** in the partner site, set `beta` live on `default`, only after step 4 passes. Announce with the changelog text.
6. **Hotfix:** branch from the release tag (`hotfix/<version>`), fix with a failing-first test, bump the patch version, repeat 2 to 5 (upload to `beta`, verify, promote). Saves are forward-compatible only: never lower `GameState.CurrentVersion`; any change to save shape adds a migration and a fixture under `Tests/EditMode/Saves`.
7. **Patch pipeline:** SteamPipe uploads only changed files, so patches are small as long as the build is deterministic enough; keep `Builds/` out of git and archive the shipped folders.
