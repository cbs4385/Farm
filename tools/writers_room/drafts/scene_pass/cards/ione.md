# Scene cards for ione

15 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 36 spoken lines in scenes, 17 dialogue lines and 12 player choice labels.

### ione_heart2 · "What Ione Found" · wholesome
event · npc_roster_core.json · map Library · priority 6
plays when: `(hearts:ione>=2 && !weekday:sat && hour>=9 && hour<17) && !flag:event.ione_heart2`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Oh, [player]! Come and see. This was folded in the back of an old almanac."  ✏ `event.ione_heart2.0`
  · (move player to (7,4))
  · (player faces up)
  · (ione emote note)
  [ione]: "A chart of the coast. The ink is nearly gone, but look. Someone drew every cove by hand."  ✏ `event.ione_heart2.1`
  [ione]: "And these were tucked in the fold. Two shells. Somebody meant to come back for them."  ✏ `event.ione_heart2.3`  (expr thinking)
  [ione]: "Take them. I thought you'd know what to do with a shell. I only know how to file one."  ✏ `event.ione_heart2.2`
  [ione]: "There's a cove at the very edge, where the ink gives out. I imagine it's lovely in winter."  ✏ `event.ione_heart2.4`
  · effects: give:forage.seashell,2, friend:ione,40, flag:event.ione_heart2

### ione_heart5 · "A Question" · wholesome
event · npc_roster_core.json · map Library · priority 6
plays when: `(hearts:ione>=5 && !weekday:sat && hour>=9 && hour<17) && !flag:event.ione_heart5`
beats may be ADDED or CUT in this scene
placed: player(7, 4)
  · (ione faces down)
  [ione]: "There's something I wanted to ask you. I wrote it on a slip so I'd get it right."  ✏ `event.ione_heart5.0`
  [ione]: "I've lost the slip. It's somewhere in Poetry. ...Never mind. I remember it."  ✏ `event.ione_heart5.3`  (emote sweat, expr embarrassed)
  · (move player to (7,4))
  · (player faces up)
  · (ione emote note)
  [ione]: "Would you walk the beach with me one evening? Only if you'd like. I'm told the tide doesn't mind either way."  ✏ `event.ione_heart5.1`
  [ione]: "Oh. Good. Then take this. I found it on the shore one winter, years ago. It's waited in a drawer long enough."  ✏ `event.ione_heart5.2`
  [ione]: "Evenings, then. The almanac marks the low tides. I've circled three already. In ink."  ✏ `event.ione_heart5.4`  (expr happy)
  · effects: give:forage.pearl,1, friend:ione,60, flag:event.ione_heart5

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
  [ione]: "{if flag:met.wren && hearts:wren>=3}Five{else}Four{/if} chairs, one lamp, and a pot of tea nobody asked for. This is my calm face. Does it work?"  ✏ `event.reading_night.0`
  [hazel]: "(It doesn't.) I brought something. It's in my sleeve. I haven't decided if it comes out."  (expr embarrassed)
  ▶ DIALOGUE `reading.hazel` (player-facing graph, plays here):
    n0 [hazel]: "(Nobody has to hear it.) What would you do? ...If you were me, I mean."
       ? "Read yours. We're all listening." → r0  effects: flag:choice.reading.poem, friend:hazel,10
       ? "Read something borrowed tonight." → r1  effects: flag:choice.reading.borrowed, friend:hazel,10
       ? "Listen first. Ione's face needs the practice." → r2  effects: flag:choice.reading.listen, friend:hazel,10
    r0 [hazel]: "(Fine.) A short one. Eleven lines. (I counted twice. It's still eleven.)"
    r1 [hazel]: "Someone else's, then. Kinder to the room. (Mine stays in my sleeve.)"
    r2 [hazel]: "Ione first. (Thank you.) I'm not leaving. I'm only sitting very still."
  [hazel]: "The lamp does not mind the dark. It keeps it company. ...That's the last of the eleven. (The rest were warming up.)"  (expr thinking) [only if flag:choice.reading.poem]
  [ione]: "I'll copy that out by hand, Hazel. Not for a shelf. For the desk drawer, where the good things go."  ✏ `event.reading_night.poem_after` [only if flag:choice.reading.poem]
  [narration]: "Hazel reads two borrowed pages about the winter sea, quietly, all the way through. Her sleeve stays shut." [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and does not leave." [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "A snowdrop. Pressed. It kept my place since winter. For the lamp."
  [wren]: "Don't mind me, I only tell stories. Somebody has to write them down. Otherwise I improve them until nobody recognizes them." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "A snowdrop, for the lamp. ...Every chair taken. Next time I'll set out a fifth and leave the door open."  ✏ `event.reading_night.4`  (expr happy)
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
  [tilda]: "Lanterns up, everyone. Not lit yet. Wait for the bell. Felix, I mean you."
  [hazel]: "One lantern, one wish. Please keep yours folded up. (Last winter four people said theirs aloud.)"
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "My wish is this big. Last winter it was this big. Wishes grow, like fish."
  [ione]: "That's the bell. Together now, and slowly. Let them go. ...Mine is drifting toward the sea. I didn't steer it."  ✏ `event.festival_winter_lanterns.3`
  · (ione emote heart)
  · (remove prop story_prop_3)
  · (remove prop story_prop_2)
  · (remove prop story_prop_1)
  · (remove prop story_prop_0)
  · effects: mark:festival.winter.lanterns, flag:event.festival_winter_lanterns, friend:hazel,10, friend:ione,10, friend:tilda,10, friend:felix,10

### ione_heart8 · "The Set-Aside Shelf" · wholesome
conversation · wave3_events.json · map Library · priority 7
plays when: `(hearts:ione>=8 && flag:event.ione_heart5 && !weekday:sat && hour>=9 && hour<17) && !flag:event.ione_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Come with me, if you have a minute. There's a shelf at the back I've never shown anyone. It's a little sad. It's mine."  ✏ `event.ione_heart8.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote sparkle)
  ▶ DIALOGUE `ione.shelf` (player-facing graph, plays here):
    n0 [narration]: "At the back of the library stands a tall shelf. Every book has a slip of paper tucked inside. Not one slip has a name."  ✏ `dlg.ione.shelf.0`   (→ n1)
    n1 [ione]: "Books I set aside for people who never came in. The oldest has waited nine years. I can't reshelve them. What would you do?"  ✏ `dlg.ione.shelf.1`
       ? "Take one to someone who should have it." → r0  effects: friend:ione,15, flag:choice.shelf.take  ✏ `dlg.ione.shelf.c0`
       ? "Read one aloud, right here." → r1  effects: friend:ione,10, flag:choice.shelf.read  ✏ `dlg.ione.shelf.c1`
       ? "Tuck a note in one, for whoever comes." → r2  effects: friend:ione,10, flag:choice.shelf.note  ✏ `dlg.ione.shelf.c2`
    r0 [ione]: "Take one. ...To someone who'll actually read it. Yes. That's all any of them wanted."  ✏ `dlg.ione.shelf.r0`   (expr happy)
    r1 [ione]: "Aloud? Nobody has ever read these to anyone. Go slowly, please. They'll want to keep up."  ✏ `dlg.ione.shelf.r1`   (expr happy)
    r2 [ione]: "A note, in a book nobody's opened. Now whoever comes will know someone was expecting them."  ✏ `dlg.ione.shelf.r2`   (expr happy)
  [ione]: "{if choice:shelf.take}The first set-aside book ever to leave the shelf. I've written it in the ledger. Tell me who it went to. Not yet. Once they've read it.{else}{if choice:shelf.read}You read one aloud. The shelf has never been so quiet. I think that was it listening.{else}I won't move your note. I won't even read it. ...Not this week, anyway.{/if}{/if}"  ✏ `event.ione_heart8.after`
  · effects: flag:event.ione_heart8
  · effects: friend:ione,70

### ione_birthday · "Ione's Birthday" · wholesome
conversation · wave4_birthdays.json · map Library · priority 8 · repeatable
plays when: `hearts:ione>=2 && birthday.in:ione==0 && unseen:birthday.ione && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "It's my birthday. The fuss is a new book I don't open until noon. Come in. I set something aside for you, too."  ✏ `event.ione_birthday.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.birthday` (player-facing graph, plays here):
    n0 [ione]: "The new book can wait. It's very patient. What shall we do?"  ✏ `dlg.ione.birthday.0`
       ? "Wish her the happiest of birthdays." → r0  effects: friend:ione,10, flag:choice.bday.ione.wish  ✏ `dlg.ione.birthday.c0`
       ? "Read the first page to her." → r1  effects: friend:ione,10, flag:choice.bday.ione.read  ✏ `dlg.ione.birthday.c1`
       ? "Give her a bookmark. A slightly crooked one." → r2  effects: friend:ione,10, flag:choice.bday.ione.mark  ✏ `dlg.ione.birthday.c2`
    r0 [ione]: "Thank you. I'll keep that between two good pages, where I'll come across it often."  ✏ `dlg.ione.birthday.r0`   (expr happy)
    r1 [ione]: "The first page, aloud. Nobody's read to me since I was small. I'll close my eyes, if you don't mind."  ✏ `dlg.ione.birthday.r1`   (expr happy)
    r2 [ione]: "A bookmark. From you. It's crooked. So is my favorite book. They'll get along."  ✏ `dlg.ione.birthday.r2`   (expr happy)
  [ione]: "{if choice:bday.ione.wish}Noon came and went, and I never opened the new book. I was too busy being wished well.{else}{if choice:bday.ione.read}Someone read to me on my birthday. I've put it in the ledger, under returned with interest.{else}The bookmark's holding a very good place. I'd been using a tea label.{/if}{/if}"  ✏ `event.ione_birthday.after`
  · effects: flag:event.ione_birthday
  · effects: mark:birthday.ione, friend:ione,30, give:forage.seashell,1, mail:ione_bday_thanks

### ione_courtship · "Ione Asks" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `(hearts:ione>=8 && flag:event.ione_heart8 && !flag:partners.ione && !flag:choice.courtship.ione.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !weekday:sat && hour>=9 && hour<17) && !flag:event.ione_courtship`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "I've read hundreds of love stories, and not one told me how to ask this. So, plainly: would you court me? I rewrote that fourteen times."  ✏ `event.ione_courtship.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.courtship` (player-facing graph, plays here):
    n0 [ione]: "What do you say? Take your time. I'll pretend to shelve something."  ✏ `dlg.ione.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:ione,20, flag:choice.courtship.ione.yes  ✏ `dlg.ione.courtship.c0`
       ? "I'd rather we stayed good friends." → r1  effects: friend:ione,15, flag:choice.courtship.ione.friends  ✏ `dlg.ione.courtship.c1`
       ? "Could we wait a while? Bookmark it?" → r2  effects: friend:ione,5, flag:choice.courtship.ione.later  ✏ `dlg.ione.courtship.c2`
    r0 [ione]: "Yes. ...Oh, good. Good. I'll clear a shelf for the two of us. A small one, by the window."  ✏ `dlg.ione.courtship.r0`   (expr happy)
    r1 [ione]: "Friends. That's a lovely thing to be, and the best stories start there. You were kind to say it plainly."  ✏ `dlg.ione.courtship.r1`   (expr happy)
    r2 [ione]: "Wait. Of course. A good book isn't rushed. I'll keep the question by the desk bell."  ✏ `dlg.ione.courtship.r2`   (expr thinking)
  [ione]: "{if flag:choice.courtship.ione.yes}A courtship, then. I'll begin properly: the first page of a new book, read aloud. Sunday. Bring nothing.{else}{if flag:choice.courtship.ione.friends}Friends, then. A good shelf to share. I'll lend you everything and ask for nothing back on time.{else}Take all the time you need. The library is very good at keeping a place.{/if}{/if}"  ✏ `event.ione_courtship.after`
  · effects: flag:event.ione_courtship
  · effects: flag:courtship.asked.ione

### ione_courtship2 · "Ione Asks Again" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:ione>=10 && flag:choice.courtship.ione.later && !flag:event.ione_courtship2.done && !flag:partners.ione && !flag:choice.courtship.ione.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "You asked me to wait. The question sat by the desk bell all this while. It held up. Would you court me?"  ✏ `event.ione_courtship2.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.courtship` (player-facing graph, plays here):
    n0 [ione]: "What do you say? Take your time. I'll pretend to shelve something."  ✏ `dlg.ione.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:ione,20, flag:choice.courtship.ione.yes  ✏ `dlg.ione.courtship.c0`
       ? "I'd rather we stayed good friends." → r1  effects: friend:ione,15, flag:choice.courtship.ione.friends  ✏ `dlg.ione.courtship.c1`
       ? "Could we wait a while? Bookmark it?" → r2  effects: friend:ione,5, flag:choice.courtship.ione.later  ✏ `dlg.ione.courtship.c2`
    r0 [ione]: "Yes. ...Oh, good. Good. I'll clear a shelf for the two of us. A small one, by the window."  ✏ `dlg.ione.courtship.r0`   (expr happy)
    r1 [ione]: "Friends. That's a lovely thing to be, and the best stories start there. You were kind to say it plainly."  ✏ `dlg.ione.courtship.r1`   (expr happy)
    r2 [ione]: "Wait. Of course. A good book isn't rushed. I'll keep the question by the desk bell."  ✏ `dlg.ione.courtship.r2`   (expr thinking)
  [ione]: "{if flag:choice.courtship.ione.yes}A courtship, then. I'll begin properly: the first page of a new book, read aloud. Sunday. Bring nothing.{else}{if flag:choice.courtship.ione.friends}Friends, then. A good shelf to share. I'll lend you everything and ask for nothing back on time.{else}Still not yet. That's all right. Waiting is most of a librarian's job.{/if}{/if}"  ✏ `event.ione_courtship2.after`
  · effects: flag:event.ione_courtship2
  · effects: flag:courtship.asked.ione, flag:event.ione_courtship2.done

### ione_partners · "Ione, Together" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `(hearts:ione>=10 && flag:choice.courtship.ione.yes && !flag:partners.ione && !weekday:sat && hour>=9 && hour<17) && !flag:event.ione_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(7, 4)
  · (ione faces down)
  [ione]: "Sit, please. I wrote you a letter. It runs four pages. I'll read it, and you'll pretend not to blush."  ✏ `event.ione_partners.intro`
  · (move player to (7,4))
  · (player faces up)
  · (ione wave (background))
  · (ione emote heart)
  ▶ DIALOGUE `ione.partners` (player-facing graph, plays here):
    n0 [ione]: "I've set the rest of the day aside for you. How shall we spend it?"  ✏ `dlg.ione.partners.0`
       ? "Listen to all four pages." → r0  effects: friend:ione,10, flag:choice.courtship.ione.letter  ✏ `dlg.ione.partners.c0`
       ? "Read to her among the stacks." → r1  effects: friend:ione,10, flag:choice.courtship.ione.read  ✏ `dlg.ione.partners.c1`
       ? "Walk the shore. The letter can come too." → r2  effects: friend:ione,10, flag:choice.courtship.ione.walk  ✏ `dlg.ione.partners.c2`
    r0 [ione]: "Every word. Thank you. ...I'm the one blushing, and I wrote it."  ✏ `dlg.ione.partners.r0`   (expr embarrassed)
    r1 [ione]: "You read, and the books lean in. They've missed being listened to. ...So had I."  ✏ `dlg.ione.partners.r1`   (expr happy)
    r2 [ione]: "The shore, with you. I'll bring a book and not open it. From a librarian, that's devotion."  ✏ `dlg.ione.partners.r2`   (expr happy)
  [ione]: "{if choice:courtship.ione.letter}The letter's in my favorite book now, marking the best page. Both are yours, eventually.{else}{if choice:courtship.ione.read}Your voice, my library. I've marked the spot where you stood. Nobody else may stand there. Hazel may.{else}The shore was bright. I forgot the book entirely. ...I'd like to see it in winter. With you.{/if}{/if}"  ✏ `event.ione_partners.after`
  · effects: flag:event.ione_partners
  · effects: flag:partners.ione, friend:ione,50

### overheard_hazel_ione_1 · "Quiet Hours" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.ione && hearts:hazel>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_hazel_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: hazel(24, 18), ione(26, 18)
  · (place hazel to (24,18))
  · (place ione to (26,18))
  · (hazel faces right)
  · (ione faces left)
  [hazel]: "The spring almanac. Third shelf, left, behind the mysteries. (Someone sorted those by color.)"
  [ione]: "You found it. I'd looked for a week. The library's quiet is really yours. I only dust it."  ✏ `event.overheard_hazel_ione_1.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (hazel point (background))
  [hazel]: "Then we split it. You dust, I shelve. (The quiet stays undivided.)"
  [ione]: "Agreed. Back to the tea, then. I'll even stir quietly."  ✏ `event.overheard_hazel_ione_1.l4`
  · effects: flag:event.overheard_hazel_ione_1

### overheard_hazel_ione_2 · "The Overdue Notice" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.ione && hearts:hazel>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_ione_1) && !flag:event.overheard_hazel_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: ione(24, 18), hazel(26, 18)
  · (place ione to (24,18))
  · (place hazel to (26,18))
  · (ione faces right)
  · (hazel faces left)
  [ione]: "There's a book on the desk seven years overdue. The card has your name on it, Hazel."  ✏ `event.overheard_hazel_ione_2.l1`
  [hazel]: "It's a very long book. (I'm on the last chapter. Again.)"
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (ione wave (background))
  [ione]: "You've been on the last chapter for seven years?"  ✏ `event.overheard_hazel_ione_2.l3`
  [hazel]: "Once a year, yes. (I fine myself four cents a time. I'm up to twenty-eight.)"
  · effects: flag:event.overheard_hazel_ione_2

### overheard_hazel_ione_3 · "The Reading Chair" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.hazel && flag:met.ione && hearts:hazel>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_ione_2) && !flag:event.overheard_hazel_ione_3`
text-only scene: rewrite lines, do not add or cut beats
placed: ione(24, 18), hazel(26, 18)
  · (place ione to (24,18))
  · (place hazel to (26,18))
  · (ione faces right)
  · (hazel faces left)
  [ione]: "I put a chair by the window. It has a cushion. It's for whoever needs it."  ✏ `event.overheard_hazel_ione_3.l1`
  [hazel]: "Oh. (It's for me.) ...I'm not going to say anything. It would only echo."
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (ione shrug (background))
  [ione]: "You didn't say a word. I heard it anyway. The window's warm after lunch."  ✏ `event.overheard_hazel_ione_3.l3`
  [hazel]: "After lunch, then. (The cushion has a lump. I'll fix it quietly.)"
  · effects: flag:event.overheard_hazel_ione_3

### overheard_odalys_ione_1 · "Late Lamps" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_odalys_ione_1`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. Your lamps were still lit an hour past closing."
  [ione]: "Dr. Penn. Your clinic stayed lit two hours past. I could see your lamp from my desk."  ✏ `event.overheard_odalys_ione_1.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys point (background))
  [odalys]: "Patients keep odd hours. Books do not. ...Agreed. Call it a draw."
  [ione]: "Then we'll both go home on time. Starting tomorrow."  ✏ `event.overheard_odalys_ione_1.l4`
  · effects: flag:event.overheard_odalys_ione_1

### overheard_odalys_ione_2 · "Reading Prescription" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=4 && hearts:ione>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_1) && !flag:event.overheard_odalys_ione_2`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "Ione. I prescribe you a nap. Fifteen minutes. I've already written it down."
  [ione]: "And I prescribe you a novel, Dr. Penn. One chapter. Equally non-negotiable."  ✏ `event.overheard_odalys_ione_2.l2`
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (odalys wave (background))
  [odalys]: "...Chapter one, then the nap. I'll return the book by hand. Your returns box is sticky again."
  [ione]: "Done. I picked a book whose first chapter is two pages. Sleep well."  ✏ `event.overheard_odalys_ione_2.l4`
  · effects: flag:event.overheard_odalys_ione_2

### overheard_odalys_ione_3 · "Tea Rounds" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.odalys && flag:met.ione && hearts:odalys>=6 && hearts:ione>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_odalys_ione_2) && !flag:event.overheard_odalys_ione_3`
text-only scene: rewrite lines, do not add or cut beats
placed: odalys(24, 18), ione(26, 18)
  · (place odalys to (24,18))
  · (place ione to (26,18))
  · (odalys faces right)
  · (ione faces left)
  [odalys]: "The tea, Ione. I've been told to drink it slowly, so I shall. Under observation."
  [ione]: "It's the calm kind. Even the library stove stopped ticking once the pot was on it."  ✏ `event.overheard_odalys_ione_3.l2`
  · (ione point (background))
  · (ione emote ellipsis)
  · (odalys shrug (background))
  [odalys]: "If Hazel reorganized nothing, it's a verified remedy. Pass the cup."
  [ione]: "Always. Here. It's gone a little cold. That's the slow part working."  ✏ `event.overheard_odalys_ione_3.l4`
  · effects: flag:event.overheard_odalys_ione_3
