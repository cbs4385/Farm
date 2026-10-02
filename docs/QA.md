# QA

## Automated (run before every milestone gate)
Editor must be closed. Commands and flags are in `docs/BUILD.md`.

| Check | How | Covers |
|---|---|---|
| EditMode tests (~205) | `-runTests -testPlatform EditMode` | clock/calendar/moon, inventory, farm growth, day cycle, save/load/migration/backup, settings, input bindings and rebinding, localization lint and hooks, content and sprite-import validation, condition language, hooks, modules, atmosphere, content packs, session flags/vars/module data |
| PlayMode tests (~17) | `-runTests -testPlatform PlayMode` (needs graphics) | boot to menu, new game, farming loop, sleep through the UI (Continue clickable over the fade), pass-out at 2 AM, warps keep state, save/load, options scrolling, avatar/cursor alignment, **simulated keyboard and mouse input**, a test module using every extension point |
| Windows + Linux builds | `BuildScript.BuildWindows` / `BuildLinux` | player builds compile and boot |
| Player capture | `-farmScene <Scene> -farmOpen <screen> -farmCapture <dir>` | look at the screenshots for every screen; the `[Perf]` log line shows avg/max frame time and GC |
| Pixel-perfect check | BUILD.md "QA flags" | integer scaling at 1x-4x |

**When to also run the player capture:** any change to scenes, scene components, UI layering/canvases, sprite import, or serialization. The Editor and tests missed two real problems that only a player build showed (a scene that crashed the player, and a fade overlay hiding the day summary).

**Bug-fix rule:** write the test that reproduces the bug first, watch it fail, then fix.

## Manual smoke checklist (vertical slice and later)
Run on Windows and Linux. Keyboard/mouse first, then gamepad. Items marked (verified) have been done by hand on Windows; see `STATUS.md`.

1. Launch -> main menu shows. Continue is disabled with no saves.
2. New Game -> enter names -> pick a slot -> arrive on the farm near the house.
3. Hotbar: keys 1-0/-/= and mouse wheel / `,` `.` change the selected slot. Gamepad: shoulder buttons.
4. Select the hoe, face a grass tile, use tool (LMB / C / gamepad X): tile tills, energy drops by 2 (verified). Out of energy: message, no action (verified).
5. Watering can on tilled tile: darker soil. Seeds on tilled tile: crop appears, seed count drops (verified). Seeds on untilled ground: explanatory message.
6. Walk into the house door: fade to the farmhouse; walk out: back on the farm, soil unchanged.
7. Interact (E / RMB / gamepad A) on the bed -> confirm -> fade -> day summary visible -> Continue -> wake in farmhouse, next day, energy full (verified).
8. After 4 watered days a parsnip is mature: Interact harvests it (verified).
9. Interact on the shipping bin with a crop selected; sleep: summary lists it and gold increases (verified).
10. Shop stall (blue box): buy seeds; gold drops; "backpack full" and "not enough gold" messages (buying verified).
11. Tab/I opens the backpack; select a slot then another to move items; Esc closes.
12. Stay up until 2:00 AM: pass out, lose some gold, wake in bed at 75% energy. (Development build: `time 01:40`, then `skip 20`.)
13. Esc pauses: Save Game, Options (sliders, scrolling, resolution, fullscreen, UI size, rebind a key), Main Menu.
14. Quit to menu -> Continue loads the latest save; also after closing and restarting the game (verified).
15. Spring 28 -> Summer 1: spring crops wither. (Development build: plant crops, then `date summer 1`.)
16. Rain day: soil already watered, bluish tint, falling rain. Also check `weather storm` (darker, heavier, lightning flashes), `weather snow` and `weather wind`; the HUD shows tomorrow's forecast under the weather. Inside buildings there is no weather. (Development build: `weather <id>`.)

## Developer tools (development builds and the Editor)
F1 opens the console (see `docs/BUILD.md`). Use it to reach states quickly: `date summer 15` + `time 22:30` for a full-moon night, `var dread 40`, `weather rain`, `give seed.parsnip 20`, `gold 5000`, `tp FarmHouse bed`. **Never test a release build with these**; they are absent from it (the build fails if any leak in).

## Extension points and horror layer (use from M2 onward)
- With `HorrorLevel` 0, play a scripted day and compare state with a build/run that has no modules: identical (T-044 automates this).
- With a test module registered: its day-cycle note appears in the summary, weather overrides show a name (not a raw key), HUD widgets render and refresh, gated warps explain themselves, conditional objects follow flags.
- When horror content exists: playthroughs at levels 0, 1 and 2; level 1 removes the most disturbing text/imagery; the intensity option and content notes are reachable from Options and the store page.

## Known limits
See `docs/adr/0001-m1-design-deviations.md`.
