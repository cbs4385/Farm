# Steam build files (T-070, T-071)

These are templates. The **App ID is 5408390** (Wetherell Farm Saga, from the Steamworks partner site, in `app_build.vdf`). The depot IDs are **5408391 (Windows)** and **5408392 (Linux)**, set in the depot files and in `app_build.vdf`; `upload.sh` still refuses to run if any placeholder `0` comes back. What remains is on the partner site (launch options, the branch and its password, tester access, steamcmd login). Nothing here has been uploaded or run.

| File | Purpose |
|---|---|
| `app_build.vdf` | The app build: depots, description, branch to set live |
| `depot_windows.vdf` | Windows depot: `Builds/Windows/<version>` |
| `depot_linux.vdf` | Linux depot: `Builds/Linux/<version>` |
| `upload.sh` | Builds nothing; checks the version folders exist and runs `steamcmd +run_app_build` |

**For playtests, see `docs/PLAYTEST.md`** (hosting playtest builds on Steam instead of GitHub).

Use: build both players first (`docs/BUILD.md`, release builds, IL2CPP for the release candidate), set `STEAMCMD`, `STEAM_USER` and run `Steam/upload.sh beta`. Never set the `default` branch live from the script; promote on the partner site after testing on `beta` (see `docs/RELEASE.md`).
