# Scene cards for wren

28 scenes. Your lines are marked ✏ with their text key; everything else is context. You own 63 spoken lines in scenes, 32 dialogue lines and 22 player choice labels.

### wren_heart2 · "The New Recipe" · funny
event · npc_roster.json · map Saloon · priority 6
plays when: `(hearts:wren>=2 && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_heart2`
beats may be ADDED or CUT in this scene
placed: player(9, 6)
  · (wren faces down)
  [wren]: "You're just in time. I'm inventing a drink, and I've run out of people who'll still taste things."  ✏ `event.wren_heart2.0`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  [wren]: "Taste this, [player]. Be honest. ...Not that honest. Medium honest."  ✏ `event.wren_heart2.1`
  [wren]: "That face. Is that a good face? You have a very private face."  ✏ `event.wren_heart2.3`  (emote question)
  [wren]: "Honest and kind. You pass. Take the juice, and come back when I'm brave enough to cook actual food."  ✏ `event.wren_heart2.2`
  · effects: give:artisan.juice,2, friend:wren,40, flag:event.wren_heart2

### wren_heart5 · "Not Just the Noise" · wholesome
event · npc_roster.json · map Saloon · priority 7
plays when: `(hearts:wren>=5 && !weekday:tue && hour>=12 && hour<24 && flag:event.wren_heart2) && !flag:event.wren_heart5`
beats may be ADDED or CUT in this scene
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Everyone thinks I love the noise. Glasses, fiddles, Felix arguing with a herring."  ✏ `event.wren_heart5.0`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  [wren]: "Truth is, noise is only people, piled up. I like the people. You're one of my favorite piles."  ✏ `event.wren_heart5.1`
  [wren]: "It's the quiet hours I mind. An empty room echoes. I've timed it."  ✏ `event.wren_heart5.3`  (expr thinking)
  [wren]: "Take a bottle for the road, so the road has company. And come back soon. Before the four o'clock quiet."  ✏ `event.wren_heart5.2`
  · effects: give:artisan.wine,1, friend:wren,60, flag:event.wren_heart5

### hall_dance · "Opening Night" · wholesome
event · spec_scenes.json · map CommunityHall · priority 8
plays when: `(flag:hall.restored && flag:hall.roof && quest:wren_full_house=done && flag:event.piper_heart10 && flag:event.juno_heart10 && hour>=18 && hour<23 && !festival.in:==0) && !flag:event.hall_dance`
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
  [marcus]: "I checked the roof once. Twice. ...Three times, my father said. It is finished. Not nearly. Finished."
  · (marcus emote sparkle)
  [wren]: "Look at this room! Full, and the porch is full too. Somebody count the porch. I need a number for the story."  ✏ `event.hall_dance.1`  (emote sparkle)
  · (spawn prop artisan.wine at (7,3))
  [wren]: "[player]. I said I'd open it on the night, and this is the night. First glass is yours. I poured it before the doors."  ✏ `event.hall_dance.2`  (expr happy)
  · (remove prop wine)
  [tilda]: "Nobody asked for credit for any of this. So I'm giving it, in the good chalk, under every name in this room."
  [piper]: "Same song, bigger room. One verse, then I step off again. My bow hand still hasn't heard."  (emote sweat)
  · (music piper_theme)
  [piper]: "{if choice:chorus.home}Home, home, wherever the lamp is on.{else}{if choice:chorus.road}The road goes on, and the road comes home.{else}There's always room, there's always room.{/if}{/if}"  (emote note)
  · (piper sway (background))
  [narration]: "The whole hall takes the chorus. Piper stops playing and sways, and the song goes on without her."  ✏ `event.hall_dance.6`
  [bram]: "Hm. Hm-hm. Hm. I sing it in my own language."  (emote note)
  [juno]: "Those door hinges are mine. My mark's on every pin. Listen to it swing. It's showing off, and I'm letting it!"  (expr happy)
  [wren]: "The roof pool is closed! Six winters of coins, and Tilda picked the right year. I'm demanding a recount."  ✏ `event.hall_dance.9`
  [tilda]: "Into the hall box, every coin. Except one. Marcus, that one's yours. Spend it on something that isn't wood."
  [marcus]: "The ladder can wait one night. One dance. I built this floor to take it."
  · (marcus dance (background))
  · (tilda dance (background))
  · (wren emote heart)
  · (juno emote note)
  · (wait 2.5s)
  [narration]: "Marcus takes his one dance, and Tilda takes hers beside him. The lamps burn low, and nobody goes home."  ✏ `event.hall_dance.12`
  · (music stop)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.hall_dance, friend:marcus,40, friend:tilda,40, friend:wren,40, friend:piper,40, friend:bram,40, friend:juno,40

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
  [ione]: "I'll copy that out by hand, Hazel. Not for a shelf. For the desk drawer, where the good things go." [only if flag:choice.reading.poem]
  [narration]: "Hazel reads two borrowed pages about the winter sea, quietly, all the way through. Her sleeve stays shut." [only if flag:choice.reading.borrowed]
  [narration]: "Ione reads first, an old page about birds in fall. Hazel says nothing, and does not leave." [only if flag:choice.reading.listen]
  · (spawn prop forage.snowdrop at (5,3))
  [dorian]: "A snowdrop. Pressed. It kept my place since winter. For the lamp."
  [wren]: "Don't mind me, I only tell stories. Somebody has to write them down. Otherwise I improve them until nobody recognizes them."  ✏ `event.reading_night.3` [only if flag:met.wren && hearts:wren>=3]
  [ione]: "A snowdrop, for the lamp. ...Every chair taken. Next time I'll set out a fifth and leave the door open."  (expr happy)
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
  [felix]: "There you both are. I promised you the boat. She's ready. The bailing tin is the elegant part."
  [elara]: "I knitted a life jacket! Felix says that's not how they work. It goes over the real one. Sandwiches are in the tin!"  (emote sparkle, expr happy)
  [felix]: "Wear both, then. The knitted one can look brave. The real one can do the floating."  (expr happy)
  ▶ DIALOGUE `past_point.talk` (player-facing graph, plays here):
    n0 [elara]: "My list is longer than the boat, and Felix says one place before breakfast. Which one?"
       ? "Round the headland. You've drawn it enough." → r0  effects: flag:choice.point.headland, friend:elara,5
       ? "The white rocks. Slowly." → r1  effects: flag:choice.point.rocks, friend:elara,5
       ? "Straight out, till the shore's a line." → r2  effects: flag:choice.point.horizon, friend:elara,5
    r0 [elara]: "The headland! I've drawn it nine times from the bench. Never from the water side!"
    r1 [elara]: "The white rocks! I've never been closer than the bench. Row slowly, Felix. I want it to last."
    r2 [elara]: "Straight out! I'll hold the sandwich tin. It's very steadying. Medically."
  · (fadeout)
  · (sfx click)
  · (camera pan)
  · (lighting warm)
  · (fadein)
  [narration]: "The boat leaks, politely. Felix pulls the oars. Elara holds her hat down with both hands. The headland slides closer."
  [elara]: "The headland is twice the size the map said! Felix, it's enormous! Best thing I've ever been wrong about!"  (expr happy) [only if flag:choice.point.headland]
  [elara]: "The white rocks look like teeth from here. Friendly teeth! Very good gums. May I wave at them?"  (expr happy) [only if flag:choice.point.rocks]
  [elara]: "The shore's just a line now. I didn't know I could feel this small and this glad at once."  (expr happy) [only if flag:choice.point.horizon]
  [wren]: "Just looking, that's all. From the point, the sea seems to start somewhere else."  ✏ `event.past_the_point.wren` [only if flag:choice.wren.bag.go && flag:met.wren && hearts:wren>=8]
  [narration]: "Elara takes out her list, looks at it, and smiles at the page."
  [felix]: "Dawn's better with two. With three, it's an expedition. ...One place crossed off. I'll want a bigger boat for the rest."  (emote heart)
  · (fadeout)
  · (camera reset)
  · (lighting reset)
  · (fadein)
  · (letterbox off)
  · effects: flag:event.past_the_point, friend:felix,40, friend:elara,40

### story_cat · "Cat With Five Names" · wholesome
event · storyline_events2.json · map Village · priority 7
plays when: `(storyline:five_names_cat && hearts:wren>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.story_cat`
beats may be ADDED or CUT in this scene
placed: wren(25, 18), player(25, 17)
  · (place wren to (25,18))
  · (wren faces down)
  · (spawn prop prop.cat at (26,18))
  [wren]: "Don't move. A cat has chosen your boots. Years I've lived here, and it's never once chosen mine."  ✏ `event.story_cat.intro`
  · (move player to (25,17))
  · (player faces up)
  · (wren emote heart)
  · (wren point (background))
  ▶ DIALOGUE `story.cat.talk` (player-facing graph, plays here):
    n0 [narration]: "A small gray cat sits on your boots, perfectly satisfied, and looks up at you with the patience of a landlord."  ✏ `dlg.story.cat.talk.0`   (→ n1)
    n1 [wren]: "Tilda calls it Ledger. Juno says Quench. Piper swears it's Sharp. Hazel files it under Index. I say Gossip. It ignores all five equally. Rude."  ✏ `dlg.story.cat.talk.1`   (→ n2)
    n2 [wren]: "So. You're the one it picked. What do you call it?"  ✏ `dlg.story.cat.talk.2`
       ? "Ledger. Tilda's right." → r0  effects: flag:choice.cat.ledger  ✏ `dlg.story.cat.talk.c0`
       ? "Quench. Juno's right." → r1  effects: flag:choice.cat.quench  ✏ `dlg.story.cat.talk.c1`
       ? "Sharp. Piper's right." → r2  effects: flag:choice.cat.sharp  ✏ `dlg.story.cat.talk.c2`
       ? "None of those. Something new." → r3  effects: flag:choice.cat.new, friend:wren,10  ✏ `dlg.story.cat.talk.c3`
    r0 [wren]: "Ledger. Tilda will write that on her slate. In ink. With the date."  ✏ `dlg.story.cat.talk.r0`
    r1 [wren]: "Quench. Juno will make it a brass name tag, and by Friday I'll say she made two."  ✏ `dlg.story.cat.talk.r1`
    r2 [wren]: "Sharp. Piper will say she was right all along, in a key of her choosing."  ✏ `dlg.story.cat.talk.r2`
    r3 [wren]: "Something new? Go on, then. ...It purred. It has never purred for me. That's the one."  ✏ `dlg.story.cat.talk.r3`   (expr happy; emote heart)
  [wren]: "{if choice:cat.ledger}Ledger! Tilda will be unbearable. By tonight, the cat will be doing her books.{else}{if choice:cat.quench}Quench! Juno will want a vote. By tonight, the cat will have won it.{else}{if choice:cat.sharp}Sharp! Piper will write it a song. By tonight, it'll be a waltz.{else}A new name! Nobody will believe me, so by tonight I'll have made it bigger.{/if}{/if}{/if}"  ✏ `event.story_cat.after`
  · (remove prop story_prop_0)
  · effects: flag:event.story_cat, friend:wren,20, flag:storydone.five_names_cat

### wren_heart4 · "Open Mic" · funny
prank · slice_events.json · map Saloon · priority 7
plays when: `(hearts:wren>=4 && flag:event.wren_heart2 && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_heart4`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Don't sit yet. I'm trying something new, and I need a witness. Possibly an accomplice."  ✏ `event.wren_heart4.setup`
  · (move player to (9,6))
  · (player faces up)
  · (wren jiggle (background))
  · (sfx click)
  · (camera shake)
  · (player emote exclaim)
  [wren]: "Quiet, everybody! The worst song ever written, sung by the person who wrote it!"  ✏ `event.wren_heart4.prank`
  ▶ DIALOGUE `wren.openmic` (player-facing graph, plays here):
    n0 [wren]: "Well? Be honest. A performer needs one honest face in the crowd. Today you're the whole crowd."  ✏ `dlg.wren.openmic.0`
       ? "Cheer like forty people." → r0  effects: friend:wren,10, flag:choice.wren.openmic.cheer  ✏ `dlg.wren.openmic.c0`
       ? "Heckle, but kindly." → r1  effects: friend:wren,10, flag:choice.wren.openmic.heckle  ✏ `dlg.wren.openmic.c1`
       ? "Hide behind the menu." → r2  effects: flag:choice.wren.openmic.hide  ✏ `dlg.wren.openmic.c2`
    r0 [wren]: "Oh, I could live on that. Do it again. Louder. Pretend the porch is full."  ✏ `dlg.wren.openmic.r0`   (expr happy)
    r1 [wren]: "A heckler! At last, I'm a real act. You're going in the second verse."  ✏ `dlg.wren.openmic.r1`   (expr happy)
    r2 [wren]: "Fair. I'd hide too. Come out, though. The second verse is worse, and you'll want to see it."  ✏ `dlg.wren.openmic.r2`
  [wren]: "Terrible, wasn't it? I wrote it in the bath. The bath had notes."  ✏ `event.wren_heart4.reaction`
  · effects: flag:event.wren_heart4
  · effects: friend:wren,50

### wren_heart6 · "The Kitchen" · wholesome
conversation · slice_events.json · map Saloon · priority 7
plays when: `(hearts:wren>=6 && flag:event.wren_heart5 && flag:event.wren_heart4 && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_heart6`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Come into the back. I'm cooking. I hate cooking. You taste, I stir, and we tell nobody it was me."  ✏ `event.wren_heart6.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren wave (background))
  · (wren emote note)
  ▶ DIALOGUE `wren.kitchen` (player-facing graph, plays here):
    n0 [wren]: "It needs one more thing. Pepper, a splash of cider, or do we leave it alone and back away slowly?"  ✏ `dlg.wren.kitchen.0`
       ? "Pepper. Be brave." → r0  effects: friend:wren,10, flag:choice.wren.kitchen.pepper  ✏ `dlg.wren.kitchen.c0`
       ? "A splash of cider." → r1  effects: friend:wren,10, flag:choice.wren.kitchen.cider  ✏ `dlg.wren.kitchen.c1`
       ? "Plain. It's already good." → r2  effects: friend:wren,10, flag:choice.wren.kitchen.plain  ✏ `dlg.wren.kitchen.c2`
    r0 [wren]: "Brave it is! If I cry, blame the pepper. Officially, it was always the pepper."  ✏ `dlg.wren.kitchen.r0`   (emote sweat)
    r1 [wren]: "Cider. Sweet, with a little mischief in it. Like most of my regulars."  ✏ `dlg.wren.kitchen.r1`
    r2 [wren]: "Plain? You think it's already good? Nobody has ever said that about my cooking. I'm keeping you."  ✏ `dlg.wren.kitchen.r2`   (expr happy)
  [wren]: "It goes on the chalkboard Friday. If anyone asks, you made it and I only stood near it."  ✏ `event.wren_heart6.after`
  · effects: flag:event.wren_heart6
  · effects: friend:wren,60

### wren_heart8 · "The Bag" · surprise
confession · slice_events.json · map Saloon · priority 7
plays when: `(hearts:wren>=8 && flag:event.wren_heart6 && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_heart8`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Close the door behind you. No, the other one. Yes. Now sit, and don't let me make a joke."  ✏ `event.wren_heart8.intro`
  · (move player to (9,6))
  · (player faces up)
  · (letterbox on)
  · (lighting dim)
  · (camera focus on wren)
  · (wren expression thinking)
  ▶ DIALOGUE `wren.bag` (player-facing graph, plays here):
    n0 [wren]: "There's a bag under my bar. Packed since the day I arrived. I haven't opened it in years, and I haven't unpacked it either."  ✏ `dlg.wren.bag.0`   (→ n1; expr thinking)
    n1 [wren]: "I came here to rest for one season. That was eight years ago. So tell me. Go, stay, or leave it under there?"  ✏ `dlg.wren.bag.1`
       ? "Go. See what's out there." → r0  effects: friend:wren,10, flag:choice.wren.bag.go  ✏ `dlg.wren.bag.c0`
       ? "Stay. We'd all miss you." → r1  effects: friend:wren,10, flag:choice.wren.bag.stay  ✏ `dlg.wren.bag.c1`
       ? "Say nothing. Just listen." → r2  effects: friend:wren,15, flag:choice.wren.bag.listen  ✏ `dlg.wren.bag.c2`
    r0 [wren]: "Go. Just like that. You say it like it's easy. ...Maybe it is. I'd want a very good map."  ✏ `dlg.wren.bag.r0`
    r1 [wren]: "Stay. Oh. That's a nicer word than I expected. I'll keep it in the bag, next to the socks."  ✏ `dlg.wren.bag.r1`
    r2 [wren]: "...You didn't tell me what to do. Everyone has advice for the one behind the bar. You just let me talk."  ✏ `dlg.wren.bag.r2`   (expr sad)
  · (wren expression neutral)
  [wren]: "There. I said it out loud, and the bag weighs less. Don't tell anyone. This one I won't improve."  ✏ `event.wren_heart8.after`
  · (camera reset)
  · (lighting reset)
  · (letterbox off)
  · effects: flag:event.wren_heart8
  · effects: friend:wren,70

### wren_heart10 · "One Listener" · wholesome
performance · slice_events.json · map Saloon · priority 7
plays when: `(hearts:wren>=10 && flag:event.wren_heart8 && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_heart10`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Tonight I'm performing. One song, one listener, and I've already picked. Sit there. Yes, you."  ✏ `event.wren_heart10.intro`
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
  [wren]: "{if choice:wren.bag.go}I'm not leaving yet. But the second verse has a map in it, and the map has the sea.{else}{if choice:wren.bag.stay}The chorus was the stay. I sang it twice, in case you missed it.{else}The quiet bit in the middle was yours. You listened once. I wanted to give some quiet back.{/if}{/if}"  ✏ `event.wren_heart10.outro`
  · effects: flag:event.wren_heart10
  · effects: friend:wren,80

### wren_friend · "Gossip Bingo" · funny
shared_activity · slice_events.json · map Saloon · priority 7
plays when: `(hearts:wren>=3 && weekday:fri && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_friend`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Friday means bingo. Not numbers. Rumors. Sit down and cross something off."  ✏ `event.wren_friend.intro`
  · (move player to (9,6))
  · (player faces up)
  · (wren emote note)
  · (wren hop)
  [wren]: "Square three: Bram said four words. Square four: Bram said none. Both on good authority. Mine."  ✏ `event.wren_friend.doing`
  [wren]: "No bingo today. A full card of nonsense, though, and half of it's mine. Bring a rumor next Friday."  ✏ `event.wren_friend.wrap`
  · effects: flag:event.wren_friend
  · effects: friend:wren,15

### wren_birthday · "Wren's Birthday" · funny
conversation · wave4_birthdays.json · map Saloon · priority 8 · repeatable
plays when: `hearts:wren>=2 && birthday.in:wren==0 && unseen:birthday.wren && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "It's my birthday, so nobody may look surprised. Sit. Be festive. Have a pickle, I saved the good jar."  ✏ `event.wren_birthday.intro`
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
    r1 [wren]: "That was dreadful. Do it again, louder. The glasses are rattling in protest."  ✏ `dlg.wren.birthday.r1`   (expr happy)
    r2 [wren]: "Oh. ...Oh, you're a menace. A kind one. Sit down before I embarrass us both."  ✏ `dlg.wren.birthday.r2`   (expr happy)
  [wren]: "{if choice:bday.wren.glass}A toast, and a good one. I'll be telling it for years, longer each time.{else}{if choice:bday.wren.sing}Your song's the new house anthem. Nobody asked. It's permanent.{else}A hug. Of everything on the shelf tonight, that's the best gift.{/if}{/if}"  ✏ `event.wren_birthday.after`
  · effects: flag:event.wren_birthday
  · effects: mark:birthday.wren, friend:wren,30, give:artisan.pickles,2, mail:wren_bday_thanks

### wren_courtship · "Wren Asks" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `(hearts:wren>=8 && flag:event.wren_heart8 && !flag:partners.wren && !flag:choice.courtship.wren.friends && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_courtship`
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
    r1 [wren]: "Friends. Yes. The best kind. You keep the good stool, and I keep talking too much. I'm relieved. Mostly."  ✏ `dlg.wren.courtship.r1`   (expr happy)
    r2 [wren]: "Wait. Fair. I'm a bartender. Waiting is half the job, and I'm good at the other half."  ✏ `dlg.wren.courtship.r2`   (expr thinking)
  [wren]: "{if flag:choice.courtship.wren.yes}Walks, then. And poems. You're stuck with me, and I'm stuck with the stool.{else}{if flag:choice.courtship.wren.friends}The best of friends. It's on my chalkboard now, in capital letters.{else}Whenever you like. I'll be on the stage, being patient. It's a new look.{/if}{/if}"  ✏ `event.wren_courtship.after`
  · effects: flag:event.wren_courtship
  · effects: flag:courtship.asked.wren

### wren_courtship2 · "Wren Asks Again" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `hearts:wren>=10 && flag:choice.courtship.wren.later && !flag:event.wren_courtship2.done && !flag:partners.wren && !flag:choice.courtship.wren.friends && !flag:choice.courtship.hazel.yes && !flag:choice.courtship.juno.yes && !flag:choice.courtship.piper.yes && !flag:choice.courtship.felix.yes && !flag:choice.courtship.dorian.yes && !flag:choice.courtship.elara.yes && !flag:choice.courtship.ione.yes && !weekday:tue && hour>=12 && hour<24`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Last time you said wait, so I waited. Patiently, which surprised everyone. I still like you. More, actually."  ✏ `event.wren_courtship2.intro`
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
    r1 [wren]: "Friends. Yes. The best kind. You keep the good stool, and I keep talking too much. I'm relieved. Mostly."  ✏ `dlg.wren.courtship.r1`   (expr happy)
    r2 [wren]: "Wait. Fair. I'm a bartender. Waiting is half the job, and I'm good at the other half."  ✏ `dlg.wren.courtship.r2`   (expr thinking)
  [wren]: "{if flag:choice.courtship.wren.yes}Walks, then. And poems. You're stuck with me, and I'm stuck with the stool.{else}{if flag:choice.courtship.wren.friends}The best of friends. It's on my chalkboard now, in capital letters.{else}Still waiting, then. I'll keep the question behind the bar, next to the bag.{/if}{/if}"  ✏ `event.wren_courtship2.after`
  · effects: flag:event.wren_courtship2
  · effects: flag:courtship.asked.wren, flag:event.wren_courtship2.done

### wren_partners · "Wren, Together" · wholesome
conversation · wave5_courtship.json · map Saloon · priority 8
plays when: `(hearts:wren>=10 && flag:choice.courtship.wren.yes && !flag:partners.wren && !weekday:tue && hour>=12 && hour<24) && !flag:event.wren_partners`
text-only scene: rewrite lines, do not add or cut beats
placed: player(9, 6)
  · (wren faces down)
  [wren]: "Evening, love. I wrote you a song. It's terrible. It's mine. Sit and be sung at."  ✏ `event.wren_partners.intro`
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
    r2 [wren]: "A walk home. Past the store, past Tilda's lamp, past all my old excuses. Lead on."  ✏ `dlg.wren.partners.r2`   (expr happy)
  [wren]: "{if choice:courtship.wren.song}The song's finished now. Your name's in the bridge. That's why I sang it twice.{else}{if choice:courtship.wren.dance}We danced. Nobody looked. Everybody looked. I loved it.{else}The bag under my bar is empty. I unpacked it this morning. You were the reason, quietly.{/if}{/if}"  ✏ `event.wren_partners.after`
  · effects: flag:event.wren_partners
  · effects: flag:partners.wren, friend:wren,50

### overheard_wren_piper_1 · "Sound Check" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=2 && hearts:piper>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_wren_piper_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "You're flat on the third bar. Fabulously flat. Do it again."  ✏ `event.overheard_wren_piper_1.l1`
  [piper]: "I'm not flat, the stool's tilted! It's a stage stool, Wren. It has opinions!"
  · (piper shrug (background))
  · (piper emote note)
  · (wren point (background))
  [wren]: "The stool doesn't have opinions. I do. Again. From the top."  ✏ `event.overheard_wren_piper_1.l3`
  [piper]: "Fine. From the top. But I'm taking the bridge up a key, out of spite."
  · effects: flag:event.overheard_wren_piper_1

### overheard_wren_piper_2 · "The Shared Verse" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=4 && hearts:piper>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_1) && !flag:event.overheard_wren_piper_2`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "I wrote a verse. It's for the saloon. It's about you, and I'm sorry."  ✏ `event.overheard_wren_piper_2.l1`
  [piper]: "Wren! You wrote a verse? Read it! No, sing it! No, read it slowly!"
  · (piper shrug (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "'Piper sings and the rafters shake, and every glass in town awake.' That's all I've got."  ✏ `event.overheard_wren_piper_2.l3`
  [piper]: "It's perfect. I'm stealing it for my next opening. You get credit. Tiny letters. Huge feelings."
  · effects: flag:event.overheard_wren_piper_2

### overheard_wren_piper_3 · "Closing Time" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=6 && hearts:piper>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_2) && !flag:event.overheard_wren_piper_3`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Today's last song, Piper. The room will be tired. So will I. Make it a soft one."  ✏ `event.overheard_wren_piper_3.l1`
  [piper]: "Soft, I can do. No words. Just a hum for you, the stool and the last of the sun."
  · (piper point (background))
  · (piper emote note)
  · (wren shrug (background))
  [wren]: "That's the best verse you've ever said out loud. Write it down, in pencil. Ink makes you shy."  ✏ `event.overheard_wren_piper_3.l3`
  [piper]: "Pencil it is. Somebody keeps fixing my pencil anyway. Save me the last glass."
  · effects: flag:event.overheard_wren_piper_3

### overheard_wren_piper_4 · "The Headliner" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.piper && hearts:wren>=8 && hearts:piper>=8 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_piper_3) && !flag:event.overheard_wren_piper_4`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), piper(26, 18)
  · (place wren to (24,18))
  · (place piper to (26,18))
  · (wren faces right)
  · (piper faces left)
  [wren]: "Piper. Next month, you headline. Top of my chalkboard. I bought new chalk."  ✏ `event.overheard_wren_piper_4.l1`
  [piper]: "Headline? Me? Your stage? Wren! I'll cry! Then write a song about crying!"
  · (piper wave (background))
  · (piper emote note)
  · (wren wave (background))
  [wren]: "Do both. Cry first, song after. I'll sell handkerchiefs at the door."  ✏ `event.overheard_wren_piper_4.l3`
  [piper]: "Deal! ...Actually, make it a duet. Your stage, your song too."
  · effects: flag:event.overheard_wren_piper_4

### overheard_tilda_wren_1 · "Slate and Chalkboard" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.tilda && flag:met.wren && hearts:tilda>=2 && hearts:wren>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_tilda_wren_1`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Wren. Your chalkboard looks remarkably like my slate."
  [wren]: "Imitation is flattery, Tilda, with better lighting."  ✏ `event.overheard_tilda_wren_1.l2`
  · (wren shrug (background))
  · (wren emote note)
  · (tilda point (background))
  [tilda]: "Flattery earns a ribbon. Imitation earns a bill."
  [wren]: "A ribbon! Pin it to the stool."  ✏ `event.overheard_tilda_wren_1.l4`
  · effects: flag:event.overheard_tilda_wren_1

### overheard_tilda_wren_2 · "Gossip Exchange" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.tilda && flag:met.wren && hearts:tilda>=4 && hearts:wren>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_1) && !flag:event.overheard_tilda_wren_2`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "I've heard something. About a certain cat, and a certain windowsill."
  [wren]: "I heard it first. Three days ago. It's grown since."  ✏ `event.overheard_tilda_wren_2.l2`
  · (wren shrug (background))
  · (wren emote note)
  · (tilda wave (background))
  [tilda]: "Then we'll trade. Yours first. I have a reputation to protect."
  [wren]: "Mine's better. It has a pickle in it."  ✏ `event.overheard_tilda_wren_2.l4`
  · effects: flag:event.overheard_tilda_wren_2

### overheard_tilda_wren_3 · "Closing the Books" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.tilda && flag:met.wren && hearts:tilda>=6 && hearts:wren>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_tilda_wren_2) && !flag:event.overheard_tilda_wren_3`
text-only scene: rewrite lines, do not add or cut beats
placed: tilda(24, 18), wren(26, 18)
  · (place tilda to (24,18))
  · (place wren to (26,18))
  · (tilda faces right)
  · (wren faces left)
  [tilda]: "Six days at that counter this week. Save me a seat later?"
  [wren]: "You've earned the good stool, Tilda. Come at seven. Cider's on me."  ✏ `event.overheard_tilda_wren_3.l2`
  · (wren point (background))
  · (wren emote note)
  · (tilda shrug (background))
  [tilda]: "One cup. Then back to the slate."
  [wren]: "The slate will wait. It's good at that."  ✏ `event.overheard_tilda_wren_3.l4`
  · effects: flag:event.overheard_tilda_wren_3

### overheard_wren_bram_1 · "The Fourth Word" · funny
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.bram && hearts:wren>=2 && hearts:bram>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_wren_bram_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), bram(26, 18)
  · (place wren to (24,18))
  · (place bram to (26,18))
  · (wren faces right)
  · (bram faces left)
  [wren]: "Bram! One cider, three words, every night. Today I want a fourth. I've a fish riding on it."  ✏ `event.overheard_wren_bram_1.l1`
  [bram]: "Hm. Cider. Please. Thanks."
  · (bram shrug (background))
  · (bram emote sweat)
  · (wren point (background))
  [wren]: "That's three. 'Hm' doesn't count. Between us, it never has."  ✏ `event.overheard_wren_bram_1.l3`
  [bram]: "Hm."
  · effects: flag:event.overheard_wren_bram_1

### overheard_wren_bram_2 · "The Unsent Letter" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.bram && hearts:wren>=4 && hearts:bram>=4 && flag:event.bram_heart6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_1) && !flag:event.overheard_wren_bram_2`
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
plays when: `(flag:met.wren && flag:met.bram && hearts:wren>=6 && hearts:bram>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_bram_2 && flag:event.wren_heart8) && !flag:event.overheard_wren_bram_3`
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
plays when: `(flag:met.wren && flag:met.hazel && hearts:wren>=2 && hearts:hazel>=2 && hour>=9 && hour<19 && !festival.in:==0) && !flag:event.overheard_wren_hazel_1`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), hazel(26, 18)
  · (place wren to (24,18))
  · (place hazel to (26,18))
  · (wren faces right)
  · (hazel faces left)
  [wren]: "Hazel, my notebook! What did I say about Juno last week?"  ✏ `event.overheard_wren_hazel_1.l1`
  [hazel]: "That she burned a clasp. Then, by Thursday, the forge. Then the idea of a forge."
  · (hazel shrug (background))
  · (hazel emote sweat)
  · (wren point (background))
  [wren]: "And which one's true? Careful. I'm attached to the forge."  ✏ `event.overheard_wren_hazel_1.l3`
  [hazel]: "The clasp. (The rest is yours.) I wrote all three versions down."
  · effects: flag:event.overheard_wren_hazel_1

### overheard_wren_hazel_2 · "The Corner Stool" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.hazel && hearts:wren>=4 && hearts:hazel>=4 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_hazel_1) && !flag:event.overheard_wren_hazel_2`
text-only scene: rewrite lines, do not add or cut beats
placed: wren(24, 18), hazel(26, 18)
  · (place wren to (24,18))
  · (place hazel to (26,18))
  · (wren faces right)
  · (hazel faces left)
  [wren]: "Years of scribbling at my bar. Will you ever read me any of it?"  ✏ `event.overheard_wren_hazel_2.l1`
  [hazel]: "(No.) ...One line. Maybe. Not today, and not at the bar."
  · (hazel shrug (background))
  · (hazel emote ellipsis)
  · (wren wave (background))
  [wren]: "One line's a start. I'll save you the corner stool. Nobody listens like that stool."  ✏ `event.overheard_wren_hazel_2.l3`
  [hazel]: "(I'll think about it.) That means yes. Slowly. Possibly by spring."
  · effects: flag:event.overheard_wren_hazel_2

### overheard_wren_hazel_3 · "One Line Aloud" · wholesome
overheard · wave6_overheard.json · map Village · priority 5
plays when: `(flag:met.wren && flag:met.hazel && hearts:wren>=6 && hearts:hazel>=6 && hour>=9 && hour<19 && !festival.in:==0 && flag:event.overheard_wren_hazel_2 && flag:event.hazel_heart8) && !flag:event.overheard_wren_hazel_3`
text-only scene: rewrite lines, do not add or cut beats
placed: hazel(24, 18), wren(26, 18)
  · (place hazel to (24,18))
  · (place wren to (26,18))
  · (hazel faces right)
  · (wren faces left)
  [hazel]: "I read a poem aloud. To one person. (I survived. I checked twice.)"
  [wren]: "Hazel! To who? No, don't tell me. Yes, tell me."  ✏ `event.overheard_wren_hazel_3.l2`
  · (wren wave (background))
  · (wren emote heart)
  · (hazel point (background))
  [hazel]: "(Ask me again when we're friends.) ...Oh. We are. That's inconvenient."
  [wren]: "Between us, we always were. Now read me that line."  ✏ `event.overheard_wren_hazel_3.l4`
  · effects: flag:event.overheard_wren_hazel_3
