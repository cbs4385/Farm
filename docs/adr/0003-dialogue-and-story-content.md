# ADR 0003: Dialogue and story content (T-034 and the M2 story systems)

Status: accepted (2026-10-02).

## Context
T-034 asked for a decision between Yarn Spinner (MIT) and a minimal in-house format. The same data language also has to carry quests (T-039), events and cutscenes (T-041), mail, the help-wanted board and random events. All of them need the same things: **conditions** on every line/choice/entry, **effects** that set flags and variables, text through `L.Get` so text filters apply, and a validator that can check every reference (T-040). The horror layer later adds content through the same format.

## Decision
1. **In-house JSON, not Yarn Spinner.** Reasons: (a) Yarn would add a package (manifest edits are only safe with the Editor open, and it brings its own runtime, localization and variable model that would duplicate `Conditions`, flags/vars and `L`); (b) our needs (branches, conditions, effects) are small; (c) a validator over our own schema is simple, over Yarn's it is not; (d) module content must register at runtime without a build step. Yarn can still be adopted later behind `DialogueRunner` if writers ask for it.
2. **One JSON schema per content file** under `Resources/Story/*.json`, with optional top-level arrays: `dialogues`, `sets`, `quests`, `letters`, `events`, `randomEvents`, `boardJobs`. `StoryContent.AddJson(json, source)` merges a file; modules call it at runtime. A duplicate id is rejected and logged, never overwritten (save compatibility).
3. **Dialogue graph.** A dialogue is a set of nodes: `{id, condition, speaker, text (string key), args, effects[], choices[{text, condition, effects[], next}], next}`. A node whose condition fails is skipped to its `next`. Choices whose condition fails are hidden (this is how dread removes favourable options). `DialogueRunner` is pure C# (world query, text resolver, effect sink in), so it is unit tested without a scene.
4. **Dialogue sets** pick which dialogue to play: ordered entries `{condition, priority, dialogue}`; the highest-priority entries whose condition holds form a pool, and one is chosen deterministically from the day (so an NPC says something new each day but the same thing when talked to twice).
5. **Effects** are strings `verb:arg1,arg2` (`flag:`, `unflag:`, `setvar:`, `addvar:`, `gold:`, `give:`, `take:`, `friend:`, `quest.start:`, `quest.done:`, `mail:`, `event:`, `toast:`, `xp:`, `learn:`). `Effects.Register` adds verbs, so modules can add their own; the validator knows every registered verb.
6. **Condition atoms added for story data** (registered by `StoryConditions`): `hearts:<npc>>=N`, `has:<item>>=N`, `quest:<id>=active|done|new`, `weekday:<mon..sun>` (built in), `knows:<recipe>`. Atoms read the game through the optional `IGameQuery` interface that `StateWorldQuery` implements, so `IWorldQuery` and its test fakes are unchanged.
7. **The dialogue box is a modal UI screen using the existing UI input map** (Submit advances, Navigate and Submit choose, Cancel skips to the end of a node's text). The separate `Dialogue` input map mentioned in the plan is not needed: modal screens already block gameplay input and pause the clock, and rebinding stays simple. Same decision as ADR 0001 for the input asset.
8. **NPC positions are a pure function of the clock.** `NpcSchedule.Where(...)` returns where an NPC is (at a stop, or walking a leg of a route between maps); the scene actors only render it. There is no per-NPC movement state to save or desynchronize, "off-screen simulation" is the same function, and NPCs never teleport in view because the on-screen actor follows the same function along an A* path.

## Consequences
- Writers edit JSON and `en.json`; no assembly changes for new content.
- Everything the horror layer needs (conditions on lines, hidden schedules, new effects/atoms, event weight hooks) goes through these registries, in line with ADR 0002.
- Dialogue and quest ids are part of save compatibility once shipped: never rename them.
