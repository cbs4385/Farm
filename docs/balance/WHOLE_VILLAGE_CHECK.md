# Whole-village balance check (2026-10-04)

The story simulation bot (`StorySimulation`, T-138) was run over all twelve villagers for two in-game years of daily visits, with topics and scenes (seed 12345; the report is written to `Builds/story_simulation.md` by `StorySimulationTests`). This note records what it found and what was changed. The bot is a model, not a person: it talks to everyone every day, raises hearts quickly (about ten hearts in a few weeks) and never skips a day.

## Targets (plan sections 2.2 and 2.3) and where the village stands

| Target | Result | |
|---|---|---|
| No ordinary line repeats within 14 days at 3+ hearts | 0 repeats for every villager | met |
| Two-back repeats | 0% | met |
| Year-one coverage, Full six: 45% | Wren 47, Hazel 48, Bram **43**, Tilda 45, Juno 47, Piper 49 | met except Bram (2 points short) |
| Year-one coverage, Enhanced six | 65 to 72% | met (they have fewer lines, so each is met sooner) |
| Longest stretch with nothing new from any villager (whole village): at most 8 days | 7 days | met |
| Rare and legendary lines are findable | every rare and legendary line heard within two years of daily visits | met after the change below (before: **none** of the 48 was heard) |
| Topics and scenes reachable | every villager's topics asked; map scenes 6 to 11 of 7 to 13 played | the rest need a choice, a drawn storyline or a window the bot misses (see STATUS) |

## What the check found and what was changed

**1. Rare lines were effectively unfindable.** Over two years of daily visits the bot heard none of the 48 rare and legendary lines, for two reasons: a rare line weighed only 8% of a common one in a pool of about a hundred, and rare lines sit at priority 0, so they only compete on visits where no mood, weather or window line is eligible (about a third of visits). The Gossip Book would have stayed nearly empty. Changed (`DialogueSet.PickVaried`): rarity is now a chance per visit, not a weight: 1.5% for a legendary line and 5% for a rare one, when one is eligible and fresh and no reaction, window or scene (priority 3 and up) is waiting; lines never said are preferred. A daily visitor now hears each rare line in a few months and a legendary line now and then. `StorySimulationTests.RareLines_AreFoundByADailyVisitor_WithinTwoYears` guards it.

**2. Preference for lines never said raised from 3 to 5.** Whole-run coverage went up for everyone (Full six from 56 to 62% to 59 to 68%; Enhanced six from 75 to 81% to 79 to 85%) with no repeats. Year-one coverage rose about one to four points.

**3. Left alone, and why:**
- *Bram, year-one coverage 43% against 45%.* He is the terse villager with the most conditioned lines (windows, seasons), and the bot passes through his friend and close-friend tiers quickly. More lines would lower coverage, not raise it. A person playing at a human pace will see more of them. Re-check after the playtests.
- *Friend-tier and close-friend-tier lines unheard.* The bot reaches ten hearts in weeks, so it leaves those tiers fast. A real player spends months there.
- *Enhanced six, long stretches without a new line from one villager (up to 34 days).* They have about 110 lines against 170 for the Full six. This is the gap the Neighbour Spotlight updates close (`narrative/POST_LAUNCH.md`).

## Re-run

`StorySimulationTests` runs it on every EditMode run. After a content wave, read `Builds/story_simulation.md` (summary table, never-heard lists) and compare with this note.
