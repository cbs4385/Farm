# NPC Interconnected Gameplay & Narrative Implementation Specification

**Status:** Agent-ready implementation specification  
**Target repository:** `cbs4385/Farm`  
**Review baseline:** 2026-10-16  
**Purpose:** Turn the existing NPC narrative review into a prioritized, testable implementation plan that makes villagers feel like people with independent lives whose goals, relationships, dialogue, quests, and the optional cosmic-horror layer reinforce one another.

---

## 0. Instructions to the implementation agent

You are implementing narrative/content improvements in an existing Unity cozy farming game. Work incrementally and preserve existing game architecture.

### Required workflow

1. Read these documents before changing content:
   - `docs/NPC_DESIGN_REVIEW_2026-10-16.md`
   - `docs/NPC_DIALOGUE_ANALYSIS.md`
   - `docs/NPC_DIALOGUE_PLAN.md`
   - `docs/narrative/RELATIONSHIPS.md`
   - `docs/narrative/BIBLE.md`, `docs/narrative/BIBLE_ENHANCED.md`, and `docs/narrative/BIBLE_WAVE2.md`
   - `docs/narrative/GOALS.md`, `docs/narrative/STYLE.md`, `docs/narrative/FSCRIPT.md`
   - `docs/adr/0002-mythos-extension-points.md`, `docs/adr/0003-dialogue-and-story-content.md`, and `docs/adr/0004-mythos-layer-decisions.md`
2. Inspect the actual current implementation and data before editing. Do not assume that an older review's line numbers or statuses are still current.
3. Make a small, coherent change set. Do not attempt a wholesale rewrite of all dialogue.
4. Preserve existing dialogue IDs, quest IDs, flags, save-compatible keys, localization keys, and authored canon unless a migration is explicitly justified. Prefer adding conditions or new IDs over renaming existing IDs.
5. Keep cozy content in the normal narrative layer. Put cosmic-horror-only lines, reactions, clues, and endings in the established Mythos overlay/data path. With horror disabled, no Mythos content or behavior may leak into play.
6. Do not add an engine/system abstraction for a content-only problem. Reuse the existing dialogue graph, condition atoms, effects, quests, mail, event templates, and validators where possible.
7. Do not claim a scene or interaction exists merely because dialogue promises it. Every invitation must lead to a real choice, an existing activity, or a clearly worded non-promise.
8. After each phase, run relevant EditMode and PlayMode tests, narrative lint/validation, and a content diff review. Report files changed, IDs added, tests run, and known gaps.
9. Do not add lore or relationships that contradict the bibles or relationship matrix. If a canon change is necessary, update the source-of-truth document and associated data/tests together.
10. Do not change art, animation, unrelated gameplay, or economy systems as part of this task.

### Definition of success

The player should be able to answer, through play rather than exposition alone:
- What does this villager want?
- Why does it matter to them emotionally?
- Which other villager is involved, knowingly or unknowingly?
- What can the player do that changes the situation?
- What visibly changes after the player helps?
- What does this reveal about the village, and (when enabled) its hidden history?

A character arc is not complete when a line says the goal is resolved. It is complete when the player sees a consequence, receives a callback, or participates in a scene that makes the change tangible.

---

## 1. Narrative design principles

### 1.1 Character motivation model

For every major NPC, maintain these linked fields in the narrative source-of-truth:

- **Surface want:** the concrete goal the character can say aloud.
- **Emotional need:** the human need underneath the goal.
- **Fear / resistance:** what makes pursuing the goal difficult.
- **Behavioral tell:** an action that reveals character without explanation.
- **Relationship dependency:** at least one other villager whose goal, secret, or behavior intersects with this arc.
- **Player contribution:** a specific action, choice, gift, quest, or act of witness that the player can perform.
- **Payoff:** a changed behavior, scene, object, mail, event, or follow-up line.
- **Optional Mythos recontextualization:** where relevant, how the horror layer changes the meaning without invalidating the cozy reading.

Avoid giving a character several unrelated props or symbols for the same desire. Choose one or two recurring motifs and let other characters notice them.

### 1.2 Interconnection rules

- Every major personal arc must touch at least one other NPC in a consequential way, not only through name-drops.
- Every cross-NPC link should be one of:
  1. **Mutual support** (one helps another),
  2. **Asymmetric care** (one quietly helps without being acknowledged),
  3. **Affectionate rivalry** (friction with a warm floor),
  4. **Shared mystery** (characters hold different pieces of evidence),
  5. **Player-mediated connection** (the player carries information or creates a meeting).
- At least some links must be discoverable only by talking to multiple people or observing behavior.
- Never make every villager equally close to every other villager. Preserve distinct clusters and asymmetrical knowledge.
- Keep the established rule: nobody is cruel or contemptuous toward the player; rivalry remains mild and affectionate.

### 1.3 Dialogue rules

- Every line must sound like its speaker even without a name label.
- Dialogue should do at least one job: reveal character, advance a goal, change a relationship, provide actionable information, react to world state, or recontextualize an earlier detail.
- Avoid generic gratitude tails, repeated metaphors, and the same joke structure across the entire cast.
- Do not reveal a secret plainly before its scene or condition.
- Time-bound statements must have matching time/season/location conditions, or be rewritten to be timeless.
- Invitations phrased as questions must offer a real accept/decline choice or be rewritten as observations.
- Keep all-ages warmth. Choices may differ in honesty, intimacy, or style, but should not punish players for choosing a kind response.
- Do not use cosmic-horror terminology in ordinary dialogue when horror is disabled. Foreshadowing in the cozy layer must remain interpretable as ordinary village life.

### 1.4 Systems rules

- Use existing deterministic dialogue selection; do not introduce reroll-to-find-new-lines behavior.
- Verify priorities do not make quests, seasonal lines, rain lines, or friendship tiers unreachable.
- Condition and effect syntax must match `docs/narrative/FSCRIPT.md`.
- Any new dialogue key must be localization-safe and pass existing narrative lint.
- Add tests for newly introduced conditions, effects, and progression gates.

---

## 2. Cast-level goals and required payoffs

Use these as design constraints, not permission to overwrite established canon.

| NPC | Surface want / motif | Emotional need | Key relationship web | Required gameplay payoff |
|---|---|---|---|---|
| Tilda Ashby | A village that stays full; kindness slate and shop lamp | Proof people will return and care can be shared | Juno (aunt-like bond), Felix (old friend), Wren (gossip rival), whole village | Her slate/sign should reflect a real milestone, especially the farm becoming a supplier; the slate can carry relationship callbacks |
| Bram Hollis | Good work; Juno's first original piece; word from old teacher | To express pride without feeling exposed; to know his work mattered | Juno (mentor/apprentice), Marcus (quiet friendship), Felix (quiet repairs), Wren (four-word bet) | Juno's achievement must be witnessed and acknowledged; the teacher's reply should arrive through the existing mailbox if that chain is completed |
| Ione Fairweather | More readers; sea in winter; books set aside for absent visitors | To be visited and to share what she loves | Hazel (mentor/favorite person), Dorian and Elara (courteous distance), village readers | Make the set-aside shelf a reusable link between characters; avoid multiplying objects that all represent the same sea-longing |
| Marcus Dell | Finish the Community Hall roof properly | To leave something sound behind and stop living with “nearly” | Bram (close quiet friend), Dorian (housemate), Odalys (pie feud), Tilda and Wren (roof bets) | The roof must be a real hall objective with a visible state change and a payoff scene; restoring unrelated rooms must not falsely imply the roof is fixed |
| Dr. Odalys Penn | A village that rests; carry the Covenant/duty | To stop being the person who must carry everything alone | Elara (warm rivalry), Wren, Tilda and the Keepers when Mythos is enabled | Her personal rest arc needs a concrete response; Mythos clues require earned trust and sabotage must change relationships |
| Wren Calloway | A full room and a story to tell; packed bag / sea | To be less lonely and to decide whether she will leave | Piper (bandmate), Hazel (listener/writer), Tilda (rival), Bram (word-count bet) | Stage or explicitly pay off the promised full-room gathering; her one-listener performance must not duplicate Hazel's emotional climax |
| Felix Hartwell | A fishing partner; sturgeon; share the dawn | To have company without needing to ask for it | Tilda (oldest friendship), Bram (quiet kindness), Elara (travel/boat tie), Piper (awkward history) | Turn at least one “come with me tomorrow” or boat promise into a real activity or scene; avoid asking an expert to supply their own routine stock without a learning purpose |
| Juno Hale | Make her first original piece; earn Bram's spoken pride | To become her own craftsperson without abandoning Bram | Bram (mentor), Tilda (aunt), Piper (best friend), Marcus (forge/hall links) | Ensure the promised material and finished item agree; her first independent work should be a visible milestone and Bram should react |
| Hazel Brandt | Be heard once, properly; protect the library's quiet | To risk being known on her own terms | Ione (mentor), Wren (stories/notebook), Dorian (shared silence), Piper (secret lyric-writing) | Keep her poem/listener climax distinct from Wren's gathering; connect her research to the Mythos clues only when horror is enabled |
| Piper Vance | A song the village sings without her | To matter as a writer, not only as a performer | Wren (bandmate), Juno (friend/rival), Hazel (secret lyric collaborator), Elara (first listener) | The village singing the song is the payoff; preserve the player's chosen word/line and reference it later where feasible |
| Dorian Lake | A friend who can share silence; mushroom notebook | To be seen without being crowded | Marcus (housemate), Hazel (quiet friend), Elara/Ione (courteous distance), forest | Make the notebook/foraging goal a shared discovery or small activity; his generosity should be legible through behavior, not exposition |
| Elara Finch | See the sea from a boat; knitted secret gifts | To be more than useful to everyone else | Odalys (clinic rivalry), Felix (boat), Piper (first listener), Juno (gloves), Ione (sea/books) | The map/list/sea longing should converge on one clear arc and culminate in a boat-related payoff, not four competing props |

---

## 3. Highest-priority coherence fixes

Implement these before adding large quantities of new dialogue. Verify each against the current branch first.

### P0 — Promise/payoff and canon consistency

1. **Wren's full-room promise**
   - The full-house request must culminate in a real gathering scene or be explicitly rewritten so it no longer promises a gathering.
   - Preserve the one-listener performance if desired, but give it a distinct purpose and title; do not let it substitute for the full-room payoff.
   - The wine or hosting preparation already requested should be used in the gathering if still compatible with current quest data.

2. **Community Hall roof**
   - Represent the roof as an explicit Hall objective/room or as the final dependency of hall completion.
   - Connect Marcus's plank quest/material contribution to that objective.
   - Do not set `flag:hall.restored` or equivalent in a way that allows Marcus to claim “fixed” before the roof state is actually complete.
   - Give the roof a visible state transition and a short reaction from Marcus.

3. **Secret timing**
   - Gate Wren's early “almost left” confession behind the appropriate heart event/flag or move it to the correct higher-trust tier.
   - Remove implausible “years ago” language from Juno/Ione lines when the timeline does not support it.
   - Check all other confession lines against event ordering and plausible in-game elapsed time.

4. **Juno/Bram reward consistency**
   - The Juno quest that requests gold must deliver what it promises, or explicitly explain why gold is used to make a copper first piece.
   - Bram's “Your Tool” memory must deliver a tool or be retitled/reframed to match the actual reward.
   - Ensure quest descriptions, event dialogue, inventory rewards, and journal text tell the same story.

5. **Elara's sea motif**
   - Consolidate the list, map, notebook, and atlas into one coherent desire arc. Prefer the map (noticed by others) and the list (seen by the player) unless current implementation has a stronger dependency.
   - Preserve Ione's book-gifting characterization without giving Elara yet another unrelated “sea” object.

6. **Repeated dialogue templates**
   - Keep the letter-writing joke with Bram and Juno as deliberate contrasts; rewrite equivalent confidant jokes for other villagers to use their own habits and fears.
   - Assign signature motifs consistently: the forge is the village's warmest building; Tilda owns the “lamp left on” kindness motif; resolve overlapping door/wind imagery.
   - Replace generic “Thank you, [player]” tails when they make all twelve sound alike.

7. **Expert asks player to do their own work**
   - Reframe Felix's bait, Dorian's forage, and Odalys's elderflower requests as teaching, shared discovery, or a meaningful reason the player should gather the item.
   - A quest request should expose character or deepen a relationship, not just outsource a shopkeeper's normal stock.

8. **Canon cleanup**
   - Reconcile Bram's age with his teacher/letter history.
   - Resolve the Felix stool joke so it reads as affectionate and consistent.
   - Update the narrative bible where existing lines contradict it (including Piper/Elara history).
   - Correct weekday/location invitations that send the player to an NPC who is not there.

### P0 — Trust and Mythos behavior

1. **Earned Keeper clues**
   - Mythos clue tiers must require both the existing lore progression and a minimum friendship tier.
   - Suggested default: clue 1 at 2 hearts, clue 2 at 4 hearts, clue 3 at 6 hearts, adjusted only where the existing scene requires a stronger gate.
   - Apply equivalent trust gating to Hazel's clues and Odalys's invitation.
   - Verify priority ordering does not let a clue interrupt a more appropriate introduction or make ordinary content unreachable.

2. **Consequences for player interference**
   - After the player steals an offering or makes a Keeper ill, the affected Keepers must react in later dialogue.
   - Reactions should express hurt and withdrawn trust, not sudden villainy or cruelty.
   - Give each affected Keeper at least two distinct, state-gated reactions in their own voice.
   - Ensure the reaction is not silently overwritten by ordinary friendship lines the next morning.
   - Test the existing duplicate-sabotage prompt bug separately; do not conflate the fix with writing.

3. **Dread should be legible**
   - The unaware six (Bram, Juno, Piper, Felix, Elara, Ione) should have brief, caring reactions when dread crosses an appropriate threshold.
   - These lines must show concern through each character's normal habits, not make them suddenly aware of the cult or explain the mechanic in meta terms.
   - Gate the lines in Mythos data so they never appear when horror is off.

4. **Harvest Edmund's clues**
   - The cellar door, Juno's brass key, Edmund's “affairs in order,” Ione's almanac margins, Hazel's pattern research, Dorian counting trees, and the two uncle letters must form a connected investigation.
   - Reuse existing map/object, inventory, mailbox, and clue systems; do not add a new investigation subsystem.
   - Every seeded mystery should either receive a payoff or be deliberately identified as a long-term unresolved mystery in the lore bible.

5. **Four-note motif**
   - Make the recurring four notes an intentional Mythos motif connecting Edmund, Piper, night-time humming, and the ending.
   - The cozy interpretation must still work as a musician's habit.
   - The final/sealed ending should pay off the motif; do not introduce a fifth note elsewhere without a clear reason.

6. **Romanceable Keepers**
   - Wren and Dorian's romance must remain emotionally sincere and non-exploitative when horror is enabled.
   - Add minimal Mythos variants to existing relationship lines/scenes where the relationship changes the stakes.
   - Do not make romance a prerequisite for solving the central mystery.

---

## 4. Highest-value cross-NPC scenes

Prefer a small number of scenes that close several arcs over many disconnected lines. Reuse current event templates.

### Scene A — The Hall Dance (top priority)

**Prerequisites:** Hall restored; roof objective complete; relevant personal arcs available.  
**Participants:** Marcus, Tilda, Wren, Piper, Bram, Juno; optional cameos from the wider cast.  
**Purpose:** Deliver the village's shared-space payoff and close multiple promises in one event.

**Beat outline**
1. Arrival: Marcus checks the roof once more and admits it is finished.
2. Wren welcomes a genuinely full room; use the hosting request's prepared item if present.
3. Piper hears the village sing the song the player helped shape. Preserve the chosen word/line if the existing system supports it.
4. Bram hums or plays a small part; Juno's independent craft is present as a visible object/detail.
5. Tilda's slate records a kindness that was repaid without being demanded.
6. End on a small, specific interaction between two villagers that demonstrates a changed relationship.
7. Follow-up: at least two participating NPCs reference the dance in later dialogue; avoid repeating the whole scene in recap.

**Acceptance criteria**
- Cannot trigger before the Hall roof is actually complete.
- Cannot trigger repeatedly unless the current event system explicitly supports repeatable festivals.
- Each participant's dialogue reflects their individual motive.
- The scene pays off, rather than merely mentions, the full-house, roof, song, and forge arcs.

### Scene B — Past the Point / The Boat

**Participants:** Felix, Elara; Wren may appear if her “go” choice is relevant.  
**Purpose:** Make the boat a real shared action and tie Elara's sea desire to Felix's wish for a dawn companion.

**Beats:** Felix prepares the boat with his characteristic understated care; Elara chooses a destination from her existing list/map; a small mishap is handled through competence and humor; the player sees the sea or an equivalent destination payoff; if Wren chose “go,” she may be encountered at the point without forcing her decision to match Elara's.

**Acceptance criteria**
- The player is not promised a boat trip unless the event can actually trigger.
- Elara's map/list are the canonical props; do not add more sea-longing objects.
- Felix's expertise is used rather than asking him to gather routine bait for himself.

### Scene C — Reading Night

**Participants:** Hazel, Ione, Dorian; optional Wren as the storyteller.  
**Purpose:** Pay off Hazel's wish to be heard and Ione's wish for readers without making Hazel's intimate heart-10 moment redundant.

**Beats:** Ione offers a small reading gathering; Hazel decides what to share and how much; Dorian contributes a pressed leaf or notebook drawing without being forced into a speech; Wren's stories can provide a bridge between oral and written memory.

**Acceptance criteria**
- Hazel retains agency over whether the poem is shared.
- Dorian can participate through a quiet action.
- The scene differs in scale and emotional purpose from Wren's full-house gathering.

### Scene D — Next Door (Juno's forge)

**Participants:** Juno, Bram; Marcus may contribute a tool or fitting if consistent with the existing relationship data.  
**Purpose:** Resolve Juno's first independent work and Bram's difficulty expressing pride.

**Beats:** Juno unveils her own piece; a practical flaw is noticed; Bram offers a tool or correction without taking over; Juno decides whether to accept the help; Bram says one direct, unmistakable sentence of pride.

**Acceptance criteria**
- The finished object/reward matches the quest description.
- The player witnesses a change in their mentor/apprentice relationship.
- Bram remains terse; do not turn him into an eloquent speech-maker.

### Scene E — The Letter Arrives

**Participants:** Bram, his former teacher (off-screen is acceptable), player via mailbox.  
**Purpose:** Turn a long-running spoken want into an actual payoff using existing mail infrastructure.

**Acceptance criteria**
- A completed quest/state produces mail with a specific response, not merely another line saying Bram hopes for a reply.
- The letter's tone is restrained and emotionally legible.
- Bram's follow-up references the actual contents without overexplaining.

---

## 5. Daily dialogue and world reactivity

Do not begin by authoring hundreds of lines. Fix unreachable content and the biggest broken promises first, then expand coverage in measured batches.

### Required content coverage

For every talk set:
- A player at 0–2 hearts should have multiple reachable ordinary lines.
- Friend and close tiers must add variety without completely shadowing ordinary chat.
- Rain lines must be reachable at every relevant heart tier.
- Seasonal and hour-specific lines must only appear at their appropriate time.
- Quest offer/reminder/turn-in lines must outrank casual dialogue when the player can act on them.
- A line with a named event, season, time, or location must have the matching condition or be rewritten.

### Reactivity backlog

Implement only when supported by existing condition atoms or a small, well-tested addition:
1. Upcoming festival (within a short countdown).
2. Recently completed quest or personal milestone.
3. Player harvest/farm progress.
4. Shop/service progress or upgrade completion.
5. Moon/night ambience, especially where it supports the existing four-note and forest motifs.

Each reactive line should be specific and temporary. Avoid a generic system that simply inserts the player's name into every line.

### Gossip

- Preserve the established “player carries knowledge between villagers” structure.
- Gossip should reflect a written relationship in `docs/narrative/RELATIONSHIPS.md` and the matching relation data.
- Do not let an NPC discuss another villager the player has not met if the established gossip rules prohibit it.
- Gossip should sometimes give the player a useful lead or reveal quiet kindness, not only provide a joke.
- Keep the relationship matrix and gossip tests synchronized.

---

## 6. Production order and scope control

### Phase 1 — Consistency and progression bugs

Implement:
- Wren full-room promise / event naming consistency.
- Community Hall roof objective and Marcus's state gate.
- Juno/Bram item and reward consistency.
- Secret timing and age/timeline contradictions.
- Elara motif consolidation.
- Repeated dialogue-template cleanup.
- Expert-quest reframing.
- Keeper clue friendship gates.
- Quest priority and reachability regression checks.

**Exit criteria:** All affected narrative lint/tests pass; no promise is still contradicted by its reward/state; no new content IDs are unreachable.

### Phase 2 — Mythos emotional consequences

Implement:
- Post-interference reactions.
- Dread reactions for the unaware six.
- Edmund/cellar/key/almanac/trees clue chain.
- Four-note motif through the ending.
- Minimal Wren/Dorian relationship variants and Marcus roof clue.

**Exit criteria:** Mythos on: reactions/clues are correctly gated and pay off seeded evidence. Mythos off: no Mythos line, flag side effect, clue object, or behavior leaks into the cozy game.

### Phase 3 — Shared-scene payoffs

Implement in this order:
1. Hall Dance.
2. Next Door.
3. Past the Point.
4. Reading Night.
5. Bram's teacher letter.

Reuse event templates and current flags. If a scene cannot be implemented without a major new engine system, propose a smaller existing-system-compatible version instead of silently expanding scope.

**Exit criteria:** Scene prerequisites, participants, rewards, one-time behavior, and callbacks are tested. At least one later line acknowledges each major completed scene.

### Phase 4 — Variety and reactive talk

Expand dialogue in small batches, beginning with Tilda, Bram, Ione, and Wren, then the remaining cast. Add seasonal, time, rain, friendship-tier, and world-state variety without reducing quest discoverability.

**Exit criteria:** Reachability/variety tests pass; no seasonal/time contradictions; new lines are distinct in voice and do not duplicate motifs.

### Phase 5 — Polish (not a blocker for narrative coherence)

Only after content works:
- Portrait expressions where art support exists.
- Configurable text speed/accessibility improvements.
- Optional speaker voice blips.
- Conversation-end friendship feedback that does not obscure dialogue.

---

## 7. Required tests and review checklist

Add or extend automated tests for:

### Dialogue selection
- Every new line is reachable under at least one valid state.
- Quest offers, reminders, and turn-ins cannot be hidden by ordinary friendship pools.
- Rain, seasonal, and hour lines are selected only when their conditions are true.
- Higher-heart pools do not erase all lower-tier variety.
- A declined offer remains available according to the intended reminder policy without permanently starving ordinary dialogue.

### Progression and payoff
- Wren's full-house event requires the correct quest/state and fires once.
- Marcus cannot claim the roof is fixed until the roof objective is complete.
- Juno's requested input and resulting item match the authored promise.
- Bram's teacher letter is delivered only after the relevant chain is complete.
- Every new event has valid prerequisites, effects, participants, and follow-up flags.

### Mythos
- Friendship gates and lore gates both apply to clues.
- Interference reactions only trigger for the affected NPCs and correct state.
- Dread reactions do not leak into the horror-off mode.
- Cellar/key/almanac/tree clues are connected and cannot soft-lock the ending.
- Four-note clues and resolution are consistent across relevant scenes.

### Narrative QA
- Every time-bound statement has a matching condition or is rewritten.
- Every invitation has a real accept/decline response or is not phrased as an invitation.
- No secret is disclosed before its intended scene.
- Every new gossip topic maps to a written relationship.
- All new IDs pass narrative lint and localization checks.
- Run relevant EditMode and PlayMode tests; report results and any baseline failures separately.

### Human-readable content review
For every changed NPC arc, answer “yes” to all:
- Is the want concrete?
- Is the emotional need distinct from the want?
- Does the NPC act in a way only they would?
- Does another NPC materially affect the arc?
- Does the player do something meaningful?
- Is there a visible or remembered payoff?
- Is the dialogue consistent with timeline, schedule, relationship state, and voice?
- Does the cozy interpretation still work with horror disabled?

---

## 8. Agent completion report template

At completion, report:

1. **Summary:** what changed and why.
2. **Files changed:** exact paths.
3. **Dialogue/event IDs:** added or modified; explicitly state that existing IDs were preserved unless otherwise approved.
4. **Behavior changes:** gates, flags, quest states, items, mail, and event prerequisites.
5. **Tests:** commands/suites run and pass/fail counts.
6. **Manual QA:** which events/conditions were inspected in game, or explicitly state if not played.
7. **Known gaps:** items deferred and why.
8. **Canon updates:** any changes to the bibles, relationship matrix, lore, or implementation status.

Do not claim a playtest was performed if only static analysis and automated tests were run.

---

## 9. Source-of-truth hierarchy

When documents conflict, use this order and resolve conflicts explicitly rather than silently choosing:
1. Current runtime behavior and save-compatible IDs/state (must not be broken).
2. Accepted architecture decisions in `docs/adr/`.
3. Narrative bibles and Mythos lore.
4. Relationship matrix and goals.
5. Existing dialogue scripts and quest/event data.
6. This implementation specification, which is a synthesis and prioritization document.

If a higher-priority source is itself inconsistent, stop and document the conflict before making a canon-changing edit.

**End of specification.**
