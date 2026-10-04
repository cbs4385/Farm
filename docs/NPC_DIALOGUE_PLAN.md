# Plan: NPC dialogue, interaction and relationships for a game worth streaming

Status: **accepted 2026-10-02 (revision 2)**, decisions recorded in `adr/0005-narrative-depth-and-stream-appeal.md`. Nothing here is built yet except what `NPC_DIALOGUE_ANALYSIS.md` section 9 lists.
Inputs: `NPC_DIALOGUE_ANALYSIS.md`, `01-GameDesign.md`, `adr/0002`, `adr/0003`, `adr/0004`.
Covers everything the analysis marked **not done** (R8, R9, R11-R14, R16-R18, the unfinished parts of R6, R10 and R15) plus the production, tooling, audio, quality and **stream-appeal** work needed to reach a professional-publisher standard.

## 1. Goal and what "good" means

**Business goal.** An indie game that people want to watch on Twitch and YouTube, so that streams and clips drive purchases. For this game that means:

- A viewer who joins a stream at any minute sees *something happening*: a joke, a reaction, a choice, a surprise.
- A streamer can play for dozens of hours without the villagers repeating themselves, and a second save plays out differently from the first.
- Chat can take part: suggest names, vote on choices, argue about gifts and favourite villagers.
- A 30-second clip makes sense and makes people want to play.
- The cozy game is warm and safe to stream; the optional horror layer gives a reason to speculate and come back.

**Six quality pillars** (from the analysis) plus a seventh for streaming:

| Pillar | Bar | Today |
|---|---|---|
| Breadth | Thousands of lines; rarely a repeat within weeks | about 8 distinct lines a visit |
| Depth | Every villager has a multi-stage arc with a peak and a resolution | 2 short heart events each |
| Reactivity | The world notices season, weather, calendar, the farm, past choices, other villagers | season, weather, hearts |
| Choice and consequence | Remembered choices, referenced later | one trivial choice |
| Presentation | Expressions, emotes, staging, camera, audio identity | one static portrait, no sound |
| Craft and process | Bible, style guide, editing, tooling, QA metrics, localisation-ready text | hand-edited JSON |
| **Stream appeal** | Varied interaction types, surprise, quotable and clippable moments, chat participation, replay variety | none designed |

**Honest limits.** Recorded voice acting, motion capture and a large writing team are not available (ADR 0005 D5, D6). The plan replaces them with what the best life sims also lean on: voice blips and signature sounds, emotes, systemic repetition-avoidance, personality-driven barks, rare lines, and heavy use of conditions. Comparisons to other games come from general knowledge, not fresh research.

## 2. Targets (the measurable definition of done)

### 2.1 Content budget

Full villager = every row at target. Enhanced villager = about half of the rows marked with an asterisk (`*`), all other rows kept. Horror content stays in `Farm.Mythos` and data.

| Content type | Per Full villager | Launch total (6 Full + 6 Enhanced) | All Full (12) |
|---|---|---|---|
| Daily talk lines* | 150 | 1,350 | 1,800 |
| Gift reactions* (tiers, per-item, repeat cases) | 24 | 216 | 288 |
| **Gift lab** (funny reactions to absurd and disliked gifts) | 40 | 360 | 480 |
| Heart events* | 6 (hearts 2, 4, 6, 8, 10 and a friend-day scene) | 54 | 72 |
| Birthday scene and 2 letters | 1 + 2 | 12 + 24 | same |
| Personal quest chain* | 3 quests, 6-9 dialogues | 27 | 36 |
| **Social actions** (joke, compliment, ask advice, tease, share a snack): 8 lines each | 40 | 360 | 480 |
| Festival lines | 6 | 72 | 72 |
| Mail* | 6 | 54 | 72 |
| Ambient barks* | 30 | 270 | 360 |
| Rare and legendary lines | 4 | 48 | 48 |
| Overheard villager-to-villager scenes (6 lines) | 40 total | 240 | 240 |
| **Seeded village storylines** (T-143): 10 storylines of about 60 lines | n/a | 600 | 600 |
| **Name reactions** (farm, pet, player names) | n/a | 60 | 60 |
| Gazette templates (T-146) | n/a | 100 | 100 |
| **Total** | | **about 7,500 lines (about 65k words)** | **about 9,000** |

Line budgets: barks at most 12 words; talk lines at most 28; event lines at most 40; never more than three text-box lines at the default font.

### 2.2 Quality targets (checked by tools and playtests)

- **Repetition:** over a simulated 28-day season of daily talks with one villager, no line repeats within 14 days at 3+ hearts; fewer than 20% of visits repeat the line from two visits ago.
- **Coverage:** a one-year bot run hears at least 60% of each Full villager's talk lines; every line is reachable (CI-enforced, extending `DialogueVarietyTests`).
- **Reactivity:** at least 50% of daily lines carry a condition beyond hearts; every visit includes at least one line tied to season, weather, calendar, the farm or a past choice.
- **Consequence:** every heart-event choice sets a flag and is referenced by at least one later line.
- **Voice:** the line-lint passes for every villager; in blind tests players attribute at least 80% of unlabelled lines to the right villager.
- **Presentation:** every line has an expression (explicit or inferred); each villager has 6 expressions and a signature sound.
- **Accessibility:** text speed, auto-advance, a backlog, a high-contrast text box and an optional dyslexia-friendly font work with keyboard, mouse and gamepad at 1280x800 and 1920x1080.
- **Safety:** all-ages review passes; at `HorrorLevel` 0 behaviour matches the cozy-only game.

### 2.3 Stream-appeal targets (new)

- **Moment density:** at least one *clippable moment* (tagged funny, wholesome, surprise or mystery) per 10 minutes of ordinary play, measured by tag counts in simulation and confirmed in watched playtest VODs.
- **Quotas:** per Full villager, at least 3 funny, 2 wholesome, 1 surprise and 1 quotable running bit; across the game at least 25 set-piece scenes.
- **Interaction variety:** a villager visit offers at least four kinds of interaction (talk, topic, social action, gift) and no more than two visits in a row feel identical.
- **Replay variety:** each new save draws 4 of 10 village storylines from its seed; two saves must differ in at least 3 storylines and in the order of seasonal gossip.
- **First hour:** by minute 20 the player has met three villagers with distinct hooks and made a choice that has a visible consequence; by minute 45 a first emotional or funny set piece; by the end of day 3 a hook that makes people continue (a mystery letter, an unclaimed gift, a rumour; a stronger horror tease at `horror:1` or higher).
- **Chat participation:** every choice can be selected by number key; an optional chat-vote timer works; a name filter protects streams; a stream mode exists (T-145).
- **Dead air:** in the first 10 hours of the bot run, no stretch longer than 8 game-days without a new villager line category, event or storyline beat.
- **Rare finds:** at least 48 rare and legendary lines, tracked in a journal so completionists and streamers chase them.

## 3. Principles and constraints

1. **Originality.** No text, characters or beats copied from other games or Lovecraft-inspired works.
2. **Data-driven.** Content in story data and `en.json`; generic engine code; no villager-specific code.
3. **Layering (ADR 0002, 0004).** Nothing in core depends on `Farm.Mythos`. Reserved priority bands (validator rule): cozy talk 0-3, reactions 3-4, quest offers 4-6, one-shot story scenes 6-7, mythos 8-10, first meeting and tutorial 100.
4. **Stable ids.** New state in flags, vars and module data, never new `GameState` fields; existing ids never renamed.
5. **Failure isolation.** New hooks, atoms and step types degrade safely and are tested.
6. **Cozy tone.** No punishing choices; relationships only suffer from neglect; all-ages romance. Humour punches up or sideways, never at real groups. Mythos shadows of these arcs live in `Farm.Mythos` data.
7. **Stream safety.** All text and audio are original (no licensing problems on stream); no slurs can be produced by name entry; horror level is visible and changeable; nothing requires Twitch or YouTube integration.
8. **Verification honesty.** `STATUS.md` records what a person verified. Nobody has yet read the lines added so far.

## 4. Workstreams

IDs use block **T-080 to T-150** as **Milestone 4b - Narrative depth and stream appeal**, between M4 and the release candidate gate (T-069). "L" marks tasks required for launch; "U" marks update-tier work.

### WS-A Narrative foundation

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-080 | ADR 0005 and decisions | L | none | **Done**: `adr/0005`, GDD pointer, STATUS rows |
| T-081 | **Narrative bible** (`docs/narrative/BIBLE.md`) | L | T-080 | Per villager: role, voice rules (vocabulary, tics, length), backstory, wants and fears, relationship to every other villager, **humour type** (dry, punny, deadpan, exuberant, anxious ...), taste rationale, 10-heart arc, yearly cycle, a **signature bit** (running gag, catchphrase, sound), mythos-compatibility note for Keepers (Tilda, Marcus, Odalys, Dorian, Wren) and resister (Hazel). Village timeline and recurring places |
| T-082 | **Writing style guide** (`docs/narrative/STYLE.md`) | L | T-081 | Length budgets, punctuation, humour rules, kindness without saccharine, inclusive language, no gendered assumptions about the player, tokens, expression and emote tags, **moment tags** (funny, wholesome, surprise, mystery), rarity tags, line categories and quotas, "write for the clip" rules (a line should land without context) |
| T-083 | Production pipeline and gates | L | T-081, T-082 | Outline, draft, consistency edit, voice lint, sensitivity read, integration, playtest. Definition of "locked". Named human reviewer per gate (ADR 0005 D6, D8) |
| T-084 | Friendship pacing model | L | T-081 | Model in `docs/balance/`: points per heart across talk, gifts, social actions, events, decay; typical player reaches 10 hearts with 4-6 villagers in years 1-2; tuned by simulation (T-138) |

### WS-B Dialogue and selection engine v2

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-090 | **Line variety engine** | L | T-080 | Per-villager "recently heard" memory (module data), cooldowns, weights, categories, **rarity tiers** (common, uncommon, rare, legendary with weights), "never-heard first" bias; deterministic by day. Back-compat with existing sets. AC: 2.2 repetition targets pass in simulation |
| T-091 | **Reaction queue** | L | T-090 | Systems publish events (harvest, quest done, festival approaching, purchase, level up, new building, animal born, sickness, someone else's gift). Villagers queue reactions with a time-to-live that play before idle chat, at most once. Data: `reactions` in story JSON (`on`, `condition`, `ttlDays`, `dialogue`) |
| T-092 | **New condition atoms** | L | T-091 | `festival.in:<days>`, `festival.today`, `birthday.in:<npc>:<days>`, `farm.crops>=N`, `farm.animals>=N`, `built:<id>`, `season.day>=N`, `heard:<id>`, `choice:<flag>`, `streak.talk>=N`, `mood:<npc>=<state>`, `weather.yesterday:<id>`, `pet:<id>`, `storyline:<id>`, `farmname:<text>`. Each registered via `StoryConditions` with validator support and tests; false when unavailable |
| T-093 | **Villager mood** | L | T-091 | Daily derived mood (content, tired, worried, delighted, lonely, mischievous) from weather, schedule, neglect, recent events, arc flags; stored as a var; drives greetings and expressions; no gameplay penalty |
| T-094 | **Rich dialogue text** | L | T-090 | Markup: `{pause=0.4}`, `{speed=slow}`, emphasis, inline variation `{a|b|c}` (deterministic), inline conditionals, tokens (`[season]`, `[pet]`, `[crop]`, `[neighbour:<npc>]`); validator and tests; mythos text filters still work |
| T-095 | **Dialogue node extensions** | L | T-094 | Optional per-node `expression`, `emote`, `sfx`, `voice`, `camera`, `tag` (moment tag), and choice `tone` icons (kind, honest, playful, shy, curt). All optional so existing data works; validator checks asset ids |
| T-096 | **Dialogue UI v2** | L | T-095 | Expressions, emote bubbles, per-speaker plate colour, text speed, auto-advance, skip, **backlog** (last 30 lines), tone icons, **number-key choice selection**, letterboxed event mode, long-line layout, controller and keyboard, Steam Deck check; 1080p stream readability (minimum 24 px equivalent) ; capture checks on the player build |
| T-097 | **Conversation topics** | L | T-090, T-092 | After the main line a villager offers 0-2 optional topics (shop, neighbour, mood, local gossip, advice) unlocked by hearts and flags; once per day each |

### WS-C Event and cutscene engine v2

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-100 | **New step types** | L | T-095 | `emote`, `expression`, `anim` (sit, wave, laugh, shrug, sigh, point, carry, dance), `camera` (focus, pan, zoom, shake), `sfx`, `music`, `lighting`, `branch`, `parallel`, `waitFor`, `spawn`/`despawn`, `letterbox`. Optional and failure-isolated; validator knows them. Heart events can already branch via the existing `dialogue` step |
| T-101 | **Staging validator and camera safety** | L | T-100 | Extend the free-floor test to every move and spawn; no stuck actors; camera inside map bounds; skipping always lands consistently (`SkipEffects`) |
| T-102 | **Memories gallery** | L | T-100 | Replay of seen heart events and festivals from the menu (no effects on replay); unlocked by flags. Also a clip-friendly way to show a scene again |
| T-103 | **Heart-event template library** | L | T-100 | Reusable patterns (shared activity, confession, heirloom, shared meal, helping scene, prank, performance) as parameterised templates; tests instantiate each |

### WS-D Relationship and gift systems

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-105 | **Full gift tastes and item reactions** | L | T-081 | 3 loved, 3 liked, 3 disliked per villager (GDD) plus category tastes, validated against item ids; per-item reactions for every loved item and a line per category; "already gave this" and repeat-gift fatigue; tests |
| T-106 | **Relationship stages and journal** | L | T-084, T-093 | Named stages (acquaintance, neighbour, friend, close friend, confidant), a **Neighbours page** per villager (portrait, birthday, discovered likes and dislikes, notes they told you, stage, arc hints), plus a **Gossip Book** tracking rare lines and storylines found |
| T-107 | **Birthdays as set pieces** | L | T-100, T-105 | 12 scenes with one choice and a gift back, calendar markers, a reminder letter the day before and a thank-you after; a letter if the player is absent |
| T-108 | **Neglect and warmth** | L | T-093 | "Missed you" greeting and a return bonus; decay still via the existing hook |
| T-109 | **Courtship without marriage** (ADR 0005 D3) | L (Full six) / U | T-080, T-081 | Optional path at 8 hearts for the romanceable villagers, ending in a "partners" stage and scene; all-ages, never required; heart-10 "closest friend" scene for the rest |

### WS-E Content production (writing)

One task per villager (T-110 to T-121) using the checklist below. Order (waves):

| Wave | Villagers | Tier | Why |
|---|---|---|---|
| Slice | **Wren, Hazel, Bram** | Full | Talkative, shy, terse; a Keeper, the resister; strong comic hooks |
| 2 | **Tilda, Juno, Piper** | Full | Onboarding shopkeeper; funniest apprentice; performer who gives clips |
| 3 (launch Enhanced, update Full) | **Dorian, Elara, Felix** | Enhanced, then Full in updates 1-3 | Mystery, warmth, fishing community |
| 4 (launch Enhanced, update Full) | **Ione, Marcus, Odalys** | Enhanced, then Full in updates 4-6 | Library, craft, clinic |

Task ids: T-110 Wren, T-111 Hazel, T-112 Bram, T-113 Tilda, T-114 Juno, T-115 Piper, T-116 Dorian, T-117 Elara, T-118 Felix, T-119 Ione, T-120 Marcus, T-121 Odalys.

**Per-villager checklist:**

1. Bible entry locked (T-081), with humour type and signature bit.
2. Talk lines (150 Full, about 75 Enhanced), tagged by category, condition, moment tag and rarity; voice-linted.
3. Gift reactions, **gift lab** (40), per-item loved lines; taste check (T-105).
4. Heart events (6 Full, 3 Enhanced) with remembered choices, staging, emotes, camera and sound cues.
5. Birthday scene, 2 letters, 6 other letters, festival lines.
6. **Social actions** (40 lines) matched to the villager's humour type, including comic failures.
7. Personal quest chain (3 quests Full, 1 Enhanced), priorities in the reserved bands.
8. Barks (30 Full, 15 Enhanced) and overheard scenes.
9. Reactions to at least 12 world events (T-091).
10. 3 rare lines and 1 legendary line (Full).
11. Mythos compatibility for the six with a role (no contradiction with clue scenes, bands respected, variants written in `Farm.Mythos`).
12. Expression and sound cues listed per line and scene.
13. Reachability, repetition, coverage and moment-quota reports attached; a named human has read it; STATUS updated.

**Cross-cast tasks:**

| ID | Task | Tier | Deps | Deliverables |
|---|---|---|---|---|
| T-122 | Villager-to-villager content | L (Full six pairs) / U | wave 2 | 40 overheard scenes, 6 relationship arcs with pairings and rivalries as fandom hooks (Juno and Bram, Hazel and Ione, Elara and Dr. Penn, Marcus and Dorian, Wren and Piper, Tilda and Felix), gossip topics (T-097), cameos in heart events |
| T-123 | Festival and calendar content | L | wave 2 | Four festivals with per-villager roles, a scene each, day-before and day-after reactions, village barks; one festival moment designed to clip |
| T-124 | Shop and service integration (R9) | L | T-092, T-091 | Tool upgrade ready, new stock, build progress, open tab, sale; read through `IGameQuery` |
| T-125 | Ambient barks system | L | T-091, T-100 | Speech bubbles near the player with category, range, cooldown and an accessibility toggle |

### WS-F Presentation: art and audio

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-130 | **Expression portraits** | L (Full six) / U | T-082, T-060 | 6 expressions per villager through `tools/art/` and `docs/art_prompts/03_portraits.md`, an emote-bubble set; licence and disclosure review as for T-060 |
| T-131 | **Character animation set** | L | T-130 | Sit, wave, laugh, shrug, sigh, point, carry, dance for villagers and player |
| T-132 | **Voice identity** | L | T-095 | Original voice blips per villager plus a **signature sound** each (the audio equivalent of a catchphrase, used in barks and set pieces); volume bus and off switch |
| T-133 | **Music identity** | L | T-061 | A short leitmotif per villager as stingers in events; an event music bus. Depends on the blocked audio pass |
| T-134 | Text-box themes | L | T-096 | Per-villager plate accent and subtle frame variation, consistent with `UiKit` and colour-blind palettes |

### WS-G Tooling and QA

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-135 | **Authoring format and compiler** (`.fscript`) | L | T-094 | Screenplay-style text compiled to story JSON and `en.json` keys; lines, conditions, choices, effects, tags; stable generated keys; round-trips existing JSON; menu `Farm/Compile Story`. Built **before** the volume writing |
| T-136 | **Narrative validator and reports** | L | T-090 | Priority bands, orphan keys and dialogues, unreachable lines, missing expressions, length and voice lint, duplicate text, token misuse, mythos-band misuse, **moment-tag quotas**; per-villager reports to `Builds/` |
| T-137 | **Dialogue debugger** | L | T-090 | F1 console: set hearts, date, weather, mood, flags, storyline; force a pick; list heard lines; replay any dialogue or event; explain exclusions. Dev builds only (release guard must pass) |
| T-138 | **Story simulation bot** | L | T-090, T-091 | Extends the soak tests: plays 1-3 years talking, gifting, using social actions; reports coverage, repetition, pacing and moment density; CI fails on regressions of 2.2 and 2.3 |
| T-139 | Localisation readiness | L | T-135 | Translator context notes, pseudo-localisation test, plural and gender-neutral rules, CSV or XLIFF export with stable ids; Unity Localization swap only if languages are added |

### WS-H Interaction variety and stream appeal (new)

| ID | Task | Tier | Deps | Deliverables / AC |
|---|---|---|---|---|
| T-140 | **Moment framework** | L | T-082, T-095 | Moment tags in data; quotas in 2.3 enforced by T-136; a "clip-worthiness" checklist for set pieces (setup, turn, payoff, readable without context, under 40 seconds) |
| T-141 | **Social actions** | L | T-097, T-093 | A small action menu beside Talk and Gift: **Joke** (pick a punchline that suits their humour type), **Compliment**, **Ask advice**, **Tease** (cozy-safe), **Share a snack**, plus **Invite on a walk** for hearts 4+. Each has personality-matched reactions including comic failures and rare outcomes; at most two per villager per day with diminishing points; tests for every action and tone |
| T-142 | **Gift lab** | L | T-105 | A unique reaction to every disliked item and to a list of absurd gifts per villager (a fish to the shopkeeper, a rock to the librarian); "regift" detection jokes; viewers love testing this, so every villager has a signature bad-gift reaction |
| T-143 | **Seeded village storylines** | L | T-092, T-090 | 10 cozy storylines of about 60 lines (rivalries, mysteries, mini-sagas such as a pie feud, anonymous notes, a competing band, a missing prize pumpkin). Each new save draws 4 from its seed (stored in flags); the rest are possible on replay or from the update roadmap. Deterministic, save-safe, conditioned on `storyline:<id>`; horror layer unaffected |
| T-144 | **Rare and legendary lines** | L | T-090 | Rarity tiers in the variety engine; 48 rare finds; **Gossip Book** page (T-106) lists found lines without spoilers; an achievement for completing it |
| T-145 | **Stream mode and audience features** | L | T-096 | Settings: **Stream Mode** (hides save paths and account names in UI, larger text, brisk text speed, number-key choices, visible horror-level banner), **name filter** for player, farm and pet names (blocklist plus a "try something else" reply), optional **chat-vote timer** for choices (off by default, pauses the clock, defaults to a configured choice on timeout), no required platform integration |
| T-146 | **Shareable moments** | L | T-102, T-062 | **Photo mode** with villager poses and emotes; the weekly **Village Gazette** (a generated page from the week's gossip and the player's deeds, screenshot-friendly); Memories replays; optional platform timeline markers through `IPlatformServices` (null by default; evaluate Steam's timeline and recording features before committing) |
| T-147 | **First-hour script** | L | T-110 to T-115 | A written and tested beat sheet for the first 60 minutes and the first 3 days (2.3): who the player meets, the first funny line, the first choice with consequence, the hook; verified by watched playtests of new players |
| T-148 | **Name reactions** | L | T-092 | Villagers use the farm name and react to about 60 well-known joke names and words (original replies); safe fallbacks for others |
| T-149 | **Creator and playtest programme** | L | T-147 | Creator preview build, press kit listing streamable scenes and a "creator-safe options" note, 5-10 outside playtesters including content creators, with watched VODs reviewed for dead air and confusing moments |
| T-150 | **Post-launch cadence** | U | T-149 | Roadmap of six **Neighbour Spotlight** updates (Dorian, Elara, Felix, Ione, Marcus, Odalys) plus seasonal storyline drops; save-compatible; announced with trailers cut from the new set pieces |

## 5. Phasing and gates

| Phase | Contents | Gate |
|---|---|---|
| **0 Foundation** | T-080 (done), T-081 to T-084 | Owner reviews the bible and style guide for the slice villagers |
| **1 Engine and tools** | T-090 to T-097, T-100 to T-103, T-135 to T-137, T-140, T-141, T-145 | Engine tests green; compiler and debugger usable; player-build capture checks pass |
| **2 Vertical slice** | Wren, Hazel, Bram (T-110 to T-112) at Full, with T-105, T-142, T-130, T-132, first slice of T-143 storylines and T-147 draft | **Slice gate:** a person plays a full in-game year; outside playtesters stream or record sessions; score against 2.2 and 2.3; **recalibrate the plan** |
| **3 Launch production** | Tilda, Juno, Piper at Full; the six Enhanced villagers; T-106 to T-109, T-122 to T-125, T-133, T-134, T-139, T-143 to T-148 | Each wave passes the checklist and a human read before the next |
| **4 Launch gate** | T-138 simulation run, T-149 creator playtests, accessibility audit, pseudo-localisation, sensitivity review, first-hour retest | **Launch narrative gate**, required before T-069 |
| **5 Updates** | T-150 and the six Spotlight updates | Each update has its own gate (checklist, human read, simulation) |

If the schedule slips, cut in this order: Enhanced-tier depth, T-122 cross-cast beyond the Full six, T-146 beyond photo mode, T-109 for the Enhanced six. Never cut the variety engine, the compiler, social actions, stream mode, the slice, or the first-hour script.

Dependencies on other blocked work: expression and animation art depend on T-060; leitmotifs depend on T-061; everything else is code and writing.

## 6. Effort and capacity

Estimates in **focused agent work sessions** (one session ends with tests green). Human review is separate owner or playtester time.

| Work | Sessions | Human time |
|---|---|---|
| Foundation (T-080 to T-084) | 6-8 | bible and style guide read, 3-4 hours |
| Engine, tools, stream features (WS-B, C, G, H systems) | 45-60 | capture checks and a play session per phase |
| Launch writing: 6 Full villagers at 8-10 sessions plus 6 Enhanced at 4-5 | 78-90 | about 1.5 hours per villager read |
| Cross-cast, storylines, barks, festivals (T-122 to T-125, T-143, T-147, T-148) | 30-40 | one play session |
| Art and audio (WS-F) | 20-30 plus the external audio dependency | art review per wave |
| QA, creator programme, polish (T-138, T-139, T-149) | 12-16 | two outside playtest rounds |
| **Launch total** | **about 190-245 sessions** | **about 40-50 hours** |
| Updates (6 Spotlights at 5-6 sessions, plus storyline drops) | 40-50 | per update: a read and a playtest |

## 7. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Scope and schedule | High | Tiered launch (ADR 0005), slice gate recalibration, defined cut order |
| Volume without quality (filler) | High | Quotas, repetition targets, moment tags, reaction and topic systems so lines are conditional, rarity tiers |
| Voice sameness | High | Bible with humour types, voice lint, blind attribution test, signature bits |
| Humour that doesn't land on stream | High | Clip-worthiness checklist, watched playtests of creators, iterate the first hour |
| Chat misuse (offensive names or choices) | Medium | Name filter, safe fallbacks, no required integrations, content review of name reactions |
| Stream-mode features becoming Twitch-specific | Low | Keep them generic: shortcuts, timers, hidden info |
| Mythos collisions | Medium | Priority bands, validator rule, compatibility notes, `HorrorLevel` parity tests |
| Save compatibility | Medium | New state in flags, vars and module data; seed stored in flags; archived-save tests rerun |
| Engine regressions in UI and events (build-only issues have happened) | Medium | Capture checks on the player build per UI task; simulated-input tests; one MonoBehaviour per file |
| Art consistency of generated expressions | Medium | Updated style guide and prompts, human art review per wave |
| Audio blocked | Medium | Procedural blips and stingers first; real assets swap in by id |
| Seeded storylines make QA harder | Medium | Each storyline independently reachable and tested; bot runs all 10 across seeds |
| AI-authored text never read by a human | High | STATUS names the reader; slice, wave and launch gates require a human read |

## 8. Per-villager arc seeds and hooks (cozy layer; to be developed in the bible)

Warm, low-stakes arcs that resolve happily and leave room for the mythos layer to colour them. The *hook* is the stream-friendly bit.

| Villager | Arc seed | Hook (funny, quotable or clippable) | Heart-10 payoff |
|---|---|---|---|
| Wren | Hates cooking but loves hosting; builds a seasonal menu from the player's produce | Gossip bingo: rumours about other villagers that evolve and contradict each other | A recipe named after the farm |
| Hazel | Reads and writes in secret; reads aloud once in public | Whispered book recommendations that cleverly mirror the player's choices | A small poem book with a dedication |
| Bram | Gruff mentor writing to a retired master smith; fears his apprentice will outgrow him | The longest sentence he ever says, saved for heart 6 | A tool forged to the player's own wear patterns |
| Tilda | Keeps her late mother's shop unchanged; talked into a community seed library | Shop-counter banter that reacts to what you buy | A shelf named for the farm |
| Juno | Wants to make something nobody has seen; enters a first original piece | Her burned-clasp tally, an escalating gag | Her mark on the player's tool |
| Piper | Rewrites the village song forever | Improvised songs about whatever the player did that week | The song with the farm's verse |
| Dorian | Shows one secret place a year; deciding whether to stay | Silent communication by emotes | He keeps the last place for the player |
| Elara | Wants to train as a doctor; Dr. Penn mentors | Over-cheerful triage jokes | A shared clinic shift |
| Felix | Solitary; revives the summer fishing contest | Absurd fish-fact predictions | His father's lure box |
| Ione | Compiling a village almanac and afraid to finish it | Hilariously wrong weather forecasts | The almanac dedicated to the neighbours |
| Marcus | Never finished his father's footbridge | Measure-twice proverbs that get stranger | The bridge opens at the winter festival |
| Odalys | Overworks; learns to rest; shares a clinic garden | Deadpan prescriptions | The garden in bloom |

**Seeded storyline candidates (T-143; ten, four drawn per save):** the pie feud, the anonymous notes, the competing band, the missing prize pumpkin, the library ghost that is only a draft, the lost-and-found of the year, the weather-forecast wager, the great scarecrow contest, the village's rival tea blends, the beach treasure map. Each has a cozy resolution and, where the horror layer wants it, a separate mythos shadow in `Farm.Mythos`.

## 9. Acceptance for the whole plan

- All T-080 to T-149 rows are done in `STATUS.md` with human verification noted per villager, and T-150 has a published roadmap.
- Targets in 2.2 and 2.3 are met in simulation and confirmed by creator and outside playtests.
- Windows and Linux player builds pass the capture checks with the new dialogue UI, expressions, events and stream mode.
- EditMode and PlayMode suites are green, the validator is clean, and the release-build guard passes.
- `HorrorLevel` 0, 1 and 2 pass the layer-parity tests with the new content.
- Docs updated: GDD, Tech Design (engine v2), QA (new checks), narrative bible and style guide.

## 10. Decisions (resolved; see ADR 0005)

| ID | Decision | Resolution |
|---|---|---|
| D1 | Timing | Launch tier (6 Full + 6 Enhanced) and free Neighbour Spotlight updates |
| D2 | Villager count | All 12; two depths |
| D3 | Romance | Optional courtship without marriage; villager pairings written as fandom hooks |
| D4 | Languages | English at launch; pipeline ready; add languages by wishlist data |
| D5 | Voice | Blips and signature sounds; no recorded voice |
| D6 | Writing process | Agent drafts, named human reviews each villager before lock |
| D7 | Authoring format | Build `.fscript` first |
| D8 | Reviewers | Owner for tone and voice; 5-10 outside players and creators for gates |

## 11. First steps

1. T-081 and T-082 for the three slice villagers; the owner reviews them.
2. In parallel: T-090 (variety engine) and T-135 (compiler), then T-094 to T-096 and T-141 (social actions).
3. Build the slice (Wren, Hazel, Bram) with T-105, T-142, T-130 and T-132 for those three; start T-143 with two storylines.
4. Hold the slice gate with outside players recording sessions; recalibrate; then launch production.
