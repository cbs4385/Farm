# QA guide: testing the villager heart events (Wren, Hazel, Bram)

Audience: a QA tester. Scope: the 15 scenes added in Phase 2 (four heart events, at hearts 4, 6, 8 and 10, and one friend-day scene for each of Wren, Hazel and Bram). Status of the content: written by an agent, **not yet read or played by a person**, so your notes are the review.

**Ready-made command files:** `docs/qa/heart_events/` has one text file per scenario (start with `00_READ_ME_FIRST.txt`): copy a line at a time into the F1 console.

## 1. What you need

- A **development build** (it has the developer console). Release builds do not. Build it, or ask the developer for `BuildsDev/`:
  `"$UNITY" -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildWindows -development -logFile Builds/build-dev.log` (see `docs/BUILD.md`). The console also works in the Unity Editor.
- Game language English; Content level (Options) any. The scenes are cozy; at Off the game must behave exactly the same.
- Close the Unity Editor before running a headless build; only one Unity instance may open the project.

## 2. The console

Press **F1** or the **backtick** key to open or close it (it will not open on top of another screen). Enter runs a command; Up and Down recall earlier ones.

| Command | Use here |
|---|---|
| `hearts <npc> <0-10>` | set a villager's hearts (`hearts wren 4`) |
| `flag <id> on` | set a story flag (used to satisfy the chain) |
| `choice <flag>` | set a choice flag as if you had picked it (for callbacks) |
| `tp <MapId>` | go to a map: `Saloon`, `Library`, `Blacksmith` |
| `time 14:00`, `date summer 15`, `day [N]` | set the clock and date; sleeping through days |
| `state` | print the date, weekday, weather, map: use it to check the day |
| `scene <eventId>` | **force** a scene to play when the map is idle (ignores its conditions) |
| `memory <eventId>` | unlock and replay a scene as a memory without spending it |
| `say <dialogueId>` | play any dialogue (for the choice dialogues alone) |
| `save` | write the active save slot |

## 3. Two ways to test, and when to use each

**A. Natural trigger (tests the conditions, the chain and the schedule).** The scene starts when you walk into its map and every condition holds. Use this for at least one full run per villager.

**B. Forced (tests the scene itself, quickly).** `scene <id>` plays it regardless of hearts, flags, weekday or hour. Use this to check text, staging, choices and callbacks, and to repeat a scene (see section 6).

Always test **A first for each villager on a fresh save**, then use **B** for the rest.

## 4. Setting up route A (natural trigger)

1. Start a **new game** (or load a copy of a fresh save: scenes are one-shot, so keep a spare).
2. Open the console. Satisfy the chain for the scene you want. Example, Wren heart 4:
   ```
   hearts wren 4
   flag event.wren_heart2 on
   time 14:00
   tp Saloon
   ```
   Do not edit scenes, only flags. Use `state` to read the weekday: Wren's scenes do not play on **Tuesday** (Saloon closed), Hazel's not on **Saturday**, Bram's not on **Monday**. Use `day 1` until the weekday is right.
3. Walk into the map (step out and back in if you are already there). The scene begins after the map is idle.
4. After it ends, check the effects (section 7), then set up the next one.

### Conditions for each scene

Each scene also needs the villager's base window: **Wren** 12:00 to 23:59, not Tuesday; **Hazel** 09:00 to 16:59, not Saturday; **Bram** 09:00 to 16:59, not Monday.

| Scene id | Memory title | Map and your cell | Needs (besides the window) | Template | Choices (flag set) | Friendship |
|---|---|---|---|---|---|---|
| `wren_heart4` | Open Mic | Saloon (9,6) | hearts 4, `event.wren_heart2` | prank | cheer, heckle, hide (`choice.wren.openmic.*`) | +50 (+10 per cheer or heckle) |
| `wren_heart6` | The Kitchen | Saloon (9,6) | hearts 6, `event.wren_heart5`, `event.wren_heart4` | conversation | pepper, cider, plain (`choice.wren.kitchen.*`) | +60 (+10) |
| `wren_heart8` | The Bag | Saloon (9,6) | hearts 8, `event.wren_heart6` | confession | go, stay, listen (`choice.wren.bag.*`) | +70 (+10 or +15) |
| `wren_heart10` | Opening Night | Saloon (9,6) | hearts 10, `event.wren_heart8` | performance | none; the closing line depends on the `bag` choice | +80 |
| `wren_friend` | Gossip Bingo | Saloon (9,6) | hearts 3, **Friday** | shared activity | none | +15 |
| `hazel_heart4` | The Swap | Library (4,4) | hearts 4, `event.hazel_heart2` | conversation | book, recipe, honest (`choice.hazel.swap.*`) | +50 (+10) |
| `hazel_heart6` | Behind the Shelves | Library (4,4) | hearts 6, `event.hazel_heart5`, `event.hazel_heart4` | confession | ask, wait, joke (`choice.hazel.hiding.*`) | +60 (+10, +15, +5) |
| `hazel_heart8` | Rehearsal | Library (4,4) | hearts 8, `event.hazel_heart6` | conversation | praise, critique, laugh (`choice.hazel.rehearsal.*`) | +70 (+10, +15) |
| `hazel_heart10` | The Reading | Library (4,4) | hearts 10, `event.hazel_heart8` | performance | none; the closing line depends on the `rehearsal` choice | +80 |
| `hazel_friend` | Overdue | Library (4,4) | hearts 3; **blackberries** in the pack for the success branch | helping scene | none | +15 on success |
| `bram_heart4` | The Repair | Blacksmith (6,3) | hearts 4, `event.bram_heart2` | conversation | hold, hammer, watch (`choice.bram.repair.*`) | +50 (+10 or +15) |
| `bram_heart6` | One Long Sentence | Blacksmith (6,3) | hearts 6, `event.bram_heart5`, `event.bram_heart4` | confession | nod, glad, word (`choice.bram.long.*`) | +60 (+10, +15) |
| `bram_heart8` | Juno's Piece | Blacksmith (6,3) | hearts 8, `event.bram_heart6` | conversation | praise, tease, ask (`choice.bram.juno.*`) | +70 (+10, +15) |
| `bram_heart10` | Your Tool | Blacksmith (6,3) | hearts 10, `event.bram_heart8` | heirloom (a gold bar) | none; the thanks line depends on the `juno` choice | +80 and one gold bar |
| `bram_friend` | The Bench | Blacksmith (6,3) | hearts 3, **Wednesday** | shared activity | none | +15 |

Chain flags to satisfy by hand when you skip ahead: `event.<npc>_heart2` and `event.<npc>_heart5` (older scenes), then the new ones in order. Blackberries: `give forage.blackberry 1`.

## 5. Setting up route B (forced)

```
tp Saloon
scene wren_heart4
```
Wait for the map to be idle (no menu open, player not moving). To test a callback (the closing lines that depend on an earlier choice) set the choice first, then force the scene:
```
choice wren.bag.go          (then: scene wren_heart10 -> closing line about the map)
choice wren.bag.stay        (closing line about staying)
(neither)                   (closing line about the quiet part)
choice hazel.rehearsal.critique   (scene hazel_heart10 -> "I cut the middle")
choice bram.juno.ask        (scene bram_heart10 -> "You said you'd see me")
```
Test every branch of every callback: three for Wren's, two for Hazel's and Bram's.

## 6. Repeating a scene

Scenes are **one-shot**: after a natural trigger it will not play again on that save. To repeat:
- Replay it from the **Memories** tab (one of the collection tabs) (no setup, no rewards), or `memory <eventId>`.
- Or reload a save made just before it (`save` before you start, and keep copies).
- `scene <id>` forces the scene even when it has been seen; its rewards apply again, so use a throwaway save for reward checks.

## 7. What to check in every scene

Staging and flow
- The player walks to the marked cell and faces the villager; no one walks through a wall or stands on the same cell.
- Cinematic bars, dim or warm light, camera focus and emotes appear and **go away** at the end; the camera returns to the player.
- The dialogue box never overflows (also try UI scale and **stream mode**: text 20% larger; and the text speed options).
- Skip with **Escape**: the scene ends cleanly, the rewards you were owed are still given, and no bars or light remain.
- The scene plays once; leaving and re-entering the map does not restart it.
- Quit and reload mid-day after a scene: it is still spent and its flags are set.

Text and choices
- Choices can be picked with the mouse, keyboard (arrows and Enter) and **number keys 1 to 3**; the choice reply matches the choice.
- With the **choice timer** on (Options), letting it expire picks the highlighted default and the scene continues.
- Callbacks: the closing line matches the earlier choice (section 5). A line that shows the wrong branch, or both, is a bug.
- Voice rules: **Hazel and Bram never use an exclamation mark**; Bram's lines are short and often open with "Hm."; Hazel's asides are in parentheses; Wren is warm and talkative. Note any line that sounds like someone else.
- American spelling; no placeholder text, no raw `{if}` markup, no `[player]` left unreplaced; nothing that shows file paths or account names.
- Voice blips: each villager has their own blip. Turn **Voice blips** off in Options: silent typing.

Effects
- After the scene, run `state` or open the villager's page: friendship rose by the amount in the table (a choice may add a little).
- Bram heart 10: one gold bar is in the pack. Hazel's Overdue: with blackberries the scene succeeds and is spent; without them she asks you to come back and the scene is **offered again** later (it is not spent).
- The scene appears under **Memories** with the title in the table (titles are at most 22 characters and must fit their button).
- Mail: a birthday reminder letter arrives one or two days before the villager's birthday (hearts 3 or more); a thank-you letter arrives after the heart-4 scene once hearts reach 5.

Coverage of the pacing
- On a normal run (talk daily, a couple of gifts a week), heart 4 should arrive within about a month and the steps from 4 to 10 should be at least a week apart (`docs/narrative/PACING.md`). Report if a scene feels rushed, late or bunched.

## 8. Reporting

For each problem, give: scene id, route (A or B), the choice taken, steps, expected versus actual, a screenshot or short video, and the build version. Note whether the scene made you smile (funny), warmed you (wholesome) or surprised you: that is what these scenes are for, and the writers need to hear it.

Known gaps (not bugs): expression portraits are not drawn yet (the emotes carry the feeling); other villagers' heart 4 to 10 scenes do not exist yet; only two of the five storylines have talk lines.
