# Milestone 3b completion plan (2026-10-06)

**Premise.** `STATUS.md` already marks X-000 to X-011 done (2026-10-02): both meters, Harrow Wood, five Keepers plus Hazel, rituals as systems, dreams, fog and blood moon, horror crops and mutants, the 20-step ladder, the intensity option, four endings, and conformance tests (`MythosLayerTests`, a six-year simulation, the PlayMode Woods test). Nothing of it has been played by a person. So "implement 3b" means: finish the listed gaps, then prove the layer by hand. (`CLAUDE.md` and the plan header still say "Milestone 2 is next"; fix them in step 0.)

## Gaps (from STATUS)
| # | Gap | Source |
|---|---|---|
| G1 | Sleepwalking and blight night events | X-005 |
| G2 | Moon-phase events beyond the ritual | X-006 |
| G3 | Sound layers for the 20 steps and for dread | X-008 |
| G4 | Text distortion filter at high dread (full only; `L.AddFilter` exists) | X-008 |
| G5 | Ending art and music; the mood of the endings | X-010 |
| G6 | Balance checked by simulation only | X-010 |
| G7 | No human playthrough at off / mild / full; Woods look, dread pacing | X-011 |
| G8 | Hand-check `HorrorLevel` 0 against the base game (T-044 equivalence) | X-011 |

## Steps
0. **Docs honesty (small).** Update `CLAUDE.md` and the plan header to the true state. Claim the rows in `STATUS.md`.
1. **G4 text distortion** (smallest, self-contained). A `Farm.Mythos` text filter registered on layer start, active only at `HorrorLevel` 2 and dread above a threshold; deterministic per string and day, never touches names, numbers or `{placeholders}`, never blocks reading. Tests: off and mild unchanged, placeholders intact, same seed gives same output, validator still passes.
2. **G3 sound layers.** Extend `AmbienceDirector`/`AmbienceSynth` with a low drone and sparse dissonant sounds keyed to wakefulness step bands (0-4 none, 5-9 faint, 10-14 audible, 15-19 constant), dread adds a heartbeat in the Woods. Mild halves volume; off is silent. Reuse the synth path (no imported audio); each layer fails soft. Tests: layer selection by step and level.
3. **G1 night events.** Sleepwalking (wake somewhere else on the farm, small fatigue cost, a clue; at most once a week, never at off) and blight (one crop or tree row withers overnight at high wakefulness; protected by a ward or a ritual success; never touches the base crops at off). Both as day-cycle hooks with flags, tests for off/mild/full and for save round-trip.
4. **G2 moon-phase events.** Two or three small events per phase tied to the existing moon variable (new, waxing, full, waning): foraging yield change, a Keeper sighting, a letter. Data only where possible.
5. **G5 endings.** Generate one illustration per ending (awakened, sealed, joined, ignored) through `tools/art/generate_sheet.py` (new `endings` set), a closing screen, and a music cue per ending (prompts in `docs/audio/MUSIC_PROMPTS.md`). Mild avoids explicit imagery.
6. **G6 balance.** Extend the simulation to a bot that investigates (clues, relics) and one that ignores; check the god never wakes for an attentive player, the failure is avoidable, and wakefulness is recoverable. Record numbers in `docs/balance/`.
7. **G8 off equivalence.** A test that a 40-day and a one-year run at level 0 with the Mythos module loaded matches a run without it (state hash).
8. **G7 manual playthrough** (owner). A checklist in `docs/QA.md`: new game at each level; first summer (Forest gate, Woods, stones, relics); one full ritual cycle by overhearing and following; each ending; the Content notes screen. Capture screenshots of the Woods and the step ladder from a dev build (`-farmCommands "var mythos.wakefulness 600"`). File what the owner finds as S1/S2.

## Order and size
G4 → G3 → G1 → G2 (code, each about a day, independent) → G5 (needs art and the closing screen) → G6/G8 (tests) → G7 (owner time). Steps 1-4 can be parallel agents. Each step: tests first, build and capture when scenes or UI change (`CLAUDE.md`), then update STATUS with what was verified by whom.

## Decisions needed from the owner
1. Is a sleepwalking wake-up spot allowed anywhere (including Harrow Wood) or only on the farm?
2. May blight destroy planted crops (with a ward to prevent it), or only damage yield?
3. Ending art style: one illustration each, or a short text-and-music sequence?
