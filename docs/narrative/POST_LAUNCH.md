# Post-launch cadence (T-150)

Status: **roadmap written; no update built.** Dates are not set: they depend on the launch date and on the T-149 playtest findings.

## What ships at 1.0 and what the updates add

At launch six villagers are Full (Tilda, Wren, Bram, Juno, Hazel, Piper) and six are Enhanced (Marcus, Dr. Penn, Felix, Dorian, Elara, Ione). The Enhanced six have, as built on 2026-10-04: about 72 talk lines each (a Full villager has about 116), a heart-8 scene with choices, a birthday scene, a quest, social profiles, barks, the festival lines, overheard scenes with their pairing, and (for Felix, Dorian, Elara, Ione) courtship. They do **not** yet have the heart-4, heart-6 and heart-10 scenes, a friend-day scene, or a villager storyline of their own. That gap is exactly what the six Neighbour Spotlight updates fill.

## The six Neighbour Spotlight updates

Order is by how much a creator's audience will want it and how much the village already points at them. Each update is **one villager brought to Full**, announced with a trailer cut from its new set pieces.

| # | Villager | Why this order | The update adds |
|---|---|---|---|
| 1 | Elara | the loudest, easiest to clip, and the pairing with Dr. Penn is already a fan favourite | heart-4, 6, 10 scenes; a friend-day scene; about 44 new talk lines; a quest chain; two overheard scenes; one storyline she leads (the scarf that took over the clinic) |
| 2 | Felix | the fishing contest is a festival anchor | the same set; a contest-night scene; a storyline (the fish that is always "this big") |
| 3 | Ione | the library pair with Hazel; a quiet counterweight | the same set; a reading-night scene; a storyline (the book that falls off the shelf) |
| 4 | Marcus | the carpenter; ties into every building upgrade | the same set; a build-day scene tied to a build upgrade (needs building state exposed through `IGameQuery`, left over from T-124); a storyline (Gerald the plank) |
| 5 | Dr. Penn | the clinic; pairs with Elara | the same set; a long-shift scene; a storyline (the clinic's missing hour of sleep) |
| 6 | Dorian | the quietest; a payoff for players who stayed | the same set; a forest-walk scene; a storyline (the owl and the understanding) |

"The same set" means, per villager, the Full checklist: lines to about 116, three more scenes (heart 4, 6, 10), a friend-day scene, a quest chain, a storyline with a seed-drawn variant, rare and legendary lines to four, and a human read before it ships.

## Seasonal storyline drops

Between Spotlights, one storyline drop each season: three to four new storylines for the pool, which is 8 at launch (the plan's long-term pool is 10). A storyline is drawn by seed per game, so a new drop widens replay without touching any save: new storyline ids only, no change to existing ids. Candidates already named in the plan: the library ghost that is only a draft, the weather-forecast wager, the village's rival tea blends, the beach treasure map, the lost-and-found of the year.

## Rules every update keeps

1. **Save-compatible.** Stable string ids never change; new content goes in new ids, flags and vars. No new `GameState` fields for story.
2. **Cozy first.** At horror level off the game is complete and identical in tone. Update content is cozy.
3. **Gate per update.** The checklist (lint clean and locked, the moment checklist, reachability), the story simulation run (repetition 0, reactivity at or above 70%, year-one coverage at or above 45%), a PlayMode walkthrough of every new scene with every choice, and a human read and play before release.
4. **Order of work in an update:** bible entry first, approved; then the compiled script; then data; then the walkthrough and captures; then the trailer cut from the scenes tagged for clipping.
5. **No new engine work** unless the villager needs it (Marcus's build state is the only known case).

## Announcement

Each update has a short patch note listing the villager, the new scenes (by their Memories titles), and anything in the options. The trailer is cut from the Memories replays of the new scenes (every set piece can be replayed alone, which is the capture route).

## Needs a person

Setting dates; writing the six bibles for approval; the human read and play of every update; cutting the trailers; the Steam release steps (`RELEASE.md`).
