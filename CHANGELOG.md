# Changelog

## 0.1.1 (2026-10-08)

- A new door-knock sound effect.
- A new opening: on the second morning (spring 2, from 8 o'clock) Elara Finch, the clinic nurse, comes to the farmhouse door to introduce herself and asks for three dandelions and three wild garlic for the clinic's herb garden (a quest, paid in gold and friendship). With the horror on, the plant is also the first ritual's fixed offering: handing it over makes that ritual fail.

## 0.1.0 (the first Steam playtests, 2026-10-08)

- Villagers face the way they walk: six of them (Bram, Marcus, Wren, Felix, Juno, Elara) had no real side-view pictures and now do.
- Xbox controller support: hints that name the controls in use, an on-screen keyboard, photo mode from the pad, a pause when the pad is unplugged; the end-of-day pop-up closes with A.
- A fourth day length, "Very long" (about 45 real minutes).
- Villagers live in homes on three streets, sleep in beds and can be woken; the title screen has a picture with a gentle breeze; the Neighbors list scrolls; the walls of the village are solid.
- Playtests are published to Steam's default branch while the game is not live.

- A macOS build (`BuildScript.BuildMac`, `Farm.app`) and a macOS Steam depot (5408393) in the build and upload scripts.
- The game is called **Wetherell Farm Saga**: the window title, main menu, crash log and bug reports use it. Existing saves and settings are copied over from the old data folder the first time the game starts (the old folder is kept). The code name, folders and executable stay `Farm`.
- Steam upload templates and script, release process, store page / EULA / privacy drafts, release-checklist tests.
- Accessibility and quality of life: colour-blind palette for the bars, reduce flashes, relaxed energy (half cost), day length (long/normal/short).
- Achievements and a platform layer (null by default; Steam behind `FARM_STEAM`); the game pauses when the window loses focus.
- Diagnostic log file; archived-save compatibility tests; one-year soak runs at each horror level.
- Performance and economy notes in `docs/PERF.md` and `docs/balance/`.

## 0.0.1 (internal milestones m0 to m3b)
See `docs/STATUS.md`.
