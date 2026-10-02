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
