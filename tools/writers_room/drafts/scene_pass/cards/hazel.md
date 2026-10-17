# Scene cards for hazel

25 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 54 spoken lines in scenes, 28 dialogue lines and 21 player choice labels.

### hazel_heart2 · "Come Look at This" · wholesome
event · npc_roster.json · map Library · priority 6
plays when: `hearts:hazel>=2 && !weekday:sat && hour>=9 && hour<17`
beats may be ADDED or CUT in this scene
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Shh. Come look at this."  ✏ `event.hazel_heart2.0`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote ellipsis)
  [hazel]: "A pressed flower, a hundred years old."  ✏ `event.hazel_heart2.1`
  [hazel]: "Isn't it wonderful?"  ✏ `event.hazel_heart2.2`
  · effects: give:forage.snowdrop,1, friend:hazel,40, flag:event.hazel_heart2

### hazel_heart5 · "A Small Poem" · wholesome
event · npc_roster.json · map Library · priority 7
plays when: `hearts:hazel>=5 && !weekday:sat && hour>=9 && hour<17 && flag:event.hazel_heart2`
beats may be ADDED or CUT in this scene
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I wrote you something."  ✏ `event.hazel_heart5.0`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote ellipsis)
  [hazel]: "It's just a small poem. Don't laugh."  ✏ `event.hazel_heart5.1`
  [hazel]: "Thank you for listening."  ✏ `event.hazel_heart5.2`
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
  [ione]: "Four chairs, one lamp, and tea nobody ordered. This is my not-nervous face. Does it work?"
  [hazel]: "(It doesn't.) I brought something. I haven't decided whether to read it."  ✏ `event.reading_night.1`  (expr embarrassed)
  ▶ DIALOGUE `reading.hazel` (player-facing graph, plays here):
    n0 [hazel]: "(Nobody has to hear it.) What would you do? ...I mean, if you were me."  ✏ `dlg.reading.hazel.0`
       ? "Read it. We're listening." → r0  effects: flag:choice.reading.poem, friend:hazel,10  ✏ `dlg.reading.hazel.c0`
       ? "Read something borrowed." → r1  effects: flag:choice.reading.borrowed, friend:hazel,10  ✏ `dlg.reading.hazel.c1`
       ? "Listen first. Ione can start." → r2  effects: flag:choice.reading.listen, friend:hazel,10  ✏ `dlg.reading.hazel.c2`
    r0 [hazel]: "(Fine.) A short one. Eleven lines. (I counted twice.)"  ✏ `dlg.reading.hazel.r0`
    r1 [hazel]: "Someone else's, then. Kinder to the room. (Mine can wait.)"  ✏ `dlg.reading.hazel.r1`
    r2 [hazel]: "Then Ione goes first. (Thank you.) I'll stay."  ✏ `dlg.reading.hazel.r2`
  [hazel]: "The lamp does not mind the dark. It keeps it company. (That is all of it. That is the poem.)"  ✏ `event.reading_night.poem`  (expr thinking) [only if flag:choice.reading.poem]
  [ione]: "That is a good poem. I won't say how many times I'll read it. Once. Many times." [only if flag:choice.reading.poem]
  [narration]: "Ione reads two pages about the winter sea, and Hazel listens with her eyes shut. Hers can wait."  ✏ `event.reading_night.borrowed` [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and stays."  ✏ `event.reading_night.listen` [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "I brought a flower. Pressed. For the lamp."
  [wren]: "I'll bridge it, friend. Everything good was told aloud first. Then somebody wrote it down so it would stay." [only if flag:met.wren && hearts:wren>=3]
  [ione]: "That is a night of readers. I counted twice, hoping for more. I will count again."  (expr happy)
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
  [hazel]: "(One lantern each.) (A small light, and a small wish.) Please keep yours private."  ✏ `event.festival_winter_lanterns.1`
  · (hazel emote sparkle)
  · (sfx lantern)
  [felix]: "It's this big. The wish, I mean. I'll say no more. The lantern will do the telling."
  [ione]: "Now. Together, and slowly. Let them go. The dark will do the rest."
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
  [hazel]: "I'd like to swap something. (A book for a book.) Is that odd?"  ✏ `event.hazel_heart4.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.swap` (player-facing graph, plays here):
    n0 [hazel]: "What would you swap? (I'm not choosing for you.)"  ✏ `dlg.hazel.swap.0`
       ? "Your favorite book." → r0  effects: friend:hazel,10, flag:choice.hazel.swap.book  ✏ `dlg.hazel.swap.c0`
       ? "A recipe card." → r1  effects: friend:hazel,10, flag:choice.hazel.swap.recipe  ✏ `dlg.hazel.swap.c1`
       ? "I don't read much." → r2  effects: friend:hazel,10, flag:choice.hazel.swap.honest  ✏ `dlg.hazel.swap.c2`
    r0 [hazel]: "Oh. (She holds it with both hands.) I'll read it twice."  ✏ `dlg.hazel.swap.r0`   (expr happy)
    r1 [hazel]: "A recipe. (A book without a spine.) I like that."  ✏ `dlg.hazel.swap.r1`
    r2 [hazel]: "Thank you for saying so. (Most people pretend.) I'll pick something short."  ✏ `dlg.hazel.swap.r2`
  [hazel]: "Thank you. (I'll look after it.) I always do."  ✏ `event.hazel_heart4.after`
  · effects: flag:event.hazel_heart4
  · effects: friend:hazel,50

### hazel_heart6 · "Behind the Shelves" · surprise
confession · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=6 && flag:event.hazel_heart5 && flag:event.hazel_heart4 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Could you come to the back? (There's something I don't say out loud.)"  ✏ `event.hazel_heart6.intro`
  · (move player to (4,4))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on hazel)
  · (hazel expression thinking)
  ▶ DIALOGUE `hazel.hiding` (player-facing graph, plays here):
    n0 [hazel]: "I hide in the stacks when it's loud. (It's quieter between the books.) Is that strange?"  ✏ `dlg.hazel.hiding.0`   (expr thinking)
       ? "Ask what she hears in the quiet." → r0  effects: friend:hazel,10, flag:choice.hazel.hiding.ask  ✏ `dlg.hazel.hiding.c0`
       ? "Sit with her in silence." → r1  effects: friend:hazel,15, flag:choice.hazel.hiding.wait  ✏ `dlg.hazel.hiding.c1`
       ? "Joke about hiding in a book." → r2  effects: friend:hazel,5, flag:choice.hazel.hiding.joke  ✏ `dlg.hazel.hiding.c2`
    r0 [hazel]: "Pages turning, far away. (Someone else's thoughts, stored.) It's company."  ✏ `dlg.hazel.hiding.r0`
    r1 [hazel]: "...Yes. This. (Thank you.)"  ✏ `dlg.hazel.hiding.r1`   (expr happy)
    r2 [hazel]: "Ha. (A small one.) You're not wrong."  ✏ `dlg.hazel.hiding.r2`
  · (hazel expression neutral)
  [hazel]: "Thank you for not laughing. (I checked your face.) It stayed kind."  ✏ `event.hazel_heart6.after`
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
  [hazel]: "Would you hear a thing I wrote? (Only one of you. Just one.)"  ✏ `event.hazel_heart8.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.rehearsal` (player-facing graph, plays here):
    n0 [hazel]: "...and so the lamp burned lower, and nobody minded. (That's the end.) What did you hear?"  ✏ `dlg.hazel.rehearsal.0`   (expr embarrassed)
       ? "It was lovely." → r0  effects: friend:hazel,10, flag:choice.hazel.rehearsal.praise  ✏ `dlg.hazel.rehearsal.c0`
       ? "The middle dragged a little." → r1  effects: friend:hazel,15, flag:choice.hazel.rehearsal.critique  ✏ `dlg.hazel.rehearsal.c1`
       ? "Laugh gently at the lamp." → r2  effects: friend:hazel,10, flag:choice.hazel.rehearsal.laugh  ✏ `dlg.hazel.rehearsal.c2`
    r0 [hazel]: "Lovely. (I'll hold on to that word.) Thank you."  ✏ `dlg.hazel.rehearsal.r0`   (expr happy)
    r1 [hazel]: "The middle. (I knew it.) Thank you for saying so out loud."  ✏ `dlg.hazel.rehearsal.r1`
    r2 [hazel]: "The lamp is a bit much. (It was supposed to be.) I'll keep it."  ✏ `dlg.hazel.rehearsal.r2`
  [hazel]: "I'll read it properly, one day. (To more than one.)"  ✏ `event.hazel_heart8.after`
  · effects: flag:event.hazel_heart8
  · effects: friend:hazel,70

### hazel_heart10 · "The Reading" · wholesome
performance · slice_events.json · map Library · priority 7
plays when: `hearts:hazel>=10 && flag:event.hazel_heart8 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "I'll read for you. (Just you. The shelves don't count.)"  ✏ `event.hazel_heart10.intro`
  · (move player to (4,4))
  · (player faces up)
  · (lighting warm)
  · (letterbox on)
  · (camera focus on hazel)
  · (music hazel_theme)
  · (together: hazel dance; hazel emote note)
  · (sfx coin)
  · (music stop)
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  [hazel]: "{if choice:hazel.rehearsal.critique}I cut the middle. (You were right.) Thank you.{else}I kept the lamp. (And you.) Thank you.{/if}"  ✏ `event.hazel_heart10.outro`
  · effects: flag:event.hazel_heart10
  · effects: friend:hazel,80

### hazel_friend · "Overdue" · funny
helping_scene · slice_events.json · map Library · priority 7 · repeatable
plays when: `!flag:event.hazel_friend && hearts:hazel>=3 && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "Do you have blackberries? (I'm baking a peace offering for an overdue book.)"  ✏ `event.hazel_friend.ask`
  · (move player to (4,4))
  · (player faces up)
  · (hazel emote question)
  [hazel]: "Come back with blackberries? (It's a small crisis. A quiet one.)"  ✏ `event.hazel_friend.fallback`
  [hazel]: "Perfect. (He'll forgive me. Or at least eat.) Thank you."  ✏ `event.hazel_friend.success`
  · effects: flag:event.hazel_friend
  · effects: friend:hazel,15

### hazel_birthday · "Hazel's Birthday" · wholesome
conversation · wave4_birthdays.json · map Library · priority 8 · repeatable
plays when: `hearts:hazel>=2 && birthday.in:hazel==0 && unseen:birthday.hazel && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "(It's my birthday. I wasn't going to say.) Ione set out a very small cake. Please sit. Quietly."  ✏ `event.hazel_birthday.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.birthday` (player-facing graph, plays here):
    n0 [hazel]: "(You came anyway.) What happens now?"  ✏ `dlg.hazel.birthday.0`
       ? "Wish her a happy birthday." → r0  effects: friend:hazel,10, flag:choice.bday.hazel.wish  ✏ `dlg.hazel.birthday.c0`
       ? "Read her a line from her favorite book." → r1  effects: friend:hazel,10, flag:choice.bday.hazel.read  ✏ `dlg.hazel.birthday.c1`
       ? "Sit with her in silence." → r2  effects: friend:hazel,10, flag:choice.bday.hazel.sit  ✏ `dlg.hazel.birthday.c2`
    r0 [hazel]: "Thank you. (That's four words, said kindly.) I'll keep them."  ✏ `dlg.hazel.birthday.r0`   (expr happy)
    r1 [hazel]: "That one. (You remembered.) I may need a moment."  ✏ `dlg.hazel.birthday.r1`   (expr embarrassed)
    r2 [hazel]: "...Yes. This. (The best present.) Please stay a while."  ✏ `dlg.hazel.birthday.r2`   (expr happy)
  [hazel]: "{if choice:bday.hazel.wish}Four kind words. I wrote them in the margin of my day.{else}{if choice:bday.hazel.read}You read my favorite line. I've never heard it aloud. It sounds true.{else}Silence, shared. It's the quietest birthday I've ever had. I loved it.{/if}{/if}"  ✏ `event.hazel_birthday.after`
  · effects: flag:event.hazel_birthday
  · effects: mark:birthday.hazel, friend:hazel,30, give:artisan.jam,2, mail:hazel_bday_thanks

### hazel_courtship · "Hazel Asks" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=8 && flag:event.hazel_heart8 && !flag:partners.hazel && !flag:choice.courtship.hazel.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "(I've drafted this in four versions.) May I ask something? Plainly. (Would you like to court me?) I didn't plan the last word."  ✏ `event.hazel_courtship.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.courtship` (player-facing graph, plays here):
    n0 [hazel]: "What do you say?"  ✏ `dlg.hazel.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:hazel,20, flag:choice.courtship.hazel.yes  ✏ `dlg.hazel.courtship.c0`
       ? "I'd rather stay friends." → r1  effects: friend:hazel,15, flag:choice.courtship.hazel.friends  ✏ `dlg.hazel.courtship.c1`
       ? "Give me time." → r2  effects: friend:hazel,5, flag:choice.courtship.hazel.later  ✏ `dlg.hazel.courtship.c2`
    r0 [hazel]: "(Yes.) That's the whole sentence. Yes. I'll write the rest later. Please stay a while."  ✏ `dlg.hazel.courtship.r0`   (expr happy)
    r1 [hazel]: "(Friends.) A good word. A full chapter. Thank you for saying it kindly."  ✏ `dlg.hazel.courtship.r1`   (expr happy)
    r2 [hazel]: "(Time.) Of course. Books wait. So can I. Quietly, at the desk."  ✏ `dlg.hazel.courtship.r2`   (expr thinking)
  [hazel]: "{if flag:choice.courtship.hazel.yes}A courtship, then. (A slow one.) I'll start by saving you the good chair.{else}{if flag:choice.courtship.hazel.friends}Friends. (The best kind of margin note.) I'm glad. I'm also a little sad, politely.{else}Take all the time. (The shelf will keep your place.) Come back whenever.{/if}{/if}"  ✏ `event.hazel_courtship.after`
  · effects: flag:event.hazel_courtship
  · effects: flag:courtship.asked.hazel

### hazel_courtship2 · "Hazel Asks Again" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=10 && flag:choice.courtship.hazel.later && !flag:event.hazel_courtship2.done && !flag:partners.hazel && !flag:choice.courtship.hazel.friends && !flag:choice.courtship.wren.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "(I've drafted this in four versions.) May I ask something? Plainly. (Would you like to court me?) I didn't plan the last word."  ✏ `event.hazel_courtship2.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote ellipsis)
  ▶ DIALOGUE `hazel.courtship` (player-facing graph, plays here):
    n0 [hazel]: "What do you say?"  ✏ `dlg.hazel.courtship.0`
       ? "Yes. I'd like that very much." → r0  effects: friend:hazel,20, flag:choice.courtship.hazel.yes  ✏ `dlg.hazel.courtship.c0`
       ? "I'd rather stay friends." → r1  effects: friend:hazel,15, flag:choice.courtship.hazel.friends  ✏ `dlg.hazel.courtship.c1`
       ? "Give me time." → r2  effects: friend:hazel,5, flag:choice.courtship.hazel.later  ✏ `dlg.hazel.courtship.c2`
    r0 [hazel]: "(Yes.) That's the whole sentence. Yes. I'll write the rest later. Please stay a while."  ✏ `dlg.hazel.courtship.r0`   (expr happy)
    r1 [hazel]: "(Friends.) A good word. A full chapter. Thank you for saying it kindly."  ✏ `dlg.hazel.courtship.r1`   (expr happy)
    r2 [hazel]: "(Time.) Of course. Books wait. So can I. Quietly, at the desk."  ✏ `dlg.hazel.courtship.r2`   (expr thinking)
  [hazel]: "{if flag:choice.courtship.hazel.yes}A courtship, then. (A slow one.) I'll start by saving you the good chair.{else}{if flag:choice.courtship.hazel.friends}Friends. (The best kind of margin note.) I'm glad. I'm also a little sad, politely.{else}Take all the time. (The shelf will keep your place.) Come back whenever.{/if}{/if}"  ✏ `event.hazel_courtship2.after`
  · effects: flag:event.hazel_courtship2
  · effects: flag:courtship.asked.hazel, flag:event.hazel_courtship2.done

### hazel_partners · "Hazel, Together" · wholesome
conversation · wave5_courtship.json · map Library · priority 8
plays when: `hearts:hazel>=10 && flag:choice.courtship.hazel.yes && !flag:partners.hazel && !weekday:sat && hour>=9 && hour<17`
text-only scene: rewrite lines, do not add or cut beats
placed: player(4, 4)
  · (hazel faces down)
  [hazel]: "(I wrote you something.) It's not a book. It's a page. A single page. Please sit. Read it with me."  ✏ `event.hazel_partners.intro`
  · (move player to (4,4))
  · (player faces up)
  · (hazel wave (background))
  · (hazel emote heart)
  ▶ DIALOGUE `hazel.partners` (player-facing graph, plays here):
    n0 [hazel]: "(You came.) The evening is ours. How shall we spend it?"  ✏ `dlg.hazel.partners.0`
       ? "Read the page together." → r0  effects: friend:hazel,10, flag:choice.courtship.hazel.read  ✏ `dlg.hazel.partners.c0`
       ? "Hold her hand over the desk." → r1  effects: friend:hazel,10, flag:choice.courtship.hazel.hand  ✏ `dlg.hazel.partners.c1`
       ? "Walk her to the forest." → r2  effects: friend:hazel,10, flag:choice.courtship.hazel.walk  ✏ `dlg.hazel.partners.c2`
    r0 [hazel]: "(We read it aloud.) It sounds truer in two voices. That's a sentence I'll keep."  ✏ `dlg.hazel.partners.r0`   (expr happy)
    r1 [hazel]: "(Oh.) Your hand is warm. Mine is ink-stained. They match. Stay a while."  ✏ `dlg.hazel.partners.r1`   (expr embarrassed)
    r2 [hazel]: "The forest. With you. (The birds will be loud.) Let them be. I'll listen to you instead."  ✏ `dlg.hazel.partners.r2`   (expr happy)
  [hazel]: "{if choice:courtship.hazel.read}The page is pinned above the desk. It has your name at the bottom. In pencil, so it can be amended.{else}{if choice:courtship.hazel.hand}Your hand and my hand, over the desk. (I may reread it for years.){else}The forest was loud. You were quiet. It was the right balance.{/if}{/if}"  ✏ `event.hazel_partners.after`
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
  [hazel]: "(Third shelf, left. The mystery is back in order.)"  ✏ `event.overheard_hazel_ione_1.l1`
  [ione]: "Thank you. The library's silence is yours, you know. I only sweep it."
  · (ione shrug (background))
  · (ione emote ellipsis)
  · (hazel point (background))
  [hazel]: "(Then let's both keep it.)"  ✏ `event.overheard_hazel_ione_1.l3`
  [ione]: "Agreed. Back to the tea, then, quietly, and nobody has to say another word."
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
  [ione]: "There's a book on the desk that's seven years overdue. It's yours."
  [hazel]: "(It's a very long book.) (I'm on the last chapter.)"  ✏ `event.overheard_hazel_ione_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (ione wave (background))
  [ione]: "You're on a chapter every year, then?"
  [hazel]: "(It's a very slow chapter.) (I'm enjoying it.)"  ✏ `event.overheard_hazel_ione_2.l4`
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
  [ione]: "I left a chair by the window. It has a cushion. It's for whoever needs it."
  [hazel]: "(It's for me. Yes? Thank you.) (I won't say so aloud.)"  ✏ `event.overheard_hazel_ione_3.l2`
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (ione shrug (background))
  [ione]: "You've said it plainly. It's plenty. The window's warm after lunch. Sit there then."
  [hazel]: "(I'll come by.) (Quiet, shared. A good arrangement.)"  ✏ `event.overheard_hazel_ione_3.l4`
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
  [hazel]: "(Dorian.) Good morning. Sit, if you like."  ✏ `event.overheard_dorian_hazel_1.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian point (background))
  [dorian]: "...Quiet today."
  [hazel]: "(The best kind.) Stay as long as you need."  ✏ `event.overheard_dorian_hazel_1.l4`
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
  [dorian]: "...I found a mushroom guide. Pressed leaves inside."
  [hazel]: "(Pressed leaves.) May I borrow it? (I'll treat them as bookmarks.)"  ✏ `event.overheard_dorian_hazel_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (dorian wave (background))
  [dorian]: "...They'd like that. They like being kept."
  [hazel]: "(Then we agree.) I'll return it, with a leaf of my own."  ✏ `event.overheard_dorian_hazel_2.l4`
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
  [dorian]: "...We could just sit."
  [hazel]: "(We could.) The library window faces the forest."  ✏ `event.overheard_dorian_hazel_3.l2`
  · (hazel point (background))
  · (hazel emote ellipsis)
  · (dorian shrug (background))
  [dorian]: "...Then the trees can join us."
  [hazel]: "(They always do.) I'll keep a chair warm for you."  ✏ `event.overheard_dorian_hazel_3.l4`
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
  [hazel]: "(That she burned a clasp.) Then the forge. Then the idea of a forge."  ✏ `event.overheard_wren_hazel_1.l2`
  · (hazel shrug (background))
  · (hazel emote sweat)
  · (wren point (background))
  [wren]: "And which one's true?"
  [hazel]: "(The clasp.) The rest is yours. I wrote it all down."  ✏ `event.overheard_wren_hazel_1.l4`
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
  [hazel]: "(No.) ...One line, maybe. Not today."  ✏ `event.overheard_wren_hazel_2.l2`
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (wren wave (background))
  [wren]: "One line's a start. I'll save you the corner stool. Nobody listens like that stool."
  [hazel]: "(I'll think about it.) That means yes, slowly."  ✏ `event.overheard_wren_hazel_2.l4`
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
  [hazel]: "I read a poem out loud. (To one person.) It didn't kill me."  ✏ `event.overheard_wren_hazel_3.l1`
  [wren]: "Hazel! To who? No, don't tell me. Yes, tell me."
  · (wren wave (background))
  · (wren emote heart)
  · (hazel point (background))
  [hazel]: "(Ask me again when we're friends.) ...Oh. We are."  ✏ `event.overheard_wren_hazel_3.l3`
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
  [piper]: "Hazel! Someone's been fixing the rhymes in my songbook. In very neat pencil."
  [hazel]: "(Shh.) Outdoor voice is still a voice. ...And it wasn't me."  ✏ `event.overheard_hazel_piper_1.l2`
  · (hazel shrug (background))
  · (hazel emote exclaim)
  · (piper point (background))
  [piper]: "It was you! 'Moon' doesn't rhyme with 'spoon' anymore. It rhymes with 'soon'. It's better!"
  [hazel]: "(It was always better.) You're welcome."  ✏ `event.overheard_hazel_piper_1.l4`
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
  [piper]: "Help. My new chorus has nine syllables and the tune only has eight."
  [hazel]: "(Cut 'really'.) Nobody needs 'really'."  ✏ `event.overheard_hazel_piper_2.l2`
  · (hazel point (background))
  · (hazel emote question)
  · (piper shrug (background))
  [piper]: "...Oh. Oh, that's it! Who taught you songs?"
  [hazel]: "(Books.) And you. Every Thursday, from the back row."  ✏ `event.overheard_hazel_piper_2.l4`
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
  [hazel]: "(I can't.) ...I already did. It's in your fiddle case."  ✏ `event.overheard_hazel_piper_3.l2`
  · (hazel shrug (background))
  · (hazel emote sparkle)
  · (piper wave (background))
  [piper]: "That's where it came from? It's the best verse I own! I'll play it with your name on it."
  [hazel]: "(Not my name.) Just play it. That's plenty."  ✏ `event.overheard_hazel_piper_3.l4`
  · effects: flag:event.overheard_hazel_piper_3
