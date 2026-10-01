# Lore bible (DRAFT)

Status: draft. **Known** items are decisions from the owner (GDD section 9). **Proposed** items are suggestions to accept, change or reject; nothing proposed is implemented, and all ids in `Farm.Mythos/MythosIds.cs` are placeholders. Task X-000 finishes this document before any horror content is built. Content boundaries are in GDD section 7; remaining open questions (A-F) are in GDD section 9.

## Known (owner decisions)
- The game is a farming/life sim and **ships with** a Lovecraftian layer, to help it stand out. Default intensity is **full**; the player can turn it down or off.
- The setting is a **non-descript, remote, rural New England community**. (Use original names; avoid places and names from existing Lovecraft stories.)
- The village contains a **secret cult** in service to an **Elder God** that lives in the neighbouring woods.
- The Elder God is a **sleeping cosmic entity from before the current world**.
- The cult **enacts rituals to keep the god asleep**. (The owner's wording of the unsuccessful case is unclear; presumed: an unsuccessful ritual stirs or wakes it. See open question A.)
- The horror layer's influence on the world is **tied to the god's wakefulness**. If the god **fully awakens, the world is consumed in fire and chaos**.
- About **a third of the NPCs are cultists**, including at least one the player is likely to befriend first. Romanceable NPCs may be cultists; handled in heart events, never exploitative.
- The **Community Hall** is a cozy arc that the cult quietly uses.
- **Dread's effects on play** are mild, optional and never block progress. Specific crops only grow at certain dread levels; ordinary plants may change as dread rises. **The main shop does not sell horror seeds.**

## What this implies (design consequences)
- **Two different meters.** *Wakefulness* is the god's state: a world value that rituals move and that decides the world's fate. *Dread* is the player's personal unease: it drives mood, text and dream effects and gates certain crops. They are related (a restless god raises dread) but separate.
- **The cult is protecting the world.** That makes the choice real: the neighbours are doing a terrible thing for a reason that may be right.
- **Resisting is dangerous.** Disrupting rituals can wake the god. A good resistance path needs an alternative way to keep it asleep (proposed below).
- **A built-in failure state.** Full wakefulness ends the game in fire and chaos, so it needs foreshadowing, a visible-but-subtle meter, thresholds and a recovery mechanic, and must be reachable only through the player's own choices over a long time.
- **Seeds and rare goods** come through the woods, the cult and rituals, never the general store (enforced by data: items opt in to shops).

## The layer's job in the game
A slow-burn undertone beneath a genuinely cozy loop. The player's choices (resist, ignore, join) change what they see and how the village treats them. Everything is delivered through the generic hooks of ADR 0002 and respects the intensity level.

## Proposed structure (to confirm)

### The village
- Original name and a specific region of New England, era to decide (open question D). Suggested feel: isolated, close-knit, old customs, long winters, few visitors, stone walls and old farmhouses.
- Seasonal festivals double as cover for cult observances (festival conditions switch to variants).
- The Community Hall (the cozy main arc) is where the cult quietly meets: hidden rooms and altars appear as flags change.

### The woods and the god
- The woods border the village and are reached from the Forest map through a gate that stays closed in the base game.
- The god is never fully shown. It is felt through fog, silence, dreams, crops that grow wrong, and how villagers behave on certain nights.
- Wakefulness (0..100) is raised by failed or skipped rituals, disturbed sites and some player actions, and lowered by successful rituals. Moon phase makes rituals more important near the full moon. Thresholds change the world: more fog, stranger weather, more dreams, then crises, and at 100 the fiery ending.

### The cult
- About a third of the 12 NPCs, including one the player is likely to meet and befriend early. Each NPC has an allegiance: unaware, cult, or resister. Allegiance drives hidden night schedules, conditional dialogue and heart-event variants.
- Offerings, forbidden items and moon-phase observances give the cult observable behaviour. Rituals are real systems (they succeed or fail and move wakefulness), not just scenery.

### The player's three paths (to confirm, open question B)
- **Resist:** gather lore and either find another way to keep the god asleep (a true fix, at a price) or break the rituals and risk waking it. Cost: isolation, suspicion, danger.
- **Ignore:** keep farming; the cult keeps the god asleep and dread still creeps. Cost: the cozy loop slowly curdles; the player is complicit by inaction.
- **Join:** earn cult standing and access to the woods; take part in rituals. Cost: complicity and what the god asks.
- Endings follow the path and the god's state (task X-010), including the fiery ending when it wakes.

### Crops and dread
- Horror crops carry a grow condition tied to dread (for example, they only grow once dread is high enough) and are obtained through the woods, the cult or rituals, never the general store.
- Ordinary crops can mutate into strange variants as dread rises (day-cycle hook swaps the crop id). Mutations stay mild and optional, never destroying progress.

### Story state (all through hooks)
| Story state | Where |
|---|---|
| `dread` (0..100) | player variable; mood tint, text distortion, dream frequency, grow conditions |
| `wakefulness` (0..100) | world variable; the god's state (placeholder id in `MythosIds`) |
| `lore` | variable; unlocks journal pages and dialogue |
| `cult.standing` | variable; changes how cultists treat the player |
| `mythos.cult_known`, `mythos.cult_revealed`, `mythos.woods_open`, `mythos.initiated` | flags that open doors, change dialogue and show hidden objects |
| fog, blood moon | weather ids from weather modifiers |
| full moon (days 15-18) | calendar atom `moon:full` in conditions |

## Tone and boundaries
Atmospheric folk-horror and cosmic dread, not gore. Original fiction only; no real-world religions; no text or named characters from existing works. See GDD section 7.

## To write (X-000)
Village and character bible (names, ages, roles, secrets, schedules and which third are cultists), the god's nature and rules, the cult's beliefs and rituals (and what makes one succeed or fail), the wakefulness model and thresholds, a timeline of the first year, per-level (mild/full) content lists, and the endings.
