# Game Design Document (GDD)

Working title: **Farm** (rename before the Steam page). A cozy farming / life sim in a remote rural village, top-down 2D pixel art, single-player first (co-op is explicitly post-1.0). Beneath the cozy surface sits a **player-tunable cosmic-horror layer that ships with 1.0** (section 8) and is meant to make the game stand out: the village of Wetherell hides a cult, the Keepers of the Covenant, whose rituals keep an Elder God, Nharoth, sleeping in Harrow Wood from waking.
Platforms: **Windows x64 and Linux x64 (Steam)**, Steam Deck as a Linux target. Engine: Unity 6000.6.2f1, URP 2D.

> This is a *genre clone*, not an asset clone. All names, characters, maps, art, music, dialogue, and item designs must be **original**. Do not copy Stardew Valley's characters, town layout, text, sprites, or sounds. Mechanics (farming, seasons, NPC friendships, mining) are genre conventions and are fine to implement. The horror layer likewise uses original creations; Lovecraftian *themes* (cosmic dread, forbidden knowledge, cults) are fine, but do not reproduce text or named characters from existing works.

Related documents: `02-TechnicalDesign.md` (how), `03-ImplementationPlan.md` (backlog), `adr/` (decisions), `mythos/LORE.md` (lore bible, draft), `STATUS.md` (where we are).

## 1. Pillars

1. **Cozy daily loop** – every day offers a few small, satisfying choices; nothing punishes the player harshly.
2. **Long-term growth** – farm, tools, skills, and relationships visibly improve over a 4-year-ish horizon.
3. **A living village** – NPCs have schedules, gifts, heart events, and festivals.
4. **Respect the player's time** – autosave each night, generous controls, gamepad support, rebindable keys.
5. **Something is wrong beneath the warmth** – the horror is a slow burn that depends on the cozy loop feeling real, and the player decides how much of it they want (HorrorLevel, default full). It ships with the game and is a selling point, yet it must never get in the way of the base game: with it off, this is a complete farming sim.

## 2. Core loop

Wake -> check weather/energy -> farm chores (till, plant, water, harvest) -> optional forage / mine / fish / socialize -> sell goods (shipping bin or shop) -> sleep (day summary, autosave) -> next day.

Session loops: **Day** (~14 real minutes, 6:00 to 02:00 game time), **Season** (28 days, one lunar cycle), **Year** (4 seasons).

## 3. Systems (scope per milestone is in the Implementation Plan)

### 3.1 Time and calendar
- 4 seasons x 28 days, 7-day weeks. Clock runs 06:00-26:00 (02:00), 10 game minutes per 7 real seconds. At 02:00 the player passes out: wakes in the farmhouse at 75% energy and loses 5% of their gold (max 500).
- **Planned change (decisions R-X, T-046): late-night fatigue.** The day will run 06:00 to 06:00 (a 24-hour day), and the player may stay up as late as they like. The first time the player tries to go past 22:00 a message explains that staying up will tire them. From 22:00 a **fatigue** rating grows the longer they stay awake, reaching its maximum at 06:00: it reduces the energy recovered when sleeping (up to no recovery at all) and reduces luck (up to half effectiveness). A **fatigue meter next to the energy bar** shows it, and is shown only while fatigue is positive. At 06:00 the player falls asleep automatically. Shops are open 09:00-17:00 and villagers follow a night schedule (going home, eating, visiting friends and neighbours). (An earlier quick-time-event design was dropped.)
- **Moon**: 8 phases per season, new moon on day 1, full moon on days 15-18. The moon is a calendar fact used by events and the horror layer.
- Pause while in menus/dialogue. Festival days fixed on the calendar.
- Weather per day, deterministic from the date: sunny and rain now (never rain in winter or the first two days); storm, snow, and wind in M2. Rain auto-waters crops. Weather is data-driven and extensible (new weather ids can be added by optional layers).
- Night matters: the lighting darkens from 19:30, and the late hours are when NPC secrets and events happen.

### 3.2 Player
- 4-directional movement (8-way input, normalized), tool use in facing direction, interact key, hotbar of 12 slots, backpack starts at 12 slots, upgradeable to 36.
- **Energy** (max 270, upgradeable) and **Health** (max 100). Tools consume energy (hoe 2, watering can 2, axe 3, pickaxe 3, scythe 2); food restores it. At zero energy tools refuse with a message.
- **Tools**: Hoe, Watering Can, Axe, Pickaxe, Scythe, Fishing Rod. Upgrade tiers: Basic, Copper, Iron, Gold (more area/less energy) at the blacksmith over 2 days.
- **Skills** (levels 1-10, XP by use): Farming, Foraging, Mining, Fishing, Combat. Each level unlocks recipes; levels 5 and 10 offer a profession choice.
- Customization: name, farm name, favorite thing, a few body/hair/outfit options (layered sprites).
- Feedback: every refused action explains itself (too tired, needs tilled soil, backpack full...).

### 3.3 Farming
- Farm tile states: untilled -> tilled -> watered (resets each night). Crops advance one growth stage per watered day (or any day when raining).
- Crops: season-bound, multi-stage, some regrow, some multi-harvest. Target: **24 crops** across seasons (6 per season) plus 4 trees (fruit). Six spring crops exist today.
- Quality tiers on harvest: Normal, Silver, Gold, Iridium (driven by Farming level and fertilizer).
- Fertilizer, sprinklers (tiers), scarecrows (crow event), greenhouse (all-season) as unlockables.
- Seeds from the general store. Out-of-season crops die on season change.
- Crops can carry a **grow condition** (data): while it does not hold the plant stays dormant. The horror layer uses this for crops that only grow at certain dread levels, and can change ordinary crops into strange variants as dread rises (see 8).
- Farm animals (post-MVP milestone): coop (chicken) and barn (cow, goat), daily feeding/petting, products.
- Farm clutter: trees, weeds, stones, stumps. Clearable with tools; regenerates slowly.

### 3.4 Foraging, fishing, mining
- **Foraging**: seasonal spawns on maps each morning.
- **Fishing**: timing mini-game, fish species by location/season/time/weather, ~20 fish at 1.0. Catch tables accept conditions, so some fish can depend on night, fog or the moon.
- **Mining**: procedurally generated floors (seeded per day), 40 floors in the first mine, stairs, ladders, ore nodes (copper, iron, gold), gems, simple enemies (5 types + 1 boss at 1.0). Elevator checkpoints every 5 floors. Dying drops some items and returns the player to the village.

### 3.5 Economy and items
- Gold currency. Shipping bin pays at day end (summary screen). Shops: general store, blacksmith, carpenter, fish shop, traveling merchant (random days). Each item lists the shops that sell it (and may add a daily condition), so **the general store never sells horror seeds**: those come from the woods, the cult and rituals.
- Item categories: seed, crop, forage, fish, ore/gem, resource (wood, stone, fiber, coal), artisan good, food, tool, tool-upgrade, furniture, fertilizer, machine, quest item.
- **Crafting** (learned recipes, inventory ingredients): sprinklers, fences, chests, furnaces, kegs, preserves jars.
- **Cooking** (kitchen in house): food buffs (speed, luck, max energy).
- **Artisan machines**: furnace (ore -> bars), keg (fruit -> juice), preserves jar. Timed processing in world time.
- Optional layers can add items and crops through content packs without touching the core item data.

### 3.6 The village and its people
- One village map (**Wetherell**, a non-descript, remote, rural New England community, a little too quiet), the farm, farmhouse interior, 6 buildings (general store, blacksmith, clinic/library, saloon, carpenter, fish shop), forest, beach, mine entrance. The **woods** bordering the village are a gated area (see 8).
- **12 NPCs at 1.0** (8 romanceable-optional, 4 non-romance; keep romance optional and all-ages friendly), each with: daily schedule per season/weather/day-of-week, 3 loved / liked / disliked gifts, dialogue pools by friendship tier, 3-4 heart events (cutscenes).
- Every schedule entry, dialogue line, event and shop stock entry can carry a **condition** (flags, variables, time, weather, moon, friendship...). This is what lets a friendly neighbour keep a hidden night schedule, or say different things once the player knows more. NPC data also carries an optional allegiance (unaware / cult / resister), unused by the base game.
- Friendship 0-10 hearts (250 points/heart). Gifting twice a week, birthdays 8x points. Talking daily gives a small amount.
- Marriage/spouse is a post-1.0 stretch (do not block 1.0). Romanceable NPCs may be cultists; this is handled in heart events and is never exploitative (section 9).

### 3.7 Quests and progression
- Main goal: restore the **Community Hall** (bundle-style collection quests, ~6 rooms) -> unlocks story ending and rewards.
- Daily "help wanted" board quests (fetch/slay/fish). Quest availability accepts conditions.
- Mailbox letters (tutorial, events, gifts) drive early game.
- 1.0 content target: ~40-60 hours first playthrough.
- The horror arc runs alongside this progression through story flags and variables (section 8) and never gates it.

### 3.8 Festivals and events
- 1 festival per season at 1.0 (e.g., spring flower dance analog, summer fishing contest, fall fair, winter feast) - original names/rules. Festivals and events accept conditions, so they can have altered variants.
- Cutscene system with scripted actors, camera, dialogue (data-driven); steps can set flags and variables.

### 3.9 UI / UX
- HUD: clock/date/weather, energy bar (with a fatigue meter beside it that appears only when the player is fatigued), gold, hotbar, tool-use cursor, tooltips, toasts, and slots for extra widgets supplied by optional layers (e.g. a dread meter).
- Menus: inventory, skills, crafting, social, map, collections, journal (extra pages possible), options, save/load, quit.
- Dialogue box with portraits, choices, and item-reward popups.
- Keyboard+mouse and gamepad (Steam Deck) for every screen. Full rebinding (keyboard actions). UI size option. Colorblind-safe indicators.
- **Options include a Horror intensity setting** (off / mild / full) and the store page carries content notes (section 5).
- Localization-ready from day one: every user-facing string goes through `L.Get(key)` and a string table (English at launch). Text can be post-filtered, which the horror layer uses for distortion.

### 3.10 Audio
- Seasonal music, location music, ambience (weather), SFX per tool/footstep surface/UI. Buses: Master, Music, SFX, Ambience, UI (a real mixer with snapshots arrives with the audio pass). Music and ambience layers can be switched by condition or mood layer.

### 3.11 Art direction
- 16x16 tile grid, characters ~16x32, 2D pixel art, **Pixel Perfect Camera** at a 480x270 reference (integer scaling), warm palette per season, URP 2D lights for day/night and indoor lamps.
- The horror layer shifts mood with tint, fog and lighting rather than gore: desaturation, long shadows, wrong details, never at the expense of readability.
- Art is produced by humans or licensed/AI-assisted per project policy; agents use **placeholder art** (see Tech Design section 9) until final art is dropped in using the same sprite names.

## 4. Out of scope for 1.0
Multiplayer co-op, marriage/children, modding API, console ports, mobile, more than 1 village map. (The horror layer is **in** scope for 1.0, see 8 and the plan's Milestone 3b.)

## 5. Steam release requirements
- Steamworks via **Steamworks.NET** (or Facepunch) with achievements (~30), cloud saves, rich presence (optional), Steam Input friendly.
- Windows x64 + Linux x64 depot builds, tested on Ubuntu LTS and SteamOS (Steam Deck verified target).
- Store page assets, trailer, capsule art, age rating questionnaire, EULA/privacy text.
- **Content disclosure (required, the horror layer ships):** fill in Steam's content survey honestly (horror themes, cult imagery), describe the intensity setting on the store page, and keep spoilers out of achievement names and descriptions. Market the unease without misleading cozy-game players: say clearly that the horror can be turned down or off.
- Performance: 60 FPS on integrated GPU / Steam Deck at 1280x800; <2 GB RAM; load times < 5 s between maps.

## 6. Success criteria (Definition of Done for 1.0)
- Complete a full year without game-breaking bugs; all 12 NPCs and the Community Hall are completable.
- Save/load round-trips at every point; saves from patch N load in patch N+1 (versioned migrations; additive fields need none).
- Passes the Release checklist in the Implementation Plan.
- The horror layer is complete: woods, cult NPCs, rituals, the Elder God's wakefulness, dread-gated crops, and all endings work at intensity 0, 1 and 2.
- With the horror intensity at **off**, every system behaves exactly as without the horror layer (enforced by tests, T-044).

## 7. Content guidelines
- Tone: atmospheric and psychological dread, folk-horror and cosmic unease. No graphic gore or torture, no sexual violence, no harm to animals shown on screen.
- Sacrifices are never shown harming animals or people: offerings dissolve on the altar (see open question Q).
- The cult and its god are fictional. No real-world religions, groups or hate content; no depiction of self-harm.
- The player is never forced into horror content: it is gated by the intensity setting, and the game must remain fully playable and completable at "off".
- Romance and relationships stay all-ages.

## 8. Mythos layer (ships with 1.0; hooks built, content not started)

**Premise.** The village of **Wetherell** is a non-descript, remote, rural New England community that hides a cult, the **Keepers of the Covenant**. The cult serves an Elder God, **Nharoth**, that **sleeps** in the neighbouring **Harrow Wood**: a cosmic entity older than the present world. The cult's rituals exist to keep it asleep. The farm sits on the village's edge, and the player slowly learns what the friendly community is protecting, and what protecting it costs. Details live in `mythos/LORE.md` (draft).

**The cult looks like the villain, but is not.** The Keepers' style, mannerisms and symbology are deliberately spooky and menacing, so the player's first impulse is to assume the cult is nefarious. The slow reveal is that they are protecting the world. This is what makes resist / ignore / join a real choice.

**Nharoth's wakefulness.** The layer's influence on the world is tied to how awake the god is. It is a world-level value, distinct from the player's personal **dread**.
- Left alone it rises **25% per season** (full wakefulness in one game year, about 0.9% per day).
- Each **successful ritual lowers it by 30-40%** (never below 0), so several successful rituals can recover from an unsuccessful one. A failed ritual lowers nothing, so that season's 25% rise stands.
- **Rituals** take place **each new moon in Harrow Wood**, attended by the Keepers: **one per season** (the calendar has one lunar cycle per season), four per year. Each ritual needs **specific participants who offer specific sacrifices**: every participant lays their offering on an altar, where it slowly dissolves during the timed ritual. The offerings are **chosen at the start of the season**, and there are **2 in spring, 3 in summer, 4 in autumn and 5 in winter**. The ritual **fails if any offering is not sacrificed in time or a required Keeper is not available**, which is where the player can intervene: help it succeed, or disrupt it.
  - *Where and how long:* an altar deep in Harrow Wood, in a small clearing. The ritual opens with **30 minutes of the leader speaking**, then **20 minutes for each sacrifice** (70, 90, 110 and 130 game minutes by season). Each offering is laid on the altar at the start of its 20 minutes and consumed at the end.
  - *The offerings:* chosen at the start of the season from the **animals, plant products and crafted items on the map**, and **marked** so they can be noticed.
  - *When:* the last night of the new-moon phase (day 4 of each season), starting at 22:00. The first ritual (spring, year 1) is off-stage or merely observed; the player can interfere from the first summer. The player has to be awake to see it, so they accept some fatigue (decision R); the ritual itself happens whether or not they are awake.
  - *How the player learns of it:* overhearing conversations between villagers, clues in direct dialogue, and following the participants as they travel to the clearing.
  - *The player's options:* take or use a marked item before the ritual, or take it from the altar before it is consumed; make a participant unavailable (illness, absence, trust, locked away); or help by guarding the altar and the Keepers.
- **Every 5% of wakefulness is a threshold** (20 steps) at which the world visibly changes: the layer's visual changes are tied to wakefulness.
- **There is no wakefulness meter.** The player learns how awake Nharoth is from visuals, lore and gameplay (the world's changes, what villagers say and do, journal entries, how rituals go).
- At 100% the god awakens fully and the world is consumed in fire and chaos: the worst ending.

**Dread** (the player's personal unease) affects, in mild and optional ways that never block progress: **luck, dialogue options, the seasonal random events, and the weather**. As dread rises each of these tends toward less favourable outcomes for the player. Dread also raises **the rate at which NPC attitudes toward the player decay**. Dread also gates crops (below).

**Principles.**
- *Slow burn:* wrongness accumulates in small details (odd schedules, things in the fog, dreams) before anything is explicit.
- *Mundane vs. wrong:* the horror depends on the warmth of the daily loop being real.
- *Player agency:* investigate and resist, ignore it, or join. Resisting means disrupting the rituals and risking the god waking, unless the player finds another way to keep it asleep. Cult standing, knowledge gathered and dread are tracked separately (flags and variables), and different endings follow.
- *Intensity is the player's choice:* off / mild / full (default **full**). Off yields the plain farming game; mild keeps the unease but removes the most disturbing imagery and text and weakens dread's effects; full is the whole experience.
- *Never blocking:* dread's effects on play are mild and optional, and never stop progress.

**Touchpoints** (all through generic hooks, see `adr/0002-mythos-extension-points.md`):
- NPCs with secret allegiances (about a third are Keepers, including at least one the player is likely to befriend first), hidden night schedules (including the new-moon rituals), conditional dialogue and heart events. Romanceable NPCs may be Keepers; this is handled in heart events and is never exploitative.
- The Community Hall stays a cozy arc that the Keepers quietly use (hidden meetings, altars appearing as flags change).
- Harrow Wood as a gated map where the rituals happen.
- Fog and blood-moon weather; moon-phase events; dread-biased weather.
- Overnight dreams, sleepwalking and blight (the night is when the layer acts on the farm).
- **Crops:** specific crops grow only at certain dread levels; ordinary plants may change (mutate) as dread rises. Horror seeds are **never** sold by the main shop; they come from the woods, the cult and rituals.
- Offerings and forbidden items (content packs).
- No meters: neither the god's wakefulness nor the player's dread is shown as a HUD meter; both are shown through the world and the journal. Mood tinting that advances with each 5% step, distorted text and audio at high dread.
- Ritual sites: an altar in Harrow Wood where offerings dissolve, an offerings list that the player can discover, and Keepers whose availability (illness, absence, trust) the player can influence.
- Luck, dialogue and seasonal random events that lean on dread; friendship decay that speeds up with dread.
- Endings that depend on the player's path and on whether the god stays asleep.

## 9. Design decisions and remaining open questions

### Decided (owner answers)
| # | Question | Decision |
|---|---|---|
| 1 | Release strategy | **Ship the horror layer with 1.0**, as a way to stand out. (Plan: Milestone 3b is required for 1.0; the cozy base game remains complete at intensity "off".) |
| 2 | The Community Hall | A cozy arc that the cult quietly uses. |
| 3 | The god and the cult's aim | A sleeping cosmic entity from before the current world. The cult's rituals keep it asleep. The layer's influence is tied to its wakefulness. If it fully awakens, the world is consumed in fire and chaos. |
| 4 | Which NPCs are cultists | About a third, including at least one NPC the player is likely to befriend first. |
| 5 | Romance and the cult | Allowed, handled in heart events, never exploitative. |
| 6 | Mechanical effects of dread | Mild, optional effects only, never blocking progress. Specific crops may only grow at certain dread levels; ordinary plants may change as dread increases; the main shop does not sell horror seeds. |
| 7 | Default intensity | Full. |
| 8 | Setting | A non-descript New England rural community. |
| A | Wakefulness numbers | Full slumber to fully awake in one game year (25% per season). Each ritual lowers wakefulness by 30-40%, floored at 0, so several successful rituals recover from an unsuccessful one. |
| B | What "resist" does | Disrupt rituals and risk the god waking, unless the player finds another way to keep it asleep. The cult's style, mannerisms and symbology are spooky or menacing, so the player's first impulse is to assume it is nefarious. |
| C | Wakefulness mechanics | The layer's visual changes are tied to wakefulness. Rituals occur each new moon in the forest, attended by the cultists. Threshold changes occur at every 5% of wakefulness. |
| D | Names | Nharoth (elder god), Wetherell (village), Keepers of the Covenant (cult), Harrow Wood (forest). Names are original; do a trademark and Steam-search check before the store page. |
| E | Dread's effects | Dread affects luck, dialogue options, the options for seasonal random events, and the weather, each tending toward less favourable outcomes as dread rises. Dread level also affects the rate at which NPC attitudes toward the player decay. |
| F | Scope protection | *Still open*: see below. |
| G | Ritual cadence | Keep the current calendar: one new moon, so one ritual, per season (four per year). |
| H | Rituals | Each ritual needs specific participants (Keepers) who offer specific sacrifices during the timed ritual. Each places their offering on an altar where it slowly dissolves. The offerings are selected at the start of the season: 2 in spring, 3 in summer, 4 in autumn, 5 in winter. The ritual fails if any offering is not sacrificed or the required Keeper is not available. *Assumed, to confirm:* a failure only lowers nothing (the season's +25% stands) and adds no extra spike. |
| I | What "mild" removes | Confirmed: dread effects at half strength, no text distortion, no explicit ritual imagery or dialogue. |
| J | Wakefulness visibility | No meter. Wakefulness is shown through visuals, lore and gameplay. |
| K | Village name | **Wetherell** (replaces the earlier "Bellweather"). |
| L | Failed rituals | Confirmed: a failed ritual adds no wakefulness beyond the season's rise (no spike). |
| M | Ritual details | The sacrificial items are selected at the start of the season from the **animals, plant products and crafted items on the map**, and are **marked**. The ritual happens at an **altar deep in the wood, in a small clearing**. It lasts **30 minutes of the leader talking, then 20 minutes per sacrifice**. The player can attempt to **take or use marked items before the ritual, or take them from the altar before they are consumed** at the end of their 20 minutes. Each participant can become unavailable (illness, absence, trust, locked away). |
| N | Dread display | **No HUD meter.** The player's dread is shown only through the world and the journal. |
| O | Ritual timing and the first ritual | Accepted. The ritual is on the last night of the new-moon phase (day 4 of each season) and starts at 22:00 (the longest, winter, runs 130 minutes and must start by 23:50). The first ritual (spring, year 1) happens off-stage or is merely observed; the player can interfere from the first summer; wakefulness starts at 0 (full slumber). The player learns about the ritual by **overhearing conversations between villagers**, from **clues in direct dialogue**, and by **following the participants as they travel to the ritual**. |
| P | Whose things are marked | Accepted. Mostly things belonging to the village and other farms, with a modest chance of something of the player's; marked things are visibly marked and listed in the journal so the player can act. |
| Q | Animals and the content guideline | Accepted. Animals are never harmed on screen; an offering dissolves into light on the altar; at mild intensity animals are not chosen (only produce and crafted goods); at off there are no rituals. |
| R | Staying up late | **The quick-time-event mechanic was removed** (the owner agreed with the concerns raised about its real-time frequency and its balance). Instead: the **first time the player tries to go past 22:00 a message is shown**, and a **fatigue meter is displayed beside the energy HUD element**, showing how the penalty grows as the player stays awake longer. **The meter is shown only when the player has a positive fatigue rating.** |
| S | Who gets it | Part of the base game for everyone (not tied to the horror level). |
| T | When the player falls asleep | The existing fall-asleep (pass-out) mechanic. The ritual happens at its set time whether or not the player is awake: **only missing participants or sacrifices stop or fail a ritual**. |
| U | How long the night lasts | The day runs until **06:00** (dawn); the clock runs 06:00 to 06:00 (a 24-hour day). The player falls asleep automatically when the clock reaches 06:00. |
| V | Quick-time event design | *Withdrawn* (no quick-time events). |
| W | Accessibility toggle for the challenge | *Withdrawn*: with no quick-time events there is nothing to switch off. Fatigue applies to everyone. |
| X | Cost of staying up | Fatigue **grows with time awake past 22:00**. At its maximum (06:00) the player has effectively **no energy recovery (100% penalty)** and **luck at half effectiveness (50% penalty)** (the earlier values, now reached by time awake instead of by quick-time events). *Assumed: linear growth from 22:00 to 06:00.* |
| Y | Real-time cadence of checks | *Moot* (no quick-time events). |
| Z | Balance when the challenge is off | *Moot*: there is no toggle, and fatigue applies to everyone, so staying up is never free. |
| AB-a | Shops and villagers after hours | **Shops close after business hours (09:00-17:00).** Villagers follow a **night schedule**: going home, eating, visiting friends and neighbours. |

### Still open
F. **Scope protection.** Because the horror layer ships with 1.0, decide what to cut from the base game if the schedule slips (see the plan's scope notes). Needed before Milestone 3.
AA. **How fatigue applies.** (a) *Growth:* assumed linear from 22:00 (0) to 06:00 (maximum), so midnight is 25%, 02:00 is 50%. Confirm, or choose a different curve (for example slower at first). (b) *Energy:* assumed that fatigue scales how much of the missing energy sleep restores (100% = none, so waking energy equals energy at bedtime). (c) *Luck:* at neutral luck, "half effectiveness" of zero is zero, so proposed that luck effectiveness (1 minus the penalty) scales good luck and bonus chances, leaving bad luck unchanged. (d) *Duration:* does the luck penalty last through the next day and clear on the next normal sleep, or end when the player wakes?
AB. **The 06:00 collapse.** At 06:00 the player falls asleep automatically. Does that still lose some gold and recover only 75% of energy (before fatigue), as the 02:00 pass-out does today? Does it wake the player in bed rather than where they collapsed?
AC. **Shop hours in detail.** Business hours are 09:00-17:00. Do all shops keep them (the saloon, a place people visit in the evening, is the obvious exception)? Does the traveling merchant? Do closed shops lock their doors, or can the player still enter and find nobody? Is there a shop day off? The temporary seed stall on the farm is a placeholder and not a shop.
AD. **The first warning.** Proposed text: "It is getting late. If you stay up, you will grow tired: you will recover less energy when you sleep, and your luck will suffer." Shown once per save (a story flag), when the clock first reaches 22:00. Confirm, or reword.
