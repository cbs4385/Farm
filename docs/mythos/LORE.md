# Lore bible (DRAFT)

Status: draft. Items under "Known" come from the owner. Items under "Proposed" are suggestions for the owner to accept, change or reject; nothing proposed here is implemented, and all ids in `Farm.Mythos/MythosIds.cs` are placeholders. Task X-000 finishes this document before any horror content is built. Content boundaries are in GDD section 7; open questions in GDD section 9.

## Known (from the owner)
- The game is a farming/life sim whose end goal includes a Lovecraftian layer.
- The local village is a **remote, rural village**.
- The village contains a **secret cult** in service to an **Elder God**.
- The Elder God **lives in the neighbouring woods**.

## The layer's job in the game
A slow-burn undertone beneath a genuinely cozy loop. The player's choices (resist, ignore, join) change what they see and how the village treats them. The player controls intensity (HorrorLevel). Everything is delivered through the generic hooks of ADR 0002.

## Proposed structure (to confirm)

### The village
- Name, region, era: *to decide* (GDD question 8). Suggested feel: isolated, close-knit, old customs, a long winter with few visitors.
- Seasonal festivals double as cover for cult observances (festival conditions can switch to variants).
- The Community Hall (cozy main arc) may be where the cult quietly meets (GDD question 2).

### The woods and the god
- The woods border the village and are reached from the Forest map through a gate that stays closed in the base game.
- The Elder God is never fully shown. It is felt through fog, silence, dreams, crops that grow wrong, and the way the villagers behave on certain nights.
- *To decide:* what the god is, what it wants, and what the cult believes they gain (GDD question 3).

### The cult
- A hidden group among the villagers; roughly a third of the 12 NPCs by default (GDD question 4), including at least one the player is likely to befriend early.
- Each NPC has an allegiance: unaware, cult, or resister. Allegiance drives hidden night schedules, conditional dialogue and heart-event variants.
- Offerings, forbidden items and moon-phase observances give the cult observable behaviour.

### The player's three paths (to confirm)
- **Resist:** gather lore, break rituals, protect the village or leave it. Cost: isolation and suspicion.
- **Ignore:** keep farming; the dread slowly rises anyway. Cost: the cozy loop slowly curdles.
- **Join:** earn cult standing and access to the woods. Cost: complicity and what the god asks.
- Endings follow the path (task X-010).

### Systems it uses (all through hooks)
| Story state | Where |
|---|---|
| `dread` (0..100) | variable; drives mood tint, text distortion, dream frequency |
| `lore` | variable; unlocks journal pages and dialogue |
| `cult.standing` | variable; changes how cultists treat the player |
| `mythos.cult_known`, `mythos.cult_revealed`, `mythos.woods_open`, `mythos.initiated` | flags that open doors, change dialogue and show hidden objects |
| fog, blood moon | weather ids from weather modifiers |
| full moon (days 15-18) | calendar atom `moon:full` in conditions |

## Tone and boundaries
Atmospheric folk-horror and cosmic dread, not gore. Original fiction only; no real-world religions; no text or named characters from existing works. See GDD section 7.

## To write (X-000)
Village and character bible (names, ages, roles, secrets, schedules), the god's nature and rules, the cult's beliefs and rituals, a timeline of the first year, per-level (mild/full) content lists, and the three endings.
