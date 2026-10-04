# Streaming features

Status: built (T-145), with the dialogue debugger (T-137) and moment framework (T-140) that support streamers and writers. The goal is a game that is good to watch: readable at a glance, safe to stream, and open to a chat that takes part.

## Stream mode (Options > Accessibility)

A setting, off by default, saved with the other settings.

- **Dialogue text is 20% larger and no slower than "fast"** (a player who chose instant keeps instant). Menus keep the player's own UI size; stream mode never changes the saved choices.
- **A content badge** ("Content: Full / Mild / Off") sits in the top-left corner while stream mode is on, so viewers always know which level the game runs at. It follows the Content setting live.
- Everything else a streamer needs is on by default for everyone: number keys 1-9 pick a choice, the conversation log (L) shows what was said, and the dialogue box fits the screen at every UI size.

## The game never shows paths or account names

The UI shows no file path, no platform account name, no user name and no device name, with or without stream mode. `StreamModeTests.TheUi_NeverShowsFilePathsOrAccountNames` scans the UI code and the English text for them and fails the build if one appears. Names the player types (player, farm) are the only personal text on screen.

## Choice timer for a chat that votes (Options > Accessibility)

Off by default. Choose 15, 30 or 60 seconds. While a choice with two or more options is on screen a countdown ("Chat vote: 24s") runs; when it reaches zero the **default choice is taken** (the one marked `default`, such as Goodbye in the chat menu, otherwise the first). Choosing in time cancels it. A single option never has a timer. The game clock is already paused during conversations, so nothing happens while chat decides. There is no Twitch or YouTube integration: a streamer reads the options aloud or shows them, chat votes in their own tool, and the streamer presses the key.

## Names are filtered

Player and farm names (and any future pet name) are checked by `NameFilter` on the new-game screen ("Please try a different name."); `GameSession.BeginNewGame` also replaces a blocked name with the default as a second line of defence. The filter is always on, not only in stream mode, because the game is all-ages.

- It ignores case, digits and symbols used for letters (`sh1t`, `a55`), and letters spread out (`f u c k`, `f.u.c.k`).
- It blocks whole words, plus a few long stems no ordinary word contains. It is careful not to block innocent names (Scunthorpe, Assassin, Dickens, Sussex, Shiitake Farm); the tests keep that honest.
- **Limit:** the list in `Resources/Filters/blocked_names.txt` is a seed of common profanity and extremist terms. **Extend it with a vetted list before release**, including slurs, which are deliberately not written out in this repository. One word per line blocks that word; `~stem` blocks the stem anywhere (only for stems that no real word or place name contains: a stem `cunt` would block the town of Scunthorpe).

## For the streamer's checklist

- Stream mode on, a timer if chat votes, Content level chosen deliberately (the badge shows it).
- The dialogue debugger and memories (below) are for prepping a stream: `hearts`, `mood`, `scene` and `memory` set up and replay moments in a development build.

## Dialogue debugger (development builds only)

Open the console with F1 or the backquote key. Commands: `hearts <npc> <0-10>`, `mood [npc] [state|clear]`, `storyline <id>`, `choice <flag>`, `heard <npc>` (lines said, newest first), `pool <npc>` (every talk line: ready, waiting, or why not, and what would be said), `pick <npc> [seed]` (a dry run that records nothing), `say <dialogueId>`, `scene <eventId>`, `memory <eventId>`, `reactions`, `fire <trigger>`, `topics <npc>`, `social <npc>` and `coverage <npc>`. They are compiled out of release builds; the release guard fails a build that contains them.

## Moments

Content is tagged `funny`, `wholesome`, `surprise` or `mystery` on talk lines, dialogue nodes and whole scenes. `Farm > Narrative Report` counts them, checks each villager's quota and the target of 25 tagged set pieces, and checks every tagged scene against a clip checklist: it has a title (so it can be replayed), runs under 150 seconds, has at least one visible or audible beat (an emote, gesture, camera move, light, sound or prop), and ends on a spoken line. Checklist problems are warnings until a villager is locked, then errors.
