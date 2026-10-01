# Game Design Document (GDD)

Working title: **Farm** (rename before the Steam page). A cozy farming / life sim in a remote rural village, top-down 2D pixel art, single-player first (co-op is explicitly post-1.0). Beneath the cozy surface sits a **player-tunable cosmic-horror layer that ships with 1.0** (section 8) and is meant to make the game stand out: the village hides a cult whose rituals keep an Elder God, sleeping in the neighbouring woods, from waking.
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
- One village map (a non-descript, remote, rural New England community, a little too quiet), the farm, farmhouse interior, 6 buildings (general store, blacksmith, clinic/library, saloon, carpenter, fish shop), forest, beach, mine entrance. The **woods** bordering the village are a gated area (see 8).
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
- HUD: clock/date/weather, energy bar, gold, hotbar, tool-use cursor, tooltips, toasts, and slots for extra widgets supplied by optional layers (e.g. a dread meter).
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
- The cult and its god are fictional. No real-world religions, groups or hate content; no depiction of self-harm.
- The player is never forced into horror content: it is gated by the intensity setting, and the game must remain fully playable and completable at "off".
- Romance and relationships stay all-ages.

## 8. Mythos layer (ships with 1.0; hooks built, content not started)

**Premise.** The village is a non-descript, remote, rural New England community that hides a cult. The cult serves an Elder God that **sleeps** in the neighbouring woods: a cosmic entity older than the present world. The cult's rituals exist to keep it asleep. The farm sits on the village's edge, and the player slowly learns what the friendly community is protecting, and what protecting it costs. Details live in `mythos/LORE.md` (draft).

**The Elder God's wakefulness.** The horror layer's influence on the world is tied to how awake the god is. Rituals move it (a successful ritual keeps it deeply asleep; an unsuccessful one stirs it; see open question A). Should it ever fully awaken, the world is consumed in fire and chaos: the worst ending. The wakefulness is a world-level value, distinct from the player's personal **dread**.

**Principles.**
- *Slow burn:* wrongness accumulates in small details (odd schedules, things in the fog, dreams) before anything is explicit.
- *Mundane vs. wrong:* the horror depends on the warmth of the daily loop being real.
- *The cult is not simply evil:* its members are neighbours doing something terrible for a reason that may be right. This is what makes "resist / ignore / join" a real choice.
- *Player agency:* investigate and resist, ignore it, or join. Cult standing, knowledge gathered and dread are tracked separately (flags and variables), and different endings follow.
- *Intensity is the player's choice:* off / mild / full (default **full**). Off yields the plain farming game; mild keeps the unease but removes the most disturbing imagery and text; full is the whole experience.
- *Never blocking:* dread's effects on play are mild and optional, and never stop progress.

**Touchpoints** (all through generic hooks, see `adr/0002-mythos-extension-points.md`):
- NPCs with secret allegiances (about a third are cultists, including at least one the player is likely to befriend first), hidden night schedules, conditional dialogue and heart events. Romanceable NPCs may be cultists; this is handled in heart events and is never exploitative.
- The Community Hall stays a cozy arc that the cult quietly uses (hidden meetings, altars appearing as flags change).
- The woods as a gated map.
- Fog and blood-moon weather; moon-phase events.
- Overnight dreams, sleepwalking and blight (the night is when the layer acts on the farm).
- **Crops:** specific crops grow only at certain dread levels; ordinary plants may change (mutate) as dread rises. Horror seeds are **never** sold by the main shop; they come from the woods, the cult and rituals.
- Offerings and forbidden items (content packs).
- A dread meter, a wakefulness indicator for the god (how visible is a design choice), mood tinting, distorted text and audio at high dread.
- Endings that depend on the player's path and on whether the god stays asleep.

## 9. Design decisions and remaining open questions

### Decided (owner answers)
| # | Question | Decision |
|---|---|---|
| 1 | Release strategy | **Ship the horror layer with 1.0**, as a way to stand out. (Plan: Milestone 3b is required for 1.0; the cozy base game remains complete at intensity "off".) |
| 2 | The Community Hall | A cozy arc that the cult quietly uses. |
| 3 | The god and the cult's aim | A sleeping cosmic entity from before the current world. The cult's rituals keep it asleep. The layer's influence is tied to its wakefulness. If it fully awakens, the world is consumed in fire and chaos. (Wording of the ritual outcomes needs confirming: open question A.) |
| 4 | Which NPCs are cultists | About a third, including at least one NPC the player is likely to befriend first. |
| 5 | Romance and the cult | Allowed, handled in heart events, never exploitative. |
| 6 | Mechanical effects of dread | Mild, optional effects only, never blocking progress. Specific crops may only grow at certain dread levels; ordinary plants may change as dread increases; the main shop does not sell horror seeds. |
| 7 | Default intensity | Full. |
| 8 | Setting | A non-descript New England rural community (original names; avoid places and names from existing Lovecraft stories). |

### Still open
A. **Ritual outcomes.** The answer reads "when successful, keep the god asleep, and when unsuccessful, keep the god asleep". Presumably the second half means an unsuccessful ritual *stirs or wakes* it. Please confirm.
B. **What "resist" does.** If the cult's rituals keep the world safe, resisting them is dangerous. Proposed: resisting means disrupting rituals and risking the god waking, unless the player finds another way to keep it asleep (see LORE.md). Confirm or change.
C. **Wakefulness mechanics.** How it rises and falls (rituals, player actions, moon phase, time), what its thresholds do, and whether the player can see it.
D. **Names.** The village, the woods, the god and the cult need original names.
E. **Dread's concrete effects** beyond crops (visual, audio, text; any energy or luck effects), and what "mild" removes.
F. **Scope protection.** Because the horror layer now ships with 1.0, decide what to cut from the base game if the schedule slips (see the plan's scope notes).
