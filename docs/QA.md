# QA

## Automated (run before every milestone gate)
Editor must be closed. Commands and flags are in `docs/BUILD.md`.

| Check | How | Covers |
|---|---|---|
| EditMode tests (~205) | `-runTests -testPlatform EditMode` | clock/calendar/moon, inventory, farm growth, day cycle, save/load/migration/backup, settings, input bindings and rebinding, localization lint and hooks, content and sprite-import validation, condition language, hooks, modules, atmosphere, content packs, session flags/vars/module data |
| PlayMode tests (~17) | `-runTests -testPlatform PlayMode` (needs graphics) | boot to menu, new game, farming loop, sleep through the UI (Continue clickable over the fade), pass-out at 6 AM, late-night warning and fatigue, warps keep state, save/load, options scrolling, avatar/cursor alignment, **simulated keyboard and mouse input**, a test module using every extension point |
| Windows + Linux builds | `BuildScript.BuildWindows` / `BuildLinux` | player builds compile and boot |
| Player capture | `-farmScene <Scene> -farmOpen <screen> -farmCapture <dir>` | look at the screenshots for every screen; the `[Perf]` log line shows avg/max frame time and GC |
| Pixel-perfect check | BUILD.md "QA flags" | integer scaling at 1x-4x |

Dialogue box check: `Farm.exe -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -farmScene Farm -farmOpen dialogue -farmCapture <dir>` (portrait, name, text, numbered choices, tone tag and the log hint should all be visible and unclipped; checked 2026-10-02 on Windows).

Chat menu check: `... -farmOpen chatmenu -farmCapture <dir>` (the social submenu, five choices: the villager's name, the prompt, the numbered choices and the highlighted Back must fit the box; checked 2026-10-02). The box holds one line of prompt and up to five choices; a longer list needs the frame to grow.

Memories tab check: `... -farmOpen memories -farmCapture <dir>` (eight tab labels on one line, the grid of scene buttons unclipped, locked entries dimmed; checked 2026-10-02). Known flaky PlayMode tests under machine load: the hoe, placement and gamepad movement tests (`GameFlowTests`, `RealInputTests`, `GamepadAndUiInputTests`, `CraftingFlowTests`) have each failed once and then passed on a re-run with no change; they are timing-sensitive.

Stream mode check: `... -farmOpen stream -farmCapture <dir>` (stream mode with a 30 second timer and the UI size at 1.4: the dialogue box must fit the screen with the portrait, the countdown and the content badge all visible; checked 2026-10-03).

**When to also run the player capture:** any change to scenes, scene components, UI layering/canvases, sprite import, or serialization. The Editor and tests missed two real problems that only a player build showed (a scene that crashed the player, and a fade overlay hiding the day summary).

**Bug-fix rule:** write the test that reproduces the bug first, watch it fail, then fix.

## Manual smoke checklist (vertical slice and later)
Run on Windows and Linux. Keyboard/mouse first, then gamepad. Items marked (verified) have been done by hand on Windows; see `STATUS.md`.

1. Launch -> main menu shows. Continue is disabled with no saves.
2. New Game -> enter names -> pick a slot -> arrive on the farm near the house.
3. Hotbar: keys 1-0/-/= and mouse wheel / `,` `.` change the selected slot. Gamepad: shoulder buttons.
4. Select the hoe, face a grass tile, use tool (LMB / C / gamepad X): tile tills, energy drops by 2 (verified). Out of energy: message, no action (verified).
5. Watering can on tilled tile: darker soil. Seeds on tilled tile: crop appears, seed count drops (verified). Seeds on untilled ground: explanatory message.
6. Walk into the house door: fade to the farmhouse; walk out: back on the farm, soil unchanged. Walk east along the farm road to the village, and into each door: the general store, blacksmith, carpenter, library, saloon and clinic open and close by their hours (a locked door shows a message with the hours and the day off), each interior has a way out back to its door. The lane leads north to the forest (the path ends at brambles) and south to the beach (fish stall: 06:00-14:00, closed Thursday). Nothing blocks the way between any two places.
7. Interact (E / RMB / gamepad A) on the bed -> confirm -> fade -> day summary visible -> Continue -> wake in farmhouse, next day, energy full (verified).
8. After 4 watered days a parsnip is mature: Interact harvests it (verified).
9. Interact on the shipping bin with a crop selected; sleep: summary lists it and gold increases (verified).
10. General store counter (village, open 09:00-17:00, closed Sunday): buy seeds; gold drops; "backpack full" and "not enough gold" messages (buying verified).
11. Tab/I opens the backpack; select a slot then another to move items; Esc closes.
12. Stay up until 2:00 AM: pass out, lose some gold, wake in bed at 75% energy. (Development build: `time 01:40`, then `skip 20`.)
13. Esc pauses: Save Game, Options (sliders, scrolling, resolution, fullscreen, UI size, rebind a key), Main Menu.
14. Quit to menu -> Continue loads the latest save; also after closing and restarting the game (verified).
15. Spring 28 -> Summer 1: spring crops wither. (Development build: plant crops, then `date summer 1`.)
15b. Late night (development build: `time 21:50`, then wait): at 22:00 a message explains tiredness once per save (Esc or Close dismisses it, it never returns); a purple fatigue meter appears beside the energy bar and grows towards 06:00; the HUD clock shows 6:00 AM at the end of the day. Sleeping late restores less energy (none after a whole night) and the summary says so; staying up until 06:00 collapses the farmer (gold lost, wakes in the farmhouse, still exhausted until a bed sleep). Good luck is reduced while tired (`state` shows luck).
15c. Clutter (farm): weeds, rocks, trees and stumps are scattered over the farm, clear of the paths, doors and spawn points; scythe cuts weeds (fiber), pickaxe breaks rocks (stone, two blows), axe fells trees (wood, six blows; leaves a stump, three more blows), the hoe will not till under a weed; wrong tools do nothing; a boulder needs a better pickaxe; skill level-ups show a toast (dev build: `xp mining 100`); clearing survives save and load.
15d. Upgrades (dev build: `give resource.copperbar 5`, `gold 3000`): at the blacksmith counter (Tue-Sun) buy a copper axe: the axe leaves the backpack, the gold and bars are used, two nights later the summary says it is ready and the counter hands it back as a Copper Axe (tooltip, lower swing cost, stronger blows; a boulder needs a copper pickaxe). The general store's second counter sells the 24- and 36-slot backpacks (the backpack screen grows); the clinic desk sells +20 energy tonics. Counters refuse on closed days with the hours.
16. Rain day: soil already watered, bluish tint, falling rain. Also check `weather storm` (darker, heavier, lightning flashes), `weather snow` and `weather wind`; the HUD shows tomorrow's forecast under the weather. Inside buildings there is no weather. (Development build: `weather <id>`.)

## Developer tools (development builds and the Editor)
F1 opens the console (see `docs/BUILD.md`). Use it to reach states quickly: `date summer 15` + `time 22:30` for a full-moon night, `var dread 40`, `weather rain`, `give seed.parsnip 20`, `gold 5000`, `tp FarmHouse bed`. **Never test a release build with these**; they are absent from it (the build fails if any leak in).

## Extension points and horror layer (use from M2 onward)
- With `HorrorLevel` 0, play a scripted day and compare state with a build/run that has no modules: identical (T-044 automates this).
- With a test module registered: its day-cycle note appears in the summary, weather overrides show a name (not a raw key), HUD widgets render and refresh, gated warps explain themselves, conditional objects follow flags.
- When horror content exists: playthroughs at levels 0, 1 and 2; level 1 removes the most disturbing text/imagery; the intensity option and content notes are reachable from Options and the store page.

## Known limits
See `docs/adr/0001-m1-design-deviations.md`.


## Milestone 3 additions
Manual checks (not yet done by a person): cast a rod at the beach and the forest pond and finish the timing bar; build the coop (carpenter), buy a chicken, feed and collect; go into the mine through the forest cave (east path), fight, mine ore, take the ladder, use the elevator after floor 5; knock yourself out and wake at the clinic; talk to all 12 villagers and trigger a heart event; donate to a hall room; visit the village on a festival day (spring 13, summer 11, fall 16, winter 25); open the Collections and Skills tabs (professions at level 5).
Automated: `MineAndCombatTests`, `FishingTests`, `AnimalTests`, `HallAndFestivalTests`, `Milestone3Tests` (EditMode, including the two-year simulation); `MineFlowTests`, `AdventureFlowTests` (PlayMode).

## Milestone 3b additions (horror layer)
**A person must play a year at each intensity before the M3b gate: `docs/qa/MYTHOS_PLAYTHROUGH.md` is the checklist.**
Manual checks (not yet done by a person): Options > Content cycles Off / Mild / Full and "Content notes" opens; at Off nothing changes (no fog, no Woods gate, no clues, no dreams) over a season; at Full with a development build `date spring 3`, `time 21:50`, `var lore 2`: Keepers leave their shops, walk to the Woods and stand at the altar at about 22:30 (the Woods open from summer: `flag woods.open`); the altar's lights appear and dissolve; `date summer 4` + `flag mythos.interference` lets you take an offering (the ritual fails); `var mythos.wakefulness 600` shows tint and fog and `dread 50` grows hollowroot; talk to Tilda, Marcus, Dr. Penn, Dorian, Wren, Hazel for clue lines (`var lore 1`, then 3 and 5); read the stones and pick up the three relics, then `quest.start mythos_seal`, `var lore 8` and use the altar to seal; `var mythos.wakefulness 1000` plays the awakening and returns to the menu.
Automated: `MythosLayerTests`, `MythosModelTests`, `RitualModelTests`, `DataValidationTests.MythosData_*` (EditMode); `VillageFlowTests` Woods tests (PlayMode).

## Milestone 4 additions
Manual checks (not yet done by a person): Options > Accessibility (colour-blind colours change the energy, health and fatigue bars; reduce flashes stops lightning flashes in a storm: dev build `weather storm`); Options > Gameplay (relaxed energy halves costs; day length changes how fast the clock moves, live); alt-tab away for a minute and back: the clock did not advance; `logs/farm.log` exists under the persistent data path and an induced error appears in it; achievements toast when a tutorial quest finishes.
**Steam Deck checklist (not done):** 1280x800 UI fits (captured on Windows: fits); all menus reachable with the gamepad; text readable at the default UI size; frame rate steady in the village; suspend/resume keeps the game running; saves sync with Auto-Cloud.
Automated: `Milestone4Tests` (EditMode).

Slice scenes and barks check (2026-10-03, development build, Windows): `Farm.exe -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -farmScene Saloon -farmCommands "time 14:00;bark wren" -farmCapture <dir>` shows the bark bubble over Wren; `-farmScene Saloon -farmCommands "hearts wren 4;time 14:00;scene wren_heart4"` opens the first scene on the right map (start in the map: `tp` followed at once by `scene` plays the scene on the old map); `-farmScene Farm -farmCommands "say social.wren.joke.great"` shows an expression portrait. A person still has to play the scenes end to end; `SliceScenesFlowTests` does it with real key presses.


## Narrative features added 2026-10-04 (automation only; a person should look at each)
- **Photo mode:** in the world press F8 (HUD hides, clock stops), 1-6 put an emote over the nearest villager, Tab picks the next, Enter or F8 saves a PNG to `<data>/Photos/`, Esc leaves. Check the PNG has no HUD or hint text in it.
- **Village Gazette:** menu tab `Gazette`; `-farmOpen gazette` for a capture. The same in-game week always shows the same issue.
- **Poses:** `anim wave|sit|shrug|point` in a scene swaps in the villager's pose sprite. Play any heart-2 scene and watch for the wave; play `overheard_*` scenes from `docs/qa/overheard/`.
- **Story props:** the umbrella, cat, notes, pumpkin and scarecrow scenes and the Lantern Release show a prop on the map (commands in `docs/qa/storylines/` and `docs/qa/festivals/`).
- **Courtship, overheard scenes, festival lines:** command files in `docs/qa/courtship/`, `docs/qa/overheard/`, `docs/qa/festivals/`.
- **A new game's mailbox holds only the welcome letter** (guarded by `MailDeliveryTests`).
- **Sound effects (listen):** WAVs of the 18 new effects are written to `Builds/sfx/` by `SfxSynthTests`. In a development build, trigger each: shutter (photo mode F8, Enter), page turn (switch menu tabs), letter (`mail`), quest done, level up, door (walk through a warp), pickup, heart, gift, cast / bite / splash (fish), sword swing and hit (mine), chest open, sleep and rooster (go to bed), lantern (`scene festival_winter_lanterns`). Check none is too loud next to the existing tones and that none repeats annoyingly (the door and pickup are the most frequent).
- **Footsteps, animals, ambience, hover (listen):** walk on the farm (a soft step each stride, not while standing still); pet an animal in the coop or barn (chicken and duck cluck and quack, cow moos, sheep and goat bleat, rabbit silent) and wait half a minute for an idle call; on a sunny morning the farm has birds, `weather rain` gives rain, `weather wind` gives wind, a clear night has crickets (not in winter), indoors is quiet; beds fade over about a second and a half. Hover the mouse over menu buttons for a tiny tick. Ambience volume is Options > Audio. The WAVs are `Builds/sfx/ambience_*.wav`.
- **Surfaces, weather and village sounds (listen):** walk on grass, then the village path, a wooden floor indoors and the beach sand: the step changes (soft, firm, firm, scuff). `weather storm` outdoors gives thunder every 15 to 40 seconds, `weather wind` gives a gust; none indoors. In the village by day an occasional faint meow; `time 11:55` then wait, the bell strikes three times at noon and twice at 18:00.

## Playtester feedback round (2026-10-04): what to try by hand
- **Throw something away:** open the inventory, pick up a stack, press Delete or the button, confirm; a tool refuses. A new game has a chest in the pack: set it down, open it with the interact key, move things in.
- **Hover:** rest the mouse over the shipping bin by the house, the bed, the mailbox, a door, a villager: a small label names it. Options > "Name things under the mouse" turns it off.
- **Shipping window:** walk up to the bin and press interact: pick a stack, use -10 -1 +1 +10 All None, read the total, "Ship the lot"; Esc leaves without selling.
- **Mouse aim:** move the mouse around the player with a hoe or the watering can: the square sits on the eight cells around you; walk and it returns to the facing direction. Options > "Aim the tool square with the mouse" turns it off.
- **Furniture:** buy pieces at the carpenter's (Marcus' shop counter), set them down in the farmhouse or on the farm with the use key, pick them up with the interact key; try the doorway (refused) and a rug in front of the door (allowed); only the rug can be walked over.
- **Corner:** walk to the upper right of the farm: the clock panel fades so the player stays visible; walk away and it returns.

## Real-input checks on the built Windows player (tools/qa)
`python tools/qa/real_input_aim.py <path to Farm.exe> <output folder>` launches the player, focuses it, moves the mouse (as real mouse input) around the avatar and screenshots the screen; it prints where the yellow tool square lands for the cell under the avatar, the four sides, the diagonals and a far pointer. `python tools/qa/real_input_till.py <exe> <folder>` walks onto grass with the keyboard, aims at the cells in and around the avatar and clicks with the hoe. Windows only; the PC must be left alone while they run (they use the real keyboard and mouse and need the game window focused). A development build can write a log with `-logFile`.

Farmer creator: `-farmScene Farm -farmOpen avatar -farmCapture <folder>` opens the creator over the farm; `-farmOpen newgame` shows the new-game screen with the farmer preview and Customize button.

## Report a bug (main menu and pause menu; automation only, nobody has sent one)
- **Try it by hand:** main menu > Report a bug, and pause > Report a bug (`-farmOpen bugreport` shows it in a development build). Leave it empty and press the button: it asks for a subject, then a description. Fill both in and press "Create report and open email": your mail program should open addressed to `gamestrubios@gmail.com` with the subject and the start of your text, and the window names the zip to attach and has an "Open the folder" button. The zip is in the game's data folder under `BugReports/` (the newest 10 are kept): `report.txt`, `logs/farm.log`, `logs/farm.prev.log`, `logs/Player.log` (the end of each) and, if ticked, `save.json`. Account name, machine name and home folder are replaced by `<user>`, `<machine>`, `<home>`.
- **Limits:** a game cannot attach a file to the mail it opens and must not carry mail credentials, so the player attaches the zip. If no mail program is configured the window still says where the file is. In stream mode the window shows only the file name, never a folder.
- **When a report arrives:** unzip it, read `report.txt` first (what the player wrote, version, platform, where in the game), then `logs/farm.log` (errors and exceptions), then `logs/Player.log`; `save.json` is the saved state: copy it to a save slot to reproduce.
