# Changelog

## Unreleased (Milestones 4 and 5 preparation)
- The game is called **Wetherell Farm Saga**: the window title, main menu, crash log and bug reports use it. Existing saves and settings are copied over from the old data folder the first time the game starts (the old folder is kept). The code name, folders and executable stay `Farm`.
- Steam upload templates and script, release process, store page / EULA / privacy drafts, release-checklist tests.
- Accessibility and quality of life: colour-blind palette for the bars, reduce flashes, relaxed energy (half cost), day length (long/normal/short).
- Achievements and a platform layer (null by default; Steam behind `FARM_STEAM`); the game pauses when the window loses focus.
- Diagnostic log file; archived-save compatibility tests; one-year soak runs at each horror level.
- Performance and economy notes in `docs/PERF.md` and `docs/balance/`.

## 0.0.1 (internal milestones m0 to m3b)
See `docs/STATUS.md`.
