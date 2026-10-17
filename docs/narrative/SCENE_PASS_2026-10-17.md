# Scene pass: every story scene rewritten by the villagers (2026-10-17)

Branch `writers-room`. **Nothing here has been compiled in Unity, tested in Unity or played.** The checks below are static (Python mirrors of the validator and the narrative lint, run in `tools/writers_room/scenepass/verify.py`).

## What was asked
Run the writers' room (each villager played by its own agent) over **every** story scene; the results are accepted as the dialogue. The aim: realistic, compelling lines that make the player want to chase the story, with variations, additions and subtractions for the dread levels and the three horror settings (off, mild, full).

## What was done
Scope: 177 story scenes, 74 scene dialogues, 1,114 owned text keys. Daily talk, barks and gifts are not in this pass.

1. **Round 1, cozy rewrite.** Twelve agents, one per villager, each rewrote the lines they speak (lines, dialogue nodes, the player's choice labels offered by their node; narration went to the scene's lead speaker). 889 strings changed, 36 beats added to the early heart stubs. Brief: `tools/writers_room/canon/scene_pass.md`.
2. **Round 2, coherence read.** Each villager re-read their scenes with everyone's new lines in place and fixed collisions, repeated catchphrases ("measure twice" was in 13 lines), impossible times of day, props that do not exist (goats, a photograph, a bakery), and lost callbacks. 171 strings changed. Brief: `canon/scene_pass_r2.md`; notes handed up: `drafts/scene_pass/r1_notes.md`, `r2_notes.md`.
3. **Round 3, horror layer.** Each villager wrote conditional variants of their own lines, plus added and cut lines, by horror setting, dread band and lore tier: 539 variants, 46 added lines, 10 cuts (353 mild-level, 186 full-only; 216 uneasy, 145 frightened; 78 hints, 83 clues, 16 revelations). Brief: `canon/scene_pass_r3.md`, canon sheet `canon/mythos_sheet.md`. Round 3b added variants to the 61 closing lines that branch on the player's choices.
4. **Stage fixes by the Director** (not text): Piper's heart 10 now answers the request she wrote down in heart 4 (three new conditional lines); Tilda is placed in the Marcus pumpkin scene; the Reading Night chair count follows whether Wren attends; narration lines in the boat trip describe only what the engine can draw. The music cues `hazel_theme` and `piper_theme` are kept as placeholders (the music is generated later).

## How the horror layer is attached
Data only; no C# changed; nothing in core data names horror.
- `Resources/Mythos/mythos_scenes.json`: 170 overlay events (`<id>__m`) and 58 dialogue copies (`<dialogue>__m`).
  An overlay is the cozy scene's copy at priority + 1 with condition `(<original>) && horror:1 && !flag:event.<id>`. Its spoken lines read `{if <most specific variant>}…{else}…{else}<cozy text>{/if}`; added lines carry their own `condition`; cut lines carry `!(<condition>)`.
- 146 once-only cozy scenes gained `&& !flag:event.<id>` so only one version plays (the flag is already set by every scene's effects). Repeatable scenes (festivals, 15 others) are not guarded.
- 494 new strings in `en.json`: `mythos.v.<original key>` (variant chains) and `mythos.scene.<id>.<n>` (added lines).
- At horror off the overlay never matches, so the game plays the cozy text; at mild the variants written for `horror:1` apply (no ritual imagery: the variant lint rejects robes, chant, altar, offering, sacrifice, ritual, Nharoth, blood, cult, covenant); at full the full-only variants (ritual named, distortion at dread 60 and up) come first.
- Not overlaid (7): the four festivals, `hall_ending`, `overheard_bram_marcus_2`, `overheard_tilda_juno_2` (left plain for contrast).

## Verified (static)
`python3 tools/writers_room/scenepass/verify.py` passes: fscript and en.json agree on all 1,047 dialogue texts; every event and dialogue resolves; every condition uses known atoms; every text (every `{if}` branch) passes the narrative lint for its speaker; no horror atom in core story JSON; overlay priority and guards correct; no test file pins a replaced string. `render_levels.py <scene> --horror N --dread D --lore L --hearts v=N` prints any scene as a player would see it.

## Not verified. Do these first
1. Open the project once: **Farm > Compile Story**, then run EditMode (`FScriptSourcesInSyncTests`, `NarrativeLintTests`, `StoryValidator` tests, the Mythos and Memories suites, `SpecScenesTests`). Check the compile keeps the 494 `mythos.*` strings in `en.json`.
2. Play a heart event at horror off, mild and full.
3. Read the new lines. Nobody but the agents has.

## Needs your decision
- **Hall Dance.** I kept the shipped structure with the Round 1 and 2 text. `hall_dance_v2` (the earlier 29-line table read with a player choice) is not merged. Say if you want it to replace the scene.
- **Daytime scenes written for the evening.** Several scenes play 9:00 to 17:00 but were written as "tonight/evening" (Odalys heart 8 against her "rare quiet evening", Juno heart 8 "the same night"). The text now says "today/afternoon". If you would rather move the start times, those scenes need their conditions changed.
- **Canon to confirm or cut** (invented by the villagers): Wren's packed bag as the way to get people out if "the fourth" fails (clue and revelation tiers); Tilda's heart 6 as her confession scene, and "the back of the slate" needing a payoff; Bram: Edmund bought nails four times a year and left a hoe at the forge; Felix: Edmund sat on his rock watching the tree line; Elara's list has 41 places, matching Dorian's 41 trees, and Dr. Penn crossed off Harrow Wood; Ione: a book set aside for Edmund; Hazel: Dr. Penn also suggested her as the festival reader; Juno: Edmund wanted his key unmarked and asked for a sword first, the hall's "woods-side door"; Bram keeps six of Juno's notes in his apron and thinks Tilda wrote them.
- **Wren's heart 4** follows the card (she sings her own bad song); the bible has her volunteer the player to meet Piper.
- **Memories page.** Overlays keep the scene title so a scene seen at mild or full can be replayed; the "seen N of M" counts will run slightly high at those levels because each scene has two versions.

## Revert
Per round: `git log --oneline -- Assets/_Project/Resources/Localization/en.json` shows one commit each for Round 1, Round 2, the stage fixes and the overlay. Removing `Resources/Mythos/mythos_scenes.json` (and its `.meta`) switches off the whole horror variation; the cozy guards (`!flag:event.<id>`) are harmless without it.

## Re-running
`tools/writers_room/scenepass/`: `corpus.py` (cards and ownership), `lint_lines.py`, `apply_round.py`, `lint_variants.py`, `build_overlay.py` (idempotent), `verify.py`, `render_levels.py`. The drafts, briefs and every villager's notes are in `tools/writers_room/drafts/scene_pass/`.
