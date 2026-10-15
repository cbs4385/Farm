# Dialogue voice pass (2026-10-15)

Status: **written and compiled; verified by automation only** (see `../STATUS.md` for the test runs). Nobody has read these lines in play. An editorial pass over the twelve villagers' talk pools with three aims: one voice per villager, lines that read as speech, and tidbits the player can *use*: what another villager loves, when their birthday is, and what they quietly want.

## 1. What was wrong

Reading all twelve pools side by side (the `.fscript` files plus the older lines in `npc_dialogue.json` and `npc_roster.json`):

1. **Templated lines across the cast.** The same joke was written for nine or ten villagers with the nouns swapped: "If I wrote you a letter, it'd be..." (10 villagers), "Thank you, [player]. For everything. That's the whole speech / verse / prescription / tide table" (10), "I used to count the days... now I count the days you come in" (4), "I fix things nobody asked me to fix" on Sundays (3), "You were gone a while. I told the chairs / made a clasp a day / wrote a song a night" (4). Heard one after another they read as one writer, not twelve people.
2. **The weekday lines were a schedule board.** Nearly every villager's Tuesday line was "Wren's closed", every Wednesday "Marcus and Bram are on the bench", every Thursday "Felix is off". Useful once; the ninth time it is filler.
3. **Juno was Bram with exclamation marks.** Her time-of-day, place, farm, storm, wind and gift-lab lines were his, lightly rephrased ("Don't stand in the sparks", "Last job of the day. Always the hard one", "I'll carve a hammer on it"). Two gags contradicted each other: both claimed the rubber-chicken prank, and the quench bucket was named by Juno in one line and by Wren in another.
4. **Doubled lines.** The `.fscript` expansions re-wrote many of the older JSON lines nearly word for word (Felix's "getting the hang of this", Dorian's "come with me tomorrow" without its choice, Elara's "stay a minute?", Ione's "I set a book aside for you", Marcus's "good hands"), so a player heard the same thought under two ids.
5. **Cross-villager mentions carried no information.** Villagers named each other constantly, but almost only for schedules. Nothing in daily talk told the player that Bram loves truffles, that Hazel's birthday is Fall 8, or that Elara keeps a map of harbors behind the kettle.

## 2. What changed

Only text and new talk entries. No ids were renamed or removed, no conditions, priorities, tags or rarities on existing entries changed, no code changed.

- **103 lines rewritten** in the villager scripts and **24 older lines** rewritten in the string table (keys unchanged). Every templated line above now exists once, in the voice it suits best (Bram keeps the three-word letter and the whole speech; Juno keeps the forty-page letter as the deliberate opposite), and the other villagers got lines of their own.
- **82 new "tidbit" lines** (`<villager>.tip.<n>`, category `reactive`, priority 0): each villager now mentions six to nine others (counting the tidbits folded into existing lines) in their own idiom, and each mention gives the player something: a loved gift, a birthday date, or an oblique hint at a want. Hints at the secrets the heart events reveal are gated on the speaker's hearts (3 or more; Bram's fiddle line at 6) so they foreshadow instead of spoiling. Facts are taken from the bibles and the roster data; no new canon.
- **Juno's copied blocks** are rewritten around her own material (labels, Piper on the quench bucket, peppers eaten like apples, sketches that are "not Bram's patterns, okay, one is"). The prank is hers; the bucket name is hers and Wren spread it.
- **Weekday lines** keep a few schedule facts where the pair matters (Bram and Marcus's bench, Tilda and Wren's rivalry, Juno watching Bram leave for the bench) and otherwise say what that villager's own week is like.
- **Generic gift lines** for Juno, Piper, Elara, Odalys, Tilda, Felix and Dorian (the fallbacks when no item line exists) are now in voice instead of "Thanks!".

## 3. Who hints at whom

The map writers should keep consistent. "Want" rows are the heart-event secrets and are gated; "love" and "birthday" rows are in the open pool.

| About | Loved gift named by | Birthday named by | Want hinted by |
|---|---|---|---|
| Wren | Hazel (pickles) | | Tilda (coast road), Felix (past the point), Piper (under the bar), Marcus (the bet and the sea), Ione (coast book) |
| Hazel | Tilda, Dorian, Ione (blackberries) | Tilda, Dorian, Ione (Fall 8) | Wren, Odalys (reads aloud), Bram (writes), Elara (books of far places) |
| Bram | Wren, Dorian, Juno, Felix (truffles, mushrooms), Marcus (roasted roots) | Hazel, Juno, Marcus (Fall 4) | Marcus (the letters), Ione (the book of letters), Piper (he hums) |
| Tilda | Wren, Felix, Marcus (strawberries) | Bram, Felix, Marcus (Spring 12) | Juno (counts the stock) |
| Juno | Bram, Odalys, Elara (peppers), Piper (gold) | Bram, Piper (Spring 25) | Wren, Tilda, Marcus, Dorian (one thing that is hers) |
| Piper | Wren (jam), Felix (sunflower) | Wren, Felix (Spring 5) | Hazel, Juno, Elara (the four notes) |
| Marcus | Wren, Tilda, Dorian, Ione (pumpkin) | Wren, Dorian (Summer 9) | Hazel (page forty), Bram, Odalys, Dorian, Piper (the roof) |
| Odalys | Tilda (elderflower) | Elara, Ione (Winter 3) | Marcus, Elara (never rests) |
| Felix | Tilda, Piper, Odalys (tuna) | Elara (Summer 21) | Wren (company at dawn), Hazel (the sea maps) |
| Dorian | Tilda (wild garlic), Odalys | Wren, Juno, Odalys (Winter 11) | Marcus, Ione (he draws) |
| Elara | Tilda (cranberries, jam), Juno (jam), Piper (snowdrops) | Tilda (Spring 18) | Wren, Odalys (the map behind the kettle), Hazel (the atlas), Felix (a leaving look), Dorian (boats), Ione (the atlas) |
| Ione | Wren (pearl), Hazel, Odalys (snowdrops) | Wren, Hazel, Piper, Odalys (Winter 20) | Felix, Dorian (the sea in winter) |

## 4. How to check it

- Voice rules: `Farm > Narrative Report` (`Builds/narrative_report.md`); every villager is locked, so any warning fails `NarrativeLintTests.TheShippedContent_HasNoNarrativeErrors_AndTheReportBuilds`.
- Sync: `FScriptSourcesInSyncTests` (sources and generated JSON agree), `DialogueVarietyTests` (reachability), `StorySimulationTests` (coverage guard).
- In play: the developer console (`F1`, development builds) can set hearts and force a pick; the new lines are `<villager>.tip.<n>`.

## 5. Not done

- The older first-meeting dialogues and the heart events were read but not rewritten; they were already in voice.
- The gossip "Ask about" topics (`gossip_topics.fscript`, `relationship_gossip.fscript`) were read for consistency and left alone.
- No person has read the new lines in the game.
