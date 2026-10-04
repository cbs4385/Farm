QA: heart-event console files (Wren, Hazel, Bram)

HOW TO USE
1. Use a DEVELOPMENT build (or the Unity Editor) with a game running: new game, or a save you can throw away.
2. Press F1 (or the backtick key) to open the console.
3. Open a file from this folder in Notepad. Copy ONE LINE, paste it into the console, press Enter. Repeat for each line.
   (The console runs one command per Enter. Do not paste several lines at once unless you have checked that your build accepts it.)
4. Close the console (F1) and walk/wait: the scene starts when the map is idle.
5. Run "state" in the console to check date, weekday, map.

FILES
forced_<scene>.txt                  Plays one scene now, ignoring its conditions. Fast check of text, staging, choices. Rewards apply each time: use a throwaway save.
forced_<scene>_branch_*.txt         The closing scenes (heart 10) change their last line by an earlier choice. One file per branch: run each.
forced_hazel_friend_*.txt           Hazel "Overdue": with blackberries (succeeds, is spent) and without (asks you to return, offered again).
dialogue_only_all_choice_dialogues  Plays the nine choice dialogues alone, one after another: paste a line, finish the dialogue, paste the next.
natural_<npc>_chain_1_setup.txt     Real triggers, the whole chain. Sets hearts to 10 and goes to the map. Run on a FRESH game.
natural_<npc>_chain_2_next_scene.txt  After each scene ends, run this to leave and re-enter the map. Repeat until no scene plays.
                                    Expected order, Wren: heart2, heart4, heart5, heart6, heart8, heart10 (6 visits).
                                    Hazel: heart2, heart4, heart5, heart6, heart8, heart10. Bram: heart2, heart4, heart5, heart6, heart8, heart10.
                                    (If a scene plays out of order, or the chain stalls, that is a bug: report the scene ids and order.)
natural_<npc>_friend_day.txt        Real trigger for the friend-day scene (hearts 3; the date is a Friday for Wren, a Wednesday for Bram, day 5 for Hazel).

NOTES
- Paste "tp ..." first, then WAIT until the new map has fully loaded (fade finished) before pasting "scene ...". If scene is sent while the old map is still up, the scene plays on the wrong map (seen in a dev-build check on 2026-10-03).
- "date" only moves forward. On a game already past spring 5, use the "date" line with the next season (the weekday is the same: day 5 is a Friday, day 3 a Wednesday in every season) or start a new game.
- Weekday = (day - 1) mod 7: day 1 Monday, 2 Tuesday, 3 Wednesday, 4 Thursday, 5 Friday, 6 Saturday, 7 Sunday.
- Scenes are one-shot. To see one again: the Memories tab, or reload a save made before it.
- If the first command says the map is unknown, run "help" and report the exact map name.
- Full checklist of what to look for in each scene: docs/narrative/QA_HEART_EVENTS.md
