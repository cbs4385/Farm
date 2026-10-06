# Mythos balance (X-010)

Bots play the horror layer for up to six years through the real session, hooks and ritual director (`MythosBalanceTests`, EditMode, a few seconds). The test writes `Builds/balance/mythos.md`; paste the table below when the numbers change. Columns: rituals that succeeded / failed / were disrupted by the bot, the highest wakefulness, when the god woke (day of the run; day 29 is the first summer day, day 113 the first spring of year 2) and when the bot sealed it.

## Bots
- **Ignorer**: never touches the layer, owns nothing the Keepers could mark.
- **Saboteur**: takes the first offering from the altar at every ritual it may interfere with (from the first summer).
- **Saboteur who seals**: the same, but completes the true fix N days after the woods open (the sealing needs the clues, the three relics and the altar; the bot just seals on that day).
- **Repentant saboteur**: disrupts two rituals, then stops.
- **Farmer**: grows crops, harvests and ships them every day (or every third day) and never reads the journal; never goes near the altar.

## Results
| Bot | Intensity | Seed | Days | Rituals | Peak | Awake | Sealed | End |
|---|---|---|---|---|---|---|---|---|
| farmer: harvests and ships every day | full | 1 | 560 | 17 / 3 / 0 | 50% | never | - | 22.3% |
| farmer: harvests and ships every day | full | 2 | 560 | 16 / 4 / 0 | 50% | never | - | 37.8% |
| farmer: harvests and ships every 3 days | full | 1 | 560 | 14 / 6 / 0 | 71.1% | never | - | 71.1% |
| farmer: harvests and ships every 3 days | full | 2 | 560 | 17 / 3 / 0 | 50% | never | - | 22.3% |
| saboteur for 2 rituals, then stops | full | 1 | 560 | 18 / 2 / 2 | 75% | never | - | 22.3% |
| saboteur for 2 rituals, then stops | full | 2 | 560 | 18 / 2 / 2 | 75% | never | - | 22.3% |
| saboteur, every ritual (mild) | mild | 1 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur, every ritual (full) | full | 1 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur, every ritual (mild) | mild | 2 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur, every ritual (full) | full | 2 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| ignorer, 6 years | mild | 1 | 672 | 24 / 0 / 0 | 25% | never | - | 22.3% |
| ignorer, 6 years | mild | 2 | 672 | 24 / 0 / 0 | 25% | never | - | 22.3% |
| ignorer, 6 years | full | 1 | 672 | 24 / 0 / 0 | 25% | never | - | 22.3% |
| ignorer, 6 years | full | 2 | 672 | 24 / 0 / 0 | 25% | never | - | 22.3% |
| saboteur, every ritual | full | 1 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur, every ritual | full | 2 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur who seals 14 days after the woods open | full | 1 | 560 | 1 / 1 / 1 | 34.8% | never | day 42 at 34.8% | 0% |
| saboteur who seals 14 days after the woods open | full | 2 | 560 | 1 / 1 / 1 | 34.8% | never | day 42 at 34.8% | 0% |
| saboteur who seals 60 days after the woods open | full | 1 | 560 | 1 / 3 / 3 | 75.9% | never | day 88 at 75.9% | 0% |
| saboteur who seals 60 days after the woods open | full | 2 | 560 | 1 / 3 / 3 | 75.9% | never | day 88 at 75.9% | 0% |
| saboteur who seals 90 days after the woods open | full | 1 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |
| saboteur who seals 90 days after the woods open | full | 2 | 115 | 1 / 3 / 3 | 100% | day 115 | - | 100% |

## What the numbers say
1. **Ignoring the layer is safe.** The god never wakes in six years; wakefulness is a sawtooth that peaks at 25% (a season of rising) and is reset by every successful ritual (30-40% off).
2. **Fighting the Keepers has a window of about twelve weeks.** A saboteur who disrupts every ritual from the first summer wakes the god on day 115, 83 days after the first disruption (three failed rituals, 22% / 47% / 72% at the rituals, then 97% before the fourth). Nobody can wake the god before the second spring.
3. **The true fix is in time for a normal investigator.** Sealing 14 days after the woods open (day 43) happens at 35% wakefulness; 60 days after (day 89) at 76%, which is late but still safe; 90 days after (day 119) is too late. The clue chain and the relics have not been timed by a person: the bots assume the fix can be completed at that day.
4. **Wakefulness is recoverable.** A saboteur who stops after two disruptions peaks at 75%, and the next successful rituals bring it back down (22% after the season ends); stopping is enough, nothing else is needed.
5. **Mild and full follow the same rules**; only the presentation differs.

## Finding: an unaware farmer fails rituals by accident
The Keepers mark some of the player's own things as offerings (a quarter of the slots: standing mature crops, items in chests and machines, animals at full intensity) and the ritual fails when a marked thing is gone before its turn. The farmer bot does nothing against the Keepers, yet 3 to 6 of its 20 rituals fail (15-30%), the peak reaches 50-71%, and in one world it ends the five years at 71%. Nobody woke the god in these worlds, but three failures in a row (about 8% to 15% of worlds at these rates over five years) would wake it, for a player who never knew. The journal only shows what is marked once the player has 4 lore, so most players cannot know.

Options for the owner (not implemented; the decision changes how the layer feels):
- **A. Substitute (suggested).** When a marked thing is gone because the player used it, the Keepers lay the village's own offering instead; the ritual only fails when the player takes an offering from the altar, makes a Keeper unavailable, or removes a thing *after* learning of it (lore 4+). Unaware players never fail a ritual; deliberate interference is unchanged.
- **B. Aware players only.** Mark the player's things only once the player has 4 lore (the journal shows them); before that only the village's offerings are chosen.
- **C. Forgive the first.** The first accidental failure of each year is forgiven (the ritual runs with a substitute); later ones count.
- **D. Keep it.** Leave it: an unaware player risks the world ending, which is harsh but is what "using a marked thing" means in the design (GDD answer N).

## Not covered by the bots
Dread and the night events, the Keepers falling sick (the `sick` and `away` effects are another way to disrupt a ritual), the time the clue chain takes (Hazel's clues need lore 1, 2 and 5 on three different days), what a human does when the woods first open, and a chest or animal based farmer (only standing crops were simulated). A person still has to play a year at each intensity.
