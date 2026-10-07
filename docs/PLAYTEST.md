# Hosting playtest builds on Steam

Wetherell Farm Saga is **Steam App ID 5408390**. Steam can host the playtest builds instead of GitHub: you upload once with SteamPipe, testers click Install, and every later upload reaches them as an ordinary game update. Nothing in this guide has been run yet (T-070 is partly done: the App ID 5408390 and the depot ids 5408391 for Windows and 5408392 for Linux are in `Steam/`; the branch, launch options and tester access are still to do in Steamworks).

## Two ways, and which to use now
| | A. A password-protected **beta branch** on the main app (recommended now) | B. **Steam Playtest** (a separate child app) |
|---|---|---|
| Who can install | Anyone you give a license for the app **and** the branch password | Players who sign up on the main game's store page; you let them in |
| Needs | Steam keys (or licenses granted to named Steam accounts) for the unreleased app | The store page up, a playtest app created and its own simplified review |
| Good for | A handful of known testers, starting now | Many strangers, later, once capsule art and the store page exist |

Both use the same SteamPipe upload. Start with A; B can be added later without changing how you upload.

## One-time setup (these steps are in the Steamworks partner site: only the owner can do them)
1. **Depots.** Done: Windows is depot 5408391 and Linux is depot 5408392 (set the operating system of each in its depot settings so a Windows player does not download the Linux files).
2. **The depot ids in the repo.** Done: `Steam/depot_windows.vdf`, `Steam/depot_linux.vdf` and the depot list in `Steam/app_build.vdf`. `Steam/upload.sh` refuses to run if a placeholder `0` ever comes back.
3. **Launch options.** In the app's installation settings: Windows launches `Farm.exe`, Linux launches `Farm` (the executable keeps its code name).
4. **Access for testers (option A).** Request Steam keys for the app and give one to each tester (they redeem it in Steam under Games > Activate a Product), or grant licenses to named accounts from your Steamworks tools.
5. **The branch.** On the Builds page create a branch named `playtest` and **set its password before the first upload** (Valve: set the password before setting a build live if the contents must stay private).
6. **Build account and steamcmd.** Install steamcmd, log in once by hand (password and Steam Guard code), then every later run needs only the account name.

## Each playtest build
1. Build the release builds (`docs/BUILD.md`): `Builds/Windows/<version>` and `Builds/Linux/<version>`.
2. From the repo root, in Git Bash:
   ```
   export STEAMCMD="C:/steamcmd/steamcmd.exe"      # wherever steamcmd is
   export STEAM_USER="your-build-account"
   Steam/upload.sh playtest
   ```
   The script stamps the version into temporary copies of the Steam scripts, uploads both depots (leaving out `Farm_BackUpThisFolder_ButDontShipItWithYourGame` and `*.pdb`), and sets the build live on the `playtest` branch. It will not set `default` live: Valve requires that to be done by hand in the partner site.
3. Check the build on the Builds page, then tell the testers.

## What a tester does
1. Redeems the key, installs the game from the Steam library.
2. Right-click the game > Properties > Betas, enters the branch password, chooses `playtest`.
3. Later builds download automatically. To report a problem: main menu or pause menu > Report a bug (it packs the logs into a zip and opens an email; see `docs/QA.md`).

## Things to check on the first upload
- **The Linux binary's executable flag.** The Linux player is built on Windows, which does not record a Linux "executable" permission, and the Steamworks upload documentation does not say how to set it. Install the first build on a Linux machine or a Steam Deck before telling Linux testers it works. If `Farm` will not start, the fix is to build the Linux player on a Linux runner (the CI workflow can) so the flag is kept.
- **Steam features are off.** The game runs on a `NullPlatform`: from Steam it plays normally, but achievements and the overlay's Steam hooks are not connected yet (`docs/RELEASE.md`, the `FARM_STEAM` define). Playtests do not need them.
- **Size and cleanliness.** The release builds are about 120 MB each and the release guard fails a build that contains developer tools.

## Sources
Valve's documentation: [Uploading to Steam](https://partner.steamgames.com/doc/sdk/uploading), [Branches (Betas)](https://partner.steamgames.com/doc/store/application/branches), [Steam Playtest](https://partner.steamgames.com/doc/features/playtest). These pages are several years old in places; check the live versions for exact limits.
