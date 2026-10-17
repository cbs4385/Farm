# Scene cards for ione

15 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 32 spoken lines in scenes, 17 dialogue lines and 12 player choice labels.

### ione_heart2 · "What Ione Found" · wholesome
event · npc_roster_core.json · map Library · priority 6
plays when: `hearts:ione>=2 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Oh, [player]! Come see what I found."  ✏ `event.ione_heart2.0`
  · (move player to (7,4))
  · (player faces up)
  · (ione emote note)
  [ione]: "An old chart of the coast. The ink is nearly gone, but look."  ✏ `event.ione_heart2.1`
  [ione]: "I thought you'd appreciate it."  ✏ `event.ione_heart2.2`
  · effects: give:forage.seashell,2, friend:ione,40, flag:event.ione_heart2

### ione_heart5 · "A Question" · wholesome
event · npc_roster_core.json · map Library · priority 6
plays when: `hearts:ione>=5 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (ione faces down)
  [ione]: "I wanted to ask you something."  ✏ `event.ione_heart5.0`
  · (move player to (7,4))
  · (player faces up)
  · (ione emote note)
  [ione]: "Would you walk the beach with me one evening? Only if you'd like."  ✏ `event.ione_heart5.1`
  [ione]: "Thank you. It means more than you know."  ✏ `event.ione_heart5.2`
  · effects: give:forage.pearl,1, friend:ione,60, flag:event.ione_heart5

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
  [ione]: "Four chairs, one lamp, and tea nobody ordered. This is my not-nervous face. Does it work?"  ✏ `event.reading_night.0`
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
  [ione]: "That is a good poem. I won't say how many times I'll read it. Once. Many times."  ✏ `event.reading_night.poem_after` [only if flag:choice.reading.poem]
  [narration]: "Ione reads two pages about the winter sea, and Hazel listens with her eyes shut. Hers can wait." [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and stays." [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "I brought a flower. Pressed. For the lamp."
  [wren]: "I'll bridge it, friend. Everything good was told aloud first. Then somebody wrote it down so it would stay." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "That is a night of readers. I counted twice, hoping for more. I will count again."  ✏ `event.reading_night.4`  (expr happy)
  · (remove prop flower)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.reading_night, friend:ione,40, friend:hazel,40, friend:dorian,40

### festival_winter_lanterns · "The Lantern Release" · wholesome
event · wave7_festival_moments.json · map Village · priority 6 · repeatable
plays when: `season:winter && day==25 && flag:festival.winter.joined && unseen:festival.winter.lanterns && hour>=17`
beats may be ADDED or CUT in this scene
placed: hazel(23, 18), ione(24, 18), tilda(26, 18), felix(27, 18)
  · (place hazel to (23,18))
  · (hazel faces up)
  · (place ione to (24,18))
  · (ione faces up)
  · (place tilda to (26,18))
  · (tilda faces up)
  · (place felix to (27,18))
  · (felix faces up)
  · (spawn prop prop.lantern at (23,17))
  · (spawn prop prop.lantern at (24,17))
  · (spawn prop prop.lantern at (26,17))
  · (spawn prop prop.lantern at (27,17))
  [tilda]: "Everyone has a lantern, dear. Hold it up. Don't light it yet. Wait for the bell."
  [hazel]: "(One lantern each.) (A small light, and a small wish.) Please keep yours private."
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "It's this big. The wish, I mean. I'll say no more. The lantern will do the telling."
  [ione]: "Now. Together, and slowly. Let them go. The dark will do the rest."  ✏ `event.festival_winter_lanterns.3`
  · (ione emote heart)
  · (remove prop story_prop_3)
  · (remove prop story_prop_2)
  · (remove prop story_prop_1)
  · (remove prop story_prop_0)
  · effects: mark:festival.winter.lanterns, flag:event.festival_winter_lanterns, friend:hazel,10, friend:ione,10, friend:tilda,10, friend:felix,10

### ione_heart8 · "The Set-Aside Shelf" · wholesome
conversation · wave3_events.json · map Library · priority 7
plays when: `hearts:ione>=8 && flag:event.ione_heart5 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Come with me. There's a shelf at the back that I haven't shown anyone. It's quiet. It's a little sad. It's mine."  ✏ `event.ione_heart8.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote sparkle)
  ▶ DIALOGUE `ione.shelf` (player-facing graph, plays here):
    n0 [narration]: "At the back of the library stands a shelf of books, each with a small slip of paper tucked in. None has a name."  ✏ `dlg.ione.shelf.0`   (→ n1)
    n1 [ione]: "Books I set aside for people who never came. I couldn't bear to shelve them. What would you do?"  ✏ `dlg.ione.shelf.1`
       ? "Take one for someone." → r0  effects: friend:ione,15, flag:choice.shelf.take  ✏ `dlg.ione.shelf.c0`
       ? "Read one aloud." → r1  effects: friend:ione,10, flag:choice.shelf.read  ✏ `dlg.ione.shelf.c1`
       ? "Leave a note in one." → r2  effects: friend:ione,10, flag:choice.shelf.note  ✏ `dlg.ione.shelf.c2`
    r0 [ione]: "Take one. ...For someone who'll come. Yes. That's what they were waiting for."  ✏ `dlg.ione.shelf.r0`   (expr happy)
    r1 [ione]: "Aloud? I haven't heard these in a human voice for years. Go slowly. Please."  ✏ `dlg.ione.shelf.r1`   (expr happy)
    r2 [ione]: "A note. In a book nobody's opened. ...It's the kindest thing anyone's done for this shelf."  ✏ `dlg.ione.shelf.r2`   (expr happy)
  [ione]: "{if choice:shelf.take}You took one for someone. That's the first set-aside book that's ever left the shelf. I'll note it.{else}{if choice:shelf.read}You read one aloud. The shelf has never been so quiet. Or so pleased.{else}You left a note. I'll never move it. It's the best bookmark I've ever had.{/if}{/if}"  ✏ `event.ione_heart8.after`
  · effects: flag:event.ione_heart8
  · effects: friend:ione,70

### ione_birthday · "Ione's Birthday" · wholesome
conversation · wave4_birthdays.json · map Library · priority 8 · repeatable
plays when: `hearts:ione>=2 && birthday.in:ione==0 && unseen:birthday.ione && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "It's my birthday. I don't make a fuss. I read a new book on the day. Come in. I set something aside for you."  ✏ `event.ione_birthday.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.birthday` (player-facing graph, plays here):
    n0 [ione]: "The book can wait. What shall it be?"  ✏ `dlg.ione.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:ione,10, flag:choice.bday.ione.wish  ✏ `dlg.ione.birthday.c0`
       ? "Read to her." → r1  effects: friend:ione,10, flag:choice.bday.ione.read  ✏ `dlg.ione.birthday.c1`
       ? "Give her a bookmark." → r2  effects: friend:ione,10, flag:choice.bday.ione.mark  ✏ `dlg.ione.birthday.c2`
    r0 [ione]: "Thank you. I'll set it aside, between two good pages."  ✏ `dlg.ione.birthday.r0`   (expr happy)
    r1 [ione]: "You read aloud. It's been years. Please go slowly. It's lovely."  ✏ `dlg.ione.birthday.r1`   (expr happy)
    r2 [ione]: "A bookmark. From you. It's going in my favorite book, right now."  ✏ `dlg.ione.birthday.r2`   (expr happy)
  [ione]: "{if choice:bday.ione.wish}It's set aside, between two good pages. I'll find it often.{else}{if choice:bday.ione.read}Your voice, my library. The books were listening. They're quiet about it.{else}The bookmark's in the favorite. It's holding a very good place.{/if}{/if}"  ✏ `event.ione_birthday.after`
  · effects: flag:event.ione_birthday
  · effects: mark:birthday.ione, friend:ione,30, give:forage.seashell,1, mail:ione_bday_thanks

### ione_courtship · "Ione Asks" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:ione>=8 && flag:event.ione_heart8 && !flag:partners.ione && !flag:choice.courtship.ione.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "I've read a great many love stories, and none of them told me how to say this. Would you court me? That's a plain question. I rewrote it eleven times."  ✏ `event.ione_courtship.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.courtship` (player-facing graph, plays here):
    n0 [ione]: "What do you say?"  ✏ `dlg.ione.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:ione,20, flag:choice.courtship.ione.yes  ✏ `dlg.ione.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:ione,15, flag:choice.courtship.ione.friends  ✏ `dlg.ione.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:ione,5, flag:choice.courtship.ione.later  ✏ `dlg.ione.courtship.c2`
    r0 [ione]: "Yes. ...Oh, good. I'll set aside a shelf for the two of us. A small one, near the window."  ✏ `dlg.ione.courtship.r0`   (expr happy)
    r1 [ione]: "Friends. A lovely thing to be. The best stories begin there. Thank you for saying it so kindly."  ✏ `dlg.ione.courtship.r1`   (expr happy)
    r2 [ione]: "Wait. Of course. A good book isn't rushed. Neither is this."  ✏ `dlg.ione.courtship.r2`   (expr thinking)
  [ione]: "{if flag:choice.courtship.ione.yes}A courtship, then. I'll start by reading you the first page of a new book. Aloud.{else}{if flag:choice.courtship.ione.friends}Friends. A wonderful shelf to share. I'll keep the invitation between two pages.{else}Take all the time you need. The library will keep your place.{/if}{/if}"  ✏ `event.ione_courtship.after`
  · effects: flag:event.ione_courtship
  · effects: flag:courtship.asked.ione

### ione_courtship2 · "Ione Asks Again" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:ione>=10 && flag:choice.courtship.ione.later && !flag:event.ione_courtship2.done && !flag:partners.ione && !flag:choice.courtship.ione.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "I've read a great many love stories, and none of them told me how to say this. Would you court me? That's a plain question. I rewrote it eleven times."  ✏ `event.ione_courtship2.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.courtship` (player-facing graph, plays here):
    n0 [ione]: "What do you say?"  ✏ `dlg.ione.courtship.0`
       ? "Yes. I'd like that." → r0  effects: friend:ione,20, flag:choice.courtship.ione.yes  ✏ `dlg.ione.courtship.c0`
       ? "I'd rather we stayed friends." → r1  effects: friend:ione,15, flag:choice.courtship.ione.friends  ✏ `dlg.ione.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:ione,5, flag:choice.courtship.ione.later  ✏ `dlg.ione.courtship.c2`
    r0 [ione]: "Yes. ...Oh, good. I'll set aside a shelf for the two of us. A small one, near the window."  ✏ `dlg.ione.courtship.r0`   (expr happy)
    r1 [ione]: "Friends. A lovely thing to be. The best stories begin there. Thank you for saying it so kindly."  ✏ `dlg.ione.courtship.r1`   (expr happy)
    r2 [ione]: "Wait. Of course. A good book isn't rushed. Neither is this."  ✏ `dlg.ione.courtship.r2`   (expr thinking)
  [ione]: "{if flag:choice.courtship.ione.yes}A courtship, then. I'll start by reading you the first page of a new book. Aloud.{else}{if flag:choice.courtship.ione.friends}Friends. A wonderful shelf to share. I'll keep the invitation between two pages.{else}Take all the time you need. The library will keep your place.{/if}{/if}"  ✏ `event.ione_courtship2.after`
  · effects: flag:event.ione_courtship2
  · effects: flag:courtship.asked.ione, flag:event.ione_courtship2.done

### ione_partners · "Ione, Together" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:ione>=10 && flag:choice.courtship.ione.yes && !flag:partners.ione && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Come in. I wrote you a letter. It's longer than I meant. Sit, please. I'll read it, and you'll pretend not to blush."  ✏ `event.ione_partners.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.partners` (player-facing graph, plays here):
    n0 [ione]: "I set the evening aside for you. How shall we spend it?"  ✏ `dlg.ione.partners.0`
       ? "Listen to the whole letter." → r0  effects: friend:ione,10, flag:choice.courtship.ione.letter  ✏ `dlg.ione.partners.c0`
       ? "Read to her in the stacks." → r1  effects: friend:ione,10, flag:choice.courtship.ione.read  ✏ `dlg.ione.partners.c1`
       ? "Walk her along the shore." → r2  effects: friend:ione,10, flag:choice.courtship.ione.walk  ✏ `dlg.ione.partners.c2`
    r0 [ione]: "Every word. Thank you. ...I'm blushing, and I'm the one who wrote it."  ✏ `dlg.ione.partners.r0`   (expr embarrassed)
    r1 [ione]: "You read aloud, and the books lean in. They've missed being listened to. So have I."  ✏ `dlg.ione.partners.r1`   (expr happy)
    r2 [ione]: "The shore. With you. I'll bring a book and not open it. That's a first."  ✏ `dlg.ione.partners.r2`   (expr happy)
  [ione]: "{if choice:courtship.ione.letter}The letter's in my favorite book. It marks the best page. It's yours.{else}{if choice:courtship.ione.read}Your voice, my library. The books are very quiet about it. I think they approve.{else}The shore was bright. I forgot the book entirely. It's a small, wonderful betrayal.{/if}{/if}"  ✏ `event.ione_partners.after`
  · effects: flag:event.ione_partners
  · effects: flag:partners.ione, friend:ione,50

### overheard_hazel_ione_1 · "Quiet Hours" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.ione && hearts:hazel>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: hazel(24, 18), ione(26, 18)
  · (place hazel to (24,18))
  · (place ione to (26,18))
  · (hazel faces right)
  · (ione faces left)
  [hazel]: "(Third shelf, left. The mystery is back in order.)"
  [ione]: "Thank you. The library's silence is yours, you know. I only sweep it."  ✏ `event.overheard_hazel_ione_1.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (hazel point (background))
  [hazel]: "(Then let's both keep it.)"
  [ione]: "Agreed. Back to the tea, then, quietly, and nobody has to say another word."  ✏ `event.overheard_hazel_ione_1.l4`
  · effects: flag:event.overheard_hazel_ione_1

### overheard_hazel_ione_2 · "The Overdue Notice" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.ione && hearts:hazel>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: ione(24, 18), hazel(26, 18)
  · (place ione to (24,18))
  · (place hazel to (26,18))
  · (ione faces right)
  · (hazel faces left)
  [ione]: "There's a book on the desk that's seven years overdue. It's yours."  ✏ `event.overheard_hazel_ione_2.l1`
  [hazel]: "(It's a very long book.) (I'm on the last chapter.)"
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (ione wave (background))
  [ione]: "You're on a chapter every year, then?"  ✏ `event.overheard_hazel_ione_2.l3`
  [hazel]: "(It's a very slow chapter.) (I'm enjoying it.)"
  · effects: flag:event.overheard_hazel_ione_2

### overheard_hazel_ione_3 · "The Reading Chair" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.ione && hearts:hazel>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: ione(24, 18), hazel(26, 18)
  · (place ione to (24,18))
  · (place hazel to (26,18))
  · (ione faces right)
  · (hazel faces left)
  [ione]: "I left a chair by the window. It has a cushion. It's for whoever needs it."  ✏ `event.overheard_hazel_ione_3.l1`
  [hazel]: "(It's for me. Yes? Thank you.) (I won't say so aloud.)"
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (ione shrug (background))
  [ione]: "You've said it plainly. It's plenty. The window's warm after lunch. Sit there then."  ✏ `event.overheard_hazel_ione_3.l3`
  [hazel]: "(I'll come by.) (Quiet, shared. A good arrangement.)"
  · effects: flag:event.overheard_hazel_ione_3

### overheard_odalys_ione_1 · "Late Lamps" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. The library stayed open an hour past closing."
  [ione]: "Dr. Penn. The clinic stayed open two."  ✏ `event.overheard_odalys_ione_1.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys point (background))
  [odalys]: "Professional courtesy. I'd call it a draw."
  [ione]: "Then let's both go home. Tomorrow."  ✏ `event.overheard_odalys_ione_1.l4`
  · effects: flag:event.overheard_odalys_ione_1

### overheard_odalys_ione_2 · "Reading Prescription" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "I prescribe you a nap, Ione. Ten minutes. Non-negotiable."
  [ione]: "I prescribe you a novel, Dr. Penn. One chapter. Equally firm."  ✏ `event.overheard_odalys_ione_2.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys wave (background))
  [odalys]: "...Chapter one. Then the nap."
  [ione]: "Agreed. Chapter one is short. The nap can be long."  ✏ `event.overheard_odalys_ione_2.l4`
  · effects: flag:event.overheard_odalys_ione_2

### overheard_odalys_ione_3 · "Tea Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.odalys && flag:met.ione && hearts:odalys>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Thank you for the tea, Ione. I will drink it slowly."
  [ione]: "It's the calm kind. The stove likes it too."  ✏ `event.overheard_odalys_ione_3.l2`
  · (ione point (background))
  · (ione emote ellipsis)
  · (odalys shrug (background))
  [odalys]: "Then it is a verified remedy. Pass the cup."
  [ione]: "Always. Here. It's nearly warm."  ✏ `event.overheard_odalys_ione_3.l4`
  · effects: flag:event.overheard_odalys_ione_3
