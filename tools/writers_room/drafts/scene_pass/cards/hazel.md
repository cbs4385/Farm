# Scene cards for hazel

25 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 58 spoken lines in scenes, 28 dialogue lines and 21 player choice labels.

### hazel_heart2 · "Come Look at This" · wholesome
event · npc_roster.json · map Library · priority 6
plays when: `hearts:hazel>=2 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Shh. Back here, behind Natural History. (Nobody comes this far. That's the point.)"  ✏ `event.hazel_heart2.0`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote ellipsis)
  [hazel]: "A snowdrop, pressed in an almanac eighty-one years ago. Page two hundred and four."  ✏ `event.hazel_heart2.1`
  [hazel]: "Someone wrote the first frost beside it, in pencil. They were three days early."  ✏ `event.hazel_heart2.3`  (expr thinking)
  [hazel]: "This is the reading nook. One chair. (I've never needed two.)"  ✏ `event.hazel_heart2.2`
  [hazel]: "Here. A fresh one, for you. (Please don't press it in a cookbook.)"  ✏ `event.hazel_heart2.4`  (expr happy)
  · effects: give:forage.snowdrop,1, friend:hazel,40, flag:event.hazel_heart2

### hazel_heart5 · "A Small Poem" · wholesome
event · npc_roster.json · map Library · priority 7
plays when: `hearts:hazel>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.hazel_heart2`
beats may be ADDED or CUT in this scene
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I wrote you something. (Five drafts. Four are on the floor.)"  ✏ `event.hazel_heart5.0`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote ellipsis)
  [hazel]: "It's a small poem. Six lines. If you laugh, laugh at line four. It's the weak one."  ✏ `event.hazel_heart5.1`
  [hazel]: "Read it to yourself. Please. (Not aloud. I'm not there yet.)"  ✏ `event.hazel_heart5.3`  (emote sweat, expr embarrassed)
  [hazel]: "You read to the end. (I watched.) Nobody's done that before."  ✏ `event.hazel_heart5.2`
  [hazel]: "Blackberry jam. Two jars. A bribe, for your silence about line four."  ✏ `event.hazel_heart5.4`  (expr happy)
  · effects: give:artisan.jam,2, friend:hazel,60, flag:event.hazel_heart5

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
  [ione]: "{if flag:met.wren && hearts:wren>=3}Five{else}Four{/if} chairs, one lamp, and a pot of tea nobody asked for. This is my calm face. Does it work?"
  [hazel]: "(It doesn't.) I brought something. It's in my sleeve. I haven't decided if it comes out."  ✏ `event.reading_night.1`  (expr embarrassed)
  ▶ DIALOGUE `reading.hazel` (player-facing graph, plays here):
    n0 [hazel]: "(Nobody has to hear it.) What would you do? ...If you were me, I mean."  ✏ `dlg.reading.hazel.0`
       ? "Read yours. We're all listening." → r0  effects: flag:choice.reading.poem, friend:hazel,10  ✏ `dlg.reading.hazel.c0`
       ? "Read something borrowed tonight." → r1  effects: flag:choice.reading.borrowed, friend:hazel,10  ✏ `dlg.reading.hazel.c1`
       ? "Listen first. Ione's face needs the practice." → r2  effects: flag:choice.reading.listen, friend:hazel,10  ✏ `dlg.reading.hazel.c2`
    r0 [hazel]: "(Fine.) A short one. Eleven lines. (I counted twice. It's still eleven.)"  ✏ `dlg.reading.hazel.r0`
    r1 [hazel]: "Someone else's, then. Kinder to the room. (Mine stays in my sleeve.)"  ✏ `dlg.reading.hazel.r1`
    r2 [hazel]: "Ione first. (Thank you.) I'm not leaving. I'm only sitting very still."  ✏ `dlg.reading.hazel.r2`
  [hazel]: "The lamp does not mind the dark. It keeps it company. ...That's the last of the eleven. (The rest were warming up.)"  ✏ `event.reading_night.poem`  (expr thinking) [only if flag:choice.reading.poem]
  [ione]: "I'll copy that out by hand, Hazel. Not for a shelf. For the desk drawer, where the good things go." [only if flag:choice.reading.poem]
  [narration]: "Hazel reads two borrowed pages about the winter sea, quietly, all the way through. Her sleeve stays shut."  ✏ `event.reading_night.borrowed` [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and does not leave."  ✏ `event.reading_night.listen` [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "A snowdrop. Pressed. It kept my place since winter. For the lamp."
  [wren]: "Don't mind me, I only tell stories. Somebody has to write them down. Otherwise I improve them until nobody recognizes them." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "A snowdrop, for the lamp. ...Every chair taken. Next time I'll set out a fifth and leave the door open."  (expr happy)
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
  [hazel]: "One lantern, one wish. Please keep yours folded up. (Last winter four people said theirs aloud.)"  ✏ `event.festival_winter_lanterns.1`
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "My wish is this big. Last winter it was this big. Wishes grow, like fish."
  [ione]: "That's the bell. Together now, and slowly. Let them go. ...Mine is drifting toward the sea. I didn't steer it."
  · (ione emote heart)
  · (remove prop story_prop_3)
  · (remove prop story_prop_2)
  · (remove prop story_prop_1)
  · (remove prop story_prop_0)
  · effects: mark:festival.winter.lanterns, flag:event.festival_winter_lanterns, friend:hazel,10, friend:ione,10, friend:tilda,10, friend:felix,10

### hazel_heart4 · "The Swap" · wholesome
conversation · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=4 && flag:event.hazel_heart2 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I'd like to swap something. A book for a book. (Or a book for anything.) Is that odd?"  ✏ `event.hazel_heart4.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.swap` (player-facing graph, plays here):
    n0 [hazel]: "What would you lend me? (I'm not choosing for you. I'm leaning slightly.)"  ✏ `dlg.hazel.swap.0`
       ? "The book I love best." → r0  effects: friend:hazel,10, flag:choice.hazel.swap.book  ✏ `dlg.hazel.swap.c0`
       ? "A recipe card, if that counts." → r1  effects: friend:hazel,10, flag:choice.hazel.swap.recipe  ✏ `dlg.hazel.swap.c1`
       ? "Honestly? I don't read much." → r2  effects: friend:hazel,10, flag:choice.hazel.swap.honest  ✏ `dlg.hazel.swap.c2`
    r0 [hazel]: "Oh. (It's a both-hands book.) I'll read it twice. Once for the story, once for you."  ✏ `dlg.hazel.swap.r0`   (expr happy)
    r1 [hazel]: "A recipe. (A book without a spine.) I'll shelve it under Fiction until I bake it."  ✏ `dlg.hazel.swap.r1`
    r2 [hazel]: "Thank you for saying so. (Most people pretend.) I'll find something short. There's a heron in it."  ✏ `dlg.hazel.swap.r2`
  [hazel]: "I'll look after it. (I always do.) Return date: whenever. I've never written that before."  ✏ `event.hazel_heart4.after`
  · effects: flag:event.hazel_heart4
  · effects: friend:hazel,50

### hazel_heart6 · "Behind the Shelves" · surprise
confession · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=6 && flag:event.hazel_heart5 && flag:event.hazel_heart4 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Could you come to the back? Past Geography. (I'm not hiding. I'm shelving very slowly.)"  ✏ `event.hazel_heart6.intro`
  · (move player to (4,4))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on hazel)
  · (hazel expression thinking)
  ▶ DIALOGUE `hazel.hiding` (player-facing graph, plays here):
    n0 [hazel]: "The festival wants someone to read aloud. Ione said my name. (I've been back here an hour.) Is that strange?"  ✏ `dlg.hazel.hiding.0`   (expr thinking)
       ? "Ask what she hears back here." → r0  effects: friend:hazel,10, flag:choice.hazel.hiding.ask  ✏ `dlg.hazel.hiding.c0`
       ? "Sit down beside her, quietly." → r1  effects: friend:hazel,15, flag:choice.hazel.hiding.wait  ✏ `dlg.hazel.hiding.c1`
       ? "Joke that she could hide in a book." → r2  effects: friend:hazel,5, flag:choice.hazel.hiding.joke  ✏ `dlg.hazel.hiding.c2`
    r0 [hazel]: "Pages turning, two rooms away. Other people's thoughts, kept. (It never asks me to read aloud.)"  ✏ `dlg.hazel.hiding.r0`
    r1 [hazel]: "...Oh. (Nobody's ever sat down. They usually say 'there you are'.)"  ✏ `dlg.hazel.hiding.r1`   (expr happy)
    r2 [hazel]: "Ha. (A small one.) I tried that once. The book was too thin."  ✏ `dlg.hazel.hiding.r2`
  · (hazel expression neutral)
  [hazel]: "I checked your face just now. It stayed kind. (I'll answer Ione by Friday. Thursday, if I'm brave.)"  ✏ `event.hazel_heart6.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.hazel_heart6
  · effects: friend:hazel,60

### hazel_heart8 · "Rehearsal" · wholesome
conversation · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=8 && flag:event.hazel_heart6 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Would you hear a thing I wrote? (I tried it on the gray cat. It left at line three.)"  ✏ `event.hazel_heart8.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.rehearsal` (player-facing graph, plays here):
    n0 [hazel]: "...and so the lamp burned lower, and nobody minded. (That's the end.) What did you hear?"  ✏ `dlg.hazel.rehearsal.0`   (expr embarrassed)
       ? "It was lovely." → r0  effects: friend:hazel,10, flag:choice.hazel.rehearsal.praise  ✏ `dlg.hazel.rehearsal.c0`
       ? "The middle dragged a little." → r1  effects: friend:hazel,15, flag:choice.hazel.rehearsal.critique  ✏ `dlg.hazel.rehearsal.c1`
       ? "Laugh gently at the lamp." → r2  effects: friend:hazel,10, flag:choice.hazel.rehearsal.laugh  ✏ `dlg.hazel.rehearsal.c2`
    r0 [hazel]: "Lovely. (I'm going to press that word in a book.) Thank you."  ✏ `dlg.hazel.rehearsal.r0`   (expr happy)
    r1 [hazel]: "The middle. (I knew it. Lines five through eight.) Thank you for saying it out loud."  ✏ `dlg.hazel.rehearsal.r1`
    r2 [hazel]: "The lamp is a bit much. (It's meant to be.) It stays. It's my only regular listener."  ✏ `dlg.hazel.rehearsal.r2`
  [hazel]: "One day I'll read it properly. To more than one. (Two, perhaps. Let's not be reckless.)"  ✏ `event.hazel_heart8.after`
  · effects: flag:event.hazel_heart8
  · effects: friend:hazel,70

### hazel_heart10 · "The Reading" · wholesome
performance · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=10 && flag:event.hazel_heart8 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I'll read it for you. Just you. (The shelves don't count. They've heard it already.)"  ✏ `event.hazel_heart10.intro`
  · (move player to (4,4))
  · (player faces up)
  · (lighting warm)
  · (letterbox on)
  · (camera focus on hazel)
  · (music library)
  · (together: hazel dance; hazel emote note)
  · (sfx coin)
  · (music stop)
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  [hazel]: "{if choice:hazel.rehearsal.critique}I cut the middle. (You were right. It breathes now.) The new line is about you. (It may hang on a lantern this winter. May.){else}I kept the lamp. And I added you, third line from the end. (It may hang on a lantern this winter. May.){/if}"  ✏ `event.hazel_heart10.outro`
  · effects: flag:event.hazel_heart10
  · effects: friend:hazel,80

### hazel_friend · "Overdue" · funny
helping_scene · slice_events.json · map Library · priority 7 · repeatable
plays when: `!flag:event.hazel_friend && hearts:hazel>=3 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Do you have blackberries? (A book is overdue by forty-one years. I'm baking an apology.)"  ✏ `event.hazel_friend.ask`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote question)
  [hazel]: "Come back with blackberries? (No hurry. It has waited this long. One more day won't show.)"  ✏ `event.hazel_friend.fallback`
  [hazel]: "Perfect. One tart, then a knock on the borrower's door. (He may not forgive the library. He will eat.)"  ✏ `event.hazel_friend.success`
  · effects: flag:event.hazel_friend
  · effects: friend:hazel,15

### hazel_birthday · "Hazel's Birthday" · wholesome
conversation · wave4_birthdays.json · map Library · priority 8 · repeatable
plays when: `hearts:hazel>=2 && birthday.in:hazel==0 && unseen:birthday.hazel && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "It's my birthday. (I wasn't going to say. Ione told the cake.) It's very small. Please sit."  ✏ `event.hazel_birthday.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.birthday` (player-facing graph, plays here):
    n0 [hazel]: "(You came anyway.) What happens now?"  ✏ `dlg.hazel.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:hazel,10, flag:choice.bday.hazel.wish  ✏ `dlg.hazel.birthday.c0`
       ? "Read her a line from her favorite book." → r1  effects: friend:hazel,10, flag:choice.bday.hazel.read  ✏ `dlg.hazel.birthday.c1`
       ? "Sit with her in silence." → r2  effects: friend:hazel,10, flag:choice.bday.hazel.sit  ✏ `dlg.hazel.birthday.c2`
    r0 [hazel]: "Two words, said kindly. (I counted. I always count.) I'll keep them."  ✏ `dlg.hazel.birthday.r0`   (expr happy)
    r1 [hazel]: "That one. (You remembered.) I need a moment. The lighthouse keeper would understand."  ✏ `dlg.hazel.birthday.r1`   (expr embarrassed)
    r2 [hazel]: "(Nobody is singing.) This is the best present. Please stay until the cake is gone."  ✏ `dlg.hazel.birthday.r2`   (expr happy)
  [hazel]: "{if choice:bday.hazel.wish}I wrote them in the margin of today. (In ink. I never use ink.){else}{if choice:bday.hazel.read}My favorite line, in your voice. (The keeper will sound like you now. I don't mind.){else}A shared silence. The quietest birthday I've had. (Next year, again, please.){/if}{/if}"  ✏ `event.hazel_birthday.after`
  · effects: flag:event.hazel_birthday
  · effects: mark:birthday.hazel, friend:hazel,30, give:artisan.jam,2, mail:hazel_bday_thanks

### hazel_courtship · "Hazel Asks" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=8 && flag:event.hazel_heart8 && !flag:partners.hazel && !flag:choice.courtship.hazel.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "May I ask something plainly? Would you like to court me? (I had a longer version. It had an index.)"  ✏ `event.hazel_courtship.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.courtship` (player-facing graph, plays here):
    n0 [hazel]: "What do you say?"  ✏ `dlg.hazel.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:hazel,20, flag:choice.courtship.hazel.yes  ✏ `dlg.hazel.courtship.c0`
       ? "I'd rather we stay friends." → r1  effects: friend:hazel,15, flag:choice.courtship.hazel.friends  ✏ `dlg.hazel.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:hazel,5, flag:choice.courtship.hazel.later  ✏ `dlg.hazel.courtship.c2`
    r0 [hazel]: "Yes. (That's the whole sentence.) I'll write the rest later. Please stay a while."  ✏ `dlg.hazel.courtship.r0`   (expr happy)
    r1 [hazel]: "Friends. A good word. A whole chapter, not a footnote. Thank you for saying it kindly."  ✏ `dlg.hazel.courtship.r1`   (expr happy)
    r2 [hazel]: "Time. Of course. Books wait on shelves for years. (I'll aim for less.)"  ✏ `dlg.hazel.courtship.r2`   (expr thinking)
  [hazel]: "{if flag:choice.courtship.hazel.yes}A courtship, then. A slow one. I'll start by bringing a second chair to the nook.{else}{if flag:choice.courtship.hazel.friends}Friends. (The best kind of margin note.) I'm glad. I'm also a little sad, politely.{else}Take the time you need. (The shelf keeps your place.) I'll be at the desk.{/if}{/if}"  ✏ `event.hazel_courtship.after`
  · effects: flag:event.hazel_courtship
  · effects: flag:courtship.asked.hazel

### hazel_courtship2 · "Hazel Asks Again" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=10 && flag:choice.courtship.hazel.later && !flag:event.hazel_courtship2.done && !flag:partners.hazel && !flag:choice.courtship.hazel.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I asked before. You asked for time. (I waited exactly as long as I could.) Would you like to court me?"  ✏ `event.hazel_courtship2.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.courtship` (player-facing graph, plays here):
    n0 [hazel]: "What do you say?"  ✏ `dlg.hazel.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:hazel,20, flag:choice.courtship.hazel.yes  ✏ `dlg.hazel.courtship.c0`
       ? "I'd rather we stay friends." → r1  effects: friend:hazel,15, flag:choice.courtship.hazel.friends  ✏ `dlg.hazel.courtship.c1`
       ? "Give me a little time." → r2  effects: friend:hazel,5, flag:choice.courtship.hazel.later  ✏ `dlg.hazel.courtship.c2`
    r0 [hazel]: "Yes. (That's the whole sentence.) I'll write the rest later. Please stay a while."  ✏ `dlg.hazel.courtship.r0`   (expr happy)
    r1 [hazel]: "Friends. A good word. A whole chapter, not a footnote. Thank you for saying it kindly."  ✏ `dlg.hazel.courtship.r1`   (expr happy)
    r2 [hazel]: "Time. Of course. Books wait on shelves for years. (I'll aim for less.)"  ✏ `dlg.hazel.courtship.r2`   (expr thinking)
  [hazel]: "{if flag:choice.courtship.hazel.yes}A courtship, then. Worth the wait. (The second chair goes in the nook tomorrow.){else}{if flag:choice.courtship.hazel.friends}Friends, then. A good ending. (Not every book needs a sequel.) I'm glad.{else}More time. (The shelf still has your place.) I'll be at the desk.{/if}{/if}"  ✏ `event.hazel_courtship2.after`
  · effects: flag:event.hazel_courtship2
  · effects: flag:courtship.asked.hazel, flag:event.hazel_courtship2.done

### hazel_partners · "Hazel, Together" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=10 && flag:choice.courtship.hazel.yes && !flag:partners.hazel && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "A page. One page, not a book. I wrote it for us. Please sit and read it with me."  ✏ `event.hazel_partners.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote heart)
  ▶ DIALOGUE `hazel.partners` (player-facing graph, plays here):
    n0 [hazel]: "The afternoon is ours. (Ione insisted. She's gone to dust something far away.) How shall we spend it?"  ✏ `dlg.hazel.partners.0`
       ? "Read the page together." → r0  effects: friend:hazel,10, flag:choice.courtship.hazel.read  ✏ `dlg.hazel.partners.c0`
       ? "Hold her hand over the desk." → r1  effects: friend:hazel,10, flag:choice.courtship.hazel.hand  ✏ `dlg.hazel.partners.c1`
       ? "Walk with her to the forest." → r2  effects: friend:hazel,10, flag:choice.courtship.hazel.walk  ✏ `dlg.hazel.partners.c2`
    r0 [hazel]: "(Aloud. Both of us.) It sounds truer in two voices. I didn't know that about pages."  ✏ `dlg.hazel.partners.r0`   (expr happy)
    r1 [hazel]: "(Oh.) Your hand is warm. Mine is ink-stained. (They match anyway.)"  ✏ `dlg.hazel.partners.r1`   (expr embarrassed)
    r2 [hazel]: "The forest. With you. The birds will be loud. (Let them. I'll be listening to you.)"  ✏ `dlg.hazel.partners.r2`   (expr happy)
  [hazel]: "{if choice:courtship.hazel.read}The page is pinned above the desk. Your name is at the bottom. In pencil, so it can be amended.{else}{if choice:courtship.hazel.hand}Your hand and mine, over the desk. Ione walked past twice. (She pretended not to notice.){else}The forest was loud. You were quiet. (I took notes. Mostly about you.){/if}{/if}"  ✏ `event.hazel_partners.after`
  · effects: flag:event.hazel_partners
  · effects: flag:partners.hazel, friend:hazel,50

### overheard_hazel_ione_1 · "Quiet Hours" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.ione && hearts:hazel>=2 && hearts:ione>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: hazel(24, 18), ione(26, 18)
  · (place hazel to (24,18))
  · (place ione to (26,18))
  · (hazel faces right)
  · (ione faces left)
  [hazel]: "The spring almanac. Third shelf, left, behind the mysteries. (Someone sorted those by color.)"  ✏ `event.overheard_hazel_ione_1.l1`
  [ione]: "You found it. I'd looked for a week. The library's quiet is really yours. I only dust it."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (hazel point (background))
  [hazel]: "Then we split it. You dust, I shelve. (The quiet stays undivided.)"  ✏ `event.overheard_hazel_ione_1.l3`
  [ione]: "Agreed. Back to the tea, then. I'll even stir quietly."
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
  [ione]: "There's a book on the desk seven years overdue. The card has your name on it, Hazel."
  [hazel]: "It's a very long book. (I'm on the last chapter. Again.)"  ✏ `event.overheard_hazel_ione_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (ione wave (background))
  [ione]: "You've been on the last chapter for seven years?"
  [hazel]: "Once a year, yes. (I fine myself four cents a time. I'm up to twenty-eight.)"  ✏ `event.overheard_hazel_ione_2.l4`
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
  [ione]: "I put a chair by the window. It has a cushion. It's for whoever needs it."
  [hazel]: "Oh. (It's for me.) ...I'm not going to say anything. It would only echo."  ✏ `event.overheard_hazel_ione_3.l2`
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (ione shrug (background))
  [ione]: "You didn't say a word. I heard it anyway. The window's warm after lunch."
  [hazel]: "After lunch, then. (The cushion has a lump. I'll fix it quietly.)"  ✏ `event.overheard_hazel_ione_3.l4`
  · effects: flag:event.overheard_hazel_ione_3

### overheard_dorian_hazel_1 · "The Quiet Bench" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.dorian && flag:met.hazel && hearts:dorian>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: dorian(24, 18), hazel(26, 18)
  · (place dorian to (24,18))
  · (place hazel to (26,18))
  · (dorian faces right)
  · (hazel faces left)
  [dorian]: "...Hazel."
  [hazel]: "Dorian. Hello. (The bench has room. Sit, if you like.)"  ✏ `event.overheard_dorian_hazel_1.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian point (background))
  [dorian]: "...Quiet today. Even the jays."
  [hazel]: "(The best kind.) I brought two books. One is for not reading."  ✏ `event.overheard_dorian_hazel_1.l4`
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
  [dorian]: "Old mushroom guide. I pressed leaves in it. Years of them."
  [hazel]: "Pressed leaves? May I borrow it? (They'll make better bookmarks than receipts.)"  ✏ `event.overheard_dorian_hazel_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian wave (background))
  [dorian]: "...They'd like that. Leaves like being kept."
  [hazel]: "Then we agree. I'll return it with one leaf more. (Maple. From the library step.)"  ✏ `event.overheard_dorian_hazel_2.l4`
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
  [dorian]: "...We could just sit. Not talk."
  [hazel]: "We could. (The library window faces the forest. The seat there is usually free.)"  ✏ `event.overheard_dorian_hazel_3.l2`
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (dorian shrug (background))
  [dorian]: "Then the trees can join us. They're good at it."
  [hazel]: "(They always come.) I'll save the window seat. You bring the quiet."  ✏ `event.overheard_dorian_hazel_3.l4`
  · effects: flag:event.overheard_dorian_hazel_3

### overheard_wren_hazel_1 · "The Notebook" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.hazel && hearts:wren>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), hazel(26, 18)
  · (place wren to (24,18))
  · (place hazel to (26,18))
  · (wren faces right)
  · (hazel faces left)
  [wren]: "Hazel, my notebook! What did I say about Juno last week?"
  [hazel]: "That she burned a clasp. Then, by Thursday, the forge. Then the idea of a forge."  ✏ `event.overheard_wren_hazel_1.l2`
  · (hazel shrug (background))
  · (hazel emote sweat)
  · (wren point (background))
  [wren]: "And which one's true? Careful. I'm attached to the forge."
  [hazel]: "The clasp. (The rest is yours.) I wrote all three versions down."  ✏ `event.overheard_wren_hazel_1.l4`
  · effects: flag:event.overheard_wren_hazel_1

### overheard_wren_hazel_2 · "The Corner Stool" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.hazel && hearts:wren>=4 && hearts:hazel>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_hazel_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), hazel(26, 18)
  · (place wren to (24,18))
  · (place hazel to (26,18))
  · (wren faces right)
  · (hazel faces left)
  [wren]: "Years of scribbling at my bar. Will you ever read me any of it?"
  [hazel]: "(No.) ...One line. Maybe. Not today, and not at the bar."  ✏ `event.overheard_wren_hazel_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (wren wave (background))
  [wren]: "One line's a start. I'll save you the corner stool. Nobody listens like that stool."
  [hazel]: "(I'll think about it.) That means yes. Slowly. Possibly by spring."  ✏ `event.overheard_wren_hazel_2.l4`
  · effects: flag:event.overheard_wren_hazel_2

### overheard_wren_hazel_3 · "One Line Aloud" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.hazel && hearts:wren>=6 && hearts:hazel>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_hazel_2 && flag:event.hazel_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: hazel(24, 18), wren(26, 18)
  · (place hazel to (24,18))
  · (place wren to (26,18))
  · (hazel faces right)
  · (wren faces left)
  [hazel]: "I read a poem aloud. To one person. (I survived. I checked twice.)"  ✏ `event.overheard_wren_hazel_3.l1`
  [wren]: "Hazel! To who? No, don't tell me. Yes, tell me."
  · (wren wave (background))
  · (wren emote heart)
  · (hazel point (background))
  [hazel]: "(Ask me again when we're friends.) ...Oh. We are. That's inconvenient."  ✏ `event.overheard_wren_hazel_3.l3`
  [wren]: "Between us, we always were. Now read me that line."
  · effects: flag:event.overheard_wren_hazel_3

### overheard_hazel_piper_1 · "Pencil Rhymes" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel! Someone's fixing the rhymes in my songbook. In very neat pencil."
  [hazel]: "(Shh.) An outdoor voice is still a voice. ...Also, it wasn't me."  ✏ `event.overheard_hazel_piper_1.l2`
  · (hazel shrug (background))
  · (hazel emote exclaim)
  · (piper point (background))
  [piper]: "It was you! 'Moon' doesn't rhyme with 'spoon' now. It rhymes with 'soon'. It's better!"
  [hazel]: "(It always wanted 'soon'.) You're welcome. ...I mean, whoever it was."  ✏ `event.overheard_hazel_piper_1.l4`
  · effects: flag:event.overheard_hazel_piper_1

### overheard_hazel_piper_2 · "Eight Syllables" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Help. My new chorus has nine syllables, and the tune only has eight."
  [hazel]: "(Cut 'really'.) Nobody has ever needed 'really'."  ✏ `event.overheard_hazel_piper_2.l2`
  · (hazel point (background))
  · (hazel emote question)
  · (piper shrug (background))
  [piper]: "...Oh! That's it! Where did you learn songs, Hazel?"
  [hazel]: "(Books.) And you. Every Thursday, back corner, behind a very large book."  ✏ `event.overheard_hazel_piper_2.l4`
  · effects: flag:event.overheard_hazel_piper_2

### overheard_hazel_piper_3 · "The Hidden Verse" · surprise
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.hazel && flag:met.piper && hearts:hazel>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_hazel_piper_2 && flag:event.piper_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: piper(24, 18), hazel(26, 18)
  · (place piper to (24,18))
  · (place hazel to (26,18))
  · (piper faces right)
  · (hazel faces left)
  [piper]: "Hazel. Write me one verse. Just one. Your words, my tune."
  [hazel]: "(I can't.) ...I already did. It's in your fiddle case, under the rosin."  ✏ `event.overheard_hazel_piper_3.l2`
  · (hazel shrug (background))
  · (hazel emote sparkle)
  · (piper wave (background))
  [piper]: "That's where it came from? It's the best verse I own! It should have your name on it."
  [hazel]: "(Not my name.) Just play it. Loudly. (It was always meant to be loud.)"  ✏ `event.overheard_hazel_piper_3.l4`
  · effects: flag:event.overheard_hazel_piper_3
