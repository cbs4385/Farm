# NPC design review: motivations, wants, interactions and dialogue

Status: review, 2026-10-16. Nothing was changed. Author: Claude (a read of every shipped villager script, the bibles, the relationship matrix, the goals and callbacks, the courtship and gossip data, the storylines, the Edmund topics, the mythos clue lines and endings, and the hall and quest data). Nothing was played; the findings are from reading, and the line ids are given so each can be checked in the developer console.

The question asked: do the twelve villagers' motivations, wants, interactions and lines add up to coherent, interconnected, compelling gameplay? Short answer: the cast is coherent and the connective tissue is unusually rich for the genre, but several of the biggest promises the writing makes are answered in the wrong place or not at all, and the horror layer leaves the cast's best material (Edmund, the four notes, the hall roof, the Keepers' affection) on the table. Most of the fixes below are text; a few are small scenes; one is a data change to the Community Hall.

## 1. What is working (keep it)

1. **Every villager has one legible want and the village knows it.** The tidbit lines (`<villager>.tip.*`) make daily talk carry information the player can act on: who loves what, whose birthday is when, and an oblique hint at what each villager quietly wants. This is the strongest interconnection mechanic in the game and it costs no systems. Seven of the twelve wants are hinted at by three or more other villagers (`VOICE_PASS_2026-10-15.md` section 3).
2. **The wants invert under the horror layer, and the inversions are good.** Odalys wants "a village that rests" and has carried the Covenant twenty years while no one sleeps through the night. Tilda's slate counts kindnesses because the village has always had to count. Marcus's "nearly fixed" roof covers a hall the Keepers use. Dorian's "Not me. The forest." becomes ambiguous. Wren's packed bag is a stake. These are the right shapes for a reveal that must land as "protective, not evil".
3. **The quiet-kindness motif is carried by behavior, not speeches.** Bram's unasked hook repairs for Felix, early book returns, Dorian's step gifts, Marcus's plank for Felix's boat, the anonymous notes. The player learns the cast by what they do for one another.
4. **Secrets are asymmetric and only the player sees both sides.** Hazel writes Piper's lyrics; Bram's fiddle tune; Elara's map behind the kettle; Bram's unsent letters. `RELATIONSHIPS.md` names this as the game's emotional structure, and the data bears it out.
5. **The Edmund topics are the best twelve lines in the cozy layer.** Each memory is in voice, each gives a fact, and six of them are seeded mysteries (the cellar door, the brass key, "affairs in order", the almanac margins that stop in fall, the four notes, counting the trees). A second playthrough reads them all differently. See finding 4.4 for the problem.
6. **The three-tier want arc (hint, after, resolve) is the right machine**, and the `{if choice}` single-entry pattern keeps it cheap. Seven resolves already depend on another villager's state (Felix on Elara's heart 8, Elara on Felix's hearts, Dorian on Hazel's hearts, Ione on Hazel's heart 10, Bram and Juno on each other's heart 10, Tilda and Marcus on the hall), which is exactly the interconnection the question asks about.

## 2. Coherence findings: promise and payoff

Severity: **A** a want is answered by the wrong thing, or canon contradicts itself in play; **B** a player who maxes two or three villagers will notice; **C** polish.

### 2.1 (A) Wren's want is a full room; her heart 10 is an empty one

Wren wants "a full room and a story to tell about it" (bible, goals hint lines). Her quest chain ends with `wren_full_house` ("I'm planning a full house. A real one. Music, food, nobody in a corner. I need one thing from you.") and the player brings the wine. Then nothing: no full house is ever staged. Her heart 10 (`wren_heart10`, memory title **"Opening Night"**) is `"Tonight's a performance. One song, one listener. I picked you."` The title promises the house; the scene is the opposite of her want. The one-listener performance is a lovely beat, but it is Hazel's beat (Hazel's want is to be heard by one person properly, and her heart 10 does exactly that). Two villagers now have the same climax, and the one whose want it does not serve has the title that says otherwise.

Fix (text only): retitle `memory.wren_heart10` ("One Listener" or "Last Call") and let the wine from `wren_full_house` be what she opens there. Fix (one scene): stage the full house as the resolve of the chain, with the bag-choice song inside it. See 5.3 for the capstone that would do this for four villagers at once.

### 2.2 (A) The hall roof is not a room of the hall

The village's longest-running "nearly" is Marcus's hall roof. Tilda, Dorian, Hazel, Piper, Odalys, Bram and Wren all talk about it; Hazel shelves the plans where he will find them; Wren runs a pool on it; Piper says he wants it done so the hall can hold a dance; Marcus's heart 8 is the roof plan; his quest is planks for the roof; his resolve line is "Not nearly. Fixed." gated on `flag:hall.restored`.

The Community Hall data (`hall_and_festivals.json`) has six rooms: pantry, crafts, fishtank, boiler, bulletin, vault. There is no roof. `marcus_planks` feeds nothing in the hall; the crafts room gives Marcus 100 friendship for reasons the text never states. The player restores the hall without Marcus touching the roof, and Marcus then says he fixed it. The cast's most-referenced shared goal has no slot in the system that resolves it.

Fix (data): add the roof as the hall's seventh room or as the final objective of `hall_complete`, with `marcus_planks` (or its wood) as an input, and a short Marcus beat when it closes ("Measure twice. Then I asked you."). The dance in 5.3 is what the roof is for.

### 2.3 (A) Three lines leak a confession before its scene

- `wren.cl.5` (6 to 8 hearts, priority 0): "I almost left this village once. Then you showed up and ruined it." This is the bag (heart 8) said plainly before the player has seen the bag. Move it to the confidant tier (9+) or gate it on `flag:event.wren_heart8`.
- `juno.co.1`: "You're the reason I stayed in Wetherell. I should've said it **years ago**." An engaged player reaches 10 hearts inside two months (`PACING.md`); the player has been in Wetherell at most a year or two. Cut "years ago".
- `ione.co.1`: "I set something aside for you, **years ago**, before we'd met." Same problem; "the day you arrived" is already her legendary line (`ione.rare.3`) and is true.

### 2.4 (B) Juno's mark: the player brings gold and receives copper

`juno_gold` asks for a gold bar "for my first mark, my own mark. If you can find one, I'll stamp the first piece with it. I think you should have it." Her heart 10 (`juno_heart10`) then hands over "a tiny copper bar" as "the first thing with my name on it". Bram's heart 10 hands over a gold bar. The chain promises gold and delivers copper while the other smith delivers the gold. Fix: make the heart 10 item gold when `quest:juno_gold=done`, or reword the quest so the gold is for the stamp and the first stamped piece is copper on purpose ("copper's honest; gold's for the die").

Also: `memory.bram_heart10` is **"Your Tool"**; the scene gives a gold bar that is "not for work". The bible's original idea was a tool shaped to the player's wear. Retitle ("Soft Gold") or deliver the tool.

### 2.5 (B) Elara's want has four props

Her want (the sea from a boat) is carried by a **list** (heart 8), a **map behind the kettle** (Wren, Odalys tips), a **notebook of daydreams** (`elara.cl.2`) and an **atlas at the library desk** (Ione, Hazel tips). Four objects for one longing reads as four writers. Keep two: the map behind the kettle (it is the one other villagers notice) and the list (it is the one the player sees). Fold the notebook line into the list; make the atlas Ione's set-aside book that she reads standing up (Hazel's line already says this).

### 2.6 (B) The letter template survived the voice pass

`VOICE_PASS_2026-10-15.md` says the "if I wrote you a letter" joke now exists once (Bram's three words, Juno's forty pages as the deliberate opposite). In the shipped pools the "I'd not write a letter, I'd X instead" shape is still at the confidant tier of eight villagers (`bram.co.4`, `juno.co.4`, `dorian.co.3`, `elara.co.3`, `felix.co.3`, `ione.co.3`, `marcus.co.3`, `odalys.co.3`), plus Piper's song variant (`piper.co.4`) and Wren's book variant (`wren.co.5`). Each is in voice; together they are one writer. Keep Bram and Juno; give the others a confidant joke that is theirs. The same applies, more mildly, to the "Thank you, [player]" plus in-voice tail at the confidant tier of all twelve.

Other repeated images: "the warmest room in Wetherell" is claimed by the forge (`bram.winter.1`, `juno.winter.1`, `juno.bark.winter.1`, `juno.co.3`), the saloon (`wren.snow.2`), the clinic (`elara.winter.2`) and the library (`react.hazel.winter.d`). The bible says the forge is the warmest building. Wren's claim can stay as an embellishment if someone corrects it (Hazel already corrects her fireflies, eleven versus forty-one, which is the right gag); Elara's and Hazel's should go. "I leave a lamp on for whoever is still out" belongs to Tilda (`tilda.winter.3`, `tilda.co.3`) and is also Hazel's (`hazel.late.2`). "The door argues with the wind" is Wren, Bram and Marcus.

### 2.7 (B) Enhanced quests ask the expert for their own stock

Felix sells bait and asks the player for bait (`felix_bait`). Dorian forages all day and asks for three mushrooms (`dorian_basket`). Odalys buys elderflower from Tilda by the bunch (`tilda.tip.3`) and asks the player to forage three. Bram's copper quest shows the fix: "Smelt them yourself. Good practice." Reframe each as the villager wanting the player to learn the thing (Felix: "Make your own bait. A partner should."; Dorian: "Find three. Look down. Everyone looks up."; Odalys: "Pick it yourself. Then you'll know the smell when it's time.").

### 2.8 (C) Small contradictions

- Bram is "thirty-something" (`bram.bday.2`) with a retired master, an apprentice, letters unsent for years, "I used to count the days alone", and Juno says he has almost told her about his teacher for eleven winters. Forty-something, or cut the number.
- Wren's stool: "the good stool's reserved. For anyone but Felix" (`wren.fri.1`) against "Felix is my favorite regular" and "Felix's stool" (`felix.here.2`). Affectionate, but the two never meet; have Felix's stool be the bad one he loves.
- The bible says Piper serenaded Elara once and both pretend it never happened; the shipped lines make Elara the first to hear every new song and the matrix scores them 2/2. Fix the bible.
- Tilda's Sunday line invites the player to "come back tomorrow" from the shop, but on Sundays she is at the beach or the saloon. Harmless; note for the schedule pass.

## 3. Interconnection: what links mechanically versus only in text

| Link | Carried by | Mechanical? |
|---|---|---|
| Felix's boat and Elara's sea | `felix.goal.r2`, `elara.goal.r1` | talk lines only; no rowing scene |
| Hazel and Ione's reading night | `hazel.goal.r2`, `ione.goal.r1` | talk lines only |
| Juno's own forge, "Next door" | `juno.goal.r2`, `bram.goal.r2`, `overheard_juno_bram_4` | talk and one overheard scene |
| Marcus's roof and the hall | `marcus.goal.r1`, `tilda.goal.w1/w2` | gated on the hall flag, which Marcus does not influence (2.2) |
| Dorian shows Hazel the notebook | `dorian.goal.r1` | talk line only |
| Piper's song sung without her | `piper.goal.r1/r2`, `piper_heart10` | the saloon sings it in heart 10 (good); Bram humming is a line |
| Bram's teacher writes back | `bram.goal.r1` | a talk line; no letter in the mailbox, though mail exists |
| The pie feud, the pumpkin, the notes, the band | storyline scenes | real scenes with choices and callbacks (the model to copy) |

The storylines prove the game can stage a cross-villager payoff with a choice and a callback. The want arcs stop one step short: every resolve is a sentence. The cheapest large win in the whole cast is to promote four or five of these resolves to scenes (section 5.3). Bram's teacher's reply should be a letter, since the mailbox already carries villager mail (`bram_tools`, `ione_recipe`).

## 4. The horror layer against the cast

### 4.1 (A) Clue lines are not gated on friendship

Every Keeper clue (`mythos.<keeper>.clue1..3`) is gated on `horror:1 && keeper:x && var:lore>=N` and nothing else; Hazel's clues likewise. Tilda will say "We have always kept something, [player]. My mother did, and hers. It is a duty" (clue2, lore 3) to a player at zero hearts. Hazel, whose cozy first meeting is "Ask me again when we're friends", tells a stranger about the dogs and the milk at priority 9 the moment lore reaches 1. This breaks both characters and the design rule that the player should befriend a Keeper before the menace shows. Gate clue1 on 2 hearts, clue2 on 4, clue3 on 6 (the clue priorities 8 to 10 already outrank cozy talk, so nothing else changes). Odalys's invitation at standing 8 could add 6 hearts: she asks a friend, not a useful stranger.

### 4.2 (A) The Keepers never react to being sabotaged

The player can take an offering from the altar (`ritual:snatch`) or, through Hazel, make Tilda or Marcus ill (`sick:`). Afterwards the ritual scene gives Odalys three lines ("Edmund's heir. Of course.") and the Keepers' cozy pools continue unchanged: Tilda puts the player on the slate the next morning. The resist ending shows the writers know the beat they are missing ("Tilda will not look at you for a month. Then she will bring you a jar of jam and say nothing"). That beat belongs in play, not only in the epilogue. A Mythos-layer reaction set (two lines per Keeper after `flag:mythos.interference`, hurt and not hostile, in each voice: Tilda's slate goes quiet, Marcus measures without speaking, Wren pours and does not sit, Dorian leaves nothing on the step, Odalys writes something down for once) would be the strongest emotional moment the horror layer has, because the affection it withdraws is real.

(The playtest note in `STATUS.md` that the sabotage prompt fired twice is already recorded as a bug.)

### 4.3 (B) Dread is invisible in the people

Dread speeds friendship decay and the bible names the unaware six as the player's "real human reaction" to it (Bram "stolid, practical, kind"). No line anywhere is conditioned on dread except in birthdays and courtship. The player's unease cools every relationship silently, which reads as a bug, not a story. Two lines each for Bram, Juno, Piper, Felix, Elara and Ione at a dread threshold, in `Farm.Mythos` data ("Hm. You look like iron that's been quenched too fast. Sit."; "Keep your socks on. No, I mean it, you're shivering."), would make the mechanic legible and give the unaware six their promised role.

### 4.4 (A) Edmund's mysteries are seeded and never harvested

The Edmund topics plant: a cellar door Edmund would not let Marcus oil; a tiny brass key Juno made for "a box he'd lost the lock to"; Odalys's "he'd put his affairs in order"; Ione's almanacs with margins that stop in fall ("Done well enough"); Dorian's "counting the trees". None pays off. The two uncle letters simply arrive in the mailbox (`mythos_uncle_note` is subtitled "Found behind the stove" with no finding). The relics are in the mine.

Tie them together with no new systems: the second letter and Juno's key live in the box behind the cellar door (a conditional object on the farm map, opened with `mythos.brass_key` given by Juno at the Edmund topic once lore is 2); Hazel's clue2 ("a pattern in the town records, four times a year") cites Edmund's almanac margins by name, so Hazel's archive work and Ione's gift of the almanacs become one thread; Dorian's "counting the trees" is where the clearing is. The cozy topics stay exactly as they are; only the Mythos layer learns to point at them.

### 4.5 (A) The four notes are already one motif and nobody has noticed

Piper hums four notes when she thinks she is alone (`hazel.tip.1`, `juno.tip.1`, `elara.tip.4`). Her legendary line plays four slow notes and says "That's yours. I'm waiting for you to hum the ending." The Edmund topic says Edmund hummed "always the same four notes", asked her to leave it unfinished, and she now plays those four notes before every set. The wakefulness ladder has "a low hum at night" and "the hum nearly a voice"; the ritual scene has the hum "find a new note" when an offering is taken. This is one motif in three places by accident. Make it deliberate in `Farm.Mythos`: the four notes are what Edmund hummed walking back from the wood, Piper's heart 10 chorus at horror on carries them, and the sealed ending resolves the fifth note. Zero new mechanics; it is the thing a second playthrough would be for.

### 4.6 (B) Keeper romance and the bag's stake are unused

Wren and Dorian are romanceable Keepers; the design says this is "handled in heart events, never exploitative". No heart event or partners line acknowledges it at any horror level. Wren's bag was approved as "a stake for her leaving"; her "go" resolve has her walk to the point and come back, and a Keeper who leaves fails the ritual. One Mythos variant each would do: Wren's "go" branch at horror on becomes "I got as far as the point. Then I remembered what I keep." and Dorian's partners line gains "...The forest knows your boots now. So do the others. Be careful of that."

### 4.7 (C) Marcus's roof is the Keepers' roof

A Keeper has kept the hall roof "nearly fixed" for six winters, and the Keepers use the hall. The layer never says so. One clue line ("A roof that's finished gets used. A roof that's nearly finished gets left alone. I know which I'd rather") would turn the village's running joke into a second reading for free.

## 5. Recommendations, in order of value for effort

### 5.1 Text fixes (a day; no ids renamed)
1. Retitle `memory.wren_heart10` and `memory.bram_heart10`; make `juno_heart10` give gold after `juno_gold` (2.1, 2.4).
2. Move or gate `wren.cl.5`; cut "years ago" from `juno.co.1` and `ione.co.1` (2.3).
3. Reduce Elara's props to the map and the list (2.5).
4. Replace eight confidant letter jokes; settle the warmest room on the forge with Hazel correcting Wren; give the lamp to Tilda (2.6).
5. Reframe the Felix, Dorian and Odalys quests as lessons (2.7).
6. Bram's age, the stool, the Piper and Elara bible line (2.8).
7. Hearts gates on every clue line and on Odalys's invitation (4.1).

### 5.2 Mythos data (two or three days; all in `Farm.Mythos`)
1. Post-sabotage reaction set for the five Keepers (4.2).
2. Dread lines for the unaware six (4.3).
3. The cellar box, Juno's key, and Hazel's clue2 naming the almanacs (4.4).
4. The four notes as the layer's motif (4.5).
5. Wren's "go" and Dorian's partners variants; Marcus's roof clue (4.6, 4.7).

### 5.3 Scenes (a week; the biggest payoff in the cast)
The resolve lines promise five scenes the game can already stage with the existing templates:
- **The Hall Dance.** When the hall is restored and the roof is Marcus's (2.2), a festival-sized scene in the hall: Piper's chorus (the word the player chose) sung by the village without her, Wren hosting the full house she asked for, Juno's bell on the door, Bram humming. One scene closes four wants and is the cozy game's true climax. Memory title "Opening Night" finally earned.
- **Past the Point.** Felix rows Elara (and the player) in the boat that leaks politely; Wren is at the point looking at the sea if she chose "go".
- **Reading Night.** Hazel reads to more than one; Ione pours; Dorian is in the back row with a leaf.
- **Next Door.** Juno's own small forge; Bram brings the good tongs (the overheard scene already wrote the dialogue).
- **The Letter.** Bram's teacher's "Good work." arrives in the mailbox as mail, not as a line.

### 5.4 Systems (later; claim in `STATUS.md`)
- The roof as a hall room fed by Marcus's planks (2.2).
- Carry-a-message errands (`RELATIONSHIPS.md` 6.3): the pie feud scene is already this; generalize it so the player can carry one kindness between any pair with a written tie.

## 6. Scorecard

| Dimension | Score (1 to 5) | Note |
|---|---|---|
| Legible motivation per villager | 4.5 | one want each, hinted by others, revealed at the right heart |
| Promise kept by payoff | 2.5 | Wren, Marcus and Juno's climaxes answer the wrong want or the wrong item |
| Villager-to-villager web | 4 | 46 topics, 55 overheard scenes, 8 storylines, 7 cross-gated resolves |
| Mechanical interconnection | 2.5 | the web is almost all text; the resolves are sentences, not scenes |
| Voice consistency | 4 | strong; one template and three repeated images remain |
| Horror layer uses the cast | 2 | gating, reactions, dread and Edmund's seeds are all unused |
| Compelling to pursue to 10 hearts | 3.5 | the arcs are good; the last step of each is the weakest |

## 7. What this review did not do

Nothing was played. The lint and the reachability tests were not run. Heart events 2 to 6 for the Full six and the festival scripts were read for consistency only. The barks, gazette, name reactions and social actions were not read. A person should read sections 2.1, 2.2 and 4.1 in the developer console before deciding anything.
