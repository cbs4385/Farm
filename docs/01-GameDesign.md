# Game Design Document (GDD)

Working title: **Farm** (rename before Steam page). Genre: cozy farming / life sim, top-down 2D pixel art, single-player first (co-op is explicitly post-1.0).
Platforms: **Windows x64 and Linux x64 (Steam)**, Steam Deck as a Linux target. Engine: Unity 6000.6.2f1, URP 2D.

> This is a *genre clone*, not an asset clone. All names, characters, maps, art, music, dialogue, and item designs must be **original**. Do not copy Stardew Valley's characters, town layout, text, sprites, or sounds. Mechanics (farming, seasons, NPC friendships, mining) are genre conventions and are fine to implement.

## 1. Pillars

1. **Cozy daily loop** – every day offers a few small, satisfying choices; nothing punishes the player harshly.
2. **Long-term growth** – farm, tools, skills, and relationships visibly improve over a 4-year-ish horizon.
3. **A living town** – NPCs have schedules, gifts, heart events, and festivals.
4. **Respect the player's time** – autosave each night, generous controls, gamepad support, rebindable keys.

## 2. Core loop

Wake -> check weather/energy -> farm chores (till, plant, water, harvest) -> optional forage / mine / fish / socialize -> sell goods (shipping bin or shop) -> sleep (day summary, autosave) -> next day.

Session loops: **Day** (~14 real minutes, 6:00 to 02:00 game time), **Season** (28 days), **Year** (4 seasons).

## 3. Systems (scope per milestone is in the Implementation Plan)

### 3.1 Time and calendar
- 4 seasons x 28 days, 7-day weeks. Clock runs 06:00-26:00 (02:00); passing out at 02:00 teleports player home with a penalty (lose some energy, small gold loss).
- Pause while in menus/dialogue. Festival days fixed on the calendar.
- Weather per day: sunny, rain, storm, snow (winter), wind. Rain auto-waters crops.

### 3.2 Player
- 4-directional movement (8-way input, normalized), tool use in facing direction, interact key, hotbar of 12 slots, backpack starts at 12 slots, upgradeable to 36.
- **Energy** (max 270, upgradeable) and **Health** (max 100). Tools consume energy; food restores it.
- **Tools**: Hoe, Watering Can, Axe, Pickaxe, Scythe, Fishing Rod. Upgrade tiers: Basic, Copper, Iron, Gold (more area/less energy) at the blacksmith over 2 days.
- **Skills** (levels 1-10, XP by use): Farming, Foraging, Mining, Fishing, Combat. Each level unlocks recipes; levels 5 and 10 offer a profession choice.
- Customization: name, farm name, favorite thing, a few body/hair/outfit options (layered sprites).

### 3.3 Farming
- Farm tiles states: untilled -> tilled -> watered (resets each night). Crops advance one growth stage per watered day.
- Crops: season-bound, multi-stage, some regrow, some multi-harvest. Initial target: **24 crops** across seasons (6 per season) plus 4 trees (fruit).
- Quality tiers on harvest: Normal, Silver, Gold, Iridium (driven by Farming level and fertilizer).
- Fertilizer, sprinklers (tiers), scarecrows (crow event), greenhouse (all-season) as unlockables.
- Seeds from the general store. Out-of-season crops die on season change.
- Farm animals (post-MVP milestone): coop (chicken) and barn (cow, goat), daily feeding/petting, products.
- Farm clutter: trees, weeds, stones, stumps. Clearable with tools; regenerates slowly.

### 3.4 Foraging, fishing, mining
- **Foraging**: seasonal spawns on maps each morning.
- **Fishing**: timing mini-game, fish species by location/season/time/weather, ~20 fish at 1.0.
- **Mining**: procedurally generated floors (seeded per day), 40 floors in the first mine, stairs, ladders, ore nodes (copper, iron, gold), gems, simple enemies (5 types + 1 boss at 1.0). Elevator checkpoints every 5 floors. Dying drops some items and returns player to town.

### 3.5 Economy and items
- Gold currency. Shipping bin pays at day end (summary screen). Shops: general store, blacksmith, carpenter, fish shop, traveling merchant (random days).
- Item categories: seed, crop, forage, fish, ore/gem, resource (wood, stone, fiber, coal), artisan good, food, tool, tool-upgrade, furniture, fertilizer, machine, quest item.
- **Crafting** (learned recipes, inventory ingredients): sprinklers, fences, chests, furnaces, kegs, preserves jars.
- **Cooking** (kitchen in house): food buffs (speed, luck, max energy).
- **Artisan machines**: furnace (ore -> bars), keg (fruit -> juice), preserves jar. Timed processing in world time.

### 3.6 Town and NPCs
- 1 town map, farm, farmhouse interior, 6 buildings (general store, blacksmith, clinic/library, saloon, carpenter, fish shop), forest, beach, mine entrance.
- **12 NPCs at 1.0** (8 romanceable-optional, 4 non-romance; keep romance optional and all-ages friendly), each with: daily schedule per season/weather/day-of-week, 3 loved / liked / disliked gifts, dialogue pools by friendship tier, 3-4 heart events (cutscenes).
- Friendship 0-10 hearts (250 points/heart). Gifting twice a week, birthdays 8x points. Talking daily gives a small amount.
- Marriage/spouse is a post-1.0 stretch (do not block 1.0).

### 3.7 Quests and progression
- Main goal: restore the **Community Hall** (bundle-style collection quests, ~6 rooms) -> unlocks story ending and rewards.
- Daily "help wanted" board quests (fetch/slay/fish).
- Mailbox letters (tutorial, events, gifts) drive early game.
- 1.0 content target: ~40-60 hours first playthrough.

### 3.8 Festivals and events
- 1 festival per season at 1.0 (e.g., spring flower dance analog, summer fishing contest, fall fair, winter feast) - original names/rules.
- Cutscene system with scripted actors, camera, dialogue (data-driven).

### 3.9 UI / UX
- HUD: clock/date/weather, energy bar, gold, hotbar, tool-use cursor, tooltips.
- Menus: inventory, skills, crafting, social, map, collections, options, save/load, quit.
- Dialogue box with portraits, choices, and item-reward popups.
- Support keyboard+mouse and gamepad (Steam Deck). Full rebinding. Text scale option. Colorblind-safe indicators.
- Localization-ready from day one (all strings via Unity Localization; English only at launch).

### 3.10 Audio
- Seasonal music, location music, ambience (weather), SFX per tool/footstep surface/UI. Mixer groups: Master, Music, SFX, Ambience, UI.

### 3.11 Art direction
- 16x16 tile grid, characters ~16x32, 2D pixel art, **Pixel Perfect Camera** at a 480x270 reference (integer scaling), warm palette per season, URP 2D lights for day/night and indoor lamps.
- Art is produced by humans or licensed/AI-assisted per project policy; agents use **placeholder art** (see Tech Design section 9) until final art is dropped in using the same sprite names/slice layout.

## 4. Out of scope for 1.0
Multiplayer co-op, marriage/children, modding API, console ports, mobile, more than 1 town map.

## 5. Steam release requirements
- Steamworks via **Steamworks.NET** (or Facepunch) with achievements (~30), cloud saves, rich presence (optional), Steam Input friendly.
- Windows x64 + Linux x64 depot builds, tested on Ubuntu LTS and SteamOS (Steam Deck verified target).
- Store page assets, trailer, capsule art, age rating questionnaire, EULA/privacy text.
- Performance: 60 FPS on integrated GPU / Steam Deck at 1280x800; <2 GB RAM; load times < 5 s between maps.

## 6. Success criteria (Definition of Done for 1.0)
- Complete a full year without game-breaking bugs; all 12 NPCs and the Community Hall are completable.
- Save/load round-trips at every point; saves from patch N load in patch N+1 (versioned migrations).
- Passes the Release checklist in the Implementation Plan.
