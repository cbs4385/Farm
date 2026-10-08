# Creator and playtest programme (T-149)

Status: **plan and kit written; nothing run yet.** Recruiting playtesters, cutting the creator build and watching the VODs are human steps (see "Needs a person"). Everything the game itself must offer to creators already exists and is listed below with where to find it.

## Goal

The game is meant to be worth streaming. This programme finds out whether it is, with five to ten outside playtesters, at least two of them content creators, before the launch narrative gate (`NPC_DIALOGUE_PLAN.md` section 5, stage 4).

## What a creator can rely on (built and tested)

| Need | Where | Notes |
|---|---|---|
| Choices by number key, chat-friendly | dialogue box | every choice has a number key; tone tags are icons, never hidden penalties |
| Choice timer | Options | off by default; 15, 30 or 60 seconds; the default choice is taken at zero and choosing in time cancels it; no timer for a lone option |
| Stream mode | Options | dialogue text 20% larger and at least fast (the content badge in the HUD was removed 2026-10-08). The UI never shows save paths or account names (a guard test enforces it) |
| Name filter | new-game screen (player and farm names) | always on; the blocklist is only a seed list and **must be extended before release** |
| Content level | Options | horror off, mild or full (default full); the cozy game is complete at off. Say so on the store page and in the press kit |
| Photo mode | F8 in the world | hides the HUD, stops the clock, emotes on nearby villagers, saves a PNG to the data folder `Photos/` |
| Memories replay | Memories tab | every set-piece scene can be replayed on its own, which is the clip button |
| Village Gazette | Gazette tab | a weekly front page, built to be screenshotted |
| Neighbours page and Gossip Book | menu | what chat can go looking for next: stages, tastes learned, rare lines found |
| Developer tools | development builds only | the console (F1) and `-farmCommands` must never reach a release build; the build guard fails if they do |

## Creator preview build

1. Build from a tagged commit with `Farm.Editor.BuildScript.BuildWindows -scriptingBackend il2cpp` (see `BUILD.md`). Not a development build.
2. Check the release guard output, then the content badge, the Options rows above and a new game start to the first dialogue.
3. Version the build with the commit in the name. Creators get a Steam key or a zip; either way the game runs without Steam.
4. Include a one-page "what to try" note (below), not a walkthrough: creators should find things.

## Press kit

- Short pitch: cozy farming with a village that notices what you do; the horror layer is optional and tunable.
- **Creator-safe options** note: the table above, in three lines (stream mode, name filter, content level).
- Streamable scenes: the list below is generated from the story data (every scene tagged funny, wholesome or surprise). Pick ten for the kit and cut a trailer from them.
- Screens: Neighbours page, Gazette, Gossip Book, a Lantern Release capture, a photo-mode photo.
- No claims the game cannot back: no voice acting, English only at launch (ADR 0005).

## Playtest protocol

**Who.** Five to ten people. At least two content creators, at least two who have never seen the game, and at least one who does not usually play farming games. No staff.

**Sessions.** Two rounds. Round one: two hours, a fresh save, recorded (their own capture is fine). Round two, two weeks later: a continued save, one hour, to see the return visit.

**Brief them with:** "Play how you normally would. Say what you are thinking. Do not read the notes first."

**Give them:** the build, the one-page "what to try" (talk to everyone, give a gift, ask a topic, try a social action, enter a name, open the Neighbours page, take a photo), a short survey, and the contact for bugs.

**Survey (1 to 5 unless noted).** I wanted to talk to villagers every day. A villager surprised me. I told someone about a scene. I always knew what to do next. Chat/viewers would enjoy this (creators). Anything I would clip (free text). The moment I got bored or confused (free text).

## What reviewers look for in the VODs

Two reviewers watch each VOD at 1.5x and log, with timestamps:

1. **Dead air.** More than 30 seconds with nothing new to look at or hear. Compare with the bot's report (`Builds/story_simulation.md`: longest dead air).
2. **Confusion.** The player stops, asks "what do I do" or opens the wrong menu twice.
3. **Repetition.** A line heard twice in a session. The bot predicts none; a person finding one is a bug.
4. **Clip moments.** Laughter, an audible reaction, "oh no", "aww". Note which scene it was.
5. **Skipped text.** Dialogue the player skips; which villager, which line.
6. **Offence.** Anything a viewer might find rude or off-tone (feeds the sensitivity review).

Each finding becomes an issue labelled `playtest` with the timestamp, the scene id (the Memories title helps) and a severity. Fix criteria: any crash or soft lock, any confusion repeated by two testers, any line a reviewer flags as offensive. Dead-air stretches over 30 seconds become content tasks.

## Exit criteria (launch narrative gate)

- No soft lock or crash in any recorded session.
- At least half the testers say they told someone about a scene.
- Every confusion found by two or more testers is fixed or explained in-game.
- Three or more scenes are named as clip-worthy by testers independently.
- The sensitivity review (offence log) is clear.

## Needs a person

Extending the name-filter blocklist; recruiting and scheduling testers; sending keys; watching the VODs; running the survey; deciding which fixes ship before launch. Nothing here is automated, and none of it has happened.

## Appendix: streamable scenes (generated from the story data on 2026-10-04)

**funny** (40): A Slice for Odalys (`story_pie_payoff`); Battle of the Bands (`story_band_battle`); Elara's Birthday (`elara_birthday`); Felix's Birthday (`felix_birthday`); Fish Prices (`overheard_tilda_felix_1`); Gossip Bingo (`wren_friend`); Hold Out Your Hand (`elara_heart2`); Hold This Plank (`marcus_heart2`); Hold the Line (`felix_heart2`); Hold the Tongs (`juno_heart2`); Juno's Birthday (`juno_birthday`); Juno's Piece (`bram_heart8`); Labeled Tongs (`overheard_juno_bram_1`); Learn a Chord (`piper_heart2`); Open Mic (`wren_heart4`); Overdue (`hazel_friend`); Parsnip Ballad (`piper_friend`); Piper's Birthday (`piper_birthday`); Pulse Check (`odalys_heart2`); Request Line (`piper_heart4`); Sawdust Silence (`overheard_marcus_dorian_2`); Sound Check (`overheard_wren_piper_1`); Stocktake Saturday (`tilda_friend`); The Big One (`felix_heart8`); The Label Maker (`juno_heart4`); The Missing Pumpkin (`story_pumpkin_dorian`); The Missing Pumpkin (`story_pumpkin_felix`); The Missing Pumpkin (`story_pumpkin_marcus`); The New Recipe (`wren_heart2`); The Overdue Notice (`overheard_hazel_ione_2`); The Red Umbrella (`story_umbrella`); The Scarecrow Contest (`story_scarecrows`); The Swimming Pool (`juno_friend`); The Waiting Room (`overheard_elara_odalys_1`); The Whistler (`story_whistler_bram`); The Whistler (`story_whistler_felix`); The Whistler (`story_whistler_marcus`); Too Many Scones (`tilda_heart5`); Wren's Birthday (`wren_birthday`); You Again (`bram_heart2`)

**surprise** (11): After Hours (`tilda_heart6`); Behind the Shelves (`hazel_heart6`); One Long Sentence (`bram_heart6`); Stage Fright (`piper_heart6`); The Bag (`wren_heart8`); The Hall Roof (`marcus_heart8`); The Horseshoe (`juno_heart8`); The List (`elara_heart8`); The Recipe Card (`tilda_heart8`); The Secret Spot (`dorian_heart5`); Words from Bram (`bram_heart5`)

**wholesome** (79): A Question (`ione_heart5`); A Small Poem (`hazel_heart5`); A Song for You (`piper_heart5`); Bram's Birthday (`bram_birthday`); Cat With Five Names (`story_cat`); Closing Time (`overheard_wren_piper_3`); Come Look at This (`hazel_heart2`); Dorian Asks (`dorian_courtship`); Dorian Asks Again (`dorian_courtship2`); Dorian's Birthday (`dorian_birthday`); Dorian, Together (`dorian_partners`); Dr. Penn's Birthday (`odalys_birthday`); Elara Asks (`elara_courtship`); Elara Asks Again (`elara_courtship2`); Elara, Together (`elara_partners`); Felix Asks (`felix_courtship`); Felix Asks Again (`felix_courtship2`); Felix, Together (`felix_partners`); Finally Finished (`juno_heart5`); First Strike (`juno_heart6`); Hazel Asks (`hazel_courtship`); Hazel Asks Again (`hazel_courtship2`); Hazel's Birthday (`hazel_birthday`); Hazel, Together (`hazel_partners`); Her Mark (`juno_heart10`); Ione Asks (`ione_courtship`); Ione Asks Again (`ione_courtship2`); Ione's Birthday (`ione_birthday`); Ione, Together (`ione_partners`); Juno Asks (`juno_courtship`); Juno Asks Again (`juno_courtship2`); Juno, Together (`juno_partners`); Lamps in Every Room (`hall_ending`); Late Shift (`overheard_elara_odalys_3`); Marcus's Birthday (`marcus_birthday`); Mother's Gratin (`tilda_heart2`); Not Just the Noise (`wren_heart5`); Office Hours (`odalys_heart8`); Opening Night (`wren_heart10`); Piper Asks (`piper_courtship`); Piper Asks Again (`piper_courtship2`); Piper, Together (`piper_partners`); Quiet Hours (`overheard_hazel_ione_1`); Rehearsal (`hazel_heart8`); Shh, Look (`dorian_heart2`); Slate and Line (`overheard_tilda_felix_3`); Something Knitted (`elara_heart5`); Soup Rounds (`overheard_elara_odalys_2`); The Apprentice Bench (`overheard_juno_bram_3`); The Bench (`bram_friend`); The Boyhood Carving (`marcus_heart5`); The Chorus (`piper_heart8`); The Forest Door (`overheard_marcus_dorian_3`); The Kitchen (`wren_heart6`); The Lantern Release (`festival_winter_lanterns`); The Lucky Charm (`felix_heart5`); The Note Writer (`story_notes_dorian`); The Note Writer (`story_notes_juno`); The Note Writer (`story_notes_tilda`); The Notebook (`dorian_heart8`); The Oldest Friend (`overheard_tilda_felix_2`); The Reading (`hazel_heart10`); The Reading Chair (`overheard_hazel_ione_3`); The Remedy Notebook (`odalys_heart5`); The Repair (`bram_heart4`); The Second Label (`overheard_juno_bram_2`); The Set-Aside Shelf (`ione_heart8`); The Shared Verse (`overheard_wren_piper_2`); The Sign (`tilda_heart10`); The Slate (`tilda_heart4`); The Song They Know (`piper_heart10`); The Straight Plank (`overheard_marcus_dorian_1`); The Swap (`hazel_heart4`); Tilda's Birthday (`tilda_birthday`); What Ione Found (`ione_heart2`); Wren Asks (`wren_courtship`); Wren Asks Again (`wren_courtship2`); Wren, Together (`wren_partners`); Your Tool (`bram_heart10`)
