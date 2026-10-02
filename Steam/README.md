# Steam build files (T-070, T-071)

These are templates. The numbers `0` are placeholders: **the App ID and depot IDs come from the Steamworks partner site** (an owner task) and must replace every `0` below before the first upload. Nothing here has been uploaded or run.

| File | Purpose |
|---|---|
| `app_build.vdf` | The app build: depots, description, branch to set live |
| `depot_windows.vdf` | Windows depot: `Builds/Windows/<version>` |
| `depot_linux.vdf` | Linux depot: `Builds/Linux/<version>` |
| `upload.sh` | Builds nothing; checks the version folders exist and runs `steamcmd +run_app_build` |

Use: build both players first (`docs/BUILD.md`, release builds, IL2CPP for the release candidate), set `STEAMCMD`, `STEAM_USER` and run `Steam/upload.sh beta`. Never set the `default` branch live from the script; promote on the partner site after testing on `beta` (see `docs/RELEASE.md`).
