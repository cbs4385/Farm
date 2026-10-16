# Mythos and Horror Layer Review: Recommended Changes

**Project:** Wetherell Farm Saga  
**Repository:** `cbs4385/Farm`  
**Document type:** Agent-ready narrative/design change specification  
**Status:** Recommendations for implementation; not canon until implemented and validated  
**Primary references:** `docs/mythos/LORE.md`, `docs/mythos/M3b-COMPLETION-PLAN.md`, `docs/mythos/RITUAL-SCENES.md`, `docs/01-GameDesign.md`, `docs/NPC_DESIGN_REVIEW_2026-10-16.md`, `CLAUDE.md`

---

## 0. Instructions to the implementing agent

Treat this document as a **targeted improvement plan**, not permission to redesign the entire mythos.

1. Read the primary references above and inspect the current implementation before editing.
2. Preserve all established canon and stable IDs unless a recommendation explicitly says otherwise.
3. Implement recommendations in the priority order in section 10. Do not attempt every optional polish item before completing the critical narrative fixes.
4. Put horror-only content and logic in `Farm.Mythos` and its data. Do not add horror checks to core assemblies (`Farm.Core`, `Farm.Data`, `Farm.Gameplay`, `Farm.UI`).
5. Use existing generic hooks, condition/flag/variable systems, localization via `L.Get(key)`, and existing scene/data-generation workflows. Do not hand-edit Unity scene or prefab YAML.
6. Keep the horror intensity contract inviolable:
   - `HorrorLevel = 0`: the game behaves as the cozy base game. No cult, mythos clues, horror events, mutations, horror audio, or horror dialogue leak into play.
   - `HorrorLevel = 1` (mild): preserve the mystery and story, but omit explicit ritual imagery and text distortion, avoid animal offerings, and use the milder visual/audio/text variants defined in the lore.
   - `HorrorLevel = 2` (full): the complete folk-horror/cosmic-dread presentation.
7. Do not implement gore, graphic animal harm, real-world religious analogues, or copied Lovecraft/other franchise text or characters.
8. Add tests for changed conditions and state transitions. Update `docs/STATUS.md` only for work actually completed; distinguish automated validation from human playtesting.
9. If a recommendation conflicts with existing shipped behavior or a newer owner decision, stop and report the conflict rather than silently changing canon.
10. At completion, report changed files, tests run and results, remaining gaps, and any assumptions. Do not claim human playtesting unless a person actually performed it.

## 1. Executive assessment

The core premise is strong and should be retained: a warm farming/life sim sits above a slow folk-horror mystery; the people who look most sinister are protecting the world; and the player may cooperate, ignore the danger, or resist at a cost. Nharoth is most effective as an immense, largely unseen presence whose effects are inferred through ordinary village life.

The main weakness is **not a lack of horror ideas**. It is that several excellent ideas are currently isolated from one another, while a few high-impact story beats do not yet earn their emotional or mechanical consequences.

The recommended direction is to make the horror layer feel like **one coherent hidden history** rather than a collection of spooky effects:

- Edmund Fenn's death, the farm inheritance, the four-note motif, the Keepers' seasonal work, Hazel's resistance, and the three sealing relics should be parts of one causal chain.
- The Keepers' kindness and their frightening behavior must coexist. The reveal should change the player's interpretation of prior behavior, not simply announce that the apparent villains are good.
- Villagers must react to what the player actually does. Sabotage, rising dread, discovery, romance, and ritual participation should alter relationships and dialogue in character-specific ways.
- Each clue should answer a small question while raising a more consequential one. Avoid lore dumps that explain the whole cosmology early.
- The player's actions must have visible, comprehensible outcomes. A ritual cannot be a lengthy spectacle that changes a hidden value but leaves every character behaving as if nothing happened.
- The cozy game must remain complete and emotionally satisfying when horror is off. Mild mode should be a genuine lower-intensity presentation of the same mystery, not a broken or content-starved mode.

**Recommended priority:** first fix narrative coherence and reactions; then connect the existing clue motifs; then improve ritual readability and pacing; finally polish sound, text distortion, and endings. Do not add more monsters or more lore nouns as a substitute for these fixes.

## 2. Canon and constraints to preserve

The following are established project decisions, not optional suggestions:

- User-facing title: **Wetherell Farm Saga**. Repository and code name remains `Farm`.
- Location: remote rural New England village of Wetherell, bordering Harrow Wood.
- Cosmic entity: **Nharoth**, a sleeping entity from before the current world. It is vast and dangerous to the world if awakened, but must not be reduced to a chatty villain with ordinary human motives.
- Cult: **Keepers of the Covenant**. Their manner and symbols are ominous; their purpose is to keep Nharoth asleep.
- Resistance: Hazel opposes the Keepers. Her motives and methods should be legible, even when her understanding is incomplete.
- The five Keepers are Tilda, Marcus, Dr. Odalys Penn, Dorian, and Wren. The other six villagers are unaware; Hazel is the active resister.
- The player inherits the farm after distant great-uncle **Edmund Fenn** was selected as an offering in the autumn before the game begins.
- The Keepers' seasonal ritual takes place on day 4 at 22:00 at the Harrow Wood altar. Required offerings scale by season (2/3/4/5). Missing an offering or required Keeper causes failure. Success lowers wakefulness; failure does not.
- Wakefulness and dread are separate hidden values. Do not display either as a HUD meter. Convey them through world state, behavior, journal, events, and atmosphere.
- The wakefulness ladder contains 20 stages, one for each 5% increment, escalating from subtle unease to catastrophe.
- Player-facing horror levels are off, mild, and full. Default is full. The cozy loop must remain complete at off.
- Main endings: sealed, joined, ignored, awakened. Preserve their intent unless the owner explicitly approves a larger narrative change.
- The Community Hall remains a cozy storyline and a place the Keepers quietly use.
- Original content only. Do not use existing Mythos franchise names or copied fiction other than the project's approved original names.

Before editing, verify the current branch and inspect current IDs and implementation. This document intentionally uses conceptual IDs such as `flag:mythos.interference` where appropriate; reuse existing stable IDs if they already represent the same state. Never rename shipped IDs just to match this document.

## 3. Primary design problems and required outcomes

### H-01 — Critical: the Edmund thread is seeded but not harvested

**Problem:** Cozy dialogue seeds several memorable facts about Edmund: the cellar door, Juno's brass key, Odalys saying he put his affairs in order, Ione's almanacs whose notes stop in autumn, Dorian's tree-counting, and Piper's four notes. These details currently risk feeling like disconnected flavor because the horror layer does not consistently connect them to the central mystery.

**Required outcome:** At least three of these existing details become a single discoverable chain that leads the player from Edmund's ordinary life to his knowledge of the Keepers, the ritual, and the possibility of sealing Nharoth. Preserve the cozy dialogue; add horror-only callbacks and journal synthesis.

**Implementation:**
1. Inspect the existing Edmund topics, Juno's key data, Hazel's clues, Ione's almanac dialogue, the farm map's conditional-object support, and the two mythos uncle letters.
2. Make the second mythos letter and the relevant evidence discoverable through the existing cellar/box/key motif rather than appearing as unexplained mailbox exposition. Use an existing conditional-object/interaction hook if available; do not create a bespoke interaction framework.
3. Have Hazel's archive/records clue explicitly connect the seasonal pattern to Edmund's almanac annotations. Her inference should be evidence-based, not omniscient.
4. Tie Dorian's tree-counting to the clearing's location or to a recognizable boundary pattern. Keep the clue subtle enough that it does not immediately identify Nharoth.
5. Ensure the journal records each discovery and that later entries reinterpret earlier clues rather than repeat their text verbatim.
6. Gate all new evidence behind the horror intensity and the necessary discovery/relationship conditions.

**Acceptance criteria:**
- A player can trace a clear, testable clue chain from at least three existing Edmund seeds to a concrete next action.
- No horror-only object, letter, line, or clue appears at `HorrorLevel = 0`.
- The player is not required to read every villager's dialogue to understand the main route.
- No evidence arrives before the game has introduced the object or person it depends on.
- Existing cozy Edmund dialogue remains valid and does not become a forced horror exposition scene.

### H-02 — Critical: the four-note motif should be deliberate and payoff-bearing

**Problem:** The same four-note phrase appears in Edmund's memories, Piper's music, the wakefulness ladder, and ritual behavior. The recurrence is evocative but can feel accidental unless the player can recognize its meaning and receive a payoff.

**Required outcome:** Establish the four notes as a recurring sonic/story motif that links Edmund, Piper, the Keepers' work, and the sealing ending.

**Implementation:**
1. Audit every current occurrence of the four-note motif. Create one authoritative description in the lore/data documentation so pitch, rhythm, and narrative meaning do not drift.
2. In the full-intensity layer, let the player recognize the phrase in at least three contexts: an ordinary village performance or memory; a horror clue or ritual; and the sealing finale.
3. Preserve Piper's emotional ownership of the tune. She should not suddenly become an exposition device who understands the whole cosmic situation. She knows the melody and its emotional history; the player assembles its wider meaning.
4. The sealed ending should resolve or complete the motif in a way the player can hear or read as closure. Do not explain the entire meaning in a speech.
5. Mild mode retains the motif as a subdued musical/story clue, without the most disturbing sound treatment. Off mode must not introduce the mythos interpretation.
6. Reuse existing audio synthesis/ambience paths and data patterns. Do not require a new audio asset pipeline.

**Acceptance criteria:**
- The same motif is recognizable across its uses.
- At least one journal entry or character response helps the player connect the occurrences.
- The sealed ending pays off the motif; the awakened ending may leave it unresolved or distort it, consistent with the established tone.
- The motif never leaks into the off experience as supernatural content.

### H-03 — Critical: the Keepers must react to sabotage and lost trust

**Problem:** The player can take offerings or interfere with a Keeper's availability, but the post-interference social world may continue as if nothing happened. This undermines the emotional cost of resistance and makes the Keepers' relationships feel cosmetic.

**Required outcome:** A failed ritual changes the village's behavior in a restrained, character-specific way. The Keepers are hurt, suspicious, and protective—not generic hostile NPCs.

**Implementation:**
1. Inspect the actual ritual outcome flags, sabotage actions, existing duplicate-trigger bug notes, and relationship/dialogue condition priorities.
2. Create or reuse a single durable state indicating that the player interfered with the Covenant's work. Prefer a reason-coded state if the existing system supports it (offering taken, Keeper unavailable, altar damaged); do not create redundant flags if one state can express the needed distinction.
3. Add two or more post-interference lines per Keeper, with priority and expiry rules so they do not permanently suppress ordinary dialogue.
4. Keep each response in voice:
   - **Tilda:** warmth becomes careful; she may stop recording small favors or stop leaving a useful item where the player can find it.
   - **Marcus:** measures, repairs, or works in silence; anger is expressed through craft and restraint rather than threats.
   - **Odalys:** speaks clinically and records the event. Her concern is the danger to the village, not a melodramatic confession.
   - **Dorian:** stops leaving foraged gifts or avoids the player's path, but does not turn cruel.
   - **Wren:** remains socially fluent, but the easy invitation or private performance disappears for a time.
5. Include a later recovery route. The player should be able to earn back trust through a concrete action, truthful discovery, successful alternative sealing work, or a later ritual contribution. Do not make one mistake permanently destroy a romance or the entire cozy loop.
6. Keep Hazel's involvement morally complicated. If she helped the player sabotage a ritual, her reaction should reflect the consequences and her own uncertainty.
7. Ensure the ritual result and reaction are emitted once, even if several conditions change at the same time.

**Acceptance criteria:**
- After sabotage, each affected Keeper has a visible reaction within the next relevant interaction/day.
- The reaction is not identical across all five Keepers.
- Normal cozy dialogue returns when its conditions expire or trust is repaired.
- No core gameplay progression is blocked by a relationship penalty.
- Tests cover repeated triggers, save/load, missing offering, unavailable Keeper, successful ritual, and recovery.

### H-04 — High: clue delivery breaks character when friendship is low

**Problem:** Mythos clues can currently be triggered by lore and Keeper status without a meaningful relationship gate. This lets characters reveal intimate secrets to a stranger and can conflict with the cozy introduction's promise that trust matters.

**Required outcome:** The player earns personal disclosures through familiarity, while environmental clues remain discoverable without forcing a friendship grind.

**Implementation:**
1. Audit all `mythos.<keeper>.clue*` and Hazel clue conditions and their priorities.
2. Use the following as a default relationship ladder, adapting only where current heart-event logic requires:
   - Clue 1: at least 2 hearts or a directly observed environmental clue that does not require confession.
   - Clue 2: at least 4 hearts and the relevant earlier clue/discovery.
   - Clue 3: at least 6 hearts and the relevant earlier clue/discovery.
3. Gate Odalys's personal invitation on a meaningful relationship plus the existing standing/lore requirement. Standing alone should not make a private invitation feel transactional.
4. Do not gate critical progression solely behind friendship. Every mandatory revelation must have an alternative evidence route (journal, object, overheard scene, or another villager's clue).
5. Check dialogue priority collisions so a clue does not endlessly override birthdays, heart scenes, seasonal lines, or urgent reactions.
6. Keep the off-mode cozy dialogue unchanged.

**Acceptance criteria:**
- No character reveals a private confession to a zero-heart stranger unless the player directly witnessed the relevant event.
- The core sealing route remains completable without maxing friendship with a specific NPC.
- The clue order is understandable and the conditions are covered by automated tests.

### H-05 — High: dread has mechanical consequences but too little human expression

**Problem:** Dread influences luck, weather/event weights, dialogue options, friendship decay, and crop conditions, but the unaware six can appear not to notice the player's growing unease. Relationship decay without dialogue feedback may feel like a bug.

**Required outcome:** Dread becomes legible through compassionate, ordinary reactions from villagers who do not know the truth. They notice fatigue, distraction, pallor, or avoidance; they do not diagnose cosmic horror.

**Implementation:**
1. Add two short, conditional response lines for each unaware villager: Bram, Juno, Piper, Felix, Elara, and Ione. At least one should be practical or kind; one may reflect concern or mild frustration at a higher dread band.
2. Use the existing dread variable and data-driven dialogue conditions. Avoid per-frame checks or new manager components.
3. Lines should respond to visible human behavior rather than naming the hidden stat. Examples of intent (write final lines in each character's voice; do not copy these verbatim unless appropriate):
   - A practical villager offers a chair, food, or help with a task.
   - A musician notices the player is listening for a sound that is not there.
   - A clinician notices poor sleep without claiming to know the cause.
   - A friend notices the player is distracted and asks whether they should leave the subject alone.
4. Mild mode can retain these human observations but remove the most disturbing phrasing. Off mode must not imply supernatural cause; do not let the horror-only dread state leak into off mode.
5. Make friendship-decay changes small, bounded, and recoverable. Ensure the player receives sufficient feedback to understand why social behavior has changed.
6. Avoid repeating a concern line every conversation. Use day/condition gating or existing dialogue cooldown support.

**Acceptance criteria:**
- Every unaware villager has at least one dread-responsive line and one higher-threshold variation.
- Dialogue never labels or exposes the dread meter.
- Lines remain believable if interpreted as stress, exhaustion, or ordinary worry.
- Dread effects are zero at off, halved at mild as designed, and bounded at full.
- Save/load and threshold-crossing tests pass.

### H-06 — High: romance and personal wants do not acknowledge Keeper obligations

**Problem:** Wren and Dorian are romanceable Keepers, but the romance/partner layer does not sufficiently acknowledge the tension between private attachment and the Covenant's duty. Wren's leaving/going arc also has an unused connection to the fact that a Keeper leaving at the wrong time can endanger a ritual.

**Required outcome:** Romance reveals the cost of the Keepers' secret without making romance coercive or turning the partner into a quest reward.

**Implementation:**
1. Add one horror-specific variant to Wren's existing leaving/going resolution. Her choice to stay should acknowledge that she has a duty she cannot explain yet; do not imply the player has trapped or saved her.
2. Add one partner line for Dorian that communicates the risk of the woods and the Covenant's attention, without demanding that the player obey him.
3. For any Keeper romance, preserve consent, mutual respect, and the NPC's independent agency. Do not use romance as a required key to the ending.
4. If the player has sabotaged the Covenant, ensure the partner can express hurt and concern without automatically ending the relationship.
5. Keep all variants in `Farm.Mythos` data, with correct localization keys and conditions.

**Acceptance criteria:**
- At least one Wren and one Dorian line acknowledge the tension between affection and duty.
- No romance route is required to seal Nharoth or join the Covenant.
- No line frames intimacy as payment, leverage, or proof of loyalty.
- Off-mode romance content remains the established cozy version.

### H-07 — High: ritual outcomes need stronger causal feedback

**Problem:** Rituals have specific rules and a long timed sequence, but players may not understand which offering or participant caused a failure, what success changed, or why the village reacts differently afterward.

**Required outcome:** Rituals remain mysterious in meaning but clear in immediate cause and effect. The player should understand what happened without seeing a meter or receiving a numerical exposition dump.

**Implementation:**
1. Audit ritual scene timing, offering assignment, participant selection, altar object state, success/failure resolution, and journal updates.
2. On success, show one restrained environmental change during or shortly after the ritual (e.g., the hum recedes, fog thins near the path, or animals resume ordinary behavior). Select a cue that matches the current wakefulness band; do not imply the world is permanently safe after one success.
3. On failure, use the actual failure reason to select a clue: an empty place at the altar, an offering missing, or a Keeper's absence. Do not expose hidden implementation terms like `wakefulness = 350`.
4. Record a short journal entry that distinguishes observation from inference. Example structure: “The last light went out before the final words. By morning the fog had not lifted.” Avoid stating as fact what the protagonist could not know.
5. Ensure one failed ritual cannot trigger multiple overlapping failure scenes or duplicate sabotage prompts.
6. Do not punish the player with an unexplained permanent crop loss or arbitrary relationship collapse. Failure's principal consequence is the lost reduction to wakefulness plus a legible narrative reaction.
7. At mild, render offerings as plain light and remove explicit ritual imagery. Animal offerings must never show harm; at mild, animals are not selected.

**Acceptance criteria:**
- Players can infer whether the ritual succeeded, without a numeric HUD.
- Each failure reason has an appropriate, non-duplicated feedback path.
- Journal language separates facts from suspicion.
- The ritual remains scheduled even if the player sleeps; player fatigue only affects the player's own state as currently designed.
- Off mode has no ritual event or related state transition.

### H-08 — Medium: the wakefulness ladder should feel like a narrative progression, not twenty unrelated filters

**Problem:** Twenty 5% thresholds are a useful content scaffold, but simple tint/fog escalation can become repetitive and may make the entire game visually muddy before the story has earned it.

**Required outcome:** Each band introduces a meaningful new signal, and escalation varies across sound, animal behavior, light, social routines, crops, and space. Keep the cozy palette readable for most of the game.

**Implementation:**
1. Audit the current twenty-stage ladder in `LORE.md` and its implementation. Preserve the established sequence unless a specific stage is demonstrably redundant or visually unreadable.
2. Group the stages into five authored bands for implementation and testing:
   - **0–19%: Unease.** Small omissions, unusual silence, slightly longer fog, a dog avoiding a path.
   - **20–39%: Pattern.** Repeated crop shapes, odd well taste, village routines shifting, the hum appearing at night.
   - **40–59%: Recognition.** The player sees that multiple signs share a source; stronger moon/woods signals at full intensity.
   - **60–79%: Intrusion.** Shadows and fog behave wrongly; villagers lose sleep; familiar spaces feel less reliable.
   - **80–99%: Imminence.** The woods seem nearer, the ground is warm, the sky appears to wait. Do not turn every frame into maximum fog or distortion.
3. Give each band at least one non-visual cue and one behavioral/world cue. Keep changes reversible or safely cosmetic where practical.
4. Apply the strongest visual effects selectively to Harrow Wood and night-time scenes. Do not obscure crops, UI, interactables, or navigation.
5. Ensure full-intensity-only elements (such as the blood moon and stronger inscriptions) never appear at mild.
6. Preserve the terminal 100% outcome: awakening is catastrophe, not a normal farming state. The player should receive enough warning to understand that the world is approaching an irreversible point.
7. Validate at multiple display sizes and on the built player, not just in the Editor.

**Acceptance criteria:**
- Each of the five bands has distinct authored behavior/sound/visual cues.
- Visual escalation never obscures essential gameplay.
- A human can identify a meaningful difference between at least three adjacent bands without a meter.
- No ladder content appears when horror is off.
- Threshold changes do not fire repeatedly when loading a save at a boundary.

### H-09 — Medium: text distortion and sound need restraint and accessibility

**Problem:** Distortion and dissonance can strengthen the atmosphere but can also interfere with readability, become repetitive, or overwhelm the cozy identity.

**Required outcome:** Audio and text effects add tension without making required information inaccessible or creating avoidable sensory barriers.

**Implementation:**
1. Follow `docs/mythos/M3b-COMPLETION-PLAN.md` for the existing ambience-synth path and text-filter extension point. Do not import a new audio framework.
2. Keep text distortion full-intensity only and only above a dread threshold. Never alter character names, item names, numeric quantities, quest objectives, localization keys, or interpolation placeholders such as `{player}`.
3. Distortion must be deterministic for a given text/day/state so dialogue does not change randomly on repeated reading. It must not remove the meaning needed to complete a quest.
4. Provide a clean way to read the underlying journal/quest information; if a passage is distorted, the journal's factual record should remain understandable.
5. Make ambience layers sparse and context-sensitive. The woods may carry a low drone; high dread may add a heartbeat-like layer; neither should play constantly in every map.
6. Off means no horror sound. Mild reduces intensity and excludes the explicitly prohibited full-only elements. Avoid relying only on volume reduction if the sound's texture itself is disturbing.
7. Test transitions, map exits, pause/menu states, and save/load so layers cannot get stuck playing.

**Acceptance criteria:**
- Text distortion preserves placeholders, names, numbers, and quest comprehension.
- Same seed and state yield the same distortion.
- Audio levels and selection respect intensity and wakefulness/dread bands.
- No horror audio leaks into off mode or remains active after leaving the relevant scene.
- Accessibility and ordinary dialogue readability remain acceptable.

### H-10 — Medium: endings should resolve the player's emotional and thematic questions

**Problem:** The four endings have strong high-level concepts, but the player needs a clear sense of what their choice means for the village, the Keepers, Edmund's legacy, and the central motif. The ending must not be a checklist with a generic final text.

**Required outcome:** Each ending provides a distinct emotional interpretation of the player's relationship to the mystery while remaining within the existing four-ending design.

**Implementation:**
1. Audit current ending triggers, ending data, scene/audio status, and `docs/mythos/M3b-COMPLETION-PLAN.md`.
2. Give each ending a short, authored sequence with three parts: immediate sensory consequence, one character-centered consequence, and a final image or sound that echoes the game's central motif.
3. **Sealed:** the player uses the seal, bell, thread, and true words; Nharoth is put to sleep permanently, dread recedes, and the village is allowed to return to ordinary life. Resolve the four-note motif subtly.
4. **Joined:** the player accepts initiation and the self-offering. The Covenant continues its work; the ending should feel like a willing but costly commitment, not a cartoon villain victory.
5. **Ignored:** the player continues farming while the Keepers carry the burden. Preserve the cozy life, but let the final beat acknowledge the unresolved cost without shaming players who chose a low-horror experience. Only trigger this ending under its established conditions.
6. **Awakened:** the world ends in fire and chaos. Keep it brief and deliberate; do not imply the player can continue normal farming afterward in the same state.
7. Preserve the off-mode experience. The mythos ending system must not run or surface when the layer is disabled.
8. Use original art/audio within the current project's production constraints. If final assets are not ready, use a deliberate, polished interim presentation rather than an empty scene.

**Acceptance criteria:**
- Every ending has a unique immediate consequence, human consequence, and final motif.
- Ending text does not contradict established lore or each character's knowledge.
- Endings are reachable only through their intended conditions.
- The build has no missing audio/art references or dead-end UI.
- Automated tests cover trigger conditions; human review checks emotional clarity.

## 4. Story structure and pacing rules

Use these rules when writing new lines, clues, events, or journal entries.

### 4.1 The player should always know the next practical step

Mystery is not the same as confusion. At every stage, the player should have at least one actionable route: follow a person, inspect a place, find an object, compare a record, attend a ritual, or ask a trusted villager. Avoid putting the only next step in a low-probability random bark.

### 4.2 Reveal in layers

Use this order as a default, adapting to existing implemented progression:

1. **Ordinary oddity:** a missing offering, a dog avoiding the wood, an unfinished tune, an unusual absence.
2. **Pattern:** clues recur on the same dates or around the same people/places.
3. **Personal connection:** Edmund, the inherited farm, and the player's place in the history.
4. **Moral reversal:** the Keepers are frightening because their work is frightening; they protect the village.
5. **Choice and cost:** help them, resist them, or pursue the sealing alternative.
6. **Resolution:** the player sees the consequence of their choice in the village and the motif.

Do not disclose Nharoth's full nature in the opening. Do not make Hazel fully correct about every fact; she should be perceptive and determined, but her resistance must have risks and gaps.

### 4.3 Keep the Keepers frightening without making them villains

The Keepers should never behave like a generic evil cult for its own sake. Their menace comes from secrecy, ritual precision, watching, and the willingness to bear a terrible duty. Seed small acts of care before the reveal: a repaired roof, medicine left quietly, a light left on, or help given without asking credit. Do not over-explain these acts. Their second meaning should become apparent after the player learns what they protect.

### 4.4 Do not confuse mystery with arbitrary punishment

A player who fails to infer a hidden rule should not lose the game without warning. Catastrophe should follow comprehensible accumulated consequences. Dread effects on luck, weather, and relationships must remain bounded, legible, and recoverable. Never let horror RNG silently destroy irreplaceable progress.

### 4.5 Protect character voice

- **Tilda:** warmth, practical care, quiet accounting; after betrayal, she withdraws small courtesies rather than threatening the player.
- **Marcus:** craft, measurement, pauses, repair; he shows emotion through what he does or refuses to do.
- **Dr. Odalys Penn:** precise, clinical, controlled; fear leaks through the details she records and the routines she cannot maintain.
- **Dorian:** plain, observant, nature-grounded; he says little and notices what others overlook.
- **Wren:** social fluency, wit, performance; silence or a missing invitation should hurt more than a melodramatic speech.
- **Hazel:** skeptical, active, emotionally invested; she questions explanations and can be wrong about the consequences.
- **Bram, Juno, Piper, Felix, Elara, Ione:** unaware of the cosmic truth. Their lines should remain grounded in ordinary care, fatigue, practical concerns, and relationships. They may notice that the player is troubled, but should not suddenly explain the supernatural plot.

All new player-facing text must use localization keys through `L.Get(key)`. Do not embed final dialogue as hard-coded strings in C#.

## 5. Data and implementation constraints

Before adding a new field, class, hook, or flag:

1. Search for an existing generic condition, story flag, variable, dialogue variant, module-data slot, day-cycle hook, weather modifier, atmosphere layer, conditional object, or gated warp that can express the requirement.
2. Reuse stable IDs where semantics match. If new IDs are necessary, add them to the established ID registry and tests; do not rely on untracked magic strings.
3. Keep state serializable and save-compatible. A new story state belongs in existing flags/variables/module data rather than a new `GameState` field unless the architecture documentation explicitly permits it.
4. Keep horror-only dependencies within `Farm.Mythos`. Core modules must not reference mythos classes or names.
5. Isolate optional hooks so a missing or malformed horror data entry cannot crash the base game.
6. Use the established editor scripts to generate scenes, prefabs, and content. Do not manually edit scene/prefab YAML.
7. Inspect existing tests and add narrow tests before broad refactoring. A bug fix should have a test that fails before the fix.
8. Maintain Linux-safe paths and APIs.
9. Do not add general-store sales for horror seeds, crops, or rare mythos goods. Existing shop opt-in behavior is intentional.
10. Do not change the hidden-meter presentation into visible HUD bars. Use world cues and journal language.

## 6. Test plan

Add or extend tests for every implemented recommendation. At minimum:

### Intensity matrix
- Off: no cult NPC behavior, mythos dialogue, ritual, mythos letter, horror crop mutation, text distortion, horror sound, or ending trigger appears.
- Mild: clues and endings remain available; explicit ritual imagery, animal offerings, blood moon, and full-only text distortion remain absent.
- Full: full layer works without bypassing the cozy game's normal systems.

### State and progression
- Wakefulness increases on the expected calendar schedule and decreases only after a successful ritual.
- Failed ritual does not lower wakefulness and does not double-apply failure effects.
- Threshold cues fire once and remain stable through save/load.
- Player can complete the sealing route through valid evidence paths without requiring one specific romance or max friendship.
- Story state round-trips through save/load.
- Endings cannot trigger early, twice, or in a contradictory combination.

### Dialogue and relationships
- Clue relationship gates are respected.
- Dread-responsive lines appear only in the correct bands and do not repeat without bound.
- Sabotage reactions appear once, match the cause, and expire or resolve through recovery.
- Priority conditions do not suppress heart events, birthdays, seasonal dialogue, or urgent story reactions.
- No dialogue leaks horror state into off mode.

### Audio, visuals, and text
- Atmosphere layers start/stop correctly on map transitions and intensity changes.
- Distortion never changes names, numbers, placeholders, or required quest information.
- Visual effects do not obscure interactables, crops, or UI.
- Build-level screenshots are captured for representative off/mild/full scenes, including Harrow Wood, a ritual night, a high-dread day, and each ending when available.

### Human playtest checklist
Automated tests are not a substitute for human review. Ask a tester to play short sessions in each mode and answer:
1. What did you think the Keepers were doing before the reveal?
2. What evidence changed your interpretation?
3. Could you tell what caused a ritual to succeed or fail?
4. Did the villagers' reactions feel earned and in character?
5. Did you know what you could investigate next?
6. Did the atmosphere feel increasingly wrong without making farming unpleasant?
7. Could you complete the cozy loop with horror off without noticing missing content?
8. Did each ending feel like a consequence of your choices?

Record actual observations; do not mark these checks passed from code inspection alone.

## 7. Implementation order

Implement in small, reviewable steps. Update the relevant status rows as each step is genuinely complete.

### Phase A — Narrative coherence (highest priority)
1. H-03 Keeper sabotage reactions and recovery.
2. H-04 clue/friendship gates plus alternate routes for mandatory evidence.
3. H-01 Edmund evidence chain.
4. H-02 four-note motif and sealed-ending payoff.

### Phase B — Human consequences and ritual clarity
5. H-05 dread-responsive lines for the unaware six.
6. H-07 ritual success/failure feedback and duplicate-trigger guard.
7. H-06 Wren/Dorian Keeper relationship variants.

### Phase C — Atmosphere and release polish
8. H-08 wakefulness bands and non-visual cues.
9. H-09 audio/text-distortion accessibility and state cleanup.
10. H-10 ending presentation and human playtest.

If implementation reveals a prerequisite or conflict, document it and adjust the order rather than making speculative changes to canon.

## 8. Definition of done

The task is complete only when:

- [ ] The implementation matches the accepted recommendations without contradicting established lore.
- [ ] All new text is localized and all IDs are stable and registered.
- [ ] Horror-specific code/data remain in the permitted module.
- [ ] Off/mild/full behavior is tested.
- [ ] Changed story state survives save/load.
- [ ] Ritual outcomes and sabotage reactions cannot double-trigger.
- [ ] Mandatory progression has a non-romance, non-max-friendship route.
- [ ] Automated tests pass.
- [ ] The relevant build is compiled and build-only scene/UI issues are checked according to `docs/QA.md`.
- [ ] Human playtest status is honestly recorded; if no human playtest occurred, mark it as outstanding.
- [ ] `docs/STATUS.md` and any affected lore documentation reflect what is actually implemented.
- [ ] The final report lists changed files, test commands/results, known gaps, and any unresolved design decisions.

## 9. Explicit non-goals

Do not:
- Replace Nharoth, Wetherell, Harrow Wood, or the Keepers' names without owner approval.
- Turn the game into a combat-focused horror game or add a monster-chasing loop.
- Make the Keepers secretly evil after all; the protective reveal is foundational.
- Make Hazel a one-dimensional villain or an infallible lore oracle.
- Add a visible wakefulness/dread meter.
- Add gore, graphic animal harm, or compulsory disturbing imagery in mild mode.
- Make the horror layer required to understand or finish ordinary farming progression.
- Add new villagers, new maps, or a new subsystem merely to deliver these fixes.
- Rewrite cozy dialogue wholesale. Prefer horror-only variants and callbacks that reinterpret existing content.
- Mark a feature done because its code exists; verify conditions, build behavior, and play experience.

## 10. Recommended first implementation slice

For the first agent pass, implement **Phase A, item 1 only: H-03 Keeper sabotage reactions and recovery**, unless the current branch reveals the duplicate-trigger bug is still active—in that case, fix and test the trigger guard first.

Deliverables for that pass:
1. Briefly document current ritual/interference state and the exact existing IDs to reuse.
2. Add character-specific post-interference dialogue for the five Keepers.
3. Add or reuse a bounded recovery path so ordinary relationships remain recoverable.
4. Add tests for offering theft, unavailable Keeper, repeated trigger, save/load, successful ritual, and recovery.
5. Validate off/mild/full isolation.
6. Update `docs/STATUS.md` and this document only to reflect verified work.
7. Report what changed and what remains; do not begin the rest of Phase A until this slice is green.

The intended result is not simply “more horror.” It is a more coherent emotional mystery: the player gradually understands what the Keepers have sacrificed, what Edmund knew, why the melody matters, and what their own choices mean for the people they have come to care about.
