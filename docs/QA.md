# QA

## Automated (run before every milestone gate)
Editor must be closed. Commands and flags are in `docs/BUILD.md`.

| Check | How | Covers |
|---|---|---|
| EditMode tests | `-runTests -testPlatform EditMode` | clock/calendar, inventory, farm growth, day cycle, save/load/migration/backup, settings, input bindings + rebinding, localization lint, content validation |
| PlayMode tests | `-runTests -testPlatform PlayMode` (needs graphics) | boot to menu, new game, till/water/plant, sleep through the UI, pass-out at 2 AM, warps keep state, harvest + shipping, save/load |
| Windows + Linux builds | `BuildScript.BuildWindows` / `BuildLinux` | player builds compile and boot |
| Screenshot capture | player flags `-farmScene <Scene> -farmOpen <inventory\|shop\|pause\|options\|summary\|newgame> -farmCapture <dir>` | visual checks of every screen; also logs a `[Perf]` line (avg/max frame time, GC) |
| Pixel-perfect check | see BUILD.md "Visual QA capture" | integer scaling at 1x-4x |

## Manual smoke checklist (M1 vertical slice)
Run on Windows and Linux. Keyboard/mouse first, then gamepad.

1. Launch -> main menu shows. Continue is disabled with no saves.
2. New Game -> enter names -> pick a slot -> arrive on the farm near the house.
3. Hotbar: keys 1-0/-/= and mouse wheel / `,` `.` change the selected slot. Gamepad: shoulder buttons.
4. Select the hoe, face a grass tile, use tool (LMB / C / gamepad X): tile tills, energy drops by 2.
5. Watering can on tilled tile: darker soil. Seeds on tilled tile: crop appears, seed count drops.
6. Walk into the house door: fade to the farmhouse; walk out: back on the farm, soil unchanged.
7. Interact (E / RMB / gamepad A) on the bed -> confirm -> fade -> day summary -> Continue -> wake in farmhouse, next day, energy full.
8. After 4 watered days a parsnip is mature: Interact harvests it into the backpack.
9. Select the harvested crop, Interact on the shipping bin: it is removed; sleep: summary lists it and gold increases.
10. Shop stall (blue box): buy seeds; gold drops, items arrive; "backpack full" and "not enough gold" toasts work.
11. Tab/I opens the backpack; select a slot then another to move items; Esc closes.
12. Stay up until 2:00 AM: you pass out, lose some gold, wake in bed at 75% energy.
13. Esc pauses: Save Game, Options (sliders, resolution, fullscreen, UI size, rebind a key), Main Menu.
14. Quit to menu -> Continue loads the latest save with the same state.
15. Spring 28 -> Summer 1: spring crops wither.

## Known limits at M1
See `docs/adr/0001-m1-design-deviations.md`.
