# Scene cards for wren

28 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 61 spoken lines in scenes, 32 dialogue lines and 22 player choice labels.

### wren_heart2 · "The New Recipe" · funny
event · npc_roster.json · map Saloon · priority 6
plays when: `hearts:wren>=2 && !weekday:tue && hour>=12 && hour<24`
beats may be ADDED or CUT in this scene
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Slow night, so I'm making a new recipe."  ✏ `event.wren_heart2.0`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  [wren]: "Taste this, [player]. Be honest."  ✏ `event.wren_heart2.1`
  [wren]: "Honest and kind. You pass."  ✏ `event.wren_heart2.2`
  · effects: give:artisan.juice,2, friend:wren,40, flag:event.wren_heart2

### wren_heart5 · "Not Just the Noise" · wholesome
event · npc_roster.json · map Saloon · priority 7
plays when: `hearts:wren>=5 && !weekday:tue && hour>=12 && hour<24 && flag:event.wren_heart2`
beats may be ADDED or CUT in this scene
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Everyone thinks I like the noise."  ✏ `event.wren_heart5.0`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  [wren]: "Truth is, I like the people. Thank you for being one."  ✏ `event.wren_heart5.1`
  [wren]: "Come back soon."  ✏ `event.wren_heart5.2`
  · effects: give:artisan.wine,1, friend:wren,60, flag:event.wren_heart5

### hall_dance · "Opening Night" · wholesome
event · spec_scenes.json · map CommunityHall · priority 8
plays when: `flag:hall.restored && flag:hall.roof && quest:wren_full_house=done && flag:event.piper_heart10 && flag:event.juno_heart10 && hour>=18 && hour<23 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: marcus(8, 6), tilda(5, 5), wren(7, 5), piper(6, 4), bram(9, 5), juno(8, 4), player(7, 2)
  · (letterbox on)
  · (lighting warm)
  · (place marcus to (8,6))
  · (marcus faces up)
  · (place tilda to (5,5))
  · (tilda faces right)
  · (place wren to (7,5))
  · (wren faces down)
  · (place piper to (6,4))
  · (piper faces down)
  · (place bram to (9,5))
  · (bram faces left)
  · (place juno to (8,4))
  · (juno faces down)
  · (move player to (7,2))
  · (player faces up)
  [marcus]: "I checked the roof once more. Then twice. It is finished."
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch, I've lost track of my own party."  ✏ `event.hall_dance.1`  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night. This is the night. You're first at the bar."  ✏ `event.hall_dance.2`  (expr happy)
  · (remove prop wine)
  [tilda]: "The roof's done, the hall's open, and nobody asked for credit. I'm writing it on the slate anyway."
  [piper]: "First verse, then I step off. I'm calm. I'm holding this fiddle like a hot loaf."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall sings the chorus. Piper has stopped playing, and nobody minds."  ✏ `event.hall_dance.6`
  [bram]: "Hm. Hm-hm. I know the tune. Say nothing."  (emote note)
  [juno]: "The hall door hangs on hinges I made. My mark's on every pin, and it swings like it's showing off!"  (expr happy)
  [wren]: "The roof pool is closed. Seven winters of coins, and Tilda had the right year."  ✏ `event.hall_dance.9`
  [tilda]: "Into the hall box it goes. Marcus, take one coin for the ladder."
  [marcus]: "Hm. One coin. One dance. Nobody tell the ladder."
  · (marcus dance (background))
  · (tilda dance (background))
  · (wren emote heart)
  · (juno emote note)
  · (wait 2.5s)
  [narration]: "Marcus takes his one dance, and Tilda lets him lead. The lamps burn low, and nobody goes home."  ✏ `event.hall_dance.12`
  · (music stop)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.hall_dance, friend:marcus,40, friend:tilda,40, friend:wren,40, friend:piper,40, friend:bram,40, friend:juno,40

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
  [dorian]: "I brought a flower. Pressed. For the lamp."
  [wren]: "I'll bridge it, friend. Everything good was told aloud first. Then somebody wrote it down so it would stay."  ✏ `event.reading_night.3` [only if flag:met.wren && hearts:wren>=3]
  [ione]: "That is a night of readers. I counted twice, hoping for more. I will count again."  (expr happy)
  · (remove prop flower)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.reading_night, friend:ione,40, friend:hazel,40, friend:dorian,40

### past_the_point · "Past the Point" · funny
event · spec_scenes.json · map Beach · priority 7
plays when: `flag:event.elara_heart8 && flag:event.felix_heart8 && hearts:felix>=6 && hearts:elara>=6 && hour>=5 && hour<10 && !weather:storm && !flag:event.past_the_point && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: felix(26, 5), elara(25, 6), wren(32, 7), player(27, 6)
  · (letterbox on)
  · (lighting dawn)
  · (place felix to (26,5))
  · (felix faces up)
  · (place elara to (25,6))
  · (elara faces up)
  · (place wren to (32,7))
  · (wren faces up)
  · (move player to (27,6))
  · (player faces up)
  [felix]: "Mornin'. Boat's ready. She leaks politely. The bailing's the elegant part."
  [elara]: "I knitted a life jacket! It isn't how they work. I brought it anyway, and a sandwich tin."  (emote sparkle, expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "Forty-two places on the list, and Felix says we can manage one before breakfast. Which one?"
       ? "Round the headland." → r0  effects: flag:choice.point.headland, friend:elara,5
       ? "The white rocks." → r1  effects: flag:choice.point.rocks, friend:elara,5
       ? "Straight out, until the shore is a line." → r2  effects: flag:choice.point.horizon, friend:elara,5
    r0 [elara]: "The headland! I've drawn it eleven times. Never from a boat."
    r1 [elara]: "The white rocks. I've never been closer than the bench. Row slowly!"
    r2 [elara]: "Straight out. I'll hold the sandwich tin. It's very steadying."
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks. Felix bails with one hand and rows with the other, and never spills his coffee."
  [elara]: "The headland is bigger than the map made it. Felix, it's enormous. It's the best thing I've ever been wrong about."  (expr happy) [only if flag:choice.point.headland]
  [elara]: "The white rocks look like teeth from here. Friendly teeth. I'd like to wave at them. May I wave?"  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "Out here the shore is only a line. I didn't know I could feel this small and this glad together."  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "Don't mind me, friends. I'm only looking. Only looking, I promise, from the point."  ✏ `event.past_the_point.wren` [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara takes out her list and crosses one place off with a stub of pencil, and smiles at the pencil."
  [felix]: "Told you, partner. Dawn's better with two. ...Better with her list in it, too."  (emote heart)
  · (fadeout)
  · (camera reset)
  · (lighting reset)
  · (fadein)
  · (letterbox off)
  · effects: flag:event.past_the_point, friend:felix,40, friend:elara,40

### story_cat · "Cat With Five Names" · wholesome
event · storyline_events2.json · map Village · priority 7
plays when: `storyline:five_names_cat && hearts:wren>=2 && hour>=9 && hour<19 && !festival.in:==0`
beats may be ADDED or CUT in this scene
placed: wren(25, 18), player(25, 17)
  · (place wren to (25,18))
  · (wren faces down)
  · (spawn prop prop.cat at (26,18))
  [wren]: "Don't move. A cat has chosen your boots. It won't tell me its name, and I've tried five."  ✏ `event.story_cat.intro`
  · (move player to (25,17))
  · (player faces up)
  · (wren emote heart)
  · (wren point (background))
  ▶ DIALOGUE `story.cat.talk` (player-facing graph, plays here):
    n0 [narration]: "A small gray cat sits on your boots, perfectly satisfied, and looks up at you with the patience of a landlord."  ✏ `dlg.story.cat.talk.0`   (→ n1)
    n1 [wren]: "Tilda calls it Ledger. Juno says Quench. Piper swears it's Sharp. Hazel files it under Index. I say Gossip. The cat answers to none of us."  ✏ `dlg.story.cat.talk.1`   (→ n2)
    n2 [wren]: "So. What do you call it?"  ✏ `dlg.story.cat.talk.2`
       ? "Ledger." → r0  effects: flag:choice.cat.ledger  ✏ `dlg.story.cat.talk.c0`
       ? "Quench." → r1  effects: flag:choice.cat.quench  ✏ `dlg.story.cat.talk.c1`
       ? "Sharp." → r2  effects: flag:choice.cat.sharp  ✏ `dlg.story.cat.talk.c2`
       ? "Something new." → r3  effects: flag:choice.cat.new, friend:wren,10  ✏ `dlg.story.cat.talk.c3`
    r0 [wren]: "Tilda's name. Careful. She keeps a very long slate."  ✏ `dlg.story.cat.talk.r0`
    r1 [wren]: "Juno's name. Brave. She'll label the cat's bowl."  ✏ `dlg.story.cat.talk.r1`
    r2 [wren]: "Piper's name. A fiddler's cat. It suits the whiskers."  ✏ `dlg.story.cat.talk.r2`
    r3 [wren]: "Something new! The cat tilts its head. ...It just purred. That's the one."  ✏ `dlg.story.cat.talk.r3`   (expr happy; emote heart)
  [wren]: "{if choice:cat.ledger}Ledger! Tilda will be insufferable. I'll tell her tonight.{else}{if choice:cat.quench}Quench! Juno will frame the vote. I'll tell her tonight.{else}{if choice:cat.sharp}Sharp! Piper will write a song. I'll tell her tonight.{else}A new name! The village will need a fresh slate. I'll tell everyone tonight.{/if}{/if}{/if}"  ✏ `event.story_cat.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_cat, friend:wren,20, flag:storydone.five_names_cat

### wren_heart4 · "Open Mic" · funny
prank · slice_events.json · map Saloon · priority 7
plays when: `hearts:wren>=4 && flag:event.wren_heart2 && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Don't sit yet. I'm trying something new, and I need a witness."  ✏ `event.wren_heart4.setup`
  · (move player to (9,6))
  · (player faces up)
  · (wren jiggle (background))
  · (sfx click)
  · (camera shake)
  · (player emote exclaim)
  [wren]: "Hush, everybody: the worst song ever written!"  ✏ `event.wren_heart4.prank`
  ▶ DIALOGUE `wren.openmic` (player-facing graph, plays here):
    n0 [wren]: "Well? Be honest. A performer needs one honest face in the crowd."  ✏ `dlg.wren.openmic.0`
       ? "Cheer like a whole crowd." → r0  effects: friend:wren,10, flag:choice.wren.openmic.cheer  ✏ `dlg.wren.openmic.c0`
       ? "Heckle her, gently." → r1  effects: friend:wren,10, flag:choice.wren.openmic.heckle  ✏ `dlg.wren.openmic.c1`
       ? "Hide behind the menu." → r2  effects: flag:choice.wren.openmic.hide  ✏ `dlg.wren.openmic.c2`
    r0 [wren]: "Oh, I could live on that. Do it again, but louder."  ✏ `dlg.wren.openmic.r0`   (expr happy)
    r1 [wren]: "A heckler! At last I'm somebody. I'll put you in the second verse."  ✏ `dlg.wren.openmic.r1`   (expr happy)
    r2 [wren]: "Ha! Fair. I'd hide too. Come out, though. The second verse is better."  ✏ `dlg.wren.openmic.r2`
  [wren]: "Terrible, wasn't it? I wrote it in the bath. The bath had notes."  ✏ `event.wren_heart4.reaction`
  · effects: flag:event.wren_heart4
  · effects: friend:wren,50

### wren_heart6 · "The Kitchen" · wholesome
conversation · slice_events.json · map Saloon · priority 7
plays when: `hearts:wren>=6 && flag:event.wren_heart5 && flag:event.wren_heart4 && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Come into the back. I'm testing a stew, and I trust nobody else's tongue."  ✏ `event.wren_heart6.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote note)
  ▶ DIALOGUE `wren.kitchen` (player-facing graph, plays here):
    n0 [wren]: "One more pinch of something. Pepper, cider, or leave it plain?"  ✏ `dlg.wren.kitchen.0`
       ? "Pepper. Be brave." → r0  effects: friend:wren,10, flag:choice.wren.kitchen.pepper  ✏ `dlg.wren.kitchen.c0`
       ? "Cider. Go soft." → r1  effects: friend:wren,10, flag:choice.wren.kitchen.cider  ✏ `dlg.wren.kitchen.c1`
       ? "Plain. It's already good." → r2  effects: friend:wren,10, flag:choice.wren.kitchen.plain  ✏ `dlg.wren.kitchen.c2`
    r0 [wren]: "Brave it is! If I weep, blame the pepper, not me."  ✏ `dlg.wren.kitchen.r0`   (emote sweat)
    r1 [wren]: "Cider. Sweet and a little sly. That's very you."  ✏ `dlg.wren.kitchen.r1`
    r2 [wren]: "Plain. Oh. You think it's already good? Nobody says that. I'm keeping you."  ✏ `dlg.wren.kitchen.r2`   (expr happy)
  [wren]: "Remember this one. If it goes wrong, I'll deny I ever made it."  ✏ `event.wren_heart6.after`
  · effects: flag:event.wren_heart6
  · effects: friend:wren,60

### wren_heart8 · "The Bag" · surprise
confession · slice_events.json · map Saloon · priority 7
plays when: `hearts:wren>=8 && flag:event.wren_heart6 && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Close the door behind you. No, the other one. Yes. Sit."  ✏ `event.wren_heart8.intro`
  · (move player to (9,6))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on wren)
  · (wren expression thinking)
  ▶ DIALOGUE `wren.bag` (player-facing graph, plays here):
    n0 [wren]: "There's a bag under my bar. Packed since the day I arrived. I haven't opened it in years, and I haven't unpacked it either."  ✏ `dlg.wren.bag.0`   (→ n1; expr thinking)
    n1 [wren]: "I came here to rest for a season. That was eight years ago. Tell me: should I go, stay, or just leave it under the bar?"  ✏ `dlg.wren.bag.1`
       ? "Go. See what's out there." → r0  effects: friend:wren,10, flag:choice.wren.bag.go  ✏ `dlg.wren.bag.c0`
       ? "Stay. We'd all miss you." → r1  effects: friend:wren,10, flag:choice.wren.bag.stay  ✏ `dlg.wren.bag.c1`
       ? "Just listen to her." → r2  effects: friend:wren,15, flag:choice.wren.bag.listen  ✏ `dlg.wren.bag.c2`
    r0 [wren]: "Go. Just like that. You say it so plainly. I might. Someday. With a very good map."  ✏ `dlg.wren.bag.r0`
    r1 [wren]: "Stay. Oh. That's a nicer word than I expected."  ✏ `dlg.wren.bag.r1`
    r2 [wren]: "...You didn't tell me what to do. That's the kindest thing anyone has done. Thank you."  ✏ `dlg.wren.bag.r2`   (expr sad)
  · (wren expression neutral)
  [wren]: "Thank you for letting me say it out loud. It weighs less now."  ✏ `event.wren_heart8.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.wren_heart8
  · effects: friend:wren,70

### wren_heart10 · "One Listener" · wholesome
performance · slice_events.json · map Saloon · priority 7
plays when: `hearts:wren>=10 && flag:event.wren_heart8 && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Tonight's a performance. One song, one listener. I picked you."  ✏ `event.wren_heart10.intro`
  · (move player to (9,6))
  · (player faces up)
  · (lighting warm)
  · (letterbox on)
  · (camera focus on wren)
  · (music wren_theme)
  · (together: wren dance; wren emote note)
  · (sfx coin)
  · (music stop)
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  [wren]: "{if choice:wren.bag.go}I'm not leaving yet. But I wrote the map into the second verse.{else}{if choice:wren.bag.stay}I wrote the stay into the chorus. You'll hear it.{else}I wrote the quiet part for you. You'll know which.{/if}{/if}"  ✏ `event.wren_heart10.outro`
  · effects: flag:event.wren_heart10
  · effects: friend:wren,80

### wren_friend · "Gossip Bingo" · funny
shared_activity · slice_events.json · map Saloon · priority 7
plays when: `hearts:wren>=3 && weekday:fri && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Friday means bingo. Not numbers. Gossip. Come and cross something off."  ✏ `event.wren_friend.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  · (wren hop)
  [wren]: "Someone's cat is in love with the postal cart. I'm marking it."  ✏ `event.wren_friend.doing`
  [wren]: "No bingo today. Only a full house of nonsense. Wonderful."  ✏ `event.wren_friend.wrap`
  · effects: flag:event.wren_friend
  · effects: friend:wren,15

### wren_birthday · "Wren's Birthday" · funny
conversation · wave4_birthdays.json · map Saloon · priority 8 · repeatable
plays when: `hearts:wren>=2 && birthday.in:wren==0 && unseen:birthday.wren && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "It's my birthday and nobody's allowed to look surprised. Sit. Be fabulous. Have a pickle."  ✏ `event.wren_birthday.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote note)
  ▶ DIALOGUE `wren.birthday` (player-facing graph, plays here):
    n0 [wren]: "Now. Between us, I expect a toast. What's it going to be?"  ✏ `dlg.wren.birthday.0`
       ? "Raise a glass to her." → r0  effects: friend:wren,10, flag:choice.bday.wren.glass  ✏ `dlg.wren.birthday.c0`
       ? "Sing something badly." → r1  effects: friend:wren,10, flag:choice.bday.wren.sing  ✏ `dlg.wren.birthday.c1`
       ? "Give her a quiet hug." → r2  effects: friend:wren,10, flag:choice.bday.wren.hug  ✏ `dlg.wren.birthday.c2`
    r0 [wren]: "To me! Oh, the best toast I've had all year, and I wrote the other ones."  ✏ `dlg.wren.birthday.r0`   (expr happy)
    r1 [wren]: "That was dreadful. Do it again. Louder. The stool is crying."  ✏ `dlg.wren.birthday.r1`   (expr happy)
    r2 [wren]: "Oh. ...Oh, you're a menace. A kind one. Sit down before I embarrass us both."  ✏ `dlg.wren.birthday.r2`   (expr happy)
  [wren]: "{if choice:bday.wren.glass}A toast, and a good one. I'll tell it for years, with improvements.{else}{if choice:bday.wren.sing}Your song's the new house anthem. Nobody asked. It's permanent.{else}A hug. Eight years in Wetherell, and that's the best gift on the shelf.{/if}{/if}"  ✏ `event.wren_birthday.after`
  · effects: flag:event.wren_birthday
  · effects: mark:birthday.wren, friend:wren,30, give:artisan.pickles,2, mail:wren_bday_thanks

### wren_courtship · "Wren Asks" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:wren>=8 && flag:event.wren_heart8 && !flag:partners.wren && !flag:choice.courtship.wren.friends && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Sit. I've rehearsed this in the mirror, in the cellar and once to a pickle. I like you. More than the pickle. Quite a lot more."  ✏ `event.wren_courtship.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote note)
  ▶ DIALOGUE `wren.courtship` (player-facing graph, plays here):
    n0 [wren]: "What do you say?"  ✏ `dlg.wren.courtship.0`
       ? "I'd like to court you." → r0  effects: friend:wren,20, flag:choice.courtship.wren.yes  ✏ `dlg.wren.courtship.c0`
       ? "Let's stay the best of friends." → r1  effects: friend:wren,15, flag:choice.courtship.wren.friends  ✏ `dlg.wren.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:wren,5, flag:choice.courtship.wren.later  ✏ `dlg.wren.courtship.c2`
    r0 [wren]: "Court. Oh. Yes. Properly, with walks and terrible poems. I'll write the first one tonight and burn it."  ✏ `dlg.wren.courtship.r0`   (expr happy)
    r1 [wren]: "Friends. Yes. The best kind, I'd say. Nothing changes and everything stays. I'm relieved, and a little proud."  ✏ `dlg.wren.courtship.r1`   (expr happy)
    r2 [wren]: "Wait. Fair. I've waited eight years for a decent stool. I can wait for the right moment."  ✏ `dlg.wren.courtship.r2`   (expr thinking)
  [wren]: "{if flag:choice.courtship.wren.yes}Walks, then. And poems. You're stuck with me, and I'm stuck with the stool.{else}{if flag:choice.courtship.wren.friends}The best of friends. It's written on the slate in my head, in capital letters.{else}Whenever you like. I'll be on the stage, being patient. It's a new look.{/if}{/if}"  ✏ `event.wren_courtship.after`
  · effects: flag:event.wren_courtship
  · effects: flag:courtship.asked.wren

### wren_courtship2 · "Wren Asks Again" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:wren>=10 && flag:choice.courtship.wren.later && !flag:event.wren_courtship2.done && !flag:partners.wren && !flag:choice.courtship.wren.friends && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Sit. I've rehearsed this in the mirror, in the cellar and once to a pickle. I like you. More than the pickle. Quite a lot more."  ✏ `event.wren_courtship2.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote note)
  ▶ DIALOGUE `wren.courtship` (player-facing graph, plays here):
    n0 [wren]: "What do you say?"  ✏ `dlg.wren.courtship.0`
       ? "I'd like to court you." → r0  effects: friend:wren,20, flag:choice.courtship.wren.yes  ✏ `dlg.wren.courtship.c0`
       ? "Let's stay the best of friends." → r1  effects: friend:wren,15, flag:choice.courtship.wren.friends  ✏ `dlg.wren.courtship.c1`
       ? "Can we wait a while?" → r2  effects: friend:wren,5, flag:choice.courtship.wren.later  ✏ `dlg.wren.courtship.c2`
    r0 [wren]: "Court. Oh. Yes. Properly, with walks and terrible poems. I'll write the first one tonight and burn it."  ✏ `dlg.wren.courtship.r0`   (expr happy)
    r1 [wren]: "Friends. Yes. The best kind, I'd say. Nothing changes and everything stays. I'm relieved, and a little proud."  ✏ `dlg.wren.courtship.r1`   (expr happy)
    r2 [wren]: "Wait. Fair. I've waited eight years for a decent stool. I can wait for the right moment."  ✏ `dlg.wren.courtship.r2`   (expr thinking)
  [wren]: "{if flag:choice.courtship.wren.yes}Walks, then. And poems. You're stuck with me, and I'm stuck with the stool.{else}{if flag:choice.courtship.wren.friends}The best of friends. It's written on the slate in my head, in capital letters.{else}Whenever you like. I'll be on the stage, being patient. It's a new look.{/if}{/if}"  ✏ `event.wren_courtship2.after`
  · effects: flag:event.wren_courtship2
  · effects: flag:courtship.asked.wren, flag:event.wren_courtship2.done

### wren_partners · "Wren, Together" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:wren>=10 && flag:choice.courtship.wren.yes && !flag:partners.wren && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Evening, my darling pickle-eater. I wrote you a song. It's rubbish. It's mine. Sit and be sung at."  ✏ `event.wren_partners.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote heart)
  ▶ DIALOGUE `wren.partners` (player-facing graph, plays here):
    n0 [wren]: "The good stool's free and the night's young. How shall we spend it?"  ✏ `dlg.wren.partners.0`
       ? "Listen to the whole song." → r0  effects: friend:wren,10, flag:choice.courtship.wren.song  ✏ `dlg.wren.partners.c0`
       ? "Dance with her between the tables." → r1  effects: friend:wren,10, flag:choice.courtship.wren.dance  ✏ `dlg.wren.partners.c1`
       ? "Walk her home under the stars." → r2  effects: friend:wren,10, flag:choice.courtship.wren.walk  ✏ `dlg.wren.partners.c2`
    r0 [wren]: "Every verse! Oh, you angel. I'll sing the bridge twice, because it's my favorite."  ✏ `dlg.wren.partners.r0`   (expr happy)
    r1 [wren]: "Dance! In the saloon! The floor's sticky and I don't care. Hold on."  ✏ `dlg.wren.partners.r1`   (expr happy)
    r2 [wren]: "A walk home. Past the bakery, past the lamp, past all my old excuses. Lead on."  ✏ `dlg.wren.partners.r2`   (expr happy)
  [wren]: "{if choice:courtship.wren.song}The song's finished now. It has your name in the second verse and an extra chorus.{else}{if choice:courtship.wren.dance}We danced. Nobody looked. Everybody looked. I loved it.{else}The bag under my bar is empty. I unpacked it this morning. You were the reason, quietly.{/if}{/if}"  ✏ `event.wren_partners.after`
  · effects: flag:event.wren_partners
  · effects: flag:partners.wren, friend:wren,50

### overheard_wren_piper_1 · "Sound Check" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "You're flat on the third bar. Fabulously flat. Do it again."  ✏ `event.overheard_wren_piper_1.l1`
  [piper]: "I'm not flat, the stool's tilted! It's a stage stool, Wren, it has opinions!"
  · (piper shrug (background))
  · (piper emote note)
  · (wren point (background))
  [wren]: "The stool doesn't have opinions. I do. Again. From the top."  ✏ `event.overheard_wren_piper_1.l3`
  [piper]: "Fine, fine, from the top. But I'm taking the bridge up a key out of spite."
  · effects: flag:event.overheard_wren_piper_1

### overheard_wren_piper_2 · "The Shared Verse" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "I wrote a verse. It's for the saloon. It's about you, and I'm sorry."  ✏ `event.overheard_wren_piper_2.l1`
  [piper]: "Wren! You wrote a verse? Read it! Read it! No, wait. Sing it. Sing it badly. I'll fix it."
  · (piper shrug (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "'Piper sings and the rafters shake, and every glass in town awake.' That's all I've got."  ✏ `event.overheard_wren_piper_2.l3`
  [piper]: "It's perfect. I'm stealing it. It's the opening of my next song, and you're getting credit in tiny letters."
  · effects: flag:event.overheard_wren_piper_2

### overheard_wren_piper_3 · "Closing Time" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_2`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Today's last song, Piper. The room will be tired. So will I. Make it a soft one."  ✏ `event.overheard_wren_piper_3.l1`
  [piper]: "Soft, I can do. I'll hum. No words. Just you and the stool and the sunlight."
  · (piper point (background))
  · (piper emote note)
  · (wren shrug (background))
  [wren]: "My darling, that's the best verse you've written. Maybe write it down. In pencil."  ✏ `event.overheard_wren_piper_3.l3`
  [piper]: "Pencil it is. It'll live in the room. Save me the last glass."
  · effects: flag:event.overheard_wren_piper_3

### overheard_wren_piper_4 · "The Headliner" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.piper && hearts:wren>=8 && hearts:piper>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_3`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Piper, my darling. Next month, you headline. I mean it."  ✏ `event.overheard_wren_piper_4.l1`
  [piper]: "Wren! Headline! Me! On your stage! I'll cry! Then write a song about crying!"
  · (piper wave (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "Do both. Cry first. Song after. The stool will survive."  ✏ `event.overheard_wren_piper_4.l3`
  [piper]: "It's a deal! Actually, it's a duet!"
  · effects: flag:event.overheard_wren_piper_4

### overheard_tilda_wren_1 · "Slate and Chalkboard" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=2 && hearts:wren>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Wren. Your chalkboard looks like my slate."
  [wren]: "Imitation is flattery, Tilda, with better lighting."  ✏ `event.overheard_tilda_wren_1.l2`
  · (wren shrug (background))
  · (wren emote note)
  · (tilda point (background))
  [tilda]: "Flattery earns a ribbon, dear."
  [wren]: "A ribbon! Pin it to the stool."  ✏ `event.overheard_tilda_wren_1.l4`
  · effects: flag:event.overheard_tilda_wren_1

### overheard_tilda_wren_2 · "Gossip Exchange" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=4 && hearts:wren>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_1`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "I heard something. About a certain cat."
  [wren]: "I heard it first, my darling. Three days ago."  ✏ `event.overheard_tilda_wren_2.l2`
  · (wren shrug (background))
  · (wren emote note)
  · (tilda wave (background))
  [tilda]: "Then we'll trade. You tell me yours."
  [wren]: "Mine's better. It has a pickle in it."  ✏ `event.overheard_tilda_wren_2.l4`
  · effects: flag:event.overheard_tilda_wren_2

### overheard_tilda_wren_3 · "Closing the Books" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.tilda && flag:met.wren && hearts:tilda>=6 && hearts:wren>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_2`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Wren, I've stood behind that counter eight hours a day all week. Save me a seat tonight?"
  [wren]: "Tilda, you've earned the good chair. Come at six. Cider's on me."  ✏ `event.overheard_tilda_wren_3.l2`
  · (wren point (background))
  · (wren emote note)
  · (tilda shrug (background))
  [tilda]: "Just one cup. Then the slate."
  [wren]: "The slate will wait. It's good at that."  ✏ `event.overheard_tilda_wren_3.l4`
  · effects: flag:event.overheard_tilda_wren_3

### overheard_wren_bram_1 · "The Fourth Word" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "Bram! Another day of our bet. One cider, three words. Give me a fourth."  ✏ `event.overheard_wren_bram_1.l1`
  [bram]: "Hm. Cider. Please. Thanks."
  · (bram shrug (background))
  · (bram emote sweat)
  · (wren point (background))
  [wren]: "That's three. 'Hm' doesn't count. Between us, it never has."  ✏ `event.overheard_wren_bram_1.l3`
  [bram]: "Hm."
  · effects: flag:event.overheard_wren_bram_1

### overheard_wren_bram_2 · "The Unsent Letter" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=4 && hearts:bram>=4 && flag:event.bram_heart6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "You're writing a letter at my bar, Bram Hollis. Who's it for?"  ✏ `event.overheard_wren_bram_2.l1`
  [bram]: "Old teacher. Won't send it."
  · (bram shrug (background))
  · (bram emote ellipsis)
  · (wren point (background))
  [wren]: "Then let me read it. I'll improve it. That's my gift."  ✏ `event.overheard_wren_bram_2.l3`
  [bram]: "Hm. No. ...Maybe the ending."
  · effects: flag:event.overheard_wren_bram_2

### overheard_wren_bram_3 · "Five Words" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.bram && hearts:wren>=6 && hearts:bram>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_2 && flag:event.wren_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "Bram. If I ever left Wetherell, would you notice?"  ✏ `event.overheard_wren_bram_3.l1`
  [bram]: "Yes. Cider'd be worse."
  · (bram shrug (background))
  · (bram emote exclaim)
  · (wren wave (background))
  [wren]: "Four words! Four! Felix owes me a fish."  ✏ `event.overheard_wren_bram_3.l3`
  [bram]: "Hm. Don't go far."
  · effects: flag:event.overheard_wren_bram_3

### overheard_wren_hazel_1 · "The Notebook" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `flag:met.wren && flag:met.hazel && hearts:wren>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), hazel(26, 18)
  · (place wren to (24,18))
  · (place hazel to (26,18))
  · (wren faces right)
  · (hazel faces left)
  [wren]: "Hazel, my notebook! What did I say about Juno last week?"  ✏ `event.overheard_wren_hazel_1.l1`
  [hazel]: "(That she burned a clasp.) Then the forge. Then the idea of a forge."
  · (hazel shrug (background))
  · (hazel emote sweat)
  · (wren point (background))
  [wren]: "And which one's true?"  ✏ `event.overheard_wren_hazel_1.l3`
  [hazel]: "(The clasp.) The rest is yours. I wrote it all down."
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
  [wren]: "Years of scribbling at my bar. Will you ever read me any of it?"  ✏ `event.overheard_wren_hazel_2.l1`
  [hazel]: "(No.) ...One line, maybe. Not today."
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (wren wave (background))
  [wren]: "One line's a start. I'll save you the corner stool. Nobody listens like that stool."  ✏ `event.overheard_wren_hazel_2.l3`
  [hazel]: "(I'll think about it.) That means yes, slowly."
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
  [hazel]: "I read a poem out loud. (To one person.) It didn't kill me."
  [wren]: "Hazel! To who? No, don't tell me. Yes, tell me."  ✏ `event.overheard_wren_hazel_3.l2`
  · (wren wave (background))
  · (wren emote heart)
  · (hazel point (background))
  [hazel]: "(Ask me again when we're friends.) ...Oh. We are."
  [wren]: "Between us, we always were. Now read me that line."  ✏ `event.overheard_wren_hazel_3.l4`
  · effects: flag:event.overheard_wren_hazel_3
