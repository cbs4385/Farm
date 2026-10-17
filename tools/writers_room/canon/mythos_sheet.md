# The Mythos sheet (what the horror layer already says, so scenes agree with it)

Source documents: `docs/mythos/LORE.md`, `RITUAL-SCENES.md`, the clue dialogues in `Resources/Mythos/mythos_story.json`, `mythos_threads.json`, `mythos_reactions.json`. This sheet is the short version for writers; if it disagrees with LORE.md, LORE.md wins and you tell the Director.

## The truth (what a fully informed player eventually learns)
- **Nharoth** sleeps in **Harrow Wood**, beyond the forest gate that stays shut. It is not evil; it is *vast*. If it wakes the world is consumed in fire.
- The **Keepers of the Covenant** have kept it asleep for as long as Wetherell has stood. On the night of day 4 of every season (new moon, 22:00) they hold a **ritual** at the altar in the clearing: the leader speaks, then each Keeper lays an offering that dissolves into light. Offerings are village goods (animals never shown harmed), sometimes something the player owns. A missing Keeper or offering fails the ritual. Wakefulness rises 25% a season untouched and falls 30-40% per success.
- **The Keepers:** Tilda (store), Marcus (carpenter), **Dr. Odalys Penn** (clinic; the leader), Dorian (forager; lodges with Marcus), Wren (saloon; protective, not cold). They look menacing (silence, watching, leaving early on ritual nights) and are warm and protective. **Hazel** worked it out and works against them. **Unaware:** Bram, Juno, Piper, Felix, Elara, Ione.
- **Edmund Fenn**, the player's great-uncle, was the offering the fall before the game. The farm is "the bargain"; its owner is the one they keep in reserve. The way out: relics seal, bell and thread (in an old chest on the mine's 30th floor) and the true words (lore 8).
- Endings: Sealed / Joined / Ignored / Awakened.

## Edmund's threads (already planted, safe to echo)
the cellar door he would not let Marcus oil · the tiny brass key Juno made "for a box he'd lost the lock to" · "he'd put his affairs in order" (Dr. Penn) · the almanac margins that stop in the fall, four dates a year, always the same nights (Ione, Hazel) · "counting the trees": forty-one from the forest path, then the way in (Dorian) · the four notes he hummed (Piper hums them; "the room hums them back") · the mine survey in the almanac drawer, "under, thirty" (Hazel) · Tilda wrote the welcome letter.

## What the world does (wakefulness ladder, felt not stated)
birds quieter · longer morning fog · dogs avoid the forest path · milk sours · lamps lit early · the new moon looks large · odd crop shapes · a low hum at night · hushed voices after dark · red-edged full moon · wells taste of iron · Keepers abroad at all hours · shadows lean the wrong way · fog stays at noon · animals face the wood · the hum nearly a voice · no one sleeps through the night · the woods seem closer · warm ground · the sky waiting.

## The three levels (CLAUDE.md, LORE "Dread at mild and full")
| Setting | Condition atom | What you may write |
|---|---|---|
| **Off** | none | Nothing. The cozy text is the whole game. Never touch it from the overlay. |
| **Mild** (1) | `horror:1` | Unease, customs, hushed manners, a Keeper's flicker of worry, the player's tiredness, the woods mentioned. Clues allowed. **No** ritual imagery (no robes, chant, altar rites, offerings as such), no distortion of text, no dream horror. Dread effects at half strength: use only the *uneasy* band for mild. |
| **Full** (2) | `horror:2` | Everything mild gets, plus the sharper version: ritual named, Edmund's fate approached, the village's watching, dread-driven text distortion (only at `var:dread>=60`: a repeated word, a stammer, a clause that intrudes, never more than a short fragment). |

`horror:N` means **at least** N, so `horror:1` is true at full too. Always write the **full** variant first in the chain and the **mild** variant after it, then the cozy text.

## Dread bands (the player's unease, `var:dread` 0-100; thresholds already in use: 20/30/40/60)
- **Calm** `< 30`: no dread variant. The scene is the cozy scene unless a lore/hearts clue applies.
- **Uneasy** `>= 30` (and `< 60`): a villager notices something small: the light, a silence, the player looking tired, a half-finished sentence. Warmth intact.
- **Frightened** `>= 60`: the scene is visibly strained. Jokes fall flat or are cut; villagers watch the player or the window; Keepers are very gentle (it frightens more); text distortion allowed at full.
Mild uses `Uneasy` only (its variants use `var:dread>=30`, no `>=60`).

## Information tiers (what a scene may let slip; `var:lore`, `hearts:<villager>`)
| Tier | Gate | May say |
|---|---|---|
| Hint | `var:lore>=1` | an odd custom, a rule ("home before dark on the new moon"), a thing noticed |
| Clue | `var:lore>=3 && hearts:<speaker>>=4` | that something is kept, a duty, a cost, Edmund "chosen", the clearing |
| Revelation | `var:lore>=5 && hearts:<speaker>>=6` | the Keepers' purpose, what the ritual keeps asleep, that Edmund was the offering (full only) |
Tier gates stack on top of `horror:1`/`horror:2`. Revelation text exists at full only unless it mentions no ritual imagery.

## By allegiance
- **Keepers (Tilda, Marcus, Penn, Dorian, Wren):** completely warm on the surface; the tell is *care that is too exact* (Tilda's lamp, Marcus's "not alone after dark", Penn's "I know, I choose", Dorian counting, Wren's jokes landing a second late). They never confess in a scene that is not their clue scene; at hearts/lore gates they may let a true thing slip and cover it.
- **Hazel (resister):** pattern-spotting, almanac, bird counts, her own fear. At higher lore she may warn the player in her dry, exact way.
- **Unaware (Bram, Juno, Piper, Felix, Elara, Ione):** notice village oddness, never explain it. Their dread variants are *human*: the forge fire burns blue-ish low, the song loses a note, the boat won't leave the dock, the clinic is shut again, the sea doesn't smell. They are the player's mirror: if the player is frightened, they gently ask.

## Hard rules
- Original text only. No gore. No cruelty to animals on screen. No gendered address. American English. Lint budgets and voice limits as in the cozy pass (a variant line obeys the same word limits as the line it replaces).
- A variant never changes who speaks, what a choice *means* (flags), the scene's flags/effects, or items given.
- A variant must read right with **no** explanation: the player who skipped every clue still understands it as mood.
- Do not duplicate an existing clue dialogue's line (see list above); echo, don't repeat.
