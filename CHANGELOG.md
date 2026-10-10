# Changelog

## 0.4.2 (unreleased, held for a human review)

The farmer and every villager are less flat and now really animate:
- Every character has a dark outline, light on its top-left edge and shade on its far edge and feet, and a soft shadow on the ground under it.
- Walking is a four-frame cycle in every direction: the feet stay on the ground, the legs step (side view) or lift (front and back view), the arms swing, the body rises as the legs pass. It replaces the old one-pixel hop.
- Tool actions: the farmer's hoe, watering can, axe, pickaxe, scythe, sword and hammer are swung with four frames (wind-up, swing, strike, recover), the tool drawn in the hand, in every direction. Villagers can swing tools too: the smith's apprentice and the carpenter hammer at their posts, the fisher casts his rod, the forager swings the scythe and the nurse waters, every few seconds while standing at work.
- The "plain joystick" controller notice now appears only when you press a button on such a controller, not just because one is plugged in (some PCs list lighting or wheel devices).

## 0.4.1 (2026-10-10)

From the overnight bug reports:
- Right-click now works in windows (chest, backpack, shipping bin, sell window): the game's UI input had no right button at all, so it never arrived. A test now clicks with a real mouse.
- You can fish at the pond on the farm and the one in the village (they had no fish), not only in the forest.
- A closed shop's doorway can no longer trap you: a villager waiting for the door cell now lets you walk through them after a moment. (Standing in the door cell, the villager waited for you, and its body blocked your only way out.)
- Buildings block only what their pictures cover. The coop, barn and every shop and cottage had invisible walls over empty corners and above their roofs; each building now has a mask made from its picture (`tools/art/build_building_masks.py`).
- The reminder when you pick the last seashell, clam or pearl on the beach now says to leave some for others (plants still say so more can grow).

## 0.4.0 (2026-10-10)

A maintenance release: the code under the game was tidied so that future changes are cheaper and safer. What players may notice:
- Props no longer block cells beside or above them: their invisible collision sat up to a cell and a half off (the clock tower's was above its base). Every solid prop now blocks the cell it stands on, and a test checks every scene.
- A controller notice that could show its raw text name while the game was starting now waits until you are in a game; the bug report now says where the player stands.
- Under the hood: one table for the village's shops (scene, routes and map read it), one slot-grid builder for the backpack, chest, shipping and sell windows, the game session split by topic, the story checker and the dialogue compiler split into small steps, test caches cleared in one place, silent save and settings read failures now log a warning, the string table is strict JSON, and PlayMode tests are faster (frame cap off, long walks sped up) and share one set-up.

## 0.3.3 (2026-10-09)

- The buildings are no longer flat: every shop, every villager's cottage, the farmhouse, the chicken coop and the barn are pictures from the Cozy Village kit (roofs, windows, signs, chimneys that smoke), each with its door where the game's door is. They stand over a player who walks behind them. The greenhouse keeps its old look (the kit has none).
- The saloon, the clinic and the Community Hall have moved to the north side of the road (the saloon and the clinic in the east, the hall behind the carpenter and the library, up the gap between them), and their doors face south like the others. Their routes, the map picture and the villagers' walking times were redone.

- Quests: the screen follows one quest at a time. With one active quest it is followed by itself; with several, the Journal has a Track button on each (the Journal list scrolls). Only the followed quest's title and what is still to do show at the top right.
- Hover labels: props (fountain, clock tower, lamps, benches, stalls, barrels...), every shop and every cottage ("<Name>'s home"), the farm's coop, barn and greenhouse, and the items in a chest now say what they are when the mouse rests on them.
- The farm's hay, sacks, wheelbarrow and barrels can be moved with the builder's mallet.
- Sitting: Interact on a chair, armchair, couch or bench sits you down, facing the way the seat does; you rest slowly while seated (about 24 energy an hour); moving, using a tool or Interact again stands you up. Villagers who stand still next to a free seat sit on it too.
- The forest is no longer a lattice: thickets and clearings, no rows. The edges of the farm, the village, the forest and the beach are bands of trees two cells thick (rocks on the beach) instead of brick walls; the roads and lanes stay open.
- The map tab shows the new building places, the three ponds and the village's fountain and clock tower.
- Controllers that Windows shows only as a generic joystick (a DirectInput pad) now move and press buttons using a usual layout, and say what to do (XInput mode, or Steam Input for generic controllers) if the buttons do not fit. Unverified without such a pad.
- Selling: the general store's keeper has "Sell something" in the conversation menu: a window with the backpack, Sell 1 and Sell all, paid on the spot at the shipping bin's prices.
- Right-click is one item: in the backpack it lifts one item of a stack (click or right-click another slot to put it down), in the shipping bin it adds one to the lot, in the sell window it sells one, in a chest it moves one (before).

## 0.3.2 (2026-10-09)

- The trees change with the season, each tree its own kind: round green ones and willows in spring and summer, orange and yellow ones in fall, bare ones and conifers in winter (some keep the old pine). Art from the Cozy Village kit.
- The village has a clock tower and a fountain between the road and the shops, benches, flags, lamp posts along the road, flower boxes and barrels at the shops' doors, and new market stalls; the farm has hay, sacks, a wheelbarrow and barrels by the house. Tall things (trees, the clock tower, lamps) now hide a player who stands behind them.
- The builder's mallet instructions (the longest of any tool) no longer overflow: the Help tab rows and the box over the item bar are as tall as the words they hold.

## 0.3.1 (2026-10-09)

- The world is less flat. Tufts and flowers are scattered over the grass in the colors of the season (green in spring and summer, orange in fall, pale blue in winter), pebbles, driftwood and dune grass lie on the beach, there is a pond on the farm and one in the village, the beach shore wanders instead of running straight, and the shops have planters beside their doors.
- A walkthrough of the whole screen when the opening story is closed: a frame points at the status bar, the gold, the energy bar, the item bar and the quest list in turn, each with a caption that calls out its commands; then the controls together; then the menu opens and each tab is explained (Next, Back or Skip). The pause menu has a Menu tour button to see it again.
- Help: a new Help tab in the menu explains every tool, everyday things (eating, chests, quality, trees, wild plants, quests, sleeping) and the controls. The tool tip over the item bar now shows once per item per game.
- Twelve more pieces of music: each place has its own theme (the village, the forest, the beach, the saloon, the library, the mine, the shops and homes), the rain has one, and each season's festival has its own on the village on festival day.
- With a controller, the label of what you face (bed, kitchen, door...) shows as the mouse's does.

## 0.3.0 (2026-10-09)

- A guided tour of the menu: a few seconds into a new game (and once for a game still in its first days) the menu opens on the Journal and a box walks through every tab, saying briefly what each is for (Next, Back or Skip; the Menu and Journal keys are named at the end). It is offered once.
- The quest list stays in the Journal, as well as at the top right of the screen.
- New art from the Colored 1-bit Nature pack, scaled up to the game's 16-pixel grid: the wild mushroom and its picture, and tall sunflowers that grow in the meadows in summer and fall (cut them with the scythe for fiber).
- Carried from the 0.2.2 work that was not published: controller menus no longer lose the cursor behind Options; bigger trees and new tool pictures; acorns and saplings, faster regrowth of trees and wild plants (leave some, they spread); larger speech bubbles; the first note names no crop; outdoor light follows the day from half brightness at night to full at noon; a shorter first quest line and a smaller tool-help box.

## 0.2.2 (2026-10-09)

- Controller: the cursor no longer escapes onto the title menu hiding behind the Options screen (down from the last volume slider used to land on a hidden title button and snap back to the top, so nothing past the volume could be reached). Only the top screen can be navigated now, for every screen that opens over another.
- Trees are bigger (a 32 x 32 pine from the Mini Farm pack; only the picture grows, the tree still blocks one cell), and the tool pictures (hoe, axe, pickaxe, watering can, fishing rod) are new: the hoe no longer looks like the axe.
- More trees and plants: felled trees may drop acorns, and an acorn planted on bare grass on your farm becomes a sapling that grows into a tree in five days. New trees grow back on the farm about once a day (it was about one in five days), and wild plants grow back faster in the village, forest and beach. Plants spread from the ones left standing, so leave some: the hover label names wild plants and says when one is the last of its kind, and picking the last one tells you.
- The villagers' speech bubbles have larger text (and follow the Text size option).
- The first anonymous note no longer compliments your corn whatever you grow ("Your fields are doing well").
- The outdoor light follows the day: about half as bright at night as at noon, rising from dawn to noon and falling to night.

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
