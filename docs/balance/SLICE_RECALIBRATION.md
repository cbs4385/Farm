# Slice recalibration (the Phase 2 gate)

Date: 2026-10-03. Source: the story simulation bot (T-138, `StorySimulation`, `StorySimulationTests`; the full report is written to `Builds/story_simulation.md` on every test run) and the narrative lint with Wren, Hazel and Bram locked. The owner approved the slice gameplay on 2026-10-03, which opens Phase 3; this note records where the measured slice stands against `NPC_DIALOGUE_PLAN.md` sections 2.2 and 2.3 and what the plan should change.

**What the bot does.** It talks to Wren, Hazel and Bram every day for two game years (224 days), visits them mostly at their workplace, rolls weather, moods and the calendar, raises hearts by talking plus two liked gifts a week (the "engaged player" of `narrative/PACING.md`), sets heart-event flags as hearts are reached, takes the first choice in every conversation and so accepts quests and finishes story beats. It uses the real variety engine. It does not run scenes, topics, social actions or reactions, so every number below is about **daily talk lines only**.

## Measured against the targets

| Target (plan section) | Plan | Measured | Verdict |
|---|---|---|---|
| Repetition: no ordinary line repeats inside 14 days at 3+ hearts (2.2) | 0 | **0** for all three | met |
| Repetition: fewer than 20% of visits repeat the line from two visits ago (2.2) | < 20% | **0%** | met |
| Reactivity: at least 50% of lines carry a condition beyond hearts (2.2) | 50% | **73 to 75%** | met |
| Coverage: a daily-visiting player hears 45% of a Full villager's lines in year one and 60% in year two (2.2, recalibrated) | 45% / 60% | **43 to 48%** in year one; **60 to 65%** in two years | met (the target was recalibrated by the owner on 2026-10-03 from 60% in one year) |
| Dead air: no stretch over 8 game-days without something new (2.3) | 8 days | **11 days** with no new talk line from any of the three (13 to 18 per villager) | not met by the stricter "new line" measure; the plan's wording also counts events and storyline beats, which the bot does not play |
| Moment density: one clippable moment per 10 minutes (2.3) | 1 per 10 min | **0.7 tagged talk lines heard per game day** (three villagers, daily); scenes and storylines are on top | cannot be judged until the length of a game day in minutes is fixed and scenes are counted |
| Replay variety: two saves differ in at least 3 storylines (2.3) | 3 | **at most 1** | **not met, and cannot be with 5 storylines** (see below) |
| Quotas: 3 funny, 2 wholesome, 1 surprise per Full villager; 25 set pieces (2.3) | quotas | met by the lint (Wren 20 funny, 15 wholesome, 2 surprise; Hazel 10, 16, 2; Bram 18, 11, 3; 53 tagged scenes) | met |
| Voice lint passes for every villager (2.2) | pass | **0 errors**, all three locked | met |

## What the bot found and fixed

1. Four storyline lines per villager (the pie feud and the notes) never switched off, so they were spoken about 16 times each over two years and took about 30% of every visit. They now stop when their storyline is finished (`!flag:storydone.<id>`). This is also a story fix: "day nine of the feud" no longer contradicts a finished feud.
2. Aftermath lines ("the feud is over") sat in a high priority band, where they crowded out the ordinary pool; they are now ordinary priority. A permanent condition in a priority band above ordinary chat will crowd out the pool: the rule for writers is that **only windows (a festival in three days, a birthday, a storyline until it finishes) may use priority 1 to 3; permanent state belongs at priority 0**.
3. After those fixes, ordinary-priority lines (hearts tiers, seasons, weekdays, hours) are the ones never heard: about 54 to 62 of 150 per villager after two years, almost all gated by a state the bot is in only briefly.

## Why one-year coverage stops near 45%

Lines gated by season (about 24 per villager), weekday (about 14) and hour (about 12) are only eligible some of the time, and they share the ordinary pool with 60 or more others, so each is chosen on a small share of eligible days. The never-heard bias already favours them. A daily-visiting engaged player passes through the friend and close-friend tiers in under three weeks each, so most tier lines are never heard. A normal player (who talks less often) hears fewer lines but also passes through the tiers more slowly.

## Decisions needed from the owner

1. **Coverage target (decided 2026-10-03: recalibrate to 45% in year one and 60% in year two; the plan is updated).** Either keep 60% in one year and make the content denser (about 30 more ordinary lines per Full villager and fewer lines gated to one season, weekday or hour), or recalibrate to **45% in year one and 60% in year two** for a daily-visiting player, which the slice already meets. The recommendation is the second: coverage is a ceiling for the most devoted player, and rare lines are meant to be chased.
2. **Replay variety needs more storylines (decided 2026-10-03: write three more; the pool is 8 with `competing_band`, `missing_pumpkin` and `five_names_cat`).** With a pool of N and K drawn per game, two games can differ in at most min(K, N−K) storylines: 1 with 4 of 5. To reach 3, the pool needs **at least 7 storylines (4 per game)**; the plan's 10 gives up to 4. Recommendation: write three more storylines (the plan lists a competing band and a missing prize pumpkin as examples) before launch.
3. **Moment density.** Fix the length of a game day in real minutes (the Options day-length setting) and agree whether scenes and storyline beats count towards "one clippable moment per 10 minutes", then re-measure; the bot should be extended to play scenes, topics and social actions (not done).
4. **Dead air.** Decide whether "new" means a new line (strict, 11 days measured) or a new line, event or storyline beat (the plan's wording). Extending the bot to play events would answer it.

## What the bot cannot tell us (still needs people)

The "players attribute 80% of unlabelled lines to the right villager" blind test, whether the lines are funny, how the first hour feels, and whether pacing feels right. The slice gate's human review is done for the three villagers (approved by the owner on 2026-10-03); the outside playtesters and watched VODs in the gate are not.

## Regression guards in CI

`StorySimulationTests` fails if repetition ever appears, if two-back repeats reach 20%, if fewer than half of lines carry a condition, if year-one coverage drops below 40% or two-year coverage below 55%, or if dead air passes 14 days. These are set at the measured level, not at the plan's targets, so they catch regressions without hiding the gap above.
