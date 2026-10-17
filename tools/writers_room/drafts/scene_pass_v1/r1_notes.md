# Notes handed up in Round 1 (read the ones that name you)

## from bram
- dlg.bram.long.0: the bible wants his longest sentence ever, but the lint caps Bram at 12 words per sentence; I wrote it at exactly 12. Raise the cap for this key if the beat should run longer.
- event.bram_heart10.thanks: the bible says he says the player's name for the first time here, but adding [player] would change the line's tokens (lint error). Consider adding [player] in a later pass with a lint exemption.
- event.story_whistler_bram.intro had stage directions inside spoken text; removed. If the tune stopping matters, the Director could add a sfx or an emote note before the line.
- bram_heart2 adds: both go after event.bram_heart2.1, in this order (strike line first, teacher line second).
- Kept as shipped (already right): next_door none; overheard_juno_bram_2 both lines, overheard_juno_bram_3.l4, overheard_juno_bram_4.l4, all overheard_wren_bram lines (the four-word bet counts depend on them), bram_marcus l1 lines, tilda l4, birthday choices (c0 is a shipped line).
- bram_heart4: the bible's heart 4 has the player bring a broken tool with ask/together/give-up choices; the card has hold/hammer/watch. I kept the card's meanings.

## from dorian
- event.story_pumpkin_dorian.after was third-person narration in Dorian's mouth ('He's returning it...'); rewritten in first person. Director: if it was meant as narration, reassign the speaker instead.
- dlg.story.pumpkin.dorian.2 'for scale. Next to the carp' made no sense in context; replaced with the spores excuse from his intro.
- dorian_courtship and dorian_courtship2 share dlg.dorian.courtship.*; one set of rewrites covers both. The two .after lines now differ only in the 'wait' branch.

## from elara
- elara_courtship and elara_courtship2 share dlg.elara.courtship.*; I rewrote those once and gave each scene its own intro and after line (courtship2 now remembers the first ask).
- Cut 'It's a ritual' from the courtship after line (horror-adjacent word in a cozy scene).
- dlg.elara.partners.0 said 'Clinic's closed' but the scene plays 9 to 17 on clinic days; now Dr. Penn gave her the afternoon. past_point.talk.0 no longer states a list count, because it is 41 or 42 depending on the heart 8 choice.

## from felix
- dlg.felix.courtship.* keys are shared by felix_courtship and felix_courtship2; rewritten once.
- dlg.felix.partners.0 said 'Evening's ours' in a scene gated 06:00-14:00; changed to Morning.
- event.story_whistler_felix.intro had stage directions inside spoken text; removed. The story_pumpkin after line was third-person in Felix's mouth; now first person.

## from hazel
- hazel_heart10 cues music hazel_theme, which is not in the music cue list; it will play silent.
- hazel_heart10 plays sfx coin during the reading; pageturn would fit better.
- hazel_birthday r0 said 'four words' for a birthday wish; changed to two and matched the after-branch.
- overheard_hazel_piper_3: card has Piper offer to credit Hazel; Hazel declines, keeping the lyric secret mostly intact per bible.

## from ione
- ione_courtship and ione_courtship2 shared an identical intro; courtship2 now acknowledges the earlier wait.
- dlg.ione.partners.0 said 'evening' but the scene plays 9-17; changed to 'the rest of the day'.

## from juno
- juno_heart8: card intro said 'Tonight I show it to Bram' but the scene plays 9-17 with Bram present; I made it 'now'. Bram's heart 8 should match (bible calls it 'the same night').
- story_notes_juno.after: new fact that Bram keeps six of her notes in his apron and thinks they're from Tilda; Bram's writer should know (or veto).
- overheard_juno_bram_3.l1: changed 'carved a small mistake' to fixing the bench's wobbly leg so Bram's 'Best joint in the shop' makes sense.
- dlg.juno.courtship.* keys are shared by juno_courtship and juno_courtship2; c0 changed from 'Forty items is a lot' to 'Read me the list' so it fits the second ask (forty-one items).
- next_door and juno_heart10 both point at the empty shop next door (measured bench, empty sign hook): one consistent thread.

## from marcus
- hall_dance: Wren says 'Seven winters of coins'; canon is six winters (I use six in heart8). Please fix Wren's line or confirm.
- hall_dance narration 'Tilda lets him lead' describes two people dancing together, which the brief forbids; suggest 'Marcus takes his one dance. Tilda dances beside him.'
- hall_dance.11 dropped 'Hm.': that is Bram's tic.
- story_whistler_marcus.intro had a stage direction inside Marcus's line; removed. Director may want a narration beat for the plank.
- story_pumpkin_marcus.after was written in third person ('Marcus will not stop smiling') though Marcus speaks it; rewritten first person.
- overheard_marcus_odalys_3.l4 said 'Sundays'; Dr. Penn's clinic closes Saturday, so changed to 'Saturdays'.
- Kept unchanged: dlg.marcus.birthday.0 (shipped line), overheard_bram_marcus_2.l4, overheard_marcus_odalys_1.l4.

## from odalys
- odalys_heart8: the trigger is hour 9-17 but the old intro said 'tonight'; I changed it to 'closed the clinic early today'. The bible's 'rare quiet evening' would need the trigger moved to evening.
- odalys_heart2: the old line called the gift 'a tonic' but the effect gives two salads; the line now names salads.

## from piper
- dlg.piper.courtship.* keys are shared by piper_courtship and piper_courtship2; rewritten once, they must work for both (they do).
- event.piper_heart4.after now says she is writing the request down for later: heart 10 should honor the request flag (choice.request.*) if the engine supports it; currently piper_heart10 only branches on chorus.
- event.hall_dance.5 (the chorus) kept as is; it already matches the heart 8 choices. piper_heart5 now plants the four notes thread; Director may want Edmund topics to pick it up.

## from tilda
- tilda_heart5: title says scones but the effect gives food.mashed_potato x2; I made line .2 name the mashed potato so text and item agree.
- tilda_heart2: my added last line says her mother wrote down only one recipe; heart 8's card is it. The gratin was taught by hand, so no contradiction.
- story_pumpkin_*: the closing line by Dorian/Marcus/Felix talks about the speaker in third person ('He's returning it...'); it reads like narration and should be [narration] or rewritten by them.
- hall_dance: Wren says 'seven winters' of the roof pool; canon says six winters.
- tilda_heart10: bible says she hands over the original two-note bell; the scene gives only the pie. I kept the item; the bell could be a later gift.
- tilda_heart6: I removed 'every night' from her worry (persona: no talk of nights) and replaced the thank-you close.
- overheard_tilda_wren_3: Wren says 'Come at six', but Tilda's schedule has her at the saloon from seven. I changed Tilda's 'tonight' to 'later'; Wren's 'six' should become 'seven'.

## from wren
- hall_dance.9: card said 'seven winters'; the running fact is six winters, so I wrote six. Check Tilda/Marcus lines in that scene for the same number.
- hall_dance.12: original 'Tilda lets him lead' implied two people dancing together; rewritten as two separate dances the engine shows.
- wren_heart4 card has Wren singing her own bad song; the bible's heart 4 has her volunteer the player to introduce Piper. I followed the card's beats; the choice flags still match.
- wren_courtship2.intro was a copy of the first ask; rewritten to acknowledge the wait. The shared dlg.wren.courtship.* keys serve both scenes, so they stay neutral.
- wren_partners.r2 mentioned a bakery, which does not exist; now Tilda's lamp. overheard_tilda_wren_3 said six o'clock but Tilda reaches the saloon at seven; fixed.
- Kept unchanged on purpose: bag n0 and birthday n0 (shipped lines), heart4 bath joke, Bram 'Four words! Four!' scene, Piper overheard 1-2.
