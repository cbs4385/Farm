# NPC Interconnected Gameplay & Narrative: Implementation Specification (revision 2)

**Status:** agent-ready work packages; revised against the game at **version 0.7.3 (2026-10-16)**
**Replaces:** revision 1 (same file, commit `7d05223`), which predated the 0.7.1 text pass and treated finished work as open.
**Evidence for the status column:** `CHANGELOG.md` (0.7.1, 0.7.2), `docs/STATUS.md`, and a read of `Assets/_Project/Narrative/*.fscript`, `Resources/Story/*.json`, `Resources/Mythos/mythos_story.json`, `Scripts/Gameplay/Story/HallRooms.cs` and `Scripts/Mythos/*`. Nothing here was played. Statuses marked **Verify** were not confirmed by a test or a read of the data and must be checked first.
**Purpose:** make the villagers feel like people whose goals, relationships, dialogue, quests and the optional cosmic-horror layer reinforce one another, by closing the promises the writing already makes.

---

## 0. How to work

1. Read first: `docs/NPC_DESIGN_REVIEW_2026-10-16.md` (the findings this spec answers; its section numbers are used below), `docs/narrative/{BIBLE,BIBLE_ENHANCED,BIBLE_WAVE2,RELATIONSHIPS,GOALS,STYLE,FSCRIPT}.md`, `docs/adr/0002`, `0003`, `0004`, `docs/mythos/LORE.md`, `docs/mythos/M3b-COMPLETION-PLAN.md`, `CLAUDE.md` and `docs/BUILD.md`.
2. Claim a package in `docs/STATUS.md` before starting. Do one package per change set. Do not batch packages.
3. **Edit sources, not generated files.** Villager lines live in `Assets/_Project/Narrative/*.fscript` and compile to `Resources/Story/zz_fs_*.json` and the string table (`Farm > Compile Story`, or `-executeMethod Farm.Editor.FScriptCompiler.CompileAndExit`). Use `tools/narrative_edit.py` for checked edits to a line in both the source and the string table. `FScriptSourcesInSyncTests` fails when they disagree.
4. Never rename a shipped id (items, maps, NPCs, flags, vars, quests, dialogue keys, localization keys). Add ids or conditions instead.
5. Horror/cult content stays out of `Farm.Core/Data/Gameplay/UI`. It goes in `Farm.Mythos` and `Resources/Mythos`, gated on `horror:1` (mild) or `horror:2` (full) conditions. At `HorrorLevel` 0 the game must behave exactly like the base game: no Mythos line, flag write, object, clue or event.
6. Hooks are failure-isolated and tested. New story state goes in flags, vars or module data, **not new `GameState` fields**.
7. Most packages below are content only. **WP1 and part of WP2 need small code and map changes** (marked **CODE**). That is allowed; keep the change minimal and say so in the completion report. Do not add a general framework.
8. Every change leaves the project compiling with all tests green. A bug fix needs a test that failed first. Bump `bundleVersion` and `CHANGELOG.md` for a player-visible change. Run filtered tests per package; the full EditMode and PlayMode runs happen once at the end (`docs/QA.md`).
9. State honestly what was verified by a person and what by automation. Do not claim a playtest that did not happen (`docs/STATUS.md` records both).

### Definition of success

The player can answer, through play: what does this villager want, why does it matter, who else is involved, what can I do, what visibly changes, and (horror on) what does it reveal. A resolve is complete when the player sees a consequence or takes part in a scene, not when a line says it is resolved.

---

## 1. Status ledger

Status: **Done** (shipped, with evidence), **Partial**, **Open**, **Verify**.

### 1.1 Review section 2 (promise and payoff)

| Review item | Status | Evidence / what remains |
|---|---|---|
| 2.1 Wren's heart 10 retitled | **Done** | `memory.wren_heart10` is "One Listener" (0.7.1). |
| 2.1 Wren's full house is staged | **Open** | `wren_full_house` quest takes the wine; no gathering event exists. Heart 10 outro already says "I wrote the map into the second verse". See WP2. |
| 2.2 Hall roof is a hall room | **Open** | `HallRooms.Rooms` has six rooms (pantry, crafts, fishtank, boiler, bulletin, vault); `hall.restored` is set when all six are done; `marcus_planks` (`enhanced_quests.json`) feeds no room. See WP1 **CODE**. |
| 2.3 Early confessions (`wren.cl.5`, `juno.co.1`, `ione.co.1`) | **Done** | 0.7.1: Wren's waits for the heart-8 bag scene; "years ago" removed. |
| 2.4 Juno's gold, Bram's "Your Tool" | **Done** | 0.7.1: quest says the bar is for the die and the first piece is copper on purpose; Bram's heart 10 is "Soft Gold". |
| 2.5 Elara's four props | **Done** | 0.7.1: map behind the kettle and the list; the atlas is Ione's set-aside book. |
| 2.6 Letter-joke template, warmest room, lamp | **Done** | 0.7.1: confidant joke rewritten per villager (Bram's three words and Juno's forty pages stay); forge is warmest, Hazel corrects Wren; the lamp is Tilda's alone. **Verify** with a grep for `co.3`/`co.4` that no template shape remains. |
| 2.7 Experts asking for their own stock | **Done** | 0.7.1: Felix, Dorian, Dr. Penn want the player to make, find or pick it. |
| 2.8 Small contradictions (Bram's age, stool, bible line) | **Done** | 0.7.1: Bram forty-something; Felix's wobbly stool; wave 2 bible fixed. Tilda's Sunday "come back tomorrow" line is **Open** (schedule pass, low value). |

### 1.2 Review section 3 (links that are only text)

| Link | Status | Package |
|---|---|---|
| Hall roof / Marcus | **Open** | WP1 |
| Wren's full room, Piper's song sung without her | **Open** (heart 10 already has the saloon sing it) | WP2 |
| Bram's teacher's reply | **Partial**: `bram.goal.r1` says "Letter came back" in a line; mail infrastructure exists (`quests_and_mail.json`, `bram_tools`, `ione_recipe`), no letter | WP3 |
| Juno's own forge / Bram's tongs | **Partial**: `juno.goal.r2`, `bram.goal.r2` and `overheard_juno_bram_4` are talk and one overheard scene | WP4 |
| Felix's boat / Elara's sea | **Open**: talk lines only | WP5 |
| Hazel and Ione's reading night | **Open**: talk lines only | WP6 |
| Dorian shows Hazel the notebook | **Open**: a line | folded into WP6 |

### 1.3 Review section 4 (horror layer)

| Review item | Status | Evidence / what remains |
|---|---|---|
| 4.1 Keeper clue gates (2, 4, 6 hearts), Hazel's clues, Odalys's invitation on 6 hearts | **Done** | 0.7.1, with a test. |
| Sabotage prompt asked twice | **Done** | `STATUS.md` (after 0.6.9): `mythos.hazel.sabotage` now sets `flag:mythos.sabotage.asked`; `MythosScenesPlayOnceTests` holds it. |
| 4.2 Keepers react to being sabotaged | **Open** | Flags exist (`sick:` effect sets `MythosIds2.Sick(npc)`; `ritual:snatch`; `mythos.interference`); no Keeper line reads them. See WP7. |
| 4.3 Dread legible in the unaware six | **Open** | Only `birthdays.fscript` and `courtship.fscript` use dread. See WP8. |
| 4.4 Edmund's mysteries harvested | **Open** | Seeds are in `edmund.fscript` and cameos; the second uncle letter just arrives in the mailbox. Needs an owner decision on resolution. See WP9. |
| 4.5 Four notes a deliberate Mythos motif | **Open** | The notes appear in `edmund.fscript`, `elara`, `hazel`, `juno`; `Farm.Mythos` does not treat them as one motif. See WP9. |
| 4.6 Keeper romance variants (Wren, Dorian) | **Open** | WP9. |
| 4.7 Marcus's roof as the Keepers' roof | **Open** | One clue line; depends on WP1. WP9. |

### 1.4 Not yet audited (do this before Phase 4)

Reactivity backlog (section 5), gossip and relationship-matrix sync, the 0 to 2 heart variety floor, rain-line reachability at every tier. The review did not read barks, gazette, name reactions or social actions. **Verify** each against the narrative lint and reachability tests before writing new lines.

---

## 2. Cast constraints (design limits, not permission to overwrite canon)

Keep the motivation model per major villager: surface want, emotional need, fear, behavioral tell, a dependency on another villager, a player contribution, a payoff, and an optional Mythos reading. Choose one or two recurring motifs per want and let others notice them.

| NPC | Want / motif | Payoff owed | Package |
|---|---|---|---|
| Tilda Ashby | A full village; kindness slate, the lamp left on | Slate records a real milestone (the dance, or the farm as a supplier) | WP2 |
| Bram Hollis | Good work; Juno's first piece; word from his old teacher | Pride said once, plainly; the teacher's letter in the mailbox | WP3, WP4 |
| Ione Fairweather | Readers; set-aside books | Reading night | WP6 |
| Marcus Dell | The hall roof, done properly | Roof is a hall objective with a visible change | WP1 |
| Dr. Odalys Penn | A village that rests | Mythos: earned invitation (**Done**), reactions to sabotage | WP7 |
| Wren Calloway | A full room and a story; packed bag | The full house, staged | WP2 |
| Felix Hartwell | A dawn companion | The boat trip | WP5 |
| Juno Hale | Her own first piece; Bram's spoken pride | Her own forge scene | WP4 |
| Hazel Brandt | Be heard once, on her terms | Reading night; agency over the poem | WP6 |
| Piper Vance | A song the village sings | Sung in the Hall Dance, with the player's word | WP2 |
| Dorian Lake | A friend for silence; the notebook | Quiet part in reading night | WP6 |
| Elara Finch | The sea from a boat | The boat trip; map and list are the only props | WP5 |

Interconnection rules still hold: every major arc touches another villager consequentially (mutual support, quiet care, affectionate rivalry, shared mystery, or the player carrying something); not everyone is equally close; nobody is cruel to the player.

---

## 3. Work packages (in dependency order)

Each package lists what it needs, how it triggers, the files, and **assertable** acceptance checks (a flag, a mail item, a priority, a test), not "pays off".

### WP1: The roof is a room of the hall (**CODE**, data)

**Why:** review 2.2. The most-referenced shared goal has no slot in the system that resolves it.
**Design (decided by the owner):** add `hall_roof` as a **seventh room**. Marcus's planks (wood) are its input. `hall.restored` is set only when all seven are done.
**Save compatibility (required):** a save that already has `flag:hall.restored` must keep it and must not be asked for a roof. On load, if `hall.restored` is set and `hall_roof` is not started, mark the roof done silently (no ending replay, no reward). Never clear a set flag.
**Files:** `Scripts/Gameplay/Story/HallRooms.cs` (`Rooms`, `FlagOf`), `Resources/Story/hall_and_festivals.json` (new quest `hall_roof` with the planks objective; `hall_complete` waits for it), `Scripts/UI/HallScreen.cs` (a row for the roof; check layout with seven rooms), `Assets/_Project/Narrative/marcus.fscript` and `enhanced_events.fscript` (`marcus_planks` outro points at the hall; Marcus's "Not nearly. Fixed." stays gated on `flag:hall.restored`), `Scripts/Gameplay/Achievements/Achievements.cs` (no change expected), `gazette3.fscript` (no change). A visible change: use the existing hall restoration holder (`VillageRestored` in `MapBuilder.AddVillageRestorations`) so the hall shows a roof change when the room is done; regenerate with `SceneSetup`, do not hand-edit scenes.
**Acceptance (tests):** with six rooms done and the roof not, `hall.restored` is unset and Marcus's resolve line is unreachable; with the roof done it is reachable and the ending plays once; a save with `hall.restored` set loads with the roof done and nothing replays; `marcus_planks` completion advances `hall_roof`. HallScreen shows seven rows without clipping (capture, `docs/QA.md`).
**Depends on:** nothing. **Blocks:** WP2.

### WP2: The Hall Dance and Wren's full house (Scene A; text plus event)

**Why:** review 2.1, 5.3. One scene closes the full house, the roof, Piper's song and the forge.
**Trigger:** `flag:hall.restored` (so the roof is done, WP1) and `quest:wren_full_house=done` and `flag:event.piper_heart10` (the song exists) and not `flag:event.hall_dance`. Fires once (`flag:event.hall_dance` is set by its own outro and ruled out in its own condition, the `MythosScenesPlayOnceTests` invariant).
**Format (decided by the owner):** an **evening event** that places only the six participants (Marcus, Tilda, Wren, Piper, Bram, Juno) in the hall; it does not replace the festival schedule for the day. Everyone else appears as cameos in the existing cameo format (`cameo_reactions.fscript`).
**Beats:** Marcus checks the roof and says it is done; Wren hosts a full room and uses the wine from `wren_full_house`; the village sings Piper's song with the word the player chose (read the stored choice the way `piper_heart10` does; if no choice is recorded use the default word); Bram hums; Juno's bell is on the door; Tilda's slate gets a kindness; end on two villagers in a changed relationship.
**Reuse:** an existing event template (`enhanced_events.fscript` / `slice_events.fscript` pattern, with `{if choice:...}` branches) and the schedule entries that place a villager at a stop, so each of the six is in the hall at the event hour and walks home after. If the six cannot all be placed with existing hooks, propose a smaller version rather than a new system.
**Acceptance:** cannot fire before WP1 completes (test by setting flags in each order); fires once; sets `flag:event.hall_dance`; at least two participants (Tilda, Marcus) have `heard:`/flag-gated follow-up lines the next day and the dance is not retold in them; Hazel's heart 10 stays distinct (one listener). Retitle nothing else: Wren's heart 10 remains "One Listener".

### WP3: Bram's teacher writes back (Scene E; content plus mail)

**Why:** review 3. The mailbox already carries villager mail.
**Trigger:** the existing Bram chain state that `bram.goal.r1` uses (`flag:event.bram_heart10` and `heard:bram.goal.a1`); deliver one mail item the next morning.
**Files:** `Resources/Story/quests_and_mail.json` (a new mail id; copy the shape of `bram_tools`), `bram.fscript` / `goals.fscript` (r1 line refers to the letter's contents without overexplaining; keep Bram terse).
**Acceptance:** the mail is delivered once, only after the chain; Bram's later line references the actual words ("Good work."); the letter's tone is restrained; no new mail system.

### WP4: Next Door (Scene D; Juno's forge)

**Trigger:** both `flag:event.juno_heart10` and `flag:event.bram_heart10`. Reuse the dialogue already written in `overheard_juno_bram_4` and `juno.goal.r2`/`bram.goal.r2`.
**Beats:** Juno unveils her piece (the copper first stamp is the visible object, matching the quest); a practical flaw; Bram offers the good tongs; Juno decides whether to accept (a choice with a callback, no punishment for either); Bram says one direct sentence of pride. Marcus may add a fitting only if the relationship data supports it.
**Acceptance:** the item named in the scene is the item the quest and journal describe; one-time flag `flag:event.next_door`; Bram stays terse (lint); the scene does not duplicate Bram's heart 10 gift.

### WP5: Past the Point (Scene B; the boat, a decorated scene)

**Decided by the owner:** the boat trip is a **real scene, appropriately decorated**. It is not a rewritten promise.
**Trigger:** the existing resolve conditions (Felix on Elara's heart 8; Elara on Felix's hearts), plus a dawn hour, weather that is not a storm, and not `flag:event.past_the_point`. Reuse `felix.goal.r2` and `elara.goal.r1`; the invitation line is only said when the scene can fire.
**Where it plays (decided by the owner):** the **Beach** map, with a conditional-object holder (the pattern of the festival holders in `MapBuilder.AddFestivalGrounds`, using `ConditionalObject`) that appears only while the scene is available. Only if a capture of the beach's east shore shows the holder cannot fit without blocking an exit, stop and report before building a separate map.
**Decoration (generated by editor scripts, never hand-edited scenes):** a rowing boat in the shallows (the `prop_fest_boat` from the pond is the starting point; a larger one-off if it reads too small), a short wooden jetty or landing, a lantern post lit for the dawn, coiled rope, a net on a rack, Felix's tackle box and flask, two oars, a gull or two (existing ambience), and the horizon: a visible sea edge with a low sun. Any new sprite goes through the placeholder art generator with a documented prompt in `docs/art_prompts/` (original art only); mark them placeholder until the art pass.
**Beats:** Felix prepares the boat with understated care (walks the jetty, checks a rowlock); Elara chooses a destination from her list (a short choice of three, each its own line, stored for callbacks); the boat leaks politely and is bailed with competence and humor; the camera pans out to the horizon; Elara's list gets a crossed-out entry (a journal update, not a new prop); if Wren chose "go" she is on the point looking out, without her decision being forced to match; on the way back Felix says one thing that shows he got his dawn companion.
**Acceptance (assertable):**
- the scene fires only from the stated trigger, once, and sets `flag:event.past_the_point`;
- the decoration holder exists only while the scene is available (test the conditional object) and is gone after;
- the three destination choices each set a distinct flag, and Elara and Felix each have a later line that reads it;
- the map and the list are the only sea-longing props; Felix is never asked to gather his own bait;
- a capture of the Beach with the holder active (Windows player, `docs/QA.md`) shows the boat, jetty and lantern on walkable shore and not blocking the beach's exits;
- horror levels 0 and 2 behave the same (no Mythos content).
**Needs:** art placeholders and a map change (`MapBuilder` only, no new system). Claim it separately from the content packages.

### WP6: Reading Night (Scene C)

**Trigger:** `flag:event.hazel_heart10` (already the one-listener climax, so this is a different scale) and Ione's want state. Participants Hazel, Ione, Dorian; Wren optional.
**Beats:** Ione offers the gathering (a real accept/decline); Hazel chooses whether to share (agency); Dorian contributes a pressed leaf or drawing by action, with no speech required; Wren's stories bridge to the written word.
**Acceptance:** declining leaves Ione's want open without starving ordinary chat (reminder policy test); accepting sets `flag:event.reading_night`; Hazel's heart 10 is not repeated.

### WP7: Keepers react to being sabotaged (Mythos data)

**Why:** review 4.2. The resist ending shows the beat; play does not.
**State:** `ritual:snatch` and the `sick:` effect (sets `MythosIds2.Sick(npc)`); both only fire with horror on and `mythos.interference`.
**Content:** two state-gated lines per affected Keeper in their own voice, in `Resources/Mythos/mythos_story.json` and the Mythos scripts: hurt and withdrawn trust, never villainy. Tilda's slate no longer lists the player the next morning; Marcus is quiet and measures something else. Ordinary friendship pools must not overwrite the reaction for at least the next in-game day (use priority and an expiry flag set by the reaction).
**Acceptance:** reactions fire only for the affected Keeper and only after the matching state; none at horror 0 (parametrized test over `HorrorLevel`); the player is not locked out of friendship (a later recovery line exists at hearts gate).

### WP8: Dread shows in the people (Mythos data)

**Why:** review 4.3. Dread cools friendship silently, which reads as a bug.
**Content:** two lines each for Bram, Juno, Piper, Felix, Elara, Ione at a dread threshold, conditioned in Mythos data only, in each villager's habits ("Sit. You look like iron quenched too fast."). They show concern and never reveal the cult or the mechanic.
**Acceptance:** lines appear only when dread >= threshold and horror on; a test proves none are reachable at horror 0; no gating of ordinary or quest lines.

### WP9: Edmund's threads, the four notes, Keeper romance and the roof clue (Mythos; **owner choice per item**)

**Why:** review 4.4 to 4.7. Each Edmund item below is a thread the cozy layer plants and nothing harvests. **Decided by the owner:** every item follows the recommendation column. **P** = pay it off in 1.0; **U** = a deliberate long-term mystery or a closed human beat, to be recorded in `docs/mythos/LORE.md` (do that in the first WP9 change set). The cellar door and its box are **Mythos-only**: at horror 0 there is no cellar object and the door is simply stuck. Where the table says "U or P" (E5), build the P version: a Dorian Mythos line that tells the player how to find the way in.

**Rules for any payoff:** the cozy topics in `edmund.fscript` stay exactly as written; the payoff lives in `Farm.Mythos` and `Resources/Mythos`, gated on horror on and the lore variable; it reuses existing objects, inventory, mail and clue systems; it cannot soft-lock the ending; it is invisible at horror 0 (parametrized test).

**The twelve topics (one per villager, asked once, `edmund.<npc>` in `Narrative/edmund.fscript`):**

| # | Thread | Source | Current state | What a payoff would be | Rec. |
|---|---|---|---|---|---|
| E1 | The cellar door Edmund would not let Marcus oil | `edmund.marcus`, tag mystery | Seed only. No cellar object on the farm map. | A cellar door on the farm map (conditional object, Mythos-gated) that opens with the key (E2) onto a box holding the second letter. At horror 0 the door is simply a stuck cellar door (no object). | P |
| E2 | The tiny brass key Juno made for "a box he'd lost the lock to" | `edmund.juno`, tag surprise | Seed only. No key item exists. | Juno gives the key (a new item, `mythos.brass_key`) once lore >= 2, in a Mythos-only Juno line. It opens E1's box. | P |
| E3 | "He'd put his affairs in order" | `edmund.odalys`, tag mystery | Answered in part by the truth letter (he knew he was the offering). | Odalys's reaction line once lore >= 4: she knew what "in order" meant and could not stop it. No new object. | P (cheap) |
| E4 | Almanac margins that stop in the fall, "Done well enough" | `edmund.ione`, tag mystery | Seed only. The almanacs are not an object. | Hazel's clue 2 (a pattern in the records, four times a year) cites Edmund's almanac margins by name; the margins' dates are the four ritual dates. Ione's later line reads them aloud. | P |
| E5 | Counting the trees at the field's edge at dusk | `edmund.dorian`, tag mystery | Seed only. | The count is the number of trees to the woods' entry (a Dorian Mythos line that tells the player how to find the way in), or it stays atmosphere. | U or P |
| E6 | Four notes, "leave it open" | `edmund.piper`, tag wholesome | Appears in Edmund's, Piper's, Elara's, Hazel's and Juno's lines and the night hum, but `Farm.Mythos` does not treat it as one motif. | Declare it the motif (night hum, Piper's pre-set notes, Edmund, the sealed ending plays the notes finished or left open). The cozy reading stays a musician's habit. No fifth note. | P |
| E7 | One book a year, the same one, renewed by post, now overdue | `edmund.hazel`, tag wholesome | Unseeded and unresolved: which book is never said. | Make it the book on the Mythos bookshelf that names the seal, the bell and the thread; Hazel's "I've decided that it isn't" becomes a callback. | P (cheap) |
| E8 | The last egg basket with a card, "thank you for everything" | `edmund.elara`, wholesome | Closed, a human goodbye. | Optional callback after the truth letter; otherwise leave. | U |
| E9 | He stayed for all of Piper's last set and bought a round | `edmund.wren`, wholesome | Closed, a goodbye. | Optional: Wren's reaction after the truth letter. | U |
| E10 | The open parsnip tab; the Friday fish reports | `edmund.tilda`, `edmund.felix` | Closed, wholesome. | Optional cozy callbacks (Tilda closes the tab after the Hall Dance; Felix keeps Friday's seat). No Mythos. | U (cheap callbacks welcome) |
| E11 | "A man ought to know his own edge" (the scythe) | `edmund.bram`, wholesome | Closed. | Optional line when Bram sharpens the player's scythe. | U |
| E12 | The porch built for pickles | `edmund.marcus`, first line | Closed. | None. | U |

**The letters and the finding (`Resources/Mythos/mythos_story.json`, and `quests_and_mail.json`):**

| # | Item | Condition today | Gap | Rec. |
|---|---|---|---|---|
| L1 | `uncle_letter` (cozy) | start of game | None. | keep |
| L2 | `mythos_uncle_note` ("I wrote kindly in the first letter...") | `horror:1 && var:lore>=1` | It just arrives; its text says it was behind the stove but nothing is found. | P: a Mythos-only interaction at the farmhouse stove delivers it (a conditional object, no new system). |
| L3 | `mythos_warning` | `horror:1 && var:lore>=2` | Arrives by mail with no source. | U (a warning in the mail is fine) |
| L4 | `mythos_uncle_truth` ("the offering for the fall dark"; a seal, a bell and a thread) | `horror:1 && var:lore>=4 && flag:woods.open` | Arrives by mail though it says "you have found the wood". | P: found at the stone or in the cellar box (E1) instead of by post, or both. |
| L5 | The three things that can end it: a seal, a bell and a thread | named in L4 | The relics are in the mine (`RelicChest`, `MythosRelics`); nothing tells the player where. | P: Hazel's records (E4) and the book (E7) point to the mine; no new relic system. |

**Other parts of WP9 (separate change sets):**
1. **Romance variants.** One Mythos variant each for Wren's "go" branch and Dorian's partners line. Romance is never a prerequisite to the central mystery.
2. **Roof clue.** One Marcus clue line after WP1 ("A roof that's finished gets used...").

**Acceptance:** each chosen P has a trigger, a flag and a test; the chain steps (E2, E1, L2, L4, L5) cannot soft-lock the ending in any order the player can reach them; every U is listed in `docs/mythos/LORE.md`; the `docs/mythos/` canon files are updated in the same change; nothing appears at horror 0.

### WP10: Variety and reactive talk (after WP1 to WP6) **Built 2026-10-16 (wave 1); see the status note below**

**Status (2026-10-16, version 0.7.4).** The audit is a set of tests that fail on gaps (`NarrativeCoverageTests`: a floor of lines at every friendship tier, rain lines at every tier that nothing outranks, morning, evening and night lines, a line for each reactive state, and a time-bound line check; the table is written to `Builds/narrative_coverage.txt`). It found rain the thinnest area (3 to 5 lines), the `recent:` condition used by no villager, `shipped:` used only by Tilda, and six villagers (Ione, Marcus, Odalys, Felix, Dorian, Elara) with pools about 40 percent smaller than the other six. Wave 1 wrote 144 lines in `Narrative/variety_wave1.fscript` (rain by friendship for all twelve, a recent quest and a recent gift, 50 and 500 items shipped, farm animals, and for the six morning, evening, night and one line a season). No existing line was edited. Not done: moon-at-night lines (there is no moon condition), upgrade-progress lines for the shopkeepers, and further waves.

Audit first (section 1.4), then add lines in small batches: Tilda, Bram, Ione, Wren, then the rest. Requirements per talk set: 0 to 2 hearts keeps multiple reachable ordinary lines; friend and close tiers add variety without shadowing; rain lines are reachable at every tier; seasonal and hour lines carry matching conditions; quest offers, reminders and turn-ins outrank casual lines. Add reactive lines only when an existing condition atom supports them (festival countdown, a recent milestone, farm progress, shop upgrades, moon at night); each is specific and temporary. Gossip stays tied to `docs/narrative/RELATIONSHIPS.md` and its tests.

### WP11: Polish (not a blocker) **Audited 2026-10-16; see the status note below**

Portrait expressions where art exists, text speed and accessibility, voice blips, conversation-end friendship feedback. Only after content works.

**Status (2026-10-16, version 0.7.4).** All four items were audited by tests (`PolishAuditTests`, `DialoguePacingTests`, `StreamModeTests`). Every villager has every expression picture, and every expression the story names exists; every villager has a voice and a signature sound of their own; text speed (slow, normal, fast, instant) and the stream-mode floor were already covered. The one gap was friendship feedback: a "heart gained" notice sat under the dialogue box and expired during the conversation, so notices now wait until the conversation closes (`HudView.AdvanceToast`, `UiService.DialogueOpen`). Anything that needs new art (portraits, animation) is not drawn as a placeholder: it goes to the art pass.

---

## 4. Dialogue and systems rules (kept from revision 1)

- Every line sounds like its speaker without a name label, and does a job: character, goal, relationship, information, world reaction, or recontextualizing an earlier detail.
- No generic gratitude tails; no repeated metaphors or joke shapes across the cast; no secret before its scene; time-bound statements carry the matching condition or are rewritten as timeless.
- An invitation phrased as a question must lead to a real accept/decline or be rewritten as an observation. No scene is promised unless it can fire.
- Deterministic dialogue selection (no reroll-for-new-lines). Check that a higher priority does not make quests, seasonal, rain or friendship lines unreachable.
- Condition and effect syntax follows `docs/narrative/FSCRIPT.md`. New keys pass the narrative lint and localization checks. Every new condition or effect has a test.
- Cozy content lives in the normal layer; no cosmic-horror vocabulary appears in ordinary dialogue at horror 0; foreshadowing must read as ordinary village life.

---

## 5. Tests and review checklist

**Per package:** the reachability and lint tests for the narrative files changed; a test for each new gate, flag and one-time behavior; the invariant that a story-band scene sets a flag its own condition rules out (`MythosScenesPlayOnceTests`); `FScriptSourcesInSyncTests`; parametrized horror-0 tests for every Mythos addition; the `LocalizationTests` lint.
**Progression:** each event requires its correct prerequisites, fires once, and has valid participants, effects and follow-up flags; the roof gate, the letter delivery and the Juno item/reward match each have a failing-first test.
**Human review for every changed arc (all answers must be yes):** is the want concrete; is the need distinct; does the villager act in a way only they would; does another villager materially affect it; does the player do something meaningful; is there a visible or remembered payoff; is it consistent with timeline, schedule, relationship state and voice; does the cozy reading survive horror off.
**Capture:** any change to a scene, the hall screen or UI layering needs a Windows player build and screenshots (`docs/QA.md`).

---

## 6. Decisions

**All made by the owner (2026-10-16):**
1. **WP1:** the roof is a seventh hall room.
2. **WP2:** the Hall Dance is an evening event placing only the six participants.
3. **WP5:** the boat trip is a real, decorated scene on the **Beach** map, using a conditional-object holder.
4. **WP9, Edmund items:** follow the recommendation column. Pay off (P): E1 cellar door, E2 brass key, E3 "affairs in order", E4 almanac margins, E5 counting the trees, E6 four notes, E7 the overdue book, L2 the note found at the stove, L4 the truth letter found at the stone or in the box, L5 the seal, bell and thread pointed to the mine. Keep as mystery or closed beat (U): E8 to E12 (with the cheap cozy callbacks in E10 welcome), L3 the warning in the mail, L1 unchanged.
5. **WP9, horror 0:** the cellar door and its box are Mythos-only; at horror 0 the door is just stuck.

**Nothing is waiting on the owner.** Start with WP1 (it blocks WP2 and WP9's roof clue), then WP3 and WP4 (content only), then WP2, WP5 and WP6; WP7 and WP8 are independent Mythos data and can run beside them; WP9 follows WP1.

---

## 7. Completion report template (per package)

1. Summary (what changed and why). 2. Files changed (exact paths). 3. Ids added or changed; state that existing ids were preserved. 4. Behavior changes (gates, flags, quest states, items, mail, event prerequisites, save handling). 5. Tests: suites and commands run, pass and fail counts, baseline failures reported separately. 6. Manual QA: what a person inspected, or state it was not played. 7. Known gaps and why. 8. Canon updates (bibles, relationship matrix, lore, `STATUS.md`, `CHANGELOG.md`).

## 8. Source-of-truth hierarchy

When documents conflict, resolve explicitly: 1) current runtime behavior and save-compatible ids and state; 2) accepted ADRs (`docs/adr/`); 3) the narrative bibles and Mythos lore; 4) the relationship matrix and goals; 5) existing dialogue scripts and quest and event data; 6) this specification. If a higher source is itself inconsistent, stop and record the conflict before a canon-changing edit.

**End of specification.**
