# Ritual scenes: update and polish (2026-10-10, owner's request)

Scope chosen by the owner: the overheard scenes and the Keepers' clues, a **witnessed ritual** (new), the altar choices and the four endings; writing, staging **and new art**.
All of it is horror content, so it lives in `Farm.Mythos` and in data (`Resources/Mythos`, `en.json`). Nothing in the core assemblies knows about it except through the generic hooks of ADR 0002.
Intensity: **full** (2) gets the whole thing, **mild** (1) gets the quiet version (plain lights, no robes, no chant, no ritual imagery: LORE "Dread at mild and full"), **off** (0) nothing.

## What exists today (the audit)
- *Overheard*: one saloon event of two narrated lines.
- *Clues*: three one-node dialogues for each of the five Keepers, Hazel's three and her sabotage offer; every first clue has the same two answers and the same two replies ("Good. Thank you." / "Nothing you need to carry. Yet.").
- *The ritual*: runs as a system on the clock (`RitualDirector`); what a player sees is the Keepers at the altar by their night schedules and a few small squares of light (`AltarObject`). Nobody wears anything different. The first ritual is off-stage.
- *Altar*: one line and three choices; the sealing and the initiation fire the ending events straight away.
- *Endings*: three narrated lines each, over four good Gemini illustrations (kept).

## What this pass adds
1. **Voices.** Each Keeper's clues are rewritten in their own voice (`docs/narrative/STYLE.md`, `narrative_rules.json`: Marcus and Dorian no exclamations, short sentences; Odalys and Tilda warm and exact; Wren dry), with answers and replies of their own and a small gesture to remember them by.
2. **Staged whispers.** The saloon whisper becomes a directed scene (the booth, lowered light, two named speakers, a glass breaking at the bar), with a mild version, and a second whisper after the Woods open (the marks).
3. **Robes.** Keepers wear robes on ritual nights: new original sprites (a hooded figure per Keeper, 16 x 32, four facings) through a generic *outfit* hook (`INpcOutfitSource`), so the walk to the clearing can be seen on the road. Full intensity only.
4. **A ritual you can see.** The altar gets a ring of candles, embers, and the offerings shown as what they are (the item's own picture) lifted into light while they dissolve; the Keepers stand in a ring and answer their leader ("What sleeps is not dead." "We keep.").
5. **The witnessed ritual** (`mythos_ritual_witness`, full intensity, the Woods open): a directed scene when the player watches from the trees on a ritual night: the leader's call and the answers, each offering laid and taken into light, a **choice** at the last one (step in and take it, from the first summer; or stay hidden), then the outcome shown either way. One new full-screen illustration for the moment the last offering dissolves.
6. **The altar.** The sealing and the initiation become short staged scenes with a confirmation before the point of no return.
7. **Endings.** Rewritten (four to six lines each, in a consistent voice), with lighting and music, over the existing illustrations; the sealed and the joined endings gain a closing line that depends on how the player got there.

## Rules for the writing
Original text only (nothing from existing mythos works); no gore; animals never harmed on screen; the Keepers read as menacing and are protectors; every line within the lint budgets (talk 28 words, event 40, choice 9, per-sentence caps by voice); American spelling; `[player]` for the farmer, no gendered address.

## Verification
EditMode: story validator, narrative lint (locked voices), conditions, outfit hook, ritual scene builder (steps are valid for every ritual plan, both outcomes). PlayMode: the whisper plays, robes appear on the ritual night and not on other nights or at mild, the witnessed scene runs to the end for success, for a taken offering and for a missing Keeper, and leaves the world clean (camera, light, props, outfits). A person must watch it: STATUS keeps the list.
