QA: storyline scene console files (docs/narrative/storylines/ for the writing; docs/narrative/QA_HEART_EVENTS.md for the general checklist)

Use a DEVELOPMENT build or the Unity Editor with a game running (a throwaway game). F1 opens the console. Paste ONE LINE at a time and press Enter.

FILES
forced_story_*.txt     Plays one scene now on the village crossing (ignores conditions). Paste "tp Village" first and WAIT until the map is loaded before "scene".
                        Take every choice: three for most, four in the scarecrow contest. The closing line changes with the choice.
natural_<storyline>.txt  Sets up the conditions for the real trigger (storyline flag, variant flag, date, time, weather) and walks into the village.
                        The scene starts on its own when the map is idle. Notes: Dorian, Juno and Tilda are three possible writers (one per game);
                        the whistler is Bram, Marcus or Felix (one per game). Run each variant file on a fresh game.
lines_sample_talk_after_scene.txt  Marks every storyline finished so you can talk to villagers and hear the after-the-scene lines.

NOTES
- A new game draws 4 of the 5 storylines. The console "storyline <id> on" adds one; the variant flag chooses who it is.
- Scenes are one-shot; replay from the Memories tab or reload a save made before.
- "date" only moves forward: day 5 is a Friday, day 6 a Saturday in every season (the contest is a fall Saturday).
- The scenes avoid festival days; if nothing starts, check "state" for a festival today.
- Check: the villager appears at the crossing, the choices and their replies, the closing line matches the choice, Bram has no exclamation marks, rewards (the contest gives a scarecrow), the Memories entry.
