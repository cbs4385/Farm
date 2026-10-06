# Horror layer playthrough checklist (X-011, by a person)

Everything in the horror layer (Milestone 3b) has been checked by automation only: tests, a six-year simulation, balance bots and a handful of screenshots. Nobody has played it. This checklist is for the person who does, once at each intensity. File what you find as S1 (blocks play or corrupts a save), S2 (wrong or confusing, play continues) or S3 (polish) in `docs/STATUS.md` under "Found by a person".

## Before you start
- Use the Windows release build (`Builds/Windows/<version>/Farm.exe`) for the real feel, and a development build (`-development`, `BuildsDev/`) when you need to jump the calendar. Developer console: F1.
- Three new games, one per intensity: **Off**, **Mild**, **Full** (Options > horror intensity; default Full). Name the saves so you can tell them apart. Play the first year of each as you normally would: farm, talk, go to the village, fish, mine.
- Keep a short log per game: day and season, what you noticed, anything odd. Screenshots (F8 photo mode, or the capture flags in `docs/QA.md`) of the Woods, the first fog, the first blight and each ending are the most useful.
- Time-savers in a development build: `date summer 1`, `var mythos.wakefulness 500` (permille, so 500 is 50%), `var dread 60`, `var lore 5`, `flag woods.open`, `effect ending:sealed`, `tp Woods`. Use them to reach a state, then play on from there.

## 1. Off (the cozy game, nothing may change)
- [ ] The first year plays as the plain farming game: no fog or blood moon weather, no dreams or warnings in the day summary, no strange letters, no tint creeping in, no sound under the weather.
- [ ] The Forest path north stays brambled all year (the way into Harrow Wood does not open) and nothing in dialogue mentions the Keepers, Nharoth or the wood.
- [ ] The Content notes screen says the horror layer is off.
- [ ] Nobody acts strangely at night; villagers keep their normal schedules (Hazel, Tilda, Marcus, Odalys, Dorian, Wren are on their usual rounds on new-moon nights).
- [ ] Moon phases still show in the calendar and there are no moon "events" in the day summary.
- [ ] Switch the option on mid-game (Mild), reach the next morning, switch back to Off: the cozy game resumes with nothing left behind (the layer's state is kept, but nothing shows).

## 2. Mild (half strength, no explicit ritual imagery)
Spring and early summer:
- [ ] Dread is invisible (no meter). Notice only through the world: a line in the day summary now and then ("You dreamed of tall trees and woke uneasy"), a colder tint, the odd eerie random event.
- [ ] The first **summer day 1**: the day summary says the brambles have withered and the way into Harrow Wood is open; the quest "The Path Opens" appears and ends when you first stand in the wood.
- [ ] A **warning letter** or overheard conversation in the saloon after 20:00 points at something without explaining it.
- [ ] **Harrow Wood**: the way in, the track, the clearing, the altar, three lore stones and three relics. Read the stones. The wood looks watchful, not gory; there is no explicit imagery. A heartbeat plays when dread is high (half volume).
- [ ] **Ritual night** (the last new-moon night of each season, day 4, from 22:00): Keepers are away from their usual places; the altar shows plain lights, not offerings dissolving. Hiding in the wood to watch is allowed.
- [ ] **Sound**: from about 25% wakefulness a low drone sits under the weather (half volume); it never masks dialogue or the effects you act on. Tones appear from 50%.
- [ ] **Text distortion does not happen** at Mild (no scrambled words in dialogue, even at high dread).

## 3. Full (everything)
Do the Mild list again, then:
- [ ] **Text distortion**: with `var dread 80`, talk to three villagers: a few words in what they say stumble or scramble ("w-w-where", swapped letters); names, `[player]` and numbers never break; choices and clue lines stay readable. At dread below 50 nothing is distorted.
- [ ] **Sound layers**: with `var mythos.wakefulness 600` the drone is clearly audible and the tones come and go; at 800 it is constant. In the Wood with `var dread 70` a slow double heartbeat plays. Check it sits under music and effects and the Options ambience volume controls it. Does it get annoying over ten minutes?
- [ ] **Blight**: from about 40% wakefulness (`var mythos.wakefulness 400`) sleep a few nights with a field of crops: some wither overnight and stay as obviously dead plants; the day summary says so. Clear one with the scythe. A scarecrow within range keeps its crops safe. After a successful ritual that season nothing withers.
- [ ] **Sleepwalking**: with `var dread 80` and `var mythos.wakefulness 300`, sleep several nights: now and then you wake somewhere else (farm, village, forest, beach, the wood once open), a little tired, with a line saying so; never twice in a week.
- [ ] **Moon events**: new moon, waxing, full and waning each give their own small events in the day summary; the full moon at 50%+ wakefulness turns into a blood moon.
- [ ] **Offerings**: in the journal (lore 4 and up) the marked offerings are listed; take one from the altar during a ritual (from the first summer): the ritual fails and nothing is lowered.
- [ ] **Dread effects**: luck, weather (more fog and storms), friendships fade a little, horror crops (nightbloom, hollowroot, ashfruit) only grow when dread is high; mutants appear among ordinary crops.

## 4. Story paths (Full, with dev commands to save time)
- [ ] **Clues and Hazel**: talk to Tilda, Marcus, Odalys, Dorian and Wren over a few days (each has three clue lines that appear as lore grows); Hazel's three clues need lore 1, 2 and 5 on different days; her third starts the sealing quest.
- [ ] **Seal (resist)**: find the three relics in the wood (`var lore 5` first), then use the altar outside a ritual: the sealing scene plays, the **sealed illustration** appears with its caption, wakefulness drops to nothing, the layer stays quiet afterwards and you can keep playing.
- [ ] **Join**: with the cult's regard high (`var cult.standing 20`, `flag mythos.initiated`) the altar offers the joining: the **joined illustration** appears, play continues.
- [ ] **Ignore**: play into year 3 without touching any of it: the quiet ending shows the **ignored illustration** at a dawn, play continues.
- [ ] **Awakening**: `var mythos.wakefulness 1000` (or a saboteur game): the burning scene plays, the **awakened illustration** appears, then the goodbye message and the main menu. The save is kept.
- [ ] Each ending: read the caption and look at the picture at the window size you play at; are they right in mood? The pictures are generated art: tell us if one is wrong (a face, a symbol, anything that should not be there).
- [ ] At Mild none of the pictures or captions should be more graphic than the others; all four are meant to be fine at Mild.

## 5. Things to watch the whole way
- [ ] Is it ever scary when it should be cozy, or cozy when it should be uneasy? Where does the pacing drag? Does anything feel unfair (a ritual failing by accident because you shipped a crop, a night event at a bad time)?
- [ ] Performance: 60 fps in the Wood, no hitches when the fog or blood moon starts, no growing slowdown by the end of year 1.
- [ ] Saving and loading in the middle of: a ritual night, a lifted building (builder's mallet), a blighted field, a held sleepwalk morning. Quit and reload each; nothing may be lost or duplicated.
- [ ] Alt-tab, pause and the pause menu during a ritual and during an ending.
- [ ] Gamepad: reach the Wood, read a stone and use the altar with a pad only.
- [ ] The Options screen: change the intensity at each stage and check the Content notes screen tells the truth.

## When you are done
Update the manual-verification lines in `docs/STATUS.md` (X-005a, X-006a, X-008a, X-008b, X-010a, X-011a): say who played, which build, which intensity and what you found. Anything that passes can then lose its "not seen by a person" note.
