# ADR 0005: Narrative depth and stream appeal (Milestone 4b)

Status: accepted 2026-10-02. The owner delegated the open questions ("use your best judgement") and set the goal: **an indie game worth streaming on Twitch and YouTube, so that streams drive purchases.** Resolves decisions D1-D8 of `NPC_DIALOGUE_PLAN.md`. Revisits the GDD 9F scope-protection decision ("no cuts").

## Context
`NPC_DIALOGUE_ANALYSIS.md` found the cozy layer's dialogue well voiced but thin: about 8 distinct lines a visit, 2 heart events per villager, almost no reactivity, one trivial choice. A streamed game lives or dies on **variety, surprise and shareable moments**: viewers must see something new every few minutes, chat must be able to take part, and a clip must make sense without context. The same cast, in the same data format, can deliver that, but only with more content, more interaction types and some systems built for it.

## Decisions
1. **D1, timing: a launch tier and an update tier.** At 1.0, six villagers (**Tilda, Wren, Bram, Juno, Hazel, Piper**) are *Full*; the other six (**Dorian, Elara, Felix, Ione, Marcus, Odalys**) are *Enhanced* (about half the targets). The Enhanced six are brought to Full in free **"Neighbour Spotlight" updates**, roughly one every 6-8 weeks, which also give streamers a reason to return. Why these six: Tilda is met first; Wren, Juno, Bram and Piper have the strongest comic and performance hooks; Hazel is the wholesome and mystery anchor. Keepers (Tilda, Wren) and the resister (Hazel) are included so the horror layer is exercised early.
2. **D2, villager count: all 12 stay, at two depths** (above). No villager is cut.
3. **D3, romance: optional courtship without marriage.** At 8 hearts, the eight romanceable villagers can enter an optional courtship path ending in a "partners" stage with its own scene. All-ages, never required. Marriage stays out of scope for 1.0. Villager-to-villager pairings are written as well, because shipping and rivalries drive fandom and clips.
4. **D4, languages: English at launch.** The pipeline is made localisation-ready (T-139). The first extra languages are chosen from wishlist and viewer data after launch.
5. **D5, voice: no recorded voice.** Each villager gets original voice blips and one signature sound (a catchphrase-like audio identity that becomes a recognisable stream moment). Recorded key lines stay a post-launch option.
6. **D6, writing process: agent drafts, human edits.** Every villager is reviewed by a named human before it is locked, and `STATUS.md` records who read what. Creator and outside playtests are part of the process (D8).
7. **D7, authoring format: build `.fscript`** (plain screenplay-style text compiled to the story JSON and `en.json`) before the volume writing starts.
8. **D8, reviewers: the owner for tone and voice, plus a panel of 5-10 outside players and content creators** for the slice gate, the launch gate and the first-hour check.
9. **Stream appeal is a design requirement, not a nice-to-have.** It adds a workstream (WS-H: social actions, a gift lab, seeded village storylines, rare lines, stream mode, shareable moments, a designed first hour) and measurable targets (section 2.3 of the plan). Content is tagged *funny*, *wholesome*, *surprise*, *mystery* and quota-checked.
10. **Scope protection (GDD 9F) revisited.** "No cuts" still holds for the base game. The expansion is bounded: the launch tier is fixed at the six Full villagers plus the Enhanced six, and anything beyond that goes to the update schedule. If the schedule slips, the cut line is: Enhanced-tier depth, then cross-cast scenes, never the engine and slice work that everything depends on.

## Consequences
- Plan: `docs/NPC_DIALOGUE_PLAN.md`, tasks T-080 to T-150, tracked in `STATUS.md` as Milestone 4b. The release candidate gate (T-069) additionally requires the **Launch narrative gate** (plan section 5).
- Reserved dialogue priority bands (cozy talk 0-3, reactions 3-4, quest offers 4-6, one-shot story scenes 6-7, mythos 8-10, first meeting and tutorial 100) become a validator rule.
- New story state lives in flags, vars and module data, never new `GameState` fields; existing ids are never renamed (ADR 0003).
- Streamer-facing options (stream mode, name filter, choice shortcuts, optional chat-vote timer) are accessibility-grade settings, off by default, and add no required Twitch or YouTube integration.
- The GDD is updated to point here; the horror layer keeps its own content and rules (ADR 0002, 0004).
