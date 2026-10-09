# Changelog

## 0.2.1 (2026-10-09)

- Controller and keyboard players can now see how to use a tool: the picked item's name and instructions show above the item bar for a few seconds whenever the pick changes (before, only a mouse resting on a slot showed them). The first quest names the hoe and how to use it.
- The quest tracker listed in 0.2.0 was not actually connected to the screen; it now shows at the top right (Options can turn it off).

## 0.2.0 (fixes from the overnight playtest reports, 2026-10-09)

- Rain: ground tilled while it is raining is watered at once (new fields no longer need the can on a rainy day).
- Community Hall: items are donated one at a time, as many as you carry, with a Donate button per item and "Donate all"; the progress is kept (before, a room needed every item in the backpack at once). The journal shows how many have been given.
- Chests: right-click moves one item, Shift-click half, and a stack can be dragged to any slot of the chest or the backpack (tools no longer snap back to their old slot); on a pad X moves one and Y half.
- Item quality (silver, gold, iridium) is shown as a coloured diamond on the slot and in the name; different qualities are kept in separate stacks, and the tooltip says so.
- Hovering over tilled soil or a planted crop names it and says whether it needs water and how many days until it is ready.
- Food: the tooltips say how to eat it, and eating (or being too well to eat) shows a message.
- A quest tracker at the top right lists active quests and what each still needs (turn it off in Options).

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
