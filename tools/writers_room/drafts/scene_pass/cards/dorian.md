# Scene cards for dorian

18 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 38 spoken lines in scenes, 28 dialogue lines and 18 player choice labels.

### dorian_heart2 · "Shh, Look" · wholesome
event · npc_roster.json · map Forest · priority 6
plays when: `(hearts:dorian>=2 && hour>=8 && hour<17) && !flag:event.dorian_heart2`
beats may be ADDED or CUT in this scene
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "Shh. Stop. Stand in my footprints. Look."  ✏ `event.dorian_heart2.0`
  · (move player to (19,11))
  · (player faces up)
  · (dorian emote ellipsis)
  [dorian]: "A fox. By the fern. Don't move. She's deciding about you."  ✏ `event.dorian_heart2.1`
  [dorian]: "Three breaths, she stayed. Foxes give you one. You stand like a tree."  ✏ `event.dorian_heart2.3`  (expr happy)
  [dorian]: "These grew where she watched you. Take them. Not me. The forest."  ✏ `event.dorian_heart2.2`
  [dorian]: "There's another place. Deeper in. Not yet. The moss is still deciding."  ✏ `event.dorian_heart2.4`  (expr thinking)
  · effects: give:forage.mushroom,3, friend:dorian,40, flag:event.dorian_heart2

### dorian_heart5 · "The Secret Spot" · surprise
event · npc_roster.json · map Forest · priority 7
plays when: `(hearts:dorian>=5 && hour>=8 && hour<17 && flag:event.dorian_heart2) && !flag:event.dorian_heart5`
beats may be ADDED or CUT in this scene
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "Step over the root. Not on the moss. This is my spot."  ✏ `event.dorian_heart5.0`
  · (move player to (19,11))
  · (player faces up)
  · (dorian emote ellipsis)
  [dorian]: "I've never shown anyone. Edmund found it alone. I didn't mind. Odd."  ✏ `event.dorian_heart5.1`
  [dorian]: "He was counting the trees. I never asked how far he got."  ✏ `event.dorian_heart5.3`  (expr thinking)
  [dorian]: "Tell nobody. Not even Marcus. The truffle under the oak is yours."  ✏ `event.dorian_heart5.2`
  · effects: give:forage.truffle,1, friend:dorian,60, flag:event.dorian_heart5

### reading_night · "Reading Night" · wholesome
event · spec_scenes.json · map Library · priority 7
plays when: `(flag:choice.reading.yes && flag:event.hazel_heart10 && flag:event.ione_heart8 && flag:met.dorian && hour>=17 && hour<21 && !weekday:sat && !festival.in:==0) && !flag:event.reading_night`
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
  [ione]: "{if flag:met.wren && hearts:wren>=3}Five{else}Four{/if} chairs, one lamp, and a pot of tea nobody asked for. This is my calm face. Does it work?"
  [hazel]: "(It doesn't.) There's a poem in my sleeve. (It's been there three days. Deciding.)"  (expr embarrassed)
  ▶ DIALOGUE `reading.hazel` (player-facing graph, plays here):
    n0 [hazel]: "(Nobody has to hear it.) What would you do? ...If you were me, I mean."
       ? "Read yours. We're all listening." → r0  effects: flag:choice.reading.poem, friend:hazel,10
       ? "Read something borrowed tonight." → r1  effects: flag:choice.reading.borrowed, friend:hazel,10
       ? "Listen first. Ione's face needs the practice." → r2  effects: flag:choice.reading.listen, friend:hazel,10
    r0 [hazel]: "(Fine.) A short one. Eleven lines. (I counted twice. It's still eleven.)"
    r1 [hazel]: "Borrowed, then. Kinder to the room. (Mine keeps my sleeve warm another night.)"
    r2 [hazel]: "Ione first. (Thank you.) I'm not leaving. I'm only sitting very still."
  [hazel]: "The lamp does not mind the dark. It keeps it company. ...That's the last of the eleven. (The rest were warming up.)"  (expr thinking) [only if flag:choice.reading.poem]
  [ione]: "I'll copy that out by hand, Hazel. Not for a shelf. For the desk drawer, where the good things go." [only if flag:choice.reading.poem]
  [narration]: "Hazel reads two borrowed pages about the winter sea, quietly, all the way through. Her sleeve stays shut." [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and does not leave." [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "A snowdrop. Pressed since winter. It kept my place. Now it keeps the lamp."  ✏ `event.reading_night.2`
  [wren]: "Don't mind me, I'm off duty. I came to hear one story I'm not allowed to improve. It's harder than it sounds." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "A snowdrop, for the lamp. ...Every chair taken. Next time, one more chair, and the door left open."  (expr happy)
  · (remove prop flower)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.reading_night, friend:ione,40, friend:hazel,40, friend:dorian,40

### story_notes_dorian · "The Note Writer" · wholesome
event · storyline_events.json · map Village · priority 7
plays when: `(flag:notes.began && storyline:anonymous_notes.dorian && hearts:dorian>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_notes_dorian`
beats may be ADDED or CUT in this scene
placed: dorian(25, 18), player(25, 17)
  · (place dorian to (25,18))
  · (dorian faces down)
  · (spawn prop prop.note at (26,18))
  [dorian]: "Oh. ...The note was meant to do this part alone. Paper doesn't look back."  ✏ `event.story_notes_dorian.intro`
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote ellipsis)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.notes.dorian` (player-facing graph, plays here):
    n0 [narration]: "A folded note sits half under the door. Dorian's hand is still on it."  ✏ `dlg.story.notes.dorian.0`   (→ n1)
    n1 [dorian]: "...So. Now you know who."  ✏ `dlg.story.notes.dorian.1`
       ? "Smile. Say nothing." → r0  effects: flag:choice.notes.dorian.smile  ✏ `dlg.story.notes.dorian.c0`
       ? "Thank him." → r1  effects: flag:choice.notes.dorian.thanks, friend:dorian,10  ✏ `dlg.story.notes.dorian.c1`
       ? "Slip a note back under his door." → r2  effects: flag:choice.notes.dorian.note, friend:dorian,15  ✏ `dlg.story.notes.dorian.c2`
    r0 [dorian]: "...A smile. No words. You speak my language."  ✏ `dlg.story.notes.dorian.r0`   (expr embarrassed)
    r1 [dorian]: "...Don't mention it. Truly. Don't. I'll be behind a tree."  ✏ `dlg.story.notes.dorian.r1`   (expr happy)
    r2 [dorian]: "For me? ...I'll read it in the forest. Twice."  ✏ `dlg.story.notes.dorian.r2`   (expr happy)
  [dorian]: "Three more doors before dark. Don't follow. Don't guess."  ✏ `event.story_notes_dorian.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_notes_dorian, friend:dorian,40, flag:storydone.anonymous_notes

### story_scarecrows · "The Scarecrow Contest" · funny
event · storyline_events.json · map Village · priority 7
plays when: `(storyline:rival_scarecrows && season:fall && weekday:sat && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_scarecrows`
beats may be ADDED or CUT in this scene
placed: tilda(25, 18), player(25, 17)
  · (place tilda to (25,18))
  · (tilda faces down)
  · (spawn prop prop.scarecrow at (26,18))
  [tilda]: "[player]! You're judging. Be completely fair, by which I mean pick mine."
  · (move player to (25,17))
  · (player faces up)
  · (tilda emote exclaim)
  · (tilda point (background))
  ▶ DIALOGUE `story.scarecrow.talk` (player-facing graph, plays here):
    n0 [tilda]: "Mine is smiling. Crows can't stand a cheerful face. That's simply known."   (→ n1)
    n1 [dorian]: "Mine just stands there. Silent. Crows hate silence. I took notes."  ✏ `dlg.story.scarecrow.talk.1`   (→ n2)
    n2 [tilda]: "Go on, judge. One scarecrow gets the field. The other guards my rhubarb."
       ? "Tilda's, obviously." → r0  effects: flag:choice.scarecrow.tilda
       ? "Dorian's. Sorry, Tilda." → r1  effects: flag:choice.scarecrow.dorian
       ? "It's a tie." → r2  effects: flag:choice.scarecrow.tie
       ? "Let the crows decide." → r3  effects: flag:choice.scarecrow.crows
    r0 [tilda]: "Ha! Taste! Take the runner-up home. It'll stand in your field being thoughtful at the crows."   (expr happy)
    r1 [dorian]: "...Mine. Oh. ...Take the runner-up home. We're both embarrassed now."  ✏ `dlg.story.scarecrow.talk.r1`   (expr happy)
    r2 [tilda]: "A tie. That word costs extra in here. Take one anyway, and we'll both be wounded together."   (emote sweat)
    r3 [dorian]: "The crows voted already. Four sat on each. A tie. Take one."  ✏ `dlg.story.scarecrow.talk.r3`   (expr thinking)
  [tilda]: "{if choice:scarecrow.tilda}I won. I'm not smug. I'm radiant. Rematch next fall.{else}{if choice:scarecrow.dorian}He won. I'll say it once, quietly. Rematch next fall.{else}{if choice:scarecrow.tie}A tie. I'm not speaking to you, with love. Rematch next fall.{else}The crows chose both. Wise birds. They're on the slate for diplomacy.{/if}{/if}{/if}"
  · (remove prop story_prop_0)
  · effects: flag:event.story_scarecrows, give:machine.scarecrow,1, friend:tilda,20, friend:dorian,20, flag:storydone.rival_scarecrows

### story_pumpkin_dorian · "The Missing Pumpkin" · funny
event · storyline_events2.json · map Village · priority 7
plays when: `(storyline:missing_pumpkin.dorian && hearts:tilda>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_pumpkin_dorian`
beats may be ADDED or CUT in this scene
placed: dorian(25, 18), player(25, 17)
  · (place dorian to (25,18))
  · (dorian faces down)
  · (spawn prop prop.pumpkin at (26,18))
  [dorian]: "...The inside was the right dark. For spores. Asking needed words. I ran out."  ✏ `event.story_pumpkin_dorian.intro`
  · (move player to (25,17))
  · (player faces up)
  · (dorian emote sweat)
  · (dorian shrug (background))
  ▶ DIALOGUE `story.pumpkin.dorian` (player-facing graph, plays here):
    n0 [narration]: "Behind the store, the prize pumpkin. Dorian stands beside it, very still."  ✏ `dlg.story.pumpkin.dorian.0`   (→ n1)
    n1 [tilda]: "Dorian. Is that my prize pumpkin? I've been blaming the goats all week, out loud!"   (→ n2)
    n2 [dorian]: "...Borrowed. The mushrooms needed a roof. It was coming back. Mostly."  ✏ `dlg.story.pumpkin.dorian.2`
       ? "Let him off gently." → r0  effects: flag:choice.pumpkin.dorian.kind, friend:dorian,10  ✏ `dlg.story.pumpkin.dorian.c0`
       ? "Ask for a proper apology." → r1  effects: flag:choice.pumpkin.dorian.apology  ✏ `dlg.story.pumpkin.dorian.c1`
       ? "Grab the heavy end. Carry it back." → r2  effects: flag:choice.pumpkin.dorian.help, friend:tilda,10  ✏ `dlg.story.pumpkin.dorian.c2`
    r0 [tilda]: "Kindly, then. But he's on the slate for a pie. Mushrooms optional."   (expr thinking)
    r1 [dorian]: "Sorry, Tilda. Truly. ...Sorry, pumpkin. You were very patient."  ✏ `dlg.story.pumpkin.dorian.r1`   (emote sweat)
    r2 [tilda]: "You take the heavy end, Dorian takes the guilt. I'll carry the conversation."   (expr happy)
  [dorian]: "Next fall I'll grow my own. Tilda may forgive me. The pumpkin won't."  ✏ `event.story_pumpkin_dorian.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_pumpkin_dorian, friend:dorian,30, friend:tilda,20, flag:storydone.missing_pumpkin

### dorian_heart8 · "The Notebook" · wholesome
conversation · wave3_events.json · map Forest · priority 7
plays when: `(hearts:dorian>=8 && flag:event.dorian_heart5 && hour>=8 && hour<17) && !flag:event.dorian_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "Sit. The log. ...I practiced this part. Don't speak yet."  ✏ `event.dorian_heart8.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.notebook` (player-facing graph, plays here):
    n0 [narration]: "Dorian takes a small, worn notebook from his coat. He holds it shut a long moment."  ✏ `dlg.dorian.notebook.0`   (→ n1)
    n1 [dorian]: "Mushrooms. Four hundred and twelve. Drawn. Nobody's seen them. ...Be gentle."  ✏ `dlg.dorian.notebook.1`
       ? "Tell him they're lovely." → r0  effects: friend:dorian,10, flag:choice.notebook.praise  ✏ `dlg.dorian.notebook.c0`
       ? "Ask to keep one." → r1  effects: friend:dorian,15, flag:choice.notebook.ask  ✏ `dlg.dorian.notebook.c1`
       ? "Draw one back. Give it a face." → r2  effects: friend:dorian,10, flag:choice.notebook.draw  ✏ `dlg.dorian.notebook.c2`
    r0 [dorian]: "Lovely. First time for that word. It goes in the margin."  ✏ `dlg.dorian.notebook.r0`   (expr happy)
    r1 [dorian]: "One. Take the small brown one, under the fern. It's me, mostly."  ✏ `dlg.dorian.notebook.r1`   (expr happy)
    r2 [dorian]: "It has a face. Mushrooms don't. ...This one does now. I love it."  ✏ `dlg.dorian.notebook.r2`   (expr happy)
  [dorian]: "{if choice:notebook.praise}You said lovely. The next page is yours. I'll draw you as a mushroom.{else}{if choice:notebook.ask}The brown one's yours. Don't lose it. I'll miss it. A little.{else}Your mushroom lives in the notebook now. Page one. Crooked. Perfect.{/if}{/if}"  ✏ `event.dorian_heart8.after`
  · effects: flag:event.dorian_heart8
  · effects: friend:dorian,70

### dorian_birthday · "Dorian's Birthday" · wholesome
conversation · wave4_birthdays.json · map Forest · priority 8 · repeatable
plays when: `hearts:dorian>=2 && birthday.in:dorian==0 && unseen:birthday.dorian && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...My birthday. Marcus told the whole saloon. The trees won't mention it. Stay a minute. Not long."  ✏ `event.dorian_birthday.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.birthday` (player-facing graph, plays here):
    n0 [dorian]: "...You stayed. What now?"  ✏ `dlg.dorian.birthday.0`
       ? "Wish him a quiet happy birthday." → r0  effects: friend:dorian,10, flag:choice.bday.dorian.wish  ✏ `dlg.dorian.birthday.c0`
       ? "Offer a slow walk in the forest." → r1  effects: friend:dorian,10, flag:choice.bday.dorian.walk  ✏ `dlg.dorian.birthday.c1`
       ? "Hand him a perfect pinecone." → r2  effects: friend:dorian,10, flag:choice.bday.dorian.cone  ✏ `dlg.dorian.birthday.c2`
    r0 [dorian]: "...Thank you. Said quietly. That's the right size."  ✏ `dlg.dorian.birthday.r0`   (expr happy)
    r1 [dorian]: "Good. The moss past the second ridge. It's waited all year."  ✏ `dlg.dorian.birthday.r1`   (expr happy)
    r2 [dorian]: "Closed tight. Perfect. ...I'm keeping it."  ✏ `dlg.dorian.birthday.r2`   (expr embarrassed)
  [dorian]: "{if choice:bday.dorian.wish}Quiet words. I kept them. They fit in a pocket.{else}{if choice:bday.dorian.walk}We walked. Said little. Good birthday. Marcus can stop asking.{else}The pinecone's on my shelf. Not from the forest. From you.{/if}{/if}"  ✏ `event.dorian_birthday.after`
  · effects: flag:event.dorian_birthday
  · effects: mark:birthday.dorian, friend:dorian,30, give:forage.mushroom,2, mail:dorian_bday_thanks

### dorian_courtship · "Dorian Asks" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `(hearts:dorian>=8 && flag:event.dorian_heart8 && !flag:partners.dorian && !flag:choice.courtship.dorian.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && hour>=8 && hour<17) && !flag:event.dorian_courtship`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...I've practiced this since spring. The trees heard it first. Will you court me? Slowly. Here. Quietly."  ✏ `event.dorian_courtship.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.courtship` (player-facing graph, plays here):
    n0 [dorian]: "What do you say?"  ✏ `dlg.dorian.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:dorian,20, flag:choice.courtship.dorian.yes  ✏ `dlg.dorian.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:dorian,15, flag:choice.courtship.dorian.friends  ✏ `dlg.dorian.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:dorian,5, flag:choice.courtship.dorian.later  ✏ `dlg.dorian.courtship.c2`
    r0 [dorian]: "...Yes. ...I asked the oak how to say it. It didn't help. All the quiet places are yours."  ✏ `dlg.dorian.courtship.r0`   (expr happy)
    r1 [dorian]: "Friends. ...Good. That's a lot, for me. You said it gently."  ✏ `dlg.dorian.courtship.r1`   (expr happy)
    r2 [dorian]: "Wait. Yes. I'm good at waiting. Ask any oak."  ✏ `dlg.dorian.courtship.r2`   (expr thinking)
  [dorian]: "{if flag:choice.courtship.dorian.yes}Tomorrow, a walk. No talking until the third tree. After, maybe.{else}{if flag:choice.courtship.dorian.friends}Friends. Good word. I'll keep it somewhere mossy.{else}Take your time. I'll be where the moss is. Not hiding.{/if}{/if}"  ✏ `event.dorian_courtship.after`
  · effects: flag:event.dorian_courtship
  · effects: flag:courtship.asked.dorian

### dorian_courtship2 · "Dorian Asks Again" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `hearts:dorian>=10 && flag:choice.courtship.dorian.later && !flag:event.dorian_courtship2.done && !flag:partners.dorian && !flag:choice.courtship.dorian.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && hour>=8 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "...You said wait. The oak and I practiced. Once more, then. Will you court me? Slowly. Quietly."  ✏ `event.dorian_courtship2.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote ellipsis)
  ▶ DIALOGUE `dorian.courtship` (player-facing graph, plays here):
    n0 [dorian]: "What do you say?"  ✏ `dlg.dorian.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:dorian,20, flag:choice.courtship.dorian.yes  ✏ `dlg.dorian.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:dorian,15, flag:choice.courtship.dorian.friends  ✏ `dlg.dorian.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:dorian,5, flag:choice.courtship.dorian.later  ✏ `dlg.dorian.courtship.c2`
    r0 [dorian]: "...Yes. ...I asked the oak how to say it. It didn't help. All the quiet places are yours."  ✏ `dlg.dorian.courtship.r0`   (expr happy)
    r1 [dorian]: "Friends. ...Good. That's a lot, for me. You said it gently."  ✏ `dlg.dorian.courtship.r1`   (expr happy)
    r2 [dorian]: "Wait. Yes. I'm good at waiting. Ask any oak."  ✏ `dlg.dorian.courtship.r2`   (expr thinking)
  [dorian]: "{if flag:choice.courtship.dorian.yes}Tomorrow, a walk. No talking until the third tree. After, maybe.{else}{if flag:choice.courtship.dorian.friends}Friends. Good word. I'll keep it somewhere mossy.{else}Longer, then. The oak's patient. I'm learning from it.{/if}{/if}"  ✏ `event.dorian_courtship2.after`
  · effects: flag:event.dorian_courtship2
  · effects: flag:courtship.asked.dorian, flag:event.dorian_courtship2.done

### dorian_partners · "Dorian, Together" · wholesome
conversation · wave5_courtship.json · map Forest · priority 8
plays when: `(hearts:dorian>=10 && flag:choice.courtship.dorian.yes && !flag:partners.dorian && hour>=8 && hour<17) && !flag:event.dorian_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(19, 11)
  · (dorian faces down)
  [dorian]: "I made this. A jay feather, willow, twine. It took three tries. Hold it."  ✏ `event.dorian_partners.intro`
  · (move player to (19,11))
  · (player faces up)
  · (dorian wave (background))
  · (dorian emote heart)
  ▶ DIALOGUE `dorian.partners` (player-facing graph, plays here):
    n0 [dorian]: "Today's ours. I didn't plan past this."  ✏ `dlg.dorian.partners.0`
       ? "Hold the feather carefully." → r0  effects: friend:dorian,10, flag:choice.courtship.dorian.feather  ✏ `dlg.dorian.partners.c0`
       ? "Walk the forest with him." → r1  effects: friend:dorian,10, flag:choice.courtship.dorian.walk  ✏ `dlg.dorian.partners.c1`
       ? "Sit. Say nothing. Together." → r2  effects: friend:dorian,10, flag:choice.courtship.dorian.sit  ✏ `dlg.dorian.partners.c2`
    r0 [dorian]: "Soft, isn't it. I found it the day we met. Kept it. For later."  ✏ `dlg.dorian.partners.r0`   (expr happy)
    r1 [dorian]: "The forest, then. Past the third tree this time. The old oak listens."  ✏ `dlg.dorian.partners.r1`   (expr happy)
    r2 [dorian]: "...Yes. This. We never needed words. Still don't."  ✏ `dlg.dorian.partners.r2`   (expr happy)
  [dorian]: "{if choice:courtship.dorian.feather}The feather lives by my notebook now. Two good things. Three, with you.{else}{if choice:courtship.dorian.walk}We walked until the light went amber. I said six words. All good ones.{else}An hour beside you. Best thing I own. Don't tell the trees.{/if}{/if}"  ✏ `event.dorian_partners.after`
  · effects: flag:event.dorian_partners
  · effects: flag:partners.dorian, give:prop.feather, friend:dorian,50

### overheard_marcus_dorian_1 · "The Straight Plank" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.dorian && hearts:marcus>=2 && hearts:dorian>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_marcus_dorian_1`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. Hold the end. No, the other end. ...There."
  [dorian]: "Straight. Like a birch."  ✏ `event.overheard_marcus_dorian_1.l2`
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus point (background))
  [marcus]: "Straight. That's the whole job. I couldn't have said it shorter."
  [dorian]: "...You could have. You saved it for me."  ✏ `event.overheard_marcus_dorian_1.l4`
  · effects: flag:event.overheard_marcus_dorian_1

### overheard_marcus_dorian_2 · "Sawdust Silence" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.dorian && hearts:marcus>=4 && hearts:dorian>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_1) && !flag:event.overheard_marcus_dorian_2`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "An hour, and you haven't said a word."
  [dorian]: "...Neither have you."  ✏ `event.overheard_marcus_dorian_2.l2`
  · (dorian shrug (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "I nearly got worried. Then I didn't. Best hour of the week."
  [dorian]: "Me too. Plane, please. That's my words for the week."  ✏ `event.overheard_marcus_dorian_2.l4`
  · effects: flag:event.overheard_marcus_dorian_2

### overheard_marcus_dorian_3 · "The Forest Door" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.dorian && hearts:marcus>=6 && hearts:dorian>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_2) && !flag:event.overheard_marcus_dorian_3`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "I hung a new door on your room. Oak. Heavier than it looks, like most things."
  [dorian]: "Oak. Good. First door I'd close on purpose."  ✏ `event.overheard_marcus_dorian_3.l2`
  · (dorian point (background))
  · (dorian emote ellipsis)
  · (marcus shrug (background))
  [marcus]: "That's all a door is for. Closing on purpose."
  [dorian]: "...It's a very good door. I'll leave it open. Mostly."  ✏ `event.overheard_marcus_dorian_3.l4`
  · effects: flag:event.overheard_marcus_dorian_3

### overheard_marcus_dorian_4 · "The Hut Window" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.marcus && flag:met.dorian && hearts:marcus>=8 && hearts:dorian>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_marcus_dorian_3) && !flag:event.overheard_marcus_dorian_4`
text-only scene: rewrite lines, do not add or cut beats
placed: marcus(24, 18), dorian(26, 18)
  · (place marcus to (24,18))
  · (place dorian to (26,18))
  · (marcus faces right)
  · (dorian faces left)
  [marcus]: "Dorian. I built a window for your room. Square. Oak. Measured three times."
  [dorian]: "...Toward the trees? Good. The light comes in green there."  ✏ `event.overheard_marcus_dorian_4.l2`
  · (dorian wave (background))
  · (dorian emote ellipsis)
  · (marcus wave (background))
  [marcus]: "Facing the owl. You two can finally be introduced."
  [dorian]: "The owl will want a name. I'll wait till it tells me."  ✏ `event.overheard_marcus_dorian_4.l4`
  · effects: flag:event.overheard_marcus_dorian_4

### overheard_dorian_hazel_1 · "The Quiet Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.dorian && flag:met.hazel && hearts:dorian>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_dorian_hazel_1`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...Hazel."  ✏ `event.overheard_dorian_hazel_1.l1`
  [hazel]: "Dorian. (A whole word today. Chatty.) The bench has room, if you like."
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian point (background))
  [dorian]: "...Quiet today. Even the jays are reading."  ✏ `event.overheard_dorian_hazel_1.l3`
  [hazel]: "(The best kind.) I brought two books. One is for not reading."
  · effects: flag:event.overheard_dorian_hazel_1

### overheard_dorian_hazel_2 · "Forage and Book" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.dorian && flag:met.hazel && hearts:dorian>=4 && hearts:hazel>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_dorian_hazel_1) && !flag:event.overheard_dorian_hazel_2`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "My old mushroom guide. Pressed leaves inside. Nine falls of them."  ✏ `event.overheard_dorian_hazel_2.l1`
  [hazel]: "Pressed leaves. May I borrow it? (This year's found bookmarks: two receipts, a sock, one apology.)"
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian wave (background))
  [dorian]: "...They'd like that. Leaves like being kept."  ✏ `event.overheard_dorian_hazel_2.l3`
  [hazel]: "Then we agree. I'll return it with one leaf more. (Maple. From the library step.)"
  · effects: flag:event.overheard_dorian_hazel_2

### overheard_dorian_hazel_3 · "Two Silences" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.dorian && flag:met.hazel && hearts:dorian>=6 && hearts:hazel>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_dorian_hazel_2) && !flag:event.overheard_dorian_hazel_3`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...We could just sit. Not talk."  ✏ `event.overheard_dorian_hazel_3.l1`
  [hazel]: "We could. (The library window faces the forest. The seat there is usually free.)"
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (dorian shrug (background))
  [dorian]: "Then the trees can come too. They invented not talking."  ✏ `event.overheard_dorian_hazel_3.l3`
  [hazel]: "(They always come.) I'll save the window seat. You bring the quiet."
  · effects: flag:event.overheard_dorian_hazel_3
