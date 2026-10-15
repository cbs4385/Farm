# Villager images: portrait versus world sprite (audit 2026-10-15)

Playtester report: "the image looks like a guy with short black hair, but the person looks like a girl with reddish pig tails" (Felix Hartwell, the Beach fish stall). The figure at the stall in the screenshot is Felix himself (the yellow selection box is on him; the farmer is the second figure beside him), so the tester did not confuse the two: **Felix's front-facing world sprite really was a different drawing** (a chibi with reddish hair tied in two tails and a rounder face) from his portrait and from his side views.

Cause: `npc_felix_idle_down` and `npc_felix_idle_up` came from the first art pass (commit f4f9f9c); when the side views were redrawn (commit ce37a79) from `Art/Generated/chr_felix.png`, only left and right were replaced, so the front and back stayed the old drawing. The ids and the code were right (`ContentGenerator` wires `npc_<id>_idle_*` and `ui_portrait_<id>` from the id; no villager uses `npc_generic_*`, which is only the placeholder for tests).

Fix: `python tools/art/redo_character.py felix --slice-only` (reuses the already generated sheet, no new image was requested) rewrote all four Felix idle sprites from that one sheet, so front, back, left and right are one drawing: short dark hair, stubble, wide straw hat, blue fisherman's jacket, teal scarf, boots.

## Table

"Match" compares the base portrait, the four idle facings and the four poses by eye at 6x and by the id wiring. Pronouns per the bibles.

| Villager | Portrait file | World sprite files | Match | What was wrong | What was fixed | Verified |
|---|---|---|---|---|---|---|
| wren (she) | ui_portrait_wren | npc_wren_idle_down/up/left/right, npc_wren_pose_* | yes | nothing: copper-red hair, red waistcoat (portrait hair is up, sprite shows it loose; same colour and outfit) | - | dialogue + world capture (Saloon), same red-haired woman; art by eye |
| hazel (she) | ui_portrait_hazel | npc_hazel_idle_*, npc_hazel_pose_* | yes | nothing: dark bob, round glasses, mustard cardigan | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| bram (he) | ui_portrait_bram | npc_bram_idle_*, npc_bram_pose_* | yes | nothing: full dark beard, grey apron | - | dialogue capture (portrait only, he was out of view); art by eye |
| tilda (she) | ui_portrait_tilda | npc_tilda_idle_*, npc_tilda_pose_* | yes | nothing: auburn bun, glasses, green apron | - | dialogue + world capture (General Store), same auburn woman in green apron; art by eye |
| juno (she) | ui_portrait_juno | npc_juno_idle_*, npc_juno_pose_* | yes | nothing: red-orange hair, goggles, tan apron | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| piper (she) | ui_portrait_piper | npc_piper_idle_*, npc_piper_pose_* | yes | nothing: black curly hair, mustard coat, fiddle | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| marcus (he) | ui_portrait_marcus | npc_marcus_idle_*, npc_marcus_pose_* | yes | nothing: flat brown cap, short beard, brown waistcoat | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| odalys (she) | ui_portrait_odalys | npc_odalys_idle_*, npc_odalys_pose_* | yes | nothing: dark hair up, cream coat | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| **felix (he)** | ui_portrait_felix | npc_felix_idle_down/up/left/right, npc_felix_pose_* | **no, now yes** | front and back sprites were a different, girl-looking drawing with red tails | all four idle sprites redrawn from one sheet (see above) | dialogue + world capture (Beach): straw hat, dark hair, blue jacket in both; art by eye |
| dorian (he) | ui_portrait_dorian | npc_dorian_idle_*, npc_dorian_pose_* | yes | nothing: brown hair, moss-green coat, scarf | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| elara (she) | ui_portrait_elara | npc_elara_idle_*, npc_elara_pose_* | yes | nothing: dark purple braids, light blue cap and dress (the sprite's purple reads lighter than the portrait's near-black at 16 x 32) | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |
| ione (she) | ui_portrait_ione | npc_ione_idle_*, npc_ione_pose_* | yes | nothing: blonde braid, lilac cardigan | - | art compared by eye (6x montage of portrait, four facings and poses); no capture |

Id wiring (all twelve): `ContentGenerator` sets `npc_<id>_idle_down/up/left/right` and `ui_portrait_<id>` from the villager id, expression portraits from `ui_portrait_<id>_<expression>`, poses from `npc_<id>_pose_<pose>`; the dialogue screen uses `NpcDefinition.PortraitFor`, the actor `NpcDefinition.SpriteFor`. `npc_generic_*` is referenced by no villager.

## Automated guard

`NpcImageConsistencyTests` (EditMode; villagers found from `Data/Npcs`, so new ones are covered): every villager has their own portrait file, four facing files and four pose files (by file name, so a generic or missing sprite that falls back to the front fails), no two villagers share a picture (byte-identical files), and the front, back and side drawings share at least 30% of their colours. **The last check is only a floor**: the old Felix front scored 47% against his side view, so a different drawing of the right colours cannot be caught by automation. Telling people apart still needs the capture comparison below.

## Verification status

Filtered tests green (`NpcImageConsistencyTests` 5, plus `PosesTests`, `ExpressionPortraitTests`, `AtlasTests`: 16 of 16); full suites pending. Development-build captures `Builds/npcqa/<villager>/` (git-ignored) were looked at for Felix, Wren and Tilda; the other nine are checked by eye from the art files only. No person has seen the new Felix in play.

## Things a person should still look at

- Felix's front sprite at 16 x 32 may show faint glasses (the generated sheet drew round spectacles; his portrait has none). If it bothers, redo with the prompt in `tools/art/redo_character.py` (`LOOK["felix"]`) adding "no glasses".
- The portraits are 32 x 32 painterly; the sprites are 16 x 32 chibi, so hair detail (Wren's bun, Tilda's bun) cannot match exactly.
