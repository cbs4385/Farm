# The scene pass, Round 2: the coherence read (cozy layer)

Round 1 is applied. Every villager rewrote their own lines in isolation; now you read the scenes as the player will, with everyone's new lines in place. The cards in `drafts/scene_pass/cards/<you>.md` were regenerated from the repo, so they show the assembled text, added lines included (they have keys like `event.<scene>.<n>`; you may now edit those too).

## What to do

1. **Read your scenes front to back, as the player.** Where another villager's new line collides with yours (they answer a question you no longer ask, repeat your joke, contradict a number or a day, steal your last line's thread), fix **your** side so it works. You may only change lines marked ✏.
2. **Read `drafts/scene_pass/r1_notes.md`.** Act on every note that names you or your lines. Where a note asks another villager to change something, do not edit it; list it in `notes` for the Director.
3. **Check your threads.** Each scene's last line should leave the player wanting one more thing. If two of your scenes end on the same shape (same kind of question, same exit line), vary one. If a thread points at something that does not exist in the game, repoint it at a thread from the brief.
4. **Callbacks across scenes.** If a later scene of yours should remember an earlier one (the first ask, the gift, the argument), say so in a line. Use the card's flags/conditions: only reference what the player is certain to have seen.
5. **Variety pass.** No two of your lines in different scenes should open with the same word or use the same sentence shape twice in a row. Read your "tic" lines against your persona file: a tic should be rare enough to be a signature, not wallpaper.
6. Fix only what is wrong. If a Round 1 line is good, leave it. Fewer, better changes beat a second full rewrite.

Specific items already known (the Director handles the ones that name no one):
- Hall Dance (`hall_dance`): "six winters" is canon; check your lines in it for the number and for the staging rule (no two people dancing together).
- Juno's `story_notes_juno` says Bram keeps six of her notes in his apron and thinks Tilda wrote them. Bram: accept it, rework it, or veto it with a reason (in `notes`).
- Bram's heart 10 name moment and his long sentence: you may use `[player]` in `event.bram_heart10.thanks` now (the lint exemption is made); `dlg.bram.long.0` may run to 18 words in one sentence.
- Piper's heart 4 plants "writes down the request"; the Director will honor it in heart 10, so Piper leave heart 4 as it is.
- Wren: "seven" for Tilda at the saloon (overheard_tilda_wren_3). Wren: change "six" to "seven" there.
- Heart-10 scenes that give an object (Tilda's bell, Odalys' notebook): do not promise an item the scene does not give.

## Same mechanics as Round 1
Everything in `canon/scene_pass.md` still binds: budgets (40 / 9 / 20 words), voice limits, American English, ids/keys/order/effects untouched, `{if}` branches and tokens kept, cozy only. At most **2** added lines per scene in editable scenes, only to fix a gap you found.

## What to hand back
Same JSON shape as Round 1 (`lines`, `adds`, `cuts`, `threads`, `notes`), written to `drafts/scene_pass/r2/<you>.json`. `lines` holds only keys you changed in Round 2 (with the text as it should now read). Run `python3 tools/writers_room/scenepass/lint_lines.py <file>`; fix every ERROR. After every ~10 scenes: save the file and send the Director one line with SendMessage to "main": "<you>: N/total scenes, L lines changed, E lint errors". Final reply at most 120 words.
