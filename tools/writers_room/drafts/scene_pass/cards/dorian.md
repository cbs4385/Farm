# Scene cards for dorian

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 35 spoken lines in scenes, 28 dialogue lines and 18 player choice labels.

### dorian_heart2 · "Shh, Look" · wholesome
event · npc_roster.json · map Forest · priority 6
plays when: `hearts:dorian>=2 && hour>=8 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "Shh. Look."  ✏ `event.dorian_heart2.0`
  · (move player to (19,11))
  · (player faces up)
  · (dorian emote ellipsis)
  [dorian]: "A fox. Don't move. There."  ✏ `event.dorian_heart2.1`
  [dorian]: "Thanks for being quiet with me."  ✏ `event.dorian_heart2.2`
  · effects: give:forage.mushroom,3, friend:dorian,40, flag:event.dorian_heart2

### dorian_heart5 · "The Secret Spot" · surprise
event · npc_roster.json · map Forest · priority 7
plays when: `hearts:dorian>=5 && hour>=8 && hour<17 && flag:event.dorian_heart2`
beats may be ADDED or CUT in this scene
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "This is my secret spot."  ✏ `event.dorian_heart5.0`
  · (move player to (19,11))
  · (player faces up)
  · (dorian emote ellipsis)
  [dorian]: "I've never shown anyone."  ✏ `event.dorian_heart5.1`
  [dorian]: "Keep it to yourself."  ✏ `event.dorian_heart5.2`
  · effects: give:forage.truffle,1, friend:dorian,60, flag:event.dorian_heart5

### reading_night · "Reading Night" · wholesome
event · spec_scenes.json · map Library · priority 7
plays when: `flag:choice.reading.yes && flag:event.hazel_heart10 && flag:event.ione_heart8 && flag:met.dorian && hour>=17 && hour<21 && !weekday:sat && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: ione(6, 4), hazel(8, 5), dorian(4, 4), wren(7, 3), player(6, 2)
  · (letterbox on)
  · (lighting warm)
  · (place ione to (6,4))
  · (ione faces down)
  · (place hazel to (8,5))
  · (hazel faces left)
  · (place dorian to (4,4))
  · (dorian faces right)
  · (place wren to (7,3))
  · (wren faces down)
  · (move player to (6,2))
  · (player faces up)
  [ione]: "Four chairs, one lamp, and tea nobody ordered. This is my not-nervous face. Does it work?"
  [hazel]: "(It doesn't.) I brought something. I haven't decided whether to read it."  (expr embarrassed)
  ▶ DIALOGUE `reading.hazel` (player-facing graph, plays here):
    n0 [hazel]: "(Nobody has to hear it.) What would you do? ...I mean, if you were me."
       ? "Read it. We're listening." → r0  effects: flag:choice.reading.poem, friend:hazel,10
       ? "Read something borrowed." → r1  effects: flag:choice.reading.borrowed, friend:hazel,10
       ? "Listen first. Ione can start." → r2  effects: flag:choice.reading.listen, friend:hazel,10
    r0 [hazel]: "(Fine.) A short one. Eleven lines. (I counted twice.)"
    r1 [hazel]: "Someone else's, then. Kinder to the room. (Mine can wait.)"
    r2 [hazel]: "Then Ione goes first. (Thank you.) I'll stay."
  [hazel]: "The lamp does not mind the dark. It keeps it company. (That is all of it. That is the poem.)"  (expr thinking) [only if flag:choice.reading.poem]
  [ione]: "That is a good poem. I won't say how many times I'll read it. Once. Many times." [only if flag:choice.reading.poem]
  [narration]: "Ione reads two pages about the winter sea, and Hazel listens with her eyes shut. Hers can wait." [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and stays." [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "I brought a flower. Pressed. For the lamp."  ✏ `event.reading_night.2`
  [wren]: "I'll bridge it, friend. Everything good was told aloud first. Then somebody wrote it down so it would stay." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "That is a night of readers. I counted twice, hoping for more. I will count again."  (expr happy)
  · (remove prop flower)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.reading_night, friend:ione,40, friend:hazel,40, friend:dorian,40

### story_notes_dorian · "The Note Writer" · wholesome
event · storyline_events.json · map Village · priority 7
plays when: `flag:notes.began && storyline:anonymous_notes.dorian && hearts:dorian>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: dorian(25, 18), player(25, 17)
  · (place dorian to (25,18))
  · (dorian faces down)
  · (spawn prop prop.note at (26,18))
  [dorian]: "Oh. You weren't meant to see that. I... it's easier on paper."  ✏ `event.story_notes_dorian.intro`
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote ellipsis)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.notes.dorian` (player-facing graph, plays here):
    n0 [narration]: "You catch Dorian slipping a folded note under a door. Neither of you moves."  ✏ `dlg.story.notes.dorian.0`   (→ n1)
    n1 [dorian]: "...Well. Now you know."  ✏ `dlg.story.notes.dorian.1`
       ? "Say nothing. Just smile." → r0  effects: flag:choice.notes.dorian.smile  ✏ `dlg.story.notes.dorian.c0`
       ? "Say thank you." → r1  effects: flag:choice.notes.dorian.thanks, friend:dorian,10  ✏ `dlg.story.notes.dorian.c1`
       ? "Leave a note back." → r2  effects: flag:choice.notes.dorian.note, friend:dorian,15  ✏ `dlg.story.notes.dorian.c2`
    r0 [dorian]: "...You smiled. That's a lot of kindness for one smile."  ✏ `dlg.story.notes.dorian.r0`   (expr embarrassed)
    r1 [dorian]: "Oh. Thank you. That's the whole reward. I'll go and be flustered somewhere."  ✏ `dlg.story.notes.dorian.r1`   (expr happy)
    r2 [dorian]: "A note, for me? ...I'm keeping it. In a drawer I've just invented."  ✏ `dlg.story.notes.dorian.r2`   (expr happy)
  [dorian]: "A note says what a face can't. Mine can't say much. Thank you for staying."  ✏ `event.story_notes_dorian.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_notes_dorian, friend:dorian,40, flag:storydone.anonymous_notes

### story_scarecrows · "The Scarecrow Contest" · funny
event · storyline_events.json · map Village · priority 7
plays when: `storyline:rival_scarecrows && season:fall && weekday:sat && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: tilda(25, 18), player(25, 17)
  · (place tilda to (25,18))
  · (tilda faces down)
  · (spawn prop prop.scarecrow at (26,18))
  [tilda]: "[player]! Just in time. You are my judge. Please be fair, which means pick mine."
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote exclaim)
  · (tilda point (background))
  ▶ DIALOGUE `story.scarecrow.talk` (player-facing graph, plays here):
    n0 [tilda]: "Mine has a smile. Smiles scare crows. That is simply known."   (→ n1)
    n1 [dorian]: "Mine is quiet. Quiet is unsettling. I did my research."  ✏ `dlg.story.scarecrow.talk.1`   (→ n2)
    n2 [tilda]: "Well? Which scarecrow rules the fields?"
       ? "Tilda's." → r0  effects: flag:choice.scarecrow.tilda
       ? "Dorian's." → r1  effects: flag:choice.scarecrow.dorian
       ? "A tie." → r2  effects: flag:choice.scarecrow.tie
       ? "Ask the crows." → r3  effects: flag:choice.scarecrow.crows
    r0 [tilda]: "Ha! A person of taste. Take this one home. It's the runner-up. It's still excellent."   (expr happy)
    r1 [dorian]: "...Oh. Thank you. Take the runner-up. It's very quiet company."  ✏ `dlg.story.scarecrow.talk.r1`   (expr happy)
    r2 [tilda]: "A tie. You coward. Take one anyway. Both of us will be wounded."   (emote sweat)
    r3 [dorian]: "The crows? ...Brave. They landed on both. A tie, then. Take this."  ✏ `dlg.story.scarecrow.talk.r3`   (expr thinking)
  [tilda]: "{if choice:scarecrow.tilda}I won. I'm not smug. I'm radiant.{else}{if choice:scarecrow.dorian}He won. I'll say it once. Never again.{else}{if choice:scarecrow.tie}A tie. I'm not speaking to you, with love.{else}The crows chose. Wisely. They're furious with both of us.{/if}{/if}{/if}"
  · (remove prop story_prop_0)
  · effects: flag:event.story_scarecrows, give:machine.scarecrow,1, friend:tilda,20, friend:dorian,20, flag:storydone.rival_scarecrows

### story_pumpkin_dorian · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:missing_pumpkin.dorian && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: dorian(25, 18), player(25, 17)
  · (place dorian to (25,18))
  · (dorian faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [dorian]: "...The pumpkin was perfect for a mushroom experiment. I meant to ask. I forgot to ask."  ✏ `event.story_pumpkin_dorian.intro`
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote sweat)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.pumpkin.dorian` (player-facing graph, plays here):
    n0 [narration]: "You find Dorian behind the store, with a very large pumpkin and a very guilty face."  ✏ `dlg.story.pumpkin.dorian.0`   (→ n1)
    n1 [tilda]: "Is that my prize pumpkin? The one the whole village has been blaming the goats for?"   (→ n2)
    n2 [dorian]: "...It was for scale. Next to the carp."  ✏ `dlg.story.pumpkin.dorian.2`
       ? "Let them off kindly." → r0  effects: flag:choice.pumpkin.dorian.kind, friend:dorian,10  ✏ `dlg.story.pumpkin.dorian.c0`
       ? "Insist on a proper apology." → r1  effects: flag:choice.pumpkin.dorian.apology  ✏ `dlg.story.pumpkin.dorian.c1`
       ? "Offer to help carry it back." → r2  effects: flag:choice.pumpkin.dorian.help, friend:tilda,10  ✏ `dlg.story.pumpkin.dorian.c2`
    r0 [tilda]: "Kindly? Fine. Kindly. But he's on the slate for a pie."   (expr thinking)
    r1 [dorian]: "An apology. Yes. Very sorry. Truly. Pumpkin, I'm also sorry to you."  ✏ `dlg.story.pumpkin.dorian.r1`   (emote sweat)
    r2 [tilda]: "You carry the heavy end. I'll supervise. We'll call it civic duty."   (expr happy)
  [dorian]: "He's returning it with a mushroom stew inside. Tilda forgives him on the spot. The pumpkin does not."  ✏ `event.story_pumpkin_dorian.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_dorian, friend:dorian,30, friend:tilda,20, flag:storydone.missing_pumpkin

### dorian_heart8 · "The Notebook" · wholesome
conversation · wave3_events.json · map Forest · priority 7
plays when: `hearts:dorian>=8 && flag:event.dorian_heart5 && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...Sit. Here. On the log. I'm going to show you something. Don't speak yet."  ✏ `event.dorian_heart8.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.notebook` (player-facing graph, plays here):
    n0 [narration]: "Dorian takes a small, worn notebook from his coat. He holds it closed for a long moment."  ✏ `dlg.dorian.notebook.0`   (→ n1)
    n1 [dorian]: "My drawings. Mushrooms. Hundreds. You're the first. Please be kind."  ✏ `dlg.dorian.notebook.1`
       ? "Praise them." → r0  effects: friend:dorian,10, flag:choice.notebook.praise  ✏ `dlg.dorian.notebook.c0`
       ? "Ask for one." → r1  effects: friend:dorian,15, flag:choice.notebook.ask  ✏ `dlg.dorian.notebook.c1`
       ? "Draw one back." → r2  effects: friend:dorian,10, flag:choice.notebook.draw  ✏ `dlg.dorian.notebook.c2`
    r0 [dorian]: "...Thank you. Nobody's said. Not once. It's a good word. I'll keep it."  ✏ `dlg.dorian.notebook.r0`   (expr happy)
    r1 [dorian]: "One. For you. Take the best. It's the small brown one. It's me, really."  ✏ `dlg.dorian.notebook.r1`   (expr happy)
    r2 [dorian]: "You drew a mushroom. It has a face. ...I love it. Don't tell anyone."  ✏ `dlg.dorian.notebook.r2`   (expr happy)
  [dorian]: "{if choice:notebook.praise}You praised them. Nobody has. I'll draw a tiny one for you, tomorrow.{else}{if choice:notebook.ask}You asked for one. I tore it out myself. It's the best one. Don't lose it.{else}You drew one back. It's crooked. It's wonderful. I'll keep it in the notebook.{/if}{/if}"  ✏ `event.dorian_heart8.after`
  · effects: flag:event.dorian_heart8
  · effects: friend:dorian,70

### dorian_birthday · "Dorian's Birthday" · wholesome
conversation · wave4_birthdays.json · map Forest · priority 8 · repeatable
plays when: `hearts:dorian>=2 && birthday.in:dorian==0 && unseen:birthday.dorian && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...It's my birthday. I didn't say. Marcus said. I'd rather be in the forest. Stay a minute. Not long."  ✏ `event.dorian_birthday.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.birthday` (player-facing graph, plays here):
    n0 [dorian]: "...You stayed. What now?"  ✏ `dlg.dorian.birthday.0`
       ? "Wish him a happy birthday." → r0  effects: friend:dorian,10, flag:choice.bday.dorian.wish  ✏ `dlg.dorian.birthday.c0`
       ? "Offer a walk in the forest." → r1  effects: friend:dorian,10, flag:choice.bday.dorian.walk  ✏ `dlg.dorian.birthday.c1`
       ? "Give him a pinecone." → r2  effects: friend:dorian,10, flag:choice.bday.dorian.cone  ✏ `dlg.dorian.birthday.c2`
    r0 [dorian]: "...Thank you. It's quiet. Good."  ✏ `dlg.dorian.birthday.r0`   (expr happy)
    r1 [dorian]: "A walk. Yes. Slowly. I'll show you a quiet spot."  ✏ `dlg.dorian.birthday.r1`   (expr happy)
    r2 [dorian]: "A pinecone. From you. ...It's the best one. I'm keeping it."  ✏ `dlg.dorian.birthday.r2`   (expr embarrassed)
  [dorian]: "{if choice:bday.dorian.wish}Quiet words. I kept them.{else}{if choice:bday.dorian.walk}We walked, we said little. It was a good birthday.{else}The pinecone's on my shelf. It's the best one. Not me. You.{/if}{/if}"  ✏ `event.dorian_birthday.after`
  · effects: flag:event.dorian_birthday
  · effects: mark:birthday.dorian, friend:dorian,30, give:forage.mushroom,2, mail:dorian_bday_thanks

### dorian_courtship · "Dorian Asks" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `hearts:dorian>=8 && flag:event.dorian_heart8 && !flag:partners.dorian && !flag:choice.courtship.dorian.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...I've been quiet a long time. I'd like to say one thing, and not take it back. Would you court me? Slowly. In the forest. Quietly."  ✏ `event.dorian_courtship.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.courtship` (player-facing graph, plays here):
    n0 [dorian]: "What do you say?"  ✏ `dlg.dorian.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:dorian,20, flag:choice.courtship.dorian.yes  ✏ `dlg.dorian.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:dorian,15, flag:choice.courtship.dorian.friends  ✏ `dlg.dorian.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:dorian,5, flag:choice.courtship.dorian.later  ✏ `dlg.dorian.courtship.c2`
    r0 [dorian]: "...Yes. I wasn't sure you'd say it. I'll show you the quiet places. All of them. Slowly."  ✏ `dlg.dorian.courtship.r0`   (expr happy)
    r1 [dorian]: "Friends. ...Good. That's a lot, for me. Thank you for saying it kindly."  ✏ `dlg.dorian.courtship.r1`   (expr happy)
    r2 [dorian]: "Wait. Yes. I'm good at waiting. The trees taught me."  ✏ `dlg.dorian.courtship.r2`   (expr thinking)
  [dorian]: "{if flag:choice.courtship.dorian.yes}A walk, tomorrow. No talking until the third tree. After that, maybe.{else}{if flag:choice.courtship.dorian.friends}Friends. That's a good word. I'll keep it, in a quiet place.{else}Take the time. I'll be in the forest. Not hiding. Waiting.{/if}{/if}"  ✏ `event.dorian_courtship.after`
  · effects: flag:event.dorian_courtship
  · effects: flag:courtship.asked.dorian

### dorian_courtship2 · "Dorian Asks Again" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `hearts:dorian>=10 && flag:choice.courtship.dorian.later && !flag:event.dorian_courtship2.done && !flag:partners.dorian && !flag:choice.courtship.dorian.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...I've been quiet a long time. I'd like to say one thing, and not take it back. Would you court me? Slowly. In the forest. Quietly."  ✏ `event.dorian_courtship2.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.courtship` (player-facing graph, plays here):
    n0 [dorian]: "What do you say?"  ✏ `dlg.dorian.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:dorian,20, flag:choice.courtship.dorian.yes  ✏ `dlg.dorian.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:dorian,15, flag:choice.courtship.dorian.friends  ✏ `dlg.dorian.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:dorian,5, flag:choice.courtship.dorian.later  ✏ `dlg.dorian.courtship.c2`
    r0 [dorian]: "...Yes. I wasn't sure you'd say it. I'll show you the quiet places. All of them. Slowly."  ✏ `dlg.dorian.courtship.r0`   (expr happy)
    r1 [dorian]: "Friends. ...Good. That's a lot, for me. Thank you for saying it kindly."  ✏ `dlg.dorian.courtship.r1`   (expr happy)
    r2 [dorian]: "Wait. Yes. I'm good at waiting. The trees taught me."  ✏ `dlg.dorian.courtship.r2`   (expr thinking)
  [dorian]: "{if flag:choice.courtship.dorian.yes}A walk, tomorrow. No talking until the third tree. After that, maybe.{else}{if flag:choice.courtship.dorian.friends}Friends. That's a good word. I'll keep it, in a quiet place.{else}Take the time. I'll be in the forest. Not hiding. Waiting.{/if}{/if}"  ✏ `event.dorian_courtship2.after`
  · effects: flag:event.dorian_courtship2
  · effects: flag:courtship.asked.dorian, flag:event.dorian_courtship2.done

### dorian_partners · "Dorian, Together" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `hearts:dorian>=10 && flag:choice.courtship.dorian.yes && !flag:partners.dorian && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...I made something. It isn't much. A feather, a stick, and a bit of twine. It's for you. Sit. Hold it."  ✏ `event.dorian_partners.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote heart)
  ▶ DIALOGUE `dorian.partners` (player-facing graph, plays here):
    n0 [dorian]: "...An evening. Ours. What shall we do?"  ✏ `dlg.dorian.partners.0`
       ? "Hold the feather." → r0  effects: friend:dorian,10, flag:choice.courtship.dorian.feather  ✏ `dlg.dorian.partners.c0`
       ? "Walk the forest with him." → r1  effects: friend:dorian,10, flag:choice.courtship.dorian.walk  ✏ `dlg.dorian.partners.c1`
       ? "Sit quietly beside him." → r2  effects: friend:dorian,10, flag:choice.courtship.dorian.sit  ✏ `dlg.dorian.partners.c2`
    r0 [dorian]: "...It's small. It's soft. It's yours. I found it the day I met you. I kept it for later."  ✏ `dlg.dorian.partners.r0`   (expr happy)
    r1 [dorian]: "The forest, with you. I'll show you the oak. It's old. It's quiet. It's the best listener I know."  ✏ `dlg.dorian.partners.r1`   (expr happy)
    r2 [dorian]: "...Yes. This. Sit. We don't need words. We never did. Thank you."  ✏ `dlg.dorian.partners.r2`   (expr happy)
  [dorian]: "{if choice:courtship.dorian.feather}The feather's on my shelf. Next to the pinecone. They're a set. You're the third.{else}{if choice:courtship.dorian.walk}We walked until the light went amber. I said six words. They were all good.{else}A quiet hour, beside you. It's the best thing I own. Don't tell the trees.{/if}{/if}"  ✏ `event.dorian_partners.after`
  · effects: flag:event.dorian_partners
  · effects: flag:partners.dorian, give:prop.feather, friend:dorian,50

### overheard_marcus_dorian_1 · "The Straight Plank" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=2 && hearts:dorian>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. Hold the end. No, the other end. There."
  [dorian]: "...Straight."  ✏ `event.overheard_marcus_dorian_1.l2`
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus point (background))
  [marcus]: "Straight. That's the whole job. I couldn't have said it better."
  [dorian]: "...You could have. You didn't. Good."  ✏ `event.overheard_marcus_dorian_1.l4`
  · effects: flag:event.overheard_marcus_dorian_1

### overheard_marcus_dorian_2 · "Sawdust Silence" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=4 && hearts:dorian>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_1`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "It's been an hour. You haven't said anything."
  [dorian]: "...Neither have you."  ✏ `event.overheard_marcus_dorian_2.l2`
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "I was getting worried. No, I wasn't. I was enjoying it."
  [dorian]: "...Me too. Hand me the plane."  ✏ `event.overheard_marcus_dorian_2.l4`
  · effects: flag:event.overheard_marcus_dorian_2

### overheard_marcus_dorian_3 · "The Forest Door" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=6 && hearts:dorian>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_2`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "I made a door for your room. It's oak. It's heavier than it looks."
  [dorian]: "...It's good. It's the first door I've owned that I'd close on purpose."  ✏ `event.overheard_marcus_dorian_3.l2`
  · (dorian point (background))
  · (dorian emote ellipsis)
  · (marcus shrug (background))
  [marcus]: "That's the whole idea of a door."
  [dorian]: "...Thank you. It's a very good door. I'll keep it open. Mostly."  ✏ `event.overheard_marcus_dorian_3.l4`
  · effects: flag:event.overheard_marcus_dorian_3

### overheard_marcus_dorian_4 · "The Hut Window" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.marcus && flag:met.dorian && hearts:marcus>=8 && hearts:dorian>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_3`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. I built a window for your room. Square. Oak."
  [dorian]: "...A window. Facing the trees."  ✏ `event.overheard_marcus_dorian_4.l2`
  · (dorian wave (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "Facing the owl. You can say hello."
  [dorian]: "...Thank you. The owl will want a name."  ✏ `event.overheard_marcus_dorian_4.l4`
  · effects: flag:event.overheard_marcus_dorian_4

### overheard_dorian_hazel_1 · "The Quiet Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.dorian && flag:met.hazel && hearts:dorian>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...Hazel."  ✏ `event.overheard_dorian_hazel_1.l1`
  [hazel]: "(Dorian.) Good morning. Sit, if you like."
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian point (background))
  [dorian]: "...Quiet today."  ✏ `event.overheard_dorian_hazel_1.l3`
  [hazel]: "(The best kind.) Stay as long as you need."
  · effects: flag:event.overheard_dorian_hazel_1

### overheard_dorian_hazel_2 · "Forage and Book" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.dorian && flag:met.hazel && hearts:dorian>=4 && hearts:hazel>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_dorian_hazel_1`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...I found a mushroom guide. Pressed leaves inside."  ✏ `event.overheard_dorian_hazel_2.l1`
  [hazel]: "(Pressed leaves.) May I borrow it? (I'll treat them as bookmarks.)"
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian wave (background))
  [dorian]: "...They'd like that. They like being kept."  ✏ `event.overheard_dorian_hazel_2.l3`
  [hazel]: "(Then we agree.) I'll return it, with a leaf of my own."
  · effects: flag:event.overheard_dorian_hazel_2

### overheard_dorian_hazel_3 · "Two Silences" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.dorian && flag:met.hazel && hearts:dorian>=6 && hearts:hazel>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_dorian_hazel_2`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...We could just sit."  ✏ `event.overheard_dorian_hazel_3.l1`
  [hazel]: "(We could.) The library window faces the forest."
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (dorian shrug (background))
  [dorian]: "...Then the trees can join us."  ✏ `event.overheard_dorian_hazel_3.l3`
  [hazel]: "(They always do.) I'll keep a chair warm for you."
  · effects: flag:event.overheard_dorian_hazel_3
