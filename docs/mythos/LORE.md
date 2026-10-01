# Lore bible (DRAFT)

Status: draft. **Known** items are decisions from the owner (GDD section 9). **Proposed** items are suggestions to accept, change or reject; nothing proposed is implemented, and ids in `Farm.Mythos/MythosIds.cs` are placeholders. Task X-000 finishes this document before any horror content is built. Content boundaries are in GDD section 7; remaining open questions (F-K) are in GDD section 9.

## Known (owner decisions)
- The game is a farming/life sim and **ships with** a Lovecraftian layer, to help it stand out. Default intensity is **full**; the player can turn it down or off.
- **Names:** the elder god is **Nharoth**; the village is **Bellweather** (spelling to confirm); the cult is the **Keepers of the Covenant**; the forest is **Harrow Wood**. (Check originality and trademarks before the store page. Avoid places and names from existing Lovecraft stories.)
- **Setting:** a non-descript, remote, rural New England community.
- Nharoth is a **sleeping cosmic entity from before the current world**, living in Harrow Wood.
- The Keepers **perform rituals to keep it asleep**. If it **fully awakens, the world is consumed in fire and chaos**.
- **Wakefulness:** rises 25% per season untouched (full in one game year); each ritual lowers it by 30-40% (floor 0); several successful rituals can undo an unsuccessful one. Rituals happen **each new moon in Harrow Wood**, attended by the Keepers. The layer's **visual changes follow wakefulness**, with a threshold change at **every 5%**.
- **The cult looks nefarious.** Its style, mannerisms and symbology are spooky or menacing so the player assumes it is evil; the reveal is that it protects the world.
- **Resisting** means disrupting rituals and risking the god waking, unless the player finds another way to keep it asleep.
- About **a third of the NPCs are Keepers**, including at least one the player is likely to befriend first. Romanceable NPCs may be Keepers (heart events, never exploitative).
- The **Community Hall** is a cozy arc that the Keepers quietly use.
- **Dread** (the player's unease) affects luck, dialogue options, the seasonal random events and the weather, each leaning to worse outcomes as it rises, and speeds up the decay of NPC attitudes. Its effects are mild, optional and never block progress. Specific crops grow only at certain dread levels; ordinary crops may change as dread rises. **The main shop does not sell horror seeds.**

## What this implies (design consequences)
- **Two different meters.** *Wakefulness* is Nharoth's state: a world value on a fixed schedule (rising by calendar, lowered by rituals) that decides the world's fate. *Dread* is the player's personal unease: it drives mood, luck, dialogue, events, weather, friendship decay and crop gating. They are related (a restless god raises dread) but separate.
- **The Keepers protect the world.** A first impression of menace plus a late reveal is a classic structure; the early game must sell the menace convincingly while planting clues (they tend the woods, they look after the village, their rituals have costs) that make the reveal land.
- **Resisting is dangerous.** Disrupting a ritual removes a 30-40% reduction at the very time wakefulness is rising 25% per season. A good resistance path needs an alternative way to keep Nharoth asleep (proposed below).
- **A built-in failure state with a clock.** Untended, the world ends in a game year. Rituals are the main brake, so players who ignore the cult still benefit from its work; players who disrupt it must replace it. The pacing (one new moon, so one ritual, per season on the current calendar) makes each ritual a major event; see open question G.
- **Wakefulness steps are content slots.** Twenty thresholds (5% each) mean twenty escalating changes to the look, sound and behaviour of the world, from barely noticeable to apocalyptic. Plan them as a ladder (X-001, X-008).
- **Dread is a general-purpose modifier.** It needs hooks in luck, dialogue conditions, seasonal event weights, weather rolls and friendship decay (ADR 0002), applied at mild strength at intensity 1 and not at all at 0.
- **Seeds and rare goods** come through Harrow Wood, the Keepers and rituals, never the general store (enforced by data: items opt in to shops).

## The layer's job in the game
A slow-burn undertone beneath a genuinely cozy loop. The player's choices (resist, ignore, join) change what they see and how the village treats them. Everything is delivered through the generic hooks of ADR 0002 and respects the intensity level.

## Proposed structure (to confirm)

### Bellweather
- A specific region of New England and an era are still to decide (suggested: present day or recent past, hill country, stone walls and old farmhouses, long winters, few visitors, a strong sense of custom).
- Seasonal festivals double as cover for Keeper observances (festival conditions switch to variants).
- The Community Hall (the cozy main arc) is where the Keepers quietly meet: hidden rooms and altars appear as flags change.

### Harrow Wood and Nharoth
- Harrow Wood borders the village and is reached from the Forest map through a gate that stays closed in the base game.
- Nharoth is never fully shown. It is felt through fog, silence, dreams, crops that grow wrong, and how villagers behave on certain nights. The 20 wakefulness steps escalate this from "something is off" to open catastrophe.
- Wakefulness is raised by the calendar and by failed or disrupted rituals (amount to decide, open question H), lowered by successful rituals.

### The Keepers of the Covenant
- About a third of the 12 NPCs, including one the player meets and befriends early. Each NPC has an allegiance: unaware, Keeper, or resister. Allegiance drives hidden night schedules (including the new-moon ritual), conditional dialogue and heart-event variants.
- Their look and manner is menacing (robes, symbols, silence, watching), so the player's first assumption is that they are the villains.
- Offerings, forbidden items and moon-phase observances give the cult observable behaviour. Rituals are real systems (they succeed or fail and move wakefulness), not just scenery.

### The player's three paths
- **Resist:** gather lore and either find another way to keep Nharoth asleep (a true fix, at a price) or disrupt the rituals and risk waking it. Cost: isolation, suspicion, danger.
- **Ignore:** keep farming; the Keepers keep Nharoth asleep and dread still creeps. Cost: the cozy loop slowly curdles; the player is complicit by inaction.
- **Join:** earn standing and take part in rituals, with access to Harrow Wood. Cost: complicity and what the god asks.
- Endings follow the path and the god's state (task X-010), including the fiery ending when Nharoth wakes.

### Crops and dread
- Horror crops carry a grow condition tied to dread (they grow only once dread is in the right range) and are obtained through Harrow Wood, the Keepers or rituals, never the general store.
- Ordinary crops can mutate into strange variants as dread rises (a day-cycle hook swaps the crop id). Mutations stay mild and optional, never destroying progress.

### Story state (all through hooks)
| Story state | Where |
|---|---|
| `dread` (0..100) | player variable; drives luck, dialogue options, seasonal event weights, weather bias, friendship decay, mood, grow conditions |
| `mythos.wakefulness` (permille, 0..1000) | world variable; Nharoth's state; steps of 50 (5%); model in `WakefulnessModel` |
| `lore` | variable; unlocks journal pages and dialogue |
| `cult.standing` | variable; changes how the Keepers treat the player |
| `mythos.cult_known`, `mythos.cult_revealed`, `mythos.woods_open`, `mythos.initiated` | flags that open doors, change dialogue and show hidden objects |
| fog, blood moon | weather ids from weather modifiers |
| new moon (day 1-4 of each season) | calendar atom `moon:new` in conditions; ritual nights |

## Tone and boundaries
Atmospheric folk-horror and cosmic dread, not gore. Original fiction only; no real-world religions; no text or named characters from existing works. See GDD section 7.

## To write (X-000)
Village and character bible (names, ages, roles, secrets, schedules and which third are Keepers), Nharoth's nature and rules, the Keepers' beliefs and rituals (what makes one succeed or fail), the 20 wakefulness steps with the world change at each, how dread's effects scale at mild and full, a timeline of the first year, per-level (mild/full) content lists, and the endings.
