# Lore bible

Status: final for 1.0 (X-000 done; the as-built layer is in ADR 0004). **Known** items are decisions from the owner (GDD section 9). **Proposed** items are suggestions to accept, change or reject; nothing proposed is implemented, and ids in `Farm.Mythos/MythosIds.cs` are placeholders. Task X-000 finishes this document before any horror content is built. Content boundaries are in GDD section 7; the only remaining open question (F, scope protection) is in GDD section 9.

## Known (owner decisions)
- The game is a farming/life sim and **ships with** a Lovecraftian layer, to help it stand out. Default intensity is **full**; the player can turn it down or off.
- **Names:** the elder god is **Nharoth**; the village is **Wetherell**; the cult is the **Keepers of the Covenant**; the forest is **Harrow Wood**. (Check originality and trademarks before the store page. Avoid places and names from existing Lovecraft stories.)
- **Setting:** a non-descript, remote, rural New England community.
- Nharoth is a **sleeping cosmic entity from before the current world**, living in Harrow Wood.
- The Keepers **perform rituals to keep it asleep**. If it **fully awakens, the world is consumed in fire and chaos**.
- **Wakefulness:** rises 25% per season untouched (full in one game year); each ritual lowers it by 30-40% (floor 0); several successful rituals can undo an unsuccessful one. Rituals happen **each new moon in Harrow Wood** (one per season on the current calendar), attended by the Keepers. The layer's **visual changes follow wakefulness**, with a threshold change at **every 5%**.
- **Rituals are specific.** Each needs particular Keepers as participants, each laying a particular offering on an altar where it slowly dissolves during the timed ritual. The offerings are chosen at the start of the season: 2 in spring, 3 in summer, 4 in autumn, 5 in winter. A ritual fails if any offering is not sacrificed or a required Keeper is not available. A failure lowers nothing and adds no spike (decided).
- **No meters.** Neither Nharoth's wakefulness nor the player's dread is a HUD meter; both are shown through the world and the journal.
- **Ritual timeline.** An altar deep in Harrow Wood, in a small clearing. 30 minutes of the leader speaking, then 20 minutes per sacrifice (70/90/110/130 game minutes by season). Offerings are chosen at season start from the animals, plant products and crafted items on the map, and are marked; the player can take or use a marked item before the ritual or take it from the altar before it is consumed at the end of its 20 minutes.
- **Mild intensity** halves dread's effects and removes text distortion and explicit ritual imagery and dialogue.
- **The cult looks nefarious.** Its style, mannerisms and symbology are spooky or menacing so the player assumes it is evil; the reveal is that it protects the world.
- **Resisting** means disrupting rituals and risking the god waking, unless the player finds another way to keep it asleep.
- About **a third of the NPCs are Keepers**, including at least one the player is likely to befriend first. Romanceable NPCs may be Keepers (heart events, never exploitative).
- The **Community Hall** is a cozy arc that the Keepers quietly use.
- **Dread** (the player's unease) affects luck, dialogue options, the seasonal random events and the weather, each leaning to worse outcomes as it rises, and speeds up the decay of NPC attitudes. Its effects are mild, optional and never block progress. Specific crops grow only at certain dread levels; ordinary crops may change as dread rises. **The main shop does not sell horror seeds.**

## What this implies (design consequences)
- **Two different meters.** *Wakefulness* is Nharoth's state: a world value on a fixed schedule (rising by calendar, lowered by rituals) that decides the world's fate. *Dread* is the player's personal unease: it drives mood, luck, dialogue, events, weather, friendship decay and crop gating. They are related (a restless god raises dread) but separate.
- **The Keepers protect the world.** A first impression of menace plus a late reveal is a classic structure; the early game must sell the menace convincingly while planting clues (they tend the woods, they look after the village, their rituals have costs) that make the reveal land.
- **Resisting is dangerous.** Disrupting a ritual removes a 30-40% reduction at the very time wakefulness is rising 25% per season. A good resistance path needs an alternative way to keep Nharoth asleep (proposed below), or the player must take over the Keepers' work.
- **A built-in failure state with a clock.** Untended, the world ends in a game year. Rituals are the main brake, so players who ignore the cult still benefit from its work; players who disrupt it must replace it. The pacing (one new moon, so one ritual, per season: decided) makes each ritual a major event, and each season's required offerings (2, 3, 4, 5) make later rituals harder to pull off and easier to sabotage.
- **Wakefulness steps are content slots.** Twenty thresholds (5% each) mean twenty escalating changes to the look, sound and behaviour of the world, from barely noticeable to apocalyptic. Plan them as a ladder (X-001, X-008).
- **Dread is a general-purpose modifier.** It needs hooks in luck, dialogue conditions, seasonal event weights, weather rolls and friendship decay (ADR 0002), applied at mild strength at intensity 1 and not at all at 0.
- **Seeds and rare goods** come through Harrow Wood, the Keepers and rituals, never the general store (enforced by data: items opt in to shops).

## The layer's job in the game
A slow-burn undertone beneath a genuinely cozy loop. The player's choices (resist, ignore, join) change what they see and how the village treats them. Everything is delivered through the generic hooks of ADR 0002 and respects the intensity level.

## Proposed structure (to confirm)

### Wetherell
- A specific region of New England and an era are still to decide (suggested: present day or recent past, hill country, stone walls and old farmhouses, long winters, few visitors, a strong sense of custom).
- Seasonal festivals double as cover for Keeper observances (festival conditions switch to variants).
- The Community Hall (the cozy main arc) is where the Keepers quietly meet: hidden rooms and altars appear as flags change.

### Harrow Wood and Nharoth
- Harrow Wood borders the village and is reached from the Forest map through a gate that stays closed in the base game.
- Nharoth is never fully shown. It is felt through fog, silence, dreams, crops that grow wrong, and how villagers behave on certain nights. The 20 wakefulness steps escalate this from "something is off" to open catastrophe.
- Wakefulness is raised by the calendar (25% per season) and lowered by successful rituals (30-40% each). A failed or disrupted ritual simply does not lower it (assumed, open question L).

### The ritual, as a system (to design in X-000 and X-004)
- At the start of each season the offerings for the coming ritual are chosen (2 to 5 depending on season) from the animals, plant products and crafted items on the map, and marked; each is tied to a specific Keeper who must attend. The marks are visible, so an observant player can notice them.
- On the new moon night, at the altar deep in Harrow Wood, the leader speaks for 30 minutes, then each participant in turn lays their offering on the altar, where it dissolves over 20 minutes. The ritual succeeds only if every offering is fully sacrificed and every required Keeper is present.
- Ways the player can affect it (proposals): **help** (supply or recover an offering, keep a Keeper healthy and unobstructed, defend the altar), **disrupt** (take an offering from the altar, make a Keeper unavailable through illness, absence, or revealing a secret, damage the altar), or **observe** (learn the offering list from lore and overheard dialogue). Making a Keeper unavailable ties into NPC schedules and conditions (a Keeper with a "sick" or "away" flag is simply not at the altar).
- Outcomes: success lowers wakefulness 30-40%; failure lowers nothing, so the season's 25% rise stands and a year without a success wakes Nharoth.
- Offerings are drawn from what exists in the world: animals, plant products and crafted items. Core systems expose them through `IWorldObjectSource`, and a marked stack carries `ItemStack.Mark`. The player's own things can be chosen, with modest odds, and animal offerings never show harm.
- **When:** the last night of the new-moon phase (day 4 of each season), starting at 22:00. The first ritual (spring, year 1) is off-stage or merely observed; the player can interfere from the first summer. Wakefulness starts at 0.
- **Discovery:** the player learns of rituals by overhearing conversations between villagers, from clues in direct dialogue, and by following the participants as they travel to the clearing.
- **Marked things:** mostly belong to the village and other farms, with a modest chance of the player's own; the marks are visible and listed in the journal.
- **Animals:** never harmed on screen; an offering dissolves into light on the altar. At mild intensity animals are not chosen; at off there are no rituals.
- **Staying awake:** to witness a ritual (22:00 onward) the player must stay up, which costs fatigue (less energy recovery and worse luck: decisions R-X). The ritual happens at its time whether or not the player is awake; only missing participants or sacrifices fail it. After business hours the villagers follow night schedules, which is when the Keepers' movements can be noticed.

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
| `mythos.wakefulness` (permille, 0..1000) | world variable; Nharoth's state; steps of 50 (5%); model in `WakefulnessModel`; not shown as a meter |
| season offerings plan | module data: the 2-5 (Keeper, offering) pairs chosen at season start (`RitualModel`); the altar's dissolving offerings |
| Keeper availability | per-NPC flags (for example `npc.<id>.sick`, `npc.<id>.away`) used by schedule conditions |
| `lore` | variable; unlocks journal pages and dialogue |
| `cult.standing` | variable; changes how the Keepers treat the player |
| `mythos.cult_known`, `mythos.cult_revealed`, `mythos.woods_open`, `mythos.initiated` | flags that open doors, change dialogue and show hidden objects |
| fog, blood moon | weather ids from weather modifiers |
| new moon (day 1-4 of each season) | calendar atom `moon:new` in conditions; ritual nights |

## Tone and boundaries
Atmospheric folk-horror and cosmic dread, not gore. Original fiction only; no real-world religions; no text or named characters from existing works. See GDD section 7.

## As built (X-000, final)

**The Keepers (five of the twelve villagers).** Tilda (the general-store keeper the player meets first), Marcus (the carpenter), Dr. Odalys Penn (the clinic; the leader), Dorian (the forager, who lodges with Marcus), Wren (the saloon). Hazel is the one villager who has worked out what is happening and works against them. The other six are unaware. Keepers are menacing in manner (silence, watching, leaving their work early on ritual nights) and protective in purpose. Their beliefs: Nharoth is not evil, it is *vast*; its waking would unmake the world; the Covenant has kept it asleep for as long as Wetherell has stood, and each keeper carries the cost privately.

**Rituals.** Day 4 of every season, 22:00, at the altar in the Harrow Wood clearing. Offerings 2/3/4/5 by season; the Keepers are drawn in rotation. Each offering is laid on the altar and dissolves into light over 20 minutes after 30 minutes of the leader's speech. Offerings are village goods, with a one-in-four chance of being something real that the player owns or made. A sick or away Keeper, or a taken or missing offering, fails the ritual. A failure lowers nothing; rituals simply keep out-running the god (the year adds 100%, four rituals take 120-160%).

**Wakefulness ladder (20 steps of 5%).** Tint and fog grow with each step; the journal records what the player has understood (lore gates which lines): birds quieter; longer morning fog; dogs avoid the forest path; milk sours; lamps lit early; the new moon looks large; odd crop shapes; a low hum at night; hushed voices after dark; red-edged full moon (a blood moon from step 10 at full intensity); iron-tasting wells; Keepers abroad at all hours; wrong-leaning shadows; fog that stays at noon; animals facing the wood; the hum nearly a voice; no one sleeps through the night; the woods closer; warm ground; the sky waiting. At 100% the world ends in fire.

**Dread at mild and full.** The same model, all effects halved at mild. Mild also removes the strangest dream text, explicit ritual imagery (the offerings are plain lights), the blood moon, and the more disturbing stone inscriptions (`.mild` variants). Level 1 keeps the clues, the Keepers and the endings. Level 0 removes the layer entirely.

**First-year timeline.** Spring 1: nothing visible; the overheard whisper in the saloon; the first ritual runs off-stage (the player cannot interfere yet). Summer 1: the Woods open (the brambles in the forest give way); the player can interfere; Hazel's second clue. Fall 1: mutations begin once dread passes 20. Winter 1: five offerings; without the rituals wakefulness would reach 100% by the end of the year. Year 2+: the Keepers' invitation (after standing and lore), the relics, the sealing.

**Endings.** *Sealed*: the three relics (seal, bell, thread) found in the Woods and the true words (lore 8) at the altar, quest `mythos_seal`; the god sleeps for good and dread falls. *Joined*: initiated, standing 20, the offering of oneself at the altar. *Ignored*: year 3 with neither, a quiet, slightly dimmer life. *Awakened*: wakefulness reaches 1000 (only possible if rituals are broken); fire, then the main menu.

## Why the player has the farm (playtest request, 2026-10-05)
The player's distant great-uncle, **Edmund Fenn**, farmed the property for forty years and was **chosen by the Keepers as the offering of the fall before the game begins**. His death is what sent the inheritance chain to the player. In the base game (and at horror off) the story is only that a distant uncle died and left the farm (`OpeningStory`, the welcome letter from Tilda, `uncle_letter`); nothing hints at more, and Tilda's warmth about him is simply kindness. At mild and full, Mythos letters reveal the rest: `mythos_uncle_note` (lore 1: he was "chosen", do not accept their hospitality) and `mythos_uncle_truth` (lore 4 and the Woods open: he was the offering, the farm is the bargain, its owner is the one they keep in reserve, and the seal, bell and thread are the way out). Tilda, a Keeper, wrote the welcome letter, which gives it a second reading on a replay. Neither letter is in core assemblies or data.
